import { useLocation } from "react-router-dom";
import { useEffect } from "react";

const NotFound = () => {
  const location = useLocation();
  useEffect(() => { console.error("404 Error:", location.pathname); }, [location.pathname]);
  return (
    <div className="not-found">
      <div className="not-found-content">
        <h1>404</h1>
        <p>الصفحة غير موجودة</p>
        <a href="/">العودة إلى الصفحة الرئيسية</a>
      </div>
    </div>
  );
};

export default NotFound;
