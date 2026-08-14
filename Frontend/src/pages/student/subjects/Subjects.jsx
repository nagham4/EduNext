import { useEffect, useMemo, useRef, useState } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { useLocation } from "react-router-dom";
import chatbotIcon from "../../../assets/robot.png";
import {
  BookMarked,
  Atom,
  Languages,
  FlaskConical,
  BookOpen,
  Globe,
  ChevronLeft,
  ChevronDown,
  Play,
  CheckCircle2,
  Send,
  Sparkles,
  X,
  ImagePlus,
  Trash2,
  Calculator,
  GraduationCap,
  Clock,
  FileDown,
  Lightbulb,
  PenTool,
  Loader2,
  Check,
} from "lucide-react";
import DashboardLayout from "../../../components/DashboardLayout";
import AchievementPopup from "../../../components/achievement-popup/AchievementPopup.jsx";
import { downloadLessonPdf } from "../../../utils/downloadLessonPdf.js";
import { API_BASE_URL } from "@/config/api";

const iconMap = {
  BookMarked,
  Atom,
  Languages,
  FlaskConical,
  BookOpen,
  Globe,
};

const quickActions = [
  { icon: Lightbulb, label: "حل أمثلة" },
  { icon: PenTool, label: "تبسيط التعريف" },
  { icon: Sparkles, label: "شرح سريع" },
];

const subjectChatbots = {
  math: {
    title: "مساعد الرياضيات الذكي",
    subtitle: "رياضيات | حل بالخطوات",
    intro: "أرسل سؤالك أو صورة المسألة، وسأرتب لك الحل مثل دفتر الرياضيات: المعطيات، القانون، الخطوات، والجواب النهائي.",
    placeholder: "اكتب سؤال رياضيات أو أرفق صورة...",
    searchingText: "أحلل السؤال وأبني الحل خطوة بخطوة...",
    accent: "#2563eb",
    icon: Calculator,
    actions: [
      { icon: Lightbulb, label: "حل مثال رياضيات مع الشرح" },
      { icon: PenTool, label: "اشرح القانون المستخدم" },
      { icon: Sparkles, label: "اعمل أسئلة تدريبية مع الإجابات" },
    ],
  },
  english: {
    title: "English Mentor AI",
    subtitle: "English | grammar and practice",
    intro: "Send a question or a photo from the lesson. I can explain grammar, answer reading questions, and create practice questions with model answers.",
    placeholder: "Ask in English or attach an image...",
    searchingText: "Reading your question and preparing a clear answer...",
    accent: "#0ea5e9",
    icon: GraduationCap,
    actions: [
      { icon: Lightbulb, label: "Explain grammar" },
      { icon: PenTool, label: "Give vocabulary" },
      { icon: Sparkles, label: "Create questions with answers" },
    ],
  },
};

const getChatStorageKey = (lesson, subject) => {
  if (!lesson?.lessonId && !lesson?.title) return "";

  const subjectPart = lesson?.subjectId || subject?.id || "subject";
  const lessonPart =
    lesson?.lessonId ||
    `${lesson?.lessonNumber || "lesson"}-${lesson?.title || "untitled"}`;

  return `edunext-ai-chat-history:${subjectPart}:${lessonPart}`;
};

const persistChatHistory = (key, messages) => {
  if (!key) return;

  try {
    const compactMessages = messages.slice(-80).map((message) => ({
      role: message.role,
      text: message.text,
      imageName: message.imageName || "",
      hasImage: Boolean(message.image || message.hasImage),
      createdAt: message.createdAt || new Date().toISOString(),
    }));

    localStorage.setItem(key, JSON.stringify(compactMessages));
  } catch (error) {
    console.warn("Unable to save chat history", error);
  }
};

const getSubjectChatbotConfig = (...labels) => {
  const normalized = labels.filter(Boolean).join(" ").toLowerCase();

  if (
    normalized.includes("math") ||
    normalized.includes("mathematics") ||
    normalized.includes("رياض") ||
    normalized.includes("\u0631\u064a\u0627\u0636")
  ) {
    return { key: "math", ...subjectChatbots.math };
  }

  if (
    normalized.includes("english") ||
    normalized.includes("انج") ||
    normalized.includes("إنج") ||
    normalized.includes("\u0627\u0646\u062c") ||
    normalized.includes("\u0625\u0646\u062c")
  ) {
    return { key: "english", ...subjectChatbots.english };
  }

  if (normalized.includes("رياض") || normalized.includes("math")) {
    return { key: "math", ...subjectChatbots.math };
  }

  if (
    normalized.includes("انج") ||
    normalized.includes("إنج") ||
    normalized.includes("english")
  ) {
    return { key: "english", ...subjectChatbots.english };
  }

  return null;
};

const getAuthToken = () => {
  return sessionStorage.getItem("token") || localStorage.getItem("token");
};

const hasArabicText = (text = "") => /[\u0600-\u06FF]/.test(text);

const normalizeChatText = (text = "") => {
  return text
    .replace(/\r\n/g, "\n")
    .replace(/\s*\*\*([^*]+)\*\*/g, "\n**$1** ")
    .replace(/\s+([،؛:؟!?])/g, "$1")
    .trim();
};

const splitChatParagraphs = (text = "") => {
  const normalized = normalizeChatText(text);
  const rawLines = normalized.split("\n").map((line) => line.trim()).filter(Boolean);

  if (rawLines.length > 1) return rawLines;

  return normalized
    .split(/(?<=[.!؟!؛])\s+/)
    .map((line) => line.trim())
    .filter(Boolean);
};

const renderChatInline = (text) => {
  const parts = text.split(/(\*\*[^*]+\*\*)/g).filter(Boolean);

  return parts.map((part, index) => {
    if (part.startsWith("**") && part.endsWith("**")) {
      return <strong key={index}>{part.slice(2, -2).trim()}</strong>;
    }

    return <span key={index}>{part}</span>;
  });
};

