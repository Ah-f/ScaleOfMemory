# Scales of Memory (기억의 비늘)

Slay the Spire에서 영감을 받은 로그라이트 덱빌딩 게임. Unity로 제작.

## 개요

옛날, 위대한 용이 세상의 기억을 비늘에 담아 지키고 있었습니다. 하지만 여섯 영웅이 용을 쓰러뜨리고 비늘을 세상 곳곳에 흩뜨렸습니다. 기억이 사라져가는 지금, 용의 마지막 후예인 당신이 비늘을 모아 잃어버린 것을 되찾아야 합니다.

## 주요 기능

- **카드 전투** - 마나 기반 턴제 전투, 7개 원소 카드 (Fire, Ice, Lightning, Water, Nature, Light, Dark), 상태이상 (Burn, Freeze, Poison)
- **비늘 방어 시스템** - 3층 방어 구조: Block > Scales > HP
- **던전 맵** - 절차적 생성 맵, 분기 경로, 총 3개 Phase
- **덱빌딩** - 전투 승리 후 카드 보상 선택, 덱 확인 기능
- **랜덤 이벤트** - 8종 스토리 이벤트 (선택지 + 보상/페널티)
- **인트로 스토리** - 타자기 효과 4페이지 세계관 스토리
- **다양한 적** - Flying, Splitting, Charging, Summoning 등 특성 보유

## 기술 스택

- Unity 2022 LTS
- C# / TextMeshPro
- 모든 UI 런타임 코드 생성 (프리팹 미사용)
- ScriptableObject 기반 카드/적 데이터

## 프로젝트 구조

```
Assets/ScalesOfMemory/
  Scripts/
    Core/       - GameManager, BattleManager, MapGenerator 등
    Data/       - CardData, EnemyData, EventData, RunState 등
    UI/         - BattleUIManager, MapUI, CardRewardUI, DeckViewUI 등
    Editor/     - AssetCreator (ScriptableObject 에셋 생성)
  ScriptableObjects/
    Cards/      - 카드 데이터 (28종)
    Enemies/    - 적 데이터
    StatusEffects/ - 상태이상 데이터
```

## 씬 구성

| 씬 | 설명 |
|----|------|
| MainMenuScene | 타이틀 화면, NEW RUN 버튼 |
| MapScene | 던전 맵 노드 선택 |
| BattleScene | 카드 전투 |

## 게임 루프

1. NEW RUN -> 인트로 스토리 -> 던전 맵
2. 노드 선택 (Battle / Elite / Event / Rest / Shop / Boss)
3. 전투 승리 -> 골드 획득 + 카드 보상 선택
4. 3개 Phase를 거쳐 보스 클리어
5. 최종 보스 격파 -> 게임 승리

## License

All rights reserved.
