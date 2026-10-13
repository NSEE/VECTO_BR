# (!) Execute in containing folder.
# DESCRIPTION:
#  Converts CHANGES.md release notes to PDF format.

pandoc '..\CHANGES.md' -o 'VECTO_AirDrag_ReleaseNotes.pdf' --css "includes\md-style.css"  -B includes/header.html --pdf-engine=$Env:weasyprint  --title="Changelog"
