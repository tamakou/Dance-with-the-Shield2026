# Dance with the Shield (DWS)

Meta Quest 3 向け / Unity 6 LTS 想定の「リズム防衛VR」サンプル実装です。
Assets/_APP 以下に、DWS のゲームロジック（スクリプト類）だけを置く構成です。

> 注意:
> - 本フォルダには Meta XR Interaction SDK / OVR Samples のアセットは同梱しません（Meta公式パッケージを別途インポートしてください）。
> - 盾（Shield）の“掴める”設定は、Meta XR Interaction SDK のサンプルにある Grabbable/GrabInteractable をベースに作る前提です。

## フォルダ構成

Assets/_APP
- Scripts
  - Core: シーン名、セッション、ローダ等
  - Config: ステージ難易度パラメータ（コード内デフォルト）
  - Gameplay: スポーン、矢の挙動、統計、ゲーム進行
  - UI: メニュー/HUD/リザルト/警告UI
  - Audio: BGM再生
- Editor: かんたんセットアップ用メニュー（任意）

## シーン構成（あなたのプロジェクト側で作成）
- Menu
- Game
- Endless

各シーンは、Meta XR Interaction SDK 83.0 のサンプルシーン（OVR Rig を含むもの）を複製して作成し、
その上に _APP/Scripts のコンポーネントを追加してください。

## 盾（Shield）の作り方（概要）
1. サンプルシーン内の “掴めるオブジェクト” を複製
2. メッシュを盾形状に置換（当面は Cube や Cylinder でもOK）
3. そのルートに `DWS.ShieldMarker` を付ける
4. `DWS.ShieldHeldDetector` の参照先に、その `Grabbable` を指定する

## 矢（Arrow）
矢はプレハブが無くても実行時に自動生成します（Primitive + TrailRenderer）。
「当たる確率の矢」だけが当たり判定（盾/プレイヤー）を持つ設計です。

## エディタメニュー（任意）
Unity 上部メニュー:
- DWS/Setup/Add DWS Managers (Menu)
- DWS/Setup/Add DWS Managers (Gameplay)

現在開いているシーンに最低限の GameObject を追加します（参照の自動解決は出来る範囲で行います）。
