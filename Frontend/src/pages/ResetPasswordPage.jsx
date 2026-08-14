import React, { useEffect, useState } from "react";
import "../styles/ResetPasswordPage.css";
import { motion as Motion } from "framer-motion";
import { Eye, EyeOff, Lock } from "lucide-react";
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
    const error = new Error(data.message || "تعذر تنفيذ الطلب. حاول مرة أخرى.");
    error.suggestions = data.passwordSuggestions || [];
    throw error;
  }

  return data;
}

export default function ResetPasswordPage() {
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [email, setEmail] = useState("");
  const [error, setError] = useState("");
  const navigate = useNavigate();

  useEffect(() => {
    const storedEmail = sessionStorage.getItem("resetEmail");
    const isOtpVerified = sessionStorage.getItem("otpVerified");

    if (!storedEmail || isOtpVerified !== "true") {
      navigate("/forgot-password", { replace: true });
      return;
    }

    setEmail(storedEmail);
  }, [navigate]);

  const getPasswordValidationErrors = (password) => {
    const errors = [];

    if (password.length < 8) {
      errors.push("يجب أن تكون كلمة المرور 8 أحرف على الأقل.");
    }

    if (!/[A-Z]/.test(password)) {
      errors.push("أضيفي حرفًا كبيرًا واحدًا على الأقل.");
    }

    if (!/[a-z]/.test(password)) {
      errors.push("أضيفي حرفًا صغيرًا واحدًا على الأقل.");
    }

    if (!/\d/.test(password)) {
      errors.push("أضيفي رقمًا واحدًا على الأقل.");
    }

    if (!/[!@#$%^&*(),.?"{}|<>_\-+=~`]/.test(password)) {
      errors.push("أضيفي رمزًا خاصًا مثل ! أو @ أو #.");
    }

    return errors;
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError("");

    const passwordErrors = getPasswordValidationErrors(newPassword);
    if (passwordErrors.length > 0) {
      setError(passwordErrors.join("\n"));
      return;
    }

    if (newPassword !== confirmPassword) {
      setError("كلمتا المرور غير متطابقتين.");
      return;
    }

    try {
      setIsSubmitting(true);

      await postJson("/api/auth/reset-password", {
        email,
        newPassword,
        confirmPassword,
      });

      sessionStorage.removeItem("resetEmail");
      sessionStorage.removeItem("otpVerified");

      navigate("/login", { replace: true });
    } catch (requestError) {
      let errorMessage = requestError.message || "فشل تحديث كلمة المرور.";

      if (requestError.suggestions && requestError.suggestions.length > 0) {
        errorMessage = errorMessage + "\n\n" + requestError.suggestions.join("\n");
      }

      setError(errorMessage);
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
              <h2 className="form-title">تعيين كلمة مرور جديدة</h2>
              <p className="form-subtitle">اختر كلمة مرور قوية وآمنة لحساب {email}</p>
            </Motion.div>

            <form onSubmit={handleSubmit} className="space-y-5">
              {error && <p className="auth-flow-message auth-flow-error">{error}</p>}

              <Motion.div
                initial={{ opacity: 0, x: 20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: 0.3, duration: 0.5 }}
                className="form-group"
              >
                <label className="form-label" htmlFor="newPassword">
                  كلمة المرور الجديدة
                </label>
                <div className="input-wrapper">
                  <span className="input-icon">
                    <Lock size={20} />
                  </span>
                  <input
                    id="newPassword"
                    type={showPassword ? "text" : "password"}
                    value={newPassword}
                    onChange={(event) => setNewPassword(event.target.value)}
                    className="form-input form-input-password"
                    autoComplete="new-password"
                    required
                    minLength="8"
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword((current) => !current)}
                    className="password-toggle"
                    aria-label={showPassword ? "إخفاء كلمة المرور" : "إظهار كلمة المرور"}
                  >
                    {showPassword ? <EyeOff size={20} /> : <Eye size={20} />}
                  </button>
                </div>
              </Motion.div>

              <Motion.div
                initial={{ opacity: 0, x: 20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: 0.4, duration: 0.5 }}
                className="form-group"
              >
                <label className="form-label" htmlFor="confirmPassword">
                  تأكيد كلمة المرور
                </label>
                <div className="input-wrapper">
                  <span className="input-icon">
                    <Lock size={20} />
                  </span>
                  <input
                    id="confirmPassword"
                    type={showConfirm ? "text" : "password"}
                    value={confirmPassword}
                    onChange={(event) => setConfirmPassword(event.target.value)}
                    className="form-input form-input-password"
                    autoComplete="new-password"
                    required
                    minLength="8"
                  />
                  <button
                    type="button"
                    onClick={() => setShowConfirm((current) => !current)}
                    className="password-toggle"
                    aria-label={showConfirm ? "إخفاء تأكيد كلمة المرور" : "إظهار تأكيد كلمة المرور"}
                  >
                    {showConfirm ? <EyeOff size={20} /> : <Eye size={20} />}
                  </button>
                </div>
              </Motion.div>

              <Motion.button
                whileHover={!isSubmitting ? { scale: 1.01 } : undefined}
                whileTap={!isSubmitting ? { scale: 0.99 } : undefined}
                type="submit"
                className="submit-btn"
                disabled={isSubmitting}
              >
                {isSubmitting ? "جاري التحديث..." : "تحديث كلمة المرور"}
              </Motion.button>
            </form>
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
                <h3 className="hero-title">كلمة مرور قوية</h3>
                <p className="hero-description">استخدم حروفا كبيرة وصغيرة ورقما ورمزا خاصا.</p>
              </div>
            </div>
          </div>
        </Motion.div>
      </main>
    </div>
  );
}
