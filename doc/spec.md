# 仕様

## エンハンス候補

## 設定項目

### 共通

- 言語： 日本語|英語|ドイツ語|フランス語

### マップ画面

#### マップ共通

- 天候： 自動|表示|非表示
  予報日数： 0～3

- 簡易表示： On|Off

- Zoom率： 0～1.0

#### フィールドエリア

- モード: モブハント|FATE|ギャザラー１|ギャザラー２

- マップ画像：
  - タイプ： 通常|アウトライン|白地図
  - 透過率： 0.0～0.8

- ターゲットの高低差表示： On|Off

##### モブハント

    // Sの絞り込み状況の表示
    const debugMode = ref(false)

    const tts = ref<string[]>(['SBpop', 'B', 'A', 'S', 'Brepop', 'Sstart'])

    const iconFontScale = ref(1.0)

    const posNotification = ref<PosNotificationSettings>({
      enabled: true,
      markingTimeoutSeconds: 180,
      miniMapTimeoutSeconds: 5
    })

    const flagRanks = ref(0x3f) // 0b111111 -> SS, S, A1, A2, B1, B2



##### FATE関連

##### ギャザラー１(採掘・採取)

##### ギャザラー２(釣り)

#### タウン

#### エウレカ

#### ボズヤ

通知画面


### NotifiedLocations

- Map画面
  - 複数の旗が分かりにくい
  - 自分が近づいた旗を消すオプションの実装
  - 旗に番号を振ったり、最新旗の強調方法を改善する
  - 最新旗と自分を線でつなぐオプションの実装
  - テレポも考慮？
  - エーテライトの旗表示を改善
- LocationMessage解析
  - FlagかPosかの判断
  - Instanceの判断（仮）
  - Worldの判断（仮）
  - 仮部分の訂正対応： 3秒以内に同じキャラクターが行った発言を追従
  - A TourのリーダーやSモブ発見者っぽいか
- Info画面
  - マップとメタ情報表示
  - 自分の現在マップと同じ場合の処理分け
- 音出す
  - A Tourの時、リーダーのNext位置(Mob or エーテライト)の度に音を鳴らしたい

### EventSource

#### 独自メモリパーサが必要になりそうなもの(難易度高、継続メンテリスクあり)

- 日次、月次報酬のステータス表示
- トークンの溢れ具合表示
- カメラの情報表示
- キーボード,マウス,コントローラ入力

#### 独自メモリパーサが不要そうなもの

- FATE情報取得
- ギャザ情報取得
