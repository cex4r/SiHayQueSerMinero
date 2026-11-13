REPORTE MUSIC DATABASE - FUENTES LATEX

Contenidos:
- Reporte_Music_Database.tex: fuente editable en LaTeX.
- Reporte_Music_Database.pdf: documento renderizado (17 paginas).
- assets/search.png, assets/library.png, assets/songform.png:
  imagenes de referencia tomadas de los bocetos aportados al proyecto.

Requisitos: distribucion TeX con XeLaTeX, fontspec, TikZ, tcolorbox,
font Liberacion Serif / Liberation Sans y paquetes estandar.

Compilar desde esta carpeta:
  xelatex -interaction=nonstopmode Reporte_Music_Database.tex
  xelatex -interaction=nonstopmode Reporte_Music_Database.tex

Nota: no muevas la carpeta assets si deseas compilar sin editar las rutas.
El reporte recoge el estado observado del codigo fuente y del desarrollo
hasta el 13 de noviembre de 2026. Los ajustes pendientes se distinguen
de las funciones implementadas.
