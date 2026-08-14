import './SidebarAdmin.css';
import { BookOpen, LayoutDashboard, FileText, BarChart3, CircleHelp, Users, Trophy, User } from "lucide-react";
import { NavLink } from "react-router-dom";
import logo from "../../../assets/EDU.svg";

export default function Sidebar() {
  return (
    <aside className="sidebar rtl">
      <div className="logo-section">
        <img className="sidebar-admin-logo-image" src={logo} alt="EduNext" />
        <div className="logo-text">
          <h1>إيديونكست</h1>
          <p>لوحة تحكم المشرف</p>
        </div>
      </div>
      
      <nav className="nav-menu">
        <NavLink to="/admin-dashboard" end className="nav-item">
          <LayoutDashboard size={20} />
          <span>لوحة القيادة</span>
        </NavLink>
        <NavLink to="/admin-subjects" className="nav-item">
          <BookOpen size={20} />
          <span>إدارة المواد</span>
        </NavLink>
        <NavLink to="/admin-lessons" className="nav-item">
          <FileText size={20} />
          <span>إدارة الدروس</span>
        </NavLink>
        <NavLink to="/admin-exams" className="nav-item">
          <CircleHelp size={20} />
          <span>إدارة الامتحانات</span>
        </NavLink>
        <NavLink to="/admin-users" className="nav-item">
          <Users size={20} />
          <span>إدارة المستخدمين</span>
        </NavLink>
        <NavLink to="/admin-analytics" className="nav-item">
          <BarChart3 size={20} />
          <span>تحليلات النظام</span>
        </NavLink>
        <NavLink to="/admin-achievements" className="nav-item">
          <Trophy size={20} />
          <span>إنجازات المستخدمين</span>
        </NavLink>
        <NavLink to="/admin-profile" className="nav-item">
          <User size={20} />
          <span>الملف الشخصي</span>
        </NavLink>
      </nav>
      
        <div className="aa-sidebar-footer">
          <div className="aa-admin-avatar">أ.ع</div>
          <div>
            <div className="aa-admin-name">أحمد علي</div>
            <div className="aa-admin-role">مدير النظام</div>
          </div>
        </div>
    </aside>

  );
}
