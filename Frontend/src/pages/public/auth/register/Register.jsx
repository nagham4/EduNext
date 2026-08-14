/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import React, { useMemo, useState } from "react";
import "./Register.css";
import { API_BASE_URL } from "@/config/api";
import { GOOGLE_CLIENT_ID } from "@/config/google";
import { GoogleLogin } from "@react-oauth/google";
import {
  Star,
  HelpCircle,
  Users,
  User,
  Mail,
  Lock,
  CheckCircle,
  ArrowLeft,
  Eye,
  EyeOff,
  Home,
  Trophy,
} from "lucide-react";
import { useNavigate } from "react-router-dom";
import { motion as Motion } from "framer-motion";
import PublicNavbar from "../../../../components/public-navbar/PublicNavbar.jsx";

export default function Register() {
  const navigate = useNavigate();

  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [acceptedTerms, setAcceptedTerms] = useState(false);
  const [legalModal, setLegalModal] = useState(null); // 'terms' | 'privacy' | null

  const [fieldErrors, setFieldErrors] = useState({});
  const [generalError, setGeneralError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isGoogleSubmitting, setIsGoogleSubmitting] = useState(false);

  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [isPasswordAutoSuggested, setIsPasswordAutoSuggested] = useState(false);
  const isGoogleLoginConfigured = Boolean(GOOGLE_CLIENT_ID);

  const handleLoginClick = (event) => {
    event.preventDefault();
    navigate("/login");
  };

  function getSecureRandomIndex(max) {
    const randomValues = new Uint32Array(1);
    window.crypto.getRandomValues(randomValues);
    return randomValues[0] % max;
  }

  function getSecureRandomCharacter(characters) {
    return characters[getSecureRandomIndex(characters.length)];
  }

  function shuffleSecurely(characters) {
    const shuffledCharacters = [...characters];

    for (let index = shuffledCharacters.length - 1; index > 0; index--) {
      const randomIndex = getSecureRandomIndex(index + 1);
      const currentValue = shuffledCharacters[index];

      shuffledCharacters[index] = shuffledCharacters[randomIndex];
      shuffledCharacters[randomIndex] = currentValue;
    }

    return shuffledCharacters.join("");
  }

  function generateSecurePassword(length = 16) {
    const uppercaseLetters = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    const lowercaseLetters = "abcdefghijkmnopqrstuvwxyz";
    const numbers = "23456789";
    const symbols = "!@#$%&*?";
    const allCharacters = uppercaseLetters + lowercaseLetters + numbers + symbols;

    const passwordCharacters = [
      getSecureRandomCharacter(uppercaseLetters),
      getSecureRandomCharacter(lowercaseLetters),
      getSecureRandomCharacter(numbers),
      getSecureRandomCharacter(symbols),
    ];

    while (passwordCharacters.length < length) {
      passwordCharacters.push(getSecureRandomCharacter(allCharacters));
    }

    return shuffleSecurely(passwordCharacters);
  }

  const suggestPasswordInsideInput = () => {
    if (password) {
      return;
    }

    const generatedPassword = generateSecurePassword();

    setPassword(generatedPassword);
    setConfirmPassword("");
    setShowPassword(true);
    setIsPasswordAutoSuggested(true);

    setFieldErrors((previousErrors) => ({
      ...previousErrors,
      password: [],
      confirmPassword: [],
    }));
  };

  const changeSuggestedPassword = () => {
    const generatedPassword = generateSecurePassword();

    setPassword(generatedPassword);
    setConfirmPassword("");
    setShowPassword(true);
    setIsPasswordAutoSuggested(true);

    setFieldErrors((previousErrors) => ({
      ...previousErrors,
      password: [],
      confirmPassword: [],
    }));
  };

  const clearSuggestedPassword = () => {
    setPassword("");
    setConfirmPassword("");
    setIsPasswordAutoSuggested(false);
  };

  const errorTextStyle = {
    color: "#dc2626",
    fontSize: "13px",
    fontWeight: 600,
    lineHeight: 1.5,
    marginTop: "5px",
    marginBottom: "0",
  };

  const helperTextStyle = {
    fontSize: "12px",
    fontWeight: 600,
    lineHeight: 1.5,
    marginTop: "6px",
    marginBottom: "0",
    color: "#64748b",
  };

  const suggestionHelperStyle = {
    fontSize: "12px",
    fontWeight: 600,
    lineHeight: 1.5,
    marginTop: "6px",
    marginBottom: "0",
    color: "#64748b",
  };

  const smallButtonStyle = {
    background: "none",
    border: "none",
    padding: 0,
    margin: "0 4px",
    color: "#105bf0",
    fontSize: "12px",
    fontWeight: 700,
    cursor: "pointer",
    fontFamily: "inherit",
  };

  const visibilityButtonStyle = {
    position: "absolute",
    left: "12px",
    top: "50%",
    transform: "translateY(-50%)",
    background: "none",
    border: "none",
    cursor: "pointer",
    color: "#64748b",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    padding: 0,
    zIndex: 2,
  };

  const formSectionFixedStyle = {
    justifyContent: "flex-start",
    overflowY: "auto",
  };

  const formHeaderFixedStyle = {
    marginBottom: "1.3rem",
    flexShrink: 0,
  };

  const clearFieldError = (fieldName) => {
    setFieldErrors((previousErrors) => ({
      ...previousErrors,
      [fieldName]: [],
    }));
  };

  const clearAllErrors = () => {
    setFieldErrors({});
    setGeneralError("");
  };

  const addFieldError = (errors, fieldName, message) => {
    if (!errors[fieldName]) {
      errors[fieldName] = [];
    }

    errors[fieldName].push(message);
  };

  const getFieldErrors = (fieldName) => {
    const errors = fieldErrors[fieldName];

    if (!errors) {
      return [];
    }

    return Array.isArray(errors) ? errors.filter(Boolean) : [errors];
  };

  const passwordStrength = useMemo(() => {
    if (!password) {
      return {
        label: "",
        hint: "",
        color: "#64748b",
      };
    }

    let score = 0;

    if (password.length >= 8) score += 1;
    if (/[A-Z]/.test(password)) score += 1;
    if (/[a-z]/.test(password)) score += 1;
    if (/\d/.test(password)) score += 1;
    if (/[!@#$%^&*(),.?":{}|<>_\-+=~`]/.test(password)) score += 1;

    if (score <= 2) {
      return {
        label: "ضعيفة",
        hint: "استخدم 8 أحرف على الأقل مع رقم ورمز.",
        color: "#dc2626",
      };
    }

    if (score <= 4) {
      return {
        label: "متوسطة",
        hint: "لزيادة القوة، أضف حرفًا كبيرًا ورمزًا خاصًا.",
        color: "#d97706",
      };
    }

    return {
      label: "قوية",
      hint: "كلمة المرور قوية.",
      color: "#16a34a",
    };
  }, [password]);

  const validateForm = () => {
    const errors = {};
    const trimmedName = name.trim();
    const trimmedEmail = email.trim();

    if (!trimmedName) {
      addFieldError(errors, "fullName", "الاسم الكامل مطلوب.");
    } else if (trimmedName.length < 10) {
      addFieldError(errors, "fullName", "الاسم الكامل يجب أن يكون 10 أحرف على الأقل.");
    }

    if (!trimmedEmail) {
      addFieldError(errors, "email", "البريد الإلكتروني مطلوب.");
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(trimmedEmail)) {
      addFieldError(errors, "email", "صيغة البريد الإلكتروني غير صحيحة.");
    }

    if (!password) {
      addFieldError(errors, "password", "كلمة المرور مطلوبة.");
    } else {
      if (password.length < 8) {
        addFieldError(errors, "password", "كلمة المرور يجب أن تحتوي على 8 أحرف على الأقل.");
      }

      if (!/[A-Z]/.test(password)) {
        addFieldError(errors, "password", "يجب أن تحتوي كلمة المرور على حرف كبير واحد على الأقل.");
      }

      if (!/[a-z]/.test(password)) {
        addFieldError(errors, "password", "يجب أن تحتوي كلمة المرور على حرف صغير واحد على الأقل.");
      }

      if (!/\d/.test(password)) {
        addFieldError(errors, "password", "يجب أن تحتوي كلمة المرور على رقم واحد على الأقل.");
      }

      if (!/[!@#$%^&*(),.?":{}|<>_\-+=~`]/.test(password)) {
        addFieldError(errors, "password", "يجب أن تحتوي كلمة المرور على رمز خاص مثل ! أو @ أو #.");
      }

      if (password.includes(" ")) {
        addFieldError(errors, "password", "كلمة المرور لا يجب أن تحتوي على مسافات.");
      }
    }

    if (!confirmPassword) {
      addFieldError(errors, "confirmPassword", "تأكيد كلمة المرور مطلوب.");
    } else if (password !== confirmPassword) {
      addFieldError(errors, "confirmPassword", "كلمتا المرور غير متطابقتين.");
    }

    if (!acceptedTerms) {
      addFieldError(errors, "acceptedTerms", "يجب الموافقة على الشروط والأحكام لإنشاء الحساب.");
    }

    return errors;
  };

  async function register() {
    clearAllErrors();

    const validationErrors = validateForm();

    if (Object.keys(validationErrors).length > 0) {
      setFieldErrors(validationErrors);
      return;
    }

    setIsSubmitting(true);

    try {
      const response = await fetch(`${API_BASE_URL}/api/auth/register`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          fullName: name.trim(),
          email: email.trim(),
          password,
          confirmPassword,
          acceptedTerms,
        }),
      });

      const data = await response.json().catch(() => ({}));

      if (!response.ok) {
        setFieldErrors(data.errors || {});
        return;
      }

      localStorage.setItem("token", data.token);
      localStorage.setItem("userId", data.userId);
      localStorage.setItem("fullName", data.fullName);
      localStorage.setItem("role", data.role);

      navigate("/onboarding/1");
    } catch {
      setFieldErrors({
        email: ["حدث خطأ في الاتصال بالسيرفر. تأكد من تشغيل الباك ثم حاول مرة أخرى."],
      });
    } finally {
      setIsSubmitting(false);
    }
  }

  const navigateAfterGoogleAuth = (data) => {
    const token = data.token || data.Token || "";
    const userId = data.userId || data.UserId || "";
    const fullName = data.fullName || data.FullName || "";
    const role = data.role || data.Role || "student";
    const branch = data.branch || data.Branch || "";
    const onboardingCompleted =
      data.isOnboardingCompleted ??
      data.IsOnboardingCompleted ??
      data.onboardingCompleted ??
      data.onboarding_completed ??
      false;

    const normalizedRole = String(role).toLowerCase().trim();

    localStorage.setItem("token", token);
    localStorage.setItem("userId", String(userId));
    localStorage.setItem("fullName", fullName || "");
    localStorage.setItem("role", normalizedRole);
    localStorage.setItem("branch", branch || "");
    localStorage.setItem("isOnboardingCompleted", String(onboardingCompleted));

    if (normalizedRole === "admin") {
      navigate("/admin-dashboard", { replace: true });
      return;
    }

    if (normalizedRole === "student" && onboardingCompleted) {
      navigate("/dashboard", { replace: true });
      return;
    }

    navigate("/onboarding/1", { replace: true });
  };

  const handleGoogleSuccess = async (credentialResponse) => {
    clearAllErrors();

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
        setGeneralError(data.message || "تعذر إنشاء الحساب باستخدام جوجل.");
        setFieldErrors(data.errors || {});
        return;
      }

      navigateAfterGoogleAuth(data);
    } catch {
      setGeneralError("حدث خطأ أثناء التسجيل باستخدام جوجل. تأكد من تشغيل الباك ثم حاول مرة أخرى.");
    } finally {
      setIsGoogleSubmitting(false);
    }
  };

  const handleGoogleError = () => {
    setGeneralError("تعذر التسجيل باستخدام جوجل. يرجى اختيار الحساب والمحاولة مرة أخرى.");
    setIsGoogleSubmitting(false);
  };

  return (
    <div className="layout-wrapper" dir="rtl">
      <PublicNavbar compact />
      <button type="button" className="auth-home-link" onClick={() => navigate("/")}>
        <Home size={18} />
        <span>العودة للرئيسية</span>
      </button>
      <div className="main-content">
        <Motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6 }}
          className="cardd"
        >
          <div className="side-branding rtl">
            <div className="branding-content">
              <Motion.span
                initial={{ opacity: 0, x: 20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: 0.3 }}
                className="badge"
              >
                منصة التوجيهي الأولى
              </Motion.span>

              <Motion.h2
                initial={{ opacity: 0, x: 20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: 0.4 }}
                className="sidebar-title"
              >
                انطلق نحو مستقبلك مع EduNext
              </Motion.h2>

              <Motion.p
                initial={{ opacity: 0, x: 20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: 0.5 }}
                className="side-desc"
              >
                انضم لآلاف الطلاب الفلسطينيين المتميزين واصل إلى أفضل مصادر التعلم التفاعلية المصممة خصيصاً لمنهاج التوجيهي.
              </Motion.p>

              <div className="features-list">
                {[
                  { icon: Star, text: "شروحات شاملة لكافة الفروع" },
                  { icon: HelpCircle, text: "اختبارات تجريبية محاكية للوزاري" },
                  { icon: Trophy, text: "تتبع مستوى الطالب وإنجازاته" },
                ].map((item, index) => (
                  <Motion.div
                    key={index}
                    initial={{ opacity: 0, x: 20 }}
                    animate={{ opacity: 1, x: 0 }}
                    transition={{ delay: 0.6 + index * 0.1 }}
                    className="feature-item"
                  >
                    <div className="material-symbols-outlined feature-icon">
                      <item.icon size={20} />
                    </div>
                    <span className="feature-text">{item.text}</span>
                  </Motion.div>
                ))}
              </div>
            </div>

            <div className="students-social">
              <div className="avatars">
                <div className="avatar-more">+500</div>
                {[3, 2, 1].map((i) => (
                  <img
                    key={i}
                    className="avatar"
                    src={`https://picsum.photos/seed/student${i}/100/100`}
                    alt={`Student ${i}`}
                    referrerPolicy="no-referrer"
                  />
                ))}
              </div>
              <p className="social-text">انضم لزملائك الطلاب اليوم</p>
            </div>
          </div>

          <div className="form-section" style={formSectionFixedStyle}>
            <Motion.div
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ delay: 0.2, duration: 0.5 }}
              className="form-header"
              style={formHeaderFixedStyle}
            >
              <h2 className="form-title">إنشاء حساب جديد</h2>
              <p className="form-subtitle">ابدأ رحلتك التعليمية المتميزة اليوم</p>
            </Motion.div>

            <form
              className="form"
              onSubmit={(event) => {
                event.preventDefault();
                register();
              }}
              noValidate
            >
              {generalError && <p style={errorTextStyle}>{generalError}</p>}

              <Motion.div
                initial={{ opacity: 0, x: 20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: 0.3, duration: 0.5 }}
                className="form-group"
              >
                <label className="label">الاسم الكامل</label>
                <div className="input-wrapper">
                  <User className="input-icon" size={20} />
                  <input
                    className="input"
                    id="name"
                    placeholder="أدخل اسمك الرباعي"
                    type="text"
                    value={name}
                    onChange={(event) => {
                      setName(event.target.value);
                      clearFieldError("fullName");
                    }}
                  />
                </div>

                {getFieldErrors("fullName").map((message) => (
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
                <label className="label">البريد الإلكتروني</label>
                <div className="input-wrapper">
                  <Mail className="input-icon" size={20} />
                  <input
                    className="input text-right ltr"
                    placeholder="example@email.com"
                    type="email"
                    value={email}
                    onChange={(event) => {
                      setEmail(event.target.value);
                      clearFieldError("email");
                    }}
                  />
                </div>

                {getFieldErrors("email").map((message) => (
                  <p style={errorTextStyle} key={message}>
                    {message}
                  </p>
                ))}
              </Motion.div>

              <Motion.div
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                transition={{ delay: 0.5 }}
                className="password-grid"
              >
                <div className="form-group">
                  <label className="label">كلمة المرور</label>
                  <div className="input-wrapper">
                    <Lock className="input-icon" size={20} />
                    <input
                      className="input text-right ltr"
                      style={{ paddingLeft: "44px" }}
                      placeholder="••••••••"
                      type={showPassword ? "text" : "password"}
                      value={password}
                      onFocus={suggestPasswordInsideInput}
                      onChange={(event) => {
                        setPassword(event.target.value);
                        setIsPasswordAutoSuggested(false);
                        clearFieldError("password");
                      }}
                      autoComplete="new-password"
                    />

                    <button
                      type="button"
                      style={visibilityButtonStyle}
                      onMouseDown={(event) => {
                        event.preventDefault();
                        setShowPassword((previousValue) => !previousValue);
                      }}
                      aria-label={showPassword ? "إخفاء كلمة المرور" : "إظهار كلمة المرور"}
                    >
                      {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                    </button>
                  </div>

                  {isPasswordAutoSuggested && password && (
                    <p style={suggestionHelperStyle}>
                      تم اقتراح كلمة مرور قوية.
                      <button
                        type="button"
                        style={smallButtonStyle}
                        onClick={changeSuggestedPassword}
                      >
                        تغيير الاقتراح
                      </button>
                      <button
                        type="button"
                        style={smallButtonStyle}
                        onClick={clearSuggestedPassword}
                      >
                        مسح
                      </button>
                    </p>
                  )}

                  {password && (
                    <p
                      style={{
                        ...helperTextStyle,
                        color: passwordStrength.color,
                      }}
                    >
                      قوة كلمة المرور: {passwordStrength.label} — {passwordStrength.hint}
                    </p>
                  )}

                  {getFieldErrors("password")
                    .slice(0, 1)
                    .map((message) => (
                      <p style={errorTextStyle} key={message}>
                        {message}
                      </p>
                    ))}
                </div>

                <div className="form-group">
                  <label className="label">تأكيد كلمة المرور</label>
                  <div className="input-wrapper">
                    <CheckCircle className="input-icon" size={20} />
                    <input
                      className="input text-right ltr"
                      style={{ paddingLeft: "44px" }}
                      placeholder="••••••••"
                      type={showConfirmPassword ? "text" : "password"}
                      value={confirmPassword}
                      onChange={(event) => {
                        setConfirmPassword(event.target.value);
                        clearFieldError("confirmPassword");
                      }}
                      autoComplete="new-password"
                    />

                    <button
                      type="button"
                      style={visibilityButtonStyle}
                      onMouseDown={(event) => {
                        event.preventDefault();
                        setShowConfirmPassword((previousValue) => !previousValue);
                      }}
                      aria-label={
                        showConfirmPassword ? "إخفاء تأكيد كلمة المرور" : "إظهار تأكيد كلمة المرور"
                      }
                    >
                      {showConfirmPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                    </button>
                  </div>

                  {getFieldErrors("confirmPassword").map((message) => (
                    <p style={errorTextStyle} key={message}>
                      {message}
                    </p>
                  ))}
                </div>
              </Motion.div>

              <Motion.div className="checkbox-group">
                <input
                  className="checkbox"
                  id="terms"
                  type="checkbox"
                  checked={acceptedTerms}
                  onChange={(event) => {
                    setAcceptedTerms(event.target.checked);
                    clearFieldError("acceptedTerms");
                  }}
                />
                <label className="checkbox-label" htmlFor="terms">
                  أوافق على{" "}
                  <button type="button" className="link link-btn" onClick={() => setLegalModal('terms')}>شروط الاستخدام</button>
                  {" "}و{" "}
                  <button type="button" className="link link-btn" onClick={() => setLegalModal('privacy')}>سياسة الخصوصية</button>
                  {" "}الخاصة بـ EduNext.
                </label>
              </Motion.div>

              {getFieldErrors("acceptedTerms").map((message) => (
                <p style={errorTextStyle} key={message}>
                  {message}
                </p>
              ))}

              <Motion.button
                whileHover={!isSubmitting ? { scale: 1.01 } : undefined}
                whileTap={!isSubmitting ? { scale: 0.98 } : undefined}
                className="submit-button"
                type="submit"
                disabled={isSubmitting}
              >
                <span>{isSubmitting ? "جاري إنشاء الحساب..." : "إنشاء حساب"}</span>
                <ArrowLeft size={20} />
              </Motion.button>

              <div className="auth-divider">
                <span>أو</span>
              </div>

              <div className="google-register-wrapper">
                {isGoogleLoginConfigured ? (
                  <GoogleLogin
                    onSuccess={handleGoogleSuccess}
                    onError={handleGoogleError}
                    text="signup_with"
                    shape="rectangular"
                    size="large"
                    width="100%"
                    locale="ar"
                    useOneTap={false}
                  />
                ) : (
                  <button
                    type="button"
                    className="google-register-btn"
                    onClick={() =>
                      setGeneralError("التسجيل بجوجل غير مفعل حالياً. تحقق من إعداد Google Client ID.")
                    }
                  >
                    التسجيل باستخدام Google
                  </button>
                )}
              </div>

              {isGoogleSubmitting && (
                <p style={{ ...errorTextStyle, color: "#64748b" }}>
                  جاري التسجيل باستخدام جوجل...
                </p>
              )}
            </form>

            <div className="form-footer">
              <p className="form-footer-text">
                لديك حساب بالفعل؟
                <a className="form-footer-link" href="#" onClick={handleLoginClick}>
                  تسجيل الدخول
                </a>
              </p>
            </div>
          </div>
        </Motion.div>
      </div>
      {legalModal && (
        <div className="legal-overlay" onClick={() => setLegalModal(null)}>
          <div className="legal-modal" onClick={(e) => e.stopPropagation()} role="dialog" aria-modal="true">
            <div className="legal-modal-header">
              <h2>{legalModal === 'terms' ? 'شروط الاستخدام' : 'سياسة الخصوصية'}</h2>
              <button type="button" className="legal-close" onClick={() => setLegalModal(null)} aria-label="إغلاق">✕</button>
            </div>
            <div className="legal-modal-body">
              {legalModal === 'terms' ? (
                <>
                  <p className="legal-date">آخر تحديث: يونيو 2026</p>

                  <h3>1. قبول الشروط</h3>
                  <p>باستخدامك لمنصة EduNext، فإنك توافق على الالتزام بهذه الشروط والأحكام. إذا كنت لا توافق على أي جزء منها، يُرجى عدم استخدام المنصة.</p>

                  <h3>2. وصف الخدمة</h3>
                  <p>EduNext منصة تعليمية إلكترونية موجهة لطلاب التوجيهي في فلسطين، تقدم خططاً دراسية ذكية، ومساعداً تعليمياً، واختبارات تجريبية، وتحليلات أداء.</p>

                  <h3>3. حساب المستخدم</h3>
                  <p>أنت مسؤول عن الحفاظ على سرية بيانات حسابك وعدم مشاركتها مع أي طرف آخر. يجب أن تكون المعلومات المُدخلة صحيحة ودقيقة.</p>

                  <h3>4. الاستخدام المقبول</h3>
                  <p>يُحظر استخدام المنصة لأغراض غير قانونية، أو نشر محتوى مسيء، أو محاولة اختراق أنظمة المنصة أو انتهاك خصوصية المستخدمين الآخرين.</p>

                  <h3>5. الملكية الفكرية</h3>
                  <p>جميع المحتويات المنشورة على EduNext — من نصوص وصور وأدوات — هي ملك حصري للمنصة ولا يجوز نسخها أو إعادة توزيعها دون إذن مسبق.</p>

                  <h3>6. إنهاء الحساب</h3>
                  <p>تحتفظ EduNext بحق تعليق أو إنهاء أي حساب يخالف هذه الشروط، دون الحاجة إلى إشعار مسبق.</p>

                  <h3>7. تحديث الشروط</h3>
                  <p>قد نعدّل هذه الشروط في أي وقت. سيتم إشعارك بأي تغييرات جوهرية عبر البريد الإلكتروني أو إشعار داخل المنصة.</p>

                  <h3>8. التواصل</h3>
                  <p>لأي استفسار يتعلق بهذه الشروط، تواصل معنا عبر: <strong>edunext.contact@gmail.com</strong></p>
                </>
              ) : (
                <>
                  <p className="legal-date">آخر تحديث: يونيو 2026</p>

                  <h3>1. المعلومات التي نجمعها</h3>
                  <p>نجمع المعلومات التي تقدمها عند التسجيل (الاسم، البريد الإلكتروني)، وبيانات الاستخدام داخل المنصة كالتقدم الدراسي ونتائج الاختبارات.</p>

                  <h3>2. كيف نستخدم معلوماتك</h3>
                  <p>نستخدم بياناتك لتخصيص تجربتك التعليمية، وتحسين الخطط الدراسية، وإرسال توصيات ذكية مبنية على أدائك الفعلي داخل المنصة.</p>

                  <h3>3. حماية البيانات</h3>
                  <p>نحمي بياناتك باستخدام تشفير SSL وإجراءات أمنية محدّثة. لا نبيع بياناتك أو نشاركها مع أطراف خارجية لأغراض تجارية.</p>

                  <h3>4. ملفات تعريف الارتباط (Cookies)</h3>
                  <p>نستخدم الكوكيز لتحسين تجربة التصفح والحفاظ على جلسة تسجيل الدخول. يمكنك تعطيلها من إعدادات متصفحك، لكن قد يؤثر ذلك على بعض ميزات المنصة.</p>

                  <h3>5. حقوقك</h3>
                  <p>يحق لك في أي وقت طلب الاطلاع على بياناتك، تعديلها، أو حذف حسابك بالكامل. تواصل معنا لتنفيذ أي من هذه الطلبات.</p>

                  <h3>6. التواصل</h3>
                  <p>لأي استفسار يتعلق بخصوصيتك، تواصل معنا عبر: <strong>edunext.contact@gmail.com</strong></p>
                </>
              )}
            </div>
            <div className="legal-modal-footer">
              <button type="button" className="legal-accept-btn" onClick={() => { setAcceptedTerms(true); setLegalModal(null); }}>
                فهمت وأوافق
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}