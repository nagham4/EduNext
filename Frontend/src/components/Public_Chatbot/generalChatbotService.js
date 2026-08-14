/*
  QUICK CUSTOMIZATION AREA
  عدل هذا الجزء فقط عند نقل الشات بوت لمشروع ثاني.

  المطلوب من كوبايلوت/أي مساعد:
  1. افحص صفحات المشروع، خصوصا الهوم وصفحات الطالب والزائر.
  2. أضف الصفحات العامة التي ممكن يسأل عنها الطالب أو الزائر داخل siteSections.
  3. أضف الإجابات العامة عن خدمات الموقع داخل featureAnswers.
  4. أضف كلمات البحث المهمة داخل siteKeywords.
  5. أضف المواضيع التي لا تريد الشات بوت يدخل فيها داخل outOfScopeKeywords.

  ملاحظة:
  هذا الشات بوت محلي، لا يحتاج API، ومناسب للأسئلة العامة عن الموقع فقط.
*/

export const EDIT_THIS_CONTENT = {
  projectName: 'EduNext',
  assistantTitle: 'مساعد EduNext',
  audienceLabel: 'دليل سريع للزوار والطلاب',

  introMessage:
    'أهلا، أنا مساعد EduNext للزوار والطلاب. اسألني عن فكرة الموقع، التسجيل، تسجيل الدخول، المواد، خطط الدراسة، أو الامتحانات.',

  helperLine:
    'اسأل عن EduNext، التسجيل، الدخول، المواد، الخطط، أو الامتحانات.',

  inputPlaceholder:
    'اسأل عن EduNext، الأقسام، الصفحات، أو ما يوفره الموقع...',

  closedTeaser:
    'مرحبا! اسألني عن EduNext، التسجيل، المواد، وخدمات الطالب.',

  loadingMessage:
    'ثواني، براجع سؤالك ضمن معلومات EduNext...',

  suggestions: [
    'شو هدف موقع EduNext؟',
    'وين بعمل حساب جديد؟',
    'وين بلاقي قسم المواد الدراسية؟',
    'شو بوفر الموقع للطالب؟',
  ],

  siteSections: [
    {
      key: 'home',
      title: 'الرئيسية',
      route: '/',
      aliases: ['الرئيسية', 'الصفحة الرئيسية', 'home', 'بداية', 'اول صفحة'],
      answer:
        'الرئيسية هي أول صفحة بالموقع. بتعطي الزائر فكرة سريعة عن EduNext، وشو الهدف من المنصة، ومنها بقدر يروح لتسجيل الدخول أو إنشاء حساب.',
    },
    {
      key: 'subjects',
      title: 'المواد الدراسية',
      route: '/#subjects',
      aliases: ['المواد', 'مواد', 'المواد الدراسية', 'subjects', 'رياضيات', 'فيزياء', 'انجليزي', 'عربي'],
      answer:
        'قسم المواد الدراسية موجود بالصفحة الرئيسية تحت "استكشف موادك الدراسية". هناك بتشوف أمثلة على المواد المتوفرة مثل الرياضيات، الفيزياء، اللغة الإنجليزية، واللغة العربية.',
    },
    {
      key: 'faq',
      title: 'الأسئلة الشائعة',
      route: '/#faq',
      aliases: ['الاسئلة الشائعة', 'أسئلة شائعة', 'faq', 'اسئلة', 'سؤال وجواب'],
      answer:
        'قسم الأسئلة الشائعة موجود أسفل الصفحة الرئيسية. بفيد الزائر يعرف بسرعة شو هي EduNext، كيف بتشتغل، وهل بتناسب الهاتف.',
    },
    {
      key: 'login',
      title: 'تسجيل الدخول',
      route: '/login',
      aliases: ['تسجيل دخول', 'دخول', 'login', 'ادخل', 'اسجل دخول', 'وين ادخل'],
      answer: 'تسجيل الدخول موجود من زر "تسجيل دخول" بأعلى الصفحة. الرابط المباشر: /login',
    },
    {
      key: 'signup',
      title: 'إنشاء حساب',
      route: '/signup',
      aliases: ['انشاء حساب', 'إنشاء حساب', 'تسجيل', 'اسجل', 'سجل', 'حساب جديد', 'ابدأ مجانا', 'ابدأ مجاناً', 'signup', 'register'],
      answer: 'إنشاء الحساب موجود من زر "ابدأ مجاناً" أو "إنشاء حساب" في الصفحة الرئيسية. الرابط المباشر: /signup',
    },
    {
      key: 'study-plans',
      title: 'خطط الدراسة',
      route: '/study-plans',
      aliases: ['خطط الدراسة', 'خطة دراسة', 'study plans', 'جدول دراسة', 'الخطة'],
      answer: 'خطط الدراسة موجودة عبر /study-plans. فكرتها تساعد الطالب يرتب دراسته ويتابع شو لازم ينجز داخل EduNext.',
    },
    {
      key: 'student-exam',
      title: 'امتحان الطالب',
      route: '/student-exam',
      aliases: ['امتحان', 'اختبار', 'امتحانات', 'student exam', 'تجريبي'],
      answer: 'صفحة امتحان الطالب موجودة عبر /student-exam. بتفيد الطالب بتجربة الاختبارات أو متابعة الامتحانات المتاحة داخل الموقع.',
    },

    // أضف صفحات الطالب هنا عند نقل الشات بوت لمشروعك الأصلي.
    // مثال:
    // {
    //   key: 'student-dashboard',
    //   title: 'لوحة الطالب',
    //   route: '/student-dashboard',
    //   aliases: ['لوحة الطالب', 'dashboard الطالب', 'صفحتي'],
    //   answer: 'لوحة الطالب تعرض ملخص سريع عن حساب الطالب وتقدمه داخل الموقع.',
    // },
  ],

  featureAnswers: [
    {
      aliases: ['شو الهدف', 'هدف', 'ليش', 'ما هي', 'ما هو', 'مين انت', 'عرفني', 'عن الموقع', 'عن المنصة'],
      answer:
        'EduNext منصة تعليمية لطلاب التوجيهي. فكرتها تجمع للطالب مكان واضح يعرف منه المواد، يسجل حساب، يستخدم خطط الدراسة، ويجرّب الامتحانات بطريقة منظمة.',
    },
    {
      aliases: ['شو بوفر', 'شو يوفر', 'مميزات', 'ميزات', 'خدمات', 'اشياء متوفرة', 'المتوفر', 'شو فيه'],
      answer:
        'EduNext يوفر للزائر والطالب: تعريف عن المنصة، عرض للمواد الدراسية، أسئلة شائعة، تسجيل دخول، إنشاء حساب، خطط دراسة، وصفحة امتحان للطالب.',
    },
    {
      aliases: ['طالب', 'للطلاب', 'student', 'كيف بفيد الطالب', 'يفيدني'],
      answer:
        'بفيد الطالب لأنه بعرّفه على المواد المتاحة، وبوفر مكان للتسجيل، وخطط دراسة، وتجربة امتحانات. الهدف إنه الطالب يعرف وين يروح داخل الموقع بدون ما يضيع.',
    },
    {
      aliases: ['زائر', 'للزوار', 'visitor', 'قبل التسجيل'],
      answer:
        'كزائر، بتقدر تشوف فكرة EduNext من الرئيسية، تتصفح قسم المواد، تقرأ الأسئلة الشائعة، وبعدها تختار تسجيل الدخول أو إنشاء حساب جديد.',
    },
    {
      aliases: ['هاتف', 'موبايل', 'جوال', 'mobile'],
      answer:
        'الموقع مناسب للهاتف والكمبيوتر. على الشاشات الصغيرة بتظهر القائمة بشكل أبسط عشان الزائر يقدر يتنقل بسهولة.',
    },

    // أضف هنا إجابات عامة عن خدمات الطالب.
    // مثال:
    // {
    //   aliases: ['درجاتي', 'علاماتي', 'النتائج'],
    //   answer: 'قسم النتائج يساعد الطالب يشوف أداءه أو علاماته إذا كان هذا القسم موجود في المشروع.',
    // },
  ],

  siteKeywords: [
    'edunext',
    'الموقع',
    'المنصة',
    'النظام',
    'قسم',
    'صفحة',
    'وين',
    'اين',
    'أين',
    'بلاقي',
    'بوفر',
    'يوفر',
    'هدف',
    'متوفر',
    'موجود',
    'تسجيل',
    'دخول',
    'حساب',
    'مواد',
    'امتحان',
    'اختبار',
    'خطة',
    'أسئلة',
    'اسئلة',
    'طالب',
    'زائر',

    // أضف كلمات المشروع الأصلية هنا.
    // مثال: 'واجبات', 'درجات', 'ملف الطالب', 'الدورات'
  ],

  outOfScopeKeywords: [
    'حفزني',
    'تحفيز',
    'متوتر',
    'خايف',
    'ادرس',
    'اشرح',
    'حل',
    'مسألة',
    'ترجم',
    'اكتبلي كود',
    'برمجة',
    'سياسة',
    'اخبار',
    'أخبار',
    'طب',
    'دواء',
    'وصفة',
    'كرة',
    'مباراة',
    'نكتة',
    'حب',
    'زواج',
    'فتوى',
    'ادمن',
    'أدمن',
    'admin',
    'لوحة التحكم',
    'إدارة',
    'ادارة',
    'coding',
    'news',
    'politics',
    'medical',
  ],

  boundaryReply:
    'أنا مساعد EduNext للزوار والطلاب. بقدر أجاوب عن فكرة الموقع، التسجيل، تسجيل الدخول، المواد، خطط الدراسة، الامتحانات، والأسئلة الشائعة. غير هيك خلينا نضل ضمن معلومات EduNext.',
};

