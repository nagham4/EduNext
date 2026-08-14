import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { motion } from "framer-motion";
import { Video, BookOpen, PenTool, BarChart3, Rocket, CheckCircle } from "lucide-react";
import successImg from "@/assets/onboarding-success.png";
import logo from "../../../assets/EDU.svg";
import { API_BASE_URL } from "@/config/api";

const learningMethods = [
  { id: "فيديوهات", label: "فيديوهات", icon: Video, desc: "تعلم بالمشاهدة" },
  { id: "قراءة وملخصات", label: "قراءة وملخصات", icon: BookOpen, desc: "تعلم بالقراءة" },
  { id: "أسئلة وتمارين", label: "أسئلة وتمارين", icon: PenTool, desc: "تعلم بالتطبيق" },
];

const levels = ["مبتدئ", "متوسط", "متقدم"];
const examOptions = ["نعم، عدة مرات", "مرة واحدة", "لا، لم أجرب بعد"];

/* ── Design tokens (Home palette) ── */
const C = {
  teal:     "#08b7aa",
  blue:     "#2f9be7",
  ink:      "#111827",
  muted:    "#667085",
  border:   "#dfe7ef",
  bg:       "#f8fbfd",
  card:     "#ffffff",
  danger:   "hsl(0,84%,60%)",
  tealBg:   "rgba(8,183,170,0.08)",
};
const grad = `linear-gradient(135deg, ${C.blue}, ${C.teal})`;
const font = "'Tajawal', system-ui, sans-serif";

const Chip = ({ label, selected, onClick }) => (
  <button
    type="button"
    onClick={onClick}
    style={{
      padding: "8px 20px",
      borderRadius: "999px",
      border: selected ? "none" : `1.5px solid ${C.border}`,
      background: selected ? grad : C.card,
      color: selected ? "#fff" : C.ink,
      fontFamily: font,
      fontSize: "14px",
      fontWeight: selected ? 800 : 500,
      cursor: "pointer",
      boxShadow: selected ? "0 4px 14px rgba(8,183,170,0.25)" : "none",
      transition: "all 0.2s ease",
      whiteSpace: "nowrap",
    }}
  >
    {label}
  </button>
);

