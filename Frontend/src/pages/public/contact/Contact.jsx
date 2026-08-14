import React, { useState } from 'react';
import {
  AlertCircle,
  CheckCircle,
  Clock,
  Mail,
  MapPin,
  MessageCircle,
  Phone,
  Send,
} from 'lucide-react';
import PublicNavbar from '../../../components/public-navbar/PublicNavbar.jsx';
import './Contact.css';
import { API_BASE_URL } from '@/config/api';

const FORMSUBMIT_ENDPOINT = 'https://formsubmit.co/edunext.contact@gmail.com';
const FORMSUBMIT_AJAX_ENDPOINT = 'https://formsubmit.co/ajax/edunext.contact@gmail.com';

export default function Contact() {
  const contactPageUrl = typeof window !== 'undefined' ? window.location.href : '';
  const [status, setStatus] = useState({ type: '', message: '' });
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (event) => {
    event.preventDefault();
    setStatus({ type: '', message: '' });
    setIsSubmitting(true);

    const form = event.currentTarget;
    const formData = new FormData(form);
    const payload = Object.fromEntries(formData.entries());

    try {
      // 1. حفظ بالداتا بيس
      await fetch(`${API_BASE_URL}/api/contact-messages`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          name: payload.name,
          email: payload.email,
          subject: payload.subject,
          message: payload.message,
        }),
      });

      // 2. إرسال للجيميل عبر formsubmit
      const response = await fetch(FORMSUBMIT_AJAX_ENDPOINT, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Accept: 'application/json',
        },
        body: JSON.stringify(payload),
      });

      if (!response.ok) {
        const result = await response.json().catch(() => null);
        setStatus({
          type: 'error',
          message: result?.message || 'تعذر إرسال الرسالة. تأكد من البيانات وحاول مرة أخرى.',
        });
        return;
      }

      form.reset();
      setStatus({
        type: 'success',
        message: 'تم إرسال رسالتك بنجاح. سنعود إليك قريبا.',
      });
    } catch {
      setStatus({
        type: 'error',
        message: 'تعذر إرسال الرسالة حاليا. تحقق من اتصال الإنترنت وحاول مرة أخرى.',
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="contact-page" dir="rtl">
      <PublicNavbar />

      <main className="contact-main grid-bg">
        <section className="edn-container contact-hero">
          <div className="contact-copy">
            <span className="pill">تواصل معنا</span>
            <h1>نحن قريبون منك في كل خطوة تعليمية.</h1>
            <p>
              عندك سؤال عن المنصة، التسجيل، أو أدوات الذكاء الاصطناعي؟ أرسل لنا رسالتك
              وسنرد عليك بأسرع وقت.
            </p>

            <div className="contact-info-grid">
              <article>
                <Mail size={20} />
                <div>
                  <strong>البريد الإلكتروني</strong>
                  <span>edunext.contact@gmail.com</span>
                </div>
              </article>
              <article>
                <Phone size={20} />
                <div>
                  <strong>الهاتف</strong>
                  <span>+970 59 895 3522</span>
                </div>
              </article>
              <article>
                <MapPin size={20} />
                <div>
                  <strong>العنوان</strong>
                  <span>جنين، فلسطين</span>
                </div>
              </article>
              <article>
                <Clock size={20} />
                <div>
                  <strong>أوقات الرد</strong>
                  <span>الأحد - الخميس، 9 صباحا - 5 مساء</span>
                </div>
              </article>
            </div>
          </div>

          <form
            className="contact-form"
            id="contact-form"
            action={FORMSUBMIT_ENDPOINT}
            method="POST"
            onSubmit={handleSubmit}
          >
            <input type="hidden" name="_subject" value="New EduNext Contact Message" />
            <input type="hidden" name="_template" value="table" />
            <input type="hidden" name="_captcha" value="false" />
            <input type="hidden" name="_next" value={contactPageUrl} />
            <input type="hidden" name="_url" value={contactPageUrl} />
            <input type="text" name="_honey" className="hidden-field" tabIndex="-1" autoComplete="off" />

            <div className="form-glow" aria-hidden="true" />
            <div className="form-title-row">
              <div className="icon-box">
                <MessageCircle size={24} />
              </div>
              <div>
                <h2>أرسل رسالة</h2>
                <p>املأ البيانات وسنعود إليك قريبا.</p>
              </div>
            </div>

            {status.type === 'success' && (
              <div className="success-note" role="status">
                <CheckCircle size={18} />
                {status.message}
              </div>
            )}

            {status.type === 'error' && (
              <div className="error-note" role="alert">
                <AlertCircle size={18} />
                {status.message}
              </div>
            )}

            <label>
              الاسم الكامل
              <input required name="name" type="text" placeholder="اكتب اسمك" />
            </label>

            <label>
              البريد الإلكتروني
              <input required name="email" type="email" placeholder="example@email.com" />
            </label>

            <label>
              الموضوع
              <input required name="subject" type="text" placeholder="كيف يمكننا مساعدتك؟" />
            </label>

            <label>
              الرسالة
              <textarea required name="message" rows={5} placeholder="اكتب رسالتك هنا..." />
            </label>

            <button className="btn btn-primary" type="submit" disabled={isSubmitting}>
              {isSubmitting ? 'جاري الإرسال...' : 'إرسال الرسالة'}
              <Send size={18} />
            </button>
          </form>
        </section>
      </main>
    </div>
  );
}