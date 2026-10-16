# StickMan Arena: guía para Claude Code

Brawler 3D de arena para 4 jugadores, estilo stickman Low Poly, inspirado en Power Stone 2. Unity 6 + URP.
El dueño es Edwin (escribe en español; responderle siempre en español). Commits y PRs en inglés o español, pero consistentes.

## Reglas duras
- **Ligero siempre**: Low Poly, colores planos, sin sombras en tiempo real, sin post-procesado. Stickman < 1.000 triángulos, arena < 50.000 triángulos por fase, 60 fps en gráfica integrada. Toda decisión de código y assets se mide con esto.
- **Balance**: cada estilo suma 12 puntos (velocidad, daño, alcance, peso, de 1 a 5). El salto es igual para todos. Hay una prueba que lo verifica.
- **Nombres propios**: nunca usar nombres ni diseños de Capcom. Los estilos tienen nombres propios (Puños de Roca, Aura Dorada, Boxeador, Pistolero...).
- **Plataformas**: solo navegador (WebGL) y escritorio (Windows/Linux). Nunca móvil: sin controles táctiles ni builds Android/iOS.
- **No hay editor abierto**: el proyecto se trabaja como código. La escena de la demo se construye por script (`DemoBootstrap`) y el build lo hace `BuildScript.BuildWebGL` en GitHub Actions. No agregar assets que requieran tocar el Inspector.

## Flujo
- Rama por cambio → PR → el workflow `WebGL` corre pruebas EditMode y compila WebGL. Al mergear a `main`, se publica en GitHub Pages.
- **Pruebas que gastan pocos tokens**: nunca leer el XML completo de resultados; usar `python3 tools/ci/test-summary.py TestResults`, que imprime solo totales y fallos.
- Binarios pequeños (fbx, glb, png, jpg < 1 MB) van en git normal; solo `.blend` y audio van por Git LFS (lfs.github.com no es accesible desde la nube de Claude y gasta cuota en cada build).
- Modelos: se generan con Blender sin pantalla desde `tools/blender/*.py` (módulo `bpy`), nunca a mano.

## Herramientas
- **Context7** (docs actualizadas): configurado en `.mcp.json`. Antes de escribir código con una API de Unity/URP que no sea básica, consultar Context7 (`resolve-library-id` → `get-library-docs`).
- **Graphify** (mapa del código, ya instalado): el grafo vive en `graphify-out/` (graph.json y GRAPH_REPORT.md versionados) y la skill en `.claude/skills/graphify`. Antes de releer archivos para entender el código, usar `graphify explain "Clase"`, `graphify path "A" "B"` o leer `graphify-out/GRAPH_REPORT.md`. Tras cambiar código: `graphify update .` (sin LLM, gratis). Si falta el CLI: `pip install graphifyy` (sí, con doble y). Un hook de git lo actualiza al hacer checkout o merge; para instalarlo en un clon nuevo: `graphify hook install`.

## Mapa del código
- `Assets/Scripts/Runtime/` lógica del juego (asmdef `StickmanArena.Runtime`).
  - `StyleData` estilos y estadísticas · `StickmanMotor` movimiento/salto/golpe · `StickmanView` personaje del FBX de Blender animado con Playables (Idle/Run/Punch/Jump, sin Animator Controller) · `ProceduralStickman` respaldo de primitivas si falta el modelo
  - `TopDownCamera` cámara cenital que encuadra a todos · `ArenaPhaseDirector` reloj de fases y avisos · `CollapsingDeck` piso que se derrumba por rangos de filas · `FloatingRuins` plataformas flotantes de la última fase · `FallAway` decorado que cae · `DemoBootstrap` arma la demo y reacciona a cada fase (cielo, alas, ruinas)
- `Assets/Scripts/Editor/BuildScript.cs` URP de bajo consumo, escena demo (se regenera en cada build con el modelo y sus clips) y build WebGL. `StickmanModelImporter.cs` ajusta la importación del FBX (Generic, sin materiales, bucles).
- `Assets/Models/Stickman/stickman.fbx` sale de `tools/blender/make_stickman.py` (Skin + subdivisión + pesos automáticos, < 1.000 triángulos). `docs/previews/` tiene la hoja de poses.
- `Assets/Tests/EditMode/` pruebas NUnit.
- `tools/blender/` generación de modelos · `tools/ci/` utilidades de CI.
- `docs/` guías (licencia de Unity, etc.).
