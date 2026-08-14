import { useEffect, useMemo, useState } from "react";
import { motion } from "framer-motion";
import { useNavigate } from "react-router-dom";
import {
  FileText,
  Award,
  BookMarked,
  Atom,
  Languages,
  FlaskConical,
  Plus,
  Filter,
  BookOpen,
  Dna,
  Loader2,
  Eye,
  Trash2,
} from "lucide-react";
import DashboardLayout from "../../../components/DashboardLayout";
import { API_BASE_URL } from "@/config/api";

const iconMap = {
  math: BookMarked,
  physics: Atom,
  arabic: Languages,
  chemistry: FlaskConical,
  english: Languages,
  biology: Dna,
  subject: BookOpen,
};

const colorMap = {
  math: "blue",
  physics: "green",
  arabic: "amber",
  chemistry: "purple",
  english: "blue",
  biology: "green",
  subject: "blue",
};

const getAuthToken = () => {
  return sessionStorage.getItem("token") || localStorage.getItem("token");
};

const normalizeExamRecord = (exam) => ({
  examResultId: exam.examResultId || exam.ExamResultId,
  examId: exam.examId || exam.ExamId,
  subjectId: exam.subjectId || exam.SubjectId,
  subjectName: exam.subjectName || exam.SubjectName || "",
  subject: exam.subjectKey || exam.SubjectKey || "subject",
  lessonId: exam.lessonId || exam.LessonId || null,
  lessonTitle: exam.lessonTitle || exam.LessonTitle || null,
  type: exam.type || exam.Type || "",
  typeName: exam.typeName || exam.TypeName || "",
  score: exam.score ?? exam.Score ?? 0,
  total: exam.questionsCount ?? exam.QuestionsCount ?? 0,
  percentage: exam.percentage ?? exam.Percentage ?? 0,
  date: exam.date || exam.Date || "",
});

const isShortType = (type) => {
  return type === "short" || type === "quick";
};

