import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  ArrowLeft,
  Atom,
  BarChart3,
  BookOpen,
  Bot,
  Calendar,
  ChevronDown,
  ClipboardCheck,
  Database,
  Facebook,
  Globe2,
  GraduationCap,
  Instagram,
  Layers,
  Lightbulb,
  LineChart,
  Mail,
  MapPin,
  Menu,
  Phone,
  Plus,
  School,
  Send,
  Sigma,
  Smartphone,
  Sparkles,
  Target,
  TrendingUp,
  UserPlus,
  UsersRound,
  X,
  Youtube,
} from 'lucide-react';
import './Home.css';
import GeneralChatbot from '../../../components/Public_Chatbot/GeneralChatbot.jsx';
import logo from '../../../assets/EDU.svg';
import heroImage from '../../../assets/lovable-home/hero.png';
import whyImage from '../../../assets/lovable-home/why.png';
import planVideo from '../../../assets/plan-demo.mp4';
import chatbotVideo from '../../../assets/chatbot-demo.mp4';
import dashboardVideo from '../../../assets/dashboard-demo.mp4';


const quickFeatures = [
  { label: 'مساعد ذكي لكل مادة', icon: Bot },
  { label: 'خطط دراسية ذكية', icon: Calendar },
  { label: 'تحليل الأداء', icon: LineChart },
  { label: 'اختبارات تجريبية', icon: ClipboardCheck },
  { label: 'توصيات مخصصة', icon: Sparkles },
];

const showcaseCards = [
  {
    title: 'الخطة الدراسية',
    text: 'جدول يومي يتكيّف مع مستواك ووقتك وأهدافك.',
    type: 'plan',
    video: planVideo,
  },
  {
    title: 'المساعد الذكي',
    text: 'اسأل أي سؤال واحصل على شرح فوري ودقيق.',
    type: 'chat',
    video: chatbotVideo,
  },
  {
    title: 'لوحة تحليل الأداء',
    text: 'تابع تقدمك وأدائك في المواد والاختبارات بسهولة.',
    type: 'chart',
    video: dashboardVideo,
  },
];

const toolCards = [
  { title: 'مساعد ذكي لكل مادة', text: 'روبوت ذكي يجيب على أسئلتك ويشرحها فوراً 24/7.', icon: Bot, tone: 'teal' },
  { title: 'خطط دراسية ذكية', text: 'جداول تتكيّف مع وقتك وأهدافك ومستواك الدراسي.', icon: Calendar, tone: 'blue' },
  { title: 'توصيات مخصصة', text: 'محتوى ودروس مخصّصة بناءً على تحليل أدائك.', icon: Sparkles, tone: 'purple' },
  { title: 'اختبارات تجريبية', text: 'اختبارات على نسق الامتحان الوزاري مع تصحيح فوري.', icon: ClipboardCheck, tone: 'teal' },
  { title: 'تتبع التقدم', text: 'لوحة تحكم ذكية تعرض تطورك ونقاط قوتك وضعفك.', icon: TrendingUp, tone: 'blue' },
];

const whyItems = [
  { title: 'تعلم مخصص', icon: BookOpen },
  { title: 'توصيات ذكية', icon: Sparkles },
  { title: 'مساعد ذكي', icon: Bot },
  { title: 'متابعة التقدم', icon: TrendingUp },
  { title: 'تنظيم الوقت', icon: Calendar },
  { title: 'تحسين الأداء الأكاديمي', icon: GraduationCap },
];

