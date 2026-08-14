import { useEffect, useState } from "react";
import { NavLink, useNavigate } from "react-router-dom";
import {
  LayoutDashboard,
  BookOpen,
  FileText,
  CalendarDays,
  BarChart3,
  Trophy,
  User,
  Sparkles,
  Menu,
  X,
  LogOut,
  CircleHelp,
  ChevronLeft,
  Mail,
} from "lucide-react";
import logo from "../assets/EDU.svg";

const studentSidebarItems = [
  { icon: LayoutDashboard, label: "لوحة التحكم", path: "/dashboard" },
  { icon: BookOpen, label: "المواد", path: "/subjects" },
  { icon: FileText, label: "الامتحانات", path: "/exams" },
  { icon: CalendarDays, label: "خطط الدراسة", path: "/plans" },
  { icon: BarChart3, label: "تحليل الأداء", path: "/analytics" },
  { icon: Trophy, label: "الإنجازات", path: "/achievements" },
  { icon: User, label: "الملف الشخصي", path: "/profile" },
];


const adminSidebarItems = [
  { icon: LayoutDashboard, label: "لوحة التحكم", path: "/admin-dashboard" },
  { icon: BookOpen, label: "إدارة المواد", path: "/admin-subjects" },
  { icon: FileText, label: "إدارة الدروس", path: "/admin-lessons" },
  { icon: CircleHelp, label: "إدارة الامتحانات", path: "/admin-exams" },
  { icon: User, label: "إدارة المستخدمين", path: "/admin-users" },
  { icon: Mail, label: "رسائل التواصل", path: "/admin-messages" },

  { icon: BarChart3, label: "تحليلات النظام", path: "/admin-analytics" },
  { icon: Trophy, label: "إنجازات المستخدمين", path: "/admin-achievements" },
  { icon: User, label: "الملف الشخصي", path: "/admin-profile" },
];

const sidebarTips = [
  {
    title: "نصيحة اليوم",
    description: "خصص ٣٠ دقيقة يومياً للمراجعة السريعة لتحسين أدائك!",
  },
  {
    title: "ابدأ بالأصعب",
    description: "ابدأ بالمادة التي تحتاج تركيزاً أكبر ثم انتقل للمهام الأسهل.",
  },
  {
    title: "اختبر نفسك",
    description: "حل سؤالين بعد كل درس يساعدك تعرف إذا الفكرة ثبتت فعلاً.",
  },
  {
    title: "راجع بذكاء",
    description: "ارجع للنقاط التي أخطأت بها أولاً بدل إعادة كل الدرس من البداية.",
  },
  {
    title: "خطوة صغيرة",
    description: "إنجاز درس واحد بتركيز أفضل من تأجيل خطة كبيرة لآخر اليوم.",
  },
];

const UNREAD_COUNT_KEY = "edunext_admin_unread_messages_count";

