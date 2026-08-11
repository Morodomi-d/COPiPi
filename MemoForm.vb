
Imports System.Drawing
Imports System.Reflection.Emit
Imports System.Windows.Forms
Imports System.Windows.Forms.VisualStyles.VisualStyleElement

'===========================================================
' ★ MemoForm：COPiPi の付箋ウィンドウ
'   - DataRow（MyRow）と完全同期して動作する
'   - タイトルバーのドラッグ、折り畳み、TopMost、位置保存など
'   - ファイル添付・URL・通知ラベルなどの UI を保持
'===========================================================
Public Class MemoForm

        '===========================================================
        ' ★ 定数・フィールド
        '===========================================================

        Private Const TitleHeight As Integer = 16          ' タイトルバーの高さ
        Private Const OriginalHeight As Integer = 206      ' 展開時の高さ
        Private isCaptionDoubleClick As Boolean = False    ' タイトルバーのダブルクリック判定

        Private hideButton As CircleButton                 ' 左上の「非表示」ボタン
        Public pinLabel As System.Windows.Forms.Label      ' TopMost ピン表示
        Private remindLabel As System.Windows.Forms.Label  ' 通知ラベル
        Public clipLabel As System.Windows.Forms.Label     ' 添付ファイルアイコン
        Private fileLabel As System.Windows.Forms.Label    ' 添付ファイル名表示

        Private titleFont As New Font("Segoe UI", 8, FontStyle.Bold) ' タイトル描画用フォント

        Private parentForm As DataViewForm                 ' 親フォーム（DataViewForm）
        Private memoTip As New System.Windows.Forms.ToolTip ' ツールチップ

        ' DataRow（メモの実体）
        Public Property MyRow As DataRow

        ' 添付ファイルの種類
        Public Enum FileTypeEnum
            None
            Local
            Original
            URL
        End Enum



    '===========================================================
    ' ★ WndProc：Windows メッセージ処理（ドラッグ・折り畳み・TopMost）
    '===========================================================
    Protected Overrides Sub WndProc(ByRef m As Message)

        Const WM_NCHITTEST As Integer = &H84
        Const WM_NCLBUTTONDOWN As Integer = &HA1
        Const WM_NCLBUTTONDBLCLK As Integer = &HA3
        Const WM_NCRBUTTONUP As Integer = &HA5
        Const WM_EXITSIZEMOVE As Integer = &H232
        Const HTCAPTION As Integer = 2

        Select Case m.Msg


            '-------------------------------------------------------
            ' ★ タイトルバーのドラッグ判定（最軽量）
            '-------------------------------------------------------
            Case WM_NCHITTEST

                ' LParam を高速に分解（Point を生成しない）
                Dim lx As Integer = m.LParam.ToInt32() And &HFFFF
                Dim ly As Integer = (m.LParam.ToInt32() >> 16)

                ' フォーム内座標に変換
                Dim localY As Integer = ly - Me.Top

                ' タイトルバー領域ならドラッグ可能にする
                If localY < TitleHeight Then
                    m.Result = CType(HTCAPTION, IntPtr)
                    Return
                End If

            '-------------------------------------------------------
            ' ★ 左クリック → メモ内容をクリップボードへコピー
            '-------------------------------------------------------
            Case WM_NCLBUTTONDOWN

                If MemoText.Text.Length > 0 Then
                    Clipboard.SetText(MemoText.Text)
                    Debug.WriteLine(Clipboard.GetText)
                End If

                MyBase.WndProc(m)
                Return

            '-------------------------------------------------------
            ' ★ ダブルクリック → 折り畳み／展開
            '-------------------------------------------------------
            Case WM_NCLBUTTONDBLCLK

                If m.WParam.ToInt32() = HTCAPTION Then
                    Dim collapsed As Boolean = CBool(MyRow("CollapsedFlag"))
                    MyRow("CollapsedFlag") = Not collapsed

                    ' 高さを切り替える
                    Me.Height = If(collapsed, OriginalHeight, TitleHeight)

                    parentForm.mainData.AcceptChanges()
                    parentForm.Save()
                End If

                Return

            '-------------------------------------------------------
            ' ★ 右クリック → TopMost トグル
            '-------------------------------------------------------
            Case WM_NCRBUTTONUP

                If m.WParam.ToInt32() = HTCAPTION Then
                    Me.TopMost = Not Me.TopMost
                    pinLabel.Text = If(Me.TopMost, "📌", "")
                    MyRow("TopMostFlag") = Me.TopMost

                    parentForm.mainData.AcceptChanges()
                    parentForm.Save()
                End If

                Return

            '-------------------------------------------------------
            ' ★ ドラッグ終了 → 位置保存
            '-------------------------------------------------------
            Case WM_EXITSIZEMOVE

                MyRow("x") = Me.Left
                MyRow("y") = Me.Top
                parentForm.mainData.AcceptChanges()
                Return

        End Select

        ' その他のメッセージは OS に任せる
        MyBase.WndProc(m)
    End Sub


    '===========================================================
    ' ★ OnResize：ピン・クリップアイコンの位置調整
    '===========================================================
    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)

        If pinLabel IsNot Nothing Then
            pinLabel.Left = Me.Width - pinLabel.Width - 4
        End If

        If clipLabel IsNot Nothing Then
            clipLabel.Left = pinLabel.Left - clipLabel.Width - 2
        End If
    End Sub


    '===========================================================
    ' ★ コンストラクタ：DataRow と親フォームを受け取る
    '===========================================================
    Public Sub New(row As DataRow, parent As DataViewForm)

        InitializeComponent()

        MyRow = row
        parentForm = parent   ' ★ MemoForm と DataViewForm をリンクさせる

        CreateMacStyleHideButton()
        CreateTopMostPin()
    End Sub


    '===========================================================
    ' ★ OnHandleCreated：ダブルバッファリング有効化（ちらつき防止）
    '===========================================================
    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        Me.DoubleBuffered = True
    End Sub


    '===========================================================
    ' ★ 非表示ボタン（左上の白丸）
    '===========================================================
    Private Sub CreateMacStyleHideButton()

        hideButton = New CircleButton With {
            .Width = 14,
            .Height = 14,
            .Left = 2,
            .Top = 1,
            .CircleColor = Color.White,
            .CircleBorderColor = Color.Black,
            .FlatStyle = FlatStyle.Flat
        }

        hideButton.FlatAppearance.BorderSize = 0
        hideButton.Cursor = Cursors.Hand

        AddHandler hideButton.Click, AddressOf HideButton_Click
        memoTip.SetToolTip(hideButton, "この付箋を非表示にします")

        Me.Controls.Add(hideButton)
    End Sub


    '===========================================================
    ' ★ 添付ファイルアイコン（クリップ）を作成
    '===========================================================
    Private Sub CreateClipIcon()

        clipLabel = New System.Windows.Forms.Label With {
            .AutoSize = False,
            .Width = 14,
            .Height = 14,
            .Top = 0,
            .TextAlign = ContentAlignment.MiddleCenter,
            .Font = New Font("Segoe UI Emoji", 8),
            .ForeColor = Color.Black,
            .BackColor = Color.Transparent,
            .Text = ""
        }

        ' pinLabel の左側に配置
        clipLabel.Left = pinLabel.Left - clipLabel.Width - 2

        Me.Controls.Add(clipLabel)
        AddHandler clipLabel.DoubleClick, AddressOf FileLabel_Click
    End Sub


    '===========================================================
    ' ★ TopMost ピンアイコンを作成
    '===========================================================
    Private Sub CreateTopMostPin()

        pinLabel = New System.Windows.Forms.Label With {
            .AutoSize = False,
            .Width = 14,
            .Height = 14,
            .Top = 0,
            .TextAlign = ContentAlignment.MiddleCenter,
            .Font = New Font("Segoe UI Emoji", 8),
            .ForeColor = Color.Black,
            .BackColor = Color.Transparent,
            .Text = ""
        }

        ' 右端に配置
        pinLabel.Left = Me.Width - pinLabel.Width - 4

        Me.Controls.Add(pinLabel)
    End Sub


    '===========================================================
    ' ★ 添付ファイル名ラベル（fileLabel）を作成
    '===========================================================
    Private Sub CreateFileLabel()

        fileLabel = New System.Windows.Forms.Label With {
            .AutoSize = False,
            .Left = 180,
            .Top = 185,
            .Width = 90,
            .Height = 20,
            .Font = New Font("Segoe UI", 8, FontStyle.Underline),
            .RightToLeft = RightToLeft.No,
            .TextAlign = ContentAlignment.TopLeft,
            .ForeColor = Color.DarkBlue,
            .BackColor = Color.Transparent,
            .Cursor = Cursors.Hand,
            .Text = "",
            .Visible = False
        }

        AddHandler fileLabel.Click, AddressOf FileLabel_Click

        Me.Controls.Add(fileLabel)
    End Sub


    '===========================================================
    ' ★ OnPaint：タイトルバー描画（背景＋タイトル文字）
    '===========================================================
    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)

        ' タイトルバー領域
        'Dim titleRect As New Rectangle(0, 0, Me.Width, TitleHeight)

        ' タイトルバー背景
        Using b As New SolidBrush(Me.BackColor)
            e.Graphics.FillRectangle(b, 0, 0, Me.Width, TitleHeight)
        End Using

        ' タイトル文字（Font の new を毎回しない）
        e.Graphics.DrawString(Me.Text, titleFont, Brushes.Black, 18, 0)
    End Sub


    '===========================================================
    ' ★ HideButton_Click：付箋を非表示にする
    '===========================================================
    Private Sub HideButton_Click(sender As Object, e As EventArgs)

        MyRow("VisibleFlag") = False
        Me.Visible = False

        ' 表示メニュー更新
        parentForm.RebuildShowCheckMenu()

        Dim colorNumber As Integer = CInt(MyRow("MemoColor"))
        parentForm.RecalculateColorMenuCheckState(colorNumber)

        parentForm.mainData.AcceptChanges()
        parentForm.Save()
    End Sub



    '===========================================================
    ' ★ FileLabel_Click：添付ファイル／URL を開く
    '===========================================================
    Private Sub FileLabel_Click(sender As Object, e As EventArgs)

        Dim ref As String = MyRow("FileReference").ToString()
        If String.IsNullOrWhiteSpace(ref) Then Return

        ' DataRow の文字列を Enum に変換
        Dim typeStr As String = MyRow("FileType").ToString()
        Dim ftype As FileTypeEnum

        If [Enum].TryParse(typeStr, True, ftype) = False Then
            ftype = FileTypeEnum.None
        End If

        Select Case ftype

            Case FileTypeEnum.Local, FileTypeEnum.Original
                ' ローカルファイルを開く
                If IO.File.Exists(ref) Then
                    Dim psi As New ProcessStartInfo(ref)
                    psi.UseShellExecute = True
                    Process.Start(psi)
                Else
                    MessageBox.Show("ファイルが見つかりません: " & ref)
                End If

            Case FileTypeEnum.URL
                ' URL をブラウザで開く
                Dim psi As New ProcessStartInfo(ref)
                psi.UseShellExecute = True
                Process.Start(psi)

            Case FileTypeEnum.None
                Return

        End Select

    End Sub


    '===========================================================
    ' ★ MemoForm_Load：初期状態の UI を反映
    '===========================================================
    Private Sub MemoForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        ' 折り畳み状態
        If MyRow("CollapsedFlag") = True Then
            Me.Height = TitleHeight
        Else
            Me.Height = OriginalHeight
        End If

        ' TopMost 状態
        If MyRow("TopMostFlag") = True Then
            Me.TopMost = True
            pinLabel.Text = "📌"
        Else
            Me.TopMost = False
            pinLabel.Text = ""
        End If

        memoTip.SetToolTip(MemoDelete, "この付箋を削除します")
        memoTip.SetToolTip(remindButton, "通知する時間を設定します")

        ' 軽いイベント登録
        AddHandler TitleText.LostFocus, AddressOf TitleText_Leave
        AddHandler MemoText.LostFocus, AddressOf MemoText_Leave
        AddHandler MemoText.DragEnter, AddressOf MemoText_DragEnter
    End Sub


    '===========================================================
    ' ★ MemoForm_Shown：UI の最終調整（BeginInvoke）
    '===========================================================
    Private Sub MemoForm_Shown(sender As Object, e As EventArgs) Handles Me.Shown

        ' Shown では重い処理を避けるため BeginInvoke に移す
        Me.BeginInvoke(Sub()

                           ' fileLabel は必ず作る
                           If fileLabel Is Nothing Then
                               CreateFileLabel()
                           End If

                           ' clipLabel も必要なら作る
                           If clipLabel Is Nothing Then
                               CreateClipIcon()
                           End If

                           ' FileTypeEnum 判定
                           Dim typeStr As String = MyRow("FileType").ToString()
                           Dim ftype As FileTypeEnum
                           If [Enum].TryParse(typeStr, True, ftype) = False Then
                               ftype = FileTypeEnum.None
                           End If

                           ' URL のときだけ URL ボタン表示
                           If ftype = FileTypeEnum.URL Then
                               urlButton.Visible = True
                               memoTip.SetToolTip(urlButton, "URLを設定します")
                               TitleText.Width = 266
                           Else
                               urlButton.Visible = False
                               TitleText.Width = 292
                           End If

                           ' 添付ファイルがある場合のみ fileLabel を表示
                           Dim fileReference As String = MyRow("FileReference").ToString()
                           If Not String.IsNullOrWhiteSpace(fileReference) Then

                               AdjustFileLabel()

                               Select Case ftype
                                   Case FileTypeEnum.Local
                                       clipLabel.Text = "🗂"
                                       memoTip.SetToolTip(clipLabel, "ファイルを開きます")

                                   Case FileTypeEnum.Original
                                       clipLabel.Text = "📄"
                                       memoTip.SetToolTip(clipLabel, "ファイルを開きます")

                                   Case FileTypeEnum.URL
                                       clipLabel.Text = "🌐"
                                       fileLabel.Text = GetSmartUrlText(fileReference)
                                       memoTip.SetToolTip(clipLabel, "リンク先を開きます")

                                   Case Else
                                       clipLabel.Text = ""

                               End Select

                               fileLabel.Visible = True

                           Else
                               fileLabel.Visible = False
                               clipLabel.Text = ""
                           End If

                           ' 通知ラベルの生成・読み込み・チェック
                           CreateRemindLabel()
                           LoadRemindLabel()
                           CheckReminder()

                       End Sub)

    End Sub

    '===========================================================
    ' ★ TitleText_Leave：タイトル編集欄からフォーカスが外れたとき
    '   - DataRow の title を更新
    '   - フォームタイトルも更新
    '   - 再描画して反映
    '===========================================================
    Private Sub TitleText_Leave(sender As Object, e As EventArgs) Handles TitleText.Leave
        If parentForm.mainData Is Nothing Then Exit Sub

        MyRow("title") = TitleText.Text
        parentForm.mainData.AcceptChanges()
        parentForm.Save()

        Me.Text = TitleText.Text
        Me.Invalidate()   ' タイトルバー再描画
    End Sub


    '===========================================================
    ' ★ MemoText_Leave：本文編集欄からフォーカスが外れたとき
    '   - DataRow の text を更新
    '===========================================================
    Private Sub MemoText_Leave(sender As Object, e As EventArgs) Handles MemoText.Leave
        If parentForm.mainData Is Nothing Then Exit Sub

        MyRow("text") = MemoText.Text
        parentForm.mainData.AcceptChanges()
        parentForm.Save()
    End Sub


    '===========================================================
    ' ★ MemoText_DragEnter：ドラッグされたデータの種類判定
    '   - ファイルなら Copy を許可
    '===========================================================
    Private Sub MemoText_DragEnter(sender As Object, e As DragEventArgs)
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effect = DragDropEffects.Copy
        Else
            e.Effect = DragDropEffects.None
        End If
    End Sub

    '===========================================================
    ' ★ MemoText_DragDrop：ファイルドロップ処理
    '   - ファイル添付モードに応じて保存方法を決定
    '   - DataRow に FileReference / FileType を保存
    '   - UI（fileLabel / clipLabel）を更新
    '===========================================================
    Private Sub MemoText_DragDrop(sender As Object, e As DragEventArgs) Handles MemoText.DragDrop

        Dim files() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())
        If files Is Nothing OrElse files.Length = 0 Then Exit Sub

        Dim filePath As String = files(0)
        Dim fileName As String = IO.Path.GetFileName(filePath)

        ' DataViewForm の設定を取得
        Dim mode As String = parentForm.GetFileAttachMode()
        Dim finalPath As String = ""

        Select Case mode

        '-----------------------------------------------------------
        ' ★ ① LocalCopy → 警告なしで即コピー
        '-----------------------------------------------------------
            Case "LocalCopy"
                finalPath = SaveFileWithAutoNumber(filePath)
                MyRow("FileType") = FileTypeEnum.Local.ToString()

        '-----------------------------------------------------------
        ' ★ ② Original / UseURL → ユーザーに確認
        '-----------------------------------------------------------
            Case "UseOriginal", "UseURL"

                Dim result = MessageBox.Show(
                $"ファイル添付を検出しました。どう取り扱いますか？{vbCrLf}{vbCrLf}" &
                $"・はい → 元ファイルへのリンクを用意する{vbCrLf}" &
                $"・いいえ → 専用フォルダにコピーして保存",
                "ファイルの扱い",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1)

                If result = DialogResult.Yes Then
                    finalPath = filePath
                    MyRow("FileType") = FileTypeEnum.Original.ToString()
                Else
                    finalPath = SaveFileWithAutoNumber(filePath)
                    MyRow("FileType") = FileTypeEnum.Local.ToString()
                End If

        End Select

        '-----------------------------------------------------------
        ' ★ DataRow に保存
        '-----------------------------------------------------------
        MyRow("FileReference") = finalPath
        parentForm.mainData.AcceptChanges()
        parentForm.Save()

        '-----------------------------------------------------------
        ' ★ UI 更新
        '-----------------------------------------------------------
        AdjustFileLabel()
        fileLabel.Visible = True

        If clipLabel Is Nothing Then
            CreateClipIcon()
        End If

        ' FileType に応じてアイコン変更
        Dim ftype As FileTypeEnum
        If [Enum].TryParse(MyRow("FileType").ToString(), True, ftype) = False Then
            ftype = FileTypeEnum.None
        End If

        Select Case ftype
            Case FileTypeEnum.Local
                clipLabel.Text = "🗂"
                memoTip.SetToolTip(clipLabel, "ファイルを開きます")
            Case FileTypeEnum.Original
                clipLabel.Text = "📄"
                memoTip.SetToolTip(clipLabel, "ファイルを開きます")
            Case FileTypeEnum.URL
                clipLabel.Text = "🌐"
                memoTip.SetToolTip(clipLabel, "リンク先を開きます")
            Case Else
                clipLabel.Text = ""
        End Select

    End Sub


    '===========================================================
    ' ★ SaveFileWithAutoNumber：ファイルを連番付きで保存
    '   - file フォルダにコピー
    '   - 同名ファイルがある場合は _1, _2, ... を付ける
    '===========================================================
    Private Function SaveFileWithAutoNumber(filePath As String) As String

        Dim baseDir As String = Application.StartupPath
        Dim fileDir As String = IO.Path.Combine(baseDir, "file")

        If Not IO.Directory.Exists(fileDir) Then
            IO.Directory.CreateDirectory(fileDir)
        End If

        Dim fileName As String = IO.Path.GetFileName(filePath)
        Dim destPath As String = IO.Path.Combine(fileDir, fileName)

        Dim count As Integer = 1
        While IO.File.Exists(destPath)
            Dim nameOnly As String = IO.Path.GetFileNameWithoutExtension(fileName)
            Dim ext As String = IO.Path.GetExtension(fileName)
            destPath = IO.Path.Combine(fileDir, $"{nameOnly}_{count}{ext}")
            count += 1
        End While

        IO.File.Copy(filePath, destPath)

        Return destPath
    End Function



    '===========================================================
    ' ★ MemoText_TextChanged：行数に応じてスクロールバー表示
    '===========================================================
    Private Sub MemoText_TextChanged(sender As Object, e As EventArgs) Handles MemoText.TextChanged
        If MemoText.Lines.Length > 8 Then
            MemoText.ScrollBars = ScrollBars.Vertical
        Else
            MemoText.ScrollBars = ScrollBars.None
        End If
    End Sub


    '===========================================================
    ' ★ SaveRemindTime：通知日時を保存
    '===========================================================
    Private Sub SaveRemindTime(value As DateTime)
        MyRow("RemindTime") = value
        parentForm.mainData.AcceptChanges()
        parentForm.Save()
    End Sub



    '===========================================================
    ' ★ remindButton_Click：通知日時の設定／解除
    '===========================================================
    Private Sub remindButton_Click(sender As Object, e As EventArgs) Handles remindButton.Click

        '-----------------------------------------------------------
        ' ★ 既に通知日が設定されている → 解除
        '-----------------------------------------------------------
        If remindLabel.Text <> "" Then
            MyRow("RemindTime") = DBNull.Value
            parentForm.mainData.AcceptChanges()
            parentForm.Save()

            remindLabel.Text = ""
            CheckReminder()
            Return
        End If

        '-----------------------------------------------------------
        ' ★ 通知日を新規設定する
        '-----------------------------------------------------------
        Dim dtp As New DateTimePicker With {
        .Format = DateTimePickerFormat.Short,
        .Left = remindButton.Left + 26,
        .Top = remindButton.Top,
        .Width = 100,
        .ShowUpDown = False
    }

        Me.Controls.Add(dtp)
        dtp.BringToFront()

        AddHandler dtp.CloseUp, Sub()
                                    SaveRemindTime(dtp.Value.Date)
                                    remindLabel.Text = "次の通知日時：" & dtp.Value.ToString("yyyy/MM/dd")
                                    Me.Controls.Remove(dtp)
                                    dtp.Dispose()
                                    CheckReminder()
                                End Sub

        dtp.Select()
        SendKeys.Send("%{DOWN}")   ' カレンダーを開く
    End Sub


    '===========================================================
    ' ★ CreateRemindLabel：通知ラベルを作成
    '===========================================================
    Private Sub CreateRemindLabel()

        remindLabel = New System.Windows.Forms.Label With {
        .AutoSize = True,
        .Left = remindButton.Left + 22,
        .Top = remindButton.Top + 5,
        .Font = New Font("Yu Gothic UI, 8pt", 8),
        .ForeColor = Color.DarkBlue,
        .BackColor = Color.Transparent,
        .Text = ""
    }

        Me.Controls.Add(remindLabel)
    End Sub


    '===========================================================
    ' ★ LoadRemindLabel：通知ラベルの内容を DataRow から反映
    '===========================================================
    Private Sub LoadRemindLabel()

        Dim remindObj = MyRow("RemindTime")

        If Not IsDBNull(remindObj) Then
            Dim d As DateTime = CType(remindObj, DateTime)
            remindLabel.Text = "次の通知日時：" & d.ToString("yyyy/MM/dd")
        Else
            remindLabel.Text = ""
        End If

    End Sub


    '===========================================================
    ' ★ CheckReminder：通知状態に応じて hideButton の色を変更
    '===========================================================
    Public Sub CheckReminder()

        Dim remindObj = MyRow("RemindTime")

        If Not IsDBNull(remindObj) Then
            Dim remindTime As DateTime = CType(remindObj, DateTime)

            If DateTime.Now >= remindTime Then
                hideButton.CircleColor = Color.Red            ' 期限到達
            Else
                hideButton.CircleColor = Color.FromArgb(120, 180, 255) ' 期限前
            End If

        Else
            hideButton.CircleColor = Color.White             ' 未設定
        End If

        hideButton.Invalidate()
    End Sub



    '===========================================================
    ' ★ MemoDelete_Click：メモ削除処理
    '===========================================================
    Private Sub MemoDelete_Click(sender As Object, e As EventArgs) Handles MemoDelete.Click

        Dim result = MessageBox.Show(
        "このメモ（" & TitleText.Text & "）を削除しますか？",
        "メモ削除確認",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Warning
    )

        If result <> DialogResult.Yes Then Return

        Dim colorNumber As Integer = CInt(MyRow("MemoColor"))

        MyRow.Delete()
        parentForm.mainData.AcceptChanges()
        parentForm.Save()

        parentForm.RebuildShowCheckMenu()
        parentForm.RecalculateColorMenuCheckState(colorNumber)

        Me.Close()
    End Sub


    '===========================================================
    ' ★ AdjustFileLabel：添付ファイル名を短縮して表示
    '===========================================================
    Private Sub AdjustFileLabel()

        Dim fullText As String = IO.Path.GetFileName(MyRow("FileReference").ToString())
        fileLabel.Text = fullText

        Dim textSize As Size = TextRenderer.MeasureText(fullText, fileLabel.Font)

        If textSize.Width > fileLabel.Width Then

            Dim shortText As String = fullText

            ' 収まるまで末尾を削る
            While TextRenderer.MeasureText(shortText & "…", fileLabel.Font).Width > fileLabel.Width
                shortText = shortText.Substring(0, shortText.Length - 1)
            End While

            fileLabel.Text = shortText & "…"
        End If

    End Sub


    '===========================================================
    ' ★ urlButton_Click：URL 添付／削除処理
    '===========================================================
    Private Sub urlButton_Click(sender As Object, e As EventArgs) Handles urlButton.Click

        Dim currentUrl As String = MyRow("FileReference").ToString()

        '-----------------------------------------------------------
        ' ★ 既存 URL → 削除モード
        '-----------------------------------------------------------
        If Not String.IsNullOrWhiteSpace(currentUrl) Then

            Dim result = MessageBox.Show(
            "現在の URL を削除しますか？" & vbCrLf & vbCrLf &
            currentUrl,
            "URL 削除確認",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question)

            If result = DialogResult.Yes Then
                MyRow("FileReference") = ""
                MyRow("FileType") = FileTypeEnum.None.ToString()
                parentForm.mainData.AcceptChanges()
                parentForm.Save()

                fileLabel.Visible = False
                clipLabel.Text = ""
            End If

            Return
        End If

        '-----------------------------------------------------------
        ' ★ 新規 URL 入力モード
        '-----------------------------------------------------------
        Dim inputUrl As String = InputBox("URL を入力してください", "URL 添付")

        If String.IsNullOrWhiteSpace(inputUrl) Then Return

        ' URL バリデーション
        If Not IsValidUrl(inputUrl) Then
            MessageBox.Show(
            "URL の形式が正しくありません。" & vbCrLf &
            "http:// または https:// で始まる URL を入力してください。",
            "URL エラー",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning)
            Return
        End If

        ' URL 保存
        MyRow("FileReference") = inputUrl
        MyRow("FileType") = FileTypeEnum.URL.ToString()

        ' Web ページタイトル取得（失敗時は空）
        Dim pageTitle As String = GetUrlTitleSafe(inputUrl)

        If pageTitle <> "" Then
            MyRow("title") = pageTitle
            TitleText.Text = pageTitle
            Me.Text = pageTitle
            parentForm.mainData.AcceptChanges()
            parentForm.Save()
            Me.Invalidate()
        End If

        ' UI 更新
        fileLabel.Text = GetSmartUrlText(inputUrl)
        fileLabel.Visible = True

        clipLabel.Text = "🌐"
        memoTip.SetToolTip(clipLabel, "リンク先を開きます")

        TitleText.Width = 266
        urlButton.Visible = True

    End Sub



    '===========================================================
    ' ★ IsValidUrl：URL の基本的な形式チェック
    '===========================================================
    Private Function IsValidUrl(url As String) As Boolean

        If String.IsNullOrWhiteSpace(url) Then Return False

        ' 正規表現で http:// または https:// をチェック
        Dim pattern As String = "^(https?://)"
        If Not System.Text.RegularExpressions.Regex.IsMatch(url, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase) Then
            Return False
        End If

        ' Uri.TryCreate で構文チェック
        Dim uri As Uri = Nothing
        If Not Uri.TryCreate(url, UriKind.Absolute, uri) Then
            Return False
        End If

        ' スキームが http/https 以外なら拒否
        If uri.Scheme <> Uri.UriSchemeHttp AndAlso uri.Scheme <> Uri.UriSchemeHttps Then
            Return False
        End If

        Return True
    End Function


    '===========================================================
    ' ★ GetSmartUrlText：URL を短縮して表示用に整形
    '===========================================================
    Private Function GetSmartUrlText(url As String) As String
        Try
            Dim uri As New Uri(url)

            Dim host As String = uri.Host
            Dim segments = uri.Segments
            Dim firstSegment As String = ""

            If segments.Length > 1 Then
                firstSegment = segments(1).Trim("/"c)
            End If

            If firstSegment = "" Then
                Return host
            Else
                Return $"{host} / {firstSegment} / …"
            End If

        Catch ex As Exception
            Return url
        End Try
    End Function


    '===========================================================
    ' ★ GetUrlTitleSafe：URL の <title> を安全に取得
    '   - 企業ネットワーク向けにプロキシ対応
    '   - 証明書エラーを無視（MITM 対策）
    '   - タイムアウト短め（固まらないように）
    '===========================================================
    Private Function GetUrlTitleSafe(url As String) As String

        Try
            Dim handler As New Net.Http.HttpClientHandler()

            handler.UseProxy = True
            handler.Proxy = Net.WebRequest.DefaultWebProxy
            handler.Proxy.Credentials = Net.CredentialCache.DefaultCredentials

            handler.ServerCertificateCustomValidationCallback =
                Function(sender, cert, chain, sslPolicyErrors) True

            Using client As New Net.Http.HttpClient(handler)
                client.Timeout = TimeSpan.FromSeconds(3)

                Dim html As String = client.GetStringAsync(url).Result

                Dim start As Integer = html.IndexOf("<title>", StringComparison.OrdinalIgnoreCase)
                Dim endPos As Integer = html.IndexOf("</title>", StringComparison.OrdinalIgnoreCase)

                If start >= 0 AndAlso endPos > start Then
                    Dim title As String = html.Substring(start + 7, endPos - (start + 7))
                    Return title.Trim()
                End If
            End Using

        Catch ex As Exception
            Debug.WriteLine("タイトル取得失敗: " & ex.Message)
        End Try

        Return ""
    End Function




End Class


'===========================================================
' ★ CircleButton：丸ボタン（非表示ボタンの描画）
'===========================================================
Public Class CircleButton
    Inherits System.Windows.Forms.Button

    Public Property CircleColor As Color = Color.White
    Public Property CircleBorderColor As Color = Color.Black

    Protected Overrides Sub OnPaint(pevent As PaintEventArgs)
        MyBase.OnPaint(pevent)

        Dim g = pevent.Graphics
        g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias

        Dim rect As New Rectangle(0, 0, Me.Width - 1, Me.Height - 1)

        ' 塗りつぶし
        Using b As New SolidBrush(CircleColor)
            g.FillEllipse(b, rect)
        End Using

        ' 黒縁（丸）
        Using p As New Pen(CircleBorderColor, 1)
            g.DrawEllipse(p, rect)
        End Using
    End Sub

End Class