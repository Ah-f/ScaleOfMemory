# Scales of Memory

A roguelike deck-building game built with Unity, inspired by Slay the Spire.

## Overview

You are the last of the dragon's kin. The great dragon that once guarded the world's memories has fallen, its scales scattered across cursed temples and forgotten lands. Collect the scales, build your deck, and restore what was lost.

## Features

- **Card Combat** - Turn-based battles with mana, elemental cards (Fire, Ice, Lightning, Nature), and status effects (Burn, Freeze, Poison)
- **Scales Defense** - Unique 3-layer defense system: Block > Scales > HP
- **Dungeon Map** - Procedurally generated maps with branching paths across 3 phases
- **Deck Building** - Choose card rewards after victories, view your deck anytime
- **Random Events** - 8 story events with meaningful choices and trade-offs
- **Intro Story** - Typewriter-effect narrative introducing the world and quest
- **Enemy Variety** - Enemies with traits like Flying, Splitting, Charging, and Summoning

## Tech Stack

- Unity 2022 LTS
- C# / TextMeshPro
- All UI built at runtime (no prefabs)
- ScriptableObject-based card/enemy data

## Project Structure

```
Assets/ScalesOfMemory/
  Scripts/
    Core/       - GameManager, BattleManager, MapGenerator, etc.
    Data/       - CardData, EnemyData, EventData, RunState, etc.
    UI/         - BattleUIManager, MapUI, CardRewardUI, DeckViewUI, etc.
  Data/
    Cards/      - CardData ScriptableObjects
    Enemies/    - EnemyData ScriptableObjects
    RunConfig/  - Run configuration assets
```

## Scenes

| Scene | Description |
|-------|-------------|
| MainMenuScene | Title screen, NEW RUN button |
| MapScene | Dungeon map with node selection |
| BattleScene | Card combat |

## Game Loop

1. NEW RUN -> Intro Story -> Dungeon Map
2. Select a node (Battle / Elite / Event / Rest / Shop / Boss)
3. Win battles to earn gold and choose new cards
4. Progress through 3 phases, each ending with a boss
5. Defeat the final boss to win

## License

All rights reserved.
