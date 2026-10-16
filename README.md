# StickMan Arena

Brawler 3D de arena para 2 a 4 jugadores con personajes stickman, inspirado en Power Stone 2. Low Poly y ligero: pensado para correr en el navegador (WebGL) y en PCs modestas.

**Jugar la última versión:** se publica automáticamente en GitHub Pages al mergear a `main` (enlace en la pestaña *Actions* o en *Settings > Pages*).

## Estado
Hito actual: **demo visual**. Un stickman animado (idle, correr, golpe, salto), una arena ("Cielo de Papel") con una transición cronometrada en la que el piso se derrumba por partes, y la cámara cenital que encuadra a los 4 jugadores.

Controles de la demo: `WASD` o flechas para moverse, `Espacio` salta, `J` golpea.

## Cómo funciona sin editor
Todo el proyecto es código: la escena se arma por script al iniciar y GitHub Actions (GameCI) compila la versión WebGL. Ver [CLAUDE.md](CLAUDE.md) para el mapa del código y [docs/licencia-unity.md](docs/licencia-unity.md) para activar la licencia de Unity en Actions (una vez).

## Estructura
- `Assets/Scripts/Runtime` lógica del juego
- `Assets/Scripts/Editor` build y preparación del proyecto
- `Assets/Tests` pruebas
- `Assets/Models` modelos generados con Blender (`tools/blender`)
- `.github/workflows` pruebas, build WebGL y publicación en Pages

## Diseño
El documento de diseño conceptual vive en Claude Docs (proyecto StickMan Arena). Estilos del MVP: Puños de Roca, Aura Dorada, Boxeador y Pistolero. Arenas del MVP: Cielo de Papel y Tumba del Garabato.