const Exams = () => {
  const navigate = useNavigate();
  const token = getAuthToken();

  const [records, setRecords] = useState([]);
  const [loading, setLoading] = useState(true);
  const [pageError, setPageError] = useState("");
  const [deleteError, setDeleteError] = useState("");
  const [deletingResultId, setDeletingResultId] = useState(null);
  const [pendingDeleteExam, setPendingDeleteExam] = useState(null);

  const [filterType, setFilterType] = useState("all");
  const [filterSubject, setFilterSubject] = useState("all");

  useEffect(() => {
    const fetchExamHistory = async () => {
      if (!token) {
        navigate("/login");
        return;
      }

      try {
        setLoading(true);
        setPageError("");

        const response = await fetch(`${API_BASE_URL}/api/student/exams`, {
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
          data = [];
        }

        if (!response.ok) {
          setPageError(data?.message || "فشل تحميل سجل الامتحانات");
          return;
        }

        setRecords((Array.isArray(data) ? data : []).map(normalizeExamRecord));
      } catch (err) {
        console.error(err);
        setPageError("تعذر الاتصال بالسيرفر");
      } finally {
        setLoading(false);
      }
    };

    fetchExamHistory();
  }, [token, navigate]);

  const handleDeleteResult = async (examResultId) => {
    try {
      setDeletingResultId(examResultId);
      setDeleteError("");

      const response = await fetch(
        `${API_BASE_URL}/api/student/exams/results/${examResultId}`,
        {
          method: "DELETE",
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );

      if (!response.ok) {
        const data = await response.json();
        throw new Error(data?.message || "فشل حذف نتيجة الامتحان.");
      }

      setRecords((prev) => prev.filter((record) => record.examResultId !== examResultId));
      setPendingDeleteExam(null);
    } catch (err) {
      console.error(err);
      setDeleteError(err.message || "تعذر حذف نتيجة الامتحان.");
    } finally {
      setDeletingResultId(null);
    }
  };

  const filteredRecords = useMemo(() => {
    return records.filter((record) => {
      if (filterType === "comprehensive" && record.type !== "comprehensive") {
        return false;
      }

      if (filterType === "short" && !isShortType(record.type)) {
        return false;
      }

      if (filterSubject !== "all" && record.subject !== filterSubject) {
        return false;
      }

      return true;
    });
  }, [records, filterType, filterSubject]);

  const uniqueSubjects = useMemo(() => {
    const map = new Map();

    records.forEach((record) => {
      if (!map.has(record.subject)) {
        map.set(record.subject, {
          key: record.subject,
          name: record.subjectName,
        });
      }
    });

    return Array.from(map.values());
  }, [records]);

  const showNoRecords = !loading && !pageError && filteredRecords.length === 0;

  return (
    <DashboardLayout title="الامتحانات" subtitle="اختبر معلوماتك وتابع تقدمك" titleIcon={FileText}>
      <div
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
          marginBottom: "1rem",
          flexWrap: "wrap",
          gap: "0.75rem",
        }}
      >
        <div className="exams-filter-row" style={{ marginBottom: 0 }}>
          <Filter size={16} style={{ color: "var(--muted-foreground)" }} />

          {[
            ["all", "الكل"],
            ["comprehensive", "شامل"],
            ["short", "قصير"],
          ].map(([key, label]) => (
            <button
              key={key}
              className={`chip ${filterType === key ? "chip-selected" : ""}`}
              onClick={() => setFilterType(key)}
            >
              {label}
            </button>
          ))}

          <span
            style={{
              width: "1px",
              height: "1.5rem",
              background: "var(--border)",
              margin: "0 0.25rem",
            }}
          />

          <button
            className={`chip ${filterSubject === "all" ? "chip-selected" : ""}`}
            onClick={() => setFilterSubject("all")}
          >
            كل المواد
          </button>

          {uniqueSubjects.map((subject) => (
            <button
              key={subject.key}
              className={`chip ${filterSubject === subject.key ? "chip-selected" : ""}`}
              onClick={() => setFilterSubject(subject.key)}
            >
              {subject.name}
            </button>
          ))}
        </div>

        <button
          className="btn btn-primary btn-sm"
          onClick={() => navigate("/exams/new")}
        >
          <Plus size={16} />
          ابدأ اختبار جديد
        </button>
      </div>

      {pendingDeleteExam && (
        <div
          style={{
            marginBottom: "1rem",
            padding: "1rem",
            borderRadius: "20px",
            background: "rgba(252, 231, 243, 0.95)",
            border: "1px solid rgba(236, 72, 153, 0.45)",
            display: "grid",
            gridTemplateColumns: "1fr auto",
            gap: "1rem",
            alignItems: "center",
          }}
        >
          <div>
            <strong style={{ display: "block", marginBottom: "0.25rem" }}>
              تأكيد حذف نتيجة الامتحان
            </strong>
            <p style={{ margin: 0, color: "#6b7280" }}>
              هل أنت متأكد من حذف نتيجة الامتحان لـ
              <strong> {pendingDeleteExam.subjectName} </strong> بتاريخ
              <strong> {pendingDeleteExam.date} </strong>؟
              هذه العملية لا يمكن التراجع عنها.
            </p>
            {deleteError && (
              <p style={{ marginTop: "0.75rem", color: "#b91c1c" }}>
                {deleteError}
              </p>
            )}
          </div>

          <div style={{ display: "flex", gap: "0.75rem", justifyContent: "flex-end" }}>
            <button
              className="btn btn-outline"
              style={{ minWidth: "8rem" }}
              onClick={() => setPendingDeleteExam(null)}
            >
              إلغاء
            </button>
            <button
              className="btn btn-danger"
              style={{ minWidth: "8rem" }}
              disabled={deletingResultId === pendingDeleteExam.examResultId}
              onClick={() => handleDeleteResult(pendingDeleteExam.examResultId)}
            >
              {deletingResultId === pendingDeleteExam.examResultId ? "جاري الحذف..." : "حذف نهائي"}
            </button>
          </div>
        </div>
      )}

      {loading ? (
        <div className="card" style={{ padding: "3rem", textAlign: "center" }}>
          <Loader2
            className="animate-spin"
            style={{ marginBottom: "1rem", color: "var(--muted-foreground)" }}
          />

          <h3 style={{ fontSize: "1.125rem", fontWeight: 600, marginBottom: "0.5rem" }}>
            جاري تحميل الامتحانات
          </h3>

          <p style={{ color: "var(--muted-foreground)" }}>يرجى الانتظار قليلاً...</p>
        </div>
      ) : pageError ? (
        <div className="card" style={{ padding: "3rem", textAlign: "center", color: "red" }}>
          <FileText size={48} style={{ marginBottom: "1rem" }} />

          <h3 style={{ fontSize: "1.125rem", fontWeight: 600, marginBottom: "0.5rem" }}>
            حدثت مشكلة
          </h3>

          <p>{pageError}</p>
        </div>
      ) : showNoRecords ? (
        <div className="card" style={{ padding: "3rem", textAlign: "center" }}>
          <FileText
            size={48}
            style={{ color: "var(--muted-foreground)", marginBottom: "1rem" }}
          />

          <h3 style={{ fontSize: "1.125rem", fontWeight: 600, marginBottom: "0.5rem" }}>
            لا توجد امتحانات
          </h3>

          <p style={{ color: "var(--muted-foreground)", marginBottom: "1.5rem" }}>
            لم يتم العثور على امتحانات بهذا الفلتر. جرب فلتر آخر أو ابدأ اختبار جديد.
          </p>

          <button className="btn btn-primary" onClick={() => navigate("/exams/new")}>
            <Plus size={16} />
            ابدأ اختبار جديد
          </button>
        </div>
      ) : (
        <motion.div className="exams-grid" initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
          {filteredRecords.map((exam, index) => {
            const Icon = iconMap[exam.subject] || BookOpen;
            const color = colorMap[exam.subject] || "blue";
            const isShortExam = isShortType(exam.type);

            return (
              <motion.div
                key={exam.examResultId || `${exam.examId}-${index}`}
                className="card exam-card"
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: index * 0.06 }}
              >
                <div className="exam-card-header">
                  <div className={`rec-icon-wrap rec-icon-${color}`}>
                    <Icon size={20} />
                  </div>

                  <span
                    className={`exam-type-badge exam-type-${exam.type === "comprehensive" ? "full" : "quick"
                      }`}
                  >
                    {exam.typeName || (exam.type === "comprehensive" ? "شامل" : "قصير")}
                  </span>
                </div>

                <h3 className="rec-title">{exam.subjectName}</h3>

                <p
                  style={{
                    fontSize: "0.8125rem",
                    color: "var(--muted-foreground)",
                    marginBottom: "0.75rem",
                  }}
                >
                  {exam.type === "comprehensive"
                    ? "امتحان شامل"
                    : isShortExam && exam.lessonTitle
                      ? `اختبار قصير — ${exam.lessonTitle}`
                      : "اختبار قصير"}
                </p>

                <div className="exam-meta">
                  <span>📋 {exam.total} سؤال</span>
                  <span>📅 {exam.date}</span>
                </div>

                <div className="exam-completed-section">
                  <div className="exam-score-badge">
                    <Award size={16} />
                    <span>النتيجة: {exam.percentage}٪</span>
                  </div>

                  <div
                    style={{
                      width: "100%",
                      height: "6px",
                      borderRadius: "3px",
                      background: "var(--accent)",
                      marginTop: "0.5rem",
                    }}
                  >
                    <div
                      style={{
                        width: `${Math.min(100, Math.max(0, exam.percentage))}%`,
                        height: "100%",
                        borderRadius: "3px",
                        background:
                          exam.percentage >= 80
                            ? "hsl(152, 70%, 45%)"
                            : exam.percentage >= 60
                              ? "hsl(38, 90%, 50%)"
                              : "hsl(0, 84%, 55%)",
                        transition: "width 0.8s ease",
                      }}
                    />
                  </div>
                </div>

                <div
                  style={{
                    display: "grid",
                    gridTemplateColumns: "1fr 1fr",
                    gap: "0.75rem",
                    marginTop: "1rem",
                  }}
                >
                  <button
                    className="btn btn-outline"
                    style={{
                      width: "100%",
                      display: "flex",
                      alignItems: "center",
                      justifyContent: "center",
                      gap: "0.5rem",
                    }}
                    onClick={() =>
                      navigate(`/exams/take?resultId=${exam.examResultId}&mode=review`)
                    }
                  >
                    <Eye size={16} />
                    مراجعة الأخطاء والنتيجة
                  </button>

                  <button
                    className="btn btn-outline"
                    style={{
                      width: "100%",
                      display: "flex",
                      alignItems: "center",
                      justifyContent: "center",
                      gap: "0.5rem",
                      borderColor: "#dc2626",
                      color: "#dc2626",
                    }}
                    disabled={deletingResultId === exam.examResultId}
                    onClick={() => setPendingDeleteExam(exam)}
                  >
                    <Trash2 size={16} />
                    حذف النتيجة
                  </button>
                </div>
              </motion.div>
            );
          })}
        </motion.div>
      )}
    </DashboardLayout>
  );
};

export default Exams;
