import sys
from pathlib import Path

BASE_DIR = Path(__file__).resolve().parent
PACKAGES_DIR = BASE_DIR / ".python_packages"

if sys.version_info[:2] not in {(3, 11), (3, 12), (3, 13)}:
    sys.stderr.write(
        f"خطأ: هذا المشروع يستعمل مكتبات مهيأة لـ Python 3.11 أو 3.12 أو 3.13. البيئة الحالية: {sys.version.split()[0]}\n"
    )
    sys.stderr.write(
        "يرجى تشغيل السكربت باستخدام Python 3.11 أو 3.12 أو 3.13 وإعادة تثبيت الحزم\n"
    )
    sys.exit(1)

sys.path[:0] = [str(PACKAGES_DIR), str(BASE_DIR)]

import uvicorn


if __name__ == "__main__":
    uvicorn.run(
        "app.main:app",
        host="0.0.0.0",
        port=5001,
        log_level="info",
        access_log=True,
    )
    
