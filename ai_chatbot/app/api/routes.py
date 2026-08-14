import json
import re

from fastapi import APIRouter
from pydantic import BaseModel

from app.ai.hf_client import ask_llm


router = APIRouter()


class ChatRequest(BaseModel):
    message: str = ""
    subject: str = "math"
    image_data: str | None = None
    image_mime_type: str | None = None


class ExamQuestionResult(BaseModel):
    questionText: str = ""
    selectedAnswer: str | None = None
    correctAnswer: str | None = None
    isCorrect: bool = False


class ExamAnalysisRequest(BaseModel):
    userId: str = ""
    examId: str = ""
    examType: str = ""
    subjectName: str = ""
    score: int = 0
    questions: list[ExamQuestionResult] = []


class QuestionExplanationRequest(BaseModel):
    subjectName: str = ""
    questionText: str = ""
    selectedAnswer: str | None = None
    selectedAnswerText: str | None = None
    correctAnswer: str = ""
    correctAnswerText: str = ""
    isCorrect: bool = False


class SubjectProgressInput(BaseModel):
    subjectId: str | None = None
    subjectName: str = ""
    progressPercent: float = 0
    averageScore: float = 0
    completedLessons: int = 0
    totalLessons: int = 0
    remainingLessons: int = 0
    nextLessonTitle: str | None = None
    nextLessonId: str | None = None


class PersonalizedRecommendationRequest(BaseModel):
    contextType: str = ""
    studentName: str = ""
    stream: str = ""
    currentLevel: str = ""
    goal: str = ""
    studyHours: str = ""
    examExperience: str = ""
    learningMethods: list[str] = []
    difficultSubjects: list[str] = []
    averageScore: float = 0
    completedLessons: int = 0
    totalLessons: int = 0
    subjects: list[SubjectProgressInput] = []


@router.post("/chat")
def chat_route(req: ChatRequest):
    from app.services.chat_service import chat

    return {
        "reply": chat(
            req.message,
            req.subject,
            image_data=req.image_data,
            image_mime_type=req.image_mime_type,
        )
    }


@router.post("/exam-analysis")
def exam_analysis_route(req: ExamAnalysisRequest):
    fallback = _build_rule_based_analysis(req)
    raw = ask_llm(_build_exam_analysis_prompt(req))
    parsed = _parse_analysis_json(raw)

    if not parsed:
        return fallback

    return {
        "strengthAreas": _clean_list(parsed.get("strengthAreas")) or fallback["strengthAreas"],
        "weakAreas": _clean_list(parsed.get("weakAreas")) or fallback["weakAreas"],
        "levelMessage": str(parsed.get("levelMessage") or fallback["levelMessage"]).strip(),
        "recommendationText": str(parsed.get("recommendationText") or fallback["recommendationText"]).strip(),
    }


@router.post("/question-explanation")
def question_explanation_route(req: QuestionExplanationRequest):
    fallback = _build_rule_based_explanation(req)
    raw = ask_llm(_build_question_explanation_prompt(req, fallback))
    parsed = _parse_analysis_json(raw)

    if parsed:
        solution = str(parsed.get("solutionText") or "").strip()
        if solution:
            return {"solutionText": solution}

    cleaned = (raw or "").strip()
    if cleaned and "GEMINI_API_KEY" not in cleaned:
        return {"solutionText": cleaned}

    return {"solutionText": fallback}


@router.post("/personalized-recommendation")
def personalized_recommendation_route(req: PersonalizedRecommendationRequest):
    fallback = _build_rule_based_recommendation(req)
    raw = ask_llm(_build_personalized_recommendation_prompt(req))
    parsed = _parse_analysis_json(raw)

    if not parsed:
        return fallback

    return {
        "recommendationText": str(parsed.get("recommendationText") or fallback["recommendationText"]).strip(),
        "focusSubjects": _clean_list(parsed.get("focusSubjects"), limit=8) or fallback["focusSubjects"],
        "weeklyStudyHours": _clean_int(parsed.get("weeklyStudyHours"), fallback["weeklyStudyHours"]),
        "lessonOrder": _clean_list(parsed.get("lessonOrder"), limit=10) or fallback["lessonOrder"],
        "strengthAreas": _clean_list(parsed.get("strengthAreas")) or fallback["strengthAreas"],
        "weakAreas": _clean_list(parsed.get("weakAreas")) or fallback["weakAreas"],
        "subjectAnalyses": _clean_subject_analyses(parsed.get("subjectAnalyses")) or fallback["subjectAnalyses"],
    }


