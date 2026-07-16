from django.urls import path

from . import views

urlpatterns = [
    path("api/wiki/ingest", views.wiki_ingest),
    path("api/graph/terms", views.graph_terms),
    path("api/graph/terms/<int:term_id>/translations", views.graph_term_translations),
    path("api/graph/export", views.graph_export),
    path("api/symbols/apply", views.symbols_apply),
]
