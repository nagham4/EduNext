import { useEffect, useMemo, useRef, useState } from "react";
import {
  Plus,
  Search,
  SlidersHorizontal,
  Pencil,
  Trash2,
  Link as LinkIcon,
  X,
  AlertTriangle,
  Download,
  Eye,
  Loader2,
  FileText,
} from "lucide-react";

import "./AdminLessons.css";
import DashboardLayout from "../../../components/DashboardLayout";
import { API_BASE_URL } from "@/config/api";

const ITEMS_PER_PAGE = 4;

const emptyForm = {
  title: "",
  subjectId: "",
  description: "",
  content: "",
  order: 1,
  videoUrl: "",
};

export default function AdminLessons() {
  const [lessons, setLessons] = useState([]);
  const [subjects, setSubjects] = useState([]);
  const [departments, setDepartments] = useState([]);

  const [searchQuery, setSearchQuery] = useState("");
  const [filterDepartment, setFilterDepartment] = useState("all");
  const [filterSubjectId, setFilterSubjectId] = useState("all");
  const [sortBy, setSortBy] = useState("default");

  const [showForm, setShowForm] = useState(false);
  const [editingLesson, setEditingLesson] = useState(null);
  const [formData, setFormData] = useState(emptyForm);

  const [deleteConfirm, setDeleteConfirm] = useState(null);
  const [viewLesson, setViewLesson] = useState(null);
  const [showFilterPanel, setShowFilterPanel] = useState(false);

  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalItems, setTotalItems] = useState(0);

  const [loading, setLoading] = useState(true);
  const [tableLoading, setTableLoading] = useState(false);
  const [saving, setSaving] = useState(false);

  const [pageError, setPageError] = useState("");
  const [formError, setFormError] = useState("");
  const [formSuccess, setFormSuccess] = useState("");
  const [deleteError, setDeleteError] = useState("");
  const [deleteSuccess, setDeleteSuccess] = useState("");

  const formRef = useRef(null);
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

  const viewModalStyles = {
    overlay: {
      position: "fixed",
      inset: 0,
      background: "rgba(15, 23, 42, 0.55)",
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      padding: "24px",
      zIndex: 9999,
    },
    content: {
      width: "min(700px, 94vw)",
      maxHeight: "88vh",
      overflowY: "auto",
      background: "#ffffff",
      borderRadius: "22px",
      padding: "28px",
      position: "relative",
      direction: "rtl",
      textAlign: "right",
      boxShadow: "0 24px 80px rgba(15, 23, 42, 0.22)",
      border: "1px solid #e5e7eb",
    },
    closeButton: {
      position: "absolute",
      top: "18px",
      left: "18px",
      width: "36px",
      height: "36px",
      borderRadius: "12px",
      border: "none",
      background: "#f1f5f9",
      color: "#64748b",
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      cursor: "pointer",
    },
    title: {
      margin: "0 0 20px",
      paddingInlineStart: "48px",
      fontSize: "1.35rem",
      fontWeight: 800,
      color: "#0f172a",
      lineHeight: 1.6,
    },
    details: {
      display: "flex",
      flexDirection: "column",
      gap: "14px",
    },
    row: {
      display: "grid",
      gridTemplateColumns: "120px 1fr",
      gap: "16px",
      alignItems: "start",
      padding: "13px 0",
      borderBottom: "1px solid #eef2f7",
    },
    label: {
      color: "#64748b",
      fontSize: "0.9rem",
      fontWeight: 800,
      whiteSpace: "nowrap",
    },
    value: {
      color: "#0f172a",
      fontSize: "0.95rem",
      lineHeight: 1.8,
      fontWeight: 500,
      overflowWrap: "anywhere",
      wordBreak: "break-word",
      whiteSpace: "pre-line",
    },
    link: {
      color: "#135bec",
      fontSize: "0.92rem",
      lineHeight: 1.8,
      overflowWrap: "anywhere",
      wordBreak: "break-word",
      textDecoration: "none",
      direction: "ltr",
      textAlign: "left",
      display: "inline-block",
      maxWidth: "100%",
    },
    badge: {
      display: "inline-flex",
      alignItems: "center",
      justifyContent: "center",
      minHeight: "30px",
      padding: "0 12px",
      borderRadius: "999px",
      background: "#eff6ff",
      color: "#135bec",
      fontSize: "0.85rem",
      fontWeight: 800,
      width: "fit-content",
    },
  };

  const normalizeLesson = (lesson) => ({
    id: lesson.id || lesson.Id,
    title: lesson.title || lesson.Title || "",
    subjectId: lesson.subjectId || lesson.SubjectId || "",
    subjectName: lesson.subjectName || lesson.SubjectName || "",
    subjectDepartment:
      lesson.subjectDepartment || lesson.SubjectDepartment || "",
    description: lesson.description || lesson.Description || "",
    order: lesson.order ?? lesson.Order ?? 1,
    unitId: lesson.unitId || lesson.UnitId || "",
    unitOrder:
      lesson.unitOrder ??
      lesson.UnitOrder ??
      lesson.unitOrderNumber ??
      lesson.UnitOrderNumber ??
      lesson.subjectUnitOrder ??
      lesson.SubjectUnitOrder ??
      null,
    unitTitle:
      lesson.unitTitle ||
      lesson.UnitTitle ||
      lesson.unitName ||
      lesson.UnitName ||
      "",
    videoUrl: lesson.videoUrl || lesson.VideoUrl || "",
    content: lesson.content || lesson.Content || "",
    pdfUrl: lesson.pdfUrl || lesson.PdfUrl || "",
    resourcesUrl: lesson.resourcesUrl || lesson.ResourcesUrl || "",
  });

  const normalizeSubject = (subject) => ({
    id: subject.id || subject.Id,
    name: subject.name || subject.Name || "",
    department: subject.department || subject.Department || "",
  });

  const readResponseBody = async (response) => {
    const rawText = await response.text();

    try {
      return JSON.parse(rawText);
    } catch {
      return { message: rawText };
    }
  };

  const getVideoHref = (url) => {
    if (!url) return "#";

    if (url.startsWith("http://") || url.startsWith("https://")) {
      return url;
    }

    return `https://${url}`;
  };

  const loadLessons = async ({ silent = false } = {}) => {
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
        pageSize: String(ITEMS_PER_PAGE),
        sortBy,
      });

      if (searchQuery.trim()) {
        params.append("search", searchQuery.trim());
      }

      if (filterDepartment !== "all") {
        params.append("department", filterDepartment);
      }

      if (filterSubjectId !== "all") {
        params.append("subjectId", filterSubjectId);
      }

      const response = await fetch(
        `${API_BASE_URL}/api/admin/lessons?${params.toString()}`,
        {
          method: "GET",
          headers: authHeaders,
        }
      );

      const data = await readResponseBody(response);

      if (!response.ok) {
        setPageError(data.message || "فشل تحميل الدروس");
        return;
      }

      const pageLessons = data.lessons || data.Lessons || [];
      const pageSubjects = data.subjects || data.Subjects || [];

      const normalizedSubjects = pageSubjects.map(normalizeSubject);

      const uniqueDepartments = [
        ...new Set(
          normalizedSubjects
            .map((subject) => subject.department)
            .filter(Boolean)
        ),
      ];

      setLessons(pageLessons.map(normalizeLesson));
      setSubjects(normalizedSubjects);
      setDepartments(uniqueDepartments);

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
    loadLessons({ silent: false });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (!loading) {
      loadLessons({ silent: true });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentPage, sortBy, filterSubjectId, filterDepartment]);

  useEffect(() => {
    if (loading) return;

    const delay = setTimeout(() => {
      setCurrentPage(1);
      loadLessons({ silent: true });
    }, 400);

    return () => clearTimeout(delay);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchQuery]);

  const filteredSubjectsByDepartment =
    filterDepartment === "all"
      ? subjects
      : subjects.filter((subject) => subject.department === filterDepartment);

  const scrollToForm = () => {
    setTimeout(() => {
      formRef.current?.scrollIntoView({ behavior: "smooth", block: "start" });
    }, 100);
  };

  const handleAddNew = () => {
    setEditingLesson(null);
    setFormData(emptyForm);
    setFormError("");
    setFormSuccess("");
    setShowForm(true);
    scrollToForm();
  };

  const handleEdit = (lesson) => {
    setEditingLesson(lesson);
    setFormData({
      title: lesson.title,
      subjectId: lesson.subjectId || "",
      description: lesson.description || "",
      content: lesson.content || "",
      order: lesson.order || 1,
      videoUrl: lesson.videoUrl || "",
    });
    setFormError("");
    setFormSuccess("");
    setShowForm(true);
    scrollToForm();
  };

  const handleCancel = () => {
    setShowForm(false);
    setEditingLesson(null);
    setFormData(emptyForm);
    setFormError("");
    setFormSuccess("");
  };

  const handleSave = async () => {
    if (!formData.subjectId) {
      setFormSuccess("");
      setFormError("اختيار المادة مطلوب.");
      return;
    }

    if (!formData.title.trim()) {
      setFormSuccess("");
      setFormError("عنوان الدرس مطلوب.");
      return;
    }

    if (Number(formData.order) <= 0) {
      setFormSuccess("");
      setFormError("رقم الدرس يجب أن يكون أكبر من صفر.");
      return;
    }

    try {
      setSaving(true);
      setFormError("");
      setFormSuccess("");

      const isEdit = Boolean(editingLesson);

      const url = isEdit
        ? `${API_BASE_URL}/api/admin/lessons/${editingLesson.id}`
        : `${API_BASE_URL}/api/admin/lessons`;

      const response = await fetch(url, {
        method: isEdit ? "PUT" : "POST",
        headers: authHeaders,
        body: JSON.stringify({
          subjectId: formData.subjectId,
          title: formData.title.trim(),
          description: formData.description.trim(),
          content: formData.content.trim(),
          order: Number(formData.order),
          videoUrl: formData.videoUrl.trim(),
        }),
      });

      const data = await readResponseBody(response);

      if (!response.ok) {
        setFormSuccess("");
        setFormError(data.message || "فشل حفظ الدرس");
        return;
      }

      setFormError("");
      setFormSuccess(
        isEdit ? "تم تعديل الدرس بنجاح." : "تمت إضافة الدرس بنجاح."
      );

      await loadLessons({ silent: true });

      setTimeout(() => {
        handleCancel();
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
        `${API_BASE_URL}/api/admin/lessons/${deleteConfirm.id}`,
        {
          method: "DELETE",
          headers: authHeaders,
        }
      );

      const data = await readResponseBody(response);

      if (!response.ok) {
        setDeleteSuccess("");
        setDeleteError(data.message || "فشل حذف الدرس");
        return;
      }

      setDeleteError("");
      setDeleteSuccess("تم حذف الدرس بنجاح.");

      if (lessons.length === 1 && currentPage > 1) {
        setCurrentPage((prev) => prev - 1);
      } else {
        await loadLessons({ silent: true });
      }

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

  const resetFilters = () => {
    setFilterDepartment("all");
    setFilterSubjectId("all");
    setSortBy("default");
    setSearchQuery("");
    setCurrentPage(1);
  };

  const handleExport = () => {
    const header =
      "العنوان,القسم,المادة,الوحدة,رقم الدرس,رابط الفيديو\n";

    const rows = lessons
      .map(
        (lesson) =>
          `${lesson.title},${lesson.subjectDepartment},${lesson.subjectName},${lesson.unitOrder ?? ""},${lesson.order},${lesson.videoUrl}`
      )
      .join("\n");

    const blob = new Blob(["\uFEFF" + header + rows], {
      type: "text/csv;charset=utf-8;",
    });

    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = "lessons.csv";
    link.click();
  };

  const safeTotalPages = totalPages <= 0 ? 1 : totalPages;

  const startItem =
    totalItems === 0 ? 0 : (currentPage - 1) * ITEMS_PER_PAGE + 1;

  const endItem = Math.min(currentPage * ITEMS_PER_PAGE, totalItems);

  const getVisiblePages = () => {
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

  if (loading) {
    return (
      <DashboardLayout
        title="إدارة الدروس"
        subtitle="إضافة وتعديل وتنظيم الدروس التعليمية لكل مادة."
        titleIcon={FileText}
      >
        <div
          className="admin-main rtl"
          style={{
            minHeight: "350px",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            gap: "12px",
          }}
        >
          <Loader2 className="animate-spin" />
          <span>جاري تحميل الدروس...</span>
        </div>
      </DashboardLayout>
    );
  }

  return (
    <DashboardLayout
      title="إدارة الدروس"
      subtitle="إضافة وتعديل وتنظيم الدروس التعليمية لكل مادة."
      titleIcon={FileText}
      headerContent={
        <div className="admin-messages-toolbar">


          <button className="btn-primary" onClick={handleAddNew}>
            <Plus size={18} />
            <span>إضافة درس</span>
          </button>

          <button className="btn-outline-al" onClick={handleExport}>
            <Download size={18} />
          </button>
        </div>
      }
    >
      <div className="app-container" dir="rtl">
        <main className="admin-main rtl">
          {pageError && (
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
              {pageError}
            </div>
          )}
          <div className="search-bar">
            <div className="search-input-wrapper">
              <Search size={18} className="search-icon" />

              <input
                type="text"
                placeholder="ابحث عن درس أو مادة..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
              />
            </div>

            <select
              className="filter-select"
              value={filterDepartment}
              onChange={(e) => {
                setFilterDepartment(e.target.value);
                setFilterSubjectId("all");
                setCurrentPage(1);
              }}
            >
              <option value="all">جميع الأقسام</option>

              {departments.map((department) => (
                <option key={department} value={department}>
                  {department}
                </option>
              ))}
            </select>

            <select
              className="filter-select"
              value={filterSubjectId}
              onChange={(e) => {
                setFilterSubjectId(e.target.value);
                setCurrentPage(1);
              }}
            >
              <option value="all">جميع المواد</option>

              {filteredSubjectsByDepartment.map((subject) => (
                <option key={subject.id} value={subject.id}>
                  {subject.name}
                </option>
              ))}
            </select>

            <button
              className={`filter-btn ${showFilterPanel ? "filter-btn--active" : ""
                }`}
              onClick={() => setShowFilterPanel(!showFilterPanel)}
            >
              <SlidersHorizontal size={18} />
            </button>
          </div>


          {showFilterPanel && (
            <div className="filter-panel-al">
              <div className="filter-panel-group">
                <label>ترتيب حسب:</label>

                <select
                  value={sortBy}
                  onChange={(e) => {
                    setSortBy(e.target.value);
                    setCurrentPage(1);
                  }}
                >
                  <option value="default">الافتراضي</option>
                  <option value="title">العنوان</option>
                  <option value="subject">المادة</option>
                  <option value="order">رقم الدرس</option>
                </select>
              </div>

              <button className="btn-text" onClick={resetFilters}>
                إعادة تعيين الفلاتر
              </button>
            </div>
          )}

          <div className="table-container">
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

            <table className="lessons-table">
              <thead>
                <tr>
                  <th>عنوان الدرس</th>
                  <th>المادة</th>
                  <th>الوحدة</th>
                  <th>رقم الدرس</th>
                  <th>رابط الفيديو</th>
                  <th>الإجراءات</th>
                </tr>
              </thead>

              <tbody>
                {lessons.length === 0 ? (
                  <tr>
                    <td
                      colSpan={6}
                      style={{
                        textAlign: "center",
                        padding: "40px",
                        color: "#94a3b8",
                      }}
                    >
                      لا توجد دروس مطابقة للبحث
                    </td>
                  </tr>
                ) : (
                  lessons.map((lesson) => (
                    <tr key={lesson.id}>
                      <td>
                        <div className="lesson-title-cell">
                          <span className="lesson-name">{lesson.title}</span>
                        </div>
                      </td>

                      <td>
                        <span className="subject-badge">
                          {lesson.subjectName || "غير محدد"}
                        </span>
                      </td>

                      <td>
                        {lesson.unitOrder ? (
                          <span>
                            الوحدة {lesson.unitOrder}
                            {lesson.unitTitle ? ` - ${lesson.unitTitle}` : ""}
                          </span>
                        ) : (
                          <span style={{ color: "#94a3b8" }}>غير محددة</span>
                        )}
                      </td>

                      <td>الدرس {lesson.order}</td>

                      <td>
                        {lesson.videoUrl ? (
                          <a
                            href={getVideoHref(lesson.videoUrl)}
                            target="_blank"
                            rel="noreferrer"
                            className="video-link"
                          >
                            <LinkIcon size={14} />

                            <span>{lesson.videoUrl}</span>
                          </a>
                        ) : (
                          <span style={{ color: "#94a3b8" }}>
                            لا يوجد رابط
                          </span>
                        )}
                      </td>

                      <td>
                        <div className="actions-cell">
                          <button
                            className="aa-action-btn view"
                            title="عرض"
                            onClick={() => setViewLesson(lesson)}
                          >
                            <Eye size={16} />
                          </button>

                          <button
                            className="aa-action-btn edit"
                            title="تعديل"
                            onClick={() => handleEdit(lesson)}
                          >
                            <Pencil size={16} />
                          </button>

                          <button
                            className="aa-action-btn delete"
                            title="حذف"
                            onClick={() => {
                              setDeleteError("");
                              setDeleteSuccess("");
                              setDeleteConfirm(lesson);
                            }}
                          >
                            <Trash2 size={16} />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>

            <div className="pagination">
              <div className="pagination-info">
                عرض {startItem} إلى {endItem} من أصل {totalItems} درس
              </div>

              <div className="pagination-controls">
                <button
                  className="page-btn"
                  disabled={currentPage <= 1}
                  onClick={() => setCurrentPage((prev) => prev - 1)}
                >
                  السابق
                </button>

                {getVisiblePages().map((page) => {
                  if (typeof page === "string") {
                    return (
                      <span
                        key={page}
                        className="page-btn"
                        style={{
                          cursor: "default",
                          background: "transparent",
                          border: "none",
                        }}
                      >
                        ...
                      </span>
                    );
                  }

                  return (
                    <button
                      key={page}
                      className={`page-btn ${currentPage === page ? "page-btn--active" : ""
                        }`}
                      onClick={() => setCurrentPage(page)}
                    >
                      {page}
                    </button>
                  );
                })}

                <button
                  className="page-btn"
                  disabled={currentPage >= safeTotalPages}
                  onClick={() => setCurrentPage((prev) => prev + 1)}
                >
                  التالي
                </button>
              </div>
            </div>
          </div>

          {showForm && (
            <div className="form-card" ref={formRef}>
              <div className="form-header">
                <h2>{editingLesson ? "تعديل الدرس" : "إضافة درس جديد"}</h2>

                <button className="form-close-btn-al" onClick={handleCancel}>
                  <X size={20} />
                </button>
              </div>

              <div className="form-grid">
                <div className="form-group">
                  <label>اختر المادة</label>

                  <select
                    value={formData.subjectId}
                    onChange={(e) => {
                      setFormData({ ...formData, subjectId: e.target.value });
                      setFormError("");
                      setFormSuccess("");
                    }}
                  >
                    <option value="">اختر مادة من القائمة...</option>

                    {subjects.map((subject) => (
                      <option key={subject.id} value={subject.id}>
                        {subject.department
                          ? `${subject.name} - ${subject.department}`
                          : subject.name}
                      </option>
                    ))}
                  </select>
                </div>

                <div className="form-group">
                  <label>عنوان الدرس</label>

                  <input
                    type="text"
                    placeholder="أدخل اسم الدرس هنا..."
                    value={formData.title}
                    onChange={(e) => {
                      setFormData({ ...formData, title: e.target.value });
                      setFormError("");
                      setFormSuccess("");
                    }}
                  />
                </div>
              </div>

              <div className="form-group full-width">
                <label>وصف الدرس</label>

                <textarea
                  placeholder="اكتب وصفاً مختصراً لمحتوى الدرس..."
                  rows={4}
                  value={formData.description}
                  onChange={(e) => {
                    setFormData({
                      ...formData,
                      description: e.target.value,
                    });
                    setFormError("");
                    setFormSuccess("");
                  }}
                />
              </div>

              <div className="form-group full-width">
                <label>شرح الدرس</label>

                <textarea
                  placeholder="اكتبي شرح الدرس الكامل هنا..."
                  rows={10}
                  value={formData.content}
                  onChange={(e) => {
                    setFormData({
                      ...formData,
                      content: e.target.value,
                    });
                    setFormError("");
                    setFormSuccess("");
                  }}
                  style={{
                    minHeight: "240px",
                    lineHeight: "1.9",
                    direction: "rtl",
                    textAlign: "right",
                    resize: "vertical",
                  }}
                />
              </div>

              <div className="form-grid">
                <div className="form-group">
                  <label>رابط فيديو الدرس</label>

                  <div className="input-with-icon">
                    <LinkIcon size={16} className="input-icon" />

                    <input
                      type="url"
                      placeholder="https://youtube.com/..."
                      value={formData.videoUrl}
                      onChange={(e) => {
                        setFormData({ ...formData, videoUrl: e.target.value });
                        setFormError("");
                        setFormSuccess("");
                      }}
                    />
                  </div>
                </div>

                <div className="form-group">
                  <label>رقم الدرس داخل الوحدة</label>

                  <input
                    type="number"
                    min={1}
                    value={formData.order}
                    onChange={(e) => {
                      setFormData({
                        ...formData,
                        order: Number(e.target.value),
                      });
                      setFormError("");
                      setFormSuccess("");
                    }}
                  />
                </div>
              </div>

              {formError && <div style={errorBoxStyle}>{formError}</div>}

              {formSuccess && <div style={successBoxStyle}>{formSuccess}</div>}

              <div className="form-actions">
                <button
                  className="btn-primary"
                  onClick={handleSave}
                  disabled={saving || Boolean(formSuccess)}
                >
                  {saving
                    ? "جاري الحفظ..."
                    : editingLesson
                      ? "حفظ التعديلات"
                      : "حفظ الدرس"}
                </button>

                <button
                  className="btn-text"
                  onClick={handleCancel}
                  disabled={saving}
                >
                  إلغاء
                </button>
              </div>
            </div>
          )}
        </main>

        {deleteConfirm && (
          <div
            className="modal-overlay-al"
            onClick={() => {
              setDeleteConfirm(null);
              setDeleteError("");
              setDeleteSuccess("");
            }}
          >
            <div
              className="modal-content-al"
              onClick={(e) => e.stopPropagation()}
            >
              <div className="modal-icon-al delete-icon-al">
                <AlertTriangle size={32} />
              </div>

              <h3>تأكيد الحذف</h3>

              <p>
                هل أنت متأكد من حذف الدرس "{deleteConfirm.title}"؟ لا يمكن
                التراجع عن هذا الإجراء.
              </p>

              {deleteError && <div style={errorBoxStyle}>{deleteError}</div>}

              {deleteSuccess && (
                <div style={successBoxStyle}>{deleteSuccess}</div>
              )}

              <div className="modal-actions-al">
                <button
                  className="btn-danger-al"
                  onClick={handleDelete}
                  disabled={saving || Boolean(deleteSuccess)}
                >
                  {saving ? "جاري الحذف..." : "نعم، احذف"}
                </button>

                <button
                  className="btn-text"
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

        {viewLesson && (
          <div
            style={viewModalStyles.overlay}
            onClick={() => setViewLesson(null)}
          >
            <div
              style={viewModalStyles.content}
              onClick={(e) => e.stopPropagation()}
            >
              <button
                type="button"
                style={viewModalStyles.closeButton}
                onClick={() => setViewLesson(null)}
                aria-label="إغلاق"
              >
                <X size={20} />
              </button>

              <h3 style={viewModalStyles.title}>{viewLesson.title}</h3>

              <div style={viewModalStyles.details}>
                <div style={viewModalStyles.row}>
                  <span style={viewModalStyles.label}>القسم:</span>

                  <span style={viewModalStyles.value}>
                    {viewLesson.subjectDepartment || "غير محدد"}
                  </span>
                </div>

                <div style={viewModalStyles.row}>
                  <span style={viewModalStyles.label}>المادة:</span>

                  <span style={viewModalStyles.badge}>
                    {viewLesson.subjectName || "غير محدد"}
                  </span>
                </div>

                <div style={viewModalStyles.row}>
                  <span style={viewModalStyles.label}>الوحدة:</span>

                  <span style={viewModalStyles.value}>
                    {viewLesson.unitOrder
                      ? `الوحدة ${viewLesson.unitOrder}${viewLesson.unitTitle
                        ? ` - ${viewLesson.unitTitle}`
                        : ""
                      }`
                      : "غير محددة"}
                  </span>
                </div>

                <div style={viewModalStyles.row}>
                  <span style={viewModalStyles.label}>رقم الدرس:</span>

                  <span style={viewModalStyles.value}>
                    الدرس {viewLesson.order}
                  </span>
                </div>

                <div style={viewModalStyles.row}>
                  <span style={viewModalStyles.label}>الوصف:</span>

                  <span style={viewModalStyles.value}>
                    {viewLesson.description || "لا يوجد وصف"}
                  </span>
                </div>

                <div style={viewModalStyles.row}>
                  <span style={viewModalStyles.label}>شرح الدرس:</span>

                  <span style={viewModalStyles.value}>
                    {viewLesson.content || "لا يوجد شرح"}
                  </span>
                </div>

                <div style={viewModalStyles.row}>
                  <span style={viewModalStyles.label}>رابط الفيديو:</span>

                  {viewLesson.videoUrl ? (
                    <a
                      href={getVideoHref(viewLesson.videoUrl)}
                      target="_blank"
                      rel="noreferrer"
                      style={viewModalStyles.link}
                    >
                      {viewLesson.videoUrl}
                    </a>
                  ) : (
                    <span style={viewModalStyles.value}>لا يوجد رابط</span>
                  )}
                </div>

                {viewLesson.pdfUrl && (
                  <div style={viewModalStyles.row}>
                    <span style={viewModalStyles.label}>ملف PDF:</span>

                    <a
                      href={getVideoHref(viewLesson.pdfUrl)}
                      target="_blank"
                      rel="noreferrer"
                      style={viewModalStyles.link}
                    >
                      {viewLesson.pdfUrl}
                    </a>
                  </div>
                )}

                {viewLesson.resourcesUrl && (
                  <div style={viewModalStyles.row}>
                    <span style={viewModalStyles.label}>المصادر:</span>

                    <a
                      href={getVideoHref(viewLesson.resourcesUrl)}
                      target="_blank"
                      rel="noreferrer"
                      style={viewModalStyles.link}
                    >
                      {viewLesson.resourcesUrl}
                    </a>
                  </div>
                )}
              </div>
            </div>
          </div>
        )}
      </div>
    </DashboardLayout>
  );
}