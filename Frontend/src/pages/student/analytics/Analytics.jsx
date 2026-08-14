import { useEffect, useMemo, useState } from "react";
import { motion } from "framer-motion";
import {
  TrendingUp,
  Star,
  Target,
  BookOpen,
  Sparkles,
  AlertTriangle,
  CheckCircle2,
  Loader2,
} from "lucide-react";
import DashboardLayout from "../../../components/DashboardLayout";
import "./Analytics.css";
import { API_BASE_URL } from "@/config/api";

const colorMap = {
  الرياضيات: "hsl(220, 85%, 55%)",
  الفيزياء: "hsl(152, 70%, 45%)",
  "اللغة العربية": "hsl(38, 90%, 50%)",
  الكيمياء: "hsl(270, 70%, 55%)",
  "اللغة الإنجليزية": "hsl(199, 80%, 50%)",
  الأحياء: "hsl(152, 55%, 40%)",
  "العلوم الحياتية": "hsl(152, 55%, 40%)",
  "التربية الإسلامية": "hsl(152, 70%, 45%)",
  "تكنولوجيا المعلومات": "hsl(220, 85%, 55%)",
};

const subjectIconMap = {
  الرياضيات: "📐",
  الفيزياء: "",
  "اللغة العربية": "📘",
  الكيمياء: "",
  "اللغة الإنجليزية": "🌍",
  الأحياء: "🧬",
  "العلوم الحياتية": "🧬",
  "التربية الإسلامية": "🕌",
  "تكنولوجيا المعلومات": "💻",
};

const getAuthToken = () => {
  return sessionStorage.getItem("token") || localStorage.getItem("token");
};

const getLevelByScore = (score) => {
  if (score >= 90) return "ممتاز";
  if (score >= 80) return "جيد جداً";
  if (score >= 70) return "جيد";
  if (score >= 60) return "مقبول";
  if (score > 0) return "بحاجة لتحسين";
  return "غير محدد";
};

const LevelBadge = ({ level }) => {
  return (
    <span
      style={{
        color: "black",
        padding: "4px 10px",
        borderRadius: "999px",
        fontSize: "1rem",
        fontWeight: "600",
      }}
    >
      {level || "غير محدد"}
    </span>
  );
};

const AIRecommendationPanel = ({ text }) => {
  return (
    <motion.section
      className="analytics-ai-recommendation"
      initial={{ opacity: 0, y: 18 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ delay: 0.22 }}
    >
      <div className="analytics-ai-icon">
        <Sparkles size={22} />
      </div>

      <div className="analytics-ai-content">
        <div className="analytics-ai-eyebrow">AI Recommendation</div>
        <h2>توصية الذكاء الاصطناعي</h2>
        <p>{text || "لا توجد توصية متاحة حالياً. ابدأ بحل اختبار حتى تظهر توصية مبنية على أدائك."}</p>
      </div>
    </motion.section>
  );
};

const normalizeList = (value) => {
  if (!value) return [];

  if (Array.isArray(value)) {
    return value.map((item) => String(item).trim()).filter(Boolean);
  }

  return String(value)
    .split(/\n|،|,/)
    .map((item) => item.trim())
    .filter(Boolean);
};

