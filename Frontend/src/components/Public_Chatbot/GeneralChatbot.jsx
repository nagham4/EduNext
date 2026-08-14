import { useEffect, useMemo, useRef, useState } from 'react';
import { Bot, ChevronDown, GraduationCap, Reply, Send, Trash2, X } from 'lucide-react';
import { EDIT_THIS_CONTENT, prepareGeneralChatTurn } from './generalChatbotService';
import './GeneralChatbot.css';

const STORAGE_KEY = `${EDIT_THIS_CONTENT.projectName.toLowerCase()}_general_site_chat_messages_v2`;

const STARTER_MESSAGE = {
  id: 'starter',
  role: 'assistant',
  content: EDIT_THIS_CONTENT.introMessage,
  time: '',
};

export default function GeneralChatbot({ compact = false }) {
  const [isOpen, setIsOpen] = useState(false);
  const [input, setInput] = useState('');
  const [isSending, setIsSending] = useState(false);
  const [hasUnread, setHasUnread] = useState(false);
  const [showTeaser, setShowTeaser] = useState(true);
  const [replyTarget, setReplyTarget] = useState(null);
  const [messages, setMessages] = useState(() => {
    const saved = loadJson(STORAGE_KEY, []);
    return Array.isArray(saved) && saved.length ? saved : [withCurrentTime(STARTER_MESSAGE)];
  });
  const logRef = useRef(null);
  const inputRef = useRef(null);
  const isOpenRef = useRef(isOpen);

  const hasConversation = useMemo(() => messages.length > 1, [messages.length]);
  const closedTeaserText = compact ? 'أنا مساعدك الذكي، اسألني عن EduNext' : EDIT_THIS_CONTENT.closedTeaser;

  useEffect(() => {
    isOpenRef.current = isOpen;
    if (isOpen) setHasUnread(false);
  }, [isOpen]);

  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(messages.filter((message) => !message.pending)));
  }, [messages]);

  useEffect(() => {
    if (!isOpen || !logRef.current) return;
    logRef.current.scrollTop = logRef.current.scrollHeight;
    setHasUnread(false);
  }, [isOpen, messages]);

  async function handleSubmit(event) {
    event.preventDefault();
    const text = input.trim();
    if (!text || isSending) return;

    const preparedTurn = prepareGeneralChatTurn({ message: text, replyTarget });
    const userMessage = withCurrentTime({
      role: 'user',
      content: text,
      replyTo: replyTarget ? getExcerpt(replyTarget.content, 120) : '',
    });

    setInput('');
    setReplyTarget(null);
    setIsSending(true);

    const pendingMessage = withCurrentTime({
      role: 'assistant',
      content: EDIT_THIS_CONTENT.loadingMessage,
      pending: true,
    });

    setMessages((current) => [...current, userMessage, pendingMessage]);

    window.setTimeout(() => {
      replacePendingMessage(pendingMessage.id, preparedTurn.localReply);
      setIsSending(false);
      notifyIfClosed();
    }, 180);
  }

  function replacePendingMessage(id, content) {
    setMessages((current) =>
      current.map((message) =>
        message.id === id
          ? {
              ...message,
              content,
              pending: false,
            }
          : message,
      ),
    );
  }

  function clearMessages() {
    const starter = withCurrentTime(STARTER_MESSAGE);
    setMessages([starter]);
    setReplyTarget(null);
    localStorage.setItem(STORAGE_KEY, JSON.stringify([starter]));
  }

  function useSuggestion(value) {
    setInput(value);
    setIsOpen(true);
    window.setTimeout(() => inputRef.current?.focus(), 0);
  }

  function startReply(message) {
    setReplyTarget({
      id: message.id,
      content: message.content,
    });
    setIsOpen(true);
    window.setTimeout(() => inputRef.current?.focus(), 0);
  }

  function openChat() {
    setIsOpen(true);
    setHasUnread(false);
  }

  function notifyIfClosed() {
    if (!isOpenRef.current) setHasUnread(true);
  }

  return (
    <section className={`general-chatbot ${isOpen ? 'open' : ''} ${compact ? 'compact' : ''}`} aria-label={EDIT_THIS_CONTENT.assistantTitle}>
      {isOpen ? (
        <div className="general-chatbot-panel" dir="rtl">
          <header className="general-chatbot-header">
            <div className="general-chatbot-title">
              <span className="general-chatbot-avatar">
                <Bot size={20} />
              </span>
              <div>
                <strong>{EDIT_THIS_CONTENT.assistantTitle}</strong>
                <small>{EDIT_THIS_CONTENT.audienceLabel}</small>
              </div>
            </div>
            <div className="general-chatbot-actions">
              <button type="button" onClick={clearMessages} title="حذف الرسائل" disabled={!hasConversation || isSending}>
                <Trash2 size={17} />
              </button>
              <button type="button" onClick={() => setIsOpen(false)} title="تصغير">
                <ChevronDown size={18} />
              </button>
            </div>
          </header>

          <div className="general-chatbot-memory">
            <Bot size={15} />
            <span>{EDIT_THIS_CONTENT.helperLine}</span>
          </div>

          <div className="general-chatbot-log" ref={logRef}>
            {messages.map((message) => (
              <article
                className={`general-chatbot-message ${message.role} ${message.pending ? 'pending' : ''}`}
                key={message.id}
              >
                <div className="general-chatbot-meta">
                  <span>
                    {message.role === 'assistant' ? 'EduNext' : 'أنت'} · {message.time}
                  </span>
                  {message.role === 'assistant' && !message.pending && (
                    <button type="button" onClick={() => startReply(message)} title="الرد على هذه الرسالة">
                      <Reply size={13} />
                    </button>
                  )}
                </div>
                {message.replyTo && (
                  <div className="general-chatbot-quoted-reply">
                    <span>رد على</span>
                    <p>{message.replyTo}</p>
                  </div>
                )}
                <div className="general-chatbot-body">{formatLines(message.content)}</div>
              </article>
            ))}
          </div>

          <div className="general-chatbot-suggestions" aria-label="اقتراحات">
            {EDIT_THIS_CONTENT.suggestions.map((suggestion) => (
              <button type="button" key={suggestion} onClick={() => useSuggestion(suggestion)}>
                {suggestion}
              </button>
            ))}
          </div>

          <form className="general-chatbot-form" onSubmit={handleSubmit}>
            {replyTarget && (
              <div className="general-chatbot-reply-target">
                <div>
                  <strong>رد على رسالة</strong>
                  <span>{getExcerpt(replyTarget.content, 86)}</span>
                </div>
                <button type="button" onClick={() => setReplyTarget(null)} title="إلغاء الرد">
                  <X size={14} />
                </button>
              </div>
            )}
            <textarea
              ref={inputRef}
              value={input}
              onChange={(event) => setInput(event.target.value)}
              placeholder={EDIT_THIS_CONTENT.inputPlaceholder}
              rows={2}
              disabled={isSending}
            />
            <button type="submit" title="إرسال" disabled={!input.trim() || isSending}>
              {isSending ? <X size={18} /> : <Send size={18} />}
            </button>
          </form>
        </div>
      ) : (
        <div className="general-chatbot-closed" dir="rtl">
          {showTeaser && (
            <div className="general-chatbot-teaser">
              <button
                type="button"
                className="general-chatbot-teaser-close"
                onClick={() => setShowTeaser(false)}
                title="إخفاء"
              >
                <X size={13} />
              </button>
              <button type="button" className="general-chatbot-teaser-text" onClick={openChat}>
                {closedTeaserText}
              </button>
            </div>
          )}
          <button className="general-chatbot-launcher" type="button" onClick={openChat} aria-label={`فتح ${EDIT_THIS_CONTENT.assistantTitle}`}>
            {hasUnread && <span className="general-chatbot-unread" aria-label="رد جديد" />}
            <span className="general-chatbot-launcher-mark" aria-hidden="true">
              <GraduationCap size={30} strokeWidth={2.5} />
            </span>
          </button>
        </div>
      )}
    </section>
  );
}

