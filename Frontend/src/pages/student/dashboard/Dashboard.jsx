import { useEffect, useMemo, useState } from "react";
import { motion } from "framer-motion";
import { useNavigate } from "react-router-dom";
import {
  TrendingUp,
  Star,
  Clock,
  GraduationCap,
  Sparkles,
  ChevronLeft,
  BookMarked,
  Atom,
  Languages,
  FlaskConical,
  BookOpen,
  Loader2,
} from "lucide-react";
import DashboardLayout from "../../../components/DashboardLayout";
import { API_BASE_URL } from "@/config/api";

const allowedColors = ["blue", "green", "amber", "purple"];

const iconMap = {
  TrendingUp,
  Star,
  Clock,
  GraduationCap,
  BookMarked,
  Atom,
  Languages,
  FlaskConical,
  BookOpen,
};

const subjectColorMap = {
  الرياضيات: "hsl(220, 85%, 55%)",
  الفيزياء: "hsl(152, 70%, 45%)",
  "اللغة العربية": "hsl(38, 90%, 50%)",
  الكيمياء: "hsl(270, 70%, 55%)",
  "اللغة الإنجليزية": "hsl(199, 80%, 50%)",
  الأحياء: "hsl(152, 70%, 45%)",
};

const fallbackColor = "hsl(220, 85%, 55%)";

const StatCard = ({ stat, index }) => {
  const color = allowedColors.includes(stat.color) ? stat.color : "blue";
  const IconComponent = iconMap[stat.icon] || TrendingUp;

  return (
    <motion.div
      className={`stat-card stat-card-${color}`}
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.4, delay: index * 0.1 }}
    >
      <div className={`stat-icon-wrap stat-icon-${color}`}>
        <IconComponent size={22} />
      </div>
      <div className="stat-info">
        <p className="stat-label">{stat.label}</p>
        <p className="stat-value">{stat.value}</p>
      </div>
    </motion.div>
  );
};

