import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { motion } from "framer-motion";
import { GraduationCap, Clock, Target, ArrowLeft, Loader2 } from "lucide-react";
import logo from "../../../assets/EDU.svg";
import { API_BASE_URL } from "@/config/api";

const branches = ["العلمي", "الأدبي", "الصناعي", "التجاري", "الشرعي"];
const studyHours = ["أقل من ساعة", "١-٢ ساعة", "٣-٤ ساعات", "أكثر من ٤ ساعات"];
const goals = ["أعلى من ٩٠٪", "٨٠٪ - ٩٠٪", "٧٠٪ - ٨٠٪", "النجاح فقط"];

/* ── Design tokens (Home palette) ── */
const C = {
  teal: "#08b7aa",
  blue: "#2f9be7",
  ink: "#111827",
  muted: "#667085",
  border: "#dfe7ef",
  bg: "#f8fbfd",
  card: "#ffffff",
  danger: "hsl(0,84%,60%)",
  tealBg: "rgba(8,183,170,0.08)",
  blueBg: "rgba(47,155,231,0.08)",
};
const grad = `linear-gradient(135deg, ${C.blue}, ${C.teal})`;
const font = "'Tajawal', system-ui, sans-serif";

const Chip = ({ label, selected, onClick, disabled = false }) => (
  <button
    type="button"
    onClick={onClick}
    disabled={disabled}
    style={{
      padding: "8px 20px",
      borderRadius: "999px",
      border: selected ? "none" : `1.5px solid ${C.border}`,
      background: selected ? grad : C.card,
      color: selected ? "#fff" : C.ink,
      fontFamily: font,
      fontSize: "14px",
      fontWeight: selected ? 800 : 500,
      cursor: disabled ? "not-allowed" : "pointer",
      opacity: disabled ? 0.5 : 1,
      boxShadow: selected ? "0 4px 14px rgba(8,183,170,0.25)" : "none",
      transition: "all 0.2s ease",
      whiteSpace: "nowrap",
    }}
  >
    {label}
  </button>
);

const SectionHeader = ({ icon: Icon, label }) => (
  <div style={{ display: "flex", alignItems: "center", gap: "10px", marginBottom: "14px" }}>
    <div style={{
      width: 36, height: 36, borderRadius: "10px",
      background: C.tealBg, color: C.teal,
      display: "flex", alignItems: "center", justifyContent: "center", flexShrink: 0,
    }}>
      <Icon size={18} />
    </div>
    <span style={{ fontSize: "15px", fontWeight: 700, color: C.ink, fontFamily: font }}>
      {label}
    </span>
  </div>
);

const normalizeSubject = (subject) => ({
  subjectId: subject.subjectId || subject.SubjectId,
  subjectName: subject.subjectName || subject.SubjectName || "",
});