function formatLines(content) {
  return cleanDisplayText(content)
    .split('\n')
    .map((line, index) =>
      line.trim() ? (
        <p key={`${line}-${index}`}>{formatInline(line)}</p>
      ) : (
        <span className="line-gap" key={index} />
      ),
    );
}

function cleanDisplayText(content) {
  return String(content || '')
    .replace(/[#*_`>]+/g, '')
    .replace(/-{3,}/g, '')
    .replace(/[\u{1F300}-\u{1FAFF}\u{2600}-\u{27BF}]/gu, '')
    .replace(/\n{3,}/g, '\n\n')
    .trim();
}

function formatInline(line) {
  const parts = String(line).split(/(\*\*[^*]+\*\*)/g);
  return parts.map((part, index) => {
    if (part.startsWith('**') && part.endsWith('**')) {
      return <strong key={`${part}-${index}`}>{part.slice(2, -2)}</strong>;
    }
    return <span key={`${part}-${index}`}>{part}</span>;
  });
}

function withCurrentTime(message) {
  return {
    id: message.id || crypto.randomUUID?.() || `${Date.now()}-${Math.random()}`,
    time: message.time || formatChatTime(new Date()),
    ...message,
  };
}

function formatChatTime(date) {
  const parts = new Intl.DateTimeFormat('ar-PS', {
    hour: 'numeric',
    minute: '2-digit',
    hour12: true,
  }).formatToParts(date);

  const hour = parts.find((part) => part.type === 'hour')?.value || '';
  const minute = parts.find((part) => part.type === 'minute')?.value || '';
  const dayPeriod = parts.find((part) => part.type === 'dayPeriod')?.value || '';
  const period = /م|PM/i.test(dayPeriod) ? 'مساءً' : 'صباحاً';

  return `${hour}:${minute} ${period}`;
}

function getExcerpt(content, limit = 86) {
  const value = String(content || '').replace(/\s+/g, ' ').trim();
  return value.length > limit ? `${value.slice(0, limit)}...` : value;
}

function loadJson(key, fallback) {
  try {
    const value = JSON.parse(localStorage.getItem(key));
    return value ?? fallback;
  } catch {
    return fallback;
  }
}
