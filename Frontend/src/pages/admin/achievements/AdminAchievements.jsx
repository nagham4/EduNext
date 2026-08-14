import { useEffect, useMemo, useState } from "react";
import {
  Search,
  Plus,
  Pencil,
  Trash2,
  Eye,
  ChevronLeft,
  ChevronRight,
  Flame,
  Target,
  Star,
  BookOpenCheck,
  UsersRound,
  TrendingUp,
  CheckCircle2,
  Loader2,
  X,
  AlertTriangle,
  Info,
  Trophy,
} from "lucide-react";

import "./AdminAchievements.css";
import DashboardLayout from "../../../components/DashboardLayout";
import { API_BASE_URL } from "@/config/api";

const achievementTypes = [
  { value: "lessons", label: "دروس" },
  { value: "exams", label: "امتحانات" },
  { value: "streaks", label: "سلاسل دراسة" },
  { value: "points", label: "نقاط" },
  { value: "collaboration", label: "تعاون" },
];

const rewardTypes = [
  { value: "points", label: "نقاط" },
  { value: "badge", label: "وسام" },
  { value: "level", label: "رفع مستوى" },
];

const typeIcons = {
  lessons: BookOpenCheck,
  exams: Target,
  streaks: Flame,
  points: Star,
  collaboration: UsersRound,
};

const typeColors = {
  lessons: "#135bec",
  exams: "#e74c3c",
  streaks: "#f39c12",
  points: "#9b59b6",
  collaboration: "#2ecc71",
};

const emptyForm = {
  titleAr: "",
  titleEn: "",
  descAr: "",
  descEn: "",
  type: "lessons",
  targetValue: "",
  reward: "points",
  rewardValue: "",
  status: "active",
};

const PER_PAGE = 5;

