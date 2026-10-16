# StickMan Arena: guía para Claude Code

Brawler 3D de arena para 4 jugadores, estilo stickman Low Poly, inspirado en Power Stone 2. Unity 6 + URP.
El dueño es Edwin (escribe en español; responderle siempre en español). Commits y PRs en inglés o español, pero consistentes.

## Reglas duras
- **Ligero siempre**: Low Poly, colores planos, sin sombras en tiempo real, sin post-procesado. Stickman < 1.000 triángulos, arena < 50.000 triángulos por fase, 60 fps en gráfica integrada. Toda decisión de código y assets se mide con esto.
- **Balance**: cada estilo suma 12 puntos (velocidad, daño, alcance, peso, de 1 a 5). El salto es igual para todos. Hay una prueba que lo verifica.
- **Nombres propios**: nunca usar nombres ni diseños de Capcom. Los estilos tienen nombres propios (Puños de Roca, Aura Dorada, Boxeador, Pistolero...).
- **No hay editor abierto**: el proyecto se trabaja como código. La escena de la demo se construye por script (`DemoBootstrap`) y el build lo hace `BuildScript.BuildWebGL` en GitHub Actions. No agregar assets que requieran tocar el Inspector.

## Flujo
- Rama por cambio → PR → el workflow `WebGL` corre pruebas EditMode y compila WebGL. Al mergear a `main`, se publica en GitHub Pages.
- **Pruebas que gastan pocos tokens**: nunca leer el XML completo de resultados; usar `python3 tools/ci/test-summary.py TestResults`, que imprime solo totales y fallos.
- Assets binarios (fbx, glb, png, wav) van por Git LFS.
- Modelos: se generan con Blender sin pantalla desde `tools/blender/*.py` (módulo `bpy`), nunca a mano.

## Herramientas
- **Context7** (docs actualizadas): configurado en `.mcp.json`. Antes de escribir código con una API de Unity/URP que no sea básica, consultar Context7 (`resolve-library-id` → `get-library-docs`).
- **Graphify** (mapa del código): cuando el repo crezca, consultar el grafo en vez de releer archivos. Instalación en Claude Code: `/plugin marketplace add safishamsi/graphify` y `/plugin install graphify`, luego `/graphify` en la raíz. (Verificar el comando exacto en el README del plugin si cambia.)

## Mapa del código
- `Assets/Scripts/Runtime/` lógica del juego (asmdef `StickmanArena.Runtime`).
  - `StyleData` estilos y estadísticas · `StickmanMotor` movimiento/salto/golpe · `ProceduralStickman` personaje de primitivas animado por código
  - `TopDownCamera` cámara cenital que encuadra a todos · `ArenaPhaseDirector` reloj de fases y avisos · `CollapsingDeck` piso que se derrumba · `DemoBootstrap` arma la demo
- `Assets/Scripts/Editor/BuildScript.cs` URP de bajo consumo, escena demo y build WebGL.
- `Assets/Tests/EditMode/` pruebas NUnit.
- `tools/blender/` generación de modelos · `tools/ci/` utilidades de CI.
- `docs/` guías (licencia de Unity, etc.).
