# Graph Report - stickman-arena  (2026-10-16)

## Corpus Check
- 18 files · ~7,324 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 5 file(s) not represented in the graph (top: .asmdef 3, (none) 2)

## Summary
- 182 nodes · 279 edges · 14 communities (10 shown, 4 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 14 edges (avg confidence: 0.94)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `90e21f65`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- make_stickman.py
- CollapsingDeck
- manifest.json
- BuildScript.cs
- StickmanMotor
- StickmanArena
- ArenaPhaseDirector
- DemoBootstrap
- StyleData
- StickMan Arena
- StyleDataTests.cs
- context7
- Generador del Stickman (Blender / bpy)

## God Nodes (most connected - your core abstractions)
1. `StickmanMotor` - 19 edges
2. `ArenaPhaseDirector` - 14 edges
3. `CollapsingDeck` - 13 edges
4. `DemoBootstrap` - 13 edges
5. `StyleData` - 12 edges
6. `TopDownCamera` - 12 edges
7. `ProceduralStickman` - 11 edges
8. `main()` - 11 edges
9. `StickmanArena` - 9 edges
10. `Mapa del código` - 8 edges

## Surprising Connections (you probably didn't know these)
- `Flujo` --references--> `main()`  [INFERRED]
  CLAUDE.md → tools/blender/make_stickman.py
- `Mapa del código` --references--> `ArenaPhaseDirector`  [INFERRED]
  CLAUDE.md → Assets/Scripts/Runtime/ArenaPhaseDirector.cs
- `Mapa del código` --references--> `CollapsingDeck`  [INFERRED]
  CLAUDE.md → Assets/Scripts/Runtime/CollapsingDeck.cs
- `Mapa del código` --references--> `ProceduralStickman`  [INFERRED]
  CLAUDE.md → Assets/Scripts/Runtime/ProceduralStickman.cs
- `Mapa del código` --references--> `StickmanMotor`  [INFERRED]
  CLAUDE.md → Assets/Scripts/Runtime/StickmanMotor.cs

## Import Cycles
- None detected.

## Communities (14 total, 4 thin omitted)

### Community 0 - "make_stickman.py"
Cohesion: 0.12
Nodes (17): add_ball(), add_capsule(), build_actions(), build_armature(), build_mesh(), clear_scene(), export(), join_objects() (+9 more)

### Community 1 - "CollapsingDeck"
Cohesion: 0.15
Nodes (4): CollapsingDeck, Length, ProceduralStickman, TopDownCamera

### Community 2 - "manifest.json"
Cohesion: 0.08
Nodes (23): dependencies, com.unity.modules.animation, com.unity.modules.audio, com.unity.modules.imgui, com.unity.modules.jsonserialize, com.unity.modules.particlesystem, com.unity.modules.physics, com.unity.modules.ui (+15 more)

### Community 4 - "StickmanMotor"
Cohesion: 0.17
Nodes (7): KeyboardInput, StickmanMotor, IsGrounded, IsPunching, PlanarSpeed, PunchTimer, Velocity

### Community 6 - "ArenaPhaseDirector"
Cohesion: 0.27
Nodes (6): ArenaPhaseDirector, CurrentPhaseIndex, CurrentPhaseName, MatchTime, WarningActive, Phase

### Community 7 - "DemoBootstrap"
Cohesion: 0.28
Nodes (6): DemoBootstrap, Flujo, Herramientas, Mapa del código, Reglas duras, StickMan Arena: guía para Claude Code

### Community 8 - "StyleData"
Cohesion: 0.22
Nodes (6): StyleData, IsBalanced, Mass, MoveSpeed, MvpStyles, Sum

### Community 9 - "StickMan Arena"
Cohesion: 0.22
Nodes (6): Activar la licencia Personal de Unity para GitHub Actions, Cómo funciona sin editor, Diseño, Estado, Estructura, StickMan Arena

### Community 12 - "context7"
Cohesion: 0.50
Nodes (3): npx, context7, @upstash/context7-mcp

### Community 13 - "Generador del Stickman (Blender / bpy)"
Cohesion: 0.33
Nodes (5): Generador del Stickman (Blender / bpy), Importar en Unity, Qué produce, Requisitos, Uso

## Knowledge Gaps
- **28 isolated node(s):** `npx`, `@upstash/context7-mcp`, `MatchTime`, `CurrentPhaseIndex`, `WarningActive` (+23 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 86 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **4 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `StickMan Arena: guía para Claude Code` connect `DemoBootstrap` to `StickMan Arena`?**
  _High betweenness centrality (0.318) - this node is a cross-community bridge._
- **Are the 2 inferred relationships involving `DemoBootstrap` (e.g. with `Mapa del código` and `Reglas duras`) actually correct?**
  _`DemoBootstrap` has 2 INFERRED edges - model-reasoned connections that need verification._
- **What connects `npx`, `@upstash/context7-mcp`, `MatchTime` to the rest of the system?**
  _28 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `make_stickman.py` be split into smaller, more focused modules?**
  _Cohesion score 0.1225071225071225 - nodes in this community are weakly interconnected._
- **Why does `Mapa del código` connect `DemoBootstrap` to `StyleData`, `CollapsingDeck`, `StickmanMotor`, `ArenaPhaseDirector`?**
  _High betweenness centrality (0.284) - this node is a cross-community bridge._
- **Should `CollapsingDeck` be split into smaller, more focused modules?**
  _Cohesion score 0.1476923076923077 - nodes in this community are weakly interconnected._
- **Why does `main()` connect `make_stickman.py` to `StickMan Arena`, `DemoBootstrap`?**
  _High betweenness centrality (0.283) - this node is a cross-community bridge._