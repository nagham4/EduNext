import { useEffect, useMemo, useState } from "react";
import { motion } from "framer-motion";
import {
  Trophy,
  Star,
  Zap,
  BookOpen,
  Target,
  Clock,
  Award,
  Flame,
  GraduationCap,
  CheckCircle2,
  Lock,
} from "lucide-react";
import DashboardLayout from "../../../components/DashboardLayout";
import { API_BASE_URL } from "@/config/api";

const achievementStyleMap = {
  "أول درس مكتمل": { icon: BookOpen, color: "green" },
  "سلسلة دراسة 3 أيام": { icon: Flame, color: "amber" },
  "5 دروس مكتملة": { icon: BookOpen, color: "blue" },
  "متفوق الدرجات": { icon: GraduationCap, color: "blue" },
  "سلسلة دراسة 7 أيام": { icon: Flame, color: "amber" },
  "روح الفريق": { icon: Award, color: "green" },
  "كاتب الملاحظات": { icon: Star, color: "purple" },
  "أول امتحان ناجح": { icon: Target, color: "amber" },

  // القديم لو ظل موجود بالداتا
  "القارئ النهم": { icon: BookOpen, color: "green" },
  "نجم الرياضيات": { icon: Star, color: "blue" },
  "المثابر": { icon: Flame, color: "amber" },
  "المتفوق": { icon: GraduationCap, color: "blue" },
  "البرق": { icon: Zap, color: "amber" },
  "الهدّاف": { icon: Target, color: "purple" },
  "الأسطوري": { icon: Trophy, color: "amber" },
  "المنظّم": { icon: Clock, color: "purple" },
  "البطل": { icon: Award, color: "green" },
};

const typeStyleMap = {
  lessons: { icon: BookOpen, color: "green" },
  exams: { icon: Target, color: "amber" },
  streaks: { icon: Flame, color: "amber" },
  points: { icon: Star, color: "purple" },
  collaboration: { icon: Award, color: "green" },
};

const normalizeHub = (data) => {
  const stats = data.stats || data.Stats || {};
  const achievements = data.achievements || data.Achievements || [];

  return {
    stats: {
      points: stats.points ?? stats.Points ?? 0,
      level: stats.level ?? stats.Level ?? 1,
      levelProgressPercent:
        stats.levelProgressPercent ?? stats.LevelProgressPercent ?? 0,
      bestStreakDays: stats.bestStreakDays ?? stats.BestStreakDays ?? 0,
      earnedAchievementsCount:
        stats.earnedAchievementsCount ?? stats.EarnedAchievementsCount ?? 0,
      totalAchievementsCount:
        stats.totalAchievementsCount ?? stats.TotalAchievementsCount ?? 0,
    },
    achievements: achievements.map((a) => ({
      achievementId: a.achievementId || a.AchievementId,
      title: a.title || a.Title || "",
      description: a.description || a.Description || "",

      conditionType: a.conditionType || a.ConditionType || "",
      conditionValue: a.conditionValue ?? a.ConditionValue ?? 0,
      currentValue: a.currentValue ?? a.CurrentValue ?? 0,
      remainingToEarn: a.remainingToEarn ?? a.RemainingToEarn ?? 0,

      earned: a.earned ?? a.Earned ?? false,
      earnedAt: a.earnedAt || a.EarnedAt || null,
      progressPercent: a.progressPercent ?? a.ProgressPercent ?? 0,
    })),
  };
};