const OnboardingStep1 = () => {
  const navigate = useNavigate();

  const [branch, setBranch] = useState("");
  const [difficult, setDifficult] = useState([]);
  const [hours, setHours] = useState("");
  const [goal, setGoal] = useState("");
  const [error, setError] = useState("");

  const [subjects, setSubjects] = useState([]);
  const [subjectsLoading, setSubjectsLoading] = useState(false);
  const [subjectsError, setSubjectsError] = useState("");

  useEffect(() => {
    const token = localStorage.getItem("token");
    const isOnboardingCompleted = localStorage.getItem("isOnboardingCompleted");

    if (!token) { navigate("/login"); return; }
    if (isOnboardingCompleted === "true") { navigate("/dashboard"); return; }

    const savedData = JSON.parse(localStorage.getItem("onboarding") || "{}");
    if (savedData.branch) setBranch(savedData.branch);
    if (savedData.difficult) setDifficult(savedData.difficult);
    if (savedData.hours) setHours(savedData.hours);
    if (savedData.goal) setGoal(savedData.goal);
  }, [navigate]);

  useEffect(() => {
    const fetchSubjectsByBranch = async () => {
      if (!branch) { setSubjects([]); setSubjectsError(""); return; }
      try {
        setSubjectsLoading(true); setSubjectsError("");
        const response = await fetch(
          `${API_BASE_URL}/api/student/subjects/by-branch?branch=${encodeURIComponent(branch)}`
        );
        const rawText = await response.text();
        let data;
        try { data = JSON.parse(rawText); } catch { data = []; }
        if (!response.ok) { setSubjectsError(data?.message || "فشل تحميل المواد"); setSubjects([]); return; }
        setSubjects((Array.isArray(data) ? data : []).map(normalizeSubject));
      } catch (err) {
        console.error(err); setSubjectsError("تعذر الاتصال بالسيرفر"); setSubjects([]);
      } finally { setSubjectsLoading(false); }
    };
    fetchSubjectsByBranch();
  }, [branch]);

  const handleBranchSelect = (selectedBranch) => {
    setBranch(selectedBranch); setDifficult([]); setSubjects([]); setSubjectsError("");
  };

  const toggleDifficult = (subjectName) => {
    setDifficult((prev) =>
      prev.includes(subjectName) ? prev.filter((x) => x !== subjectName) : [...prev, subjectName]
    );
  };

  const handleNext = () => {
    if (!branch || difficult.length === 0 || !hours || !goal) {
      setError("الرجاء تعبئة جميع الحقول"); return;
    }
    setError("");
    localStorage.setItem("onboarding", JSON.stringify({ branch, difficult, hours, goal }));
    navigate("/onboarding/2");
  };

  return (
    <div style={{
      minHeight: "100vh", background: C.bg, display: "flex",
      alignItems: "center", justifyContent: "center",
      padding: "2rem 1rem", fontFamily: font, direction: "rtl",
    }}>
      {/* subtle background blobs */}
      <div style={{ position: "fixed", inset: 0, pointerEvents: "none", overflow: "hidden", zIndex: 0 }}>
        <div style={{ position: "absolute", top: "-10%", right: "-8%", width: 420, height: 420, borderRadius: "50%", background: "rgba(8,183,170,0.07)", filter: "blur(60px)" }} />
        <div style={{ position: "absolute", bottom: "-10%", left: "-8%", width: 380, height: 380, borderRadius: "50%", background: "rgba(47,155,231,0.07)", filter: "blur(60px)" }} />
      </div>

      <motion.div
        style={{ width: "100%", maxWidth: 560, position: "relative", zIndex: 1 }}
        initial={{ opacity: 0, y: 24 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5 }}
      >
        {/* ── Header ── */}
        <div style={{ textAlign: "center", marginBottom: "2rem" }}>
          <Link to="/" style={{ display: "inline-flex", alignItems: "center", gap: "8px", marginBottom: "1.75rem", textDecoration: "none" }}>
            <div style={{ width: "2rem", height: "2rem" }}>
              <img src={logo} alt="logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} />
            </div>
            <span style={{
              fontFamily: "'Tajawal', system-ui, sans-serif",
              fontSize: "1.4rem", fontWeight: 900,
              background: grad, WebkitBackgroundClip: "text",
              WebkitTextFillColor: "transparent", backgroundClip: "text",
            }}>EduNext</span>
          </Link>

          {/* progress bars */}
          <div style={{ display: "flex", gap: "8px", justifyContent: "center", marginBottom: "1.5rem" }}>
            {[true, false].map((active, i) => (
              <div key={i} style={{
                height: "5px", width: "80px", borderRadius: "999px",
                background: active ? grad : C.border,
                transition: "background 0.3s",
              }} />
            ))}
          </div>

          <p style={{ fontSize: "13px", fontWeight: 700, color: C.teal, marginBottom: "6px", fontFamily: font }}>
            الخطوة ١ من ٢
          </p>
          <h1 style={{ fontSize: "1.6rem", fontWeight: 900, color: C.ink, margin: "0 0 8px", fontFamily: font }}>
            أخبرنا عن نفسك
          </h1>
          <p style={{ fontSize: "14px", color: C.muted, margin: 0, fontFamily: font }}>
            سنستخدم هذه المعلومات لبناء خطة دراسية مخصصة لك.
          </p>
        </div>

        {/* ── Card ── */}
        <div style={{
          background: C.card, borderRadius: "22px",
          border: `1px solid ${C.border}`,
          boxShadow: "0 4px 24px rgba(34,62,88,0.07)",
          padding: "2rem",
          display: "flex", flexDirection: "column", gap: "1.75rem",
        }}>

          {/* Branch */}
          <div>
            <SectionHeader icon={GraduationCap} label="ما هو فرعك الدراسي؟" />
            <div style={{ display: "flex", flexWrap: "wrap", gap: "10px" }}>
              {branches.map((b) => (
                <Chip key={b} label={b} selected={branch === b} onClick={() => handleBranchSelect(b)} />
              ))}
            </div>
          </div>

          {/* Difficult subjects */}
          <div>
            <SectionHeader icon={Target} label="ما المواد التي تجدها صعبة؟" />
            {!branch ? (
              <p style={{ textAlign: "center", color: C.muted, fontSize: "14px", fontFamily: font, margin: 0 }}>
                اختر الفرع أولًا لتظهر المواد الخاصة به
              </p>
            ) : subjectsLoading ? (
              <div style={{ textAlign: "center", display: "flex", alignItems: "center", justifyContent: "center", gap: "8px" }}>
                <Loader2 size={18} className="animate-spin" style={{ color: C.teal }} />
                <p style={{ color: C.muted, fontFamily: font, margin: 0, fontSize: "14px" }}>جاري تحميل المواد...</p>
              </div>
            ) : subjectsError ? (
              <p style={{ textAlign: "center", color: C.danger, fontSize: "14px", fontFamily: font, margin: 0 }}>{subjectsError}</p>
            ) : subjects.length === 0 ? (
              <p style={{ textAlign: "center", color: C.muted, fontSize: "14px", fontFamily: font, margin: 0 }}>لا توجد مواد متاحة لهذا الفرع حاليًا</p>
            ) : (
              <div style={{ display: "flex", flexWrap: "wrap", gap: "10px" }}>
                {subjects.map((s) => (
                  <Chip key={s.subjectId} label={s.subjectName}
                    selected={difficult.includes(s.subjectName)}
                    onClick={() => toggleDifficult(s.subjectName)} />
                ))}
              </div>
            )}
          </div>

          {/* Study hours */}
          <div>
            <SectionHeader icon={Clock} label="كم ساعة تدرس يوميًا؟" />
            <div style={{ display: "flex", flexWrap: "wrap", gap: "10px" }}>
              {studyHours.map((h) => (
                <Chip key={h} label={h} selected={hours === h} onClick={() => setHours(h)} />
              ))}
            </div>
          </div>

          {/* Goal */}
          <div>
            <SectionHeader icon={Target} label="ما هدفك في التوجيهي؟" />
            <div style={{ display: "flex", flexWrap: "wrap", gap: "10px" }}>
              {goals.map((g) => (
                <Chip key={g} label={g} selected={goal === g} onClick={() => setGoal(g)} />
              ))}
            </div>
          </div>

          {/* Error */}
          {error && (
            <p style={{ color: C.danger, textAlign: "center", fontSize: "14px", fontFamily: font, fontWeight: 600, margin: 0 }}>
              {error}
            </p>
          )}

          {/* Next button */}
          <button
            onClick={handleNext}
            style={{
              width: "100%", minHeight: "52px", borderRadius: "14px", border: "none",
              background: grad, color: "#fff",
              fontSize: "16px", fontWeight: 800, fontFamily: font,
              cursor: "pointer",
              display: "flex", alignItems: "center", justifyContent: "center", gap: "8px",
              boxShadow: "0 4px 18px rgba(8,183,170,0.30)",
              transition: "opacity 0.2s ease, transform 0.2s ease",
            }}
            onMouseEnter={(e) => { e.currentTarget.style.opacity = "0.92"; e.currentTarget.style.transform = "translateY(-1px)"; }}
            onMouseLeave={(e) => { e.currentTarget.style.opacity = "1"; e.currentTarget.style.transform = "translateY(0)"; }}
          >
            التالي
            <ArrowLeft size={18} />
          </button>
        </div>
      </motion.div>
    </div>
  );
};

export default OnboardingStep1;