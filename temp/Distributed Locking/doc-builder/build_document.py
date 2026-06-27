#!/usr/bin/env python3
"""Entry point: assemble the Distributed Locking Word document from the template
styles and the bilingual content."""
import os
from doc_builder import DocBuilder
from content_distributed_locking import build

OUT = os.path.join(os.path.dirname(__file__), "..",
                   "Distributed Locking - Viet Anh.docx")


def main():
    b = DocBuilder()
    b.cover_page(
        title_en="DISTRIBUTED LOCKING",
        title_vi="Khóa phân tán",
        members=["…………………………", "…………………………", "…………………………"],
    )
    b.table_of_contents()
    build(b)
    b.save(os.path.abspath(OUT))


if __name__ == "__main__":
    main()
