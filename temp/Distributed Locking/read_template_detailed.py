#!/usr/bin/env python3
"""Detailed analysis of template content - body chapters."""
from docx import Document
from docx.shared import RGBColor
from docx.oxml.ns import qn

doc = Document("Fundamentals of Project Management Viet Anh.docx")

# Find the first Heading 1 paragraph (start of actual content)
started = False
chapter_count = 0
para_count = 0

for i, para in enumerate(doc.paragraphs):
    text = para.text.strip()
    if not text:
        continue
    
    style_name = para.style.name if para.style else "None"
    
    # Look for headings to understand structure
    if 'Heading' in style_name or 'heading' in style_name:
        started = True
        if 'Heading 1' in style_name:
            chapter_count += 1
            print(f"\n{'#' * 80}")
            print(f"CHAPTER: {text}")
            print(f"{'#' * 80}")
            para_count = 0
        elif 'Heading 2' in style_name:
            print(f"\n  ## {text}")
            para_count = 0
        elif 'Heading 3' in style_name:
            print(f"\n    ### {text}")
        elif 'Heading 4' in style_name:
            print(f"\n      #### {text}")
        continue
    
    if not started:
        continue
    
    para_count += 1
    if para_count > 15:  # Limit content per section
        if para_count == 16:
            print("      [...more content...]")
        continue
    
    # Check for special formatting
    has_highlight = False
    has_border = False
    highlight_colors = set()
    
    # Check paragraph borders (framing)
    pPr = para._element.find(qn('w:pPr'))
    if pPr is not None:
        pBdr = pPr.find(qn('w:pBdr'))
        if pBdr is not None:
            has_border = True
    
    for run in para.runs:
        if run.font.highlight_color:
            has_highlight = True
            highlight_colors.add(str(run.font.highlight_color))
    
    prefix = ""
    if has_highlight:
        prefix += f" [HIGHLIGHT:{','.join(highlight_colors)}]"
    if has_border:
        prefix += " [BORDER]"
    if '01_GachTruInput' in style_name:
        prefix += " [ENGLISH_INPUT]"
    if '01. GachTruDauDongViet' in style_name:
        prefix += " [VIETNAMESE]"
    if 'List' in style_name or 'list' in style_name:
        prefix += f" [LIST:{style_name}]"
    
    short_text = text[:200]
    print(f"      [{style_name}]{prefix} {short_text}")

print(f"\n\nTotal chapters found: {chapter_count}")
