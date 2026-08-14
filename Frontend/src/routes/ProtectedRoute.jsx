import { Navigate, Outlet, useLocation } from "react-router-dom";

const getTokenPayload = (token) => {
  try {
    if (!token) return null;

    const payloadBase64 = token.split(".")[1];
    if (!payloadBase64) return null;

    const payloadJson = atob(
      payloadBase64.replace(/-/g, "+").replace(/_/g, "/")
    );

    return JSON.parse(payloadJson);
  } catch {
    return null;
  }
};

const isTokenExpired = (token) => {
  const payload = getTokenPayload(token);

  if (!payload?.exp) {
    return false;
  }

  const currentTimeInSeconds = Math.floor(Date.now() / 1000);

  return payload.exp < currentTimeInSeconds;
};

const clearAuthStorage = () => {
  localStorage.removeItem("token");
  localStorage.removeItem("userId");
  localStorage.removeItem("fullName");
  localStorage.removeItem("role");
  localStorage.removeItem("branch");
  localStorage.removeItem("isOnboardingCompleted");
  localStorage.removeItem("onboarding");
};

const normalizeRole = (role) => {
  return String(role || "").toLowerCase().trim();
};

const isOnboardingCompleted = () => {
  const value = localStorage.getItem("isOnboardingCompleted");

  return value === "true" || value === "True" || value === "1";
};

const getDefaultPathByRole = (role) => {
  if (role === "admin") {
    return "/admin-dashboard";
  }

  if (role === "student") {
    return isOnboardingCompleted() ? "/dashboard" : "/onboarding/1";
  }

  return "/login";
};

export function GuestOnlyRoute() {
  const token = localStorage.getItem("token");
  const role = normalizeRole(localStorage.getItem("role"));

  if (token && !isTokenExpired(token)) {
    return <Navigate to={getDefaultPathByRole(role)} replace />;
  }

  if (token && isTokenExpired(token)) {
    clearAuthStorage();
  }

  return <Outlet />;
}

export function ProtectedRoute({ allowedRoles = [] }) {
  const location = useLocation();

  const token = localStorage.getItem("token");
  const role = normalizeRole(localStorage.getItem("role"));

  if (!token) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  if (isTokenExpired(token)) {
    clearAuthStorage();
    return <Navigate to="/login" replace />;
  }

  if (allowedRoles.length > 0 && !allowedRoles.includes(role)) {
    return <Navigate to={getDefaultPathByRole(role)} replace />;
  }

  return <Outlet />;
}

export function StudentRoute() {
  const location = useLocation();

  const token = localStorage.getItem("token");
  const role = normalizeRole(localStorage.getItem("role"));

  if (!token) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  if (isTokenExpired(token)) {
    clearAuthStorage();
    return <Navigate to="/login" replace />;
  }

  if (role !== "student") {
    return <Navigate to={getDefaultPathByRole(role)} replace />;
  }

  if (!isOnboardingCompleted()) {
    return <Navigate to="/onboarding/1" replace />;
  }

  return <Outlet />;
}

export function OnboardingRoute() {
  const location = useLocation();

  const token = localStorage.getItem("token");
  const role = normalizeRole(localStorage.getItem("role"));

  if (!token) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  if (isTokenExpired(token)) {
    clearAuthStorage();
    return <Navigate to="/login" replace />;
  }

  if (role !== "student") {
    return <Navigate to={getDefaultPathByRole(role)} replace />;
  }

  if (isOnboardingCompleted()) {
    return <Navigate to="/dashboard" replace />;
  }

  return <Outlet />;
}

export function AdminRoute() {
  const location = useLocation();

  const token = localStorage.getItem("token");
  const role = normalizeRole(localStorage.getItem("role"));

  if (!token) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  if (isTokenExpired(token)) {
    clearAuthStorage();
    return <Navigate to="/login" replace />;
  }

  if (role !== "admin") {
    return <Navigate to={getDefaultPathByRole(role)} replace />;
  }

  return <Outlet />;
}