const SelectableCard = ({ label, desc, icon: Icon, selected, onClick }) => (
  <button
    type="button"
    onClick={onClick}
    style={{
      display: "flex", flexDirection: "column", alignItems: "center",
      gap: "8px", padding: "1.25rem 1rem",
      borderRadius: "16px",
      border: selected ? "2px solid transparent" : `1.5px solid ${C.border}`,
      background: selected
        ? `linear-gradient(${C.card}, ${C.card}) padding-box, ${grad} border-box`
        : C.card,
      boxShadow: selected ? "0 4px 18px rgba(8,183,170,0.18)" : "0 1px 4px rgba(34,62,88,0.04)",
      cursor: "pointer",
      transition: "all 0.22s ease",
      flex: 1,
      minWidth: "120px",
    }}
  >
    <div style={{
      width: 44, height: 44, borderRadius: "12px",
      background: selected ? grad : C.tealBg,
      color: selected ? "#fff" : C.teal,
      display: "flex", alignItems: "center", justifyContent: "center",
      transition: "all 0.22s ease",
    }}>
      <Icon size={20} />
    </div>
    <span style={{ fontSize: "14px", fontWeight: 800, color: selected ? C.teal : C.ink, fontFamily: font }}>
      {label}
    </span>
    <span style={{ fontSize: "12px", color: C.muted, fontFamily: font }}>
      {desc}
    </span>
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

const OnboardingStep2 = () => {
  const navigate = useNavigate();

  const [methods, setMethods] = useState([]);
  const [level, setLevel] = useState("");
  const [examExp, setExamExp] = useState("");
  const [error, setError] = useState("");
  const [done, setDone] = useState(false);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const token = localStorage.getItem("token");
    const isOnboardingCompleted = localStorage.getItem("isOnboardingCompleted");
    const savedData = JSON.parse(localStorage.getItem("onboarding") || "{}");

    if (!token) { navigate("/login"); return; }
    if (isOnboardingCompleted === "true") { navigate("/dashboard"); return; }
    if (!savedData.branch || !savedData.difficult || !savedData.hours || !savedData.goal) {
      navigate("/onboarding/1"); return;
    }

    if (savedData.methods) setMethods(savedData.methods);
    if (savedData.level) setLevel(savedData.level);
    if (savedData.examExp) setExamExp(savedData.examExp);
  }, [navigate]);

  const toggleMethod = (id) => {
    setMethods((prev) =>
      prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]
    );
  };

  const handleFinish = async () => {
    if (methods.length === 0 || !level || !examExp) {
      setError("الرجاء تعبئة جميع الحقول"); return;
    }
    setError("");
    const step1Data = JSON.parse(localStorage.getItem("onboarding") || "{}");
    const token = localStorage.getItem("token");
    const payload = { branch: step1Data.branch, difficult: step1Data.difficult || [], hours: step1Data.hours, goal: step1Data.goal, methods, level, examExp };

    try {
      setLoading(true);
      const response = await fetch(`${API_BASE_URL}/api/student/setup/complete-onboarding`, {
        method: "POST",
        headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
        body: JSON.stringify(payload),
      });
      const data = await response.json();
      if (!response.ok) { setError(data.message || "فشل حفظ بيانات الطالب"); return; }
      localStorage.setItem("onboarding", JSON.stringify({ ...step1Data, methods, level, examExp }));
      localStorage.setItem("isOnboardingCompleted", "true");
      localStorage.setItem("branch", data.branch || payload.branch);
      setDone(true);
    } catch (err) {
      console.error(err); setError("تعذر الاتصال بالسيرفر");
    } finally {
      setLoading(false);
    }
  };

  /* ── Success screen ── */
  if (done) {
    return (
      <div style={{
        minHeight: "100vh", background: C.bg,
        display: "flex", alignItems: "center", justifyContent: "center",
        fontFamily: font, direction: "rtl", padding: "2rem",
      }}>
        <div style={{ position: "fixed", inset: 0, pointerEvents: "none", overflow: "hidden" }}>
          <div style={{ position: "absolute", top: "-10%", right: "-8%", width: 420, height: 420, borderRadius: "50%", background: "rgba(8,183,170,0.07)", filter: "blur(60px)" }} />
          <div style={{ position: "absolute", bottom: "-10%", left: "-8%", width: 380, height: 380, borderRadius: "50%", background: "rgba(47,155,231,0.07)", filter: "blur(60px)" }} />
        </div>

        <motion.div
          style={{ textAlign: "center", maxWidth: 420, position: "relative", zIndex: 1 }}
          initial={{ opacity: 0, scale: 0.9 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ duration: 0.5 }}
        >
          <div style={{ marginBottom: "1.5rem" }}>
            <img src={successImg} alt="تم بنجاح" width={200} height={200} style={{ margin: "0 auto" }} />
          </div>

          <div style={{
            width: 60, height: 60, borderRadius: "50%",
            background: "rgba(8,183,170,0.12)", display: "flex",
            alignItems: "center", justifyContent: "center", margin: "0 auto 1.25rem",
          }}>
            <CheckCircle size={28} style={{ color: C.teal }} />
          </div>

          <h1 style={{ fontSize: "1.75rem", fontWeight: 900, color: C.ink, margin: "0 0 8px", fontFamily: font }}>
            تم بنجاح! 🎉
          </h1>
          <p style={{ fontSize: "16px", fontWeight: 700, color: C.teal, margin: "0 0 12px", fontFamily: font }}>
            خطتك الدراسية جاهزة
          </p>
          <p style={{ fontSize: "14px", color: C.muted, margin: "0 0 2rem", lineHeight: 1.7, fontFamily: font }}>
            سنساعدك خطوة بخطوة للوصول إلى هدفك في التوجيهي. يلا نبدأ!
          </p>

          <button
            onClick={() => { localStorage.removeItem("onboarding"); navigate("/dashboard"); }}
            style={{
              display: "inline-flex", alignItems: "center", justifyContent: "center", gap: "8px",
              padding: "0 2.5rem", minHeight: "52px", borderRadius: "14px", border: "none",
              background: grad, color: "#fff",
              fontSize: "16px", fontWeight: 800, fontFamily: font,
              cursor: "pointer",
              boxShadow: "0 4px 18px rgba(8,183,170,0.30)",
              transition: "opacity 0.2s ease, transform 0.2s ease",
            }}
            onMouseEnter={(e) => { e.currentTarget.style.opacity = "0.92"; e.currentTarget.style.transform = "translateY(-1px)"; }}
            onMouseLeave={(e) => { e.currentTarget.style.opacity = "1"; e.currentTarget.style.transform = "translateY(0)"; }}
          >
            <Rocket size={18} />
            انطلق الآن
          </button>
        </motion.div>
      </div>
    );
  }

  /* ── Main form ── */
  return (
    <div style={{
      minHeight: "100vh", background: C.bg, display: "flex",
      alignItems: "center", justifyContent: "center",
      padding: "2rem 1rem", fontFamily: font, direction: "rtl",
    }}>
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
              fontFamily: font, fontSize: "1.4rem", fontWeight: 900,
              background: grad, WebkitBackgroundClip: "text",
              WebkitTextFillColor: "transparent", backgroundClip: "text",
            }}>EduNext</span>
          </Link>

          {/* progress bars — both active */}
          <div style={{ display: "flex", gap: "8px", justifyContent: "center", marginBottom: "1.5rem" }}>
            {[true, true].map((active, i) => (
              <div key={i} style={{
                height: "5px", width: "80px", borderRadius: "999px",
                background: grad, transition: "background 0.3s",
              }} />
            ))}
          </div>

          <p style={{ fontSize: "13px", fontWeight: 700, color: C.teal, marginBottom: "6px", fontFamily: font }}>
            الخطوة ٢ من ٢
          </p>
          <h1 style={{ fontSize: "1.6rem", fontWeight: 900, color: C.ink, margin: 0, fontFamily: font }}>
            كيف تفضّل أن تتعلم؟
          </h1>
        </div>

        {/* ── Card ── */}
        <div style={{
          background: C.card, borderRadius: "22px",
          border: `1px solid ${C.border}`,
          boxShadow: "0 4px 24px rgba(34,62,88,0.07)",
          padding: "2rem",
          display: "flex", flexDirection: "column", gap: "1.75rem",
        }}>

          {/* Learning methods */}
          <div>
            <p style={{ fontSize: "15px", fontWeight: 700, color: C.ink, margin: "0 0 14px", fontFamily: font }}>
              طريقة التعلم المفضلة
            </p>
            <div style={{ display: "flex", gap: "12px", flexWrap: "wrap" }}>
              {learningMethods.map((m) => (
                <SelectableCard
                  key={m.id}
                  label={m.label}
                  desc={m.desc}
                  icon={m.icon}
                  selected={methods.includes(m.id)}
                  onClick={() => toggleMethod(m.id)}
                />
              ))}
            </div>
          </div>

          {/* Level */}
          <div>
            <SectionHeader icon={BarChart3} label="مستواك الحالي" />
            <div style={{ display: "flex", flexWrap: "wrap", gap: "10px" }}>
              {levels.map((l) => (
                <Chip key={l} label={l} selected={level === l} onClick={() => setLevel(l)} />
              ))}
            </div>
          </div>

          {/* Exam experience */}
          <div>
            <p style={{ fontSize: "15px", fontWeight: 700, color: C.ink, margin: "0 0 14px", fontFamily: font }}>
              هل سبق وأجريت اختبارات تجريبية؟
            </p>
            <div style={{ display: "flex", flexWrap: "wrap", gap: "10px" }}>
              {examOptions.map((o) => (
                <Chip key={o} label={o} selected={examExp === o} onClick={() => setExamExp(o)} />
              ))}
            </div>
          </div>

          {/* Error */}
          {error && (
            <p style={{ color: C.danger, textAlign: "center", fontSize: "14px", fontFamily: font, fontWeight: 600, margin: 0 }}>
              {error}
            </p>
          )}

          {/* Submit button */}
          <button
            onClick={handleFinish}
            disabled={loading}
            style={{
              width: "100%", minHeight: "52px", borderRadius: "14px", border: "none",
              background: grad, color: "#fff",
              fontSize: "16px", fontWeight: 800, fontFamily: font,
              cursor: loading ? "not-allowed" : "pointer",
              opacity: loading ? 0.75 : 1,
              display: "flex", alignItems: "center", justifyContent: "center", gap: "8px",
              boxShadow: "0 4px 18px rgba(8,183,170,0.30)",
              transition: "opacity 0.2s ease, transform 0.2s ease",
            }}
            onMouseEnter={(e) => { if (!loading) { e.currentTarget.style.opacity = "0.92"; e.currentTarget.style.transform = "translateY(-1px)"; } }}
            onMouseLeave={(e) => { e.currentTarget.style.opacity = loading ? "0.75" : "1"; e.currentTarget.style.transform = "translateY(0)"; }}
          >
            <Rocket size={18} />
            {loading ? "جاري حفظ البيانات..." : "ابدأ رحلتك التعليمية"}
          </button>
        </div>
      </motion.div>
    </div>
  );
};

export default OnboardingStep2;