# Graph Report - stickman-arena  (2026-10-16)

## Corpus Check
- 23 files · ~29,426 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 6 file(s) not represented in the graph (top: .asmdef 3, (none) 2, .fbx 1)

## Summary
- 239 nodes · 428 edges · 13 communities (10 shown, 3 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 17 edges (avg confidence: 0.93)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `e78e7df6`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- make_stickman.py
- StyleData
- manifest.json
- BuildScript.cs
- StickmanMotor
- unityengine
- DemoBootstrap
- StickmanView
- Mapa del código
- StickmanModelTests.cs
- context7
- BuildScript

## God Nodes (most connected - your core abstractions)
1. `DemoBootstrap` - 26 edges
2. `StickmanMotor` - 21 edges
3. `StickmanView` - 18 edges
4. `ArenaPhaseDirector` - 14 edges
5. `CollapsingDeck` - 14 edges
6. `StyleData` - 14 edges
7. `main()` - 14 edges
8. `FloatingRuins` - 13 edges
9. `StickmanArena` - 12 edges
10. `TopDownCamera` - 12 edges

## Surprising Connections (you probably didn't know these)
- `Flujo` --references--> `main()`  [INFERRED]
  CLAUDE.md → tools/blender/make_stickman.py
- `Mapa del código` --references--> `ProceduralStickman`  [INFERRED]
  CLAUDE.md → Assets/Scripts/Runtime/ProceduralStickman.cs
- `Mapa del código` --references--> `StickmanMotor`  [INFERRED]
  CLAUDE.md → Assets/Scripts/Runtime/StickmanMotor.cs
- `Mapa del código` --references--> `StickmanView`  [INFERRED]
  CLAUDE.md → Assets/Scripts/Runtime/StickmanView.cs
- `Mapa del código` --references--> `StyleData`  [INFERRED]
  CLAUDE.md → Assets/Scripts/Runtime/StyleData.cs

## Import Cycles
- None detected.

## Communities (13 total, 3 thin omitted)

### Community 0 - "make_stickman.py"
Cohesion: 0.12
Nodes (21): apply_modifiers(), attach_face(), build_actions(), build_armature(), build_body(), build_face(), clear_scene(), _closest_point_on_segment() (+13 more)

### Community 1 - "StyleData"
Cohesion: 0.17
Nodes (7): ProceduralStickman, StyleData, IsBalanced, Mass, MoveSpeed, MvpStyles, Sum

### Community 2 - "manifest.json"
Cohesion: 0.08
Nodes (23): dependencies, com.unity.modules.animation, com.unity.modules.audio, com.unity.modules.imgui, com.unity.modules.jsonserialize, com.unity.modules.particlesystem, com.unity.modules.physics, com.unity.modules.ui (+15 more)

### Community 3 - "BuildScript.cs"
Cohesion: 0.13
Nodes (3): StickmanModelImporter, IsStickman, StickmanArena.EditorTools

### Community 4 - "StickmanMotor"
Cohesion: 0.17
Nodes (7): KeyboardInput, StickmanMotor, IsGrounded, IsPunching, PlanarSpeed, PunchTimer, Velocity

### Community 6 - "DemoBootstrap"
Cohesion: 0.07
Nodes (19): ArenaPhaseDirector, CurrentPhaseIndex, CurrentPhaseName, MatchTime, WarningActive, Phase, CollapsingDeck, Length (+11 more)

### Community 7 - "StickmanView"
Cohesion: 0.15
Nodes (6): State, Idle, Jump, Punch, Run, StickmanView

### Community 9 - "Mapa del código"
Cohesion: 0.22
Nodes (6): Activar la licencia Personal de Unity para GitHub Actions, Cómo funciona sin editor, Diseño, Estado, Estructura, StickMan Arena

### Community 10 - "StickmanModelTests.cs"
Cohesion: 0.17
Nodes (3): StickmanModelTests, StyleDataTests, StickmanArena.Tests

### Community 12 - "context7"
Cohesion: 0.50
Nodes (3): npx, context7, @upstash/context7-mcp

## Knowledge Gaps
- **35 isolated node(s):** `npx`, `@upstash/context7-mcp`, `IsStickman`, `MatchTime`, `CurrentPhaseIndex` (+30 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 101 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **3 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `StickMan Arena: guía para Claude Code` connect `DemoBootstrap` to `Mapa del código`?**
  _High betweenness centrality (0.287) - this node is a cross-community bridge._
- **Are the 2 inferred relationships involving `DemoBootstrap` (e.g. with `Mapa del código` and `Reglas duras`) actually correct?**
  _`DemoBootstrap` has 2 INFERRED edges - model-reasoned connections that need verification._
- **What connects `npx`, `@upstash/context7-mcp`, `IsStickman` to the rest of the system?**
  _35 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `make_stickman.py` be split into smaller, more focused modules?**
  _Cohesion score 0.12121212121212122 - nodes in this community are weakly interconnected._
- **Why does `Mapa del código` connect `DemoBootstrap` to `StyleData`, `StickmanMotor`, `StickmanView`?**
  _High betweenness centrality (0.259) - this node is a cross-community bridge._
- **Should `manifest.json` be split into smaller, more focused modules?**
  _Cohesion score 0.08333333333333333 - nodes in this community are weakly interconnected._
- **Why does `main()` connect `make_stickman.py` to `Mapa del código`, `DemoBootstrap`?**
  _High betweenness centrality (0.248) - this node is a cross-community bridge._