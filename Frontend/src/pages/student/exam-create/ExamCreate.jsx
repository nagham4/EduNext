import { useEffect, useMemo, useState } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { useNavigate } from "react-router-dom";
import {
  Sigma,
  Atom,
  Languages,
  FlaskConical,
  Dna,
  Zap,
  ClipboardCheck,
  Play,
  CheckCircle2,
  BookOpen,
  Loader2,
  Sparkles,
} from "lucide-react";
import DashboardLayout from "../../../components/DashboardLayout";
import { API_BASE_URL } from "@/config/api";

const iconMap = {
  Sigma,
  Atom,
  Languages,
  FlaskConical,
  Dna,
  BookOpen,
  Sparkles,
  Globe: Languages,
  BookMarked: BookOpen,
};

const examTypes = [
  {
    id: "comprehensive",
    label: "امتحان شامل (نهائي)",
    icon: ClipboardCheck,
    desc: "يغطي جميع دروس المادة المختارة. يحاكي نمط الامتحانات النهائية لقياس مستواك العام.",
    color: "hsl(220, 85%, 55%)",
    badge: "جميع الدروس",
  },
  {
    id: "short",
    label: "اختبار قصير",
    icon: Zap,
    desc: "اختبار محدد بدرس واحد تختاره أنت. مثالي للمراجعة السريعة بعد الدراسة اليومية.",
    color: "hsl(30, 90%, 55%)",
    badge: "درس واحد",
  },
];

const normalizeSubject = (subject) => ({
  subjectId: subject.subjectId || subject.SubjectId,
  subjectName: subject.subjectName || subject.SubjectName || "",
  iconKey: subject.iconKey || subject.IconKey || "BookOpen",
  color: subject.color || subject.Color || "blue",
});

const normalizeLesson = (lesson) => ({
  lessonId: lesson.lessonId || lesson.LessonId,
  lessonTitle: lesson.lessonTitle || lesson.LessonTitle || "",
  orderNumber: lesson.orderNumber ?? lesson.OrderNumber ?? 0,
  displayOrder: lesson.displayOrder ?? lesson.DisplayOrder ?? 0,
  unitTitle: lesson.unitTitle || lesson.UnitTitle || "بدون وحدة",
});