def _build_exam_analysis_prompt(req: ExamAnalysisRequest) -> str:
    wrong_questions = [q for q in req.questions if not q.isCorrect][:8]
    correct_questions = [q for q in req.questions if q.isCorrect][:5]

    def format_question(q: ExamQuestionResult) -> str:
        return (
            f"- السؤال: {q.questionText}\n"
            f"  إجابة الطالب: {q.selectedAnswer or 'غير محددة'}\n"
            f"  الإجابة الصحيحة: {q.correctAnswer or 'غير محددة'}"
        )

    return f"""
حلل نتيجة امتحان الطالب وأعد JSON فقط بدون markdown.

المادة: {req.subjectName or 'غير محددة'}
نوع الامتحان: {req.examType or 'غير محدد'}
العلامة: {req.score}%

أسئلة صحيحة:
{chr(10).join(format_question(q) for q in correct_questions) or '- لا يوجد'}

أسئلة خاطئة:
{chr(10).join(format_question(q) for q in wrong_questions) or '- لا يوجد'}

الشكل المطلوب:
{{
  "strengthAreas": ["نقطة قوة محددة", "نقطة قوة محددة"],
  "weakAreas": ["نقطة ضعف محددة", "نقطة ضعف محددة"],
  "levelMessage": "جملة قصيرة تصف مستوى الطالب",
  "recommendationText": "توصية عملية من جملة إلى جملتين"
}}
""".strip()


def _build_personalized_recommendation_prompt(req: PersonalizedRecommendationRequest) -> str:
    subjects_payload = [
        {
            "subjectName": s.subjectName,
            "progressPercent": s.progressPercent,
            "averageScore": s.averageScore,
            "completedLessons": s.completedLessons,
            "totalLessons": s.totalLessons,
            "remainingLessons": s.remainingLessons,
            "nextLessonTitle": s.nextLessonTitle,
        }
        for s in req.subjects[:12]
    ]

    return f"""
أنت رفيق دراسة داخل EduNext. ابنِ توصية دراسة عربية دافئة ومباشرة من بيانات الطالب فقط.
أعد JSON فقط بدون markdown وبدون نص خارجي.

السياق: {req.contextType or 'recommendations'}
اسم الطالب: {req.studentName or 'الطالب'}
الفرع: {req.stream or 'غير محدد'}
المستوى الحالي: {req.currentLevel or 'غير محدد'}
الهدف: {req.goal or 'غير محدد'}
وقت الدراسة المتاح: {req.studyHours or 'غير محدد'}
خبرة الاختبارات: {req.examExperience or 'غير محدد'}
طرق التعلم المفضلة: {', '.join(req.learningMethods) or 'غير محدد'}
المواد الصعبة من التهيئة: {', '.join(req.difficultSubjects) or 'غير محدد'}
متوسط العلامات: {req.averageScore}%
الدروس المكتملة: {req.completedLessons} من {req.totalLessons}
بيانات المواد:
{json.dumps(subjects_payload, ensure_ascii=False)}

قواعد مهمة:
- اختر كل المواد التي تحتاج تركيزاً فعلياً، وليس مادة واحدة فقط، خصوصاً إذا كانت ضمن المواد الصعبة أو علامتها أقل من 70 أو لديها دروس كثيرة متبقية.
- لا تضع مادة في focusSubjects إذا لم تكن موجودة في بيانات المواد أو المواد الصعبة.
- اربط كل حكم برقم واضح: علامة، نسبة تقدم، دروس مكتملة، أو الدرس التالي.
- weeklyStudyHours يجب أن يناسب وقت الدراسة المتاح. إذا الوقت قليل لا تقترح رقماً كبيراً.
- lessonOrder خطوات عملية قابلة للتنفيذ، واذكر اسم المادة مع الدرس عندما تتوفر nextLessonTitle.

الشكل المطلوب:
{{
  "recommendationText": "رسالة قصيرة توضّح من أين يبدأ الطالب ولماذا",
  "focusSubjects": ["مادة 1", "مادة 2"],
  "weeklyStudyHours": 8,
  "lessonOrder": ["ابدأ في مادة كذا بدرس كذا", "راجع نقطة محددة", "حل اختباراً قصيراً"],
  "strengthAreas": ["قوة محددة مرتبطة برقم", "قوة محددة مرتبطة برقم"],
  "weakAreas": ["ضعف محدد مع السبب", "ضعف محدد مع السبب"],
  "subjectAnalyses": [
    {{
      "subjectName": "اسم المادة",
      "strengths": ["ما الذي يسير جيداً ولماذا"],
      "weaknesses": ["ما الذي يحتاج تحسيناً ولماذا"]
    }}
  ]
}}
""".strip()


