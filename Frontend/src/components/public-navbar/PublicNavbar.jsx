import React, { useState } from 'react';
import { Menu, X } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import logo from '../../assets/EDU.svg';
import './PublicNavbar.css';

export default function PublicNavbar({ compact = false }) {
  const navigate = useNavigate();
  const [mobileOpen, setMobileOpen] = useState(false);

  const go = (path) => {
    setMobileOpen(false);
    navigate(path);
  };

  return (
    <header className="edn-header">
      <div className="edn-container edn-nav">
        <button className="edn-brand" type="button" onClick={() => go('/')}>
          <img className="edn-brand-logo" src={logo} alt="EduNext" />
        </button>

        <nav className={`edn-links ${mobileOpen ? 'open' : ''}`}>
          <button type="button" onClick={() => go('/')}>الرئيسية</button>
          {!compact && (
            <>
              <button type="button" onClick={() => go('/#subjects')}>المواد الدراسية</button>
              <button type="button" onClick={() => go('/#faq')}>الأسئلة الشائعة</button>
            </>
          )}
          <button type="button" onClick={() => go('/contact')}>تواصل معنا</button>
        </nav>

        <div className="edn-actions">
          <button className="btn btn-ghost" type="button" onClick={() => go('/login')}>تسجيل دخول</button>
          <button className="btn btn-primary" type="button" onClick={() => go('/register')}>ابدأ مجانا</button>
        </div>

        <button className="menu-btn" type="button" onClick={() => setMobileOpen((value) => !value)} aria-label="فتح القائمة">
          {mobileOpen ? <X size={22} /> : <Menu size={22} />}
        </button>
      </div>
    </header>
  );
}