const normalizeAnalytics = (data) => {
  const overview = data?.overview || data?.Overview || {};
  const subjectScores = data?.subjectScores || data?.SubjectScores || [];
  const strengthAreas = data?.strengthAreas || data?.StrengthAreas || [];
  const weakAreas = data?.weakAreas || data?.WeakAreas || [];
  const recommendationText =
    data?.recommendationText || data?.RecommendationText || "";
  const monthlyProgress = data?.monthlyProgress || data?.MonthlyProgress || [];

  const examScores = data?.examScores || data?.ExamScores || [];
  const subjectDetails = data?.subjectDetails || data?.SubjectDetails || [];

  return {
    overview: {
      overallLevel: overview.overallLevel || overview.OverallLevel || "",
      averageScore: overview.averageScore ?? overview.AverageScore ?? 0,
      completedLessons:
        overview.completedLessons ?? overview.CompletedLessons ?? 0,
      totalLessons: overview.totalLessons ?? overview.TotalLessons ?? 0,
      passedExams: overview.passedExams ?? overview.PassedExams ?? 0,
      totalExams: overview.totalExams ?? overview.TotalExams ?? 0,
    },

    subjectScores: subjectScores.map((subject) => {
      const name = subject.subjectName || subject.SubjectName || "";
      const score = subject.score ?? subject.Score ?? 0;

      return {
        subjectId: subject.subjectId || subject.SubjectId,
        name,
        score,
        color: colorMap[name] || "hsl(220, 85%, 55%)",
      };
    }),

    strengthAreas: normalizeList(strengthAreas),
    weakAreas: normalizeList(weakAreas),
    recommendationText,

    monthlyProgress: monthlyProgress.map((point) => ({
      month: point.month || point.Month || "",
      value: point.value ?? point.Value ?? 0,
    })),

    examScores: examScores.map((exam) => ({
      name: exam.name || exam.Name || exam.title || exam.Title || "اختبار",
      date: exam.date || exam.Date || exam.createdAt || exam.CreatedAt || "",
      average: exam.average ?? exam.Average ?? exam.score ?? exam.Score ?? 0,
      subjectsCount:
        exam.subjectsCount ?? exam.SubjectsCount ?? exam.questionsCount ?? exam.QuestionsCount ?? 0,
      level:
        exam.level ||
        exam.Level ||
        getLevelByScore(exam.average ?? exam.Average ?? exam.score ?? exam.Score ?? 0),
    })),

    subjectDetails: subjectDetails.map((subject) => {
      const name = subject.name || subject.Name || subject.subjectName || subject.SubjectName || "";
      const averageScore =
        subject.averageScore ??
        subject.AverageScore ??
        subject.score ??
        subject.Score ??
        0;

      return {
        subjectId: subject.subjectId || subject.SubjectId,
        name,
        averageScore,
        level: subject.level || subject.Level || getLevelByScore(averageScore),
        icon: subjectIconMap[name] || subject.icon || subject.Icon,
        strengths: normalizeList(subject.strengths || subject.Strengths),
        weaknesses: normalizeList(subject.weaknesses || subject.Weaknesses),
      };
    }),
  };
};

const ExamCard = ({ exam }) => (
  <div className="exam-card">
    <div>
      <div className="exam-name">{exam.name}</div>
      <div className="exam-date">{exam.date || "بدون تاريخ"}</div>
    </div>

    <div className="exam-score">{Math.round(exam.average)}%</div>

    <div className="exam-meta">
      <span>{exam.subjectsCount} اختبارات</span>
      <LevelBadge level={exam.level} />
    </div>
  </div>
);

const ExamList = ({ exams }) => {
  if (!exams?.length) {
    return (
      <div className="card" style={{ padding: "1.25rem", color: "var(--muted-foreground)" }}>
        لا توجد اختبارات كافية لعرض المتوسطات حالياً.
      </div>
    );
  }

  return (
    <div className="exam-list">
      {exams.map((exam, index) => (
        <ExamCard key={`${exam.name}-${index}`} exam={exam} />
      ))}
    </div>
  );
};

const SubjectCard = ({ subject }) => (
  <div className="subject-card">
    <div className="subject-head">
      <div className="subject-title">
        <span className="subject-icon" aria-hidden="true">
          <BookOpen size={18} />
        </span>
        {subject.name}
      </div>

      <LevelBadge level={subject.level} />
    </div>

    <div className="subject-score-row">
      <div className="subject-score" style={{ width: `${subject.averageScore}%` }}>
        {Math.round(subject.averageScore)}
        <small>/100</small>
      </div>
    </div>

    <div className="progress-track">
      <div
        className="progress-fill"
        style={{ width: `${Math.min(100, Math.max(0, subject.averageScore))}%` }}
      />
    </div>

    <div className="sw-block">
      <div className="sw-list strengths">
        <h4>نقاط القوة</h4>
        {subject.strengths.length > 0 ? (
          <ul>
            {subject.strengths.map((item, index) => (
              <li key={`${item}-${index}`}>{item}</li>
            ))}
          </ul>
        ) : (
          <p>لا توجد بيانات كافية.</p>
        )}
      </div>

      <div className="sw-list weaknesses">
        <h4>بحاجة لتحسين</h4>
        {subject.weaknesses.length > 0 ? (
          <ul>
            {subject.weaknesses.map((item, index) => (
              <li key={`${item}-${index}`}>{item}</li>
            ))}
          </ul>
        ) : (
          <p>لا توجد بيانات كافية.</p>
        )}
      </div>
    </div>
  </div>
);

