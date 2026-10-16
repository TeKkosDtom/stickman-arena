# Graph Report - stickman-arena  (2026-10-16)

## Corpus Check
- 17 files · ~26,750 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 6 file(s) not represented in the graph (top: .asmdef 3, (none) 2, .fbx 1)

## Summary
- 182 nodes · 293 edges · 13 communities (9 shown, 4 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 13 edges (avg confidence: 0.93)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `2fb72cec`
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

## God Nodes (most connected - your core abstractions)
1. `StickmanMotor` - 19 edges
2. `ArenaPhaseDirector` - 14 edges
3. `main()` - 14 edges
4. `CollapsingDeck` - 13 edges
5. `DemoBootstrap` - 13 edges
6. `StyleData` - 12 edges
7. `TopDownCamera` - 12 edges
8. `ProceduralStickman` - 11 edges
9. `StickmanArena` - 9 edges
10. `select_only()` - 8 edges

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

## Communities (13 total, 4 thin omitted)

### Community 0 - "make_stickman.py"
Cohesion: 0.12
Nodes (21): apply_modifiers(), attach_face(), build_actions(), build_armature(), build_body(), build_face(), clear_scene(), _closest_point_on_segment() (+13 more)

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

## Knowledge Gaps
- **28 isolated node(s):** `npx`, `@upstash/context7-mcp`, `MatchTime`, `CurrentPhaseIndex`, `WarningActive` (+23 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 83 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **4 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `StickMan Arena: guía para Claude Code` connect `DemoBootstrap` to `StickMan Arena`?**
  _High betweenness centrality (0.286) - this node is a cross-community bridge._
- **Are the 2 inferred relationships involving `main()` (e.g. with `Flujo` and `StickMan Arena`) actually correct?**
  _`main()` has 2 INFERRED edges - model-reasoned connections that need verification._
- **What connects `npx`, `@upstash/context7-mcp`, `MatchTime` to the rest of the system?**
  _28 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `make_stickman.py` be split into smaller, more focused modules?**
  _Cohesion score 0.12121212121212122 - nodes in this community are weakly interconnected._
- **Why does `Mapa del código` connect `DemoBootstrap` to `StyleData`, `CollapsingDeck`, `StickmanMotor`, `ArenaPhaseDirector`?**
  _High betweenness centrality (0.259) - this node is a cross-community bridge._
- **Are the 2 inferred relationships involving `DemoBootstrap` (e.g. with `Mapa del código` and `Reglas duras`) actually correct?**
  _`DemoBootstrap` has 2 INFERRED edges - model-reasoned connections that need verification._
- **Should `CollapsingDeck` be split into smaller, more focused modules?**
  _Cohesion score 0.1476923076923077 - nodes in this community are weakly interconnected._