const ExamCreate = () => {
  const navigate = useNavigate();
  const token = localStorage.getItem("token");

  const [subjects, setSubjects] = useState([]);
  const [subjectsLoading, setSubjectsLoading] = useState(true);
  const [subjectsError, setSubjectsError] = useState("");

  const [selectedSubject, setSelectedSubject] = useState(null);
  const [selectedType, setSelectedType] = useState(null);
  const [selectedLesson, setSelectedLesson] = useState(null);

  const [lessons, setLessons] = useState([]);
  const [lessonsLoading, setLessonsLoading] = useState(false);
  const [lessonsError, setLessonsError] = useState("");

  const [startingExam, setStartingExam] = useState(false);
  const [startError, setStartError] = useState("");

  useEffect(() => {
    const fetchSubjects = async () => {
      if (!token) {
        navigate("/login");
        return;
      }

      try {
        setSubjectsLoading(true);
        setSubjectsError("");

        const response = await fetch(`${API_BASE_URL}/api/student/exams/subjects`, {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        });

        const rawText = await response.text();
        let data;

        try {
          data = JSON.parse(rawText);
        } catch {
          data = [];
        }

        if (!response.ok) {
          setSubjectsError(data?.message || "فشل تحميل المواد");
          return;
        }

        setSubjects((Array.isArray(data) ? data : []).map(normalizeSubject));
      } catch (err) {
        console.error(err);
        setSubjectsError("تعذر الاتصال بالسيرفر");
      } finally {
        setSubjectsLoading(false);
      }
    };

    fetchSubjects();
  }, [token, navigate]);

  useEffect(() => {
    const fetchLessons = async () => {
      if (!selectedSubject || selectedType !== "short") {
        setLessons([]);
        return;
      }

      try {
        setLessonsLoading(true);
        setLessonsError("");
        setSelectedLesson(null);

        const response = await fetch(
          `${API_BASE_URL}/api/student/exams/subjects/${selectedSubject}/lessons`,
          {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
        );

        const rawText = await response.text();
        let data;

        try {
          data = JSON.parse(rawText);
        } catch {
          data = [];
        }

        if (!response.ok) {
          setLessonsError(data?.message || "فشل تحميل الدروس");
          return;
        }

        setLessons((Array.isArray(data) ? data : []).map(normalizeLesson));
      } catch (err) {
        console.error(err);
        setLessonsError("تعذر الاتصال بالسيرفر");
      } finally {
        setLessonsLoading(false);
      }
    };

    fetchLessons();
  }, [selectedSubject, selectedType, token]);

  const isComprehensive = selectedType === "comprehensive";
  const canStart =
    !!selectedSubject &&
    !!selectedType &&
    (isComprehensive ? true : !!selectedLesson);

  const steps = ["اختر المادة الدراسية", "اختر نوع الامتحان", "اختر الدرس"];

  const currentStep = !selectedSubject
    ? 0
    : !selectedType
    ? 1
    : isComprehensive
    ? 2
    : !selectedLesson
    ? 2
    : 3;

  const selectedSubjectObj = useMemo(
    () => subjects.find((s) => s.subjectId === selectedSubject),
    [subjects, selectedSubject]
  );

  const groupedLessons = useMemo(() => {
    const groups = [];

    lessons.forEach((lesson) => {
      const unitTitle = lesson.unitTitle || "بدون وحدة";
      const existing = groups.find((g) => g.unitTitle === unitTitle);

      if (existing) {
        existing.lessons.push(lesson);
      } else {
        groups.push({
          unitTitle,
          lessons: [lesson],
        });
      }
    });

    return groups;
  }, [lessons]);

  const handleStart = async () => {
    if (!canStart || !selectedSubject || !selectedType) return;

    try {
      setStartingExam(true);
      setStartError("");

      const body = {
        subjectId: selectedSubject,
        type: selectedType,
        lessonId: selectedType === "short" ? selectedLesson : null,
      };

      const response = await fetch(`${API_BASE_URL}/api/student/exams/start`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(body),
      });

      const rawText = await response.text();
      let data;

      try {
        data = JSON.parse(rawText);
      } catch {
        data = { message: rawText };
      }

      if (!response.ok) {
        setStartError(data?.message || "فشل بدء الامتحان");
        return;
      }

      navigate(`/exams/take?examId=${data.examId || data.ExamId}`, {
        state: { startedExam: data },
      });
    } catch (err) {
      console.error(err);
      setStartError("تعذر الاتصال بالسيرفر");
    } finally {
      setStartingExam(false);
    }
  };

  return (
    <DashboardLayout
      title="بدء اختبار جديد"
      subtitle="خصّص تجربة الاختبار الخاصة بك للوصول إلى التميز في التوجيهي"
      hideSearch
    >
      <nav className="ec-breadcrumb">
        <span onClick={() => navigate("/dashboard")}>الرئيسية</span>
        <span className="ec-breadcrumb-sep">›</span>
        <span onClick={() => navigate("/exams")}>الامتحانات</span>
        <span className="ec-breadcrumb-sep">›</span>
        <span className="ec-breadcrumb-current">بدء اختبار جديد</span>
      </nav>

      <div className="ec-stepper">
        {steps.map((label, i) => {
          const stepIndex = i + 1;
          const isActive = currentStep === stepIndex - 1 || currentStep === stepIndex;
          const isDone = currentStep > stepIndex - 1;

          return (
            <div key={i} className="ec-stepper-item">
              {i > 0 && (
                <div
                  className={`ec-stepper-line ${
                    currentStep >= stepIndex - 1 ? "ec-stepper-line-active" : ""
                  }`}
                />
              )}

              <motion.div
                className={`ec-stepper-circle ${
                  isDone
                    ? "ec-stepper-circle-done"
                    : isActive
                    ? "ec-stepper-circle-active"
                    : ""
                }`}
                animate={isActive ? { scale: [1, 1.08, 1] } : {}}
                transition={{ duration: 0.45, ease: "easeInOut" }}
              >
                {isDone && currentStep > stepIndex ? (
                  <CheckCircle2 size={18} />
                ) : (
                  <span>{["١", "٢", "٣"][i]}</span>
                )}
              </motion.div>

              <span
                className={`ec-stepper-label ${
                  isActive
                    ? "ec-stepper-label-active"
                    : isDone
                    ? "ec-stepper-label-done"
                    : ""
                }`}
              >
                {label}
              </span>
            </div>
          );
        })}
      </div>

      <div className="ec-container">
        <section className="ec-section">
          <div className="ec-step-header">
            <div className={`ec-step-dot ${currentStep >= 0 ? "ec-step-dot-active" : ""}`}>
              {currentStep > 0 ? <CheckCircle2 size={18} /> : "١"}
            </div>
            <h2 className="ec-step-title">اختر المادة الدراسية</h2>
          </div>

          {subjectsLoading ? (
            <div className="card" style={{ padding: "2rem", textAlign: "center" }}>
              <Loader2 className="animate-spin" style={{ margin: "0 auto 1rem" }} />
              جاري تحميل المواد...
            </div>
          ) : subjectsError ? (
            <div className="card" style={{ padding: "2rem", textAlign: "center", color: "red" }}>
              {subjectsError}
            </div>
          ) : subjects.length === 0 ? (
            <div className="card" style={{ padding: "2rem", textAlign: "center" }}>
              لا توجد مواد متاحة لهذا الطالب حالياً.
            </div>
          ) : (
            <div className="ec-subjects-grid">
              {subjects.map((s) => {
                const Icon = iconMap[s.iconKey] || BookOpen;

                return (
                  <motion.button
                    key={s.subjectId}
                    className={`ec-subject-card ${
                      selectedSubject === s.subjectId ? "ec-subject-card-active" : ""
                    }`}
                    onClick={() => {
                      setSelectedSubject(s.subjectId);
                      setSelectedLesson(null);
                      setSelectedType(null);
                      setLessons([]);
                      setLessonsError("");
                      setStartError("");
                    }}
                    whileHover={{ y: -2 }}
                    whileTap={{ scale: 0.97 }}
                  >
                    <Icon size={28} />
                    <span>{s.subjectName}</span>
                  </motion.button>
                );
              })}
            </div>
          )}
        </section>

        <AnimatePresence>
          {selectedSubject && (
            <motion.section
              className="ec-section"
              initial={{ opacity: 0, y: 16 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: 16 }}
            >
              <div className="ec-step-header">
                <div className={`ec-step-dot ${currentStep >= 1 ? "ec-step-dot-active" : ""}`}>
                  {currentStep > 1 ? <CheckCircle2 size={18} /> : "٢"}
                </div>
                <h2 className="ec-step-title">اختر نوع الامتحان</h2>
              </div>

              <div className="ec-type-grid">
                {examTypes.map((t) => (
                  <motion.button
                    key={t.id}
                    className={`ec-type-card ${selectedType === t.id ? "ec-type-card-active" : ""}`}
                    onClick={() => {
                      setSelectedType(t.id);
                      setSelectedLesson(null);
                      setStartError("");
                    }}
                    whileHover={{ y: -2 }}
                    whileTap={{ scale: 0.98 }}
                  >
                    <div className="ec-type-radio">
                      {selectedType === t.id && <div className="ec-type-radio-dot" />}
                    </div>

                    <div
                      className="ec-type-icon"
                      style={{ background: `${t.color}15`, color: t.color }}
                    >
                      <t.icon size={24} />
                    </div>

                    <h3>{t.label}</h3>
                    <span className="ec-type-badge-label">{t.badge}</span>
                    <p>{t.desc}</p>
                  </motion.button>
                ))}
              </div>
            </motion.section>
          )}
        </AnimatePresence>

        <AnimatePresence>
          {selectedType === "short" && selectedSubject && (
            <motion.section
              className="ec-section ec-section-lessons"
              initial={{ opacity: 0, y: 16 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: 16 }}
            >
              <div className="ec-step-header">
                <div className={`ec-step-dot ${selectedLesson ? "ec-step-dot-active" : ""}`}>
                  ٣
                </div>
                <h2 className="ec-step-title">اختر الدرس</h2>
              </div>

              <div className="ec-lessons-wrap">
                <p className="ec-lessons-hint">اختر درسًا واحدًا لإنشاء اختبار قصير عليه</p>

                {lessonsLoading ? (
                  <div className="card" style={{ padding: "1.5rem", textAlign: "center" }}>
                    <Loader2 className="animate-spin" style={{ margin: "0 auto 1rem" }} />
                    جاري تحميل الدروس...
                  </div>
                ) : lessonsError ? (
                  <div className="card" style={{ padding: "1.5rem", textAlign: "center", color: "red" }}>
                    {lessonsError}
                  </div>
                ) : groupedLessons.length === 0 ? (
                  <div className="card" style={{ padding: "1.5rem", textAlign: "center" }}>
                    لا توجد دروس متاحة لهذه المادة حالياً.
                  </div>
                ) : (
                  <div style={{ display: "flex", flexDirection: "column", gap: "1rem" }}>
                    {groupedLessons.map((group, groupIndex) => (
                      <div key={`${group.unitTitle}-${groupIndex}`}>
                        <div
                          style={{
                            padding: "0.75rem 1rem",
                            borderRadius: "0.75rem",
                            background: "var(--primary-light)",
                            marginBottom: "0.75rem",
                            fontWeight: 700,
                          }}
                        >
                          {group.unitTitle}
                        </div>

                        <div className="ec-lessons-grid">
                          {group.lessons.map((l) => (
                            <motion.button
                              key={l.lessonId}
                              className={`ec-lesson-pill ${
                                selectedLesson === l.lessonId ? "ec-lesson-pill-active" : ""
                              }`}
                              onClick={() => setSelectedLesson(l.lessonId)}
                              whileTap={{ scale: 0.96 }}
                            >
                              {selectedLesson === l.lessonId && <CheckCircle2 size={16} />}
                              <span>{l.lessonTitle}</span>
                            </motion.button>
                          ))}
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </motion.section>
          )}
        </AnimatePresence>

        <AnimatePresence>
          {selectedType === "comprehensive" && selectedSubject && (
            <motion.div
              className="ec-comprehensive-info"
              initial={{ opacity: 0, y: 16 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: 16 }}
            >
              <ClipboardCheck size={22} />
              <div>
                <h3>الامتحان الشامل</h3>
                <p>
                  سيتم تضمين جميع دروس مادة {selectedSubjectObj?.subjectName || ""} تلقائيًا.
                </p>
              </div>
            </motion.div>
          )}
        </AnimatePresence>

        {startError && (
          <div className="card" style={{ padding: "1rem", textAlign: "center", color: "red" }}>
            {startError}
          </div>
        )}
      </div>

      <div className="ec-bottom-bar">
        <button className="ec-cancel-btn" onClick={() => navigate("/exams")}>
          إلغاء
        </button>

        <motion.button
          className="ec-start-btn"
          disabled={!canStart || startingExam}
          whileHover={canStart && !startingExam ? { scale: 1.02 } : {}}
          whileTap={canStart && !startingExam ? { scale: 0.98 } : {}}
          onClick={handleStart}
        >
          {startingExam ? (
            <>
              <Loader2 size={18} className="animate-spin" />
              جاري البدء...
            </>
          ) : (
            <>
              <Play size={18} />
              بدء الامتحان الآن
            </>
          )}
        </motion.button>
      </div>
    </DashboardLayout>
  );
};

export default ExamCreate;