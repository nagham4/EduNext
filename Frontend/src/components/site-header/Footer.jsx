import React from 'react';
import edunextLogo from '../../assets/EDU.svg';
const Footer = () => {
  return (
    <footer className="footer">
      <div className="container footer-content">
        <div className="footer-brand-logo">
          <img src={edunextLogo} alt="EduNext Logo" width={50} height={50}/>
          <h2>EduNext</h2>
        </div>
        
        <div className="footer-links">
          <a href="#">سياسة الخصوصية</a>
          <a href="#">شروط الخدمة</a>
          <a href="#">اتصل بنا</a>
        </div>
        
        <p className="copyright">
        © 2026 EduNext -. جميع الحقوق محفوظة.
        </p>
      </div>
    </footer>
  );
};

export default Footer;
