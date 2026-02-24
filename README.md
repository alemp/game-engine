# 🎮 Game Engine Idle

Modular mobile game engine focused on **Idle Games**, built with Unity. One engine, multiple games—through configuration only.

---

## Documentation

| Document | Purpose |
|----------|---------|
| [**MASTER_SPEC.md**](MASTER_SPEC.md) | Full specification, architecture, and implementation decisions |
| [**/docs/NEXT_STEPS.md**](docs/NEXT_STEPS.md) | Implementation tracker with checkboxes |
| [**/docs/**](docs/) | Architecture notes, API docs, and other project documentation |

---

## Prerequisites

* **Unity 6** (6000.x) if it is stable, or Unity 2022.3 LTS
* .NET Standard 2.1
* Target platforms: Android, iOS (mobile-first)

---

## Getting Started

1. **Clone** the repository
2. **Open** `UnityProject/` in Unity
3. **Build** the active game via build config (see MASTER_SPEC §18.5)

> Full setup, folder structure, and development workflow are described in [MASTER_SPEC.md](MASTER_SPEC.md).

---

## Project Structure (Overview)

```
/
├─ README.md           # This file – project info & quick start
├─ MASTER_SPEC.md      # Full specification
├─ docs/               # Documentation
├─ UnityProject/       # Unity project
├─ Tools/              # Validator, Simulator (CLI)
└─ AI/                 # Prompts, rules
```

---

## License

See [LICENSE](LICENSE) if present.
