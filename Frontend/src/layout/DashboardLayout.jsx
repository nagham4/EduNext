
// import { useEffect, useState } from "react";
// import { Link, useLocation, useNavigate } from "react-router-dom";
// import {
//   LayoutDashboard,
//   BookOpen,
//   FileText,
//   CalendarDays,
//   BarChart3,
//   Trophy,
//   User,
//   Sparkles,
//   Menu,
//   X,
//   LogOut,
//   CircleHelp,
//   Mail,
// } from "lucide-react";
// import logo from "../assets/EDU.svg";

// const studentSidebarItems = [
//   { icon: LayoutDashboard, label: "لوحة التحكم", path: "/dashboard" },
//   { icon: BookOpen, label: "المواد", path: "/subjects" },
//   { icon: FileText, label: "الامتحانات", path: "/exams" },
//   { icon: CalendarDays, label: "خطط الدراسة", path: "/plans" },
//   { icon: BarChart3, label: "تحليل الأداء", path: "/analytics" },
//   { icon: Trophy, label: "الإنجازات", path: "/achievements" },
//   { icon: User, label: "الملف الشخصي", path: "/profile" },
// ];


// const adminSidebarItems = [
//   { icon: LayoutDashboard, label: "لوحة التحكم", path: "/admin-dashboard" },
//   { icon: BookOpen, label: "إدارة المواد", path: "/admin-subjects" },
//   {icon:FileText, label: "إدارة الدروس", path: "/admin-lessons"},
//   { icon: CircleHelp, label: "إدارة الامتحانات", path: "/admin-exams" },
//   { icon: User, label: "إدارة المستخدمين", path: "/admin-users" },
//   { icon: Mail, label: "رسائل التواصل", path: "/admin-messages" },
//   { icon: BarChart3, label: "تحليلات النظام", path: "/admin-analytics" },
//   { icon: Trophy, label: "إنجازات المستخدمين", path: "/admin-achievements" },
//   { icon: User, label: "الملف الشخصي", path: "/admin-profile" },
// ];

// const sidebarTips = [
//   {
//     title: "نصيحة اليوم",
//     description: "خصص ٣٠ دقيقة يومياً للمراجعة السريعة لتحسين أدائك!",
//   },
//   {
//     title: "ابدأ بالأصعب",
//     description: "ابدأ بالمادة التي تحتاج تركيزاً أكبر ثم انتقل للمهام الأسهل.",
//   },
//   {
//     title: "اختبر نفسك",
//     description: "حل سؤالين بعد كل درس يساعدك تعرف إذا الفكرة ثبتت فعلاً.",
//   },
//   {
//     title: "راجع بذكاء",
//     description: "ارجع للنقاط التي أخطأت بها أولاً بدل إعادة كل الدرس من البداية.",
//   },
//   {
//     title: "خطوة صغيرة",
//     description: "إنجاز درس واحد بتركيز أفضل من تأجيل خطة كبيرة لآخر اليوم.",
//   },
// ];

// const DashboardLayout = ({ children, title, subtitle, hideSearch, fullName, titleIcon: TitleIcon }) => {
//   const [sidebarOpen, setSidebarOpen] = useState(false);
//   const [tipIndex, setTipIndex] = useState(0);
//   const location = useLocation();
//   const navigate = useNavigate();
//   const handleLogout = () => {
//     localStorage.removeItem("token");
//     localStorage.removeItem("userId");
//     localStorage.removeItem("fullName");
//     localStorage.removeItem("role");
//     localStorage.removeItem("branch");
//     localStorage.removeItem("isOnboardingCompleted");
//     localStorage.removeItem("onboarding");

//     setSidebarOpen(false);
//     navigate("/login");
//   };
//   const role = localStorage.getItem("role");
//   const sidebarItems = role === "admin" 
//   ? adminSidebarItems 
//   : studentSidebarItems;
//   const profilePath = role === "admin" ? "/admin-profile" : "/profile";
//   const activeTip = sidebarTips[tipIndex];

//   useEffect(() => {
//     const interval = setInterval(() => {
//       setTipIndex((currentIndex) => (currentIndex + 1) % sidebarTips.length);
//     }, 8000);

//     return () => clearInterval(interval);
//   }, []);

//   return (
//     <div
//       className={`dashboard-layout ${role === "admin" ? "dashboard-layout-admin" : "dashboard-layout-student"}`}
//       dir="rtl"
//     >
//       {sidebarOpen && <div className="sidebar-overlay" onClick={() => setSidebarOpen(false)} />}

//       <aside className={`dashboard-sidebar ${sidebarOpen ? "sidebar-mobile-open" : ""}`}>
//         <div className="sidebar-header">
//             <div className="dashboard-logo-brand">
//               <img className="dashboard-logo-image" src={logo} alt="EduNext" />
//             </div>
//             <span className="dashboard-logo-title">EduNext</span>

//           <button className="sidebar-close-btn" onClick={() => setSidebarOpen(false)}>
//             <X size={20} />
//           </button>
//         </div>

//         <nav className="sidebar-nav">
//           {sidebarItems.map((item) => (
//             <Link
//               key={item.label}
//               to={item.path}
//               className={`sidebar-nav-item ${
//                 location.pathname === item.path ? "sidebar-nav-item-active" : ""
//               }`}
//               onClick={() => setSidebarOpen(false)}
//             >
//               <item.icon size={20} />
//               <span>{item.label}</span>
//             </Link>
//           ))}
//         </nav>

//         <div className="sidebar-footer-section">
//           {role !== "admin" && (
//             <div className="sidebar-footer-card">
//               <Sparkles size={20} style={{ color: "var(--primary)" }} />
//               <p className="sidebar-footer-title" key={`tip-title-${activeTip.title}`}>{activeTip.title}</p>
//               <p className="sidebar-footer-desc" key={`tip-desc-${activeTip.description}`}>
//                 {activeTip.description}
//               </p>
//             </div>
//           )}

//           <button className="sidebar-logout-btn" onClick={handleLogout} type="button">
//             <LogOut size={18} />
//             <span>تسجيل الخروج</span>
//           </button>
//         </div>
//       </aside>

//       <main className="dashboard-main">
//         <header className="dashboard-header">
//           <div className="dashboard-header-right">
//             <button className="sidebar-toggle-btn" onClick={() => setSidebarOpen(true)}>
//               <Menu size={35} />
//             </button>

//             <div>
//               <h1 className="dashboard-greeting">
//                 {TitleIcon && (
//                   <span className="dashboard-title-icon" aria-hidden="true">
//                     <TitleIcon size={22} />
//                   </span>
//                 )}
//                 <span>{title}</span>
//               </h1>
//               {subtitle && <p className="dashboard-subgreeting">{subtitle}</p>}
//             </div>
//           </div>

//         </header>

//         {children}
//       </main>
//     </div>
//   );
// };

// export default DashboardLayout;