const RadarChart = ({ subjects }) => {
  const safeSubjects = subjects || [];
  const cx = 150;
  const cy = 140;
  const r = 110;
  const n = safeSubjects.length || 1;
  const angleStep = (2 * Math.PI) / n;

  const getPoint = (index, value) => {
    const angle = angleStep * index - Math.PI / 2;
    const dist = (value / 100) * r;

    return {
      x: cx + dist * Math.cos(angle),
      y: cy + dist * Math.sin(angle),
    };
  };

  const gridLevels = [25, 50, 75, 100];
  const dataPoints = safeSubjects.map((subject, index) =>
    getPoint(index, subject.score)
  );

  const dataPath =
    dataPoints.length > 0
      ? dataPoints
          .map((point, index) => `${index === 0 ? "M" : "L"}${point.x},${point.y}`)
          .join(" ") + " Z"
      : "";

  if (safeSubjects.length === 0) {
    return (
      <div
        style={{
          minHeight: "220px",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          color: "var(--muted-foreground)",
          textAlign: "center",
        }}
      >
        لا توجد بيانات كافية لعرض مخطط الأداء.
      </div>
    );
  }

  return (
    <svg viewBox="0 0 300 290" className="radar-chart-svg">
      <defs>
        <linearGradient id="radarGrad" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stopColor="hsl(220, 85%, 55%)" stopOpacity="0.35" />
          <stop offset="100%" stopColor="hsl(199, 80%, 50%)" stopOpacity="0.08" />
        </linearGradient>
      </defs>

      {gridLevels.map((level) => {
        const points = Array.from({ length: n }, (_, index) =>
          getPoint(index, level)
        );

        const path =
          points
            .map((point, index) => `${index === 0 ? "M" : "L"}${point.x},${point.y}`)
            .join(" ") + " Z";

        return (
          <path
            key={level}
            d={path}
            fill="none"
            stroke="hsl(216, 20%, 90%)"
            strokeWidth="1"
          />
        );
      })}

      {safeSubjects.map((subject, index) => {
        const end = getPoint(index, 100);

        return (
          <line
            key={`${subject.name}-${index}`}
            x1={cx}
            y1={cy}
            x2={end.x}
            y2={end.y}
            stroke="hsl(216, 20%, 90%)"
            strokeWidth="1"
          />
        );
      })}

      {dataPath && (
        <motion.path
          d={dataPath}
          fill="url(#radarGrad)"
          stroke="hsl(220, 85%, 55%)"
          strokeWidth="2.5"
          strokeLinejoin="round"
          initial={{ opacity: 0, scale: 0.5 }}
          animate={{ opacity: 1, scale: 1 }}
          style={{ transformOrigin: `${cx}px ${cy}px` }}
          transition={{ duration: 1, delay: 0.3, ease: "easeOut" }}
        />
      )}

      {safeSubjects.map((subject, index) => {
        const point = getPoint(index, subject.score);
        const labelPoint = getPoint(index, 118);

        return (
          <motion.g
            key={`${subject.name}-${index}`}
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            transition={{ delay: 0.6 + index * 0.1 }}
          >
            <circle
              cx={point.x}
              cy={point.y}
              r="5"
              fill={subject.color}
              stroke="white"
              strokeWidth="2.5"
            />

            <text
              x={labelPoint.x}
              y={labelPoint.y}
              textAnchor="middle"
              dominantBaseline="middle"
              fontSize="10"
              fontWeight="600"
              fill="hsl(220, 12%, 48%)"
            >
              {subject.name}
            </text>

            <text
              x={point.x}
              y={point.y - 12}
              textAnchor="middle"
              fontSize="9"
              fontWeight="700"
              fill={subject.color}
            >
              {Math.round(subject.score)}٪
            </text>
          </motion.g>
        );
      })}
    </svg>
  );
};

