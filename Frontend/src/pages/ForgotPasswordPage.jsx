import React, { useState } from "react";
import "../styles/ForgotPasswordPage.css";
import { motion as Motion } from "framer-motion";
import { ArrowLeft, Mail } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { API_BASE_URL } from "@/config/api";
import PublicNavbar from "../components/public-navbar/PublicNavbar.jsx";

async function postJson(path, body) {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });

  const data = await response.json().catch(() => null);

  if (!response.ok) {
    throw new Error(
      data?.message ||
        "تعذر إرسال رمز التحقق. تأكد من إعدادات البريد الإلكتروني ثم حاول مرة أخرى."
    );
  }

  return data;
}

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const navigate = useNavigate();

  const handleSubmit = async (event) => {
    event.preventDefault();
    setMessage("");
    setError("");

    try {
      setIsSubmitting(true);

      await postJson("/api/auth/forgot-password", { email: email.trim() });

      sessionStorage.setItem("resetEmail", email.trim());
      sessionStorage.removeItem("otpVerified");

      setMessage("تم إرسال رمز التحقق إلى بريدك الإلكتروني.");
      navigate("/otp-verification");
    } catch (requestError) {
      setError(requestError.message || "حدث خطأ. يرجى المحاولة لاحقا.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="layout-wrapper auth-flow-page" dir="rtl">
      <PublicNavbar compact />
      <main className="main-content">
        <Motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6, ease: "easeOut" }}
          className="card-container"
        >
          <div className="form-section">
            <Motion.div
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ delay: 0.2, duration: 0.5 }}
              className="form-header"
            >
              <h2 className="form-title">استعادة كلمة المرور</h2>
              <p className="form-subtitle">أدخل بريدك الإلكتروني لإرسال رمز تحقق آمن.</p>
            </Motion.div>

            <form onSubmit={handleSubmit} className="space-y-5">
              {error && <p className="auth-flow-message auth-flow-error">{error}</p>}
              {message && <p className="auth-flow-message auth-flow-success">{message}</p>}

              <Motion.div
                initial={{ opacity: 0, x: 20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: 0.3, duration: 0.5 }}
                className="form-group"
              >
                <label className="form-label" htmlFor="email">
                  البريد الإلكتروني
                </label>
                <div className="input-wrapper">
                  <span className="input-icon">
                    <Mail size={20} />
                  </span>
                  <input
                    id="email"
                    type="email"
                    value={email}
                    onChange={(event) => setEmail(event.target.value)}
                    className="form-input form-input-email"
                    placeholder="example@gmail.com"
                    autoComplete="email"
                    required
                  />
                </div>
              </Motion.div>

              <Motion.button
                whileHover={!isSubmitting ? { scale: 1.01 } : undefined}
                whileTap={!isSubmitting ? { scale: 0.99 } : undefined}
                type="submit"
                className="submit-btn"
                disabled={isSubmitting}
              >
                {isSubmitting ? "جاري الإرسال..." : "إرسال رمز التحقق"}
              </Motion.button>
            </form>

            <div className="footer-link-section">
              <button type="button" className="footer-link" onClick={() => navigate("/login")}>
                <ArrowLeft size={14} className="rotate-180 ml-1" />
                العودة إلى تسجيل الدخول
              </button>
            </div>
          </div>

          <div className="visual-section">
            <div className="bg-blur-1"></div>
            <div className="bg-blur-2"></div>
            <div className="visual-content">
              <Motion.div
                initial={{ scale: 0.8, opacity: 0 }}
                animate={{ scale: 1, opacity: 1 }}
                transition={{ delay: 0.4, type: "spring", stiffness: 100 }}
                className="robot-image-wrapper"
              >
                <div className="robot-image"></div>
              </Motion.div>
              <div className="hero-text">
                <h3 className="hero-title">استعد الوصول إلى حسابك</h3>
                <p className="hero-description">سنرسل رمز تحقق صالحا لمدة 3 دقائق.</p>
              </div>
            </div>
          </div>
        </Motion.div>
      </main>
    </div>
  );
}
