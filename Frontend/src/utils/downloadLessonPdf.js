export function downloadLessonPdf({ lessonTitle, subjectTitle, lessonNumber, totalLessons, duration, explanation, summary }) {
  const html = `
<!DOCTYPE html>
<html dir="rtl" lang="ar">
<head>
<meta charset="utf-8">
<title>${lessonTitle} - ملخص</title>
<style>
  @page { margin: 2cm; }
  body { font-family: 'Segoe UI', Tahoma, Arial, sans-serif; direction: rtl; color: #1a1a2e; line-height: 1.8; padding: 2rem; }
  .header { text-align: center; margin-bottom: 2rem; padding-bottom: 1.5rem; border-bottom: 3px solid #6c5ce7; }
  .header h1 { font-size: 1.75rem; color: #6c5ce7; margin: 0 0 0.5rem; }
  .header .meta { color: #666; font-size: 0.9rem; }
  .section { margin-bottom: 1.5rem; }
  .section h2 { font-size: 1.2rem; color: #2d3436; margin-bottom: 0.75rem; padding-bottom: 0.5rem; border-bottom: 2px solid #dfe6e9; }
  .explanation { background: #f8f9fa; padding: 1.25rem; border-radius: 8px; border-right: 4px solid #6c5ce7; }
  .summary-list { list-style: none; padding: 0; margin: 0; }
  .summary-list li { padding: 0.6rem 1rem; margin-bottom: 0.5rem; background: #f0f0ff; border-radius: 6px; display: flex; align-items: center; gap: 0.5rem; }
  .summary-list li::before { content: "✓"; color: #6c5ce7; font-weight: bold; font-size: 1.1rem; }
  .footer { margin-top: 2rem; text-align: center; color: #999; font-size: 0.8rem; border-top: 1px solid #eee; padding-top: 1rem; }
  @media print { body { padding: 0; } }
</style>
</head>
<body>
  <div class="header">
    <h1>${lessonTitle}</h1>
    <div class="meta">${subjectTitle} • الدرس ${lessonNumber} من ${totalLessons} • ${duration}</div>
  </div>
  <div class="section">
    <h2>📖 شرح الدرس</h2>
    <div class="explanation">${explanation}</div>
  </div>
  <div class="section">
    <h2>📝 النقاط الرئيسية</h2>
    <ul class="summary-list">
      ${summary.map(p => `<li>${p}</li>`).join("\n      ")}
    </ul>
  </div>
  <div class="footer">تم إنشاء هذا الملخص من منصة EduNext</div>
</body>
</html>`;

  const printWindow = window.open("", "_blank");
  if (!printWindow) return;
  printWindow.document.write(html);
  printWindow.document.close();
  printWindow.onload = () => {
    printWindow.print();
  };
}