const Achievements = () => {
  const token = localStorage.getItem("token");

  const [hub, setHub] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    fetchAchievements();
  }, []);

  const fetchAchievements = async () => {
    if (!token) {
      window.location.href = "/login";
      return;
    }

    try {
      setLoading(true);
      setError("");

      const response = await fetch(
        `${API_BASE_URL}/api/student/achievements?leaderboardSize=10`,
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );

      const text = await response.text();
      let data = {};

      try {
        data = text ? JSON.parse(text) : {};
      } catch {
        data = { message: text };
      }

      if (!response.ok) {
        setError(data.message || "فشل تحميل الإنجازات");
        return;
      }

      setHub(normalizeHub(data));
    } catch (err) {
      console.error(err);
      setError("تعذر الاتصال بالسيرفر");
    } finally {
      setLoading(false);
    }
  };

  const orderedAchievements = useMemo(() => {
    if (!hub) return [];

    return [...hub.achievements].sort((a, b) => {
      if (a.earned !== b.earned) {
        return a.earned ? -1 : 1;
      }

      return (b.progressPercent || 0) - (a.progressPercent || 0);
    });
  }, [hub]);

  const earnedCount = useMemo(() => {
    if (!hub) return 0;

    return hub.stats.earnedAchievementsCount || orderedAchievements.filter((a) => a.earned).length;
  }, [hub, orderedAchievements]);

  const totalCount = useMemo(() => {
    if (!hub) return 0;

    return hub.stats.totalAchievementsCount || orderedAchievements.length;
  }, [hub, orderedAchievements]);

  return (
    <DashboardLayout title="الإنجازات" subtitle="اجمع الشارات وكن الأفضل!" titleIcon={Trophy}>
      {loading ? (
        <div className="card" style={{ padding: "2rem", textAlign: "center" }}>
          جاري تحميل الإنجازات...
        </div>
      ) : error ? (
        <div
          className="card"
          style={{ padding: "2rem", textAlign: "center", color: "red" }}
        >
          {error}
        </div>
      ) : (
        <>
          <motion.div
            className="card"
            style={{
              padding: "1.5rem",
              marginBottom: "1.5rem",
              textAlign: "center",
            }}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
          >
            <div className="achievements-summary">
              <div className="achievements-summary-circle">
                <Trophy size={32} style={{ color: "hsl(38, 90%, 50%)" }} />
              </div>
              <div>
                <p
                  style={{
                    fontSize: "1.5rem",
                    fontWeight: 700,
                    direction: "ltr",
                    display: "inline-block",
                  }}
                >
                  {earnedCount} / {totalCount}
                </p>
                <p
                  style={{
                    color: "var(--muted-foreground)",
                    fontSize: "0.875rem",
                  }}
                >
                  إنجازات مكتملة
                </p>
              </div>
            </div>

            <div
              className="lesson-progress-bar-bg"
              style={{ maxWidth: "20rem", margin: "1rem auto 0" }}
            >
              <motion.div
                className="lesson-progress-bar-fill"
                initial={{ width: 0 }}
                animate={{
                  width: `${
                    totalCount === 0 ? 0 : (earnedCount / totalCount) * 100
                  }%`,
                }}
                transition={{ duration: 1 }}
              />
            </div>
          </motion.div>

          <div className="achievements-grid">
            {orderedAchievements.length === 0 ? (
              <div
                className="card"
                style={{
                  padding: "2rem",
                  textAlign: "center",
                  color: "var(--muted-foreground)",
                }}
              >
                لا توجد إنجازات متاحة حالياً.
              </div>
            ) : (
              orderedAchievements.map((a, i) => {
                const style =
                  achievementStyleMap[a.title] ||
                  typeStyleMap[a.conditionType] ||
                  {
                    icon: Trophy,
                    color: "blue",
                  };

                const Icon = style.icon;
                const color = style.color;

                return (
                  <motion.div
                    key={a.achievementId}
                    className={`card achievement-card ${
                      !a.earned ? "achievement-locked" : ""
                    }`}
                    initial={{ opacity: 0, scale: 0.9 }}
                    animate={{ opacity: 1, scale: 1 }}
                    transition={{ delay: i * 0.06 }}
                  >
                    <div
                      className={`achievement-icon-wrap ${
                        a.earned
                          ? `rec-icon-${color}`
                          : "achievement-icon-locked"
                      }`}
                    >
                      {a.earned ? <Icon size={28} /> : <Lock size={24} />}
                    </div>

                    <h3 className="achievement-title">{a.title}</h3>
                    <p className="achievement-desc">{a.description}</p>

                    {a.earned && (
                      <span className="achievement-earned-badge">
                        <CheckCircle2 size={14} /> مكتمل
                      </span>
                    )}
                  </motion.div>
                );
              })
            )}
          </div>
        </>
      )}
    </DashboardLayout>
  );
};

export default Achievements;