const DashboardLayout = ({ children, title, subtitle, hideSearch, fullName, titleIcon: TitleIcon, headerContent }) => {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [sidebarCollapsed, setSidebarCollapsed] = useState(() => {
    const saved = localStorage.getItem("sidebarCollapsed");
    return saved ? JSON.parse(saved) : false;
  });
  const [tipIndex, setTipIndex] = useState(0);
  const [unreadMessagesCount, setUnreadMessagesCount] = useState(() => {
    return Number(localStorage.getItem(UNREAD_COUNT_KEY) || 0);
  });
  
  const navigate = useNavigate();

  useEffect(() => {
    const handleUnreadUpdate = (e) => {
      setUnreadMessagesCount(e.detail ?? Number(localStorage.getItem(UNREAD_COUNT_KEY) || 0));
    };
    window.addEventListener("unreadMessagesUpdated", handleUnreadUpdate);
    return () => window.removeEventListener("unreadMessagesUpdated", handleUnreadUpdate);
  }, []);

  const handleSidebarCollapse = () => {
    const newState = !sidebarCollapsed;
    setSidebarCollapsed(newState);
    localStorage.setItem("sidebarCollapsed", JSON.stringify(newState));
  };

  const handleLogout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("userId");
    localStorage.removeItem("fullName");
    localStorage.removeItem("role");
    localStorage.removeItem("branch");
    localStorage.removeItem("isOnboardingCompleted");
    localStorage.removeItem("onboarding");
    localStorage.removeItem(UNREAD_COUNT_KEY);

    setSidebarOpen(false);
    navigate("/login");
  };
  const role = localStorage.getItem("role");
  const sidebarItems = role === "admin"
    ? adminSidebarItems
    : studentSidebarItems;
  const profilePath = role === "admin" ? "/admin-profile" : "/profile";
  const activeTip = sidebarTips[tipIndex];

  useEffect(() => {
    const interval = setInterval(() => {
      setTipIndex((currentIndex) => (currentIndex + 1) % sidebarTips.length);
    }, 8000);

    return () => clearInterval(interval);
  }, []);

  return (
    <div
      className={`dashboard-layout ${role === "admin" ? "dashboard-layout-admin" : "dashboard-layout-student"}`}
      dir="rtl"
    >
      {sidebarOpen && <div className="sidebar-overlay" onClick={() => setSidebarOpen(false)} />}

      <aside className={`dashboard-sidebar ${sidebarOpen ? "sidebar-mobile-open" : ""} ${sidebarCollapsed ? "sidebar-collapsed" : ""}`}>
        <div className="sidebar-header">
          <div className="sidebar-header-top">
            <div className="dashboard-logo-brand">
              <img src={logo} alt="EduNext Logo" className="dashboard-logo" />
            </div>
            {!sidebarCollapsed && <span className="dashboard-logo-title">EduNext</span>}
            <button
              className="sidebar-collapse-btn"
              onClick={handleSidebarCollapse}
              title={sidebarCollapsed ? "توسيع القائمة الجانبية" : "تصغير القائمة الجانبية"}
              aria-label={sidebarCollapsed ? "Expand sidebar" : "Collapse sidebar"}
            >
              <ChevronLeft size={20} />
            </button>
          </div>

          <button className="sidebar-close-btn" onClick={() => setSidebarOpen(false)}>
            <X size={20} />
          </button>
        </div>

       <nav className="sidebar-nav">
  {sidebarItems.map((item) => (
    <NavLink
        key={item.label}
  to={item.path}
  end={item.path === "/dashboard" || item.path === "/admin-dashboard"}
  className={({ isActive }) =>
    `sidebar-nav-item ${
      isActive ? "sidebar-nav-item-active" : ""
    }`
      }
      onClick={() => setSidebarOpen(false)}
      title={sidebarCollapsed ? item.label : ""}
    >
      <item.icon size={20} />

      {!sidebarCollapsed && <span>{item.label}</span>}

      {item.path === "/admin-messages" &&
        unreadMessagesCount > 0 && (
          <span className="sidebar-message-badge">
            {unreadMessagesCount}
          </span>
        )}
    </NavLink>
  ))}
</nav>

        <div className="sidebar-footer-section">
          {role !== "admin" && !sidebarCollapsed && (
            <div className="sidebar-footer-card">
              <Sparkles size={20} style={{ color: "var(--primary)" }} />
              <p className="sidebar-footer-title" key={`tip-title-${activeTip.title}`}>{activeTip.title}</p>
              <p className="sidebar-footer-desc" key={`tip-desc-${activeTip.description}`}>
                {activeTip.description}
              </p>
            </div>
          )}

          <button className="sidebar-logout-btn" onClick={handleLogout} type="button" title={sidebarCollapsed ? "تسجيل الخروج" : ""}>
            <LogOut size={18} />
            {!sidebarCollapsed && <span>تسجيل الخروج</span>}
          </button>
        </div>
      </aside>

      <main className="dashboard-main">
        <header className="dashboard-header">
          <div className="dashboard-header-right">
            <button className="sidebar-toggle-btn" onClick={() => setSidebarOpen(true)}>
              <Menu size={35} />
            </button>

            <div>
              <h1 className="dashboard-greeting">
                {TitleIcon && (
                  <span className="dashboard-title-icon" aria-hidden="true">
                    <TitleIcon size={22} />
                  </span>
                )}
                <span>{title}</span>
              </h1>
              {subtitle && <p className="dashboard-subgreeting">{subtitle}</p>}
            </div>
          </div>

          {headerContent && (
            <div className="dashboard-header-content">
              {headerContent}
            </div>
          )}
        </header>

        {children}
      </main>
    </div>
  );
};

export default DashboardLayout;