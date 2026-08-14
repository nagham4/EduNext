import { useState } from "react";
import { Link } from "react-router-dom";
import { motion } from "framer-motion";
import {
  ArrowLeft, Sparkles, Users, BookOpen, Trophy,
  Play, ChevronDown, Calculator, Atom, FlaskConical, Languages,
} from "lucide-react";
import logo from "@/assets/EDU.svg";
import heroImg from "@/assets/hero-students.png";
import featureAnalytics from "@/assets/feature-analytics.png";
import featureAI from "@/assets/feature-ai.png";
import featurePlan from "@/assets/feature-plan.png";
import featureChat from "@/assets/feature-chat.png";

const fadeUp = {
  hidden: { opacity: 0, y: 30 },
  visible: (i) => ({
    opacity: 1, y: 0,
    transition: { delay: i * 0.12, duration: 0.6, ease: [0.25, 0.46, 0.45, 0.94] },
  }),
};

const features = [
  { img: featureAnalytics, title: "تحليل أداء ذكي", desc: "تتبع تقدمك في كل مادة واحصل على تقارير مفصلة تساعدك على تحسين نقاط ضعفك." },
  { img: featureAI, title: "توصيات مخصصة بالذكاء الاصطناعي", desc: "يقترح عليك الذكاء الاصطناعي دروسًا وتمارين بناءً على مستواك الحقيقي." },
  { img: featurePlan, title: "خطط دراسية ذكية", desc: "جداول مخصصة تتكيف مع وقتك وأهدافك لتنظيم دراستك بشكل فعّال." },
  { img: featureChat, title: "مساعد ذكي لكل مادة", desc: "تحدث مع مساعد ذكي متخصص يجيب عن أسئلتك ويشرح لك المفاهيم الصعبة." },
];

const subjects = [
  { name: "الرياضيات", desc: "تفاضل، تكامل، إحصاء وأكثر — شروحات مبسطة وتمارين تفاعلية.", icon: Calculator, colorClass: "subject-blue" },
  { name: "الفيزياء", desc: "ميكانيكا، كهرباء، موجات — فيديوهات توضيحية وحلول نموذجية.", icon: Atom, colorClass: "subject-amber" },
  { name: "الكيمياء", desc: "كيمياء عضوية وغير عضوية — تجارب افتراضية وملخصات شاملة.", icon: FlaskConical, colorClass: "subject-green" },
  { name: "اللغة الإنجليزية", desc: "قواعد، مفردات، فهم مقروء — تدريبات ذكية تعزز مهاراتك.", icon: Languages, colorClass: "subject-purple" },
];

const stats = [
  { value: "+٥٠٠٠", label: "طالب نشط", icon: Users },
  { value: "+١٢٠٠", label: "درس تفاعلي", icon: BookOpen },
  { value: "٩٢٪", label: "نسبة رضا الطلاب", icon: Trophy },
];

const faqs = [
  { q: "كيف أبدأ باستخدام المنصة؟", a: "ببساطة أنشئ حسابًا مجانيًا، أكمل خطوات التعريف بنفسك، وسيُعد لك الذكاء الاصطناعي خطة دراسية مخصصة فورًا." },
  { q: "هل المنصة مجانية؟", a: "نعم، يمكنك البدء مجانًا والوصول إلى العديد من المواد والدروس. تتوفر أيضًا خطط مدفوعة بميزات إضافية." },
  { q: "كيف يتم تحليل أدائي؟", a: "يتتبع النظام إجاباتك في الاختبارات والتمارين، ثم يحلل نقاط القوة والضعف ويقدم لك توصيات مبنية على البيانات." },
  { q: "هل المحتوى يغطي كل مواد التوجيهي؟", a: "نعم، نغطي جميع مواد التوجيهي للفرعين العلمي والأدبي بمناهج فلسطينية محدّثة." },
];

const FaqItem = ({ q, a }) => {
  const [open, setOpen] = useState(false);
  return (
    <div className="faq-item">
      <button className="faq-trigger" data-open={open} onClick={() => setOpen(!open)}>
        <span>{q}</span>
        <ChevronDown />
      </button>
      {open && <div className="faq-answer">{a}</div>}
    </div>
  );
};

