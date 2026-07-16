"""
The Karpathy-style knowledge graph: vocabulary terms as nodes, cross-language
equivalence as edges. Everything the game (Unity frontend) plays through is derived
from this graph, so ingestion is idempotent and safe to re-run on an unchanged file
(WikiDocument.content_hash lets the ingest command skip files it has already seen).
"""
import hashlib

from django.db import models


class Language(models.TextChoices):
    """FR-2: the eight languages the platform must cross-map."""
    ENGLISH = "en", "English"
    SPANISH = "es", "Spanish"
    GERMAN = "de", "German"
    FRENCH = "fr", "French"
    POLISH = "pl", "Polish"
    RUSSIAN = "ru", "Russian"
    JAPANESE = "ja", "Japanese"
    CHINESE = "zh", "Chinese"


class WikiDocument(models.Model):
    """One ingested Markdown source file (FR-5)."""
    path = models.CharField(max_length=1024)
    content_hash = models.CharField(max_length=64, unique=True)
    raw_length = models.IntegerField()
    ingested_at = models.DateTimeField(auto_now_add=True)

    def __str__(self) -> str:
        return self.path

    @staticmethod
    def hash_of(content: str) -> str:
        return hashlib.sha256(content.encode("utf-8")).hexdigest()


class SymbolMapping(models.Model):
    """
    A user-defined symbol <-> phrase substitution, scoped to the document that defined
    it (e.g. `m>=Scorpio`). Symbols and phrases are used interchangeably in content
    from that document: SymbolCodec (knowledge/symbols.py) can substitute in either
    direction.
    """
    document = models.ForeignKey(WikiDocument, on_delete=models.CASCADE,
                                  related_name="symbol_mappings")
    symbol = models.CharField(max_length=64)
    phrase = models.CharField(max_length=512)

    class Meta:
        unique_together = ("document", "symbol")

    def __str__(self) -> str:
        return f"{self.symbol}={self.phrase}"


class Term(models.Model):
    """A vocabulary node: one word/phrase in one language."""
    text = models.CharField(max_length=256)
    language = models.CharField(max_length=2, choices=Language.choices)
    reading = models.CharField(max_length=256, blank=True, default="")
    first_seen_in = models.ForeignKey(WikiDocument, on_delete=models.CASCADE,
                                      related_name="terms")
    created_at = models.DateTimeField(auto_now_add=True)

    class Meta:
        unique_together = ("text", "language")

    def __str__(self) -> str:
        return f"{self.text} [{self.language}]"


class TranslationSource(models.TextChoices):
    MARKDOWN = "MARKDOWN", "Declared in Markdown"
    AI = "AI", "AI-generated"


class TranslationEdge(models.Model):
    """
    A cross-language equivalence edge between two terms — the graph structure Unity
    consumes to build the cross-language matrix (FR-2). `source` distinguishes edges a
    teacher declared explicitly from gaps the AI worker filled in: AI-authored edges
    are always distinguishable from human-authored ones.
    """
    from_term = models.ForeignKey(Term, on_delete=models.CASCADE, related_name="edges_out")
    to_term = models.ForeignKey(Term, on_delete=models.CASCADE, related_name="edges_in")
    source = models.CharField(max_length=16, choices=TranslationSource.choices)
    confidence = models.FloatField(default=1.0)
    created_at = models.DateTimeField(auto_now_add=True)

    class Meta:
        unique_together = ("from_term", "to_term")

    def __str__(self) -> str:
        return f"{self.from_term} -> {self.to_term} ({self.source})"