const ChatMessageContent = ({ text }) => {
  if (/^https?:\/\//i.test(text)) {
    return (
      <a href={text} target="_blank" rel="noreferrer">
        {text}
      </a>
    );
  }

  const lines = splitChatParagraphs(text);
  const listItems = [];
  const blocks = [];

  const flushList = () => {
    if (!listItems.length) return;
    blocks.push(
      <ul className="chatbot-message-list" key={`list-${blocks.length}`}>
        {listItems.splice(0).map((item, index) => (
          <li key={index}>{renderChatInline(item)}</li>
        ))}
      </ul>
    );
  };

  lines.forEach((line) => {
    const bulletMatch = line.match(/^[-*•]\s+(.+)$/);
    const numberMatch = line.match(/^\d+[.)]\s+(.+)$/);
    const headingMatch = line.match(/^\*\*([^*]+)\*\*:?\s*(.*)$/);

    if (bulletMatch || numberMatch) {
      listItems.push((bulletMatch || numberMatch)[1]);
      return;
    }

    flushList();

    if (headingMatch) {
      blocks.push(
        <div className="chatbot-message-section" key={`section-${blocks.length}`}>
          <strong>{headingMatch[1].trim()}</strong>
          {headingMatch[2] ? <p>{renderChatInline(headingMatch[2].trim())}</p> : null}
        </div>
      );
      return;
    }

    blocks.push(<p key={`p-${blocks.length}`}>{renderChatInline(line)}</p>);
  });

  flushList();

  return <div className="chatbot-message-text">{blocks}</div>;
};

const normalizeSubject = (subject) => ({
  id: subject.id || subject.Id,
  title: subject.title || subject.Title || "",
  desc: subject.desc || subject.Desc || "",
  progress: subject.progress ?? subject.Progress ?? 0,
  color: subject.color || subject.Color || "blue",
  lessons: subject.lessons ?? subject.Lessons ?? 0,
  completed: subject.completed ?? subject.Completed ?? 0,
  iconKey: subject.iconKey || subject.IconKey || "BookOpen",
});

const normalizeSubjectDetails = (subject) => ({
  id: subject.id || subject.Id,
  title: subject.title || subject.Title || "",
  desc: subject.desc || subject.Desc || "",
  progress: subject.progress ?? subject.Progress ?? 0,
  color: subject.color || subject.Color || "blue",
  lessons: subject.lessons ?? subject.Lessons ?? 0,
  completed: subject.completed ?? subject.Completed ?? 0,
  iconKey: subject.iconKey || subject.IconKey || "BookOpen",
  units: (subject.units || subject.Units || []).map((unit) => ({
    id: unit.id || unit.Id,
    title: unit.title || unit.Title || "",
    orderNumber: unit.orderNumber ?? unit.OrderNumber ?? 0,
    lessons: (unit.lessons || unit.Lessons || []).map((lesson, index) => ({
      displayNumber: index + 1,
      id: lesson.id || lesson.Id || index + 1,
      lessonId: lesson.lessonId || lesson.LessonId,
      title: lesson.title || lesson.Title || "",
      duration: lesson.duration || lesson.Duration || "غير مكتمل",
      completed: lesson.completed ?? lesson.Completed ?? false,
    })),
  })),
});

const normalizeLessonDetails = (lesson) => ({
  lessonId: lesson.lessonId || lesson.LessonId,
  subjectId: lesson.subjectId || lesson.SubjectId,
  subjectTitle: lesson.subjectTitle || lesson.SubjectTitle || "",
  lessonNumber: lesson.lessonNumber ?? lesson.LessonNumber ?? 1,
  totalLessons: lesson.totalLessons ?? lesson.TotalLessons ?? 1,
  title: lesson.title || lesson.Title || "",
  duration: lesson.duration || lesson.Duration || "غير مكتمل",
  completed: lesson.completed ?? lesson.Completed ?? false,
  videoUrl: lesson.videoUrl || lesson.VideoUrl || "",
  explanation: lesson.explanation || lesson.Explanation || "",
  summary: lesson.summary || lesson.Summary || "",
  pdfUrl: lesson.pdfUrl || lesson.PdfUrl || "",
  resourcesUrl: lesson.resourcesUrl || lesson.ResourcesUrl || "",
});

const getEmbedVideoUrl = (url) => {
  if (!url) return "";

  try {
    const parsedUrl = new URL(url);

    if (parsedUrl.hostname.includes("youtube.com")) {
      const videoId = parsedUrl.searchParams.get("v");

      if (videoId) {
        return `https://www.youtube.com/embed/${videoId}`;
      }

      if (parsedUrl.pathname.startsWith("/shorts/")) {
        const videoIdFromShorts = parsedUrl.pathname.split("/shorts/")[1];

        if (videoIdFromShorts) {
          return `https://www.youtube.com/embed/${videoIdFromShorts}`;
        }
      }

      if (parsedUrl.pathname.startsWith("/embed/")) {
        return url;
      }
    }

    if (parsedUrl.hostname.includes("youtu.be")) {
      const videoId = parsedUrl.pathname.replace("/", "").split("?")[0];

      if (videoId) {
        return `https://www.youtube.com/embed/${videoId}`;
      }
    }

    return url;
  } catch {
    return url;
  }
};

const splitLessonText = (text) => {
  if (!text) return [];

  return String(text)
    .split(/\n+/)
    .map((line) => line.trim())
    .filter(Boolean);
};

const normalizeSummaryItems = (summary) => {
  if (!summary) return [];

  if (Array.isArray(summary)) {
    return summary.map((item) => String(item).trim()).filter(Boolean);
  }

  return String(summary)
    .split(/\n|-/)
    .map((item) => item.trim())
    .filter(Boolean);
};

const normalizeUnlockedAchievement = (achievement) => {
  if (!achievement) return null;

  const title = achievement.title || achievement.Title || "";
  const description = achievement.description || achievement.Description || "";

  return {
    type:
      achievement.type ||
      achievement.Type ||
      achievement.conditionType ||
      achievement.ConditionType ||
      "lessons",
    title,
    description,
    reward: achievement.reward || achievement.Reward || "+50 نقطة",
  };
};

