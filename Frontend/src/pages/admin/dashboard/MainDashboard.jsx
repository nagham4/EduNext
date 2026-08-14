import { useEffect, useMemo, useState } from "react";
import {
  BookOpen,
  Users,
  RefreshCw,
  FileText,
  CheckCircle,
  Sigma,
  FlaskConical,
  Languages,
  Monitor,
  CircleHelp,
  Loader2,
  LayoutDashboard,
} from "lucide-react";

import "./MainDashboard.css";
import DashboardLayout from "../../../components/DashboardLayout.jsx";
import { API_BASE_URL } from "@/config/api";

const subjectIcons = [Sigma, FlaskConical, Languages, Monitor];
const subjectIconClasses = ["math", "physics", "arabic", "cs"];
const subjectColors = ["#135bec", "#8b5cf6", "#22c55e", "#f5841f"];

const badgeClassMap = {
  student_registered: "new",
  exam_completed: "exam",
  lesson_completed: "update",
  admin_action: "update",
};

const badgeTextMap = {
  student_registered: "جديد",
  exam_completed: "امتحان",
  lesson_completed: "درس",
  admin_action: "تحديث",
};

const MainDashboard = () => {
  const [dashboardData, setDashboardData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState("");

  const token = localStorage.getItem("token");

  const fetchDashboard = async (isRefresh = false) => {
    if (!token) {
      window.location.href = "/login";
      return;
    }

    try {
      if (isRefresh) {
        setRefreshing(true);
      } else {
        setLoading(true);
      }

      setError("");

      const response = await fetch(`${API_BASE_URL}/api/admin/dashboard`, {
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
      setRefreshing(false);
    }
  };

  useEffect(() => {
    fetchDashboard();
  }, [token]);

  const normalizedData = useMemo(() => {
    if (!dashboardData) return null;

    return {
      header: dashboardData.header || dashboardData.Header || {},
      summary: dashboardData.summary || dashboardData.Summary || {},
      mostActiveSubjects:
        dashboardData.mostActiveSubjects ||
        dashboardData.MostActiveSubjects ||
        [],
      recentActivities:
        dashboardData.recentActivities ||
        dashboardData.RecentActivities ||
        [],
      studentPerformanceTrends:
        dashboardData.studentPerformanceTrends ||
        dashboardData.StudentPerformanceTrends ||
        [],
      hasData:
        dashboardData.hasData ??
        dashboardData.HasData ??
        false,
    };
  }, [dashboardData]);

  if (loading) {
    return (
      <DashboardLayout
        title="لوحة التحكم"
        subtitle="نظرة سريعة على ما يحدث في المنصة اليوم."
        titleIcon={LayoutDashboard}
      >
        <div
          className="dashboard1-content rtl"
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
      <DashboardLayout
        title="لوحة التحكم"
        subtitle="نظرة سريعة على ما يحدث في المنصة اليوم."
        titleIcon={LayoutDashboard}
      >
        <div
          className="dashboard1-content rtl"
          style={{
            minHeight: "300px",
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
            justifyContent: "center",
            gap: "12px",
            color: "red",
          }}
        >
          <p>{error}</p>

          <button
            type="button"
            onClick={() => fetchDashboard(true)}
            style={{
              width: "fit-content",
              minWidth: "150px",
              height: "40px",
              borderRadius: "10px",
              border: "none",
              background: "#135bec",
              color: "#fff",
              fontWeight: 700,
              cursor: "pointer",
            }}
          >
            إعادة المحاولة
          </button>
        </div>
      </DashboardLayout>
    );
  }

  if (!normalizedData) return null;

  const header = normalizedData.header;
  const summary = normalizedData.summary;

  const adminName =
    header.adminName ||
    header.AdminName ||
    "Admin";

  const lastLoginMessage =
    header.lastLoginMessage ||
    header.LastLoginMessage ||
    "إليك نظرة سريعة على ما يحدث في المنصة اليوم.";

  const statsData = [
    {
      label: "إجمالي الطلاب",
      value: summary.totalStudents ?? summary.TotalStudents ?? 0,
      icon: Users,
      color: "blue",
      change:
        (summary.newStudentsThisMonth ?? summary.NewStudentsThisMonth ?? 0) > 0
          ? `+${summary.newStudentsThisMonth ?? summary.NewStudentsThisMonth} هذا الشهر`
          : "— مستقر",
      type:
        (summary.newStudentsThisMonth ?? summary.NewStudentsThisMonth ?? 0) > 0
          ? "positive"
          : "neutral",
    },
    {
      label: "إجمالي المواد",
      value: summary.totalSubjects ?? summary.TotalSubjects ?? 0,
      icon: BookOpen,
      color: "orange",
      change:
        (summary.newSubjectsThisMonth ?? summary.NewSubjectsThisMonth ?? 0) > 0
          ? `+${summary.newSubjectsThisMonth ?? summary.NewSubjectsThisMonth} هذا الشهر`
          : "— مستقر",
      type:
        (summary.newSubjectsThisMonth ?? summary.NewSubjectsThisMonth ?? 0) > 0
          ? "positive"
          : "neutral",
    },
    {
      label: "إجمالي الدروس",
      value: summary.totalLessons ?? summary.TotalLessons ?? 0,
      icon: FileText,
      color: "green",
      change:
        (summary.newLessonsThisMonth ?? summary.NewLessonsThisMonth ?? 0) > 0
          ? `+${summary.newLessonsThisMonth ?? summary.NewLessonsThisMonth} هذا الشهر`
          : "— مستقر",
      type:
        (summary.newLessonsThisMonth ?? summary.NewLessonsThisMonth ?? 0) > 0
          ? "positive"
          : "neutral",
    },
    {
      label: "إجمالي الامتحانات",
      value: summary.totalExams ?? summary.TotalExams ?? 0,
      icon: CircleHelp,
      color: "blue",
      change: "— مستقر",
      type: "neutral",
    },
    {
      label: "امتحانات مكتملة",
      value: summary.completedExams ?? summary.CompletedExams ?? 0,
      icon: CheckCircle,
      color: "red",
      change:
        (summary.completedExamsThisMonth ??
          summary.CompletedExamsThisMonth ??
          0) > 0
          ? `+${summary.completedExamsThisMonth ??
          summary.CompletedExamsThisMonth
          } نشاط`
          : "— مستقر",
      type:
        (summary.completedExamsThisMonth ??
          summary.CompletedExamsThisMonth ??
          0) > 0
          ? "positive"
          : "neutral",
    },
  ];

  const trends = normalizedData.studentPerformanceTrends.map((item, index) => ({
    id: index + 1,
    day: item.day || item.Day || "",
    value: item.value ?? item.Value ?? 0,
  }));

  const subjects = normalizedData.mostActiveSubjects.map((sub, index) => ({
    id: sub.subjectId || sub.SubjectId || index + 1,
    name: sub.subjectName || sub.SubjectName || "مادة",
    percentage: sub.percentage ?? sub.Percentage ?? 0,
    activityValue: sub.activityValue ?? sub.ActivityValue ?? 0,
  }));

  const activities = normalizedData.recentActivities.map((act, index) => ({
    id: index + 1,
    type: act.type || act.Type || "admin_action",
    title: act.title || act.Title || "نشاط",
    description: act.description || act.Description || "",
    createdAt: act.createdAt || act.CreatedAt,
  }));


  return (
    <DashboardLayout
      title={`مرحباً، ${adminName}`}
      subtitle={lastLoginMessage}
      titleIcon={LayoutDashboard}
      headerContent={
        <div className="admin-messages-toolbar">
          <button
            type="button"
            onClick={() => fetchDashboard(true)}
            disabled={refreshing}
            style={{
              minWidth: "118px",
              height: "42px",
              borderRadius: "12px",
              border: "1px solid #d8e2f0",
              background: "#ffffff",
              color: "#08b7aa",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              gap: "0.45rem",
              fontWeight: 700,
              cursor: refreshing ? "not-allowed" : "pointer",
              boxShadow: "0 6px 18px rgba(15, 23, 42, 0.05)",
            }}
          >
            <RefreshCw
              size={16}
              className={refreshing ? "animate-spin" : ""}
            />
            {refreshing ? "جاري التحديث..." : "تحديث"}
          </button>
        </div>
      }
    >
      <div className="app-container rtl">
        <div className="dashboard1-main">
          <div className="dashboard1-content rtl">
            <div className="admin-summary-grid">
              {statsData.map((stat, i) => (
                <div className="admin-summary-card" key={i}>
                  <div className={`admin-summary-icon ${stat.color}`}>
                    <stat.icon size={24} />
                  </div>

                  <div className="admin-summary-copy">
                    <div className="admin-summary-label">{stat.label}</div>

                    <div className="admin-summary-value">
                      {Number(stat.value).toLocaleString()}
                    </div>

                    <div className={`admin-summary-change ${stat.type}`}>
                      <span>{stat.change}</span>
                    </div>
                  </div>
                </div>
              ))}
            </div>

            <div
              className="charts-row"
              style={{
                gridTemplateColumns: "1fr",
              }}
            >
              <div className="chart-card">
                <div className="chart-card-header">
                  <h3>اتجاهات أداء الطلاب</h3>
                  <span className="chart-period-badge">آخر ٧ أيام</span>
                </div>

                <div className="bar-chart-container">
                  <div className="bars-wrapper">
                    {trends.map((item) => (
                      <div className="bar-item" key={item.id}>
                        <div
                          className="bar"
                          style={{
                            height: `${item.value * 2}px`,
                            background:
                              item.value > 70 ? "#135bec" : "#c7d8fb",
                          }}
                        />

                        <span className="bar-label">{item.day}</span>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </div>

            <div className="bottom-row">
              <div className="bottom-card">
                <div className="bottom-card-header">
                  <h3>المواد الأكثر نشاطاً</h3>
                  <button className="view-all-btn">عرض الكل</button>
                </div>

                {subjects.length === 0 ? (
                  <p>لا يوجد نشاط على المواد بعد.</p>
                ) : (
                  <div
                    style={{
                      maxHeight: "255px",
                      overflowY: "auto",
                      paddingInlineEnd: "0.25rem",
                      scrollbarWidth: "thin",
                    }}
                  >
                    {subjects.map((sub, i) => {
                      const Icon = subjectIcons[i % subjectIcons.length];
                      const iconClass =
                        subjectIconClasses[i % subjectIconClasses.length];
                      const color = subjectColors[i % subjectColors.length];

                      return (
                        <div className="subject-item" key={sub.id}>
                          <div className={`subject-icon ${iconClass}`}>
                            <Icon size={20} />
                          </div>

                          <div className="subject-info">
                            <h4>{sub.name}</h4>

                            <div className="subject-progress-bar">
                              <div
                                className="subject-progress-fill"
                                style={{
                                  width: `${sub.percentage}%`,
                                  background: color,
                                }}
                              />
                            </div>
                          </div>

                          <span className="subject-percent">
                            ٪{sub.percentage}
                          </span>
                        </div>
                      );
                    })}
                  </div>
                )}
              </div>

              <div className="bottom-card">
                <div className="bottom-card-header">
                  <h3>النشاط الأخير للنظام</h3>
                </div>

                {activities.length === 0 ? (
                  <p>لا يوجد نشاط حديث.</p>
                ) : (
                  <div
                    style={{
                      maxHeight: "255px",
                      overflowY: "auto",
                      paddingInlineEnd: "0.25rem",
                      scrollbarWidth: "thin",
                    }}
                  >
                    {activities.map((act) => (
                      <div className="activity-item" key={act.id}>
                        <span
                          className={`activity-badge ${badgeClassMap[act.type] || "update"
                            }`}
                        >
                          {badgeTextMap[act.type] || "نشاط"}
                        </span>

                        <div className="activity-content">
                          <h4>{act.title}</h4>
                          <p>{act.description}</p>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>
    </ DashboardLayout >
  );
};

export default MainDashboard;