const MonthlyProgressChart = ({ monthlyProgress }) => {
  const data = monthlyProgress || [];

  if (data.length === 0) {
    return (
      <div
        style={{
          minHeight: "180px",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          color: "var(--muted-foreground)",
          textAlign: "center",
        }}
      >
        لا توجد بيانات كافية لعرض التقدم الشهري.
      </div>
    );
  }

  const chartWidth = 600;
  const chartHeight = 240;
  const startX = 74;
  const endX = 560;
  const bottomY = 188;
  const topHeight = 150;
  const step = data.length <= 1 ? 0 : (endX - startX) / (data.length - 1);
  const latestPoint = data[data.length - 1];
  const bestPoint = data.reduce((best, item) => (item.value > best.value ? item : best), data[0]);
  const averageValue = Math.round(
    data.reduce((sum, item) => sum + Number(item.value || 0), 0) / data.length
  );
  const trendValue =
    data.length > 1
      ? Math.round(latestPoint.value - data[data.length - 2].value)
      : null;

  const points = data.map((item, index) => {
    const x = data.length === 1 ? chartWidth / 2 : startX + index * step;
    const y = bottomY - (item.value / 100) * topHeight;

    return { ...item, x, y };
  });

  const areaPath =
    points.length > 1
      ? `M${points[0].x},${bottomY} L${points
          .map((point) => `${point.x},${point.y}`)
          .join(" L")} L${points[points.length - 1].x},${bottomY} Z`
      : "";

  const linePoints = points.map((point) => `${point.x},${point.y}`).join(" ");

  return (
    <div className="monthly-progress-panel">
      <div className="monthly-progress-summary">
        <div className="monthly-summary-card monthly-summary-primary">
          <span>آخر شهر</span>
          <strong>{latestPoint.month}</strong>
          <small>{Math.round(latestPoint.value)}٪</small>
        </div>

        <div className="monthly-summary-card">
          <span>المتوسط</span>
          <strong>{averageValue}٪</strong>
          <small>حسب الأشهر المتاحة</small>
        </div>

        <div className="monthly-summary-card">
          <span>أفضل شهر</span>
          <strong>{bestPoint.month}</strong>
          <small>{Math.round(bestPoint.value)}٪</small>
        </div>

        <div className="monthly-summary-card">
          <span>الاتجاه</span>
          <strong>
            {trendValue === null
              ? "قيد التتبع"
              : trendValue > 0
              ? `+${trendValue}٪`
              : `${trendValue}٪`}
          </strong>
          <small>{trendValue === null ? "أضف شهرا آخر للمقارنة" : "عن الشهر السابق"}</small>
        </div>
      </div>

      <div className="monthly-chart-frame">
        <svg
          viewBox={`0 0 ${chartWidth} ${chartHeight}`}
          className="area-chart-svg monthly-progress-svg"
          preserveAspectRatio="xMidYMid meet"
        >
          <defs>
            <linearGradient id="monthAreaGrad" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" stopColor="hsl(152, 70%, 45%)" stopOpacity="0.28" />
              <stop offset="100%" stopColor="hsl(199, 80%, 50%)" stopOpacity="0.03" />
            </linearGradient>

            <linearGradient id="monthLineGrad" x1="0" y1="0" x2="1" y2="0">
              <stop offset="0%" stopColor="hsl(152, 70%, 42%)" />
              <stop offset="100%" stopColor="hsl(199, 85%, 48%)" />
            </linearGradient>

            <filter id="monthPointGlow" x="-50%" y="-50%" width="200%" height="200%">
              <feGaussianBlur stdDeviation="6" result="blur" />
              <feMerge>
                <feMergeNode in="blur" />
                <feMergeNode in="SourceGraphic" />
              </feMerge>
            </filter>
          </defs>

          {[0, 25, 50, 75, 100].map((value) => {
            const y = bottomY - (value / 100) * topHeight;

            return (
              <g key={value}>
                <line
                  x1={startX}
                  y1={y}
                  x2={endX}
                  y2={y}
                  stroke="hsl(218, 28%, 88%)"
                  strokeWidth="1.2"
                  strokeDasharray={value === 0 ? "0" : "5 7"}
                />

                <text
                  x={startX - 16}
                  y={y + 4}
                  textAnchor="end"
                  fontSize="11"
                  fontWeight="700"
                  fill="hsl(220, 12%, 46%)"
                >
                  {value}٪
                </text>
              </g>
            );
          })}

          {points.length === 1 && (
            <motion.g initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
              <line
                x1={points[0].x}
                y1={bottomY}
                x2={points[0].x}
                y2={points[0].y}
                stroke="url(#monthLineGrad)"
                strokeWidth="3"
                strokeLinecap="round"
                strokeDasharray="6 7"
                opacity="0.55"
              />
            </motion.g>
          )}

          {areaPath && (
            <motion.path
              d={areaPath}
              fill="url(#monthAreaGrad)"
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              transition={{ duration: 1, delay: 0.35 }}
            />
          )}

          {points.length > 1 && (
            <motion.polyline
              points={linePoints}
              fill="none"
              stroke="url(#monthLineGrad)"
              strokeWidth="4"
              strokeLinecap="round"
              strokeLinejoin="round"
              initial={{ pathLength: 0 }}
              animate={{ pathLength: 1 }}
              transition={{ duration: 1.3, delay: 0.35, ease: "easeInOut" }}
            />
          )}

          {points.map((point, index) => (
            <motion.g
              key={`${point.month}-${index}`}
              initial={{ opacity: 0, scale: 0.75 }}
              animate={{ opacity: 1, scale: 1 }}
              transition={{ delay: 0.65 + index * 0.1 }}
            >
              <circle
                cx={point.x}
                cy={point.y}
                r="15"
                fill="hsl(152, 70%, 45%)"
                opacity="0.14"
                filter="url(#monthPointGlow)"
              />

              <circle
                cx={point.x}
                cy={point.y}
                r="8"
                fill="hsl(152, 70%, 45%)"
                stroke="white"
                strokeWidth="4"
              />

              <g transform={`translate(${point.x - 36}, ${point.y - 50})`}>
                <rect
                  width="72"
                  height="30"
                  rx="15"
                  fill="white"
                  stroke="hsl(152, 58%, 82%)"
                />
                <text
                  x="36"
                  y="20"
                  textAnchor="middle"
                  fontSize="13"
                  fontWeight="800"
                  fill="hsl(152, 70%, 38%)"
                >
                  {Math.round(point.value)}٪
                </text>
              </g>

              <text
                x={point.x}
                y={bottomY + 30}
                textAnchor="middle"
                fontSize="13"
                fontWeight="800"
                fill="hsl(220, 12%, 42%)"
              >
                {point.month}
              </text>
            </motion.g>
          ))}
        </svg>

        {data.length === 1 && (
          <div className="monthly-progress-note">
            يظهر هنا أول شهر متاح. عند إضافة نتائج في أشهر أخرى سيظهر خط الاتجاه والمقارنة تلقائياً.
          </div>
        )}
      </div>
    </div>
  );
};

