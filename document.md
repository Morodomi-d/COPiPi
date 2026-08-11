# COPiPi クラス相互関係ドキュメント

このドキュメントは、`DataViewForm` と `MemoForm` を中心とした  
COPiPi の内部構造と相互関係を整理したものです。

メモデータは `DataTable` を介して共有され、各フォームは同じ `DataRow` を参照して動作します。

---

## 1. 全体構成

### 主要クラス
- **DataViewForm**  
  メイン管理画面・トレイアイコン・メモ一覧・通知管理
- **MemoForm**  
  個々の付箋ウィンドウ（タイトル・本文・添付・通知など）
- **CircleButton**  
  MemoForm 左上の丸ボタン（非表示＋通知状態表示）
- **SettingsForm**  
  設定画面（ファイル添付モードなど）

### データ構造（DataTable）
`DataViewForm.mainData As DataTable` が全メモの共通データストア。

| カラム名 | 内容 |
|---------|------|
| id | メモの一意な ID |
| title | メモタイトル |
| text | 本文 |
| MemoColor | 色番号 |
| x, y | ウィンドウ位置 |
| CollapsedFlag | 折り畳み状態 |
| VisibleFlag | 表示／非表示 |
| TopMostFlag | 最前面固定 |
| FileReference | 添付ファイル／URL |
| RemindTime | 通知日時 |
| FileType | None / Local / Original / URL |

---

## 2. DataViewForm と MemoForm の関係

### インスタンス生成とリンク
- `DataViewForm.Init()`  
  → `mainData` を読み込み  
  → `MemoBuild()` で MemoForm を生成
- `MemoBuild(i)`  
  → `mainData.Rows(i)` を渡して `New MemoForm(row, Me)`
- MemoForm 側  
  → `MyRow = row`  
  → `parentForm = parent`  
  → 双方向リンクが成立

### DataRow の共有
MemoForm は `MyRow As DataRow` を通じて DataViewForm の DataTable と同期。

| 操作 | 更新される DataRow |
|------|---------------------|
| タイトル変更 | `MyRow("title")` |
| 本文変更 | `MyRow("text")` |
| 位置変更 | `MyRow("x"), MyRow("y")` |
| 折り畳み | `MyRow("CollapsedFlag")` |
| TopMost | `MyRow("TopMostFlag")` |
| 通知日時 | `MyRow("RemindTime")` |
| 添付ファイル | `MyRow("FileReference"), MyRow("FileType")` |

---

## 3. 表示制御の流れ

### ① メニューからの表示／非表示（DataViewForm）
- `BuildShowCheckMenu()`  
  色別にメモ一覧メニューを構築
- `ShowColorMenu_Click`  
  色単位で VisibleFlag を一括更新
- `ShowSingleMemo_Click`  
  個別メモの VisibleFlag を更新

### ② MemoForm からの非表示操作
- 左上丸ボタン → `HideButton_Click`  
  → `MyRow("VisibleFlag") = False`  
  → メニュー再構築

---

## 4. 通知機能の相互関係

### ① 通知日時の設定（MemoForm）
- `remindButton_Click`  
  - 既に設定 → 解除  
  - 未設定 → DateTimePicker で日付選択
- `SaveRemindTime()`  
  → `MyRow("RemindTime")` を保存
- `CheckReminder()`  
  → hideButton の色を変更（白／青／赤）

### ② 通知チェック（DataViewForm）
- `Timer1_Tick` → `CheckAllReminders()`
- `CheckAllReminders()`  
  - RemindTime 到達メモを検出  
  - バルーン通知  
  - メモを強制表示  
  - TopMost に設定  
  - メニュー更新

---

## 5. 添付ファイル／URL の相互関係

### ① 添付モード設定
- SettingsForm → `SaveSetting("FileAttachMode")`
- DataViewForm → `LoadSetting("FileAttachMode")`
- MemoForm → `parentForm.GetFileAttachMode()`

### ② ファイルドロップ（MemoForm）
- `MemoText_DragDrop`
  - LocalCopy → 自動コピー
  - UseOriginal / UseURL → ダイアログで選択
  - `MyRow("FileReference")` と `MyRow("FileType")` を保存
  - UI 更新（fileLabel / clipLabel）

### ③ URL 添付
- `urlButton_Click`
  - 既存 URL → 削除確認
  - 新規 URL → 入力 → バリデーション → 保存
  - `GetUrlTitleSafe()` でページタイトル取得
  - `GetSmartUrlText()` で短縮表示

---

## 6. 永続化と終了処理

### XML 永続化
- `DataViewForm.Save()`  
  → `mainData.WriteXml(Memo.xml)`
- 各種操作後に Save() が呼ばれる

### 終了処理
- 「終了」メニュー → Save → Application.Exit()

---

## 7. まとめ

COPiPi は **1つの DataTable を中心に、複数の MemoForm が同じ DataRow を共有する構造**になっています。

- DataViewForm  
  → 一覧・通知・メニュー・永続化  
- MemoForm  
  → 個別編集・表示・添付・通知設定  

役割分担が明確で、拡張しやすい構造になっています。