const subjects = [
  {
    id: 'arabic',
    name: 'اللغة العربية',
    description: 'الأدب، القواعد، التعبير والبلاغة.',
    longDescription:
      'مادة أساسية تساعدك على إتقان النصوص والقواعد والبلاغة والتعبير، مع تدريبات مناسبة لنمط أسئلة التوجيهي.',
    icon: BookOpen,
    tone: 'teal',
  },
  {
    id: 'english',
    name: 'اللغة الإنجليزية',
    description: 'القواعد والتراكم، الأدب والاستيعاب القرائي.',
    longDescription:
      'مراجعة مركزة للقواعد والمفردات والاستيعاب القرائي والنصوص، مع شرح مبسط وتمارين تقيس تقدمك.',
    icon: Globe2,
    tone: 'blue',
  },
  {
    id: 'math',
    name: 'الرياضيات',
    description: 'التفاضل والتكامل، الجبر والإحصاء.',
    longDescription:
      'مسار تدريبي للرياضيات يغطي التفاضل والتكامل والجبر والإحصاء، مع تحليل للأخطاء وخطة مراجعة ذكية.',
    icon: Sigma,
    tone: 'purple',
  },
  {
    id: 'physics',
    name: 'الفيزياء',
    description: 'الميكانيكا، الكهرومغناطيسية والفيزياء الحديثة.',
    longDescription:
      'شرح مفاهيم الفيزياء خطوة بخطوة، من الميكانيكا إلى الكهرباء والفيزياء الحديثة، مع تطبيقات وأسئلة تدريبية.',
    icon: Atom,
    tone: 'teal',
  },
];

const steps = [
  { title: 'إنشاء حساب', text: 'سجل حساباً مجانياً وابدأ رحلتك الآن.', icon: UserPlus, tone: 'teal' },
  { title: 'اختر موادك', text: 'اختر المواد التي تريد دراستها حسب مستواك وأهدافك.', icon: Layers, tone: 'blue' },
  { title: 'ادرس مع المساعد الذكي', text: 'احصل على شرح وحلول فورية مخصصة لكل مادة.', icon: Bot, tone: 'purple' },
  { title: 'تابع تقدمك', text: 'راقب أداءك وحقق أهدافك بخطوات ثابتة.', icon: TrendingUp, tone: 'teal' },
];

const upcoming = [
  { title: 'تطبيق الهاتف', text: 'تابع دراستك في أي وقت ومن أي مكان.', icon: Smartphone },
  { title: 'مجتمع تعليمي متكامل', text: 'مجموعات دراسة، نقاشات، وتحديات بين الطلاب.', icon: UsersRound },
  { title: 'بنك أسئلة أكبر', text: 'آلاف الأسئلة الجديدة مع تحديثات مستمرة.', icon: Database },
  { title: 'توصيات أكثر ذكاءً', text: 'تحليل أعمق للنقاط والفرص لتعلّم أكثر دقة.', icon: Lightbulb },
  { title: 'لوحة للمعلمين', text: 'متابعة أداء الطلاب وإدارة المهام بسهولة.', icon: School },
  { title: 'دعم مواد إضافية', text: 'توسيع تغطية المواد والفروع المستقبلية.', icon: Plus },
];


const heroCards = [
  {
    progress: "82%",
    recommendation: "راجع الفيزياء لمدة 30 دقيقة",
    goals: "3 أهداف مكتملة هذا الأسبوع",
    exam: "رياضيات — 9/10",

    subject: "الرياضيات",
  },
  {
    progress: "87%",
    recommendation: "حل 15 سؤال كيمياء",
    goals: "4 أهداف مكتملة هذا الأسبوع",
    exam: "فيزياء — 8/10",

    subject: "الفيزياء",
  },
  {
    progress: "91%",
    recommendation: "مراجعة الوحدة الثالثة إنجليزي",
    goals: "5 أهداف مكتملة هذا الأسبوع",
    exam: "إنجليزي — 10/10",

    subject: "اللغة الإنجليزية",
  },
  {
    progress: "95%",
    recommendation: "مراجعة مكثفة للوزاري",
    goals: "6 أهداف مكتملة هذا الأسبوع",
    exam: "عربي — 10/10",

    subject: "اللغة العربية",
  },
  {
    progress: "78%",
    recommendation: "أكمل وحدة التفاضل والتكامل",
    goals: "2 أهداف مكتملة هذا الأسبوع",
    exam: "كيمياء — 7/10",

    subject: "الكيمياء",
  },
  {
    progress: "89%",
    recommendation: "راجع قواعد اللغة العربية",
    goals: "5 أهداف مكتملة هذا الأسبوع",
    exam: "أحياء — 9/10",

    subject: "الأحياء",
  },
];

