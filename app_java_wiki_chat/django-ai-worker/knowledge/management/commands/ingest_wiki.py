"""
FR-5: scans AI_WIKI['INGEST_DIR'] (env: WIKI_INGEST_DIR) for `.md` files and ingests
any whose content hasn't been seen before. One pass per invocation — run it from cron,
a systemd timer, or a wrapper polling loop for continuous "drop a file, it gets
digested" behaviour; the command itself stays a simple, testable single pass rather
than owning a long-lived filesystem-watch process.
"""
from pathlib import Path

from django.conf import settings
from django.core.management.base import BaseCommand

from knowledge.graph_pipeline import ingest_markdown
from knowledge.markdown_sandbox import MarkdownTooLargeError


class Command(BaseCommand):
    help = "Ingest any not-yet-seen Markdown files from AI_WIKI['INGEST_DIR']."

    def handle(self, *args, **options):
        ingest_dir = Path(settings.AI_WIKI["INGEST_DIR"])
        if not ingest_dir.is_dir():
            self.stdout.write(self.style.WARNING(
                f"Ingest directory does not exist: {ingest_dir}"))
            return

        ingested = skipped = failed = 0
        for md_file in sorted(ingest_dir.glob("*.md")):
            content = md_file.read_text(encoding="utf-8")
            try:
                summary = ingest_markdown(str(md_file), content)
            except (ValueError, MarkdownTooLargeError) as e:
                failed += 1
                self.stderr.write(self.style.ERROR(f"{md_file}: {e}"))
                continue
            if summary.get("alreadyIngested"):
                skipped += 1
                continue
            ingested += 1
            self.stdout.write(self.style.SUCCESS(
                f"Ingested {md_file.name}: {summary['termsCreated']} terms, "
                f"{summary['edgesCreated']} edges, {summary['symbolsFound']} symbols"))

        self.stdout.write(
            f"Done: {ingested} ingested, {skipped} already seen, {failed} failed.")