def _build_question_explanation_prompt(req: QuestionExplanationRequest, fallback: str) -> str:
    selected = req.selectedAnswerText or req.selectedAnswer or "لم يختر الطالب إجابة"

    return f"""
اشرح حل السؤال للطالب خطوة بخطوة بلغة واضحة ومختصرة.
أعد JSON فقط بدون markdown:
{{"solutionText": "شرح منظم على عدة أسطر"}}

المادة: {req.subjectName or 'غير محددة'}
السؤال:
{req.questionText}

إجابة الطالب:
{selected}

الإجابة الصحيحة:
{req.correctAnswerText or req.correctAnswer}

إذا كانت البيانات لا تكفي، استخدم هذا الشرح:
{fallback}
""".strip()


def _parse_analysis_json(raw: str) -> dict | None:
    raw = (raw or "").strip()
    if not raw:
        return None

    raw = re.sub(r"^```(?:json)?|```$", "", raw, flags=re.IGNORECASE | re.MULTILINE).strip()
    match = re.search(r"\{.*\}", raw, flags=re.DOTALL)
    if match:
        raw = match.group(0)

    try:
        value = json.loads(raw)
    except json.JSONDecodeError:
        return None

    return value if isinstance(value, dict) else None


def _clean_list(value, limit: int = 5) -> list[str]:
    if not isinstance(value, list):
        return []

    return [str(item).strip() for item in value if str(item).strip()][:limit]


def _clean_int(value, fallback: int) -> int:
    try:
        parsed = int(value)
    except (TypeError, ValueError):
        return fallback

    return parsed if parsed > 0 else fallback


def _clean_subject_analyses(value) -> list[dict]:
    if not isinstance(value, list):
        return []

    analyses = []
    for item in value[:12]:
        if not isinstance(item, dict):
            continue

        subject_name = str(item.get("subjectName") or "").strip()
        strengths = _clean_list(item.get("strengths"))
        weaknesses = _clean_list(item.get("weaknesses"))

        if subject_name and (strengths or weaknesses):
            analyses.append(
                {
                    "subjectName": subject_name,
                    "strengths": strengths,
                    "weaknesses": weaknesses,
                }
            )

    return analyses


def _lesson_count_text(count: int) -> str:
    if count == 1:
        return "درساً واحداً"
    if count == 2:
        return "درسين"
    if 3 <= count <= 10:
        return f"{count} دروس"
    return f"{count} درساً"


def _available_weekly_hours(study_hours: str) -> int:
    text = (study_hours or "").strip()
    numbers = [int(value) for value in re.findall(r"\d+", text)]
    if numbers:
        base = max(numbers)
        return base * 5 if base <= 6 else base

    if any(word in text for word in ("أقل", "قليل", "ساعة")):
        return 6
    if any(word in text for word in ("كثير", "متفرغ", "4", "٤")):
        return 18
    return 12


def _build_rule_based_recommendation(req: PersonalizedRecommendationRequest) -> dict:
    subjects = sorted(
        req.subjects,
        key=lambda s: (
            0 if s.subjectName in req.difficultSubjects else 1,
            s.averageScore if s.averageScore > 0 else 100,
            s.progressPercent,
            -s.remainingLessons,
        ),
    )

    focus_subjects = [
        s.subjectName
        for s in subjects
        if s.subjectName and (
            s.subjectName in req.difficultSubjects
            or 0 < s.averageScore < 70
            or s.progressPercent < 60
            or s.remainingLessons > 0
        )
    ][:4]
    if not focus_subjects:
        focus_subjects = [s.subjectName for s in subjects if s.subjectName][:2] or req.difficultSubjects[:2]

    lesson_order = []
    for subject in subjects:
        if subject.subjectName not in focus_subjects:
            continue
        if subject.nextLessonTitle:
            lesson_order.append(f"{subject.subjectName}: ابدأ بدرس {subject.nextLessonTitle}")
        else:
            lesson_order.append(f"{subject.subjectName}: حل اختباراً قصيراً لتحديد أول نقطة ضعف")
    lesson_order = lesson_order[:6]

    strength_areas = []
    weak_areas = []
    subject_analyses = []

    for subject in subjects[:12]:
        strengths = []
        weaknesses = []
        subject_name = subject.subjectName or "هذه المادة"

        if subject.averageScore >= 70:
            strengths.append(f"أداؤك في {subject_name} جيد لأن متوسطك وصل إلى {subject.averageScore:.0f}%.")
        elif subject.completedLessons > 0:
            strengths.append(f"عندك بداية واضحة في {subject_name}: أنجزت {_lesson_count_text(subject.completedLessons)}.")

        if subject.averageScore and subject.averageScore < 70:
            weaknesses.append(f"{subject_name} تحتاج مراجعة لأن متوسطك فيها {subject.averageScore:.0f}%.")
        if subject.remainingLessons > 0:
            weaknesses.append(f"ما زال عندك {_lesson_count_text(subject.remainingLessons)} غير مكتمل في {subject_name}.")

        if strengths:
            strength_areas.extend(strengths[:1])
        if weaknesses:
            weak_areas.extend(weaknesses[:1])

        if subject.subjectName and (strengths or weaknesses):
            subject_analyses.append(
                {
                    "subjectName": subject.subjectName,
                    "strengths": strengths[:2],
                    "weaknesses": weaknesses[:2],
                }
            )

    if not strength_areas and req.completedLessons > 0:
        strength_areas.append(f"أنجزت {_lesson_count_text(req.completedLessons)} من أصل {req.totalLessons}.")
    if not weak_areas and req.averageScore < 70:
        weak_areas.append(f"متوسطك الحالي {req.averageScore:.0f}%، لذلك الأفضل مراجعة الأساسيات.")

    weekly_hours = _available_weekly_hours(req.studyHours)
    student_name = req.studentName.strip() if req.studentName.strip() else "صديقي"
    recommendation = (
        f"{student_name}، ركّز هذا الأسبوع على {' و'.join(focus_subjects)} لأنها الأوضح حسب تقدمك وعلاماتك. "
        f"ابدأ بخطوة صغيرة من ترتيب الدروس، ثم حل اختباراً قصيراً بعد كل مراجعة."
    )

    return {
        "recommendationText": recommendation,
        "focusSubjects": focus_subjects,
        "weeklyStudyHours": weekly_hours,
        "lessonOrder": lesson_order,
        "strengthAreas": strength_areas[:3],
        "weakAreas": weak_areas[:3],
        "subjectAnalyses": subject_analyses,
    }