const faqs = [
  {
    question: 'ما هي منصة EduNext؟',
    answer: 'EduNext منصة تعليمية ذكية مصممة لطلاب التوجيهي، تقدم خططاً دراسية مخصصة ومساعداً ذكياً لكل مادة وتحليلات أداء دقيقة.',
  },
  {
    question: 'هل المنصة مجانية؟',
    answer: 'يمكنك البدء مجاناً وتجربة أدوات المنصة الأساسية، ثم اختيار الميزات المناسبة لاحتياجاتك لاحقاً.',
  },
  {
    question: 'كيف يعمل الذكاء الاصطناعي في المنصة؟',
    answer: 'يحلل أداءك وأهدافك ونمط دراستك، ثم يقترح خطة وتوصيات وأسئلة تدريبية تساعدك على تحسين نتائجك.',
  },
  {
    question: 'هل يمكنني استخدام المنصة على الهاتف؟',
    answer: 'نعم، التصميم متجاوب ويعمل بسلاسة على الهاتف والتابلت والكمبيوتر.',
  },
];

function BrowserMockup({ type, video, onVideoClick }) {
  return (
    <div className={`mock-window ${type}`}>
      <div className="window-dots">
        <span />
        <span />
        <span />
      </div>
      {video && (
        <div className="showcase-video-wrapper" onClick={onVideoClick} role="button" tabIndex={0} onKeyDown={(e) => e.key === 'Enter' && onVideoClick()} aria-label="تكبير الفيديو">
          <video
            className="showcase-video"
            src={video}
            autoPlay
            muted
            loop
            playsInline
          />
          <div className="showcase-video-overlay">
            <span>🔍</span>
          </div>
        </div>
      )}
    </div>
  );
}

const navLinks = [
  { href: '#hero', label: 'الرئيسية', section: 'hero' },
  { href: '#showcase', label: 'شاهد المنصة', section: 'showcase' },
  { href: '#subjects', label: 'المواد', section: 'subjects' },
  { href: '#upcoming', label: 'ميزات قادمة', section: 'upcoming' },
  { href: '#faq', label: 'الأسئلة الشائعة', section: 'faq' },
  { href: '/contact', label: 'تواصل معنا', isRoute: true },
];

