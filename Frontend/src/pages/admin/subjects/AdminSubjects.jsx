import { useEffect, useMemo, useState } from "react";
import {
  BookOpen,
  GraduationCap,
  FileText,
  Plus,
  Search,
  SlidersHorizontal,
  Pencil,
  Trash2,
  Eye,
  Download,
  X,
  AlertTriangle,
  ChevronLeft,
  ChevronRight,
  Loader2,
} from "lucide-react";

import "./AdminSubjects.css";
import DashboardLayout from "../../../components/DashboardLayout";
import { API_BASE_URL } from "@/config/api";

const ITEMS_PER_PAGE = 4;

const emptyForm = {
  name: "",
  department: "",
};

const getSubjectInitials = (name = "") => {
  const cleanName = String(name || "").trim();

  if (!cleanName) return "؟";

  const words = cleanName
    .split(/\s+/)
    .map((word) => word.trim())
    .filter(Boolean)
    .map((word) => {
      if (word.startsWith("ال") && word.length > 2) {
        return word.slice(2);
      }

      return word;
    })
    .filter(Boolean);

  if (words.length === 0) return "؟";

  if (words.length === 1) {
    return words[0].slice(0, 1);
  }

  return words
    .slice(0, 2)
    .map((word) => word.slice(0, 1))
    .join(" ");
};