def _build_rule_based_analysis(req: ExamAnalysisRequest) -> dict:
    wrong_answers = [q for q in req.questions if not q.isCorrect]

    if req.score >= 85:
        return {
            "strengthAreas": ["فهم المفاهيم", "الدقة في الإجابة"],
            "weakAreas": ["تفاصيل بسيطة تحتاج مراجعة"] if wrong_answers else ["لا توجد نقاط ضعف واضحة حالياً"],
            "levelMessage": "أداء ممتاز، الطالب متمكن من أغلب مهارات الامتحان.",
            "recommendationText": "استمر على نفس الخطة وراجع الأخطاء القليلة قبل الانتقال لأسئلة أعلى مستوى.",
        }
    if req.score >= 60:
        return {
            "strengthAreas": ["امتلاك أساس مناسب", "حل جزء جيد من الأسئلة"],
            "weakAreas": ["الأسئلة التطبيقية", "التركيز أثناء الحل"],
            "levelMessage": "أداء جيد، لكنه يحتاج تدريباً إضافياً على الأسئلة التي سببت أخطاء.",
            "recommendationText": "راجع الأسئلة الخاطئة ثم حل تدريباً قصيراً على نفس الأفكار قبل إعادة المحاولة.",
        }
    return {
        "strengthAreas": ["المحاولة والاستمرار"],
        "weakAreas": ["المفاهيم الأساسية", "الدقة في اختيار الإجابة"],
        "levelMessage": "المستوى يحتاج تحسيناً من الأساسيات قبل الانتقال للأسئلة الصعبة.",
        "recommendationText": "ارجع إلى شرح الدرس وملخصه، ثم ابدأ بالأسئلة السهلة تدريجياً.",
    }


def _build_rule_based_explanation(req: QuestionExplanationRequest) -> str:
    selected = req.selectedAnswerText or req.selectedAnswer or "لم يختر الطالب إجابة"
    correct = req.correctAnswerText or req.correctAnswer or "غير محددة"

    if req.isCorrect:
        return (
            f"فكرة السؤال هي اختيار الإجابة الأنسب من الخيارات.\n"
            f"إجابتك كانت: {selected}، وهي صحيحة.\n"
            f"الإجابة المطابقة هي: {correct}.\n"
            "استمر بنفس الطريقة، وراجع نص السؤال جيداً قبل تثبيت الإجابة."
        )

    return (
        f"فكرة السؤال هي فهم المطلوب ثم مقارنة الخيارات.\n"
        f"إجابتك كانت: {selected}.\n"
        f"الإجابة الصحيحة هي: {correct}.\n"
        "سبب الخطأ غالباً هو الخلط بين الخيارات أو عدم الانتباه للكلمة المفتاحية في السؤال.\n"
        "راجع السؤال مرة أخرى وحدد الدليل الذي يقود للإجابة الصحيحة."
    )