const EnhancedChart = ({ weeklyProgress = [], subjectProgress = [] }) => {
  const normalizedWeekly = weeklyProgress.map((d, index) => ({
    id: d.id || index + 1,
    day: d.day || d.Day,
    value: d.value ?? d.Value ?? 0,
  }));

  const normalizedSubjects = subjectProgress.map((sub, index) => ({
    id: sub.subjectId || sub.SubjectId || index + 1,
    name: sub.subjectName || sub.SubjectName || "مادة",
    progress: Math.round(sub.progressPercent ?? sub.ProgressPercent ?? 0),
    color: subjectColorMap[sub.subjectName || sub.SubjectName] || fallbackColor,
  }));

  return (
    <div className="enhanced-chart">
      {normalizedSubjects.length > 0 && (
        <div className="chart-subject-rings">
          {normalizedSubjects.map((sub, i) => (
            <motion.div
              key={sub.id}
              className="chart-ring-item"
              initial={{ opacity: 0, scale: 0.8 }}
              animate={{ opacity: 1, scale: 1 }}
              transition={{ duration: 0.5, delay: 0.3 + i * 0.1 }}
            >
              <div className="chart-ring-circle">
                <svg viewBox="0 0 36 36" className="chart-ring-svg">
                  <path
                    className="chart-ring-bg"
                    d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831"
                  />
                  <motion.path
                    className="chart-ring-fill"
                    d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831"
                    style={{ stroke: sub.color }}
                    initial={{ strokeDasharray: "0 100" }}
                    animate={{ strokeDasharray: `${sub.progress} 100` }}
                    transition={{
                      duration: 1.2,
                      delay: 0.5 + i * 0.15,
                      ease: "easeOut",
                    }}
                  />
                </svg>
                <span className="chart-ring-value">{sub.progress}%</span>
              </div>
              <span className="chart-ring-name">{sub.name}</span>
            </motion.div>
          ))}
        </div>
      )}

      <div className="chart-weekly-section">
        <h4 className="chart-weekly-title">النشاط الأسبوعي</h4>
        <div className="weekly-area-chart">
          <svg
            viewBox="0 0 700 160"
            className="area-chart-svg"
            preserveAspectRatio="none"
          >
            <defs>
              <linearGradient id="areaGrad" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stopColor="hsl(220, 85%, 55%)" stopOpacity="0.3" />
                <stop offset="100%" stopColor="hsl(220, 85%, 55%)" stopOpacity="0.02" />
              </linearGradient>
              <linearGradient id="lineGrad" x1="0" y1="0" x2="1" y2="0">
                <stop offset="0%" stopColor="hsl(220, 85%, 55%)" />
                <stop offset="50%" stopColor="hsl(199, 80%, 50%)" />
                <stop offset="100%" stopColor="hsl(180, 65%, 50%)" />
              </linearGradient>
            </defs>

            {[0, 25, 50, 75, 100].map((v) => {
              const y = 140 - (v / 100) * 130;
              return (
                <line
                  key={v}
                  x1="0"
                  y1={y}
                  x2="700"
                  y2={y}
                  stroke="hsl(216, 20%, 90%)"
                  strokeWidth="1"
                  strokeDasharray="4 4"
                />
              );
            })}

            <motion.path
              d={(() => {
                const pts = normalizedWeekly.map((d, i) => {
                  const x = 50 + i * 100;
                  const y = 140 - (d.value / 100) * 130;
                  return `${x},${y}`;
                });

                if (pts.length === 0) return "M0,140 L700,140 Z";

                return `M50,140 L${pts.join(" L")} L${50 + (normalizedWeekly.length - 1) * 100},140 Z`;
              })()}
              fill="url(#areaGrad)"
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              transition={{ duration: 1, delay: 0.5 }}
            />

            {normalizedWeekly.length > 0 && (
              <motion.polyline
                points={normalizedWeekly
                  .map((d, i) => `${50 + i * 100},${140 - (d.value / 100) * 130}`)
                  .join(" ")}
                fill="none"
                stroke="url(#lineGrad)"
                strokeWidth="3"
                strokeLinecap="round"
                strokeLinejoin="round"
                initial={{ pathLength: 0 }}
                animate={{ pathLength: 1 }}
                transition={{ duration: 1.5, delay: 0.3, ease: "easeInOut" }}
              />
            )}

            {normalizedWeekly.map((d, i) => {
              const x = 50 + i * 100;
              const y = 140 - (d.value / 100) * 130;
              return (
                <motion.g
                  key={d.id}
                  initial={{ opacity: 0, scale: 0 }}
                  animate={{ opacity: 1, scale: 1 }}
                  transition={{ delay: 0.8 + i * 0.1 }}
                >
                  <circle cx={x} cy={y} r="6" fill="hsl(220, 85%, 55%)" stroke="white" strokeWidth="3" />
                  <text
                    x={x}
                    y={y - 14}
                    textAnchor="middle"
                    fill="hsl(220, 85%, 55%)"
                    fontSize="12"
                    fontWeight="700"
                  >
                    {d.value}٪
                  </text>
                </motion.g>
              );
            })}

            {normalizedWeekly.map((d, i) => (
              <text
                key={`label-${d.id}`}
                x={50 + i * 100}
                y={158}
                textAnchor="middle"
                fill="hsl(220, 12%, 48%)"
                fontSize="11"
                fontWeight="500"
              >
                {d.day}
              </text>
            ))}
          </svg>
        </div>
      </div>
    </div>
  );
};