export function prepareGeneralChatTurn({ message, replyTarget = null }) {
  return {
    shouldAsk: false,
    clearPending: true,
    resetContext: true,
    localReply: buildSiteReply(message, replyTarget),
  };
}

export function buildSiteReply(message, replyTarget = null) {
  const text = normalizeText(message);

  if (!text) {
    return `اسألني عن ${EDIT_THIS_CONTENT.projectName}، مثل: شو هدف الموقع؟ وين بعمل حساب؟ وين بلاقي المواد؟`;
  }

  if (isOutOfScope(text)) {
    return EDIT_THIS_CONTENT.boundaryReply;
  }

  const section = findSection(text);
  if (section) {
    return `${section.answer}\n\nالرابط: ${section.route}`;
  }

  const feature = findFeatureAnswer(text);
  if (feature) {
    return feature.answer;
  }

  if (replyTarget && hasSiteIntent(text)) {
    return 'وضحلي اسم الصفحة أو القسم اللي بتسأل عنه، مثل: تسجيل الدخول، إنشاء حساب، المواد الدراسية، الأسئلة الشائعة، خطط الدراسة، أو امتحان الطالب.';
  }

  if (!hasSiteIntent(text)) {
    return EDIT_THIS_CONTENT.boundaryReply;
  }

  return [
    `بقدر أساعدك بمعلومات عامة عن ${EDIT_THIS_CONTENT.projectName} للزوار والطلاب.`,
    '',
    'اسألني مثلا عن:',
    '1. شو هدف الموقع؟',
    '2. وين بعمل حساب أو بسجل دخول؟',
    '3. وين بلاقي المواد أو صفحات الطالب؟',
    '4. شو بوفر الموقع للطالب؟',
    '5. وين الأسئلة الشائعة أو صفحة الامتحان؟',
  ].join('\n');
}

