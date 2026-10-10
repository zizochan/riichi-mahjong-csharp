# Mahjong Logic Library

日本式リーチ麻雀のルール処理を行う C# ライブラリです。描画やUIには依存せず、
**牌・山・手牌の管理、向聴数計算、和了判定、役判定、符・点数計算、1局の進行、シンプルなCPU思考**
までをまとめて提供します。

- UnityEngine などのエンジンや外部ライブラリに依存しない純粋な C# コード（Unity でもそのまま動作）
- 人数・牌構成・役リスト・点数ルールを差し替えられる設計
- 標準役 42 種を実装済み（ダブル役満を含む）
- NUnit によるテスト付き

> English summary is available at the [bottom of this page](#english-summary).

## 目次

- [動作環境](#動作環境)
- [導入方法](#導入方法)
- [クイックスタート](#クイックスタート)
- [全体像](#全体像)
- [使い方](#使い方)
- [デフォルトのルール](#デフォルトのルール)
- [実装済みの役](#実装済みの役)
- [未対応のルール・制限事項](#未対応のルール制限事項)
- [拡張する](#拡張する)
- [テスト](#テスト)
- [ライセンス](#ライセンス)

## 動作環境

- C# 9.0 以降（`is A or B` パターン等を使用）
- .NET Standard 2.1 相当の API（`HashCode` 等）
- Unity の場合は C# 9.0 に対応した 2021.2 以降が目安です（開発・テストは Unity 6000.4 で実施）
- 外部ライブラリへの依存なし（テストのみ NUnit を使用）

## 導入方法

`*.cs` ファイルをそのまま自分のプロジェクトにコピーして使います（パッケージとしての配布は行っていません）。

1. このリポジトリを clone するか、ZIP でダウンロードします。
2. `Tests/` 以外のフォルダ（`Tiles/`, `Players/`, `Rules/`, `Judgment/`, `Yaku/`, `Scoring/`, `Flow/`, `Cpu/`,
   `Generation/`）を、プロジェクトの任意の場所にコピーします。
3. ルート名前空間は `Zizo.Mahjong` です。必要な名前空間を `using` して使ってください。

### Unity の場合

`Assets/` 以下の任意のフォルダにコピーすれば動作します。他のコードとアセンブリを分けたい場合は、
コピーしたフォルダに Assembly Definition を作成してください（UnityEngine を参照しないため、
`No Engine References` を有効にできます）。

### Unity 以外の .NET の場合

`netstandard2.1` / `net6.0` 以降などのプロジェクトに含めてください。C# 9.0 以降が必要です。

## クイックスタート

```csharp
using System;
using System.Collections.Generic;
using Zizo.Mahjong.Rules;
using Zizo.Mahjong.Tiles;
using Zizo.Mahjong.Players;
using Zizo.Mahjong.Yaku;
using Zizo.Mahjong.Scoring;

// 手牌（和了牌を含む14枚）と和了状況から、役・符・点数を計算する
var tiles = new List<Tile>
{
    new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
    new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7),
    new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
    new Tile(TileSuit.Pin, 9), new Tile(TileSuit.Pin, 9),
    new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7), new Tile(TileSuit.Sou, 8),
};
var east = new TileKind(TileSuit.Wind, 1);
var situation = new WinningSituation(
    melds: new List<Meld>(),
    winningTile: new Tile(TileSuit.Sou, 6),
    isTsumo: false,
    seatWind: new TileKind(TileSuit.Wind, 2), // 南家（子）
    roundWind: east,
    isRiichi: true
);

MahjongRuleSet ruleSet = MahjongRuleSet.CreateStandardFourPlayer();
YakuEvaluationResult result = YakuEvaluator.EvaluateBest(tiles, calledMeldCount: 0, situation, ruleSet.EnabledYaku);

foreach ((IYaku yaku, int han) in result.ScoringYaku)
{
    Console.WriteLine($"{yaku.Name} {han}翻"); // 立直 1翻 / 平和 1翻
}

int fu = FuCalculator.Calculate(result.Context, ruleSet.ScoringRule);
ScoreResult score = ScoreCalculator.Calculate(result.TotalHan, fu, winnerIsDealer: false, ruleSet.ScoringRule);
Console.WriteLine($"{result.TotalHan}翻{fu}符 {score.RonPayment}点"); // 2翻30符 2000点
```

## 全体像

```
MahjongRuleSet        … 人数・牌構成・役リスト・点数ルールをまとめた設定
  └ RoundController   … 1局分（配牌〜和了/流局）の進行を管理する
       ├ Wall              … 山（生牌山・王牌・ドラ表示牌・嶺上牌）
       ├ Players           … PlayerState のリスト（手牌・鳴き・捨て牌・点数）
       └ ICpuStrategy      … CPU の打牌/鳴き/リーチ判断（任意）
```

| 名前空間 | 主なクラス | 役割 |
|---|---|---|
| `Zizo.Mahjong.Tiles` | `Tile`, `TileKind`, `TileSetDefinition`, `Wall` | 牌の定義・牌構成・山 |
| `Zizo.Mahjong.Players` | `Hand`, `Meld`, `DiscardPile`, `PlayerState` | プレイヤーの手牌・鳴き・捨て牌・点数 |
| `Zizo.Mahjong.Rules` | `MahjongRuleSet`, `WallRule`, `ScoringRule` | 可変ルールの設定 |
| `Zizo.Mahjong.Judgment` | `ShantenCalculator`, `HandDecomposer`, `WinningHandDetector`, `ActionValidator` | 向聴数・面子分解・和了/聴牌・鳴き/リーチ可否・フリテン |
| `Zizo.Mahjong.Yaku` | `IYaku`, `YakuEvaluator`, `WinningSituation`, `WinningContext` | 役判定 |
| `Zizo.Mahjong.Yaku.Standard` | `StandardYakuSet`, 各役クラス | 標準役の実装 |
| `Zizo.Mahjong.Scoring` | `FuCalculator`, `DoraCalculator`, `ScoreCalculator`, `ScoreManager` | 符・ドラ・点数計算と点数移動 |
| `Zizo.Mahjong.Flow` | `RoundController`, `TurnManager` | 1局の進行 |
| `Zizo.Mahjong.Cpu` | `PlayerView`, `ICpuStrategy`, `DefaultCpuStrategy` | CPU の意思決定 |
| `Zizo.Mahjong.Generation` | `RandomWinningHandGenerator` | 役が確定した和了手牌のランダム生成 |

### 牌の表現

- `TileKind` は牌の種類（`Suit` + `Rank`）です。数牌は `Rank` 1〜9、風牌は 1=東 2=南 3=西 4=北、
  三元牌は 1=白 2=發 3=中 です。
- `Tile` は牌1枚で、`TileKind` に赤ドラフラグを加えたものです。**`Tile.Equals` は赤ドラかどうかを区別しません**
  （手牌の形の判定では赤5と通常の5を同じ牌として扱うため）。

## 使い方

### 1. ルールセットを作る

```csharp
using Zizo.Mahjong.Rules;

// 標準的な4人麻雀（136枚・赤5各1枚・標準役一式）
MahjongRuleSet ruleSet = MahjongRuleSet.CreateStandardFourPlayer();
```

人数・牌構成・役・点数ルールはコンストラクタで差し替えられます。

```csharp
using Zizo.Mahjong.Tiles;
using Zizo.Mahjong.Yaku.Standard;

var customRuleSet = new MahjongRuleSet(
    playerCount: 3,                                              // 2〜4人
    tileSet: TileSetDefinition.CreateStandard(redDoraCountPerNumberSuit: 0),
    enabledYaku: StandardYakuSet.CreateAll(),
    wallRule: new WallRule(),
    scoringRule: new ScoringRule(useKiriageMangan: true)
);
```

牌構成を変える例（白を6枚にする）:

```csharp
List<TileSetEntry> entries = TileSetDefinition.CreateStandard().Entries.ToList();
var white = new TileKind(TileSuit.Dragon, 1);
int index = entries.FindIndex(entry => entry.Kind.Equals(white));
entries[index] = new TileSetEntry(white, count: 6);
var customTileSet = new TileSetDefinition(entries);
```

### 2. 1局を開始する

```csharp
using Zizo.Mahjong.Flow;

var controller = new RoundController(
    ruleSet,
    roundWind: new TileKind(TileSuit.Wind, 1), // 東場
    dealerSeatIndex: 0,
    initialScorePerPlayer: 25000,
    honbaCount: 0,
    riichiStickCount: 0,
    randomSource: null // new DefaultRandomSource(seed) を渡すと山の並びを固定できる
);

// controller.Players[i].Hand.ConcealedTiles に配牌13枚が入っている
```

`RoundController` は **1局分** の処理だけを受け持ちます。半荘・東風戦の進行、親の交代、連荘、
本場・供託の持ち越しは呼び出し側で管理し、局ごとに `RoundController` を作り直してください。

また、手番の順序や鳴きの優先順位（ロン > ポン/カン > チー）の制御も呼び出し側の責務です。
`RoundController` は「今この操作ができるか」の問い合わせと、操作の実行を提供します。

### 3. ツモ〜打牌〜鳴きの基本ループ

```csharp
int seat = controller.CurrentTurnSeatIndex;

// ツモ
Tile drawn = controller.DrawForCurrentPlayer();

// ツモ和了の確認（役が無ければ HasYaku が false になる）
YakuEvaluationResult tsumoCheck = controller.EvaluateYaku(seat, drawn, isTsumo: true);
if (tsumoCheck.HasYaku)
{
    WinOutcome outcome = controller.TryWin(seat, drawn, isTsumo: true);
    // outcome.TotalHan / outcome.Fu / outcome.Score / outcome.TotalGain
    // 各プレイヤーの点数への反映は TryWin 内で完了している
}

// 打牌（isRiichiDeclaration: true でリーチ宣言。供託1000点の支払いも行われる）
DiscardedTile discarded = controller.Discard(seat, drawn);

// 他家の鳴き・ロンの機会を確認
List<CallOpportunity> opportunities = controller.GetCallOpportunities(seat, discarded.Tile);
foreach (CallOpportunity opportunity in opportunities)
{
    if (opportunity.CanRon)
    {
        WinOutcome ronOutcome = controller.TryWin(
            opportunity.SeatIndex,
            discarded.Tile,
            isTsumo: false,
            discarderSeatIndex: seat
        );
    }
    else if (opportunity.CanPon)
    {
        // ExecutePon / ExecuteChi / ExecuteOpenKan を呼ぶと、手番は鳴いたプレイヤーに移る
        controller.ExecutePon(opportunity.SeatIndex, seat, discarded.Tile);
    }
}

// 誰も鳴かなければ次の人へ
controller.AdvanceTurnToNext();
```

- `GetCallOpportunities` は、フリテン・役の有無を考慮したロン可否と、チー（上家のみ）・ポン・明槓の可否を返します。
  リーチ中のプレイヤーと、山の最後の打牌（河底牌）に対しては、ロン以外の選択肢は返しません。
- `TryWin` はフリテンを確認しません。ロンの前には `CanDeclareRon` または `GetCallOpportunities` で確認してください。
- リーチ宣言の可否は `ActionValidator.CanRiichi` で確認できます（`Discard` 側では検証しません）。

### 4. カン

自分の手番中に行う暗槓・加槓:

```csharp
using Zizo.Mahjong.Judgment;

List<TileKind> closedKanCandidates = ActionValidator.GetClosedKanCandidates(controller.Players[seat].Hand);
if (closedKanCandidates.Count > 0 && controller.Wall.CanDrawFromDeadWall)
{
    Tile rinshanTile = controller.ExecuteClosedKan(seat, closedKanCandidates[0]);
    // 嶺上牌でツモ和了する場合は EvaluateYaku / TryWin に isRinshan: true を渡す
}
```

加槓の前には、槍槓（他家のロン）の機会を確認してください:

```csharp
List<int> chankanWinners = controller.GetChankanRonSeats(seat, kind);
if (chankanWinners.Count == 0)
{
    controller.ExecuteAddedKan(seat, kind);
}
// 槍槓する場合は TryWin(..., isTsumo: false, discarderSeatIndex: seat, isChankan: true)
```

カン（暗槓を含む）が行われると、新ドラ表示牌がめくられ、全員の一発と第一巡（天和・地和・人和・
ダブルリーチ・九種九牌の条件）が消滅します。

### 5. 流局

```csharp
if (controller.IsExhaustiveDraw)
{
    // 荒牌平局のテンパイ料を清算する（点数への反映も行われる）
    Dictionary<int, int> deltas = controller.SettleExhaustiveDraw();
}

// 九種九牌を宣言できるか（流局として扱うかどうかは呼び出し側で決める）
bool canAbort = controller.CanDeclareKyuushuKyuuhai(seat);
```

### 6. CPU に打たせる

`ICpuStrategy` は `PlayerView`（そのプレイヤーに見えてよい情報だけをまとめたもの）を受け取って判断します。
他家の手牌は渡されないため、戦略の実装でイカサマをすることはできません。

```csharp
using Zizo.Mahjong.Cpu;

ICpuStrategy strategy = new DefaultCpuStrategy();
PlayerView view = PlayerView.Create(controller, seat);

Tile discard = strategy.ChooseDiscard(view, ruleSet);           // 14枚から切る牌を選ぶ
bool riichi = strategy.ShouldDeclareRiichi(view, ruleSet);      // 13枚で聴牌時にリーチするか
CallDecision decision = strategy.DecideCall(view, ruleSet, opportunity, discardedTile);
// decision.Type: None / Chi / Pon / OpenKan / Ron
```

`DefaultCpuStrategy` はシンプルな戦略です。

- 打牌: 向聴数が最も小さくなる牌を切り、同じなら見えている牌から数えた受け入れ枚数が多い方を選ぶ
- 鳴き: ロンは常に行う。チー・ポンは向聴数が進む場合のみ。カンはしない
- リーチ: 面前で聴牌し、供託分の点数と残り枚数があれば常に宣言する
- リーチ後のツモ切りは行わないため、リーチ中は呼び出し側でツモ切りを行ってください

### 7. 役・点数計算だけを使う

`RoundController` を使わずに、役・符・点数だけを計算することもできます（[クイックスタート](#クイックスタート)を参照）。

```csharp
YakuEvaluationResult result = YakuEvaluator.EvaluateBest(concealedTiles, melds.Count, situation, ruleSet.EnabledYaku);
if (result.HasYaku)
{
    int fu = FuCalculator.Calculate(result.Context, ruleSet.ScoringRule);
    int totalHan = result.TotalHan;
    int yakumanMultiplier = 0;
    if (result.HasYakuman)
    {
        yakumanMultiplier = result.TotalHan; // 役満の場合、TotalHan は「何倍役満か」を表す
    }
    else
    {
        totalHan += DoraCalculator.CountDora(result.Context.AllTiles, doraIndicators)
            + DoraCalculator.CountRedDora(result.Context.AllTiles);
    }
    ScoreResult score = ScoreCalculator.Calculate(totalHan, fu, winnerIsDealer, ruleSet.ScoringRule, yakumanMultiplier);
}
```

- `concealedTiles` は鳴いた面子を除いた手の内の牌（和了牌を含む）です。
- 鳴いた面子は `WinningSituation.Melds` に `Meld` として渡します。暗槓は `MeldType.ClosedKan` です。
- 向聴数・待ち牌だけを知りたい場合は `ShantenCalculator` / `WinningHandDetector` を使います。

### 8. 和了手牌をランダムに生成する

点数計算の練習問題などに使える、役が確定した和了手牌を生成できます。

```csharp
using Zizo.Mahjong.Generation;

GeneratedWinningHand hand = RandomWinningHandGenerator.Generate(
    ruleSet,
    new DefaultRandomSource(seed),
    chiProbability: 0.3f,     // 順子の1つをチーにする確率
    redDoraProbability: 0.5f  // 5の牌を赤ドラにする確率
);
// hand.AllTiles / hand.WinningTile / hand.YakuResult / hand.Fu / hand.Score
```

和了者は常に子、自風・場風は東として計算します。

## デフォルトのルール

`MahjongRuleSet.CreateStandardFourPlayer()` のルールです。`ScoringRule` / `WallRule` / 役リストで変更できます。

| 項目 | 既定値 |
|---|---|
| 人数 | 4人 |
| 牌 | 136枚（赤5を萬子・筒子・索子に各1枚） |
| 王牌 | 14枚、カンは4回まで |
| 喰いタン | あり |
| 切り上げ満貫 | なし（`ScoringRule(useKiriageMangan: true)` で有効化） |
| 数え役満 | あり（13翻以上） |
| ダブル役満 | あり（国士無双十三面待ち・四暗刻単騎・純正九蓮宝燈・大四喜） |
| 役満の複合 | あり（倍率を合算） |
| 人和 | 役満 |
| 積み棒 | ロン300点 / ツモ各100点 |
| 供託（リーチ棒） | 1000点 |
| 流局時のテンパイ料 | 合計3000点 |

## 実装済みの役

| 翻数 | 役 |
|---|---|
| 1翻 | 立直、一発、門前清自摸和、平和、断幺九、一盃口、役牌（三元牌・自風・場風）、嶺上開花、槍槓、海底摸月、河底撈魚 |
| 2翻 | 両立直、七対子、対々和、三暗刻、三色同刻、三槓子、混老頭、小三元、混全帯幺九※、一気通貫※、三色同順※ |
| 3翻 | 二盃口、純全帯幺九※、混一色※ |
| 6翻 | 清一色※ |
| 役満 | 天和、地和、人和、国士無双、四暗刻、大三元、小四喜、字一色、緑一色、清老頭、四槓子、九蓮宝燈 |
| ダブル役満 | 国士無双十三面待ち、四暗刻単騎、大四喜、純正九蓮宝燈 |

※ 鳴くと1翻下がります（食い下がり）。ドラ・裏ドラ・赤ドラは役ではなく、和了時に翻数へ加算されます。

## 未対応のルール・制限事項

- **局をまたぐ進行**: 半荘・東風戦、親の交代、連荘、本場の加算は呼び出し側で実装する必要があります。
- **途中流局**: 九種九牌の宣言可否の判定のみ対応しています。四風連打・四家立直・四開槓（四槓散了）・三家和了は未対応です。
- **複数人のロン**: ダブロン・トリプルロン・頭ハネの解決は呼び出し側に任せています（`GetCallOpportunities` は該当者を全員返します）。
- **フリテン**: 自分の捨て牌に待ち牌がある場合のフリテンのみ判定します。同巡内フリテン・立直後の見逃しによるフリテンは未対応です。
- **その他未対応のルール**: 流し満貫、包（パオ）、喰い替え禁止、立直後の暗槓による待ち変化の禁止。
- **三人麻雀**: 人数を3人にした場合の座席・手番・テンパイ料の分配には対応しています。北抜きドラ、萬子2〜8の除外、ツモ損なしなどの三麻固有ルールは、牌構成や点数ルールを自分で調整する必要があります。
- **高点法**: 複数の面子分解がある場合は、翻数が最も高い解釈を採用します（翻数が同じ場合、符の比較は行いません）。
  採用した分解の中で和了牌が両面・嵌張のどちらとも取れる場合、符は高くなる方（嵌張・辺張）で計算します。

## 拡張する

### 役を追加する

`IYaku` を実装したクラスを作り、`MahjongRuleSet` に渡す役リストへ追加します。既存のクラスを変更する必要はありません。

```csharp
using Zizo.Mahjong.Yaku;

public class MyCustomYaku : IYaku
{
    public string Name => "オリジナル役";
    public bool IsYakuman => false;

    // 成立する場合は true を返し、han に翻数（役満の場合は何倍役満か）を入れる
    public bool TryEvaluate(WinningContext context, out int han)
    {
        // context.Sets / context.PairKind（通常形の面子分解）
        // context.IsChiitoitsuShape / context.IsKokushiShape（特殊形）
        // context.AllTiles（鳴きを含む全14枚）/ context.Melds / context.Situation（ツモ・ロン、自風・場風、リーチ等）
        han = 0;
        return false;
    }
}

List<IYaku> yakuList = StandardYakuSet.CreateAll();
yakuList.Add(new MyCustomYaku());
var ruleSet = new MahjongRuleSet(4, TileSetDefinition.CreateStandard(), yakuList, new WallRule(), new ScoringRule());
```

役を無効にしたい場合（例: 人和を採用しない）は、リストから取り除いてください。
`Yaku/Standard/` 以下の実装が参考になります。

### CPU の戦略を追加する

`ICpuStrategy` を実装したクラスを作るだけで、強さや打ち筋の異なる CPU を追加できます。

### 乱数を差し替える

山のシャッフルは `IRandomSource` を通して行います。シード付きの `DefaultRandomSource(seed)` を使うか、
独自の実装を渡すことで、リプレイやテストのために山の並びを再現できます。

## テスト

`Tests/` に NUnit のテストがあります。ライブラリ本体はテストに依存しないため、不要であればコピーしなくて構いません。

- **Unity**: `Tests/` もコピーし、NUnit（Test Framework）を参照する EditMode 用の Assembly Definition を作成すると、
  Test Runner ウィンドウから実行できます。
- **dotnet**: ソース一式と `Tests/` を含み、`NUnit` / `NUnit3TestAdapter` / `Microsoft.NET.Test.Sdk` を参照する
  テストプロジェクトを作成し、`dotnet test` で実行できます。

テストでは `Tests/Support/HandNotation` の一般的な麻雀記法で手牌を記述します。

```csharp
List<Tile> tiles = HandNotation.Parse("123m456p789s11z"); // m=萬子 p=筒子 s=索子 z=字牌(1-4=東南西北 5-7=白發中) 0=赤5
```

## ライセンス

[MIT License](LICENSE)

---

## English summary

A rule engine for Japanese Riichi Mahjong written in plain C# with no dependency on UnityEngine.
It covers tiles and walls, shanten calculation, winning-hand detection, 42 standard yaku (including double yakuman),
fu/score calculation, single-round flow control (`RoundController`), and a simple CPU player.
Player count, tile set, enabled yaku and scoring rules are configurable through `MahjongRuleSet`.

- Requires C# 9.0 and .NET Standard 2.1 APIs. Works in Unity 2021.2+ as well.
- Distributed as plain source files: copy the folders except `Tests/` into your project. Root namespace: `Zizo.Mahjong`.
- Start with `MahjongRuleSet.CreateStandardFourPlayer()` and `RoundController`, or use `YakuEvaluator` /
  `FuCalculator` / `ScoreCalculator` directly to score a single hand.
- Multi-round game flow (dealer rotation, renchan), abortive draws other than nine terminals, multiple-ron
  resolution, temporary/riichi furiten, nagashi mangan and pao are not implemented.
- Yaku names and in-code comments are written in Japanese.
- Licensed under the [MIT License](LICENSE).
