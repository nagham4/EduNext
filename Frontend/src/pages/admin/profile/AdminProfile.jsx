import { useEffect, useMemo, useState } from "react";
import DashboardLayout from "../../../components/DashboardLayout";
import { motion, AnimatePresence } from "framer-motion";
import {
  User, Mail, Phone, Edit3, Save, BookOpen, Trophy, Star,
  AlertTriangle, Trash2, Lock, Eye, EyeOff, CheckCircle,
  Loader2, RefreshCw, ShieldCheck, ScrollText,
} from "lucide-react";
import { useNavigate } from "react-router-dom";
import { API_BASE_URL } from "@/config/api";

const PROFILE_ENDPOINT = `${API_BASE_URL}/api/admin/profile`;
const CHANGE_PASSWORD_ENDPOINT = `${API_BASE_URL}/api/admin/profile/change-password`;

const activityIconMap = { lesson: BookOpen, exam: Star, achievement: Trophy, admin: ShieldCheck };

const normalizeProfile = (data) => ({
  id: data?.id || data?.Id || "",
  fullName: data?.fullName || data?.FullName || "",
  email: data?.email || data?.Email || "",
  phone: data?.phone || data?.Phone || "",
  role: data?.role || data?.Role || "admin",
  roleLabel: data?.roleLabel || data?.RoleLabel || "مسؤول",
  activityHistory: (data?.activityHistory || data?.ActivityHistory || []).map((group) => ({
    dateLabel: group.dateLabel || group.DateLabel || "",
    items: (group.items || group.Items || []).map((item) => ({
      type: item.type || item.Type || "admin",
      text: item.text || item.Text || "",
      time: item.time || item.Time || "",
      color: item.color || item.Color || "purple",
    })),
  })),
});

const readResponseBody = async (res) => {
  const raw = await res.text();
  if (!raw) return null;
  try { return JSON.parse(raw); } catch { return { message: raw }; }
};

const normalizeInternationalPhone = (value) =>
  String(value || "").replace(/[^\d+]/g, "").replace(/(?!^)\+/g, "");

const isValidInternationalPhone = (value) => {
  if (!value) return true;
  return /^\+[1-9]\d{7,14}$/.test(value);
};

/* ── Design tokens (match Home page) ── */
const C = {
  teal: "#08b7aa",
  blue: "#2f9be7",
  purple: "#9b75f6",
  ink: "#111827",
  muted: "#667085",
  border: "#dfe7ef",
  bg: "#f8fbfd",
  card: "#ffffff",
  danger: "hsl(0,84%,60%)",
  dangerBg: "hsla(0,84%,60%,0.10)",
  tealBg: "rgba(8,183,170,0.10)",
  blueBg: "rgba(47,155,231,0.10)",
  purpleBg: "rgba(155,117,246,0.12)",
  green: "hsl(142,71%,45%)",
  shadow: "0 20px 50px rgba(34,62,88,0.08)",
  r: "22px",
  rIcon: "14px",
};

const grad = `linear-gradient(135deg, ${C.blue}, ${C.teal})`;

const card = { background: C.card, border: `1px solid ${C.border}`, borderRadius: C.r, boxShadow: "0 2px 6px rgba(25,38,52,0.03)" };
const lbl = { fontSize: "13px", fontWeight: 700, color: C.muted, marginBottom: "6px", display: "flex", alignItems: "center", gap: "6px", fontFamily: "'Tajawal',system-ui,sans-serif" };
const inp = { width: "100%", padding: "10px 14px", borderRadius: "14px", border: `1px solid ${C.border}`, background: C.bg, fontFamily: "'Tajawal',system-ui,sans-serif", fontSize: "14px", fontWeight: 500, color: C.ink, outline: "none", direction: "rtl", transition: "border-color .2s,box-shadow .2s" };
const fval = { fontSize: "15px", fontWeight: 600, color: C.ink, padding: "10px 0", margin: 0, fontFamily: "'Tajawal',system-ui,sans-serif" };
const btnB = { display: "inline-flex", alignItems: "center", justifyContent: "center", gap: "6px", padding: "0 20px", minHeight: "40px", borderRadius: "14px", fontFamily: "'Tajawal',system-ui,sans-serif", fontSize: "14px", fontWeight: 800, cursor: "pointer", transition: "transform .2s,box-shadow .2s", border: "none" };
const btnP = { ...btnB, background: grad, color: "#fff", boxShadow: "0 8px 20px rgba(8,183,170,0.25)" };
const btnO = { ...btnB, background: C.card, color: C.teal, border: `1px solid ${C.border}` };
const btnD = { ...btnB, background: C.danger, color: "#fff" };
const font = "'Tajawal',system-ui,sans-serif";