export default function AdminSubjects() {
  const [subjects, setSubjects] = useState([]);
  const [departments, setDepartments] = useState([]);

  const [stats, setStats] = useState({
    totalSubjects: 0,
    totalLessons: 0,
    totalExams: 0,
    newSubjectsThisMonth: 0,
  });

  const [searchQuery, setSearchQuery] = useState("");
  const [showAddModal, setShowAddModal] = useState(false);
  const [editingSubject, setEditingSubject] = useState(null);
  const [formData, setFormData] = useState(emptyForm);

  const [deleteConfirm, setDeleteConfirm] = useState(null);

  const [viewLessons, setViewLessons] = useState(null);
  const [viewLessonsItems, setViewLessonsItems] = useState([]);

  const [showFilterPanel, setShowFilterPanel] = useState(false);
  const [filterDept, setFilterDept] = useState("الكل");
  const [sortBy, setSortBy] = useState("default");
  const [currentPage, setCurrentPage] = useState(1);

  const [totalPages, setTotalPages] = useState(1);
  const [totalItems, setTotalItems] = useState(0);

  const [loading, setLoading] = useState(true);
  const [tableLoading, setTableLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [lessonsLoading, setLessonsLoading] = useState(false);

  const [pageError, setPageError] = useState("");

  const [formError, setFormError] = useState("");
  const [formSuccess, setFormSuccess] = useState("");

  const [deleteError, setDeleteError] = useState("");
  const [deleteSuccess, setDeleteSuccess] = useState("");

  const [lessonsError, setLessonsError] = useState("");

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

  const normalizeSubject = (subject) => {
    const subjectName = subject.name || subject.Name || "";

    return {
      id: subject.id || subject.Id,
      name: subjectName,
      abbr: getSubjectInitials(subjectName),
      department: subject.department || subject.Department || "",
      lessonsCount: subject.lessonsCount ?? subject.LessonsCount ?? 0,
      examsCount: subject.examsCount ?? subject.ExamsCount ?? 0,
    };
  };

  const normalizeStats = (rawStats) => ({
    totalSubjects: rawStats?.totalSubjects ?? rawStats?.TotalSubjects ?? 0,
    totalLessons: rawStats?.totalLessons ?? rawStats?.TotalLessons ?? 0,
    totalExams: rawStats?.totalExams ?? rawStats?.TotalExams ?? 0,
    newSubjectsThisMonth:
      rawStats?.newSubjectsThisMonth ?? rawStats?.NewSubjectsThisMonth ?? 0,
  });

  const normalizeDepartments = (rawDepartments) => {
    if (!Array.isArray(rawDepartments)) return [];

    return rawDepartments
      .map((department) => String(department || "").trim())
      .filter(Boolean);
  };

  const readResponseBody = async (response) => {
    const rawText = await response.text();

    try {
      return JSON.parse(rawText);
    } catch {
      return { message: rawText };
    }
  };

  const loadSubjects = async ({ silent = false } = {}) => {
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

      if (filterDept && filterDept !== "الكل") {
        params.append("department", filterDept);
      }

      const response = await fetch(
        `${API_BASE_URL}/api/admin/subjects?${params.toString()}`,
        {
          method: "GET",
          headers: authHeaders,
        }
      );

      const data = await readResponseBody(response);

      if (!response.ok) {
        setPageError(data.message || "فشل تحميل المواد");
        return;
      }

      const pageSubjects = data.subjects || data.Subjects || [];
      const pageStats = data.stats || data.Stats || {};
      const pageDepartments = data.departments || data.Departments || [];

      setSubjects(pageSubjects.map(normalizeSubject));
      setStats(normalizeStats(pageStats));
      setDepartments(normalizeDepartments(pageDepartments));

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
    loadSubjects({ silent: false });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (!loading) {
      loadSubjects({ silent: true });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentPage, sortBy, filterDept]);

  useEffect(() => {
    if (loading) return;

    const delay = setTimeout(() => {
      setCurrentPage(1);
      loadSubjects({ silent: true });
    }, 400);

    return () => clearTimeout(delay);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchQuery]);

  const handleAddNew = () => {
    setEditingSubject(null);
    setFormData(emptyForm);
    setFormError("");
    setFormSuccess("");
    setShowAddModal(true);
  };

  const handleEdit = (subject) => {
    setEditingSubject(subject);
    setFormData({
      name: subject.name,
      department: subject.department,
    });
    setFormError("");
    setFormSuccess("");
    setShowAddModal(true);
  };

  const closeFormModal = () => {
    setShowAddModal(false);
    setEditingSubject(null);
    setFormData(emptyForm);
    setFormError("");
    setFormSuccess("");
  };

  const handleSave = async () => {
    if (!formData.name.trim()) {
      setFormSuccess("");
      setFormError("اسم المادة مطلوب.");
      return;
    }

    if (!formData.department.trim()) {
      setFormSuccess("");
      setFormError("القسم مطلوب.");
      return;
    }

    try {
      setSaving(true);
      setFormError("");
      setFormSuccess("");

      const isEdit = Boolean(editingSubject);

      const url = isEdit
        ? `${API_BASE_URL}/api/admin/subjects/${editingSubject.id}`
        : `${API_BASE_URL}/api/admin/subjects`;

      const response = await fetch(url, {
        method: isEdit ? "PUT" : "POST",
        headers: authHeaders,
        body: JSON.stringify({
          name: formData.name.trim(),
          department: formData.department.trim(),
        }),
      });

      const data = await readResponseBody(response);

      if (!response.ok) {
        setFormSuccess("");
        setFormError(data.message || "فشل حفظ المادة");
        return;
      }

      setFormError("");
      setFormSuccess(
        isEdit ? "تم تعديل المادة بنجاح." : "تمت إضافة المادة بنجاح."
      );

      await loadSubjects({ silent: true });

      setTimeout(() => {
        closeFormModal();
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
        `${API_BASE_URL}/api/admin/subjects/${deleteConfirm.id}`,
        {
          method: "DELETE",
          headers: authHeaders,
        }
      );

      const data = await readResponseBody(response);

      if (!response.ok) {
        setDeleteSuccess("");
        setDeleteError(data.message || "فشل حذف المادة");
        return;
      }

      setDeleteError("");
      setDeleteSuccess("تم حذف المادة بنجاح.");

      if (subjects.length === 1 && currentPage > 1) {
        setCurrentPage((prev) => prev - 1);
      } else {
        await loadSubjects({ silent: true });
      }

      setTimeout(() => {
        setDeleteConfirm(null);
        setDeleteSuccess("");
        setDeleteError("");
      }, 900);
    } catch (err) {
      console.error(err);
      setDeleteSuccess("");
      setDeleteError("تعذر الاتصال بالسيرفر");
    } finally {
      setSaving(false);
    }
  };

  const handleViewLessons = async (subject) => {
    try {
      setViewLessons(subject);
      setViewLessonsItems([]);
      setLessonsLoading(true);
      setLessonsError("");

      const response = await fetch(
        `${API_BASE_URL}/api/admin/subjects/${subject.id}/lessons`,
        {
          method: "GET",
          headers: authHeaders,
        }
      );

      const data = await readResponseBody(response);

      if (!response.ok) {
        setLessonsError(data.message || "فشل تحميل دروس المادة");
        return;
      }

      const lessons = Array.isArray(data) ? data : [];

      const sortedLessons = [...lessons].sort((a, b) => {
        const unitA =
          a.unitOrder ??
          a.UnitOrder ??
          a.unitOrderNumber ??
          a.UnitOrderNumber ??
          a.subjectUnitOrder ??
          a.SubjectUnitOrder ??
          0;

        const unitB =
          b.unitOrder ??
          b.UnitOrder ??
          b.unitOrderNumber ??
          b.UnitOrderNumber ??
          b.subjectUnitOrder ??
          b.SubjectUnitOrder ??
          0;

        const orderA =
          a.order ??
          a.Order ??
          a.orderNumber ??
          a.OrderNumber ??
          a.order_number ??
          0;

        const orderB =
          b.order ??
          b.Order ??
          b.orderNumber ??
          b.OrderNumber ??
          b.order_number ??
          0;

        if (unitA !== unitB) return unitA - unitB;

        return orderA - orderB;
      });

      const normalizedLessons = sortedLessons.map((lesson, index) => {
        const unitOrder =
          lesson.unitOrder ??
          lesson.UnitOrder ??
          lesson.unitOrderNumber ??
          lesson.UnitOrderNumber ??
          lesson.subjectUnitOrder ??
          lesson.SubjectUnitOrder ??
          null;

        const unitTitle =
          lesson.unitTitle ||
          lesson.UnitTitle ||
          lesson.unitName ||
          lesson.UnitName ||
          "";

        return {
          id: lesson.id || lesson.Id || index + 1,
          title: lesson.title || lesson.Title || "درس بدون عنوان",

          // رقم الدرس التسلسلي داخل القائمة
          displayOrder: index + 1,

          // رقم الوحدة الحقيقي، وهذا الذي يظهر داخل badge على اليمين
          unitOrder,

          unitTitle,
        };
      });

      setViewLessonsItems(normalizedLessons);
    } catch (err) {
      console.error(err);
      setLessonsError("تعذر الاتصال بالسيرفر");
    } finally {
      setLessonsLoading(false);
    }
  };

  const closeLessonsModal = () => {
    setViewLessons(null);
    setLessonsError("");
    setViewLessonsItems([]);
  };

  const handleExport = () => {
    const header = "اسم المادة,الاختصار,القسم,عدد الدروس,عدد الامتحانات\n";

    const rows = subjects
      .map(
        (s) =>
          `${s.name},${s.abbr},${s.department},${s.lessonsCount},${s.examsCount}`
      )
      .join("\n");

    const blob = new Blob(["\uFEFF" + header + rows], {
      type: "text/csv;charset=utf-8;",
    });

    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = "subjects.csv";
    link.click();
  };

  const resetFilters = () => {
    setFilterDept("الكل");
    setSortBy("default");
    setSearchQuery("");
    setCurrentPage(1);
  };

  const safeTotalPages = totalPages <= 0 ? 1 : totalPages;

  const startItem =
    totalItems === 0 ? 0 : (currentPage - 1) * ITEMS_PER_PAGE + 1;

  const endItem = Math.min(currentPage * ITEMS_PER_PAGE, totalItems);

  if (loading) {
    return (
      <DashboardLayout
        title="إدارة المواد"
        subtitle="تتبع وإدارة كافة المواد التعليمية والدروس المرتبطة بها."
        titleIcon={BookOpen}
      >
        <div
          className="admin-main-sub rtl"
          style={{
            minHeight: "350px",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            gap: "12px",
          }}
        >
          <Loader2 className="animate-spin" />
          <span>جاري تحميل المواد...</span>
        </div>
      </DashboardLayout>
    );
  }

  return (
    <DashboardLayout
      title="إدارة المواد"
      subtitle="تتبع وإدارة كافة المواد التعليمية والدروس المرتبطة بها."
      titleIcon={BookOpen}
      headerContent={
        <div className="admin-messages-toolbar">
          <button className="btn-primary-sub" onClick={handleAddNew}>
            <Plus size={18} />
            <span>إضافة مادة</span>
          </button>

          <button className="btn-outline-al" onClick={handleExport}>
            <Download size={18} />
          </button>
        </div>
      }
    >

      <div className="app-container" dir="rtl">
        <main className="admin-main-sub rtl">
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

          {showFilterPanel && (
            <div className="filter-panel-al1">
              <div className="filter-panel-group">
                <label>القسم:</label>

                <select
                  value={filterDept}
                  onChange={(e) => {
                    setFilterDept(e.target.value);
                    setCurrentPage(1);
                  }}
                >
                  <option>الكل</option>

                  {departments.map((department) => (
                    <option key={department} value={department}>
                      {department}
                    </option>
                  ))}
                </select>
              </div>

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
                  <option value="name">الاسم</option>
                  <option value="lessons">عدد الدروس</option>
                  <option value="exams">عدد الامتحانات</option>
                </select>
              </div>

              <button className="btn-text" onClick={resetFilters}>
                إعادة تعيين
              </button>
            </div>
          )}
          <div className="search-bar1">
            <div className="search-input-wrapper">
              <Search size={18} className="search-icon" />

              <input
                type="text"
                placeholder="ابحث عن مواد..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
              />
            </div>

            <button
              className={`filter-btn ${showFilterPanel ? "filter-btn--active" : ""
                }`}
              onClick={() => setShowFilterPanel(!showFilterPanel)}
            >
              <SlidersHorizontal size={18} />
            </button>
          </div>
          <div className="stats-grid-sub">
            <div className="stat-card-sub">
              <div className="stat-info-sub">
                <span className="stat-change-sub positive">
                  +{stats.newSubjectsThisMonth}
                </span>

                <span className="stat-label-sub">إجمالي المواد</span>

                <span className="stat-value-sub">
                  {stats.totalSubjects} مادة
                </span>
              </div>

              <div className="stat-icon-sub blue">
                <BookOpen size={24} />
              </div>
            </div>

            <div className="stat-card-sub">
              <div className="stat-info-sub">
                <span className="stat-change-sub neutral">محسوب</span>

                <span className="stat-label-sub">إجمالي الدروس</span>

                <span className="stat-value-sub">
                  {stats.totalLessons} درس
                </span>
              </div>

              <div className="stat-icon-sub red">
                <GraduationCap size={24} />
              </div>
            </div>

            <div className="stat-card-sub">
              <div className="stat-info-sub">
                <span className="stat-change-sub neutral">محسوب</span>

                <span className="stat-label-sub">إجمالي الاختبارات</span>

                <span className="stat-value-sub">
                  {stats.totalExams} اختبار
                </span>
              </div>

              <div className="stat-icon-sub purple">
                <FileText size={24} />
              </div>
            </div>
          </div>

          <div className="subjects-section-sub">
            <div className="section-header-sub">
              <h2>قائمة المواد الدراسية</h2>
            </div>

            <div className="table-container-sub">
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

              <table className="subjects-table-sub">
                <thead>
                  <tr>
                    <th>اسم المادة</th>
                    <th>عدد الدروس</th>
                    <th>عدد الامتحانات</th>
                    <th>الإجراءات</th>
                  </tr>
                </thead>

                <tbody>
                  {subjects.length === 0 ? (
                    <tr>
                      <td
                        colSpan={4}
                        style={{
                          textAlign: "center",
                          padding: "40px",
                          color: "#94a3b8",
                        }}
                      >
                        لا توجد مواد مطابقة للبحث
                      </td>
                    </tr>
                  ) : (
                    subjects.map((subject) => (
                      <tr key={subject.id}>
                        <td>
                          <div className="subject-name-cell-sub">
                            <span className="subject-abbr-sub">
                              {subject.abbr}
                            </span>

                            <div className="subject-text-sub">
                              <span className="subject-main-name-sub">
                                {subject.name}
                              </span>

                              <span className="subject-dept-sub">
                                {subject.department || "غير محدد"}
                              </span>
                            </div>
                          </div>
                        </td>

                        <td>{subject.lessonsCount} درس</td>

                        <td>{subject.examsCount} امتحانات</td>

                        <td>
                          <div className="actions-cell-sub">
                            <button
                              className="btn-view-lessons-sub"
                              onClick={() => handleViewLessons(subject)}
                            >
                              <Eye size={14} />
                              عرض الدروس
                            </button>

                            <button
                              className="aa-action-btn edit"
                              title="تعديل"
                              onClick={() => handleEdit(subject)}
                            >
                              <Pencil size={16} />
                            </button>

                            <button
                              className="aa-action-btn delete"
                              title="حذف"
                              onClick={() => {
                                setDeleteError("");
                                setDeleteSuccess("");
                                setDeleteConfirm(subject);
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

              <div className="pagination-sub">
                <span className="pagination-info-sub">
                  عرض {startItem} إلى {endItem} من {totalItems} مادة
                </span>

                <div className="pagination-controls-sub">
                  <button
                    className="page-btn-sub"
                    disabled={currentPage >= safeTotalPages}
                    onClick={() => setCurrentPage((prev) => prev + 1)}
                  >
                    <ChevronRight size={16} />
                  </button>

                  {Array.from({ length: safeTotalPages }, (_, i) => (
                    <button
                      key={i + 1}
                      className={`page-btn-sub ${currentPage === i + 1 ? "page-btn-sub--active" : ""
                        }`}
                      onClick={() => setCurrentPage(i + 1)}
                    >
                      {i + 1}
                    </button>
                  ))}

                  <button
                    className="page-btn-sub"
                    disabled={currentPage <= 1}
                    onClick={() => setCurrentPage((prev) => prev - 1)}
                  >
                    <ChevronLeft size={16} />
                  </button>
                </div>
              </div>
            </div>
          </div>
        </main>

        {showAddModal && (
          <div className="modal-overlay-sub" onClick={closeFormModal}>
            <div
              className="modal-content-sub"
              onClick={(e) => e.stopPropagation()}
            >
              <button className="modal-close-sub" onClick={closeFormModal}>
                <X size={18} />
              </button>

              <h3>{editingSubject ? "تعديل المادة" : "إضافة مادة جديدة"}</h3>

              <div className="form-group-sub">
                <label>اسم المادة</label>

                <input
                  type="text"
                  placeholder="مثال: الرياضيات"
                  value={formData.name}
                  onChange={(e) => {
                    setFormData({ ...formData, name: e.target.value });
                    setFormError("");
                    setFormSuccess("");
                  }}
                />
              </div>

              <div className="form-group-sub">
                <label>القسم</label>

                <select
                  value={formData.department}
                  onChange={(e) => {
                    setFormData({ ...formData, department: e.target.value });
                    setFormError("");
                    setFormSuccess("");
                  }}
                >
                  <option value="">اختر القسم...</option>

                  {departments.map((department) => (
                    <option key={department} value={department}>
                      {department}
                    </option>
                  ))}
                </select>
              </div>

              {formError && <div style={errorBoxStyle}>{formError}</div>}

              {formSuccess && <div style={successBoxStyle}>{formSuccess}</div>}

              <div className="form-actions-sub">
                <button
                  className="btn-primary-sub"
                  onClick={handleSave}
                  disabled={saving || Boolean(formSuccess)}
                >
                  {saving
                    ? "جاري الحفظ..."
                    : editingSubject
                      ? "حفظ التعديلات"
                      : "إضافة المادة"}
                </button>

                <button
                  className="btn-cancel-sub"
                  onClick={closeFormModal}
                  disabled={saving}
                >
                  إلغاء
                </button>
              </div>
            </div>
          </div>
        )}

        {deleteConfirm && (
          <div
            className="modal-overlay-sub"
            onClick={() => {
              setDeleteConfirm(null);
              setDeleteError("");
              setDeleteSuccess("");
            }}
          >
            <div
              className="modal-content-sub"
              onClick={(e) => e.stopPropagation()}
            >
              <div className="delete-icon-sub">
                <AlertTriangle size={32} />
              </div>

              <h3>تأكيد الحذف</h3>

              <p>
                هل أنت متأكد من حذف مادة "{deleteConfirm.name}"؟ سيتم حذف
                الدروس والامتحانات المرتبطة بها حسب علاقات قاعدة البيانات.
              </p>

              {deleteError && <div style={errorBoxStyle}>{deleteError}</div>}

              {deleteSuccess && (
                <div style={successBoxStyle}>{deleteSuccess}</div>
              )}

              <div className="form-actions-sub">
                <button
                  className="btn-danger-sub"
                  onClick={handleDelete}
                  disabled={saving || Boolean(deleteSuccess)}
                >
                  {saving ? "جاري الحذف..." : "نعم، احذف"}
                </button>

                <button
                  className="btn-cancel-sub"
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

        {viewLessons && (
          <div className="modal-overlay-sub" onClick={closeLessonsModal}>
            <div
              className="modal-content-sub modal-wide-sub"
              onClick={(e) => e.stopPropagation()}
            >
              <button className="modal-close-sub" onClick={closeLessonsModal}>
                <X size={18} />
              </button>

              <h3>دروس مادة {viewLessons.name}</h3>

              {lessonsError && <div style={errorBoxStyle}>{lessonsError}</div>}

              {lessonsLoading ? (
                <div
                  style={{
                    minHeight: "120px",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    gap: "10px",
                  }}
                >
                  <Loader2 className="animate-spin" />
                  <span>جاري تحميل الدروس...</span>
                </div>
              ) : viewLessonsItems.length === 0 ? (
                <div className="empty-lessons-sub">
                  لا توجد دروس مسجلة لهذه المادة بعد.
                </div>
              ) : (
                <div className="lessons-list-sub">
                  {viewLessonsItems.map((lesson) => (
                    <div className="lesson-item-sub" key={lesson.id}>
                      <div className="lesson-item-info-sub">
                        <span className="lesson-item-title-sub">
                          {lesson.title}
                        </span>

                        <span className="lesson-item-meta-sub">
                          الدرس {lesson.displayOrder}
                          {lesson.unitTitle ? ` • ${lesson.unitTitle}` : ""}
                        </span>
                      </div>

                      <span
                        className="lesson-item-order-sub"
                        title={
                          lesson.unitTitle
                            ? `الوحدة ${lesson.unitOrder} - ${lesson.unitTitle}`
                            : "رقم الوحدة"
                        }
                      >
                        {lesson.unitOrder ? `#${lesson.unitOrder}` : "#—"}
                      </span>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        )}
      </div>
    </DashboardLayout>
  );
}