const Analytics = () => {
  const token = getAuthToken();

  const [analytics, setAnalytics] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const fetchAnalytics = async () => {
    if (!token) {
      window.location.href = "/login";
      return;
    }

    try {
      setLoading(true);
      setError("");

      const response = await fetch(`${API_BASE_URL}/api/student/analytics`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      const text = await response.text();

      let data;
      try {
        data = text ? JSON.parse(text) : {};
      } catch {
        data = {};
      }

      if (!response.ok) {
        setError(data?.message || "فشل تحميل التحليل");
        return;
      }

      setAnalytics(normalizeAnalytics(data));
    } catch (err) {
      console.error(err);
      setError("تعذر الاتصال بالسيرفر");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAnalytics();
  }, []);

  const overallStats = useMemo(() => {
    if (!analytics) return [];

    return [
      {
        icon: TrendingUp,
        label: "المستوى العام",
        value: analytics.overview.overallLevel || "غير محدد",
        color: "blue",
      },
      {
        icon: Star,
        label: "متوسط الدرجات",
        value: `${Math.round(analytics.overview.averageScore)}٪`,
        color: "amber",
      },
      {
        icon: BookOpen,
        label: "الدروس المكتملة",
        value: `${analytics.overview.completedLessons} / ${analytics.overview.totalLessons}`,
        color: "green",
      },
      {
       icon: Target,
       label: "الاختبارات المقدمة",
       value: `${analytics.overview.totalExams}`,
       color: "purple",
      },
      

    ];
  }, [analytics]);

  return (
    <DashboardLayout
      title="تحليل الأداء"
      subtitle="تابع مستواك واكتشف نقاط القوة والضعف"
    >
      {loading ? (
        <div className="card" style={{ padding: "2rem", textAlign: "center" }}>
          <Loader2 className="animate-spin" style={{ margin: "0 auto 1rem" }} />
          جاري تحميل تحليل الأداء...
        </div>
      ) : error ? (
        <div
          className="card"
          style={{ padding: "2rem", textAlign: "center", color: "red" }}
        >
          {error}
        </div>
      ) : !analytics ? (
        <div className="card" style={{ padding: "2rem", textAlign: "center" }}>
          لا توجد بيانات تحليل متاحة حالياً.
        </div>
      ) : (
        <>
          <motion.div
            className="stats-grid"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
          >
            {overallStats.map((stat, index) => (
              <motion.div
                key={stat.label}
                className="stat-card"
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: index * 0.1 }}
              >
                <div className={`stat-icon-wrap stat-icon-${stat.color}`}>
                  <stat.icon size={22} />
                </div>

                <div className="stat-info">
                  <p className="stat-label">{stat.label}</p>
                  <p className="stat-value" dir="ltr">
                  {stat.value}
                  </p>
                </div>
              </motion.div>
            ))}
          </motion.div>

          <AIRecommendationPanel text={analytics.recommendationText} />

          <div className="dashboard-content-grid">
            <motion.div
              className="card"
              style={{ padding: "1.75rem" }}
              initial={{ opacity: 0, x: -20 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ delay: 0.3 }}
            >
              <h2 className="card-title-dash" style={{ marginBottom: "1.5rem" }}>
                مخطط الأداء حسب المادة
              </h2>

              <div className="radar-chart-container">
                <RadarChart subjects={analytics.subjectScores} />
              </div>
            </motion.div>

            <motion.div
              style={{ display: "flex", flexDirection: "column", gap: "1rem" }}
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ delay: 0.3 }}
            >
              <div className="card" style={{ padding: "1.5rem" }}>
                <h3
                  className="card-title-dash"
                  style={{
                    marginBottom: "1rem",
                    color: "hsl(152, 70%, 40%)",
                  }}
                >
                  نقاط القوة
                </h3>

                {analytics.strengthAreas.length > 0 ? (
                  analytics.strengthAreas.map((item, index) => (
                    <div key={`${item}-${index}`} className="strength-item">
                      <CheckCircle2
                        size={18}
                        style={{
                          color: "hsl(152, 70%, 40%)",
                          flexShrink: 0,
                        }}
                      />
                      <span>{item}</span>
                    </div>
                  ))
                ) : (
                  <p>لا توجد بيانات كافية.</p>
                )}
              </div>

              <div className="card" style={{ padding: "1.5rem" }}>
                <h3
                  className="card-title-dash"
                  style={{
                    marginBottom: "1rem",
                    color: "hsl(0, 84%, 50%)",
                  }}
                >
                  نقاط الضعف
                </h3>

                {analytics.weakAreas.length > 0 ? (
                  analytics.weakAreas.map((item, index) => (
                    <div key={`${item}-${index}`} className="strength-item">
                      <AlertTriangle
                        size={18}
                        style={{
                          color: "hsl(38, 90%, 50%)",
                          flexShrink: 0,
                        }}
                      />
                      <span>{item}</span>
                    </div>
                  ))
                ) : (
                  <p>لا توجد بيانات كافية.</p>
                )}
              </div>

              <div
                className="card"
                style={{
                  display: "none",
                  padding: "1.5rem",
                  background: "var(--hero-gradient-soft)",
                  border: "1px solid hsla(220, 85%, 55%, 0.15)",
                }}
              >
                <div
                  style={{
                    display: "flex",
                    alignItems: "center",
                    gap: "0.5rem",
                    marginBottom: "0.75rem",
                  }}
                >
                  <Sparkles size={18} style={{ color: "var(--primary)" }} />
                  <h3 className="card-title-dash">توصية الذكاء الاصطناعي</h3>
                </div>

                <p
                  style={{
                    fontSize: "0.875rem",
                    color: "var(--muted-foreground)",
                    lineHeight: 1.7,
                  }}
                >
                  {analytics.recommendationText || "لا توجد توصية متاحة حاليًا."}
                </p>
              </div>
            </motion.div>
          </div>

          <section className="perf-section">
            <h2 className="perf-section-title">
              <span className="perf-dot" /> متوسطات الاختبارات
            </h2>

            <div className="exam-grid">
              <ExamList exams={analytics.examScores} />
            </div>
          </section>

          <section className="perf-section">
            <h2 className="perf-section-title">
              <span className="perf-dot" /> تحليل الأداء حسب المادة
            </h2>

            {analytics.subjectDetails.length > 0 ? (
              <div className="subject-grid">
                {analytics.subjectDetails.map((subject, index) => (
                  <SubjectCard
                    key={subject.subjectId || `${subject.name}-${index}`}
                    subject={subject}
                  />
                ))}
              </div>
            ) : (
              <div
                className="card"
                style={{
                  padding: "1.5rem",
                  color: "var(--muted-foreground)",
                  textAlign: "center",
                }}
              >
                لا توجد بيانات تفصيلية كافية حسب المادة حالياً.
              </div>
            )}
          </section>

          <motion.div
            className="card"
            style={{ padding: "1.75rem", marginTop: "1.5rem" }}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.5 }}
          >
            <h2 className="card-title-dash" style={{ marginBottom: "1.5rem" }}>
              التقدم عبر الأشهر
            </h2>

            <MonthlyProgressChart monthlyProgress={analytics.monthlyProgress} />
          </motion.div>
        </>
      )}
    </DashboardLayout>
  );
};

export default Analytics;
