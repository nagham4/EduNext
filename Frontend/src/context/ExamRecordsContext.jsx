import { createContext, useContext, useState, useCallback } from "react";

const ExamRecordsContext = createContext(null);

const initialRecords = [
  { id: "rec-1", subject: "arabic", subjectName: "اللغة العربية", type: "comprehensive", typeName: "شامل", score: 92, total: 25, percentage: 92, date: "2026-03-28", lessons: [] },
  { id: "rec-2", subject: "math", subjectName: "الرياضيات", type: "comprehensive", typeName: "شامل", score: 22, total: 25, percentage: 88, date: "2026-03-25", lessons: [] },
  { id: "rec-3", subject: "chemistry", subjectName: "الكيمياء", type: "comprehensive", typeName: "شامل", score: 19, total: 25, percentage: 75, date: "2026-03-20", lessons: [] },
];

export function ExamRecordsProvider({ children }) {
  const [records, setRecords] = useState(initialRecords);

  const addRecord = useCallback((record) => {
    const isDuplicate = records.some(
      (r) => r.subject === record.subject && r.type === record.type && r.date === record.date && r.score === record.score
    );
    if (isDuplicate) return;

    setRecords((prev) => [
      { ...record, id: "rec-" + Date.now() },
      ...prev,
    ]);
  }, [records]);

  return (
    <ExamRecordsContext.Provider value={{ records, addRecord }}>
      {children}
    </ExamRecordsContext.Provider>
  );
}

export function useExamRecords() {
  const ctx = useContext(ExamRecordsContext);
  if (!ctx) throw new Error("useExamRecords must be used within ExamRecordsProvider");
  return ctx;
}
