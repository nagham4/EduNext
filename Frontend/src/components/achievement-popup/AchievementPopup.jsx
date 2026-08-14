import { useEffect } from "react";
import { Award, BookOpen, Flame, Sparkles, Star, Trophy, Users } from "lucide-react";
import "./AchievementPopup.css";

const ICONS = {
  streaks: Flame,
  lessons: BookOpen,
  exams: Trophy,
  points: Star,
  collaboration: Users,
  trophy: Award,
};

const AchievementPopup = ({ open, onClose, achievement }) => {
  useEffect(() => {
    if (!open) return;

    const handleKeyDown = (event) => {
      if (event.key === "Escape") {
        onClose?.();
      }
    };

    window.addEventListener("keydown", handleKeyDown);

    if (typeof navigator !== "undefined" && navigator.vibrate) {
      navigator.vibrate([20, 50, 20]);
    }

    return () => {
      window.removeEventListener("keydown", handleKeyDown);
    };
  }, [open, onClose]);

  if (!open || !achievement) return null;

  const Icon =
    ICONS[achievement.conditionType] ||
    ICONS[achievement.type] ||
    ICONS.trophy;

  return (
    <div className="achievement-popup-overlay" onClick={onClose}>
      <div
        className="achievement-popup-card"
        onClick={(e) => e.stopPropagation()}
      >
        <button
          type="button"
          className="achievement-popup-close"
          onClick={onClose}
          aria-label="إغلاق"
        >
          ×
        </button>

        <div className="achievement-popup-icon-wrapper">
          <div className="achievement-popup-icon">
            <Icon size={42} strokeWidth={1.7} />
          </div>
        </div>

        <div className="achievement-popup-content">
          <p className="achievement-popup-badge">
            <Sparkles size={14} aria-hidden="true" /> تم فتح إنجاز جديد
          </p>

          <h2 className="achievement-popup-title">
            {achievement.title || "إنجاز جديد"}
          </h2>

          <p className="achievement-popup-description">
            {achievement.description || "أحسنت! لقد حققت إنجازًا جديدًا."}
          </p>

          {achievement.reward && (
            <div className="achievement-popup-reward">
              <Star className="achievement-popup-reward-icon" size={16} aria-hidden="true" />
              <span>{achievement.reward}</span>
            </div>
          )}

          <div className="achievement-popup-actions">
            <button
              type="button"
              className="achievement-popup-button"
              onClick={onClose}
            >
              رائع! تابع التقدم
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AchievementPopup;