function findSection(text) {
  return EDIT_THIS_CONTENT.siteSections.find((section) =>
    section.aliases.some((alias) => text.includes(normalizeText(alias))),
  );
}

function findFeatureAnswer(text) {
  return EDIT_THIS_CONTENT.featureAnswers.find((item) =>
    item.aliases.some((alias) => text.includes(normalizeText(alias))),
  );
}

function hasSiteIntent(text) {
  return EDIT_THIS_CONTENT.siteKeywords.some((word) => text.includes(normalizeText(word)));
}

function isOutOfScope(text) {
  return (
    EDIT_THIS_CONTENT.outOfScopeKeywords.some((word) => text.includes(normalizeText(word))) &&
    !isAllowedVisitorQuestion(text)
  );
}

function isAllowedVisitorQuestion(text) {
  return hasSiteIntent(text) && !/(ادمن|أدمن|admin|لوحة التحكم|ادارة|إدارة)/.test(text);
}

function normalizeText(value) {
  return String(value || '')
    .trim()
    .replace(/[أإآ]/g, 'ا')
    .replace(/ى/g, 'ي')
    .replace(/ة/g, 'ه')
    .replace(/[ًٌٍَُِّْ]/g, '')
    .replace(/\s+/g, ' ')
    .toLowerCase();
}
