import { useEffect, useMemo, useState, useRef } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { useLocation, useNavigate, useSearchParams } from "react-router-dom";
import {
  ChevronRight,
  ChevronLeft,
  Clock,
  CheckCircle2,
  XCircle,
  AlertCircle,
  Flag,
  RotateCcw,
  Lightbulb,
  Loader2,
} from "lucide-react";
import DashboardLayout from "../../../components/DashboardLayout";
import { API_BASE_URL } from "@/config/api";

const getAuthToken = () => {
  return sessionStorage.getItem("token") || localStorage.getItem("token");
};

const normalizeStartedExam = (data) => ({
  examId: data.examId || data.ExamId,
  subjectId: data.subjectId || data.SubjectId,
  subjectName: data.subjectName || data.SubjectName || "",
  type: data.type || data.Type || "",
  typeName: data.typeName || data.TypeName || "",
  lessonId: data.lessonId || data.LessonId || null,
  lessonTitle: data.lessonTitle || data.LessonTitle || null,
  questions: (data.questions || data.Questions || []).map((question) => ({
    questionId: question.questionId || question.QuestionId,
    text: question.text || question.Text || "",
    options: (question.options || question.Options || []).map((option) => ({
      key: option.key || option.Key,
      text: option.text || option.Text || "",
    })),
  })),
});

const normalizeExamResult = (data) => ({
  examResultId: data.examResultId || data.ExamResultId,
  examId: data.examId || data.ExamId,
  score: data.score ?? data.Score ?? 0,
  totalQuestions: data.totalQuestions ?? data.TotalQuestions ?? 0,
  correctAnswers: data.correctAnswers ?? data.CorrectAnswers ?? 0,
  wrongAnswers: data.wrongAnswers ?? data.WrongAnswers ?? 0,
  percentage: data.percentage ?? data.Percentage ?? 0,
  strengthAreas: data.strengthAreas || data.StrengthAreas || [],
  weakAreas: data.weakAreas || data.WeakAreas || [],
  levelMessage: data.levelMessage || data.LevelMessage || "",
  recommendationText: data.recommendationText || data.RecommendationText || "",
  createdAt: data.createdAt || data.CreatedAt || null,
  review: (data.review || data.Review || []).map((item) => ({
    questionId: item.questionId || item.QuestionId,
    questionText: item.questionText || item.QuestionText || "",
    selectedAnswer: item.selectedAnswer || item.SelectedAnswer || null,
    selectedAnswerText: item.selectedAnswerText || item.SelectedAnswerText || "",
    correctAnswer: item.correctAnswer || item.CorrectAnswer || "",
    correctAnswerText: item.correctAnswerText || item.CorrectAnswerText || "",
    isCorrect: item.isCorrect ?? item.IsCorrect ?? false,
    solution: item.solution || item.Solution || "",
  })),
});