export default function AdminAchievements() {
  const [achievements, setAchievements] = useState([]);
  const [analyticsData, setAnalyticsData] = useState([]);

  const [searchQuery, setSearchQuery] = useState("");
  const [typeFilter, setTypeFilter] = useState("all");
  const [statusFilter, setStatusFilter] = useState("all");

  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalItems, setTotalItems] = useState(0);

  const [showForm, setShowForm] = useState(false);
  const [formData, setFormData] = useState({ ...emptyForm });
  const [editingAchievement, setEditingAchievement] = useState(null);

  const [previewAchievement, setPreviewAchievement] = useState(null);
  const [deleteConfirm, setDeleteConfirm] = useState(null);

  const [loading, setLoading] = useState(true);
  const [tableLoading, setTableLoading] = useState(false);
  const [saving, setSaving] = useState(false);

  const [pageError, setPageError] = useState("");
  const [formError, setFormError] = useState("");
  const [formSuccess, setFormSuccess] = useState("");
  const [deleteError, setDeleteError] = useState("");
  const [deleteSuccess, setDeleteSuccess] = useState("");

  const token = localStorage.getItem("token");

  const authHeaders = useMemo(() => {
    return {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    };
  }, [token]);

  const errorBoxStyle = {
    marginTop: "12px",
    marginBottom: "8px",
    padding: "10px 12px",
    borderRadius: "10px",
    background: "#fef2f2",
    color: "#dc2626",
    border: "1px solid #fecaca",
    fontSize: "14px",
    textAlign: "right",
  };

  const successBoxStyle = {
    marginTop: "12px",
    marginBottom: "8px",
    padding: "10px 12px",
    borderRadius: "10px",
    background: "#f0fdf4",
    color: "#16a34a",
    border: "1px solid #bbf7d0",
    fontSize: "14px",
    textAlign: "right",
  };

  const warningBoxStyle = {
    marginTop: "12px",
    marginBottom: "8px",
    padding: "10px 12px",
    borderRadius: "10px",
    background: "#fffbeb",
    color: "#b45309",
    border: "1px solid #fde68a",
    fontSize: "14px",
    textAlign: "right",
    display: "flex",
    alignItems: "center",
    gap: "8px",
  };

  const normalizeAchievement = (item) => ({
    id: item.id || item.Id,
    titleAr: item.titleAr || item.TitleAr || "",
    titleEn: item.titleEn || item.TitleEn || "",
    descAr: item.descAr || item.DescAr || "",
    descEn: item.descEn || item.DescEn || "",
    type: item.type || item.Type || "lessons",
    typeLabel: item.typeLabel || item.TypeLabel || "",
    typeColor:
      item.typeColor ||
      item.TypeColor ||
      typeColors[item.type || item.Type] ||
      "#135bec",
    targetValue: item.targetValue ?? item.TargetValue ?? 0,
    reward: item.reward || item.Reward || "points",
    rewardLabel: item.rewardLabel || item.RewardLabel || "",
    rewardValue: item.rewardValue ?? item.RewardValue ?? 0,
    status: item.status || item.Status || "inactive",
    isActive: item.isActive ?? item.IsActive ?? false,
    unlockedBy: item.unlockedBy ?? item.UnlockedBy ?? 0,
    hasUnlockedStudents:
      item.hasUnlockedStudents ??
      item.HasUnlockedStudents ??
      (item.unlockedBy ?? item.UnlockedBy ?? 0) > 0,
  });

  const normalizeAnalytics = (item) => ({
    id: item.id || item.Id,
    titleAr: item.titleAr || item.TitleAr || "",
    type: item.type || item.Type || "lessons",
    typeColor:
      item.typeColor ||
      item.TypeColor ||
      typeColors[item.type || item.Type] ||
      "#135bec",
    unlockedBy: item.unlockedBy ?? item.UnlockedBy ?? 0,
  });

  const readResponseBody = async (response) => {
    const rawText = await response.text();

    try {
      return JSON.parse(rawText);
    } catch {
      return { message: rawText };
    }
  };

  const loadAchievements = async ({ silent = false } = {}) => {
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

      setPageError("");

      const params = new URLSearchParams({
        page: String(currentPage),
        pageSize: String(PER_PAGE),
        type: typeFilter,
        status: statusFilter,
      });

      if (searchQuery.trim()) {
        params.append("search", searchQuery.trim());
      }

      const response = await fetch(
        `${API_BASE_URL}/api/admin/achievements?${params.toString()}`,
        {
          method: "GET",
          headers: authHeaders,
        }
      );

      const data = await readResponseBody(response);

      if (!response.ok) {
        setPageError(data.message || "فشل تحميل الإنجازات");
        return;
      }

      const pageAchievements = data.achievements || data.Achievements || [];
      const pageAnalytics = data.analytics || data.Analytics || [];

      setAchievements(pageAchievements.map(normalizeAchievement));
      setAnalyticsData(pageAnalytics.map(normalizeAnalytics));

      setCurrentPage(data.currentPage ?? data.CurrentPage ?? currentPage);
      setTotalPages(data.totalPages ?? data.TotalPages ?? 1);
      setTotalItems(data.totalItems ?? data.TotalItems ?? 0);
    } catch (err) {
      console.error(err);
      setPageError("تعذر الاتصال بالسيرفر");
    } finally {
      setLoading(false);
      setTableLoading(false);
    }
  };

  useEffect(() => {
    loadAchievements({ silent: false });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (!loading) {
      loadAchievements({ silent: true });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentPage, typeFilter, statusFilter]);

  useEffect(() => {
    if (loading) return;

    const delay = setTimeout(() => {
      setCurrentPage(1);
      loadAchievements({ silent: true });
    }, 400);

    return () => clearTimeout(delay);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchQuery]);

  const getTypeLabel = (type) => {
    return achievementTypes.find((t) => t.value === type)?.label || type;
  };

  const getRewardLabel = (reward) => {
    return rewardTypes.find((r) => r.value === reward)?.label || reward;
  };

  const IconForType = (type) => typeIcons[type] || Star;

  const resetForm = () => {
    setShowForm(false);
    setEditingAchievement(null);
    setFormData({ ...emptyForm });
    setFormError("");
    setFormSuccess("");
  };

  const openCreateForm = () => {
    setEditingAchievement(null);
    setFormData({ ...emptyForm });
    setFormError("");
    setFormSuccess("");
    setShowForm(true);
  };

  const handleEdit = (achievement) => {
    setEditingAchievement(achievement);
    setFormData({
      titleAr: achievement.titleAr,
      titleEn: achievement.titleEn,
      descAr: achievement.descAr,
      descEn: achievement.descEn,
      type: achievement.type,
      targetValue: String(achievement.targetValue),
      reward: achievement.reward,
      rewardValue: String(achievement.rewardValue),
      status: achievement.status,
    });
    setFormError("");
    setFormSuccess("");
    setShowForm(true);
  };

  const validateForm = () => {
    if (!formData.titleAr.trim()) return "العنوان العربي مطلوب.";
    if (!formData.titleEn.trim()) return "العنوان الإنجليزي مطلوب.";
    if (!formData.descAr.trim()) return "الوصف العربي مطلوب.";
    if (!formData.descEn.trim()) return "الوصف الإنجليزي مطلوب.";

    const targetValue = Number(formData.targetValue);
    const rewardValue = Number(formData.rewardValue);

    if (!Number.isFinite(targetValue) || targetValue <= 0) {
      return "القيمة المستهدفة يجب أن تكون أكبر من صفر.";
    }

    if (!Number.isFinite(rewardValue) || rewardValue < 0) {
      return "قيمة المكافأة يجب أن تكون صفر أو أكثر.";
    }

    return "";
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    const validationMessage = validateForm();

    if (validationMessage) {
      setFormSuccess("");
      setFormError(validationMessage);
      return;
    }

    try {
      setSaving(true);
      setFormError("");
      setFormSuccess("");

      const isEdit = Boolean(editingAchievement);

      const url = isEdit
        ? `${API_BASE_URL}/api/admin/achievements/${editingAchievement.id}`
        : `${API_BASE_URL}/api/admin/achievements`;

      const payload = {
        titleAr: formData.titleAr.trim(),
        titleEn: formData.titleEn.trim(),
        descAr: formData.descAr.trim(),
        descEn: formData.descEn.trim(),
        type: formData.type,
        targetValue: Number(formData.targetValue),
        reward: formData.reward,
        rewardValue: Number(formData.rewardValue),
        status: formData.status,
      };

      const response = await fetch(url, {
        method: isEdit ? "PUT" : "POST",
        headers: authHeaders,
        body: JSON.stringify(payload),
      });

      const data = await readResponseBody(response);

      if (!response.ok) {
        setFormSuccess("");
        setFormError(data.message || "فشل حفظ الإنجاز");
        return;
      }

      setFormSuccess(
        isEdit ? "تم تعديل الإنجاز بنجاح." : "تم إنشاء الإنجاز بنجاح."
      );

      await loadAchievements({ silent: true });

      setTimeout(() => {
        resetForm();
      }, 900);
    } catch (err) {
      console.error(err);
      setFormSuccess("");
      setFormError("تعذر الاتصال بالسيرفر");
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!deleteConfirm) return;

    try {
      setSaving(true);
      setDeleteError("");
      setDeleteSuccess("");

      const response = await fetch(
        `${API_BASE_URL}/api/admin/achievements/${deleteConfirm.id}`,
        {
          method: "DELETE",
          headers: authHeaders,
        }
      );

      const data = await readResponseBody(response);

      if (!response.ok) {
        setDeleteSuccess("");
        setDeleteError(data.message || "فشل تعطيل الإنجاز");
        return;
      }

      setDeleteSuccess("تم تعطيل الإنجاز بنجاح.");

      await loadAchievements({ silent: true });

      setTimeout(() => {
        setDeleteConfirm(null);
        setDeleteError("");
        setDeleteSuccess("");
      }, 900);
    } catch (err) {
      console.error(err);
      setDeleteSuccess("");
      setDeleteError("تعذر الاتصال بالسيرفر");
    } finally {
      setSaving(false);
    }
  };

  const getVisiblePages = () => {
    const safeTotalPages = totalPages <= 0 ? 1 : totalPages;
    const pages = [];

    if (safeTotalPages <= 7) {
      for (let i = 1; i <= safeTotalPages; i++) {
        pages.push(i);
      }

      return pages;
    }

    pages.push(1);

    if (currentPage > 4) {
      pages.push("start-ellipsis");
    }

    const start = Math.max(2, currentPage - 1);
    const end = Math.min(safeTotalPages - 1, currentPage + 1);

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }

    if (currentPage < safeTotalPages - 3) {
      pages.push("end-ellipsis");
    }

    pages.push(safeTotalPages);

    return pages;
  };

  const makePreviewFromForm = () => {
    setPreviewAchievement({
      id: editingAchievement?.id || "preview",
      titleAr: formData.titleAr,
      titleEn: formData.titleEn,
      descAr: formData.descAr,
      descEn: formData.descEn,
      type: formData.type,
      typeColor: typeColors[formData.type] || "#135bec",
      targetValue: Number(formData.targetValue) || 1,
      reward: formData.reward,
      rewardValue: Number(formData.rewardValue) || 0,
      status: formData.status,
      unlockedBy: editingAchievement?.unlockedBy || 0,
    });
  };

  const maxUnlocked = Math.max(...analyticsData.map((a) => a.unlockedBy), 1);

  const startItem = totalItems === 0 ? 0 : (currentPage - 1) * PER_PAGE + 1;
  const endItem = Math.min(currentPage * PER_PAGE, totalItems);

  const shouldShowEditWarning = Boolean(
    editingAchievement?.hasUnlockedStudents || editingAchievement?.unlockedBy > 0
  );

  if (loading) {
    return (
      <DashboardLayout
        title="إدارة الإنجازات"
        subtitle="إنشاء وتعديل وإدارة إنجازات الطلاب على المنصة."
        titleIcon={Trophy}
      >
        <div
          className="aa-main rtl"
          style={{
            minHeight: "350px",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            gap: "12px",
          }}
        >
          <Loader2 className="animate-spin" />
          <span>جاري تحميل الإنجازات...</span>
        </div>
      </DashboardLayout>
    );
  }

  return (
    <DashboardLayout
      title="إدارة الإنجازات"
      subtitle="إنشاء وتعديل وإدارة إنجازات الطلاب على المنصة."
      titleIcon={Trophy}
      headerContent={
        <div className="admin-messages-toolbar">


          <button className="aa-btn-primary" onClick={openCreateForm}>
            <Plus size={18} />
            إنجاز جديد
          </button>
        </div>
      }
    >
      <div className="app-container rtl">
        <main className="aa-main rtl">
          {pageError && <div style={errorBoxStyle}>{pageError}</div>}


          <div className="aa-filters">
            
              <div className="aa-search-box">
                <Search size={18} />
                <input
                  type="text"
                  placeholder="البحث في الإنجازات..."
                  value={searchQuery}
                  onChange={(e) => {
                    setSearchQuery(e.target.value);
                  }}
                />
              </div>
            
            <div className="aa-topbar-right"> 
              <span className="aa-filter-label">تصفية حسب:</span>

              <select
                value={typeFilter}
                onChange={(e) => {
                  setTypeFilter(e.target.value);
                  setCurrentPage(1);
                }}
              >
                <option value="all">جميع الأنواع</option>
                {achievementTypes.map((t) => (
                  <option key={t.value} value={t.value}>
                    {t.label}
                  </option>
                ))}
              </select>

              <select
                value={statusFilter}
                onChange={(e) => {
                  setStatusFilter(e.target.value);
                  setCurrentPage(1);
                }}
              >
                <option value="all">جميع الحالات</option>
                <option value="active">نشط</option>
                <option value="inactive">غير نشط</option>
              </select>
            </div>
          </div>


          {showForm && (
            <div className="aa-form-card">
              <h2 className="aa-form-title">
                {editingAchievement ? "تعديل الإنجاز" : "إنشاء إنجاز جديد"}
              </h2>

              <form onSubmit={handleSubmit} className="aa-form">
                <div className="aa-form-row">
                  <div className="aa-form-group">
                    <label>العنوان (عربي)</label>
                    <input
                      type="text"
                      value={formData.titleAr}
                      onChange={(e) => {
                        setFormData({ ...formData, titleAr: e.target.value });
                        setFormError("");
                        setFormSuccess("");
                      }}
                      placeholder="مثال: أول درس مكتمل"
                    />
                  </div>

                  <div className="aa-form-group">
                    <label>Title (English)</label>
                    <input
                      type="text"
                      value={formData.titleEn}
                      onChange={(e) => {
                        setFormData({ ...formData, titleEn: e.target.value });
                        setFormError("");
                        setFormSuccess("");
                      }}
                      placeholder="e.g. First Lesson Complete"
                    />
                  </div>
                </div>

                <div className="aa-form-row">
                  <div className="aa-form-group">
                    <label>الوصف (عربي)</label>
                    <textarea
                      value={formData.descAr}
                      onChange={(e) => {
                        setFormData({ ...formData, descAr: e.target.value });
                        setFormError("");
                        setFormSuccess("");
                      }}
                      placeholder="وصف الإنجاز بالعربية"
                    />
                  </div>

                  <div className="aa-form-group">
                    <label>Description (English)</label>
                    <textarea
                      value={formData.descEn}
                      onChange={(e) => {
                        setFormData({ ...formData, descEn: e.target.value });
                        setFormError("");
                        setFormSuccess("");
                      }}
                      placeholder="Achievement description in English"
                    />
                  </div>
                </div>

                <div className="aa-form-row">
                  <div className="aa-form-group">
                    <label>النوع</label>
                    <select
                      value={formData.type}
                      onChange={(e) => {
                        setFormData({ ...formData, type: e.target.value });
                        setFormError("");
                        setFormSuccess("");
                      }}
                    >
                      {achievementTypes.map((t) => (
                        <option key={t.value} value={t.value}>
                          {t.label}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div className="aa-form-group">
                    <label>القيمة المستهدفة</label>
                    <input
                      type="number"
                      value={formData.targetValue}
                      onChange={(e) => {
                        setFormData({
                          ...formData,
                          targetValue: e.target.value,
                        });
                        setFormError("");
                        setFormSuccess("");
                      }}
                      placeholder="مثال: 5"
                      min="1"
                    />
                  </div>
                </div>

                <div className="aa-form-row">
                  <div className="aa-form-group">
                    <label>نوع المكافأة</label>
                    <select
                      value={formData.reward}
                      onChange={(e) => {
                        setFormData({ ...formData, reward: e.target.value });
                        setFormError("");
                        setFormSuccess("");
                      }}
                    >
                      {rewardTypes.map((r) => (
                        <option key={r.value} value={r.value}>
                          {r.label}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div className="aa-form-group">
                    <label>قيمة المكافأة</label>
                    <input
                      type="number"
                      value={formData.rewardValue}
                      onChange={(e) => {
                        setFormData({
                          ...formData,
                          rewardValue: e.target.value,
                        });
                        setFormError("");
                        setFormSuccess("");
                      }}
                      placeholder="مثال: 100"
                      min="0"
                    />
                  </div>
                </div>

                <div className="aa-form-row">
                  <div className="aa-form-group">
                    <label>الحالة</label>
                    <div className="aa-toggle-wrap">
                      <button
                        type="button"
                        className={`aa-toggle-btn ${formData.status === "active" ? "active" : ""
                          }`}
                        onClick={() => {
                          setFormData({
                            ...formData,
                            status:
                              formData.status === "active"
                                ? "inactive"
                                : "active",
                          });
                          setFormError("");
                          setFormSuccess("");
                        }}
                      >
                        <div className="aa-toggle-knob" />
                      </button>

                      <span>
                        {formData.status === "active" ? "نشط" : "غير نشط"}
                      </span>
                    </div>
                  </div>
                </div>

                {shouldShowEditWarning && (
                  <div style={warningBoxStyle}>
                    <Info size={18} />
                    <span>
                      هذا الإنجاز فتحه طلاب. أي تعديل على الهدف أو المكافأة أو
                      النوع سيغير تعريف الإنجاز من الآن فصاعدًا.
                    </span>
                  </div>
                )}

                {formError && <div style={errorBoxStyle}>{formError}</div>}
                {formSuccess && (
                  <div style={successBoxStyle}>{formSuccess}</div>
                )}

                <div className="aa-form-actions">
                  <button
                    type="submit"
                    className="aa-btn-primary"
                    disabled={saving || Boolean(formSuccess)}
                  >
                    {saving
                      ? "جاري الحفظ..."
                      : editingAchievement
                        ? "حفظ التعديلات"
                        : "إنشاء الإنجاز"}
                  </button>

                  <button
                    type="button"
                    className="aa-btn-secondary"
                    onClick={resetForm}
                    disabled={saving}
                  >
                    إلغاء
                  </button>

                  {formData.titleAr && (
                    <button
                      type="button"
                      className="aa-btn-preview"
                      onClick={makePreviewFromForm}
                    >
                      <Eye size={16} />
                      معاينة
                    </button>
                  )}
                </div>
              </form>
            </div>
          )}

          <div className="aa-table-card">
            <div className="aa-table-header">
              <h2>قائمة الإنجازات</h2>
              <span className="aa-badge-count">{totalItems} إنجاز</span>
            </div>

            {tableLoading && (
              <div
                style={{
                  padding: "12px",
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

            <div className="aa-table-wrap">
              <table className="aa-table">
                <thead>
                  <tr>
                    <th>الإنجاز</th>
                    <th>النوع</th>
                    <th>الهدف</th>
                    <th>المكافأة</th>
                    <th>الحالة</th>
                    <th>فتحه</th>
                    <th>الإجراءات</th>
                  </tr>
                </thead>

                <tbody>
                  {achievements.length === 0 ? (
                    <tr>
                      <td
                        colSpan={7}
                        style={{
                          textAlign: "center",
                          padding: "40px",
                          color: "#94a3b8",
                        }}
                      >
                        لا توجد إنجازات مطابقة
                      </td>
                    </tr>
                  ) : (
                    achievements.map((achievement) => {
                      const TypeIcon = IconForType(achievement.type);
                      const color =
                        achievement.typeColor ||
                        typeColors[achievement.type] ||
                        "#135bec";

                      return (
                        <tr key={achievement.id}>
                          <td>
                            <div className="aa-cell-achievement">
                              <div
                                className="aa-cell-icon"
                                style={{
                                  backgroundColor: color + "18",
                                  color,
                                }}
                              >
                                <TypeIcon size={20} />
                              </div>

                              <div>
                                <div className="aa-cell-title">
                                  {achievement.titleAr}
                                </div>
                                <div className="aa-cell-subtitle">
                                  {achievement.titleEn}
                                </div>
                              </div>
                            </div>
                          </td>

                          <td>
                            <span
                              className="aa-type-badge"
                              style={{
                                backgroundColor: color + "18",
                                color,
                              }}
                            >
                              {achievement.typeLabel ||
                                getTypeLabel(achievement.type)}
                            </span>
                          </td>

                          <td className="aa-cell-target">
                            {achievement.targetValue}
                          </td>

                          <td>
                            <span className="aa-reward-text">
                              {achievement.rewardValue}{" "}
                              {achievement.rewardLabel ||
                                getRewardLabel(achievement.reward)}
                            </span>
                          </td>

                          <td>
                            <span
                              className={`aa-status-badge ${achievement.status
                                }`}
                            >
                              <span className="aa-status-dot" />
                              {achievement.status === "active"
                                ? "نشط"
                                : "غير نشط"}
                            </span>
                          </td>

                          <td className="aa-cell-unlocked">
                            {Number(achievement.unlockedBy).toLocaleString()} طالب
                          </td>

                          <td>
                            <div className="aa-actions">
                              <button
                                className="aa-action-btn view"
                                title="معاينة"
                                onClick={() =>
                                  setPreviewAchievement(achievement)
                                }
                              >
                                <Eye size={16} />
                              </button>

                              <button
                                className="aa-action-btn edit"
                                title="تعديل"
                                onClick={() => handleEdit(achievement)}
                              >
                                <Pencil size={16} />
                              </button>

                              <button
                                className="aa-action-btn delete"
                                title="تعطيل"
                                onClick={() => {
                                  setDeleteError("");
                                  setDeleteSuccess("");
                                  setDeleteConfirm(achievement);
                                }}
                                disabled={!achievement.isActive}
                              >
                                <Trash2 size={16} />
                              </button>
                            </div>
                          </td>
                        </tr>
                      );
                    })
                  )}
                </tbody>
              </table>
            </div>

            <div className="aa-pagination">
              <span className="aa-pagination-info">
                عرض {startItem}-{endItem} من أصل {totalItems} إنجاز
              </span>

              <div className="aa-pagination-btns">
                <button
                  disabled={currentPage <= 1}
                  onClick={() => setCurrentPage((p) => p - 1)}
                >
                  <ChevronRight size={16} />
                  السابق
                </button>

                {getVisiblePages().map((page) => {
                  if (typeof page === "string") {
                    return (
                      <span key={page} style={{ padding: "0 6px" }}>
                        ...
                      </span>
                    );
                  }

                  return (
                    <button
                      key={page}
                      className={currentPage === page ? "active" : ""}
                      onClick={() => setCurrentPage(page)}
                    >
                      {page}
                    </button>
                  );
                })}

                <button
                  disabled={currentPage >= totalPages}
                  onClick={() => setCurrentPage((p) => p + 1)}
                >
                  التالي
                  <ChevronLeft size={16} />
                </button>
              </div>
            </div>
          </div>

          <div className="aa-analytics-card">
            <h2 className="aa-section-title">
              <TrendingUp size={20} />
              إحصائيات فتح الإنجازات
            </h2>

            <div
              style={{
                color: "#64748b",
                fontSize: "13px",
                marginBottom: "14px",
                textAlign: "right",
              }}
            >
              تعرض عدد الطلاب الذين فتحوا كل إنجاز.
            </div>

            <div className="aa-analytics-bars">
              {analyticsData.length === 0 ? (
                <div style={{ color: "#94a3b8", padding: "20px" }}>
                  لا توجد بيانات فتح إنجازات حتى الآن.
                </div>
              ) : (
                analyticsData.map((item) => (
                  <div key={item.id} className="aa-bar-row">
                    <span className="aa-bar-label">{item.titleAr}</span>

                    <div className="aa-bar-track">
                      <div
                        className="aa-bar-fill"
                        style={{
                          width: `${Math.max(
                            (item.unlockedBy / maxUnlocked) * 100,
                            item.unlockedBy > 0 ? 8 : 0
                          )}%`,
                          backgroundColor:
                            item.typeColor ||
                            typeColors[item.type] ||
                            "#135bec",
                        }}
                      />
                    </div>

                    <span className="aa-bar-value">
                      فتحه {item.unlockedBy} طالب
                    </span>
                  </div>
                ))
              )}
            </div>
          </div>
        </main>

        {previewAchievement && (
          <div
            className="aa-modal-overlay"
            style={{
              position: "fixed",
              inset: 0,
              background: "rgba(15, 23, 42, 0.45)",
              zIndex: 9999,
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              padding: "20px",
            }}
            onClick={() => setPreviewAchievement(null)}
          >
            <div
              style={{
                width: "430px",
                maxWidth: "94vw",
                background: "#fff",
                borderRadius: "24px",
                padding: "24px",
                boxShadow: "0 20px 60px rgba(15, 23, 42, 0.22)",
                position: "relative",
                direction: "rtl",
              }}
              onClick={(e) => e.stopPropagation()}
            >
              <button
                type="button"
                onClick={() => setPreviewAchievement(null)}
                style={{
                  position: "absolute",
                  top: "14px",
                  left: "14px",
                  width: "36px",
                  height: "36px",
                  border: "none",
                  borderRadius: "12px",
                  background: "#f1f5f9",
                  color: "#64748b",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  cursor: "pointer",
                }}
              >
                <X size={18} />
              </button>

              <h3
                style={{
                  textAlign: "right",
                  margin: "0 0 20px",
                  color: "#0f172a",
                  fontSize: "20px",
                  fontWeight: 800,
                  paddingLeft: "44px",
                }}
              >
                معاينة بطاقة الإنجاز
              </h3>

              <div
                style={{
                  display: "flex",
                  justifyContent: "center",
                }}
              >
                <AchievementPreviewCard achievement={previewAchievement} />
              </div>
            </div>
          </div>
        )}

        {deleteConfirm && (
          <div
            className="aa-modal-overlay"
            style={{
              position: "fixed",
              inset: 0,
              background: "rgba(15, 23, 42, 0.45)",
              zIndex: 9999,
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              padding: "20px",
            }}
            onClick={() => {
              setDeleteConfirm(null);
              setDeleteError("");
              setDeleteSuccess("");
            }}
          >
            <div
              className="aa-modal-content"
              style={{
                width: "420px",
                maxWidth: "100%",
                background: "#fff",
                borderRadius: "18px",
                padding: "24px",
                textAlign: "center",
                boxShadow: "0 20px 50px rgba(15, 23, 42, 0.2)",
              }}
              onClick={(e) => e.stopPropagation()}
            >
              <div
                style={{
                  width: "56px",
                  height: "56px",
                  borderRadius: "16px",
                  background: "#fef2f2",
                  color: "#ef4444",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  margin: "0 auto 12px",
                }}
              >
                <AlertTriangle size={30} />
              </div>

              <h3>تعطيل الإنجاز</h3>

              <p>
                هل تريد تعطيل الإنجاز "{deleteConfirm.titleAr}"؟ لن يتم حذفه من
                قاعدة البيانات.
              </p>

              {deleteError && <div style={errorBoxStyle}>{deleteError}</div>}
              {deleteSuccess && (
                <div style={successBoxStyle}>{deleteSuccess}</div>
              )}

              <div
                style={{
                  display: "flex",
                  justifyContent: "center",
                  gap: "10px",
                  marginTop: "18px",
                }}
              >
                <button
                  className="aa-btn-primary"
                  onClick={handleDelete}
                  disabled={saving || Boolean(deleteSuccess)}
                  style={{ background: "#ef4444" }}
                >
                  {saving ? "جاري التعطيل..." : "نعم، عطّل"}
                </button>

                <button
                  className="aa-btn-secondary"
                  onClick={() => {
                    setDeleteConfirm(null);
                    setDeleteError("");
                    setDeleteSuccess("");
                  }}
                  disabled={saving}
                >
                  إلغاء
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </DashboardLayout>
  );
}

function AchievementPreviewCard({ achievement }) {
  const TypeIcon = typeIcons[achievement.type] || Star;
  const color =
    achievement.typeColor || typeColors[achievement.type] || "#135bec";

  const targetValue = Number(achievement.targetValue) || 1;
  const progress =
    achievement.unlockedBy > 0 ? targetValue : Math.min(2, targetValue);
  const completed = progress >= targetValue;

  return (
    <div
      className={`aa-preview-card ${completed ? "completed" : ""}`}
      style={{
        width: "260px",
        minHeight: "230px",
        margin: "0 auto",
        textAlign: "center",
      }}
    >
      {completed && (
        <div className="aa-preview-check">
          <CheckCircle2 size={18} />
        </div>
      )}

      <div
        className="aa-preview-icon-wrap"
        style={{ backgroundColor: color + "20" }}
      >
        <TypeIcon size={32} color={color} />
      </div>

      <h4 className="aa-preview-title">{achievement.titleAr}</h4>
      <p className="aa-preview-desc">{achievement.descAr}</p>

      <div className="aa-preview-progress">
        <div className="aa-preview-progress-info">
          <span>
            {progress}/{targetValue}
          </span>
          <span>{completed ? "مكتمل" : "جاري التقدم..."}</span>
        </div>

        <div className="aa-preview-bar-track">
          <div
            className="aa-preview-bar-fill"
            style={{
              width: `${(progress / targetValue) * 100}%`,
              backgroundColor: color,
            }}
          />
        </div>
      </div>
    </div>
  );
}