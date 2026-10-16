# Graph Report - stickman-arena  (2026-10-16)

## Corpus Check
- 23 files · ~29,881 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 6 file(s) not represented in the graph (top: .asmdef 3, (none) 2, .fbx 1)

## Summary
- 228 nodes · 373 edges · 14 communities (11 shown, 3 thin omitted)
- Extraction: 97% EXTRACTED · 3% INFERRED · 0% AMBIGUOUS · INFERRED: 12 edges (avg confidence: 0.95)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `64648898`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- make_stickman.py
- CollapsingDeck
- manifest.json
- BuildScript.cs
- StickmanMotor
- unityengine
- ArenaPhaseDirector
- StickmanView
- StyleData
- Mapa del código
- StickmanModelTests.cs
- context7
- BuildScript

## God Nodes (most connected - your core abstractions)
1. `StickmanMotor` - 23 edges
2. `StickmanView` - 18 edges
3. `CollapsingDeck` - 15 edges
4. `main()` - 14 edges
5. `StyleData` - 13 edges
6. `ArenaPhaseDirector` - 12 edges
7. `StickmanArena` - 11 edges
8. `FloatingRuins` - 11 edges
9. `ProceduralStickman` - 11 edges
10. `TopDownCamera` - 10 edges

## Surprising Connections (you probably didn't know these)
- `Flujo` --references--> `main()`  [INFERRED]
  CLAUDE.md → tools/blender/make_stickman.py
- `Mapa del código` --references--> `ArenaPhaseDirector`  [INFERRED]
  CLAUDE.md → Assets/Scripts/Runtime/ArenaPhaseDirector.cs
- `Mapa del código` --references--> `CollapsingDeck`  [INFERRED]
  CLAUDE.md → Assets/Scripts/Runtime/CollapsingDeck.cs
- `Mapa del código` --references--> `FloatingRuins`  [INFERRED]
  CLAUDE.md → Assets/Scripts/Runtime/FloatingRuins.cs
- `Mapa del código` --references--> `ProceduralStickman`  [INFERRED]
  CLAUDE.md → Assets/Scripts/Runtime/ProceduralStickman.cs

## Import Cycles
- None detected.

## Communities (14 total, 3 thin omitted)

### Community 0 - "make_stickman.py"
Cohesion: 0.12
Nodes (21): apply_modifiers(), attach_face(), build_actions(), build_armature(), build_body(), build_face(), clear_scene(), _closest_point_on_segment() (+13 more)

### Community 1 - "CollapsingDeck"
Cohesion: 0.11
Nodes (6): CollapsingDeck, Length, StandingRows, FloatingRuins, SpawnPoint, ProceduralStickman

### Community 2 - "manifest.json"
Cohesion: 0.08
Nodes (23): dependencies, com.unity.modules.animation, com.unity.modules.audio, com.unity.modules.imgui, com.unity.modules.jsonserialize, com.unity.modules.particlesystem, com.unity.modules.physics, com.unity.modules.ui (+15 more)

### Community 3 - "BuildScript.cs"
Cohesion: 0.12
Nodes (3): StickmanModelImporter, IsStickman, StickmanArena.EditorTools

### Community 4 - "StickmanMotor"
Cohesion: 0.17
Nodes (7): KeyboardInput, StickmanMotor, IsGrounded, IsPunching, PlanarSpeed, PunchTimer, Velocity

### Community 6 - "ArenaPhaseDirector"
Cohesion: 0.20
Nodes (7): ArenaPhaseDirector, CurrentPhaseIndex, CurrentPhaseName, MatchTime, WarningActive, Phase, TopDownCamera

### Community 7 - "StickmanView"
Cohesion: 0.16
Nodes (6): State, Idle, Jump, Punch, Run, StickmanView

### Community 8 - "StyleData"
Cohesion: 0.25
Nodes (6): StyleData, IsBalanced, Mass, MoveSpeed, MvpStyles, Sum

### Community 9 - "Mapa del código"
Cohesion: 0.12
Nodes (11): FallAway, Flujo, Herramientas, Mapa del código, StickMan Arena: guía para Claude Code, Activar la licencia Personal de Unity para GitHub Actions, Cómo funciona sin editor, Diseño (+3 more)

### Community 10 - "StickmanModelTests.cs"
Cohesion: 0.19
Nodes (3): StickmanModelTests, StyleDataTests, StickmanArena.Tests

### Community 12 - "context7"
Cohesion: 0.50
Nodes (3): npx, context7, @upstash/context7-mcp

## Knowledge Gaps
- **35 isolated node(s):** `npx`, `@upstash/context7-mcp`, `IsStickman`, `MatchTime`, `CurrentPhaseIndex` (+30 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 104 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **3 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `StickMan Arena: guía para Claude Code` connect `Mapa del código` to `BuildScript`?**
  _High betweenness centrality (0.299) - this node is a cross-community bridge._
- **Are the 2 inferred relationships involving `main()` (e.g. with `Flujo` and `StickMan Arena`) actually correct?**
  _`main()` has 2 INFERRED edges - model-reasoned connections that need verification._
- **What connects `npx`, `@upstash/context7-mcp`, `IsStickman` to the rest of the system?**
  _35 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `make_stickman.py` be split into smaller, more focused modules?**
  _Cohesion score 0.12121212121212122 - nodes in this community are weakly interconnected._
- **Why does `Mapa del código` connect `Mapa del código` to `CollapsingDeck`, `StickmanMotor`, `ArenaPhaseDirector`, `StickmanView`, `StyleData`?**
  _High betweenness centrality (0.290) - this node is a cross-community bridge._
- **Should `CollapsingDeck` be split into smaller, more focused modules?**
  _Cohesion score 0.11494252873563218 - nodes in this community are weakly interconnected._
- **Why does `main()` connect `make_stickman.py` to `Mapa del código`?**
  _High betweenness centrality (0.255) - this node is a cross-community bridge._