const ExamTake = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const [searchParams] = useSearchParams();
  const token = getAuthToken();

  const examId = searchParams.get("examId");
  const resultId = searchParams.get("resultId");
  const mode = searchParams.get("mode");

  const startedExamFromState = useMemo(() => {
    const exam = location.state?.startedExam;

    if (!exam) return null;

    return normalizeStartedExam(exam);
  }, [location.state]);

  const isReviewMode = !!resultId || mode === "review";

  const [startedExam, setStartedExam] = useState(startedExamFromState);
  const [loading, setLoading] = useState(true);
  const [pageError, setPageError] = useState("");

  const [current, setCurrent] = useState(0);
  const [answers, setAnswers] = useState({});
  const [flagged, setFlagged] = useState(new Set());
  const [submitted, setSubmitted] = useState(false);

  const [timeLeft, setTimeLeft] = useState(0);
  const [timerReady, setTimerReady] = useState(false);

  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState("");
  const [result, setResult] = useState(null);
  const [expandedSolution, setExpandedSolution] = useState(null);

  const answersRef = useRef({});

  useEffect(() => {
    const initialize = async () => {
      if (!token) {
        navigate("/login");
        return;
      }

      if (isReviewMode) {
        if (!resultId) {
          setPageError("رقم النتيجة غير موجود.");
          setLoading(false);
          return;
        }

        try {
          setLoading(true);
          setPageError("");

          const response = await fetch(`${API_BASE_URL}/api/student/exams/results/${resultId}`, {
            method: "GET",
            headers: {
              Authorization: `Bearer ${token}`,
            },
          });

          const rawText = await response.text();
          let data;

          try {
            data = JSON.parse(rawText);
          } catch {
            data = { message: rawText };
          }

          if (!response.ok) {
            setPageError(data?.message || "فشل تحميل نتيجة الامتحان.");
            return;
          }

          setResult(normalizeExamResult(data));
          setSubmitted(true);
        } catch (err) {
          console.error(err);
          setPageError("تعذر الاتصال بالسيرفر.");
        } finally {
          setLoading(false);
        }

        return;
      }

      if (!examId) {
        setPageError("رقم الامتحان غير موجود.");
        setLoading(false);
        return;
      }

      if (startedExamFromState) {
        const initialTime = Math.max(startedExamFromState.questions.length * 90, 60);

        setTimeLeft(initialTime);
        setStartedExam(startedExamFromState);
        setTimerReady(true);
        setLoading(false);
        return;
      }

      setPageError("تعذر تحميل بيانات الامتحان. ابدئي الامتحان من صفحة إنشاء امتحان.");
      setLoading(false);
    };

    initialize();
  }, [token, navigate, examId, resultId, isReviewMode, startedExamFromState]);

  useEffect(() => {
    if (!timerReady || submitted || !startedExam || isReviewMode) return;

    const interval = setInterval(() => {
      setTimeLeft((previous) => {
        if (previous <= 1) {
          clearInterval(interval);

          setTimeout(() => {
            handleSubmitExam();
          }, 0);

          return 0;
        }

        return previous - 1;
      });
    }, 1000);

    return () => clearInterval(interval);
  }, [timerReady, submitted, startedExam, isReviewMode]);

  const questions = startedExam?.questions || [];
  const q = questions[current];

  const formatTime = (seconds) => {
    const minutes = Math.floor(seconds / 60);
    const sec = seconds % 60;

    return `${minutes}:${sec.toString().padStart(2, "0")}`;
  };

  const selectAnswer = (key) => {
    if (submitted || !q) return;

    setAnswers((previous) => {
      const updated = {
        ...previous,
        [q.questionId]: key,
      };

      answersRef.current = updated;

      return updated;
    });
  };

  const toggleFlag = () => {
    if (!q) return;

    setFlagged((previous) => {
      const next = new Set(previous);

      if (next.has(q.questionId)) {
        next.delete(q.questionId);
      } else {
        next.add(q.questionId);
      }

      return next;
    });
  };

  const handleSubmitExam = async () => {
    if (!examId || !startedExam || submitting) return;

    try {
      setSubmitting(true);
      setSubmitError("");

      const payload = {
        answers: questions
          .filter((question) => answersRef.current[question.questionId])
          .map((question) => ({
            questionId: question.questionId,
            selectedAnswer: answersRef.current[question.questionId],
          })),
      };

      const response = await fetch(`${API_BASE_URL}/api/student/exams/${examId}/submit`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(payload),
      });

      const rawText = await response.text();
      let data;

      try {
        data = JSON.parse(rawText);
      } catch {
        data = { message: rawText };
      }

      if (!response.ok) {
        setSubmitError(data?.message || "فشل إرسال الامتحان");
        return;
      }

      setResult(normalizeExamResult(data));
      setSubmitted(true);
    } catch (err) {
      console.error(err);
      setSubmitError("تعذر الاتصال بالسيرفر");
    } finally {
      setSubmitting(false);
    }
  };

  const reviewItems = useMemo(() => result?.review || [], [result]);

  if (loading) {
    return (
      <DashboardLayout
        title={isReviewMode ? "جاري تحميل النتيجة..." : "جاري تحميل الامتحان..."}
        subtitle="يرجى الانتظار"
        hideSearch
      >
        <div
          className="card"
          style={{
            padding: "2rem",
            textAlign: "center",
            minHeight: "280px",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            gap: "0.75rem",
          }}
        >
          <Loader2 className="animate-spin" />
          <span>{isReviewMode ? "جاري تحميل النتيجة..." : "جاري تحميل الامتحان..."}</span>
        </div>
      </DashboardLayout>
    );
  }

  if (pageError) {
    return (
      <DashboardLayout title="الامتحان" subtitle="حدثت مشكلة" hideSearch>
        <div className="card" style={{ padding: "2rem", textAlign: "center", color: "red" }}>
          <p style={{ marginBottom: "1rem" }}>{pageError}</p>

          <button className="btn btn-primary" onClick={() => navigate("/exams")}>
            العودة للامتحانات
          </button>
        </div>
      </DashboardLayout>
    );
  }

  return (
    <DashboardLayout
      title={
        isReviewMode
          ? "مراجعة نتيجة الامتحان"
          : `${startedExam?.typeName || "امتحان"} — ${startedExam?.subjectName || ""}`
      }
      subtitle={
        isReviewMode
          ? "راجع إجاباتك واعرف أخطاءك"
          : `${questions.length} سؤال`
      }
      hideSearch
    >
      {!submitted ? (
        <div className="et-layout">
          <aside className="et-sidebar">
            <div className="et-timer">
              <Clock size={18} />
              <span>{formatTime(timeLeft)}</span>
            </div>

            <div className="et-qnav">
              {questions.map((question, index) => (
                <button
                  key={question.questionId}
                  className={`et-qnav-btn ${
                    index === current ? "et-qnav-active" : ""
                  } ${answers[question.questionId] != null ? "et-qnav-answered" : ""} ${
                    flagged.has(question.questionId) ? "et-qnav-flagged" : ""
                  }`}
                  onClick={() => setCurrent(index)}
                >
                  {index + 1}
                </button>
              ))}
            </div>

            <button className="et-submit-btn" onClick={handleSubmitExam} disabled={submitting}>
              {submitting ? "جاري الإرسال..." : "إنهاء الامتحان"}
            </button>

            {submitError && (
              <p style={{ color: "red", marginTop: "0.75rem", fontSize: "0.875rem" }}>
                {submitError}
              </p>
            )}
          </aside>

          <main className="et-main">
            <div className="et-progress-bar">
              <div
                className="et-progress-fill"
                style={{ width: `${((current + 1) / questions.length) * 100}%` }}
              />
            </div>

            <AnimatePresence mode="wait">
              <motion.div
                key={q?.questionId}
                className="et-question-card"
                initial={{ opacity: 0, x: -20 }}
                animate={{ opacity: 1, x: 0 }}
                exit={{ opacity: 0, x: 20 }}
                transition={{ duration: 0.25 }}
              >
                <div className="et-q-header">
                  <span className="et-q-num">
                    سؤال {current + 1} من {questions.length}
                  </span>

                  <button
                    className={`et-flag-btn ${
                      flagged.has(q?.questionId) ? "et-flag-active" : ""
                    }`}
                    onClick={toggleFlag}
                  >
                    <Flag size={16} />
                  </button>
                </div>

                <h3 className="et-q-text">{q?.text}</h3>

                <div className="et-options">
                  {(q?.options || []).map((option, index) => (
                    <motion.button
                      key={option.key}
                      className={`et-option ${
                        answers[q.questionId] === option.key ? "et-option-selected" : ""
                      }`}
                      onClick={() => selectAnswer(option.key)}
                      whileTap={{ scale: 0.98 }}
                    >
                      <span className="et-option-letter">{["أ", "ب", "ج", "د"][index]}</span>
                      <span>{option.text}</span>
                    </motion.button>
                  ))}
                </div>
              </motion.div>
            </AnimatePresence>

            <div className="et-nav-buttons">
              <button
                className="et-nav-btn"
                disabled={current === 0}
                onClick={() => setCurrent((previous) => previous - 1)}
              >
                <ChevronRight size={18} /> السابق
              </button>

              <button
                className="et-nav-btn et-nav-btn-primary"
                disabled={current === questions.length - 1}
                onClick={() => setCurrent((previous) => previous + 1)}
              >
                التالي <ChevronLeft size={18} />
              </button>
            </div>
          </main>
        </div>
      ) : (
        <motion.div
          className="et-results"
          initial={{ opacity: 0, scale: 0.95 }}
          animate={{ opacity: 1, scale: 1 }}
        >
          <div
            className={`et-results-icon ${
              (result?.percentage || 0) >= 50 ? "et-results-pass" : "et-results-fail"
            }`}
          >
            {(result?.percentage || 0) >= 50 ? (
              <CheckCircle2 size={48} />
            ) : (
              <AlertCircle size={48} />
            )}
          </div>

          <h2 className="et-results-title">
            {(result?.percentage || 0) >= 90
              ? "ممتاز!"
              : (result?.percentage || 0) >= 70
              ? "أحسنت!"
              : (result?.percentage || 0) >= 50
              ? "جيد، واصل التحسن!"
              : "حاول مرة أخرى"}
          </h2>

          <p className="et-results-score">
            {result?.correctAnswers || 0} / {result?.totalQuestions || 0} —{" "}
            {result?.percentage || 0}%
          </p>

          <div className="et-results-summary-bar">
            <div className="et-results-summary-item et-summary-correct">
              <CheckCircle2 size={16} />
              <span>{result?.correctAnswers || 0} صحيحة</span>
            </div>

            <div className="et-results-summary-item et-summary-wrong">
              <XCircle size={16} />
              <span>{result?.wrongAnswers || 0} خاطئة</span>
            </div>
          </div>

          {result?.levelMessage && (
            <div className="card" style={{ padding: "1rem", marginBottom: "1rem" }}>
              <p style={{ textAlign: "center" }}>{result.levelMessage}</p>
            </div>
          )}

          {result?.recommendationText && (
            <div className="card" style={{ padding: "1rem", marginBottom: "1rem" }}>
              <p style={{ textAlign: "center" }}>{result.recommendationText}</p>
            </div>
          )}

          <h3 className="et-review-heading">📋 مراجعة الإجابات والحلول</h3>

          <div className="et-results-review">
            {reviewItems.length === 0 ? (
              <div className="card" style={{ padding: "1.5rem", textAlign: "center" }}>
                لا توجد تفاصيل مراجعة لهذا الامتحان.
              </div>
            ) : (
              reviewItems.map((question, index) => {
                const isExpanded = expandedSolution === question.questionId;

                return (
                  <motion.div
                    key={question.questionId}
                    className={`et-review-item ${
                      question.isCorrect ? "et-review-correct" : "et-review-wrong"
                    }`}
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: index * 0.05 }}
                  >
                    <div className="et-review-icon">
                      {question.isCorrect ? (
                        <CheckCircle2 size={20} />
                      ) : (
                        <XCircle size={20} />
                      )}
                    </div>

                    <div className="et-review-body">
                      <span className="et-review-num">سؤال {index + 1}</span>

                      <p className="et-review-question">{question.questionText}</p>

                      <div className="et-review-answers">
                        <div
                          className={`et-review-answer-row ${
                            question.isCorrect ? "et-answer-correct" : "et-answer-wrong"
                          }`}
                        >
                          <span className="et-answer-label">إجابتك:</span>
                          <span>{question.selectedAnswerText || "لم تُجب"}</span>
                        </div>

                        {!question.isCorrect && (
                          <div className="et-review-answer-row et-answer-correct">
                            <span className="et-answer-label">الإجابة الصحيحة:</span>
                            <span>{question.correctAnswerText}</span>
                          </div>
                        )}
                      </div>

                      {question.solution && (
                        <>
                          <button
                            className="et-solution-toggle"
                            onClick={() =>
                              setExpandedSolution(isExpanded ? null : question.questionId)
                            }
                          >
                            <Lightbulb size={15} />
                            {isExpanded ? "إخفاء الحل" : "عرض الحل خطوة بخطوة"}
                          </button>

                          <AnimatePresence>
                            {isExpanded && (
                              <motion.div
                                className="et-solution-box"
                                initial={{ opacity: 0, height: 0 }}
                                animate={{ opacity: 1, height: "auto" }}
                                exit={{ opacity: 0, height: 0 }}
                                transition={{ duration: 0.3 }}
                              >
                                <div className="et-solution-content">
                                  {question.solution.split("\n").map((line, lineIndex) => (
                                    <p key={lineIndex}>{line}</p>
                                  ))}
                                </div>
                              </motion.div>
                            )}
                          </AnimatePresence>
                        </>
                      )}
                    </div>
                  </motion.div>
                );
              })
            )}
          </div>

          <div className="et-results-actions">
            <button className="et-nav-btn et-nav-btn-primary" onClick={() => navigate("/exams/new")}>
              <RotateCcw size={16} /> اختبار جديد
            </button>

            <button className="et-nav-btn" onClick={() => navigate("/exams")}>
              العودة للامتحانات
            </button>
          </div>
        </motion.div>
      )}
    </DashboardLayout>
  );
};

export default ExamTake;
