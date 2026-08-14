import argparse

from app.rag.embedder import embed_texts
from app.rag.pdf_loader import available_subjects, load_subject_pdfs
from app.rag.vector_store import build_index, save_index


def ingest_subject(subject: str) -> int:
    chunks = load_subject_pdfs(subject)
    if not chunks:
        print(f"No text chunks found for {subject}. Check the PDFs in data/pdfs/{subject}.")
        return 0

    print(f"Building {subject} vector database from {len(chunks)} chunks...")
    embeddings = embed_texts(chunk["text"] for chunk in chunks)
    build_index(embeddings, chunks, subject=subject)
    save_index(subject)
    print(f"Saved {subject} vector database.")
    return len(chunks)


def main() -> None:
    parser = argparse.ArgumentParser(description="Build FAISS vector databases from subject PDFs.")
    parser.add_argument(
        "--subject",
        choices=available_subjects(),
        help="Subject folder under data/pdfs. If omitted, all subjects are ingested.",
    )
    args = parser.parse_args()

    subjects = [args.subject] if args.subject else available_subjects()
    if not subjects:
        raise SystemExit("No subject folders found under data/pdfs.")

    for subject in subjects:
        ingest_subject(subject)


if __name__ == "__main__":
    main()
