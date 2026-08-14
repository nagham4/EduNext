import { useEffect, useMemo, useState } from "react";
import {
  AlertTriangle,
  Check,
  Eye,
  Inbox,
  Loader2,
  Mail,
  MessageCircle,
  RefreshCw,
  Search,
  Trash2,
  X,
} from "lucide-react";

import DashboardLayout from "../../../components/DashboardLayout";
import { API_BASE_URL } from "@/config/api";
import "./AdminMessages.css";

const SEEN_MESSAGES_KEY = "edunext_admin_seen_contact_messages";
const UNREAD_COUNT_KEY = "edunext_admin_unread_messages_count";

const getId = (message) => String(message.id || message.Id || "");
const getName = (message) => message.name || message.Name || "مرسل بدون اسم";
const getEmail = (message) => message.email || message.Email || "";
const getSubject = (message) => message.subject || message.Subject || "بدون موضوع";
const getBody = (message) => message.message || message.Message || "";
const getDate = (message) => message.createdAt || message.CreatedAt || "";

const readSeenIds = () => {
  try {
    return new Set(JSON.parse(localStorage.getItem(SEEN_MESSAGES_KEY) || "[]"));
  } catch {
    return new Set();
  }
};

const saveSeenIds = (ids) => {
  localStorage.setItem(SEEN_MESSAGES_KEY, JSON.stringify([...ids]));
};

const formatDate = (value) => {
  if (!value) return "غير محدد";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "غير محدد";
  return new Intl.DateTimeFormat("ar", { dateStyle: "medium", timeStyle: "short" }).format(date);
};

