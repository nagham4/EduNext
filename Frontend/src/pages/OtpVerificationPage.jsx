import React, { useEffect, useState } from "react";
import "../styles/OtpVerificationPage.css";
import { motion as Motion } from "framer-motion";
import { ArrowLeft, Lock } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { API_BASE_URL } from "@/config/api";
import PublicNavbar from "../components/public-navbar/PublicNavbar.jsx";

async function postJson(path, body) {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });

  const data = await response.json().catch(() => ({}));

  if (!response.ok) {
    throw new Error(data.message || "تعذر تنفيذ الطلب. حاول مرة أخرى.");
  }

  return data;
}

export default function OtpVerificationPage() {
  const [otp, setOtp] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [email, setEmail] = useState("");
  const [error, setError] = useState("");
  const navigate = useNavigate();

  useEffect(() => {
    const storedEmail = sessionStorage.getItem("resetEmail");

    if (!storedEmail) {
      navigate("/forgot-password", { replace: true });
      return;
    }

    setEmail(storedEmail);
  }, [navigate]);

  const handleOtpChange = (event) => {
    const value = event.target.value;

    if (/^\d{0,6}$/.test(value)) {
      setOtp(value);
      setError("");
    }
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError("");

    if (otp.length !== 6) {
      setError("يرجى إدخال رمز التحقق كاملا من 6 أرقام.");
      return;
    }

    try {
      setIsSubmitting(true);

      await postJson("/api/auth/verify-otp", { email, otp });

      sessionStorage.setItem("otpVerified", "true");
      navigate("/reset-password");
    } catch (requestError) {
      setError(requestError.message || "رمز التحقق غير صحيح أو منتهي الصلاحية.");
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
              <h2 className="form-title">التحقق من الرمز</h2>
              <p className="form-subtitle">أدخل الرمز الذي تم إرساله إلى {email}</p>
            </Motion.div>

            <form onSubmit={handleSubmit} className="space-y-5">
              {error && <p className="auth-flow-message auth-flow-error">{error}</p>}

              <Motion.div
                initial={{ opacity: 0, x: 20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: 0.3, duration: 0.5 }}
                className="form-group"
              >
                <label className="form-label" htmlFor="otp">
                  رمز التحقق
                </label>
                <div className="input-wrapper">
                  <span className="input-icon">
                    <Lock size={20} />
                  </span>
                  <input
                    id="otp"
                    type="text"
                    inputMode="numeric"
                    value={otp}
                    onChange={handleOtpChange}
                    className="form-input form-input-otp"
                    placeholder="000000"
                    maxLength="6"
                    autoComplete="one-time-code"
                    required
                  />
                </div>
              </Motion.div>

              <div className="otp-info">
                <p className="info-text">الرمز صالح لمدة 3 دقائق فقط.</p>
              </div>

              <Motion.button
                whileHover={!isSubmitting && otp.length === 6 ? { scale: 1.01 } : undefined}
                whileTap={!isSubmitting && otp.length === 6 ? { scale: 0.99 } : undefined}
                type="submit"
                className="submit-btn"
                disabled={isSubmitting || otp.length !== 6}
              >
                {isSubmitting ? "جاري التحقق..." : "التحقق من الرمز"}
              </Motion.button>
            </form>

            <div className="footer-link-section">
              <button type="button" className="footer-link" onClick={() => navigate("/forgot-password")}>
                <ArrowLeft size={14} className="rotate-180 ml-1" />
                إرسال رمز جديد
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
                <h3 className="hero-title">تحقق من بريدك</h3>
                <p className="hero-description">استخدم آخر رمز وصل إلى بريدك الإلكتروني.</p>
              </div>
            </div>
          </div>
        </Motion.div>
      </main>
    </div>
  );
}
