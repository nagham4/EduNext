from functools import lru_cache
from typing import Iterable

import numpy as np
from sentence_transformers import SentenceTransformer


MODEL_NAME = "sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2"


@lru_cache(maxsize=1)
def get_model() -> SentenceTransformer:
    return SentenceTransformer(MODEL_NAME)


def embed_texts(texts: Iterable[str]) -> np.ndarray:
    texts = list(texts)
    if not texts:
        return np.empty((0, 384), dtype="float32")

    return get_model().encode(
        texts,
        show_progress_bar=True,
        normalize_embeddings=True,
    ).astype("float32")


def embed_query(question: str) -> np.ndarray:
    return get_model().encode(
        question,
        normalize_embeddings=True,
    ).astype("float32")