export default function AdminMessages() {
  const [messages, setMessages] = useState([]);
  const [seenIds, setSeenIds] = useState(() => readSeenIds());
  const [selectedMessage, setSelectedMessage] = useState(null);
  const [messageToDelete, setMessageToDelete] = useState(null);
  const [searchQuery, setSearchQuery] = useState("");
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const token = localStorage.getItem("token");

  const fetchMessages = async (isRefresh = false) => {
    if (!token) {
      window.location.href = "/login";
      return;
    }

    try {
      setError("");
      setSuccess("");
      isRefresh ? setRefreshing(true) : setLoading(true);

      const response = await fetch(`${API_BASE_URL}/api/admin/contact-messages`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const text = await response.text();
      const data = text ? JSON.parse(text) : [];

      if (!response.ok) {
        setError(data.message || "تعذر تحميل رسائل التواصل.");
        return;
      }

      setMessages(Array.isArray(data) ? data : []);
    } catch (err) {
      console.error(err);
      setError("تعذر الاتصال بالسيرفر. تأكد أن الباك إند يعمل.");
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  useEffect(() => {
    fetchMessages();
  }, []);

  const unseenMessages = useMemo(
    () => messages.filter((message) => !seenIds.has(getId(message))),
    [messages, seenIds]
  );

  useEffect(() => {
    localStorage.setItem(UNREAD_COUNT_KEY, String(unseenMessages.length));
    window.dispatchEvent(new CustomEvent("unreadMessagesUpdated", { detail: unseenMessages.length }));
  }, [unseenMessages.length]);

  const filteredMessages = useMemo(() => {
    const query = searchQuery.trim().toLowerCase();
    if (!query) return messages;

    return messages.filter((message) =>
      [getName(message), getEmail(message), getSubject(message), getBody(message)]
        .join(" ")
        .toLowerCase()
        .includes(query)
    );
  }, [messages, searchQuery]);

  const markMessageSeen = (message) => {
    const id = getId(message);
    if (!id) return;
    const nextSeenIds = new Set(seenIds);
    nextSeenIds.add(id);
    saveSeenIds(nextSeenIds);
    setSeenIds(nextSeenIds);
  };

  const markAllSeen = () => {
    const nextSeenIds = new Set(seenIds);
    messages.forEach((message) => {
      const id = getId(message);
      if (id) nextSeenIds.add(id);
    });
    saveSeenIds(nextSeenIds);
    setSeenIds(nextSeenIds);
  };

  const openPreview = (message) => {
    markMessageSeen(message);
    setSelectedMessage(message);
  };

  const openGmailReply = (message) => {
    markMessageSeen(message);
    const name = getName(message);
    const email = getEmail(message);
    const originalSubject = getSubject(message);
    const body = getBody(message);
    const date = formatDate(getDate(message));
    const subject = encodeURIComponent(`Re: ${originalSubject}`);
    const replyBody = encodeURIComponent(
      `\n\n---\nالرسالة الأصلية من ${name}\nالموضوع: ${originalSubject}\n${date}\n\n${body}`
    );

    window.open(
      `https://mail.google.com/mail/?view=cm&fs=1&to=${encodeURIComponent(email)}&su=${subject}&body=${replyBody}`,
      "_blank",
      "noopener,noreferrer"
    );
  };

  const deleteMessage = async () => {
    if (!messageToDelete) return;
    const id = getId(messageToDelete);

    try {
      setDeleting(true);
      setError("");
      setSuccess("");

      const response = await fetch(`${API_BASE_URL}/api/admin/contact-messages/${id}`, {
        method: "DELETE",
        headers: { Authorization: `Bearer ${token}` },
      });

      if (!response.ok) {
        setError("تعذر حذف الرسالة.");
        return;
      }

      setMessages((current) => current.filter((message) => getId(message) !== id));
      setSuccess("تم حذف الرسالة بنجاح.");
      setMessageToDelete(null);
      setSelectedMessage((current) => (current && getId(current) === id ? null : current));
    } catch (err) {
      console.error(err);
      setError("تعذر الاتصال بالسيرفر أثناء الحذف.");
    } finally {
      setDeleting(false);
    }
  };

  return (
    <DashboardLayout
      title="رسائل التواصل"
      subtitle="راجع رسائل الزوار، افتح الرد عبر Gmail، واحذف الرسائل المنتهية."
      titleIcon={Mail}
    >
      <section className="admin-messages-page">
        {unseenMessages.length > 0 && (
          <div className="admin-messages-alert" role="status">
            <div>
              <strong>{unseenMessages.length} رسائل جديدة</strong>
              <span>لم تتم معاينتها بعد.</span>
            </div>
            <button type="button" onClick={markAllSeen}>
              <Check size={16} />
              تعيين الكل كمقروء
            </button>
          </div>
        )}

        <div className="admin-messages-toolbar">
          <label className="admin-messages-search">
            <Search size={18} />
            <input
              type="search"
              value={searchQuery}
              onChange={(event) => setSearchQuery(event.target.value)}
              placeholder="ابحث بالاسم أو البريد أو نص الرسالة"
            />
          </label>

          <button
            type="button"
            className="admin-messages-refresh"
            onClick={() => fetchMessages(true)}
            disabled={refreshing}
          >
            <RefreshCw size={17} className={refreshing ? "spin" : ""} />
            تحديث
          </button>
        </div>

        {error && (
          <div className="admin-messages-feedback error">
            <AlertTriangle size={18} />
            {error}
          </div>
        )}

        {success && (
          <div className="admin-messages-feedback success">
            <Check size={18} />
            {success}
          </div>
        )}

        {loading ? (
          <div className="admin-messages-empty">
            <Loader2 className="spin" size={34} />
            <p>جاري تحميل الرسائل...</p>
          </div>
        ) : filteredMessages.length === 0 ? (
          <div className="admin-messages-empty">
            <Inbox size={42} />
            <h2>لا توجد رسائل</h2>
            <p>{searchQuery ? "لا توجد نتائج مطابقة للبحث." : "كل شيء هادئ حالياً."}</p>
          </div>
        ) : (
          <div className="admin-messages-table-wrap">
            <table className="admin-messages-table">
              <thead>
                <tr>
                  <th>الحالة</th>
                  <th>المرسل</th>
                  <th>البريد الإلكتروني</th>
                  <th>الموضوع</th>
                  <th>معاينة الرسالة</th>
                  <th>التاريخ</th>
                  <th>الإجراءات</th>
                </tr>
              </thead>
              <tbody>
                {filteredMessages.map((message) => {
                  const id = getId(message);
                  const isSeen = seenIds.has(id);
                  const body = getBody(message);

                  return (
                    <tr key={id} className={isSeen ? "seen" : "unseen"}>
                      <td>
                        <span className={`admin-message-status ${isSeen ? "seen" : "unseen"}`}>
                          {isSeen ? "مقروءة" : "جديدة"}
                        </span>
                      </td>
                      <td>
                        <div className="admin-message-sender">
                          <span>{getName(message).charAt(0).toUpperCase()}</span>
                          <strong>{getName(message)}</strong>
                        </div>
                      </td>
                      <td>
                        <a className="admin-message-email" href={`mailto:${getEmail(message)}`}>
                          {getEmail(message)}
                        </a>
                      </td>
                      <td className="admin-message-subject">{getSubject(message)}</td>
                      <td className="admin-message-preview">
                        {body.length > 84 ? `${body.slice(0, 84)}...` : body}
                      </td>
                      <td className="admin-message-date">{formatDate(getDate(message))}</td>
                      <td>
                        <div className="admin-message-actions">
                          <button type="button" title="معاينة الرسالة" onClick={() => openPreview(message)}>
                            <Eye size={17} />
                          </button>
                          <button type="button" title="الرد عبر Gmail" onClick={() => openGmailReply(message)}>
                            <MessageCircle size={17} />
                          </button>
                          <button
                            type="button"
                            className="danger"
                            title="حذف الرسالة"
                            onClick={() => setMessageToDelete(message)}
                          >
                            <Trash2 size={17} />
                          </button>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </section>

      {selectedMessage && (
        <div className="admin-message-modal-backdrop" onClick={() => setSelectedMessage(null)}>
          <article className="admin-message-modal" onClick={(event) => event.stopPropagation()}>
            <button type="button" className="admin-message-modal-close" onClick={() => setSelectedMessage(null)}>
              <X size={20} />
            </button>
            <div className="admin-message-modal-header">
              <span>{getName(selectedMessage).charAt(0).toUpperCase()}</span>
              <div>
                <h2>{getName(selectedMessage)}</h2>
                <p>{getEmail(selectedMessage)}</p>
              </div>
            </div>
            <time>{formatDate(getDate(selectedMessage))}</time>
            <div className="admin-message-modal-subject">
              <span>الموضوع</span>
              <strong>{getSubject(selectedMessage)}</strong>
            </div>
            <div className="admin-message-modal-body">{getBody(selectedMessage)}</div>
            <div className="admin-message-modal-actions">
              <button type="button" className="primary" onClick={() => openGmailReply(selectedMessage)}>
                <MessageCircle size={17} />
                الرد عبر Gmail
              </button>
              <button type="button" onClick={() => setSelectedMessage(null)}>
                إغلاق
              </button>
            </div>
          </article>
        </div>
      )}

      {messageToDelete && (
        <div className="admin-message-modal-backdrop" onClick={() => setMessageToDelete(null)}>
          <article className="admin-message-confirm" onClick={(event) => event.stopPropagation()}>
            <AlertTriangle size={34} />
            <h2>حذف الرسالة؟</h2>
            <p>سيتم حذف رسالة {getName(messageToDelete)} نهائياً من قاعدة البيانات.</p>
            <div>
              <button type="button" className="danger" onClick={deleteMessage} disabled={deleting}>
                {deleting ? "جاري الحذف..." : "نعم، احذف"}
              </button>
              <button type="button" onClick={() => setMessageToDelete(null)}>
                إلغاء
              </button>
            </div>
          </article>
        </div>
      )}
    </DashboardLayout>
  );
}