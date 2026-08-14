from typing import Dict, List
from pathlib import Path
import pickle

import faiss
import numpy as np

from app.core.config import VECTOR_DB_DIR


STORE_DIR = VECTOR_DB_DIR
INDEX_FILE = "faiss.index"
CHUNKS_FILE = "chunks.pkl"

indexes: Dict[str, faiss.Index] = {}
chunks_store: Dict[str, List[Dict]] = {}


def _subject_dir(subject: str) -> Path:
    return STORE_DIR / subject


def build_index(embeddings: np.ndarray, chunks: List[Dict], subject: str) -> None:
    if embeddings is None or len(embeddings) == 0:
        raise ValueError(f"No embeddings to index for subject: {subject}")

    vectors = np.asarray(embeddings, dtype="float32")
    index = faiss.IndexFlatIP(vectors.shape[1])
    index.add(vectors)

    indexes[subject] = index
    chunks_store[subject] = chunks


def save_index(subject: str) -> None:
    if subject not in indexes:
        raise ValueError(f"Index is not loaded for subject: {subject}")

    target = _subject_dir(subject)
    target.mkdir(parents=True, exist_ok=True)

    faiss.write_index(indexes[subject], str(target / INDEX_FILE))
    with (target / CHUNKS_FILE).open("wb") as file:
        pickle.dump(chunks_store[subject], file)


def load_index(subject: str) -> bool:
    target = _subject_dir(subject)
    index_path = target / INDEX_FILE
    chunks_path = target / CHUNKS_FILE

    if not index_path.exists() or not chunks_path.exists():
        return False

    indexes[subject] = faiss.read_index(str(index_path))
    with chunks_path.open("rb") as file:
        chunks_store[subject] = pickle.load(file)

    return True


def has_index(subject: str) -> bool:
    return subject in indexes or ((_subject_dir(subject) / INDEX_FILE).exists() and (_subject_dir(subject) / CHUNKS_FILE).exists())


def search(vector: np.ndarray, subject: str, k: int = 5) -> List[Dict]:
    if subject not in indexes and not load_index(subject):
        return []

    texts = chunks_store.get(subject, [])
    if not texts:
        return []

    query = np.asarray([vector], dtype="float32")
    scores, ids = indexes[subject].search(query, min(k, len(texts)))

    results = []
    for score, idx in zip(scores[0], ids[0]):
        if 0 <= idx < len(texts):
            item = dict(texts[idx])
            item["score"] = float(score)
            results.append(item)

    return results
