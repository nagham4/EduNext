from typing import List

from app.rag.embedder import embed_query
from app.rag.vector_store import search


def retrieve(question: str, subject: str, k: int = 5) -> List[dict]:
    if not question or not question.strip():
        return []

    vector = embed_query(question)
    return search(vector, subject=subject, k=k)


def format_context(results: List[dict]) -> str:
    blocks = []
    for index, result in enumerate(results, start=1):
        source = result.get("source", "unknown")
        page = result.get("page", "?")
        text = result.get("text", "").strip()
        if text:
            blocks.append(f"[{index}] Source: {source}, page {page}\n{text}")

    return "\n\n".join(blocks)