export default function Home() {
  const navigate = useNavigate();
  const [mobileOpen, setMobileOpen] = useState(false);
  const [openFaq, setOpenFaq] = useState(0);
  const [activeSubject, setActiveSubject] = useState(null);
  const [activeVideo, setActiveVideo] = useState(null);
  const [scrolled, setScrolled] = useState(false);
  const [activeSection, setActiveSection] = useState('hero');

  const [heroIndex, setHeroIndex] = useState(0);
  const [cardsVisible, setCardsVisible] = useState(true);
  const [mousePosition, setMousePosition] = useState({ x: 0, y: 0 });

  useEffect(() => {
    const handleScroll = () => {
      setScrolled(window.scrollY > 16);
      const sections = ['hero', 'showcase', 'tools', 'why', 'subjects', 'steps', 'upcoming', 'faq'];
      const offsets = sections.map((id) => {
        const el = document.getElementById(id);
        return el ? { id, top: el.getBoundingClientRect().top } : null;
      }).filter(Boolean);
      const current = offsets.filter((s) => s.top <= 110).at(-1);
      if (current) setActiveSection(current.id);
    };
    window.addEventListener('scroll', handleScroll, { passive: true });
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  useEffect(() => {
    const timer = setInterval(() => {

      setCardsVisible(false);
      setTimeout(() => {
        setHeroIndex((prev) => (prev + 1) % heroCards.length);
        setCardsVisible(true);
      }, 400);
    }, 10000);
    return () => clearInterval(timer);
  }, []);

  const handleHeroMove = (e) => {
    const rect = e.currentTarget.getBoundingClientRect();
    const x = (e.clientX - rect.left - rect.width / 2) / 20;
    const y = (e.clientY - rect.top - rect.height / 2) / 20;
    setMousePosition({ x, y });
  };


  const handleLogin = () => navigate('/login');
  const handleSignup = () => navigate('/register');

  return (
    <div className="edn-home" dir="rtl">
      <header className={`edn-header ${scrolled ? 'scrolled' : ''}`}>
        <div className="edn-container edn-nav">
          <button className="edn-brand" type="button" onClick={() => navigate('/')}>
            <img className="edn-brand-logo" src={logo} alt="EduNext" />
          </button>

          <nav className={`edn-links ${mobileOpen ? 'open' : ''}`}>
            {navLinks.map((link) => (
              <a
                key={link.href}
                href={link.isRoute ? undefined : link.href}
                className={!link.isRoute && activeSection === link.section ? 'nav-active' : ''}
                onClick={(e) => {
                  if (link.isRoute) { e.preventDefault(); navigate(link.href); }
                  setMobileOpen(false);
                }}
              >
                {link.label}
                <span className="nav-dot" />
              </a>
            ))}
          </nav>

          <div className="edn-actions">
            <button className="btn btn-ghost" type="button" onClick={handleLogin}>تسجيل دخول</button>
            <button className="btn btn-primary" type="button" onClick={handleSignup}>ابدأ مجاناً</button>
          </div>

          <button className="menu-btn" type="button" onClick={() => setMobileOpen((value) => !value)} aria-label="فتح القائمة">
            {mobileOpen ? <X size={22} /> : <Menu size={22} />}
          </button>
        </div>
      </header>

      <main>
        <section className="hero-section grid-bg" id="hero">
          <div className="edn-container hero-grid">
            <div className="hero-copy">
              <span className="pill">منصة التوجيهي الذكية</span>
              <h1>تعلّم بذكاء،<br /><mark>وتفوّق بثقة</mark></h1>
              <p>
                EduNext هي منصة تعليمية ذكية لطلاب التوجيهي تقدم خططاً دراسية مخصصة،
                مساعداً ذكياً لكل مادة، وتحليلات أداء تساعدك على تحقيق أفضل النتائج.
              </p>
              <div className="hero-buttons">
                <button className="btn btn-primary" type="button" onClick={handleSignup}>ابدأ مجاناً</button>
                <button className="btn btn-ghost" type="button" onClick={handleLogin}>تسجيل الدخول</button>
              </div>
            </div>

            <div
              className="hero-visual"
              onMouseMove={handleHeroMove}
              onMouseLeave={() => setMousePosition({ x: 0, y: 0 })}
            >
              <div
                className="hero-image-panel"
              >
                <img src={heroImage} alt="طالب يستخدم منصة EduNext" />
              </div>
              <div
                className={`float-card progress ${cardsVisible ? 'cards-in' : 'cards-out'}`}
                style={{ transform: `translate(${mousePosition.x * -1.4}px, ${mousePosition.y * -1.4}px)` }}
              >
                <span>معدل التقدم</span>
                <strong>{heroCards[heroIndex].progress}</strong>
                <LineChart size={18} />
              </div>
              <div
                className={`float-card recommend ${cardsVisible ? 'cards-in' : 'cards-out'}`}
                style={{ transform: `translate(${mousePosition.x * -1.8}px, ${mousePosition.y * -1.0}px)` }}
              >
                <span>توصية ذكية</span>
                <strong>{heroCards[heroIndex].recommendation}</strong>
                <Bot size={18} />
              </div>
              <div
                className={`float-card target ${cardsVisible ? 'cards-in' : 'cards-out'}`}
                style={{ transform: `translate(${mousePosition.x * 1.6}px, ${mousePosition.y * 1.2}px)` }}
              >
                <span>الأهداف</span>
                <strong>{heroCards[heroIndex].goals}</strong>
                <Target size={18} />
              </div>
              <div
                className={`float-card exam ${cardsVisible ? 'cards-in' : 'cards-out'}`}
                style={{ transform: `translate(${mousePosition.x * -1.2}px, ${mousePosition.y * 1.6}px)` }}
              >
                <span>اختبار مكتمل</span>
                <strong>{heroCards[heroIndex].exam}</strong>
                <ClipboardCheck size={18} />
              </div>

              <div
                className={`float-card subject-best ${cardsVisible ? 'cards-in' : 'cards-out'}`}
                style={{ transform: `translate(${mousePosition.x * -1.0}px, ${mousePosition.y * 1.8}px)` }}
              >
                <span>أفضل مادة</span>
                <strong>{heroCards[heroIndex].subject}</strong>
                <GraduationCap size={18} />
              </div>
            </div>
          </div>

          <div className="edn-container">
            <div className="quick-strip">
              {quickFeatures.map((item) => (
                <div key={item.label}>
                  <item.icon size={18} />
                  <span>{item.label}</span>
                </div>
              ))}
            </div>
          </div>
        </section>

        <section className="showcase-section grid-bg" id="showcase">
          <div className="edn-container">
            <div className="section-title">
              <span className="pill">تجربة واقعية</span>
              <h2>شاهد EduNext أثناء العمل</h2>
              <p>اكتشف كيف تساعدك المنصة على تنظيم الدراسة وتحليل الأداء والتعلّم بذكاء.</p>
            </div>
            <div className="showcase-grid">
              {showcaseCards.map((card) => (
                <article className="showcase-card" key={card.title}>
                  <BrowserMockup type={card.type} video={card.video} onVideoClick={() => setActiveVideo(card.video)} />
                  <h3>{card.title}</h3>
                  <p>{card.text}</p>
                </article>
              ))}
            </div>
          </div>
        </section>

        <section className="tools-section" id="tools">
          <div className="edn-container">
            <div className="section-title">
              <span className="eyebrow">أدوات EduNext</span>
              <h2>كل ما تحتاجه للتعلّم بذكاء</h2>
              <p>أدوات متكاملة تساعدك على التعلم بتركيز وتحقق أفضل النتائج.</p>
            </div>
            <div className="tools-grid">
              {toolCards.map((tool) => (
                <article className={`tool-card ${tool.tone}`} key={tool.title}>
                  <div className="icon-box"><tool.icon size={26} /></div>
                  <h3>{tool.title}</h3>
                  <p>{tool.text}</p>
                </article>
              ))}
            </div>
          </div>
        </section>

        <section className="why-section grid-bg" id="why">
          <div className="edn-container why-grid">
            <div className="why-copy">
              <span className="eyebrow">لماذا EduNext</span>
              <h2>التوجيهي صعب بما فيه الكفاية،<br />الدراسة لا يجب أن تكون كذلك.</h2>
              <div className="why-list">
                {whyItems.map((item) => (
                  <div key={item.title}>
                    <item.icon size={18} />
                    <span>{item.title}</span>
                  </div>
                ))}
              </div>
            </div>
            <div className="why-image">
              <img src={whyImage} alt="" />
            </div>
          </div>
        </section>

        <section className="subjects-section" id="subjects">
          <div className="edn-container">
            <div className="section-title">
              <span className="eyebrow">المواد</span>
              <h2>استكشف موادك الدراسية</h2>
              <p>اختر المادة وابدأ رحلتك التعليمية.</p>
            </div>
            <div className="subjects-grid">
              {subjects.map((subject) => (
                <article className={`subject-card ${subject.tone}`} key={subject.id}>
                  <div className="icon-box"><subject.icon size={30} /></div>
                  <h3>{subject.name}</h3>
                  <p>{subject.description}</p>
                  <button type="button" onClick={() => setActiveSubject(subject)}>
                    استكشف المادة <ArrowLeft size={16} />
                  </button>
                </article>
              ))}
            </div>
            <div className="subjects-signup">
              <div>
                <span>مواد أكثر بانتظارك</span>
                <h3>لتصفح باقي المواد، سجل حسابك وابدأ رحلتك معنا</h3>
                <p>افتح كل المواد والخطط والاختبارات من حسابك الشخصي.</p>
              </div>
              <button className="btn btn-primary" type="button" onClick={handleSignup}>
                سجل الآن <ArrowLeft size={18} />
              </button>
            </div>
          </div>
        </section>

        <section className="steps-section grid-bg" id="steps">
          <div className="edn-container">
            <div className="section-title">
              <span className="pill">الخطوات</span>
              <h2>كيف تبدأ رحلتك مع EduNext</h2>
            </div>
            <div className="steps-line">
              {steps.map((step, index) => (
                <article className={`step-item ${step.tone}`} key={step.title}>
                  <div className="step-icon">
                    <step.icon size={32} />
                    <span>{index + 1}</span>
                  </div>
                  <h3>{step.title}</h3>
                  <p>{step.text}</p>
                </article>
              ))}
            </div>
          </div>
        </section>

        <section className="upcoming-section grid-bg-soft" id="upcoming">
          <div className="edn-container">
            <div className="section-title">
              <span className="pill">يتم العمل عليها</span>
              <h2>ميزات قادمة قريباً</h2>
              <p>نعمل باستمرار على تطوير المنصة لتقديم أفضل تجربة تعليمية ممكنة.</p>
            </div>
            <div className="upcoming-grid">
              {upcoming.map((item) => (
                <article className="upcoming-card" key={item.title}>
                  <div><item.icon size={22} /></div>
                  <h3>{item.title}</h3>
                  <p>{item.text}</p>
                </article>
              ))}
            </div>
          </div>
        </section>

        <section className="faq-section grid-bg" id="faq">
          <div className="edn-container">
            <div className="section-title">
              <span className="eyebrow">FAQ</span>
              <h2>الأسئلة الشائعة</h2>
            </div>
            <div className="faq-list">
              {faqs.map((faq, index) => (
                <article className={`faq-item ${openFaq === index ? 'open' : ''}`} key={faq.question}>
                  <button type="button" onClick={() => setOpenFaq(openFaq === index ? null : index)}>
                    <span>{faq.question}</span>
                    <ChevronDown size={20} />
                  </button>
                  {openFaq === index && <p>{faq.answer}</p>}
                </article>
              ))}
            </div>
          </div>
        </section>

      </main>

      <footer className="edn-footer">
        <div className="edn-container footer-grid">
          <div>
            <div className="edn-brand footer-brand">
              <img className="edn-brand-logo" src={logo} alt="EduNext" />
              <strong>EduNext</strong>
            </div>
            <p>منصة تعليمية ذكية لطلاب التوجيهي.</p>
            <div className="socials">
              <Instagram size={18} />
              <Send size={18} />
              <Youtube size={18} />
              <Facebook size={18} />
            </div>
          </div>
          <div>
            <h3>المواد الدراسية</h3>
            <a href="#subjects">اللغة العربية</a>
            <a href="#subjects">اللغة الإنجليزية</a>
            <a href="#subjects">الرياضيات</a>
            <a href="#subjects">الفيزياء</a>
          </div>
          <div>
            <h3>المنصة</h3>
            <button type="button" onClick={handleLogin}>تسجيل دخول</button>
            <button type="button" onClick={handleSignup}>إنشاء حساب</button>
            <a href="#tools">أدوات المنصة</a>
            <a href="#faq">الأسئلة الشائعة</a>
          </div>
          <div>
            <h3>تواصل معنا</h3>
            <p><Mail size={16} /> edunext.contact@gmail.com</p>
            <p><Phone size={16} /> 3522 895 59 970+</p>
            <p><MapPin size={16} /> جنين، فلسطين</p>
          </div>
        </div>
        <div className="edn-container copyright">جميع الحقوق محفوظة © 2026 EduNext</div>
      </footer>

      {activeSubject && (
        <div className="subject-modal-overlay" onClick={() => setActiveSubject(null)} role="presentation">
          <div className="subject-modal" onClick={(event) => event.stopPropagation()} role="dialog" aria-modal="true">
            <button className="modal-close" type="button" onClick={() => setActiveSubject(null)} aria-label="إغلاق"><X size={18} /></button>
            <div className={`icon-box ${activeSubject.tone}`}><activeSubject.icon size={32} /></div>
            <h3>{activeSubject.name}</h3>
            <p>{activeSubject.longDescription}</p>
            <div className="modal-actions">
              <button className="btn btn-primary" type="button" onClick={handleLogin}>ابدأ التعلم</button>
              <button className="btn btn-ghost" type="button" onClick={() => setActiveSubject(null)}>إغلاق</button>
            </div>
          </div>
        </div>
      )}
      {activeVideo && (
        <div className="video-modal-overlay" onClick={() => setActiveVideo(null)} role="presentation">
          <div className="video-modal" onClick={(e) => e.stopPropagation()} role="dialog" aria-modal="true">
            <button className="modal-close" type="button" onClick={() => setActiveVideo(null)} aria-label="إغلاق"><X size={18} /></button>
            <video
              className="video-modal-player"
              src={activeVideo}
              autoPlay
              muted
              loop
              playsInline
              controls
            />
          </div>
        </div>
      )}
      <GeneralChatbot compact />
    </div>
  );
}