const RecommendationCard = ({ rec, index }) => {
  const navigate = useNavigate();
  const color = allowedColors.includes(rec.tagColor) ? rec.tagColor : "blue";
  const IconComponent = iconMap[rec.iconKey] || BookOpen;
  const buttonLabel = rec.lessonId
    ? "افتح الدرس"
    : rec.subjectId
      ? "افتح المادة"
      : "أنشئ خطة";

  const handleStart = () => {
    if (rec.lessonId || rec.subjectId) {
      navigate("/subjects", {
        state: {
          subjectId: rec.subjectId || null,
          lessonId: rec.lessonId || null,
        },
      });
      return;
    }

    navigate("/plans", {
      state: {
        openAiSuggestion: true,
      },
    });
  };

  return (
    <motion.div
      className="recommendation-card card"
      initial={{ opacity: 0, y: 15 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.4, delay: 0.5 + index * 0.15 }}
    >
      <div className="rec-card-top">
        <div className={`rec-icon-wrap rec-icon-${color}`}>
          <IconComponent size={20} />
        </div>
        <span className={`rec-tag rec-tag-${color}`}>{rec.tag}</span>
      </div>

      <h3 className="rec-title">{rec.title}</h3>
      <p className="rec-desc">{rec.description}</p>

      <button
        onClick={handleStart}
        style={{
          width: "100%",
          minHeight: "52px",
          borderRadius: "14px",
          border: "none",
          background: "#08b7aa",
          color: "#ffffff",
          fontSize: "16px",
          fontWeight: 800,
          fontFamily: "inherit",
          cursor: "pointer",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          gap: "8px",
          marginTop: "1rem",
          boxShadow: "0 4px 18px rgba(8, 183, 170, 0.30)",
          transition: "opacity 0.2s ease, transform 0.2s ease",
        }}
        onMouseEnter={(e) => {
          e.currentTarget.style.opacity = "0.92";
          e.currentTarget.style.transform = "translateY(-1px)";
        }}
        onMouseLeave={(e) => {
          e.currentTarget.style.opacity = "1";
          e.currentTarget.style.transform = "translateY(0)";
        }}
      >
        {buttonLabel}
        <ChevronLeft size={18} />
      </button>
    </motion.div>
  );
};

