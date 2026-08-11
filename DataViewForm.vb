Imports System.Security.Cryptography
Imports System.Windows

'===========================================================
' ★ COPiPi メイン管理フォーム
'   - メモ一覧（DataTable）を保持し、MemoForm を生成・管理する
'   - タスクトレイ常駐、メニュー表示、通知処理など全体の司令塔
'===========================================================
Public Class DataViewForm

    '===========================================================
    ' ★ 基本設定・フィールド
    '===========================================================

    ' メッセージボックスのタイトル
    Const msgtitle As String = "COPiPi"

    ' メモデータ XML のファイル名
    Const dataFileXML As String = "\Memo.xml"

    ' メモデータのフルパス
    Dim dataFilename As String = System.Environment.CurrentDirectory & dataFileXML

    ' 設定ファイル（ファイル添付モードなど）
    Private settingsFile As String = Application.StartupPath & "\AppSettings.xml"

    ' 全メモを保持する DataTable（COPiPi の中心データ）
    Public mainData As DataTable

    ' ファイル添付モード（LocalCopy / Original / URL）デフォルトでローカル設定。
    Private FileAttachMode As String = "LocalCopy"

    ' タスクトレイ左クリック／右クリックメニュー
    Private menuLeft As ContextMenuStrip
    Private menuRight As ContextMenuStrip


    '===========================================================
    ' ★ Init：COPiPi 起動時の初期化処理
    '   - メニュー構築
    '   - 設定読み込み
    '   - メモデータ読み込み
    '   - VisibleFlag=True のメモだけ表示
    '   - 表示メニュー構築
    '   - リマインダー確認
    '===========================================================
    Public Sub Init()

        ' タスクトレイメニュー構築
        BuildMenus()

        ' 設定読み込み（ファイル添付モード）
        FileAttachMode = LoadSetting("FileAttachMode")
        If FileAttachMode = "" Then FileAttachMode = "LocalCopy"

        ' 添付ファイル保存フォルダの準備
        Dim baseDir As String = Application.StartupPath
        Dim fileDir As String = IO.Path.Combine(baseDir, "file")
        If Not IO.Directory.Exists(fileDir) Then IO.Directory.CreateDirectory(fileDir)

        ' メモデータ読み込み（XML → DataTable）
        mainData = readData()

        ' タスクトレイアイコンを表示
        NotifyIcon1.Visible = True

        ' VisibleFlag=True のメモだけ生成して表示
        For Each row As DataRow In mainData.Rows
            If CBool(row("VisibleFlag")) Then
                MemoBuild(mainData.Rows.IndexOf(row))
            End If
        Next

        ' 表示／非表示メニュー構築
        BuildShowCheckMenu()

        ' リマインダー確認（期限到達メモを通知）
        CheckAllReminders()
    End Sub


    '===========================================================
    ' ★ BuildMenus：タスクトレイの左／右クリックメニュー構築
    '===========================================================
    Private Sub BuildMenus()

        '-------------------------------
        ' ★ 左クリックメニュー
        '-------------------------------
        menuLeft = New ContextMenuStrip()

        ' 新規作成（色別）
        Dim newMenu As New ToolStripMenuItem("新規作成")

        Dim newYellow = New ToolStripMenuItem("Yellow")
        AddHandler newYellow.Click, AddressOf YellowToolStripMenuItem_Click
        newMenu.DropDownItems.Add(newYellow)

        Dim newPink = New ToolStripMenuItem("Pink")
        AddHandler newPink.Click, AddressOf PinkToolStripMenuItem_Click
        newMenu.DropDownItems.Add(newPink)

        Dim newGreen = New ToolStripMenuItem("Green")
        AddHandler newGreen.Click, AddressOf GreenToolStripMenuItem_Click
        newMenu.DropDownItems.Add(newGreen)

        Dim newBlue = New ToolStripMenuItem("Blue")
        AddHandler newBlue.Click, AddressOf BlueToolStripMenuItem_Click
        newMenu.DropDownItems.Add(newBlue)

        Dim newOrange = New ToolStripMenuItem("Orange")
        AddHandler newOrange.Click, AddressOf OrangeToolStripMenuItem_Click
        newMenu.DropDownItems.Add(newOrange)

        Dim newGray = New ToolStripMenuItem("Gray")
        AddHandler newGray.Click, AddressOf GrayToolStripMenuItem_Click
        newMenu.DropDownItems.Add(newGray)

        menuLeft.Items.Add(newMenu)

        ' 表示／非表示メニュー（後で動的に構築）
        Dim showMenu As New ToolStripMenuItem("表示／非表示")
        showMenu.Name = "ShowCheckMenu"
        menuLeft.Items.Add(showMenu)

        ' メニューを閉じる
        Dim hideMenu As New ToolStripMenuItem("メニューを閉じる")
        AddHandler hideMenu.Click, Sub() CloseAllMenus()
        menuLeft.Items.Add(hideMenu)


        '-------------------------------
        ' ★ 右クリックメニュー
        '-------------------------------
        menuRight = New ContextMenuStrip()

        Dim sortMenu = New ToolStripMenuItem("全付箋整列")
        AddHandler sortMenu.Click, AddressOf 並べ直しToolStripMenuItem_Click
        menuRight.Items.Add(sortMenu)

        Dim remindCheckMenu = New ToolStripMenuItem("通知予定確認")
        AddHandler remindCheckMenu.Click, AddressOf 通知確認ToolStripMenuItem_Click
        menuRight.Items.Add(remindCheckMenu)

        Dim settingsMenu = New ToolStripMenuItem("設定")
        AddHandler settingsMenu.Click, AddressOf 設定ToolStripMenuItem_Click
        menuRight.Items.Add(settingsMenu)

        Dim exitMenu As New ToolStripMenuItem("終了")
        AddHandler exitMenu.Click, AddressOf 終了ToolStripMenuItem_Click
        menuRight.Items.Add(exitMenu)
    End Sub


    '===========================================================
    ' ★ フォーム終了時：メモデータを XML に保存
    '===========================================================
    Private Sub DataViewForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        mainData.WriteXml(dataFilename)
    End Sub


    '===========================================================
    ' ★ DataGridView のヘッダー設定（管理画面用）
    '===========================================================
    Private Sub DataGridView1_DataBindingComplete(sender As Object, e As DataGridViewBindingCompleteEventArgs) Handles DataGridView1.DataBindingComplete

        ' DataTable の Caption をヘッダーに反映
        For Each col As DataGridViewColumn In DataGridView1.Columns
            col.HeaderText = DataGridView1.DataSource.Columns(col.Index).Caption
        Next

        ' 列幅を固定（管理画面の見やすさ向上）
        For i As Integer = 0 To DataGridView1.Columns.Count - 1
            DataGridView1.Columns(i).Width = 80
        Next
    End Sub


    '===========================================================
    ' ★ readData：メモデータを XML から読み込む
    '===========================================================
    Private Function readData() As DataTable

        Dim dt As New DataTable("TableName")

        ' メモの全カラム定義（COPiPi のデータ構造）
        dt.Columns.Add("id", GetType(String))
        dt.Columns.Add("title", GetType(String))
        dt.Columns.Add("text", GetType(String))
        dt.Columns.Add("MemoColor", GetType(Int16))
        dt.Columns.Add("x", GetType(Int32))
        dt.Columns.Add("y", GetType(Int32))
        dt.Columns.Add("CollapsedFlag", GetType(Boolean))
        dt.Columns.Add("VisibleFlag", GetType(Boolean))
        dt.Columns.Add("TopMostFlag", GetType(Boolean))
        dt.Columns.Add("FileReference", GetType(String))
        dt.Columns.Add("RemindTime", GetType(DateTime))
        dt.Columns.Add("FileType", GetType(String))

        ' XML 読み込み
        Try
            dt.ReadXml(dataFilename)
        Catch ex As IO.FileNotFoundException
            MessageBox.Show("ファイル " & dataFilename & " が見つかりません。空データを生成します", msgtitle)
        End Try

        Return dt
    End Function


    '===========================================================
    ' ★ タスクトレイアイコンのクリック処理
    '   左クリック → 左メニュー
    '   右クリック → 右メニュー
    '===========================================================
    Private Sub NotifyIcon1_MouseDown(sender As Object, e As MouseEventArgs) Handles NotifyIcon1.MouseDown

        ' 既存メニューを必ず閉じる（重複表示防止）
        CloseAllMenus()

        Dim pos = PointToClient(Cursor.Position)

        If e.Button = MouseButtons.Left Then
            menuLeft.Show(Me, pos)
        Else
            menuRight.Show(Me, pos)
        End If
    End Sub


    '===========================================================
    ' ★ 全メニューを閉じる（左／右）
    '===========================================================
    Public Sub CloseAllMenus()
        If menuLeft IsNot Nothing Then menuLeft.Hide()
        If menuRight IsNot Nothing Then menuRight.Hide()
    End Sub

    '===========================================================
    ' ★ タスクトレイアイコンのダブルクリック
    '   - メニューが開いていたら閉じるだけ
    '===========================================================
    Private Sub NotifyIcon1_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles NotifyIcon1.MouseDoubleClick
        CloseAllMenus()
    End Sub


    '===========================================================
    ' ★ 新規メモ作成（色別メニュー）
    '===========================================================
    Private Sub YellowToolStripMenuItem_Click(sender As Object, e As EventArgs)
        CreateNewMemo(0)
    End Sub

    Private Sub PinkToolStripMenuItem_Click(sender As Object, e As EventArgs)
        CreateNewMemo(1)
    End Sub

    Private Sub GreenToolStripMenuItem_Click(sender As Object, e As EventArgs)
        CreateNewMemo(2)
    End Sub

    Private Sub BlueToolStripMenuItem_Click(sender As Object, e As EventArgs)
        CreateNewMemo(3)
    End Sub

    Private Sub OrangeToolStripMenuItem_Click(sender As Object, e As EventArgs)
        CreateNewMemo(4)
    End Sub

    Private Sub GrayToolStripMenuItem_Click(sender As Object, e As EventArgs)
        CreateNewMemo(5)
    End Sub


    '===========================================================
    ' ★ CreateNewMemo：新規メモを DataTable に追加し、表示する
    '===========================================================
    Private Sub CreateNewMemo(colorNumber As Integer)

        ' 一意の ID（タイムスタンプ）
        Dim timeId As String = DateTime.Now.ToString("yyyyMMddHHmmssfff")

        Dim row As DataRow = mainData.NewRow()

        ' メモ初期値
        row("id") = timeId
        row("title") = "新規コピピ"
        row("text") = ""
        row("MemoColor") = colorNumber

        ' 重ならない位置を自動探索
        Dim safePos As Point = FindSafePosition()
        row("x") = safePos.X
        row("y") = safePos.Y

        row("CollapsedFlag") = False
        row("VisibleFlag") = True
        row("TopMostFlag") = False
        row("FileReference") = ""
        row("RemindTime") = DBNull.Value
        row("FileType") = "None"

        ' ファイル添付モードに応じて初期 FileType を設定
        Select Case FileAttachMode
            Case "LocalCopy" : row("FileType") = "Local"
            Case "UseOriginal" : row("FileType") = "Original"
            Case "UseURL" : row("FileType") = "URL"
        End Select

        ' DataTable に追加
        mainData.Rows.Add(row)

        ' XML 保存
        mainData.WriteXml(dataFilename)

        ' 実際の MemoForm を生成して表示
        MemoBuild(mainData.Rows.Count - 1)

        ' 表示メニュー更新
        BuildShowCheckMenu()
    End Sub



    '===========================================================
    ' ★ FindSafePosition：新規メモの安全な位置を探索する
    '   - 既存 MemoForm と重ならない位置を探す
    '   - Y をずらしながら探索
    '   - 画面下に来たら X を右にずらす
    '===========================================================
    Private Function FindSafePosition() As Point

        Dim startX As Integer = 200
        Dim startY As Integer = 100
        Dim offset As Integer = 30

        Dim x As Integer = startX
        Dim y As Integer = startY

        Dim screenHeight = Screen.PrimaryScreen.WorkingArea.Height
        Dim screenWidth = Screen.PrimaryScreen.WorkingArea.Width

        While True

            ' 既存メモと重なるかチェック
            Dim overlap = Application.OpenForms.Cast(Of Form).
                Where(Function(f) TypeOf f Is MemoForm).
                Any(Function(f)
                        Dim mf = CType(f, MemoForm)
                        Dim rect1 As New Rectangle(x, y, 300, 200)
                        Dim rect2 As New Rectangle(mf.Left, mf.Top, mf.Width, mf.Height)
                        Return rect1.IntersectsWith(rect2)
                    End Function)

            ' 重ならなければこの位置を採用
            If Not overlap Then
                Return New Point(x, y)
            End If

            ' 重なっていたら Y をずらす
            y += offset

            ' 画面下に来たら X を右へ、Y を初期化
            If y + 200 > screenHeight Then
                x += 320
                y = startY
            End If

            ' 画面右端まで来たら左に戻す（安全策）
            If x + 300 > screenWidth Then
                x = startX
            End If
        End While

    End Function

    '===========================================================
    ' ★ MemoForm 生成（MyRow を渡す）
    '   - DataRow の内容を UI に反映して MemoForm を生成
    '   - 表示位置・色・タイトルなどを初期化
    '===========================================================
    Private Sub MemoBuild(i As Integer)

        Dim row As DataRow = mainData.Rows(i)

        ' MemoForm に DataRow と親フォーム（Me）を渡す
        Dim objForm As New MemoForm(row, Me)

        ' メモの基本情報を UI に反映
        objForm.IDLabel.Text = row("id").ToString()
        objForm.Text = row("title").ToString()          ' フォームタイトルバー
        objForm.TitleText.Text = row("title").ToString() ' タイトル編集欄
        objForm.MemoText.Text = row("text").ToString()   ' 本文

        ' メモの色設定（MemoColor に応じて背景色を決定）
        Select Case CInt(row("MemoColor"))
            Case 0 : objForm.BackColor = Color.FromArgb(255, 240, 120)
            Case 1 : objForm.BackColor = Color.FromArgb(255, 160, 200)
            Case 2 : objForm.BackColor = Color.FromArgb(160, 240, 160)
            Case 3 : objForm.BackColor = Color.FromArgb(140, 180, 255)
            Case 4 : objForm.BackColor = Color.FromArgb(255, 200, 120)
            Case 5 : objForm.BackColor = Color.FromArgb(200, 200, 200)
        End Select

        ' 表示位置を DataRow の x,y から復元
        objForm.StartPosition = FormStartPosition.Manual
        objForm.Location = New Point(CInt(row("x")), CInt(row("y")))

        ' メモを表示
        objForm.Show()

    End Sub


    '===========================================================
    ' ★ 表示メニュー構築（色別 → 個別メモ）
    '   - mainData を LINQ で色別に分類
    '   - VisibleFlag に応じてチェック状態を設定
    '   - メニューを動的に生成する
    '===========================================================
    Private Sub BuildShowCheckMenu()

        ' 左クリックメニューの「表示／非表示」を取得
        Dim showMenu As ToolStripMenuItem = CType(menuLeft.Items("ShowCheckMenu"), ToolStripMenuItem)
        showMenu.DropDownItems.Clear()

        ' 色番号 → 色名の対応表
        Dim colorNames As New Dictionary(Of Integer, String) From {
        {0, "Yellow"}, {1, "Pink"}, {2, "Green"},
        {3, "Blue"}, {4, "Orange"}, {5, "Gray"}
    }

        ' 色ごとにメニューを構築
        For Each kv In colorNames

            Dim colorNumber = kv.Key
            Dim colorName = kv.Value

            ' 該当色のメモを抽出
            Dim rows = mainData.Rows.Cast(Of DataRow)().
            Where(Function(r) CInt(r("MemoColor")) = colorNumber)

            If rows.Any() Then

                ' 色メニュー（親）
                Dim colorItem As New ToolStripMenuItem(colorName)
                colorItem.Tag = colorNumber

                ' 色のメモが全て VisibleFlag=True のときだけチェック
                colorItem.Checked = rows.All(Function(r) CBool(r("VisibleFlag")))

                AddHandler colorItem.Click, AddressOf ShowColorMenu_Click

                ' 個別メモ（子メニュー）
                For Each r In rows
                    Dim memoItem As New ToolStripMenuItem(r("title").ToString())
                    memoItem.Tag = r("id").ToString()
                    memoItem.Checked = CBool(r("VisibleFlag"))
                    AddHandler memoItem.Click, AddressOf ShowSingleMemo_Click
                    colorItem.DropDownItems.Add(memoItem)
                Next

                showMenu.DropDownItems.Add(colorItem)
            End If
        Next
    End Sub


    '===========================================================
    ' ★ 色メニュークリック（色のメモを一括表示／非表示）
    '   - VisibleFlag を一括更新
    '   - 既存フォームは Visible を変更
    '   - 無い場合は新規生成
    '===========================================================
    Private Sub ShowColorMenu_Click(sender As Object, e As EventArgs)

        Dim item = CType(sender, ToolStripMenuItem)
        Dim colorNumber = CInt(item.Tag)

        ' チェック状態を反転
        item.Checked = Not item.Checked

        ' mainData の VisibleFlag を一括更新
        For Each r In mainData.Rows.Cast(Of DataRow)().
        Where(Function(row) CInt(row("MemoColor")) = colorNumber)

            r("VisibleFlag") = item.Checked
        Next

        ' フォーム更新（存在しない場合は生成）
        For Each row In mainData.Rows.Cast(Of DataRow)().
        Where(Function(r) CInt(r("MemoColor")) = colorNumber)

            Dim exists = Application.OpenForms.Cast(Of Form).
            Any(Function(f) TypeOf f Is MemoForm AndAlso CType(f, MemoForm).MyRow Is row)

            If exists Then
                ' 既存フォームの表示状態を変更
                Dim mf = Application.OpenForms.Cast(Of Form).
                First(Function(f) TypeOf f Is MemoForm AndAlso CType(f, MemoForm).MyRow Is row)
                mf.Visible = item.Checked

            ElseIf item.Checked Then
                ' 非表示 → 表示に変更された場合は新規生成
                MemoBuild(mainData.Rows.IndexOf(row))
            End If
        Next

        ' 子メニュー（個別メモ）のチェック状態を同期
        For Each memoItem As ToolStripMenuItem In item.DropDownItems
            memoItem.Checked = item.Checked
        Next

        Save()
        CloseAllMenus()

    End Sub


    '===========================================================
    ' ★ 個別メモ表示切替
    '   - VisibleFlag を更新
    '   - 既存フォームの Visible を変更
    '   - 無い場合は新規生成
    '   - 親メニュー（色）のチェック状態を再計算
    '===========================================================
    Private Sub ShowSingleMemo_Click(sender As Object, e As EventArgs)

        Dim item = CType(sender, ToolStripMenuItem)
        Dim memoId = item.Tag.ToString()

        ' チェック状態を反転
        item.Checked = Not item.Checked

        ' mainData の VisibleFlag を更新
        Dim row = mainData.Rows.Cast(Of DataRow)().
        First(Function(r) r("id").ToString() = memoId)

        row("VisibleFlag") = item.Checked

        ' フォーム存在チェック
        Dim exists = Application.OpenForms.Cast(Of Form).
        Any(Function(f) TypeOf f Is MemoForm AndAlso CType(f, MemoForm).MyRow Is row)

        If exists Then
            ' 既存フォームの表示状態を変更
            Dim mf = Application.OpenForms.Cast(Of Form).
            First(Function(f) TypeOf f Is MemoForm AndAlso CType(f, MemoForm).MyRow Is row)
            mf.Visible = item.Checked

        ElseIf item.Checked Then
            ' 非表示 → 表示に変更された場合は新規生成
            MemoBuild(mainData.Rows.IndexOf(row))
        End If

        CloseAllMenus()

        ' 親メニュー（色）のチェック状態を再計算
        Dim parentItem = CType(item.OwnerItem, ToolStripMenuItem)
        Dim colorNumber = CInt(parentItem.Tag)

        Dim rows = mainData.Rows.Cast(Of DataRow)().
        Where(Function(r) CInt(r("MemoColor")) = colorNumber)

        parentItem.Checked = rows.All(Function(r) CBool(r("VisibleFlag")))

        Save()
        CloseAllMenus()

    End Sub


    '===========================================================
    ' ★ 表示メニュー再構築（外部から呼び出し）
    '===========================================================
    Public Sub RebuildShowCheckMenu()
        BuildShowCheckMenu()
    End Sub


    '===========================================================
    ' ★ 色メニューのチェック状態再計算
    '   - 色のメモが全て VisibleFlag=True ならチェック
    '===========================================================
    Public Sub RecalculateColorMenuCheckState(colorNumber As Integer)

        Dim rows = mainData.Rows.Cast(Of DataRow)().
        Where(Function(r) CInt(r("MemoColor")) = colorNumber)

        Dim allVisible = rows.All(Function(r) CBool(r("VisibleFlag")))

        Dim showMenu As ToolStripMenuItem =
        CType(menuLeft.Items("ShowCheckMenu"), ToolStripMenuItem)

        If showMenu Is Nothing Then Exit Sub

        ' 対象色のメニューだけ更新
        For Each item As ToolStripMenuItem In showMenu.DropDownItems
            If CInt(item.Tag) = colorNumber Then
                item.Checked = allVisible
                Exit For
            End If
        Next

    End Sub


    '===========================================================
    ' ★ CheckAllReminders：リマインダー（通知）チェック
    '   - 表示中の MemoForm に対して CheckReminder() を呼び出し色更新
    '   - 非表示メモの RemindTime を確認し、期限到達なら通知＋表示
    '   - 通知内容は「今日」または「超過（◯日）」のみ
    '===========================================================
    Public Sub CheckAllReminders()

        '---------------------------------------------
        ' ★ 表示中の MemoForm の色更新（期限超過色など）
        '---------------------------------------------
        For Each f As Form In Application.OpenForms
            If TypeOf f Is MemoForm Then
                CType(f, MemoForm).CheckReminder()
            End If
        Next

        '---------------------------------------------
        ' ★ 非表示メモの期限チェック
        '---------------------------------------------
        For Each row As DataRow In mainData.Rows

            ' RemindTime が未設定ならスキップ
            If IsDBNull(row("RemindTime")) Then Continue For

            Dim remindTime As DateTime = CType(row("RemindTime"), DateTime)

            ' まだ通知日になっていない場合はスキップ
            If DateTime.Now < remindTime Then Continue For

            '---------------------------------------------
            ' ★ 日数計算（今日 or 超過のみ）
            '---------------------------------------------
            Dim daysDiff As Integer = (remindTime.Date - DateTime.Now.Date).Days
            Dim status As String

            If daysDiff = 0 Then
                status = "今日"
            Else
                status = $"超過（{Math.Abs(daysDiff)} 日）"
            End If

            '---------------------------------------------
            ' ★ バルーン通知（NotifyIcon）
            '---------------------------------------------
            NotifyIcon1.BalloonTipTitle = $"通知：{remindTime:yyyy/MM/dd}"
            NotifyIcon1.BalloonTipText = $"{row("title")}（{status}）"
            NotifyIcon1.ShowBalloonTip(4000)

            '---------------------------------------------
            ' ★ メモを表示状態にする（強制表示）
            '---------------------------------------------
            row("VisibleFlag") = True
            row("TopMostFlag") = True
            mainData.AcceptChanges()

            '---------------------------------------------
            ' ★ 既存フォームがあるかチェック
            '---------------------------------------------
            Dim exists = False

            For Each f As Form In Application.OpenForms
                If TypeOf f Is MemoForm Then
                    Dim mf = CType(f, MemoForm)
                    If mf.MyRow Is row Then
                        exists = True
                        mf.TopMost = True
                        mf.pinLabel.Text = "📌"   ' ピン表示
                        Exit For
                    End If
                End If
            Next

            '---------------------------------------------
            ' ★ フォームが無ければ新規生成
            '---------------------------------------------
            If Not exists Then
                MemoBuild(mainData.Rows.IndexOf(row))
            End If

            ' 表示メニュー更新
            RebuildShowCheckMenu()
            RecalculateColorMenuCheckState(CInt(row("MemoColor")))
        Next

    End Sub


    '===========================================================
    ' ★ Timer1_Tick：3時間ごとにリマインダーをチェック
    '===========================================================
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        CheckAllReminders()
    End Sub


    '===========================================================
    ' ★ 終了メニュー：データ保存してアプリ終了
    '===========================================================
    Private Sub 終了ToolStripMenuItem_Click(sender As Object, e As EventArgs)
        Save()
        Application.Exit()
    End Sub



    '===========================================================
    ' ★ 並べ直し：全メモを画面左上から整列させる
    '   - 保存されていた位置はクリアされる
    '   - 表示中のメモ → 位置変更
    '   - 非表示メモ → 新規生成して位置変更
    '===========================================================
    Private Sub 並べ直しToolStripMenuItem_Click(sender As Object, e As EventArgs)

        Dim result = MessageBox.Show(
        "保存されていた表示位置はクリアされます。全ての付箋を並べ直しますか？",
        "ソート確認",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Warning
    )

        If result <> DialogResult.Yes Then Return

        ' 全メモを強制表示＋折りたたみ状態にする
        For Each row As DataRow In mainData.Rows
            row("VisibleFlag") = True
            row("CollapsedFlag") = True
        Next

        mainData.AcceptChanges()
        RebuildShowCheckMenu()

        ' 整列開始位置
        Dim posX = 50
        Dim posY = 50
        Dim offset = 18
        Dim maxY = Screen.PrimaryScreen.WorkingArea.Height - 50

        '---------------------------------------------
        ' ★ 表示中の MemoForm を並べ直す
        '---------------------------------------------
        For Each f As Form In Application.OpenForms
            If TypeOf f Is MemoForm Then
                Dim mf = CType(f, MemoForm)

                ' 画面下に到達したら右にずらす
                If posY + mf.Height > maxY Then
                    posX += 305
                    posY = 50
                End If

                mf.Left = posX
                mf.Top = posY

                mf.MyRow("x") = posX
                mf.MyRow("y") = posY

                posY += offset
            End If
        Next

        '---------------------------------------------
        ' ★ 非表示メモを生成して並べ直す
        '---------------------------------------------
        For Each row As DataRow In mainData.Rows

            Dim exists = Application.OpenForms.Cast(Of Form).
            Any(Function(f) TypeOf f Is MemoForm AndAlso CType(f, MemoForm).MyRow Is row)

            If Not exists Then
                ' 新規生成
                MemoBuild(mainData.Rows.IndexOf(row))

                Dim mf = Application.OpenForms.Cast(Of Form).
                First(Function(f) TypeOf f Is MemoForm AndAlso CType(f, MemoForm).MyRow Is row)

                If posY + mf.Height > maxY Then
                    posX += 305
                    posY = 50
                End If

                mf.Left = posX
                mf.Top = posY

                row("x") = posX
                row("y") = posY

                posY += offset
            End If
        Next

        mainData.AcceptChanges()
        Save()
    End Sub


    '===========================================================
    ' ★ 通知確認：通知予定のメモを 1 件ずつ表示
    '===========================================================
    Private Sub 通知確認ToolStripMenuItem_Click(sender As Object, e As EventArgs)

        ' RemindTime が設定されている行を抽出
        Dim remindRows = mainData.Rows.Cast(Of DataRow)().
        Where(Function(r) Not IsDBNull(r("RemindTime"))).
        OrderBy(Function(r) CType(r("RemindTime"), DateTime)).
        ToList()

        If remindRows.Count = 0 Then
            MessageBox.Show("現在、通知が設定されているメモはありません。", "通知確認")
            Return
        End If

        Dim total As Integer = remindRows.Count

        ' 1 件ずつ表示
        For i As Integer = 0 To total - 1

            Dim row = remindRows(i)
            Dim title As String = row("title").ToString()
            Dim remindTime As DateTime = CType(row("RemindTime"), DateTime)
            Dim visible As Boolean = CBool(row("VisibleFlag"))

            Dim daysDiff As Integer = (remindTime.Date - DateTime.Now.Date).Days
            Dim status As String

            If daysDiff > 0 Then
                status = $"あと {daysDiff} 日"
            ElseIf daysDiff = 0 Then
                status = "今日"
            Else
                status = $"超過（{Math.Abs(daysDiff)} 日）"
            End If

            Dim msg As String =
            $"タイトル：{title}{vbCrLf}" &
            $"通知日：{remindTime:yyyy/MM/dd}：{status}{vbCrLf}" &
            $"現在の状態：{If(visible, "表示中", "非表示")}"

            MessageBox.Show(msg, $"通知予定全件表示（{i + 1} / {total} 件）")
        Next

        CloseAllMenus()
    End Sub


    '===========================================================
    ' ★ LoadSetting：設定ファイルから値を読み込む
    '===========================================================
    Public Function LoadSetting(key As String) As String
        If Not IO.File.Exists(settingsFile) Then Return ""

        Dim doc As XDocument = XDocument.Load(settingsFile)
        Dim elem = doc.Root.Element(key)
        If elem Is Nothing Then Return ""
        Return elem.Value
    End Function


    '===========================================================
    ' ★ SaveSetting：設定ファイルに値を書き込む
    '===========================================================
    Public Sub SaveSetting(key As String, value As String)

        Dim doc As XDocument

        If IO.File.Exists(settingsFile) Then
            doc = XDocument.Load(settingsFile)
        Else
            doc = New XDocument(New XElement("Settings"))
        End If

        Dim elem = doc.Root.Element(key)
        If elem Is Nothing Then
            doc.Root.Add(New XElement(key, value))
        Else
            elem.Value = value
        End If

        doc.Save(settingsFile)
    End Sub


    '===========================================================
    ' ★ 設定画面を開く
    '===========================================================
    Private Sub 設定ToolStripMenuItem_Click(sender As Object, e As EventArgs)
        Dim sf As New SettingsForm(Me)
        sf.ShowDialog()
        CloseAllMenus()
    End Sub


    '===========================================================
    ' ★ メモデータを XML に保存
    '===========================================================
    Public Sub Save()
        mainData.WriteXml(dataFilename)
    End Sub


    '===========================================================
    ' ★ ファイル添付モードの設定
    '===========================================================
    Public Sub SetFileAttachMode(mode As String)
        FileAttachMode = mode
    End Sub

    Public Function GetFileAttachMode() As String
        Return FileAttachMode
    End Function

End Class
