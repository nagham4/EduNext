import { useEffect, useMemo, useState } from "react";
import {
  CheckSquare,
  UsersRound,
  TrendingUp,
  Clock,
  Loader2,
  BarChart3,
} from "lucide-react";
import {
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Area,
  AreaChart,
} from "recharts";

import "./AdminAnalytics.css";
import DashboardLayout from "../../../components/DashboardLayout";
import { API_BASE_URL } from "@/config/api";

const defaultColors = ["#135bec", "#8b5cf6", "#f59e0b", "#22c55e", "#ef4444"];

export default function AdminAnalytics() {
  const [analyticsData, setAnalyticsData] = useState(null);
  const [days, setDays] = useState(30);
  const [loading, setLoading] = useState(true);
  const [tableLoading, setTableLoading] = useState(false);
  const [error, setError] = useState("");

  const token = localStorage.getItem("token");

  const authHeaders = useMemo(() => {
    return {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    };
  }, [token]);

  const readResponseBody = async (response) => {
    const rawText = await response.text();

    try {
      return JSON.parse(rawText);
    } catch {
      return { message: rawText };
    }
  };

  const normalizeStats = (stats) => ({
    completedExams: stats?.completedExams ?? stats?.CompletedExams ?? 0,
    activeStudents: stats?.activeStudents ?? stats?.ActiveStudents ?? 0,
    completedExamsChange:
      stats?.completedExamsChange ?? stats?.CompletedExamsChange ?? "",
    activeStudentsChange:
      stats?.activeStudentsChange ?? stats?.ActiveStudentsChange ?? "",
  });

  const normalizeDailyExams = (items) => {
    if (!Array.isArray(items)) return [];

    return items.map((item) => ({
      name: item.name || item.Name || "",
      value: item.value ?? item.Value ?? 0,
    }));
  };

  const normalizePopularSubjects = (items) => {
    if (!Array.isArray(items)) return [];

    return items.map((item, index) => ({
      name: item.name || item.Name || "مادة بدون اسم",
      value: item.value ?? item.Value ?? 0,
      activityCount: item.activityCount ?? item.ActivityCount ?? 0,
      unit: item.unit || item.Unit || "دقيقة",
      color:
        item.color || item.Color || defaultColors[index % defaultColors.length],
    }));
  };

  const normalizeActivityTimes = (items) => {
    if (!Array.isArray(items)) return [];

    return items.map((item) => ({
      time: item.time || item.Time || "",
      level: item.level || item.Level || "لا يوجد",
      percent: item.percent ?? item.Percent ?? 0,
      minutes: item.minutes ?? item.Minutes ?? 0,
    }));
  };

  const normalizeLessonCompletion = (items) => {
    if (!Array.isArray(items)) return [];

    return items.map((item) => ({
      subject: item.subject || item.Subject || "مادة بدون اسم",
      percent: item.percent ?? item.Percent ?? 0,
      completedCount: item.completedCount ?? item.CompletedCount ?? 0,
      requiredCount: item.requiredCount ?? item.RequiredCount ?? 0,
    }));
  };

  const loadAnalytics = async ({ silent = false } = {}) => {
    if (!token) {
      window.location.href = "/login";
      return;
    }

    try {
      if (silent) {
        setTableLoading(true);
      } else {
        setLoading(true);
      }

      setError("");

      const response = await fetch(
        `${API_BASE_URL}/api/admin/analytics?days=${days}`,
        {
          method: "GET",
          headers: authHeaders,
        }
      );

      const data = await readResponseBody(response);

      if (!response.ok) {
        setError(data.message || "فشل تحميل تحليلات النظام");
        return;
      }

      setAnalyticsData(data);
    } catch (err) {
      console.error(err);
      setError("تعذر الاتصال بالسيرفر");
    } finally {
      setLoading(false);
      setTableLoading(false);
    }
  };

  useEffect(() => {
    loadAnalytics({ silent: false });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (!loading) {
      loadAnalytics({ silent: true });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [days]);

  const normalizedData = useMemo(() => {
    if (!analyticsData) {
      return {
        stats: normalizeStats({}),
        dailyExams: [],
        popularSubjects: [],
        activityTimes: [],
        lessonCompletion: [],
      };
    }

    return {
      stats: normalizeStats(analyticsData.stats || analyticsData.Stats),
      dailyExams: normalizeDailyExams(
        analyticsData.dailyExams || analyticsData.DailyExams
      ),
      popularSubjects: normalizePopularSubjects(
        analyticsData.popularSubjects || analyticsData.PopularSubjects
      ),
      activityTimes: normalizeActivityTimes(
        analyticsData.activityTimes || analyticsData.ActivityTimes
      ),
      lessonCompletion: normalizeLessonCompletion(
        analyticsData.lessonCompletion || analyticsData.LessonCompletion
      ),
    };
  }, [analyticsData]);

  const totalActivityMinutes = normalizedData.popularSubjects.reduce(
    (sum, item) => sum + item.activityCount,
    0
  );

  const peakActivity = normalizedData.activityTimes.reduce(
    (max, item) => (item.minutes > max.minutes ? item : max),
    { time: "", percent: 0, minutes: 0 }
  );

  if (loading) {
    return (
      <DashboardLayout
        title="تحليلات النظام"
        subtitle="إحصائيات وتقارير أداء المنصة والطلاب."
        titleIcon={BarChart3}
      >
        <div
          className="analytics-main rtl"
          style={{
            minHeight: "350px",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            gap: "12px",
          }}
        >
          <Loader2 className="animate-spin" />
          <span>جاري تحميل التحليلات...</span>
        </div>
      </DashboardLayout>
    );
  }

  return (
    <DashboardLayout
      title="تحليلات النظام"
      subtitle="إحصائيات وتقارير أداء المنصة والطلاب."
      titleIcon={BarChart3}
    >
      <div className="app-container" dir="rtl">
        <main className="analytics-main rtl">
          {error && (
            <div
              style={{
                marginBottom: "16px",
                padding: "12px 16px",
                borderRadius: "12px",
                background: "#fef2f2",
                color: "#dc2626",
                border: "1px solid #fecaca",
              }}
            >
              {error}
            </div>
          )}

          {tableLoading && (
            <div
              style={{
                marginBottom: "12px",
                display: "flex",
                alignItems: "center",
                gap: "8px",
                color: "#64748b",
              }}
            >
              <Loader2 className="animate-spin" size={16} />
              <span>تحديث البيانات...</span>
            </div>
          )}

          <div className="analytics-stats-row">
            <div className="analytics-stat-card">
              <div
                className="analytics-stat-icon"
                style={{ backgroundColor: "#e8f5e9" }}
              >
                <CheckSquare size={24} color="#4caf50" />
              </div>

              <div className="analytics-stat-info">
                <p className="analytics-stat-label">الامتحانات المنجزة</p>
                <h3 className="analytics-stat-value">
                  {Number(normalizedData.stats.completedExams).toLocaleString()}
                </h3>
                <span className="analytics-stat-change positive">
                  <TrendingUp size={14} />
                  {normalizedData.stats.completedExamsChange ||
                    "عدد محاولات الامتحانات المكتملة"}
                </span>
              </div>
            </div>

            <div className="analytics-stat-card">
              <div
                className="analytics-stat-icon"
                style={{ backgroundColor: "#e3f2fd" }}
              >
                <UsersRound size={24} color="#135bec" />
              </div>

              <div className="analytics-stat-info">
                <p className="analytics-stat-label">إجمالي الطلاب النشطين</p>
                <h3 className="analytics-stat-value">
                  {Number(normalizedData.stats.activeStudents).toLocaleString()}
                </h3>
                <span className="analytics-stat-change positive">
                  <TrendingUp size={14} />
                  {normalizedData.stats.activeStudentsChange ||
                    "حسب الطلاب النشطين حالياً"}
                </span>
              </div>
            </div>
          </div>

          <div className="analytics-charts-row">
            <div className="analytics-chart-card analytics-chart-large">
              <div className="analytics-chart-header">
                <h3>الامتحانات اليومية آخر {days} يوم</h3>

                <select
                  className="analytics-period-select"
                  value={days}
                  onChange={(e) => setDays(Number(e.target.value))}
                >
                  <option value={30}>آخر 30 يوم</option>
                  <option value={7}>آخر 7 أيام</option>
                  <option value={90}>آخر 90 يوم</option>
                </select>
              </div>

              <div className="analytics-chart-body">
                <ResponsiveContainer width="100%" height={250}>
                  <AreaChart data={normalizedData.dailyExams}>
                    <defs>
                      <linearGradient
                        id="colorValue"
                        x1="0"
                        y1="0"
                        x2="0"
                        y2="1"
                      >
                        <stop
                          offset="5%"
                          stopColor="#135bec"
                          stopOpacity={0.3}
                        />
                        <stop
                          offset="95%"
                          stopColor="#135bec"
                          stopOpacity={0}
                        />
                      </linearGradient>
                    </defs>

                    <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                    <XAxis dataKey="name" tick={{ fontSize: 12 }} />
                    <YAxis tick={{ fontSize: 12 }} />
                    <Tooltip />
                    <Area
                      type="monotone"
                      dataKey="value"
                      stroke="#135bec"
                      strokeWidth={2.5}
                      fill="url(#colorValue)"
                    />
                  </AreaChart>
                </ResponsiveContainer>
              </div>
            </div>

            <div className="analytics-chart-card analytics-chart-small">
              <h3 className="analytics-chart-title">توزيع نشاط المواد</h3>

              <div
                style={{
                  color: "#64748b",
                  fontSize: "13px",
                  marginBottom: "10px",
                  textAlign: "right",
                }}
              >
                النسبة محسوبة من إجمالي دقائق الدراسة خلال آخر {days} يوم.
              </div>

              <div className="analytics-chart-body analytics-pie-wrapper">
                <ResponsiveContainer width="100%" height={200}>
                  <PieChart>
                    <Pie
                      data={normalizedData.popularSubjects}
                      cx="50%"
                      cy="50%"
                      innerRadius={55}
                      outerRadius={85}
                      dataKey="value"
                      stroke="none"
                    >
                      {normalizedData.popularSubjects.map((entry, index) => (
                        <Cell
                          key={`${entry.name}-${index}`}
                          fill={entry.color}
                        />
                      ))}
                    </Pie>
                  </PieChart>
                </ResponsiveContainer>

                <div className="analytics-pie-center">
                  <span className="analytics-pie-total">
                    {totalActivityMinutes}
                  </span>
                  <span className="analytics-pie-label">دقيقة دراسة</span>
                </div>
              </div>

              <div className="analytics-pie-legend">
                {normalizedData.popularSubjects.length === 0 ? (
                  <div className="analytics-legend-item">
                    لا توجد بيانات نشاط كافية
                  </div>
                ) : (
                  normalizedData.popularSubjects.map((item) => (
                    <div key={item.name} className="analytics-legend-item">
                      <span
                        className="analytics-legend-dot"
                        style={{ backgroundColor: item.color }}
                      />
                      <span>{item.name}</span>
                      <span className="analytics-legend-value">
                        {item.value}% - {item.activityCount} {item.unit}
                      </span>
                    </div>
                  ))
                )}
              </div>
            </div>
          </div>

          <div className="analytics-bottom-row">
            <div className="analytics-chart-card analytics-chart-large">
              <h3 className="analytics-chart-title">
                نسب إكمال الدروس حسب المادة
              </h3>

              <div
                style={{
                  color: "#64748b",
                  fontSize: "13px",
                  marginBottom: "14px",
                  textAlign: "right",
                }}
              >
                النسبة = الدروس المكتملة من الطلاب النشطين ÷ الدروس المطلوبة لهم.
              </div>

              <div className="analytics-completion-list">
                {normalizedData.lessonCompletion.length === 0 ? (
                  <div style={{ color: "#94a3b8", padding: "20px" }}>
                    لا توجد بيانات تقدم دروس كافية
                  </div>
                ) : (
                  normalizedData.lessonCompletion.map((item) => (
                    <div
                      key={item.subject}
                      className="analytics-completion-item"
                    >
                      <div className="analytics-completion-header">
                        <span className="analytics-completion-subject">
                          {item.subject}
                        </span>
                        <span className="analytics-completion-percent">
                          {item.percent}% ({item.completedCount} /{" "}
                          {item.requiredCount})
                        </span>
                      </div>

                      <div className="analytics-progress-bar">
                        <div
                          className="analytics-progress-fill"
                          style={{ width: `${item.percent}%` }}
                        />
                      </div>
                    </div>
                  ))
                )}
              </div>
            </div>

            <div className="analytics-chart-card analytics-chart-small">
              <h3 className="analytics-chart-title">أوقات النشاط</h3>

              <div
                style={{
                  color: "#64748b",
                  fontSize: "13px",
                  marginBottom: "14px",
                  textAlign: "right",
                }}
              >
                محسوبة حسب دقائق جلسات الدراسة خلال آخر {days} يوم.
              </div>

              <div className="analytics-activity-list">
                {normalizedData.activityTimes.map((item) => (
                  <div key={item.time} className="analytics-activity-item">
                    <span className="analytics-activity-time">{item.time}</span>

                    <div className="analytics-activity-bar-wrapper">
                      <div
                        className="analytics-activity-bar"
                        style={{ width: `${item.percent}%` }}
                      />
                    </div>

                    <span className="analytics-activity-level">
                      {item.level} - {item.minutes} د
                    </span>
                  </div>
                ))}
              </div>

              <div className="analytics-activity-note">
                <Clock size={16} />
                <span>
                  {peakActivity.minutes > 0
                    ? `أعلى نشاط تم رصده حول الساعة ${peakActivity.time} بإجمالي ${peakActivity.minutes} دقيقة.`
                    : "لا توجد بيانات نشاط كافية حالياً."}
                </span>
              </div>
            </div>
          </div>
        </main>
      </div>
    </DashboardLayout>
  );
}