/* ── icon color helpers ── */
const iconBgColor = (c) => c === "blue" ? C.blueBg : c === "green" ? "rgba(34,197,94,.10)" : c === "amber" ? "rgba(245,132,31,.10)" : C.purpleBg;
const iconTextColor = (c) => c === "blue" ? C.blue : c === "green" ? "#22c55e" : c === "amber" ? "#f5841f" : C.purple;

/* ─────────────────────────────────────────── */
const Profile = () => {
  const navigate = useNavigate();
  const token = localStorage.getItem("token");

  const [loading, setLoading] = useState(true);
  const [pageError, setPageError] = useState("");
  const [profileData, setProfileData] = useState(null);
  const [editing, setEditing] = useState(false);
  const [savingProfile, setSavingProfile] = useState(false);
  const [profileSaved, setProfileSaved] = useState(false);
  const [phoneError, setPhoneError] = useState("");
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [deletingAccount, setDeletingAccount] = useState(false);
  /* password lives inside the info card, shown only while editing */
  const [showPassFields, setShowPassFields] = useState(false);
  const [changingPassword, setChangingPassword] = useState(false);
  const [passwordChanged, setPasswordChanged] = useState(false);
  const [passwordError, setPasswordError] = useState("");
  const [showCurrentPass, setShowCurrentPass] = useState(false);
  const [showNewPass, setShowNewPass] = useState(false);
  const [showConfirmPass, setShowConfirmPass] = useState(false);

  const [formData, setFormData] = useState({ fullName: "", email: "", phone: "" });
  const [passData, setPassData] = useState({ currentPassword: "", newPassword: "", confirmNewPassword: "" });

  const authHeaders = useMemo(() => ({ "Content-Type": "application/json", Authorization: `Bearer ${token}` }), [token]);
  const activityHistory = useMemo(() => profileData?.activityHistory || [], [profileData]);

  const handlePhoneChange = (value) => {
    const cleaned = normalizeInternationalPhone(value);
    setFormData((prev) => ({ ...prev, phone: cleaned }));
    setPhoneError(cleaned && !isValidInternationalPhone(cleaned) ? "رقم الهاتف يجب أن يكون بصيغة دولية مثل: +970599123456" : "");
  };

  /* ── fetch ── */
  const fetchProfile = async () => {
    if (!token) { navigate("/login"); return; }
    try {
      setLoading(true); setPageError("");
      const res = await fetch(PROFILE_ENDPOINT, { headers: { Authorization: `Bearer ${token}` } });
      const data = await readResponseBody(res);
      if (res.status === 401 || res.status === 403) { localStorage.removeItem("token"); navigate("/login"); return; }
      if (!res.ok) { setPageError(data?.message || "فشل تحميل الملف الشخصي"); setProfileData(null); return; }
      const n = normalizeProfile(data);
      setProfileData(n);
      setFormData({ fullName: n.fullName || "", email: n.email || "", phone: n.phone || "" });
    } catch { setPageError("تعذر الاتصال بالسيرفر"); setProfileData(null); }
    finally { setLoading(false); }
  };

  useEffect(() => { fetchProfile(); }, [token]); // eslint-disable-line

  /* ── save profile ── */
  const handleSaveProfile = async () => {
    if (!formData.fullName.trim()) { setPageError("الاسم مطلوب."); return; }
    if (!isValidInternationalPhone(formData.phone)) { setPhoneError("رقم الهاتف يجب أن يكون بصيغة دولية مثل: +970599123456"); return; }
    try {
      setSavingProfile(true); setPageError(""); setPhoneError(""); setProfileSaved(false);
      const res = await fetch(PROFILE_ENDPOINT, { method: "PUT", headers: authHeaders, body: JSON.stringify({ fullName: formData.fullName.trim(), phone: formData.phone?.trim() || null }) });
      const data = await readResponseBody(res);
      if (res.status === 401 || res.status === 403) { localStorage.removeItem("token"); navigate("/login"); return; }
      if (!res.ok) { setPageError(data?.message || "فشل تحديث البيانات"); return; }
      const u = normalizeProfile(data || {});
      setProfileData((p) => p ? { ...p, fullName: u.fullName || formData.fullName.trim(), phone: u.phone } : p);
      setFormData((p) => ({ ...p, fullName: u.fullName || formData.fullName.trim(), phone: u.phone }));
      localStorage.setItem("fullName", u.fullName || formData.fullName.trim());
      setEditing(false); setShowPassFields(false); setProfileSaved(true);
      setTimeout(() => setProfileSaved(false), 2200);
    } catch { setPageError("تعذر الاتصال بالسيرفر"); }
    finally { setSavingProfile(false); }
  };

  const handleCancelEdit = () => {
    if (profileData) setFormData({ fullName: profileData.fullName || "", email: profileData.email || "", phone: profileData.phone || "" });
    setEditing(false); setShowPassFields(false); setPageError(""); setPhoneError(""); setPasswordError(""); setPassData({ currentPassword: "", newPassword: "", confirmNewPassword: "" });
  };

  /* ── change password ── */
  const handleChangePassword = async () => {
    if (!passData.currentPassword) { setPasswordError("كلمة المرور الحالية مطلوبة."); return; }
    if (!passData.newPassword) { setPasswordError("كلمة المرور الجديدة مطلوبة."); return; }
    if (passData.newPassword.length < 6) { setPasswordError("كلمة المرور الجديدة يجب أن تكون 6 أحرف على الأقل."); return; }
    if (passData.newPassword !== passData.confirmNewPassword) { setPasswordError("كلمتا المرور غير متطابقتين."); return; }
    try {
      setChangingPassword(true); setPasswordError(""); setPasswordChanged(false);
      const res = await fetch(CHANGE_PASSWORD_ENDPOINT, { method: "PUT", headers: authHeaders, body: JSON.stringify(passData) });
      const data = await readResponseBody(res);
      if (res.status === 401 || res.status === 403) { localStorage.removeItem("token"); navigate("/login"); return; }
      if (!res.ok) { setPasswordError(data?.message || "فشل تغيير كلمة المرور"); return; }
      setPasswordChanged(true);
      setPassData({ currentPassword: "", newPassword: "", confirmNewPassword: "" });
      setTimeout(() => { setShowPassFields(false); setPasswordChanged(false); }, 1600);
    } catch { setPasswordError("تعذر الاتصال بالسيرفر"); }
    finally { setChangingPassword(false); }
  };

  /* ── delete account ── */
  const handleDeleteAccount = async () => {
    try {
      setDeletingAccount(true); setPageError("");
      const res = await fetch(PROFILE_ENDPOINT, { method: "DELETE", headers: { Authorization: `Bearer ${token}` } });
      const data = await readResponseBody(res);
      if (res.status === 401 || res.status === 403) { localStorage.removeItem("token"); navigate("/login"); return; }
      if (!res.ok) { setPageError(data?.message || "فشل حذف الحساب"); return; }
      ["token", "userId", "fullName", "role", "branch", "isOnboardingCompleted", "onboarding"].forEach((k) => localStorage.removeItem(k));
      navigate("/");
    } catch { setPageError("تعذر الاتصال بالسيرفر"); }
    finally { setDeletingAccount(false); setShowDeleteConfirm(false); }
  };

  /* ── Loading / Error screens ── */
  if (loading) return (
    <DashboardLayout title="👤 الملف الشخصي" subtitle="جاري تحميل بياناتك">
      <div style={{ ...card, padding: "2rem", minHeight: "260px", display: "flex", alignItems: "center", justifyContent: "center", gap: "0.75rem" }}>
        <Loader2 style={{ color: C.teal }} className="animate-spin" />
        <span style={{ color: C.muted, fontFamily: font, fontWeight: 600 }}>جاري تحميل الملف الشخصي...</span>
      </div>
    </DashboardLayout>
  );

  if (pageError && !profileData) return (
    <DashboardLayout title="👤 الملف الشخصي" subtitle="حدثت مشكلة">
      <div style={{ ...card, padding: "2rem", textAlign: "center" }}>
        <p style={{ marginBottom: "1rem", fontWeight: 700, color: C.danger }}>{pageError}</p>
        <button style={{ ...btnP, margin: "0 auto", minWidth: "180px" }} onClick={fetchProfile}>
          <RefreshCw size={16} /> إعادة المحاولة
        </button>
      </div>
    </DashboardLayout>
  );


  return (
    <DashboardLayout title="الملف الشخصي" subtitle="معلوماتك الشخصية ونشاطك" titleIcon={User}>

      {/* outer wrapper */}
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "40px", alignItems: "start" }}>

        {/* ══ LEFT COL: info card (+ password inline) ══ */}
        <motion.div
          style={{ ...card, padding: "2.25rem", display: "flex", flexDirection: "column", gap: "2rem" }}
          initial={{ opacity: 0, x: -18 }} animate={{ opacity: 1, x: 0 }}
        >
          {/* ── avatar + name row ── */}
          <div style={{ display: "flex", alignItems: "center", gap: "1rem" }}>
            <div style={{ width: 60, height: 60, borderRadius: "50%", background: grad, display: "flex", alignItems: "center", justifyContent: "center", color: "#fff", fontSize: "24px", fontWeight: 800, boxShadow: "0 8px 20px rgba(8,183,170,0.28)", fontFamily: font, flexShrink: 0 }}>
              {(formData.fullName || "A").charAt(0).toUpperCase()}
            </div>
            <div style={{ flex: 1, minWidth: 0 }}>
              <h2 style={{ margin: 0, fontSize: "1.125rem", fontWeight: 800, color: C.ink, fontFamily: font, overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>
                {formData.fullName || "مسؤول النظام"}
              </h2>
              <p style={{ margin: "3px 0 0", color: C.teal, display: "flex", alignItems: "center", gap: "5px", fontSize: "13px", fontWeight: 700, fontFamily: font }}>
                <ShieldCheck size={13} />{profileData?.roleLabel || "مسؤول"}
              </p>
            </div>

            {/* edit / save / cancel buttons */}
            <div style={{ display: "flex", gap: "8px", flexShrink: 0 }}>
              {editing && <button style={btnO} onClick={handleCancelEdit} disabled={savingProfile}>إلغاء</button>}
              <button
                style={editing ? btnP : btnO}
                onClick={() => { if (editing) { handleSaveProfile(); } else { setEditing(true); setPageError(""); setPhoneError(""); setProfileSaved(false); } }}
                disabled={savingProfile}
              >
                {savingProfile ? <><Loader2 size={14} className="animate-spin" /> حفظ...</>
                  : editing ? <><Save size={14} /> حفظ</>
                    : <><Edit3 size={14} /> تعديل</>}
              </button>
            </div>
          </div>

          {/* ── success / error banners ── */}
          {profileSaved && (
            <div style={{ color: C.green, display: "flex", alignItems: "center", gap: "6px", fontWeight: 700, fontSize: "14px", fontFamily: font }}>
              <CheckCircle size={15} /> تم حفظ البيانات بنجاح
            </div>
          )}
          {pageError && (
            <div style={{ color: C.danger, fontWeight: 700, fontSize: "14px", fontFamily: font }}>{pageError}</div>
          )}

          {/* ── profile fields ── */}
          <div style={{ display: "flex", flexDirection: "column" }}>
            {[
              { label: "الاسم", icon: <User size={13} />, field: "fullName", editable: true, ph: "" },
              { label: "رقم الهاتف", icon: <Phone size={13} />, field: "phone", editable: true, ph: "مثال: 0590000000" },
              { label: "البريد الإلكتروني", icon: <Mail size={13} />, field: "email", editable: false, ph: "" },
            ].map(({ label, icon, field, editable, ph }, idx, arr) => (
              <div key={field} style={{ paddingBottom: "1.5rem", marginBottom: idx < arr.length - 1 ? "0" : "0", borderBottom: idx < arr.length - 1 ? `1px solid ${C.border}` : "none" }}>
                <label style={lbl}>{icon}{label}</label>
                {editing && editable
                  ? <input
                    style={field === "phone" ? { ...inp, paddingLeft: "14px", textAlign: "left" } : inp}
                    value={formData[field]}
                    placeholder={field === "phone" ? "+970599123456" : ph}
                    type={field === "phone" ? "tel" : "text"}
                    inputMode={field === "phone" ? "tel" : undefined}
                    autoComplete={field === "phone" ? "tel" : undefined}
                    dir={field === "phone" ? "ltr" : undefined}
                    onChange={(e) => field === "phone" ? handlePhoneChange(e.target.value) : setFormData({ ...formData, [field]: e.target.value })}
                    onFocus={(e) => { e.target.style.borderColor = C.teal; e.target.style.boxShadow = `0 0 0 3px ${C.tealBg}`; }}
                    onBlur={(e) => { e.target.style.borderColor = C.border; e.target.style.boxShadow = "none"; }} />
                  : <p style={fval}>{formData[field] || "—"}</p>
                }
                {editing && field === "phone" && phoneError && (
                  <p style={{ margin: "4px 0 0", fontSize: "12px", color: C.danger, fontFamily: font, fontWeight: 600 }}>{phoneError}</p>
                )}
              </div>
            ))}
          </div>

          {/* ── password section — visible only while editing ── */}
          <AnimatePresence>
            {editing && (
              <motion.div
                key="pass-section"
                initial={{ opacity: 0, height: 0 }} animate={{ opacity: 1, height: "auto" }} exit={{ opacity: 0, height: 0 }}
                style={{ overflow: "hidden" }}
              >
                {/* divider */}
                <div style={{ borderTop: `1px dashed ${C.border}`, marginBottom: "1.25rem" }} />

                {/* toggle row */}
                <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", marginBottom: showPassFields ? "1rem" : 0 }}>
                  <h4 style={{ margin: 0, fontSize: "14px", fontWeight: 800, color: C.ink, display: "flex", alignItems: "center", gap: "7px", fontFamily: font }}>
                    <span style={{ color: C.teal, display: "flex" }}><Lock size={16} /></span>
                    تغيير كلمة المرور
                  </h4>
                  <button style={{ ...btnO, minHeight: "34px", padding: "0 14px", fontSize: "13px" }}
                    onClick={() => { setShowPassFields(!showPassFields); setPasswordError(""); setPasswordChanged(false); }}>
                    {showPassFields ? "إخفاء" : "تغيير"}
                  </button>
                </div>

                <AnimatePresence>
                  {showPassFields && (
                    <motion.div key="pass-fields"
                      initial={{ opacity: 0, y: -8 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0, y: -8 }}
                      style={{ display: "flex", flexDirection: "column", gap: "1.4rem", marginTop: "1rem" }}
                    >
                      {[
                        { label: "كلمة المرور الحالية", field: "currentPassword", show: showCurrentPass, toggle: () => setShowCurrentPass(!showCurrentPass) },
                        { label: "كلمة المرور الجديدة", field: "newPassword", show: showNewPass, toggle: () => setShowNewPass(!showNewPass) },
                        { label: "تأكيد كلمة المرور الجديدة", field: "confirmNewPassword", show: showConfirmPass, toggle: () => setShowConfirmPass(!showConfirmPass) },
                      ].map(({ label, field, show, toggle }) => (
                        <div key={field}>
                          <label style={lbl}>{label}</label>
                          <div style={{ position: "relative" }}>
                            <input style={{ ...inp, paddingLeft: "2.5rem" }}
                              type={show ? "text" : "password"} value={passData[field]} placeholder={label}
                              onChange={(e) => setPassData({ ...passData, [field]: e.target.value })}
                              onFocus={(e) => { e.target.style.borderColor = C.teal; e.target.style.boxShadow = `0 0 0 3px ${C.tealBg}`; }}
                              onBlur={(e) => { e.target.style.borderColor = C.border; e.target.style.boxShadow = "none"; }} />
                            <button type="button" onClick={toggle} style={{ position: "absolute", left: "0.7rem", top: "50%", transform: "translateY(-50%)", background: "none", border: "none", cursor: "pointer", color: C.muted, padding: 0 }}>
                              {show ? <EyeOff size={15} /> : <Eye size={15} />}
                            </button>
                          </div>
                        </div>
                      ))}

                      {passData.newPassword && passData.confirmNewPassword && passData.newPassword !== passData.confirmNewPassword && (
                        <p style={{ margin: 0, fontSize: "13px", color: C.danger, fontWeight: 600, fontFamily: font }}>كلمتا المرور غير متطابقتين</p>
                      )}
                      {passwordError && (
                        <p style={{ margin: 0, fontSize: "13px", color: C.danger, fontWeight: 600, fontFamily: font }}>{passwordError}</p>
                      )}
                      {passwordChanged && (
                        <div style={{ display: "flex", alignItems: "center", gap: "6px", color: C.green, fontSize: "13px", fontWeight: 700, fontFamily: font }}>
                          <CheckCircle size={15} /> تم تغيير كلمة المرور بنجاح
                        </div>
                      )}

                      <button
                        style={{ ...btnP, width: "fit-content", minWidth: "150px" }}
                        disabled={changingPassword || !passData.currentPassword || !passData.newPassword || !passData.confirmNewPassword || passData.newPassword !== passData.confirmNewPassword}
                        onClick={handleChangePassword}
                      >
                        {changingPassword ? <><Loader2 size={13} className="animate-spin" /> جاري الحفظ...</> : <><Save size={13} /> حفظ كلمة المرور</>}
                      </button>
                    </motion.div>
                  )}
                </AnimatePresence>
              </motion.div>
            )}
          </AnimatePresence>
        </motion.div>

        {/* ══ DANGER ZONE — full width ══ */}
        <motion.div
          style={{ ...card, padding: "1.5rem", gridColumn: "1 / -1", borderColor: C.danger, marginTop: "8px" }}
          initial={{ opacity: 0, y: 18 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.15 }}
        >
          <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", flexWrap: "wrap", gap: "1rem" }}>
            <div>
              <h3 style={{ margin: "0 0 4px", fontSize: "15px", fontWeight: 800, color: C.danger, display: "flex", alignItems: "center", gap: "8px", fontFamily: font }}>
                <AlertTriangle size={17} /> منطقة الخطر
              </h3>
              <p style={{ margin: 0, fontSize: "13px", color: C.muted, fontFamily: font, fontWeight: 500 }}>
                حذف الحساب سيؤدي إلى تعطيل حسابك أو حذف بياناتك حسب إعدادات النظام.
              </p>
            </div>
            <button style={{ ...btnD, minWidth: "130px" }} onClick={() => setShowDeleteConfirm(true)}>
              <Trash2 size={15} /> حذف الحساب
            </button>
          </div>
        </motion.div>
      </div>

      {/* ══ Delete confirm modal ══ */}
      {showDeleteConfirm && (
        <div style={{ position: "fixed", inset: 0, background: "hsla(0,0%,0%,.5)", zIndex: 100, display: "flex", alignItems: "center", justifyContent: "center", padding: "1rem" }}
          onClick={() => setShowDeleteConfirm(false)}>
          <motion.div style={{ ...card, padding: "2rem", maxWidth: "420px", width: "100%", textAlign: "center" }}
            initial={{ scale: .9, opacity: 0 }} animate={{ scale: 1, opacity: 1 }}
            onClick={(e) => e.stopPropagation()}>
            <div style={{ width: "3.5rem", height: "3.5rem", borderRadius: "50%", background: C.dangerBg, display: "flex", alignItems: "center", justifyContent: "center", margin: "0 auto 1rem" }}>
              <AlertTriangle size={28} style={{ color: C.danger }} />
            </div>
            <h3 style={{ fontSize: "1.125rem", fontWeight: 800, marginBottom: "0.5rem", fontFamily: font, color: C.ink }}>هل أنت متأكد؟</h3>
            <p style={{ fontSize: "14px", color: C.muted, marginBottom: "1.5rem", lineHeight: 1.7, fontFamily: font, fontWeight: 500 }}>
              هذا الإجراء حساس. سيتم حذف أو تعطيل الحساب حسب منطق الباك إند.
            </p>
            <div style={{ display: "flex", gap: "0.75rem", justifyContent: "center" }}>
              <button style={btnO} onClick={() => setShowDeleteConfirm(false)} disabled={deletingAccount}>إلغاء</button>
              <button style={btnD} onClick={handleDeleteAccount} disabled={deletingAccount}>
                {deletingAccount ? <><Loader2 size={13} className="animate-spin" /> جاري التنفيذ...</> : <><Trash2 size={13} /> تأكيد</>}
              </button>
            </div>
          </motion.div>
        </div>
      )}
    </DashboardLayout>
  );
};

export default Profile;
