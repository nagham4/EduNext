from app.ingest import ingest_subject
from app.core.config import PDFS_DIR, VECTOR_DB_DIR
from app.rag.retriever import format_context, retrieve
from app.rag.vector_store import has_index, load_index
from app.subjects.english_bot import EnglishBot
from app.subjects.math_bot import MathBot


SUPPORTED_SUBJECTS = {
    "math": MathBot(),
    "english": EnglishBot(),
}

MATH_EXAM_SOURCES = {
    "5ee4bfafcf66d.pdf",
    "math.pdf",
}

ENGLISH_EXAM_SOURCES = {
    "en12s.pdf",
    "اللغة الإنجليزية_ الدورة الثانية_الفرع العلمي_14_08_2024.pdf",
    "اللغة الانجليزية_العلمي_الدورة الأولى 2023.pdf",
}

GREETINGS = {
    "مرحبا",
    "مرحباً",
    "السلام عليكم",
    "اهلا",
    "أهلا",
    "هلا",
    "hi",
    "hello",
    "hey",
}

EXAM_PRACTICE_KEYWORDS = {
    "سنوات",
    "امتحان",
    "امتحانات",
    "دورة",
    "الدورة",
    "وزاري",
    "تدريب",
    "تدرب",
    "past exam",
    "exam",
    "practice",
    "tawjihi",
}


def _is_greeting(message: str) -> bool:
    normalized = message.casefold().strip(" .!؟?,،")
    greetings = {item.casefold().strip(" .!؟?,،") for item in GREETINGS}
    return normalized in greetings


def _is_exam_practice_request(message: str) -> bool:
    normalized = message.casefold()
    return any(keyword in normalized for keyword in EXAM_PRACTICE_KEYWORDS)


def _english_retrieval_query(message: str, exam_practice: bool) -> str:
    if not exam_practice:
        return message

    return (
        f"{message}\n"
        "English Tawjihi past exam questions الدورة الأولى الدورة الثانية الدورة الثالثة "
        "2023 2024 scientific literary grammar reading writing"
    )


def _math_retrieval_query(message: str, exam_practice: bool) -> str:
    if not exam_practice:
        return message

    return (
        f"{message}\n"
        "رياضيات توجيهي امتحان وزاري تدريب أسئلة سنوات تفاضل تكامل جبر هندسة احتمالات "
        "Math Tawjihi past exam questions ministry exam practice scientific stream"
    )


def _retrieval_query(subject: str, message: str, exam_practice: bool) -> str:
    if subject == "english":
        return _english_retrieval_query(message, exam_practice)
    if subject == "math":
        return _math_retrieval_query(message, exam_practice)
    return message


def _prioritize_exam_sources(results: list[dict], subject: str) -> list[dict]:
    preferred_sources = {
        "english": ENGLISH_EXAM_SOURCES,
        "math": MATH_EXAM_SOURCES,
    }.get(subject, set())

    return sorted(
        results,
        key=lambda item: (
            (item.get("source") or "").casefold() not in {s.casefold() for s in preferred_sources},
            -float(item.get("score", 0)),
        ),
    )


def _subject_index_is_stale(subject: str) -> bool:
    subject_pdf_dir = PDFS_DIR / subject
    subject_index_dir = VECTOR_DB_DIR / subject
    index_files = [
        subject_index_dir / "faiss.index",
        subject_index_dir / "chunks.pkl",
    ]

    if not subject_pdf_dir.exists() or not all(path.exists() for path in index_files):
        return False

    pdf_files = list(subject_pdf_dir.glob("*.pdf"))
    if not pdf_files:
        return False

    newest_pdf = max(path.stat().st_mtime for path in pdf_files)
    oldest_index_file = min(path.stat().st_mtime for path in index_files)
    return newest_pdf > oldest_index_file


def _ensure_subject_index(subject: str) -> None:
    if subject not in SUPPORTED_SUBJECTS:
        raise ValueError(f"Unsupported subject: {subject}")

    if _subject_index_is_stale(subject):
        ingest_subject(subject)

    if not load_index(subject):
        if not has_index(subject):
            ingest_subject(subject)
        load_index(subject)


def chat(
    message: str,
    subject: str = "math",
    image_data: str | None = None,
    image_mime_type: str | None = None,
) -> str:
    subject = (subject or "math").strip().lower()
    message = (message or "").strip()
    has_image = bool((image_data or "").strip())

    if not message and not has_image:
        return "اكتب سؤالك أو أرفق صورة أولاً."

    if message and not has_image and _is_greeting(message):
        return "أهلاً وسهلاً، كيف أقدر أساعدك اليوم؟"

    if subject not in SUPPORTED_SUBJECTS:
        return "المادة غير مدعومة. اختر الرياضيات أو اللغة الإنجليزية."

    context = ""
    if message:
        _ensure_subject_index(subject)
        exam_practice = subject in {"english", "math"} and _is_exam_practice_request(message)
        query = _retrieval_query(subject, message, exam_practice)
        results = retrieve(query, subject=subject, k=12 if exam_practice else 6)
        if exam_practice:
            results = _prioritize_exam_sources(results, subject)[:8]
        context = format_context(results)

    return SUPPORTED_SUBJECTS[subject].answer(
        message,
        context,
        image_data=image_data,
        image_mime_type=image_mime_type,
    )
