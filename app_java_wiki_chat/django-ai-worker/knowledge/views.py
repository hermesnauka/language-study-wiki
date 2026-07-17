"""
Thin REST view layer: parses the request, delegates to the domain layer
(graph_pipeline/symbols/models), shapes the JSON response. Consumed by the Spring
backend (which proxies wiki uploads here) and, via /api/graph/export, directly by the
Unity frontend to populate playable levels.
"""
import json

from django.http import HttpResponse, JsonResponse
from django.views.decorators.csrf import csrf_exempt
from django.views.decorators.http import require_http_methods

from .graph_pipeline import ingest_markdown
from .markdown_sandbox import MarkdownTooLargeError
from .models import SymbolMapping, Term, TranslationEdge, WikiDocument
from .symbols import SymbolCodec
from .translation import get_translation_provider


def _body(request) -> dict:
    if not request.body:
        return {}
    try:
        return json.loads(request.body)
    except json.JSONDecodeError:
        raise ValueError("Malformed JSON body")


def api_view(handler):
    """ValueError (bad input/oversized document) -> 400 JSON {"error": ...}."""
    @csrf_exempt
    def wrapped(request, *args, **kwargs):
        try:
            result = handler(request, *args, **kwargs)
        except (ValueError, MarkdownTooLargeError) as e:
            return JsonResponse({"error": str(e)}, status=400)
        except WikiDocument.DoesNotExist:
            return JsonResponse({"error": "No such document"}, status=404)
        except Term.DoesNotExist:
            return JsonResponse({"error": "No such term"}, status=404)
        if isinstance(result, HttpResponse):
            return result
        return JsonResponse(result, safe=isinstance(result, dict))
    return wrapped


def _term_to_dict(term: Term) -> dict:
    return {"id": term.id, "text": term.text, "language": term.language, "reading": term.reading}


@api_view
@require_http_methods(["POST"])
def wiki_ingest(request):
    body = _body(request)
    content = body.get("content")
    if not content or not str(content).strip():
        raise ValueError("Missing required field: content")
    path = str(body.get("path") or "inline")
    return ingest_markdown(path, content)


@api_view
@require_http_methods(["GET"])
def graph_terms(request):
    queryset = Term.objects.all().order_by("language", "text")
    language = request.GET.get("language")
    if language:
        queryset = queryset.filter(language=language)
    return [_term_to_dict(term) for term in queryset]


@api_view
@require_http_methods(["GET"])
def graph_term_translations(request, term_id):
    term = Term.objects.get(id=term_id)
    edges = TranslationEdge.objects.filter(from_term=term).select_related("to_term")
    return [{"toTerm": _term_to_dict(edge.to_term), "source": edge.source,
            "confidence": edge.confidence} for edge in edges]


@api_view
@require_http_methods(["GET"])
def graph_export(request):
    return {
        "terms": [_term_to_dict(term) for term in Term.objects.all()],
        "edges": [{"from": edge.from_term_id, "to": edge.to_term_id,
                  "source": edge.source, "confidence": edge.confidence}
                 for edge in TranslationEdge.objects.all()],
    }


@api_view
@require_http_methods(["POST"])
def symbols_apply(request):
    body = _body(request)
    document_id = body.get("documentId")
    text = body.get("text")
    direction = body.get("direction", "expand")
    if document_id is None:
        raise ValueError("Missing required field: documentId")
    if text is None:
        raise ValueError("Missing required field: text")
    if direction not in ("expand", "contract"):
        raise ValueError('direction must be "expand" or "contract"')

    document = WikiDocument.objects.get(id=document_id)
    mappings = dict(SymbolMapping.objects.filter(document=document)
                    .values_list("symbol", "phrase"))
    codec = SymbolCodec(mappings)
    result = codec.expand(text) if direction == "expand" else codec.contract(text)
    return {"result": result}


@api_view
@require_http_methods(["POST"])
def chat_translate(request):
    """FR-3: on-demand translation for the Spring backend's real-time 2-user chat.
    Best-effort like the wiki ingestion pipeline's own translation gap-filling —
    `translation` is null rather than an error when no provider is unavailable."""
    body = _body(request)
    text = body.get("text")
    source_lang = body.get("sourceLang")
    target_lang = body.get("targetLang")
    if not text or not source_lang or not target_lang:
        raise ValueError("Missing required field: text, sourceLang, targetLang are all required")
    translation = get_translation_provider().translate(text, source_lang, target_lang)
    return {"translation": translation}