const Dashboard = () => {
  const [dashboardData, setDashboardData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const token = localStorage.getItem("token");

  useEffect(() => {
    const fetchDashboard = async () => {
      if (!token) {
        window.location.href = "/login";
        return;
      }

      try {
        setLoading(true);
        setError("");

        const response = await fetch(`${API_BASE_URL}/api/student/dashboard`, {
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
          setError(data.message || "فشل تحميل بيانات لوحة التحكم");
          return;
        }

        setDashboardData(data);
      } catch (err) {
        console.error(err);
        setError("تعذر الاتصال بالسيرفر");
      } finally {
        setLoading(false);
      }
    };

    fetchDashboard();
  }, [token]);

  const normalizedData = useMemo(() => {
    if (!dashboardData) return null;

    return {
      header: dashboardData.header || dashboardData.Header || {},
      stats: dashboardData.stats || dashboardData.Stats || [],
      weeklyProgress: dashboardData.weeklyProgress || dashboardData.WeeklyProgress || [],
      subjectProgress: dashboardData.subjectProgress || dashboardData.SubjectProgress || [],
      recommendations: dashboardData.recommendations || dashboardData.Recommendations || [],
      motivationalMessage:
        dashboardData.motivationalMessage ||
        dashboardData.MotivationalMessage ||
        "",
      recommendationsTitle:
        dashboardData.recommendationsTitle ||
        dashboardData.RecommendationsTitle ||
        "توصيات دراسية مخصصة لك",
      recommendationsEmptyMessage:
        dashboardData.recommendationsEmptyMessage ||
        dashboardData.RecommendationsEmptyMessage ||
        "لا توجد توصيات حالياً.",
      hasAnyProgress:
        dashboardData.hasAnyProgress ??
        dashboardData.HasAnyProgress ??
        false,
      isAiRecommendations:
        dashboardData.isAiRecommendations ??
        dashboardData.IsAiRecommendations ??
        false,
    };
  }, [dashboardData]);

  const headerTitle = normalizedData?.header?.title || normalizedData?.header?.Title || "أهلاً بك";
  const headerSubtitle =
    normalizedData?.header?.subtitle ||
    normalizedData?.header?.Subtitle ;

  const statsData = normalizedData?.stats || [];
  const weeklyProgress = normalizedData?.weeklyProgress || [];
  const subjectProgress = normalizedData?.subjectProgress || [];
  const motivationalMessage = normalizedData?.motivationalMessage || "";
  const recommendationsTitle = normalizedData?.recommendationsTitle || "توصيات دراسية مخصصة لك";
  const recommendationsEmptyMessage =
    normalizedData?.recommendationsEmptyMessage || "لا توجد توصيات حالياً.";

  const recommendations = useMemo(() => {
    const recs = normalizedData?.recommendations || [];

    return recs.map((rec, index) => ({
      id: rec.lessonId || rec.LessonId || rec.subjectId || rec.SubjectId || index + 1,
      iconKey:
        (rec.tag || rec.Tag) === "الرياضيات"
          ? "BookMarked"
          : (rec.tag || rec.Tag) === "الفيزياء"
            ? "Atom"
            : (rec.tag || rec.Tag) === "اللغة العربية"
              ? "Languages"
              : (rec.tag || rec.Tag) === "الكيمياء"
                ? "FlaskConical"
                : "BookOpen",
      title: rec.title || rec.Title,
      tag: rec.tag || rec.Tag,
      tagColor: rec.tagColor || rec.TagColor,
      description: rec.description || rec.Description,
      subjectId: rec.subjectId || rec.SubjectId,
      lessonId: rec.lessonId || rec.LessonId,
    }));
  }, [normalizedData]);

  if (loading) {
    return (
      <DashboardLayout title="جاري التحميل..." subtitle="نقوم بجلب بياناتك الآن">
        <div
          style={{
            minHeight: "300px",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            gap: "12px",
          }}
        >
          <Loader2 className="animate-spin" />
          <span>جاري تحميل لوحة التحكم...</span>
        </div>
      </DashboardLayout>
    );
  }

  if (error) {
    return (
      <DashboardLayout title="لوحة التحكم" subtitle="حدثت مشكلة أثناء التحميل">
        <div className="card" style={{ padding: "2rem", textAlign: "center", color: "red" }}>
          {error}
        </div>
      </DashboardLayout>
    );
  }

  return (
    <DashboardLayout title={headerTitle} subtitle={headerSubtitle}>
      <motion.div
        className="stats-grid"
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.4 }}
      >
        {statsData.map((stat, i) => (
          <StatCard
            key={`${stat.label || stat.Label}-${i}`}
            stat={{
              label: stat.label || stat.Label,
              value: stat.value || stat.Value,
              color: stat.color || stat.Color,
              icon: stat.icon || stat.Icon,
            }}
            index={i}
          />
        ))}
      </motion.div>

      <div className="dashboard-content-grid">
        <motion.section
          className="progress-summary-card card"
          initial={{ opacity: 0, x: -20 }}
          animate={{ opacity: 1, x: 0 }}
          transition={{ duration: 0.5, delay: 0.3 }}
        >
          <div className="card-header-row">
            <h2 className="card-title-dash">📊 ملخص التقدم</h2>
            <span className="card-badge">هذا الأسبوع</span>
          </div>

          <EnhancedChart weeklyProgress={weeklyProgress} subjectProgress={subjectProgress} />

          {motivationalMessage && (
            <div className="progress-motivational">
              <div className="progress-motivational-icon">
                <Sparkles size={18} />
              </div>
              <p>{motivationalMessage}</p>
            </div>
          )}
        </motion.section>

        <motion.section
          className="ai-recommendations-card"
          initial={{ opacity: 0, x: 20 }}
          animate={{ opacity: 1, x: 0 }}
          transition={{ duration: 0.5, delay: 0.3 }}
        >
          <div className="ai-recommendations-header">
            <Sparkles size={20} style={{ color: "var(--primary)" }} />
            <h2 className="card-title-dash">
              {recommendationsTitle}
              {normalizedData?.isAiRecommendations ? (
                <Sparkles size={16} className="inline-title-icon" aria-hidden="true" />
              ) : null}
            </h2>
          </div>

          <div className="recommendations-list">
            {recommendations.length > 0 ? (
              recommendations.map((rec, i) => (
                <RecommendationCard key={rec.id} rec={rec} index={i} />
              ))
            ) : (
              <div className="card" style={{ padding: "1.25rem", textAlign: "center" }}>
                {recommendationsEmptyMessage}
              </div>
            )}
          </div>
        </motion.section>
      </div>
    </DashboardLayout>
  );
};

export default Dashboard;