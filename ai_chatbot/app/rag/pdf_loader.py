from pathlib import Path
from typing import Dict, List

from pypdf import PdfReader

from app.core.config import PDFS_DIR, STORAGE_DIR


OCR_CACHE_DIR = STORAGE_DIR / "ocr_cache"
OCR_LANGUAGES = ["ar", "en"]
OCR_RENDER_SCALE = 1.25

_ocr_reader = None


def _clean_text(text: str) -> str:
    return " ".join(text.replace("\x00", " ").split())


def split_text(text: str, chunk_size: int = 1000, chunk_overlap: int = 200) -> List[str]:
    if chunk_overlap >= chunk_size:
        raise ValueError("chunk_overlap must be smaller than chunk_size")

    text = _clean_text(text)
    if not text:
        return []

    chunks = []
    start = 0
    while start < len(text):
        end = min(start + chunk_size, len(text))
        chunk = text[start:end].strip()
        if chunk:
            chunks.append(chunk)
        if end == len(text):
            break
        start = end - chunk_overlap

    return chunks


def _ocr_cache_path(pdf_path: Path, page_number: int) -> Path:
    return OCR_CACHE_DIR / pdf_path.parent.name / pdf_path.stem / f"page-{page_number}.txt"


def _get_ocr_reader():
    global _ocr_reader
    if _ocr_reader is None:
        import easyocr

        _ocr_reader = easyocr.Reader(OCR_LANGUAGES, gpu=False)
    return _ocr_reader


def _extract_page_text_with_ocr(pdf_path: Path, page_index: int) -> str:
    page_number = page_index + 1
    cache_path = _ocr_cache_path(pdf_path, page_number)
    if cache_path.exists():
        return cache_path.read_text(encoding="utf-8")

    import fitz
    import numpy as np

    with fitz.open(pdf_path) as document:
        page = document.load_page(page_index)
        pixmap = page.get_pixmap(
            matrix=fitz.Matrix(OCR_RENDER_SCALE, OCR_RENDER_SCALE),
            alpha=False,
        )

    image = np.frombuffer(pixmap.samples, dtype=np.uint8).reshape(
        pixmap.height,
        pixmap.width,
        pixmap.n,
    )
    lines = _get_ocr_reader().readtext(image, detail=0, paragraph=False)
    text = "\n".join(line.strip() for line in lines if line and line.strip())

    cache_path.parent.mkdir(parents=True, exist_ok=True)
    cache_path.write_text(text, encoding="utf-8")
    return text


def load_pdf_chunks(pdf_path: str | Path, chunk_size: int = 1000, chunk_overlap: int = 200) -> List[Dict]:
    pdf_path = Path(pdf_path)
    reader = PdfReader(str(pdf_path))
    chunks = []

    for page_number, page in enumerate(reader.pages, start=1):
        text = page.extract_text() or ""
        if not text.strip():
            text = _extract_page_text_with_ocr(pdf_path, page_number - 1)
        for chunk_number, chunk in enumerate(split_text(text, chunk_size, chunk_overlap), start=1):
            chunks.append(
                {
                    "text": chunk,
                    "source": pdf_path.name,
                    "path": str(pdf_path),
                    "page": page_number,
                    "chunk": chunk_number,
                }
            )

    return chunks


def load_subject_pdfs(subject: str, data_dir: str | Path = PDFS_DIR) -> List[Dict]:
    subject_dir = Path(data_dir) / subject
    if not subject_dir.exists():
        raise FileNotFoundError(f"Subject folder not found: {subject_dir}")

    all_chunks = []
    for pdf_path in sorted(subject_dir.glob("*.pdf")):
        all_chunks.extend(load_pdf_chunks(pdf_path))

    return all_chunks


def available_subjects(data_dir: str | Path = PDFS_DIR) -> List[str]:
    data_path = Path(data_dir)
    if not data_path.exists():
        return []

    return sorted(path.name for path in data_path.iterdir() if path.is_dir())