const Subjects = () => {
  const token = getAuthToken();
  const location = useLocation();
  const pendingNavigationRef = useRef(location.state || null);
  const lessonOpenedAtRef = useRef(null);

  const [subjects, setSubjects] = useState([]);
  const [subjectsLoading, setSubjectsLoading] = useState(true);
  const [subjectsError, setSubjectsError] = useState("");

  const [selectedSubjectId, setSelectedSubjectId] = useState(null);
  const [subjectDetails, setSubjectDetails] = useState(null);
  const [subjectDetailsLoading, setSubjectDetailsLoading] = useState(false);
  const [subjectDetailsError, setSubjectDetailsError] = useState("");

  const [selectedLessonId, setSelectedLessonId] = useState(null);
  const [lessonDetails, setLessonDetails] = useState(null);
  const [lessonLoading, setLessonLoading] = useState(false);
  const [lessonError, setLessonError] = useState("");
  const [completionLoading, setCompletionLoading] = useState(false);
  const [completionError, setCompletionError] = useState("");

  const [achievementPopupOpen, setAchievementPopupOpen] = useState(false);
  const [unlockedAchievement, setUnlockedAchievement] = useState(null);

  const [chatOpen, setChatOpen] = useState(false);
  const [chatMsg, setChatMsg] = useState("");
  const [chatMessages, setChatMessages] = useState([]);
  const [chatImage, setChatImage] = useState(null);
  const [chatHistoryOpen, setChatHistoryOpen] = useState(false);
  const [chatSending, setChatSending] = useState(false);
  const chatFileInputRef = useRef(null);
  const chatMessagesRef = useRef(null);
  const skipNextChatPersistRef = useRef(false);

  const [openUnits, setOpenUnits] = useState({});

  const currentSubject = useMemo(() => {
    if (subjectDetails) return subjectDetails;
    return subjects.find((s) => s.id === selectedSubjectId) || null;
  }, [subjects, selectedSubjectId, subjectDetails]);

  const allSubjectLessons = useMemo(() => {
    let counter = 1;

    return (subjectDetails?.units || []).flatMap((unit) =>
      (unit.lessons || []).map((lesson) => ({
        ...lesson,
        displayNumber: counter++,
      }))
    );
  }, [subjectDetails]);

  const currentLessonIndex = useMemo(() => {
    if (!selectedLessonId) return -1;

    return allSubjectLessons.findIndex(
      (lesson) => lesson.lessonId === selectedLessonId
    );
  }, [allSubjectLessons, selectedLessonId]);

  useEffect(() => {
    if (!lessonDetails?.lessonId) return;

    const historyKey = getChatStorageKey(lessonDetails, currentSubject);
    skipNextChatPersistRef.current = true;

    try {
      const savedMessages = JSON.parse(localStorage.getItem(historyKey) || "[]");
      setChatMessages(Array.isArray(savedMessages) ? savedMessages : []);
    } catch {
      setChatMessages([]);
    }

    setChatMsg("");
    setChatImage(null);
    setChatHistoryOpen(false);
  }, [lessonDetails?.lessonId, currentSubject?.id]);

  useEffect(() => {
    const messagesElement = chatMessagesRef.current;
    if (!messagesElement) return;

    messagesElement.scrollTop = messagesElement.scrollHeight;
  }, [chatMessages, chatSending, chatOpen]);

  useEffect(() => {
    if (!lessonDetails?.lessonId) return;
    if (skipNextChatPersistRef.current) {
      skipNextChatPersistRef.current = false;
      return;
    }

    persistChatHistory(getChatStorageKey(lessonDetails, currentSubject), chatMessages);
  }, [chatMessages, lessonDetails?.lessonId, currentSubject?.id]);

  const toggleUnit = (unitId) => {
    setOpenUnits((prev) => ({
      ...prev,
      [unitId]: !prev[unitId],
    }));
  };

  const updateLocalLessonCompletion = (lessonId, completed) => {
    setSubjectDetails((previous) => {
      if (!previous) return previous;

      const updatedUnits = previous.units.map((unit) => ({
        ...unit,
        lessons: unit.lessons.map((lesson) =>
          lesson.lessonId === lessonId
            ? {
              ...lesson,
              completed,
            }
            : lesson
        ),
      }));

      const allLessons = updatedUnits.flatMap((unit) => unit.lessons);
      const completedCount = allLessons.filter((lesson) => lesson.completed).length;
      const totalLessons = allLessons.length;

      const progress =
        totalLessons === 0 ? 0 : Math.round((completedCount * 100) / totalLessons);

      const updatedSubject = {
        ...previous,
        units: updatedUnits,
        completed: completedCount,
        lessons: totalLessons,
        progress,
      };

      setSubjects((prevSubjects) =>
        prevSubjects.map((subject) =>
          subject.id === updatedSubject.id
            ? {
              ...subject,
              completed: completedCount,
              lessons: totalLessons,
              progress,
            }
            : subject
        )
      );

      return updatedSubject;
    });

    setLessonDetails((previous) =>
      previous
        ? {
          ...previous,
          completed,
        }
        : previous
    );
  };

  const showAchievementIfExists = (data) => {
    const newAchievements =
      data?.newAchievements ||
      data?.NewAchievements ||
      data?.unlockedAchievements ||
      data?.UnlockedAchievements ||
      [];

    if (!Array.isArray(newAchievements) || newAchievements.length === 0) {
      return;
    }

    const normalized = normalizeUnlockedAchievement(newAchievements[0]);

    if (!normalized) return;

    setUnlockedAchievement(normalized);
    setAchievementPopupOpen(true);
  };

  const markLessonCompletion = async (completed) => {
    if (!token || !selectedSubjectId || !selectedLessonId) return;

    try {
      setCompletionLoading(true);
      setCompletionError("");
      const durationSeconds =
        completed && lessonOpenedAtRef.current
          ? Math.max(1, Math.round((Date.now() - lessonOpenedAtRef.current) / 1000))
          : null;

      const response = await fetch(
        `${API_BASE_URL}/api/student/subjects/${selectedSubjectId}/lessons/${selectedLessonId}/completion`,
        {
          method: "PATCH",
          headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            completed,
            durationSeconds,
          }),
        }
      );

      const rawText = await response.text();

      let data;
      try {
        data = JSON.parse(rawText);
      } catch {
        data = null;
      }

      if (!response.ok) {
        setCompletionError(data?.message || "تعذر تحديث حالة الدرس.");
        return;
      }

      const updatedLesson = data?.lesson || data?.Lesson || data;
      const normalizedLesson = normalizeLessonDetails(updatedLesson);

      setLessonDetails((previous) => ({
        ...(previous || normalizedLesson),
        ...normalizedLesson,
        completed,
      }));
      lessonOpenedAtRef.current = Date.now();

      updateLocalLessonCompletion(selectedLessonId, completed);

      if (completed) {
        showAchievementIfExists(data);
      }
    } catch (error) {
      console.error(error);
      setCompletionError("تعذر الاتصال بالسيرفر.");
    } finally {
      setCompletionLoading(false);
    }
  };

  useEffect(() => {
    const fetchSubjects = async () => {
      if (!token) {
        window.location.href = "/login";
        return;
      }

      try {
        setSubjectsLoading(true);
        setSubjectsError("");

        const response = await fetch(`${API_BASE_URL}/api/student/subjects`, {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        });

        const rawText = await response.text();

        let data;
        try {
          data = JSON.parse(rawText);
        } catch {
          data = [];
        }

        if (!response.ok) {
          setSubjectsError(data?.message || "فشل تحميل المواد");
          return;
        }

        setSubjects((Array.isArray(data) ? data : []).map(normalizeSubject));
      } catch (error) {
        console.error(error);
        setSubjectsError("تعذر الاتصال بالسيرفر");
      } finally {
        setSubjectsLoading(false);
      }
    };

    fetchSubjects();
  }, [token]);

  const openSubject = async (subjectId, lessonIdToOpen = null) => {
    if (!token) return;

    try {
      setSelectedSubjectId(subjectId);
      setSelectedLessonId(null);
      lessonOpenedAtRef.current = null;
      setLessonDetails(null);
      setSubjectDetailsLoading(true);
      setSubjectDetailsError("");
      setCompletionError("");
      setChatOpen(false);
      setChatMessages([]);
      setChatSending(false);
      setOpenUnits({});

      const response = await fetch(`${API_BASE_URL}/api/student/subjects/${subjectId}`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      const rawText = await response.text();

      let data;
      try {
        data = JSON.parse(rawText);
      } catch {
        data = null;
      }

      if (!response.ok) {
        setSubjectDetailsError(data?.message || "فشل تحميل تفاصيل المادة");
        return;
      }

      const normalized = normalizeSubjectDetails(data);
      setSubjectDetails(normalized);

      const firstUnitId = normalized.units?.[0]?.id;
      if (firstUnitId) {
        setOpenUnits({ [firstUnitId]: true });
      }

      if (lessonIdToOpen) {
        const lessonUnit = normalized.units?.find((unit) =>
          (unit.lessons || []).some((lesson) => lesson.lessonId === lessonIdToOpen)
        );

        if (lessonUnit?.id) {
          setOpenUnits((prev) => ({ ...prev, [lessonUnit.id]: true }));
        }

        await openLesson(lessonIdToOpen, subjectId);
      }
    } catch (error) {
      console.error(error);
      setSubjectDetailsError("تعذر الاتصال بالسيرفر");
    } finally {
      setSubjectDetailsLoading(false);
    }
  };

  const openLesson = async (lessonId, subjectIdOverride = selectedSubjectId) => {
    if (!token || !subjectIdOverride) return;

    try {
      setSelectedLessonId(lessonId);
      lessonOpenedAtRef.current = Date.now();
      setLessonDetails(null);
      setLessonLoading(true);
      setLessonError("");
      setCompletionError("");
      setChatOpen(false);
      setChatMsg("");
      setChatImage(null);
      setChatHistoryOpen(false);
      setChatMessages([]);
      setChatSending(false);

      const response = await fetch(
        `${API_BASE_URL}/api/student/subjects/${subjectIdOverride}/lessons/${lessonId}`,
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );

      const rawText = await response.text();

      let data;
      try {
        data = JSON.parse(rawText);
      } catch {
        data = null;
      }

      if (!response.ok) {
        setLessonError(data?.message || "فشل تحميل تفاصيل الدرس");
        return;
      }

      setLessonDetails(normalizeLessonDetails(data));
    } catch (error) {
      console.error(error);
      setLessonError("تعذر الاتصال بالسيرفر");
    } finally {
      setLessonLoading(false);
    }
  };

  useEffect(() => {
    const pendingNavigation = pendingNavigationRef.current;

    if (!pendingNavigation || subjectsLoading || subjects.length === 0) {
      return;
    }

    const targetSubjectId = pendingNavigation.subjectId;
    const targetLessonId = pendingNavigation.lessonId;

    if (!targetSubjectId) {
      pendingNavigationRef.current = null;
      return;
    }

    pendingNavigationRef.current = null;
    openSubject(targetSubjectId, targetLessonId);
  }, [subjectsLoading, subjects]);

  const handleChatImageChange = (event) => {
    const file = event.target.files?.[0];
    if (!file) return;

    if (!file.type.startsWith("image/")) {
      setChatMessages((prev) => [
        ...prev,
        { role: "ai", text: "الملف المرفق لازم يكون صورة.", createdAt: new Date().toISOString() },
      ]);
      event.target.value = "";
      return;
    }

    if (file.size > 8 * 1024 * 1024) {
      setChatMessages((prev) => [
        ...prev,
        { role: "ai", text: "حجم الصورة كبير. اختاري صورة أصغر من 8MB.", createdAt: new Date().toISOString() },
      ]);
      event.target.value = "";
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      setChatImage({
        data: reader.result,
        mimeType: file.type,
        name: file.name,
      });
    };
    reader.readAsDataURL(file);
  };

  const clearChatHistory = () => {
    const historyKey = getChatStorageKey(lessonDetails, currentSubject);
    if (historyKey) {
      localStorage.removeItem(historyKey);
    }
    setChatMessages([]);
  };

  const sendChat = async () => {
    if ((!chatMsg.trim() && !chatImage) || !lessonDetails || chatSending) return;

    const msg = chatMsg.trim();
    const chatbotConfig = getSubjectChatbotConfig(
      lessonDetails.subjectTitle,
      currentSubject?.title,
      currentSubject?.desc,
      currentSubject?.iconKey
    );
    const imageToSend = chatImage;

    if (!chatbotConfig) return;

    const historyKey = getChatStorageKey(lessonDetails, currentSubject);
    const userMessage = {
      role: "user",
      text: msg || "حلل الصورة المرفقة وأجب عن السؤال الظاهر فيها.",
      image: imageToSend?.data || null,
      imageName: imageToSend?.name || "",
      hasImage: Boolean(imageToSend),
      createdAt: new Date().toISOString(),
    };

    setChatMessages((prev) => {
      const next = [...prev, userMessage];
      persistChatHistory(historyKey, next);
      return next;
    });
    setChatMsg("");
    setChatImage(null);
    if (chatFileInputRef.current) {
      chatFileInputRef.current.value = "";
    }
    setChatSending(true);

    try {
      const response = await fetch(`${API_BASE_URL}/api/student/ai-chatbot/chat`, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          message: msg,
          subjectTitle: lessonDetails.subjectTitle || currentSubject?.title || "",
          lessonTitle: lessonDetails.title,
          subjectKey: chatbotConfig.key,
          imageData: imageToSend?.data || null,
          imageMimeType: imageToSend?.mimeType || null,
        }),
      });

      const rawText = await response.text();

      let data;
      try {
        data = JSON.parse(rawText);
      } catch {
        data = null;
      }

      if (!response.ok) {
        throw new Error(data?.message || "تعذر إرسال السؤال للشات بوت.");
      }

      setChatMessages((prev) => [
        ...prev,
        {
          role: "ai",
          text: data?.reply || "لم يصل رد واضح من الشات بوت.",
        },
      ]);
    } catch (error) {
      setChatMessages((prev) => [
        ...prev,
        {
          role: "ai",
          text:
            error.message ||
            "تعذر الاتصال بالشات بوت. تأكد أن خدمة Python تعمل ثم حاول مرة أخرى.",
        },
      ]);
    } finally {
      setChatSending(false);
    }
  };

  const renderIcon = (iconKey, size = 24) => {
    const IconComponent = iconMap[iconKey] || BookOpen;
    return <IconComponent size={size} />;
  };

  if (selectedLessonId && currentSubject) {
    const lessons = allSubjectLessons;
    const lesson = lessonDetails;
    const resourceLink = lesson?.resourcesUrl || lesson?.pdfUrl || "";
    const chatbotConfig = getSubjectChatbotConfig(
      lesson?.subjectTitle,
      currentSubject?.title,
      currentSubject?.desc,
      currentSubject?.iconKey
    );
    const ChatbotIcon = chatbotConfig?.icon || Sparkles;

    const nextLesson =
      currentLessonIndex >= 0 && currentLessonIndex < lessons.length - 1
        ? lessons[currentLessonIndex + 1]
        : null;

    const prevLesson =
      currentLessonIndex > 0 ? lessons[currentLessonIndex - 1] : null;

    return (
      <DashboardLayout
        title={lesson?.title || "تفاصيل الدرس"}
        subtitle={currentSubject.title}
        hideSearch
      >
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ duration: 0.3 }}
        >
          <button
            className="btn btn-ghost btn-sm"
            style={{ marginBottom: "1rem" }}
            onClick={() => {
              setSelectedLessonId(null);
              lessonOpenedAtRef.current = null;
              setLessonDetails(null);
              setLessonError("");
              setCompletionError("");
              setChatOpen(false);
              setChatMessages([]);
              setChatSending(false);
            }}
          >
            العودة للدروس ←
          </button>

          {lessonLoading ? (
            <div className="card" style={{ padding: "2rem", textAlign: "center" }}>
              <Loader2 className="animate-spin" style={{ margin: "0 auto 1rem" }} />
              جاري تحميل الدرس...
            </div>
          ) : lessonError ? (
            <div
              className="card"
              style={{ padding: "2rem", textAlign: "center", color: "red" }}
            >
              {lessonError}
            </div>
          ) : lesson ? (
            <div className="lesson-detail-single">
              <div className="lesson-main-content">
                <div className="lesson-video-container card">
                  {lesson.videoUrl ? (
                    <div
                      style={{
                        position: "relative",
                        width: "100%",
                        aspectRatio: "16 / 9",
                        borderRadius: "1rem",
                        overflow: "hidden",
                        background: "#111827",
                      }}
                    >
                      <iframe
                        src={getEmbedVideoUrl(lesson.videoUrl)}
                        title={lesson.title}
                        style={{
                          width: "100%",
                          height: "100%",
                          border: "none",
                        }}
                        allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share"
                        allowFullScreen
                      />
                    </div>
                  ) : (
                    <div className="lesson-video-placeholder">
                      <div className="lesson-play-overlay">
                        <button className="video-play-btn hero-gradient">
                          <Play size={32} />
                        </button>
                      </div>

                      <div className="lesson-video-info">
                        <span>📹 {lesson.title}</span>
                        <span>غير متاح</span>
                      </div>

                      <div className="lesson-video-progress-bar">
                        <div
                          className="lesson-video-progress-fill"
                          style={{
                            width: lesson.completed ? "100%" : "0%",
                            background: lesson.completed ? "#22c55e" : undefined,
                          }}
                        />
                      </div>
                    </div>
                  )}
                </div>

                <div className="card" style={{ padding: "1.5rem", marginBottom: "1rem" }}>
                  <div
                    style={{
                      display: "flex",
                      alignItems: "center",
                      justifyContent: "space-between",
                      marginBottom: "1rem",
                      gap: "1rem",
                    }}
                  >
                    <h3 className="card-title-dash">📹 معلومات الدرس</h3>

                    <span
                      className="card-badge"
                      style={{
                        background: lesson.completed ? "#dcfce7" : "#fee2e2",
                        color: lesson.completed ? "#15803d" : "#dc2626",
                      }}
                    >
                      {lesson.completed ? "مكتمل" : "غير مكتمل"}
                    </span>
                  </div>

                  <p
                    style={{
                      color: "var(--muted-foreground)",
                      fontSize: "0.875rem",
                      marginBottom: "0.75rem",
                    }}
                  >
                    المادة: {lesson.subjectTitle} &nbsp;•&nbsp; الدرس{" "}
                    {currentLessonIndex >= 0
                      ? currentLessonIndex + 1
                      : lesson.lessonNumber}{" "}
                    من {allSubjectLessons.length || lesson.totalLessons}
                  </p>

                  <div style={{ display: "flex", gap: "0.5rem", flexWrap: "wrap" }}>
                    <button
                      className="btn btn-outline btn-sm"
                      onClick={() => {
                        downloadLessonPdf({
                          lessonTitle: lesson.title,
                          subjectTitle: lesson.subjectTitle,
                          lessonNumber:
                            currentLessonIndex >= 0
                              ? currentLessonIndex + 1
                              : lesson.lessonNumber,
                          totalLessons: allSubjectLessons.length || lesson.totalLessons,
                          duration: lesson.duration,
                          explanation: lesson.explanation,
                          summary: lesson.summary,
                        });
                      }}
                    >
                      <FileDown size={14} /> تحميل الملخص
                    </button>

                    {resourceLink ? (
                      <a
                        className="btn btn-outline btn-sm"
                        href={resourceLink}
                        target="_blank"
                        rel="noreferrer"
                      >
                        <BookOpen size={14} /> المصادر المرفقة
                      </a>
                    ) : (
                      <button className="btn btn-outline btn-sm" disabled>
                        <BookOpen size={14} /> المصادر المرفقة
                      </button>
                    )}
                  </div>
                </div>

                <div className="card lesson-scroll-card" style={{ padding: "1.5rem", marginBottom: "1rem" }}>
                  <h3 className="card-title-dash" style={{ marginBottom: "1rem" }}>
                    📖 شرح الدرس
                  </h3>

                  {splitLessonText(lesson.explanation).length > 0 ? (
                    <div
                      className="lesson-scroll-panel lesson-explanation-scroll"
                      style={{
                        display: "flex",
                        flexDirection: "column",
                        gap: "0.85rem",
                        maxHeight: "420px",
                        overflowY: "scroll",
                        overflowX: "hidden",
                        paddingLeft: "0.55rem",
                        color: "var(--foreground)",
                        fontSize: "0.95rem",
                        lineHeight: 2,
                        textAlign: "right",
                      }}
                    >
                      {splitLessonText(lesson.explanation).map((paragraph, index) => (
                        <p
                          key={index}
                          style={{
                            margin: 0,
                            color: "#334155",
                          }}
                        >
                          {paragraph}
                        </p>
                      ))}
                    </div>
                  ) : (
                    <p style={{ color: "var(--muted-foreground)" }}>
                      لا يوجد شرح متاح لهذا الدرس حالياً.
                    </p>
                  )}
                </div>

                <div className="card lesson-scroll-card" style={{ padding: "1.5rem", marginBottom: "1rem" }}>
                  <h3 className="card-title-dash" style={{ marginBottom: "1rem" }}>
                    📝 ملخص الدرس
                  </h3>

                  {normalizeSummaryItems(lesson.summary).length > 0 ? (
                    <div
                      className="lesson-scroll-panel lesson-summary-scroll"
                      style={{
                        display: "flex",
                        flexDirection: "column",
                        gap: "0.65rem",
                        maxHeight: "320px",
                        overflowY: "scroll",
                        overflowX: "hidden",
                        paddingLeft: "0.55rem",
                      }}
                    >
                      {normalizeSummaryItems(lesson.summary).map((point, index) => (
                        <div
                          key={index}
                          style={{
                            display: "flex",
                            alignItems: "flex-start",
                            gap: "0.65rem",
                            padding: "0.75rem 0.9rem",
                            borderRadius: "0.75rem",
                            background: "#f8fafc",
                            border: "1px solid #e2e8f0",
                            fontSize: "0.92rem",
                            lineHeight: 1.8,
                            color: "#334155",
                          }}
                        >
                          <CheckCircle2
                            size={17}
                            style={{
                              color: "#2563eb",
                              flexShrink: 0,
                              marginTop: "0.25rem",
                            }}
                          />
                          <span>{point.replace(/^•\s*/, "")}</span>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <p style={{ color: "var(--muted-foreground)" }}>
                      لا يوجد ملخص متاح.
                    </p>
                  )}
                </div>

                <div
                  className="card"
                  style={{
                    padding: "1.25rem 1.5rem",
                    marginBottom: "1rem",
                    border: lesson.completed ? "1px solid #86efac" : "1px solid #bfdbfe",
                    background: lesson.completed ? "#ecfdf5" : "#eff6ff",
                  }}
                >
                  <div
                    style={{
                      display: "flex",
                      alignItems: "center",
                      justifyContent: "space-between",
                      gap: "1rem",
                      flexWrap: "wrap",
                    }}
                  >
                    <div>
                      <h3
                        style={{
                          margin: 0,
                          marginBottom: "0.35rem",
                          fontSize: "1rem",
                          fontWeight: 800,
                          color: lesson.completed ? "#15803d" : "#1d4ed8",
                        }}
                      >
                        {lesson.completed
                          ? "تم إنهاء هذا الدرس"
                          : "هل أنهيت دراسة هذا الدرس؟"}
                      </h3>

                      <p
                        style={{
                          margin: 0,
                          fontSize: "0.875rem",
                          color: lesson.completed ? "#166534" : "#475569",
                          lineHeight: 1.7,
                        }}
                      >
                        {lesson.completed
                          ? "تم تسجيل هذا الدرس ضمن تقدمك الدراسي."
                          : "بعد مشاهدة الفيديو وقراءة الشرح والملخص، اضغط على الزر لتسجيل تقدمك."}
                      </p>
                    </div>

                    <button
                      type="button"
                      className="btn btn-sm"
                      disabled={completionLoading}
                      onClick={() => markLessonCompletion(!lesson.completed)}
                      style={{
                        background: lesson.completed ? "#dcfce7" : "#2563eb",
                        color: lesson.completed ? "#15803d" : "#ffffff",
                        border: lesson.completed ? "1px solid #86efac" : "1px solid #2563eb",
                        minWidth: "170px",
                        height: "44px",
                        fontWeight: 800,
                      }}
                    >
                      {completionLoading ? (
                        <>
                          <Loader2 size={14} className="animate-spin" /> جاري الحفظ...
                        </>
                      ) : lesson.completed ? (
                        <>
                          <CheckCircle2 size={16} /> مكتمل
                        </>
                      ) : (
                        <>
                          <Check size={16} /> إنهاء الدرس
                        </>
                      )}
                    </button>
                  </div>

                  {completionError && (
                    <p
                      style={{
                        color: "#dc2626",
                        fontSize: "0.875rem",
                        marginTop: "0.75rem",
                        marginBottom: 0,
                      }}
                    >
                      {completionError}
                    </p>
                  )}
                </div>

                <div
                  style={{
                    display: "flex",
                    justifyContent: "space-between",
                    gap: "0.75rem",
                    marginBottom: "1rem",
                  }}
                >
                  <button
                    className="btn btn-outline btn-sm"
                    disabled={!prevLesson}
                    onClick={() => prevLesson && openLesson(prevLesson.lessonId)}
                    style={{ flex: 1 }}
                  >
                    الدرس السابق →
                  </button>

                  <button
                    className="btn btn-primary btn-sm"
                    disabled={!nextLesson}
                    onClick={() => nextLesson && openLesson(nextLesson.lessonId)}
                    style={{ flex: 1 }}
                  >
                    ← الدرس التالي
                  </button>
                </div>
              </div>
            </div>
          ) : null}

          <motion.button
            className="chatbot-fab"
            onClick={() => setChatOpen(true)}
            initial={{ scale: 0 }}
            animate={{ scale: 1 }}
            exit={{ scale: 0 }}
            whileHover={{ scale: 1.1 }}
            whileTap={{ scale: 0.9 }}
          >
            <img
              src={chatbotIcon}
              alt="Chatbot"
              style={{
                width: "32px",
                height: "32px",
                objectFit: "contain",
              }}
            />
          </motion.button>

          <AnimatePresence>
            {chatOpen && lesson && chatbotConfig && (
              <motion.div
                className="chatbot-panel"
                initial={{ opacity: 0, y: 20, scale: 0.95 }}
                animate={{ opacity: 1, y: 0, scale: 1 }}
                exit={{ opacity: 0, y: 20, scale: 0.95 }}
                transition={{ duration: 0.2 }}
              >
                <div className="chatbot-panel-header" style={{ "--chat-accent": chatbotConfig.accent }}>
                  <div className="chatbot-title-wrap">
                    <span className="chatbot-avatar">
                      <img
                        src={chatbotIcon}
                        alt="Chatbot"
                        style={{
                          width: "28px",
                          height: "28px",
                          objectFit: "contain",
                        }}
                      />
                    </span>
                    <span>
                      <strong>{chatbotConfig.title}</strong>
                      <small>{chatbotConfig.subtitle}</small>
                    </span>
                  </div>

                  <div className="chatbot-header-actions">
                    {chatMessages.length > 0 && (
                      <button
                        onClick={() => setChatHistoryOpen((open) => !open)}
                        className={`chatbot-close-btn ${chatHistoryOpen ? "is-active" : ""}`}
                        title="عرض السجل"
                        type="button"
                      >
                        <Clock size={17} />
                      </button>
                    )}
                    {chatMessages.length > 0 && (
                      <button
                        onClick={clearChatHistory}
                        className="chatbot-close-btn"
                        title="مسح المحادثة"
                        type="button"
                      >
                        <Trash2 size={17} />
                      </button>
                    )}
                    <button
                      onClick={() => setChatOpen(false)}
                      className="chatbot-close-btn"
                      title="إغلاق"
                      type="button"
                    >
                      <X size={18} />
                    </button>
                  </div>
                </div>

                {chatHistoryOpen && (
                  <div className="chatbot-history-panel">
                    <div className="chatbot-history-title">
                      <Clock size={16} />
                      <span>سجل المحادثة</span>
                    </div>
                    {chatMessages.length ? (
                      <div className="chatbot-history-list">
                        {chatMessages.map((message, index) => (
                          <button
                            key={`${message.createdAt || "history"}-${index}`}
                            type="button"
                            className="chatbot-history-item"
                            onClick={() => setChatHistoryOpen(false)}
                          >
                            <strong>
                              {message.role === "ai" ? chatbotConfig.title : "أنت"}
                            </strong>
                            <span>
                              {message.hasImage && !message.image ? "صورة مرفقة - " : ""}
                              {message.text}
                            </span>
                          </button>
                        ))}
                      </div>
                    ) : (
                      <p>لا يوجد سجل لهذا الدرس بعد.</p>
                    )}
                  </div>
                )}

                <div className="chatbot-panel-messages" ref={chatMessagesRef}>
                  <div className="chat-message chat-message-ai">
                    <span className="chatbot-message-label">{chatbotConfig.title}</span>
                    <p>{chatbotConfig.intro}</p>
                  </div>

                  {chatMessages.map((message, index) => (
                    <div
                      key={index}
                      className={`chat-message ${message.role === "ai" ? "chat-message-ai" : "chat-message-user"
                        }`}
                      dir={hasArabicText(message.text) ? "rtl" : "ltr"}
                    >
                      <span className="chatbot-message-label">
                        {message.role === "ai"
                          ? chatbotConfig.title
                          : hasArabicText(message.text)
                            ? "\u0623\u0646\u062a"
                            : "You"}
                      </span>
                      {message.image && (
                        <img
                          className="chatbot-message-image"
                          src={message.image}
                          alt={message.imageName || "صورة مرفقة"}
                        />
                      )}
                      {message.hasImage && !message.image && (
                        <div className="chatbot-image-history-badge">
                          <ImagePlus size={14} />
                          <span>{message.imageName || "صورة مرفقة"}</span>
                        </div>
                      )}
                      <ChatMessageContent text={message.text} />
                    </div>
                  ))}

                  {chatSending && (
                    <div className="chat-message chat-message-ai">
                      <span className="chatbot-message-label">{chatbotConfig.title}</span>
                      <p>{chatbotConfig.searchingText}</p>
                    </div>
                  )}
                </div>

                <div className="chatbot-panel-actions">
                  {(chatbotConfig.actions || quickActions).map((action) => (
                    <button
                      key={action.label}
                      className="lesson-ai-quick-btn"
                      onClick={() => setChatMsg(action.label)}
                    >
                      <action.icon size={14} />
                      {action.label}
                    </button>
                  ))}
                </div>

                {chatImage && (
                  <div className="chatbot-image-preview">
                    <img src={chatImage.data} alt={chatImage.name || "صورة مرفقة"} />
                    <span>{chatImage.name}</span>
                    <button
                      type="button"
                      className="chatbot-close-btn"
                      onClick={() => {
                        setChatImage(null);
                        if (chatFileInputRef.current) {
                          chatFileInputRef.current.value = "";
                        }
                      }}
                      title="إزالة الصورة"
                    >
                      <X size={16} />
                    </button>
                  </div>
                )}

                <div className="chatbot-panel-input">
                  <input
                    ref={chatFileInputRef}
                    type="file"
                    accept="image/*"
                    hidden
                    onChange={handleChatImageChange}
                  />
                  <button
                    className="chatbot-attach-btn"
                    type="button"
                    disabled={chatSending}
                    onClick={() => chatFileInputRef.current?.click()}
                    title="إرفاق صورة"
                  >
                    <ImagePlus size={19} />
                  </button>
                  <input
                    className="input"
                    placeholder={chatbotConfig.placeholder}
                    value={chatMsg}
                    disabled={chatSending}
                    onChange={(event) => setChatMsg(event.target.value)}
                    onKeyDown={(event) => event.key === "Enter" && sendChat()}
                  />

                  <button
                    className="btn btn-primary btn-icon"
                    disabled={chatSending || (!chatMsg.trim() && !chatImage)}
                    style={{ width: "2.75rem", height: "2.75rem", flexShrink: 0 }}
                    onClick={sendChat}
                  >
                    {chatSending ? <Loader2 size={18} className="animate-spin" /> : <Send size={18} />}
                  </button>
                </div>
              </motion.div>
            )}
          </AnimatePresence>

          <AchievementPopup
            open={achievementPopupOpen}
            onClose={() => setAchievementPopupOpen(false)}
            achievement={unlockedAchievement}
          />
        </motion.div>
      </DashboardLayout>
    );
  }

  if (selectedSubjectId) {
    return (
      <DashboardLayout
        title={currentSubject?.title || "تفاصيل المادة"}
        subtitle={currentSubject?.desc || ""}
      >
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ duration: 0.4 }}
        >
          <button
            className="btn btn-ghost btn-sm"
            style={{ marginBottom: "1.5rem" }}
            onClick={() => {
              setSelectedSubjectId(null);
              setSubjectDetails(null);
              setSubjectDetailsError("");
              setSelectedLessonId(null);
              lessonOpenedAtRef.current = null;
              setLessonDetails(null);
              setCompletionError("");
              setChatOpen(false);
              setChatMessages([]);
              setChatSending(false);
              setOpenUnits({});
            }}
          >
            العودة للمواد ←
          </button>

          {subjectDetailsLoading ? (
            <div className="card" style={{ padding: "2rem", textAlign: "center" }}>
              <Loader2 className="animate-spin" style={{ margin: "0 auto 1rem" }} />
              جاري تحميل تفاصيل المادة...
            </div>
          ) : subjectDetailsError ? (
            <div
              className="card"
              style={{ padding: "2rem", textAlign: "center", color: "red" }}
            >
              {subjectDetailsError}
            </div>
          ) : subjectDetails ? (
            <div className="card" style={{ padding: "1.5rem", marginBottom: "1.5rem" }}>
              <div
                style={{
                  display: "flex",
                  justifyContent: "space-between",
                  alignItems: "center",
                  marginBottom: "1rem",
                }}
              >
                <h2 className="card-title-dash">قائمة المحتوى</h2>

                <span className="card-badge">
                  {subjectDetails.completed} / {subjectDetails.lessons} درس
                </span>
              </div>

              <div className="lesson-progress-bar-bg" style={{ marginBottom: "1.5rem" }}>
                <div
                  className="lesson-progress-bar-fill"
                  style={{
                    width: `${subjectDetails.progress}%`,
                    background: subjectDetails.progress === 100 ? "#22c55e" : undefined,
                  }}
                />
              </div>

              <div className="lessons-list">
                {(subjectDetails.units || []).map((unit, unitIndex) => {
                  const isOpen = !!openUnits[unit.id];

                  return (
                    <div key={unit.id || unitIndex} style={{ marginBottom: "1rem" }}>
                      <button
                        type="button"
                        onClick={() => toggleUnit(unit.id)}
                        style={{
                          width: "100%",
                          border: "none",
                          borderRadius: "0.75rem",
                          padding: "1rem 1.25rem",
                          cursor: "pointer",
                          display: "flex",
                          justifyContent: "space-between",
                          alignItems: "center",
                          fontWeight: 700,
                          fontSize: "1rem",
                          background: "var(--primary-light)",
                        }}
                      >
                        <span>{unit.title}</span>
                        {isOpen ? <ChevronDown size={20} /> : <ChevronLeft size={20} />}
                      </button>

                      <AnimatePresence initial={false}>
                        {isOpen && (
                          <motion.div
                            initial={{ height: 0, opacity: 0 }}
                            animate={{ height: "auto", opacity: 1 }}
                            exit={{ height: 0, opacity: 0 }}
                            transition={{ duration: 0.25 }}
                            style={{ overflow: "hidden" }}
                          >
                            <div
                              style={{
                                display: "flex",
                                flexDirection: "column",
                                gap: "0.75rem",
                                marginTop: "0.75rem",
                              }}
                            >
                              {unit.lessons.map((lesson, index) => {
                                const displayNumber =
                                  allSubjectLessons.findIndex(
                                    (item) => item.lessonId === lesson.lessonId
                                  ) + 1;

                                return (
                                  <motion.div
                                    key={lesson.lessonId}
                                    className={`lesson-item ${lesson.completed ? "lesson-completed" : ""
                                      }`}
                                    initial={{ opacity: 0, x: -10 }}
                                    animate={{ opacity: 1, x: 0 }}
                                    transition={{ delay: index * 0.04 }}
                                    onClick={() => openLesson(lesson.lessonId)}
                                    style={{
                                      cursor: "pointer",
                                      background: lesson.completed ? "#ecfdf5" : undefined,
                                      border: lesson.completed
                                        ? "1px solid #86efac"
                                        : "1px solid var(--border)",
                                    }}
                                  >
                                    <div className="lesson-item-right">
                                      <div
                                        className={`lesson-number ${lesson.completed ? "lesson-number-done" : ""
                                          }`}
                                        style={{
                                          background: lesson.completed ? "#dcfce7" : undefined,
                                          color: lesson.completed ? "#15803d" : undefined,
                                        }}
                                      >
                                        {lesson.completed ? (
                                          <CheckCircle2 size={18} />
                                        ) : (
                                          <span>
                                            {displayNumber || lesson.displayNumber || index + 1}
                                          </span>
                                        )}
                                      </div>

                                      <div>
                                        <p
                                          className="lesson-title"
                                          style={{
                                            color: lesson.completed ? "#15803d" : undefined,
                                            fontWeight: lesson.completed ? 800 : undefined,
                                          }}
                                        >
                                          {lesson.title}
                                        </p>

                                        <span
                                          className="lesson-duration"
                                          style={{
                                            color: lesson.completed ? "#16a34a" : undefined,
                                          }}
                                        >
                                          {lesson.completed ? "تم إنهاء الدرس" : "غير مكتمل"}
                                        </span>
                                      </div>
                                    </div>

                                    <ChevronLeft
                                      size={18}
                                      style={{
                                        color: lesson.completed
                                          ? "#16a34a"
                                          : "var(--muted-foreground)",
                                      }}
                                    />
                                  </motion.div>
                                );
                              })}
                            </div>
                          </motion.div>
                        )}
                      </AnimatePresence>
                    </div>
                  );
                })}
              </div>
            </div>
          ) : null}
        </motion.div>
      </DashboardLayout>
    );
  }

  return (
    <DashboardLayout title="المواد الدراسية" subtitle="اختر مادة للبدء في التعلم" titleIcon={BookOpen}>
      {subjectsLoading ? (
        <div className="card" style={{ padding: "2rem", textAlign: "center" }}>
          <Loader2 className="animate-spin" style={{ margin: "0 auto 1rem" }} />
          جاري تحميل المواد...
        </div>
      ) : subjectsError ? (
        <div
          className="card"
          style={{ padding: "2rem", textAlign: "center", color: "red" }}
        >
          {subjectsError}
        </div>
      ) : (
        <motion.div
          className="subjects-page-grid"
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ duration: 0.4 }}
        >
          {subjects.map((subject, index) => (
            <motion.div
              key={subject.id}
              className="subject-page-card card"
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: index * 0.08 }}
              onClick={() => openSubject(subject.id)}
            >
              <div className={`subject-page-icon rec-icon-${subject.color}`}>
                {renderIcon(subject.iconKey, 24)}
              </div>

              <h3 className="subject-page-title">{subject.title}</h3>
              <p className="subject-page-desc">{subject.desc}</p>

              <div className="subject-page-meta">
                <span>
                  {subject.completed} / {subject.lessons} درس
                </span>
                <span>{subject.progress}٪</span>
              </div>

              <div className="lesson-progress-bar-bg">
                <motion.div
                  className={`lesson-progress-bar-fill lesson-bar-${subject.color}`}
                  initial={{ width: 0 }}
                  animate={{ width: `${subject.progress}%` }}
                  transition={{ duration: 0.8, delay: 0.3 + index * 0.1 }}
                  style={{
                    background: subject.progress === 100 ? "#22c55e" : undefined,
                  }}
                />
              </div>
            </motion.div>
          ))}
        </motion.div>
      )}
    </DashboardLayout>
  );
};

export default Subjects;