const Index = () => {
  return (
    <div style={{ minHeight: "100vh", overflowX: "hidden" }}>
      <nav className="navbar glass">
        <div className="container navbar-inner">
          <div className="navbar-brand">
            <img className="navbar-logo-image" src={logo} alt="EduNext" />
          </div>
          <div className="navbar-actions">
            <Link to="/login"><button className="btn btn-default btn-ghost">تسجيل الدخول</button></Link>
            <Link to="/register"><button className="btn btn-sm btn-primary shadow-primary">إنشاء حساب</button></Link>
          </div>
        </div>
      </nav>

      <section className="hero">
        <div className="pattern-dots" style={{ position: "absolute", inset: 0 }} />
        <div className="hero-bg-blob" style={{ top: "5rem", left: "2.5rem", width: "18rem", height: "18rem", background: "hsla(220,85%,55%,0.05)" }} />
        <div className="hero-bg-blob" style={{ bottom: "2.5rem", right: "2.5rem", width: "24rem", height: "24rem", background: "hsla(199,80%,50%,0.05)" }} />
        <div className="container relative">
          <div className="hero-content">
            <div className="hero-text">
              <motion.div initial="hidden" animate="visible" variants={fadeUp} custom={0} className="hero-badge">
                <Sparkles /><span>منصة مدعومة بالذكاء الاصطناعي</span>
              </motion.div>
              <motion.h1 className="hero-title" initial="hidden" animate="visible" variants={fadeUp} custom={1}>
                مستقبلك الدراسي<br /><span className="text-gradient">يبدأ من هنا</span>
              </motion.h1>
              <motion.p className="hero-desc" initial="hidden" animate="visible" variants={fadeUp} custom={2}>
                منصة تعليمية ذكية مصممة لطلاب التوجيهي في فلسطين — شروحات، اختبارات، وخطط دراسية مدعومة بالذكاء الاصطناعي.
              </motion.p>
              <motion.div className="hero-buttons" initial="hidden" animate="visible" variants={fadeUp} custom={3}>
                <Link to="/register">
                  <button className="btn btn-lg btn-primary shadow-primary-lg" style={{ fontSize: "1rem", padding: "0.5rem 2rem", height: "3rem" }}>
                    ابدأ التعلم مجانًا<ArrowLeft style={{ width: "1rem", height: "1rem", marginRight: "0.5rem" }} />
                  </button>
                </Link>
                <Link to="/login">
                  <button className="btn btn-lg btn-outline" style={{ fontSize: "1rem", padding: "0.5rem 2rem", height: "3rem" }}>تسجيل الدخول</button>
                </Link>
              </motion.div>
              <motion.div className="hero-stats" initial="hidden" animate="visible" variants={fadeUp} custom={4}>
                {stats.map((s) => (
                  <div key={s.label} style={{ textAlign: "center" }}>
                    <p className="hero-stat-value text-gradient">{s.value}</p>
                    <p className="hero-stat-label">{s.label}</p>
                  </div>
                ))}
              </motion.div>
            </div>
            <motion.div className="hero-image" initial={{ opacity: 0, x: -40 }} animate={{ opacity: 1, x: 0 }} transition={{ duration: 0.8, delay: 0.3 }}>
              <img src={heroImg} alt="منصة EduNext التعليمية" width={800} height={600} />
            </motion.div>
          </div>
        </div>
      </section>

      <section className="section" style={{ background: "var(--hero-gradient-soft)" }}>
        <div className="container">
          <div className="section-header">
            <span className="section-badge">المميزات</span>
            <h2 className="section-title">لماذا EduNext؟</h2>
            <p className="section-desc">أدوات ذكية تساعدك على التفوق في التوجيهي بأقل جهد وأكبر فائدة.</p>
          </div>
          <div className="features-grid">
            {features.map((f, i) => (
              <motion.div key={f.title} className="feature-card" initial="hidden" whileInView="visible" viewport={{ once: true }} variants={fadeUp} custom={i}>
                <img src={f.img} alt={f.title} width={120} height={120} loading="lazy" />
                <div style={{ flex: 1 }}><h3>{f.title}</h3><p>{f.desc}</p></div>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      <section className="section">
        <div className="container">
          <div className="section-header">
            <span className="section-badge">المواد الدراسية</span>
            <h2 className="section-title">مواد التوجيهي</h2>
            <p className="section-desc">اختر المادة وابدأ رحلتك التعليمية الآن.</p>
          </div>
          <div className="subjects-grid">
            {subjects.map((s, i) => (
              <motion.div key={s.name} className="subject-card" initial="hidden" whileInView="visible" viewport={{ once: true }} variants={fadeUp} custom={i}>
                <div className={`subject-emoji-area ${s.colorClass}`}>{(() => { const SubjectIcon = s.icon; return <SubjectIcon size={42} strokeWidth={1.8} />; })()}</div>
                <div className="subject-card-body">
                  <h3>{s.name}</h3><p>{s.desc}</p>
                  <Link to="/login"><button className="subject-btn">عرض المزيد</button></Link>
                </div>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      <section className="section" style={{ background: "var(--hero-gradient-soft)" }}>
        <div className="container max-w-3xl text-center">
          <span className="section-badge">شرح المنصة</span>
          <h2 className="section-title">شاهد كيف تعمل المنصة</h2>
          <p className="section-desc" style={{ marginBottom: "2.5rem" }}>فيديو تعريفي قصير يوضح لك كيف يمكنك الاستفادة من EduNext في دراستك.</p>
          <div className="video-placeholder">
            <div className="video-overlay hero-gradient" />
            <div style={{ position: "relative", display: "flex", flexDirection: "column", alignItems: "center", gap: "1rem" }}>
              <button className="video-play-btn hero-gradient"><Play /></button>
              <span style={{ color: "var(--muted-foreground)", fontSize: "0.875rem", fontWeight: 500 }}>اضغط لمشاهدة الفيديو</span>
            </div>
          </div>
        </div>
      </section>

      <section className="section">
        <div className="container max-w-2xl">
          <div className="section-header" style={{ marginBottom: "3rem" }}>
            <span className="section-badge">أسئلة شائعة</span>
            <h2 className="section-title">الأسئلة الشائعة</h2>
            <p className="section-desc">إجابات سريعة لأكثر الأسئلة شيوعًا حول المنصة.</p>
          </div>
          <div className="faq-list">
            {faqs.map((faq, i) => (<FaqItem key={i} q={faq.q} a={faq.a} />))}
          </div>
        </div>
      </section>

      <section className="section">
        <div className="container max-w-4xl">
          <div className="cta-box hero-gradient relative">
            <div style={{ position: "absolute", inset: 0, opacity: 0.1, backgroundImage: "radial-gradient(circle at 20% 50%, white 1px, transparent 1px)", backgroundSize: "32px 32px" }} />
            <div className="relative">
              <h2>جاهز تبدأ رحلتك نحو التفوق؟</h2>
              <p>انضم لآلاف الطلاب الذين يستخدمون EduNext للتحضير للتوجيهي بذكاء وثقة.</p>
              <div className="cta-buttons">
                <Link to="/register"><button className="btn btn-lg btn-secondary" style={{ fontSize: "1rem", fontWeight: 600 }}>ابدأ مجانًا<ArrowLeft style={{ width: "1rem", height: "1rem", marginRight: "0.5rem" }} /></button></Link>
                <Link to="/login"><button className="btn btn-lg btn-outline cta-btn-outline" style={{ fontSize: "1rem" }}>تسجيل الدخول</button></Link>
              </div>
            </div>
          </div>
        </div>
      </section>

      <footer className="footer">
        <div className="container footer-inner">
          <div className="footer-brand">
            <img className="footer-logo-image" src={logo} alt="EduNext" />
          </div>
          <p className="footer-text">© ٢٠٢٦ EduNext — منصة تعليمية ذكية لطلاب التوجيهي في فلسطين.</p>
        </div>
      </footer>
    </div>
  );
};

export default Index;
