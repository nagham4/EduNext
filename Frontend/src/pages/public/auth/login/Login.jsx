/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import React, { useState } from "react";
import "./Login.css";
import { API_BASE_URL } from "@/config/api";
import { GOOGLE_CLIENT_ID } from "@/config/google";
import { GoogleLogin } from "@react-oauth/google";
import { motion as Motion } from "framer-motion";
import {
  Mail,
  Lock,
  Eye,
  EyeOff,
  ArrowLeft,
  Sparkles,
  Home,
} from "lucide-react";
import { useNavigate } from "react-router-dom";
import PublicNavbar from "../../../../components/public-navbar/PublicNavbar.jsx";


export default function Login() {
  const navigate = useNavigate();

  const [showPassword, setShowPassword] = useState(false);
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [rememberMe, setRememberMe] = useState(false);

  const [fieldErrors, setFieldErrors] = useState({});
  const [generalError, setGeneralError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isGoogleSubmitting, setIsGoogleSubmitting] = useState(false);
  const isGoogleLoginConfigured = Boolean(GOOGLE_CLIENT_ID);

  const errorTextStyle = {
    color: "#dc2626",
    fontSize: "13px",
    fontWeight: 600,
    lineHeight: 1.5,
    marginTop: "6px",
    marginBottom: "0",
  };

  const generalErrorStyle = {
    color: "#dc2626",
    fontSize: "13px",
    fontWeight: 700,
    lineHeight: 1.5,
    marginBottom: "12px",
  };

  const clearFieldError = (fieldName) => {
    setFieldErrors((previousErrors) => ({
      ...previousErrors,
      [fieldName]: [],
    }));

    setGeneralError("");
  };

  const getFieldErrors = (fieldName) => {
    const errors = fieldErrors[fieldName];

    if (!errors) {
      return [];
    }

    return Array.isArray(errors) ? errors.filter(Boolean) : [errors];
  };

  const navigateAfterLogin = (data) => {
    const token = data.token || data.Token || "";
    const refreshToken = data.refreshToken || data.RefreshToken || "";
    const userId = data.userId || data.UserId || "";
    const fullName = data.fullName || data.FullName || "";
    const role = data.role || data.Role || "";
    const branch = data.branch || data.Branch || "";

    const onboardingCompleted =
      data.isOnboardingCompleted ??
      data.IsOnboardingCompleted ??
      data.onboardingCompleted ??
      data.onboarding_completed ??
      false;

    const normalizedRole = String(role).toLowerCase().trim();

    localStorage.setItem("token", token);
    if (refreshToken) {
      localStorage.setItem("refreshToken", refreshToken);
    }
    localStorage.setItem("userId", String(userId));
    localStorage.setItem("fullName", fullName || "");
    localStorage.setItem("role", normalizedRole);
    localStorage.setItem("branch", branch || "");
    localStorage.setItem("isOnboardingCompleted", String(onboardingCompleted));

    if (normalizedRole === "admin") {
      navigate("/admin-dashboard", { replace: true });
      return;
    }

    if (normalizedRole === "student") {
      if (onboardingCompleted) {
        navigate("/dashboard", { replace: true });
      } else {
        navigate("/onboarding/1", { replace: true });
      }

      return;
    }

    navigate("/login", { replace: true });
  };

  const validateLoginForm = () => {
    const errors = {};

    if (!email.trim()) {
      errors.email = ["البريد الإلكتروني مطلوب."];
    }

    if (!password) {
      errors.password = ["كلمة المرور مطلوبة."];
    }

    return errors;
  };

  async function login() {
    setGeneralError("");
    setFieldErrors({});

    const validationErrors = validateLoginForm();

    if (Object.keys(validationErrors).length > 0) {
      setFieldErrors(validationErrors);
      return;
    }

    setIsSubmitting(true);

    try {
      const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          email: email.trim(),
          password,
          rememberMe,
        }),
      });

      const data = await response.json().catch(() => ({}));

      if (!response.ok) {
        setGeneralError(
          data.message || "تعذر تسجيل الدخول. يرجى المحاولة مرة أخرى."
        );
        setFieldErrors(data.errors || {});
        return;
      }

      navigateAfterLogin(data);
    } catch {
      setGeneralError(
        "حدث خطأ في الاتصال بالسيرفر. تأكد من تشغيل الباك ثم حاول مرة أخرى."
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  const handleGoogleSuccess = async (credentialResponse) => {
    setGeneralError("");
    setFieldErrors({});

    if (!credentialResponse?.credential) {
      setGeneralError("تعذر الحصول على بيانات حساب جوجل. يرجى المحاولة مرة أخرى.");
      return;
    }

    setIsGoogleSubmitting(true);

    try {
      const response = await fetch(`${API_BASE_URL}/api/auth/google-login`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          idToken: credentialResponse.credential,
        }),
      });

      const data = await response.json().catch(() => ({}));

      if (!response.ok) {
        setGeneralError(data.message || "تعذر تسجيل الدخول باستخدام جوجل.");
        setFieldErrors(data.errors || {});
        return;
      }

      navigateAfterLogin(data);
    } catch {
      setGeneralError("حدث خطأ أثناء تسجيل الدخول باستخدام جوجل. يرجى المحاولة مرة أخرى.");
    } finally {
      setIsGoogleSubmitting(false);
    }
  };

  const handleGoogleError = () => {
    setGeneralError("تعذر تسجيل الدخول باستخدام جوجل.");
    setIsGoogleSubmitting(false);
  };

  const handleSubmit = (event) => {
    event.preventDefault();
    login();
  };

  const handleSignupClick = (event) => {
    event.preventDefault();
    navigate("/register");
  };

  const handleForgotPasswordClick = (event) => {
    event.preventDefault();
    navigate("/forgot-password");
  };

  return (
    <div className="layout-wrapper" dir="rtl">
      <PublicNavbar compact />
      <button type="button" className="auth-home-link" onClick={() => navigate("/")}>
        <Home size={18} />
        <span>العودة للرئيسية</span>
      </button>
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
              <h2 className="form-title">أهلاً بك مجدداً</h2>
              <p className="form-subtitle">
                سجل دخولك لمواصلة رحلة تفوقك في التوجيهي.
              </p>
            </Motion.div>

            <form onSubmit={handleSubmit} className="space-y-5" noValidate>
              {generalError && <p style={generalErrorStyle}>{generalError}</p>}

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
                    onChange={(event) => {
                      setEmail(event.target.value);
                      clearFieldError("email");
                    }}
                    className="form-input"
                    placeholder="example@gmail.com"
                    autoComplete="email"
                  />
                </div>

                {getFieldErrors("email").map((message) => (
                  <p style={errorTextStyle} key={message}>
                    {message}
                  </p>
                ))}
              </Motion.div>

              <Motion.div
                initial={{ opacity: 0, x: 20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: 0.4, duration: 0.5 }}
                className="form-group"
              >
                <div className="password-label-wrapper">
                  <label className="form-label" htmlFor="password">
                    كلمة المرور
                  </label>

                  <a href="#" onClick={handleForgotPasswordClick}>
                    نسيت كلمة المرور؟
                  </a>
                </div>

                <div className="input-wrapper">
                  <span className="input-icon">
                    <Lock size={20} />
                  </span>

                  <input
                    id="password"
                    type={showPassword ? "text" : "password"}
                    value={password}
                    onChange={(event) => {
                      setPassword(event.target.value);
                      clearFieldError("password");
                    }}
                    className="form-input form-input-password"
                    placeholder="••••••••"
                    autoComplete="current-password"
                  />

                  <button
                    type="button"
                    onClick={() => setShowPassword(!showPassword)}
                    className="password-toggle"
                    aria-label={
                      showPassword ? "إخفاء كلمة المرور" : "إظهار كلمة المرور"
                    }
                  >
                    {showPassword ? <EyeOff size={20} /> : <Eye size={20} />}
                  </button>
                </div>

                {getFieldErrors("password").map((message) => (
                  <p style={errorTextStyle} key={message}>
                    {message}
                  </p>
                ))}
              </Motion.div>

              <Motion.div
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                transition={{ delay: 0.5 }}
                className="remember-me"
              >
                <input
                  id="remember"
                  type="checkbox"
                  className="checkbox-input"
                  checked={rememberMe}
                  onChange={(event) => setRememberMe(event.target.checked)}
                />

                <label className="checkbox-label" htmlFor="remember">
                  تذكرني على هذا الجهاز
                </label>
              </Motion.div>

              <Motion.button
                whileHover={!isSubmitting ? { scale: 1.01 } : undefined}
                whileTap={!isSubmitting ? { scale: 0.99 } : undefined}
                type="submit"
                className="submit-btn"
                disabled={isSubmitting}
              >
                {isSubmitting ? "جاري تسجيل الدخول..." : "تسجيل الدخول"}
              </Motion.button>

              <div className="divider">
                <div className="divider-line" aria-hidden="true">
                  <div className="divider-line-inner"></div>
                </div>

                <div className="divider-text-wrapper">
                  <span className="divider-text">أو</span>
                </div>
              </div>

              <div
                style={{
                  width: "100%",
                  display: "flex",
                  justifyContent: "center",
                }}
              >
                {isGoogleLoginConfigured ? (
                  <GoogleLogin
                    onSuccess={handleGoogleSuccess}
                    onError={handleGoogleError}
                    text="continue_with"
                    shape="rectangular"
                    size="large"
                    width="100%"
                    locale="ar"
                    useOneTap={false}
                  />
                ) : (
                  <button
                    type="button"
                    className="google-btn"
                    onClick={() =>
                      setGeneralError("تسجيل الدخول بجوجل غير مفعّل حالياً. تحقق من إعداد Google Client ID.")
                    }
                  >
                    المتابعة باستخدام Google
                  </button>
                )}
              </div>

              {isGoogleSubmitting && (
                <p style={{ ...errorTextStyle, color: "#64748b" }}>
                  جاري تسجيل الدخول باستخدام جوجل...
                </p>
              )}
            </form>

            <div className="footer-link-section">
              <p className="footer-link-text">
                ليس لديك حساب؟{" "}
                <a href="#" className="footer-link" onClick={handleSignupClick}>
                  إنشاء حساب <ArrowLeft size={14} className="rotate-180" />
                </a>
              </p>
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
                <img
                  src="https://lh3.googleusercontent.com/aida-public/AB6AXuBwC8D_P72GA4RnAo1GAhMCpvJgFFjLlvzI-iJ3k6M7navS_vQ6U7fF2riHebRGiKyEcwUw53ST5QqhHLz4sIKQblZuO0zmVVLqi5elweBk3QgrOpl57JHykU_1wHtvNES-Z0MU45a1SxkEnVcN0TjRLkrix3QYS1F8pzFESYaUi1fH18tl5OXvWGB1n7pHu1S5C6lke59KIerO0UwuE0FZJ5W4POEijdAh4MR01mJpo19aKNY9eJwLUrRRxLlJgsC1ufn_9JNnPB8"
                  alt="AI Robot Tutor"
                  className="robot-image"
                  referrerPolicy="no-referrer"
                />
              </Motion.div>

              <div className="space-y-4">
                <Motion.div
                  initial={{ opacity: 0, y: 10 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: 0.6 }}
                  className="badge"
                >
                  <Sparkles size={14} />
                  مدعوم بالذكاء الاصطناعي
                </Motion.div>

                <Motion.h3
                  initial={{ opacity: 0, y: 10 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: 0.7 }}
                  className="visual-title"
                >
                  ارفع من مستوى دراستك
                </Motion.h3>

                <Motion.p
                  initial={{ opacity: 0, y: 10 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: 0.8 }}
                  className="visual-description"
                >
                  انضم إلى آلاف الطلاب الذين يستخدمون منصتنا للوصول إلى أفضل الدروس المخصصة والتدريبات الذكية.
                </Motion.p>
              </div>
            </div>
          </div>
        </Motion.div>
      </main>
    </div>
  );
}
