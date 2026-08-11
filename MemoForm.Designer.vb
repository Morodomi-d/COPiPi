<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MemoForm
    Inherits System.Windows.Forms.Form

    'フォームがコンポーネントの一覧をクリーンアップするために dispose をオーバーライドします。
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows フォーム デザイナーで必要です。
    Private components As System.ComponentModel.IContainer

    'メモ: 以下のプロシージャは Windows フォーム デザイナーで必要です。
    'Windows フォーム デザイナーを使用して変更できます。  
    'コード エディターを使って変更しないでください。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        TitleText = New TextBox()
        Label2 = New Label()
        MemoText = New TextBox()
        IDLabel = New Label()
        PrintDocument1 = New Printing.PrintDocument()
        remindButton = New Button()
        MemoDelete = New Button()
        urlButton = New Button()
        SuspendLayout()
        ' 
        ' TitleText
        ' 
        TitleText.Location = New Point(4, 19)
        TitleText.Name = "TitleText"
        TitleText.RightToLeft = RightToLeft.No
        TitleText.Size = New Size(292, 23)
        TitleText.TabIndex = 1
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Location = New Point(13, 43)
        Label2.Name = "Label2"
        Label2.Size = New Size(0, 15)
        Label2.TabIndex = 2
        ' 
        ' MemoText
        ' 
        MemoText.AllowDrop = True
        MemoText.Location = New Point(4, 48)
        MemoText.Multiline = True
        MemoText.Name = "MemoText"
        MemoText.RightToLeft = RightToLeft.No
        MemoText.Size = New Size(292, 130)
        MemoText.TabIndex = 3
        ' 
        ' IDLabel
        ' 
        IDLabel.AutoSize = True
        IDLabel.Location = New Point(13, 26)
        IDLabel.Name = "IDLabel"
        IDLabel.Size = New Size(41, 15)
        IDLabel.TabIndex = 4
        IDLabel.Text = "Label1"
        ' 
        ' remindButton
        ' 
        remindButton.Font = New Font("Segoe UI Emoji", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        remindButton.Location = New Point(4, 180)
        remindButton.Name = "remindButton"
        remindButton.Size = New Size(23, 23)
        remindButton.TabIndex = 5
        remindButton.Text = "⏰"
        remindButton.UseVisualStyleBackColor = True
        ' 
        ' MemoDelete
        ' 
        MemoDelete.Location = New Point(273, 180)
        MemoDelete.Name = "MemoDelete"
        MemoDelete.Size = New Size(23, 23)
        MemoDelete.TabIndex = 6
        MemoDelete.Text = "🗑️"
        MemoDelete.UseVisualStyleBackColor = True
        ' 
        ' urlButton
        ' 
        urlButton.Location = New Point(273, 19)
        urlButton.Name = "urlButton"
        urlButton.Size = New Size(23, 23)
        urlButton.TabIndex = 7
        urlButton.Text = "🌎"
        urlButton.UseVisualStyleBackColor = True
        ' 
        ' MemoForm
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(300, 206)
        Controls.Add(urlButton)
        Controls.Add(MemoDelete)
        Controls.Add(remindButton)
        Controls.Add(MemoText)
        Controls.Add(Label2)
        Controls.Add(TitleText)
        Controls.Add(IDLabel)
        FormBorderStyle = FormBorderStyle.None
        Name = "MemoForm"
        RightToLeft = RightToLeft.Yes
        ShowInTaskbar = False
        Text = "MemoForm"
        ResumeLayout(False)
        PerformLayout()
    End Sub
    Friend WithEvents TitleText As TextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents MemoText As TextBox
    Friend WithEvents IDLabel As Label
    Friend WithEvents PrintDocument1 As Printing.PrintDocument
    Friend WithEvents remindButton As Button
    Friend WithEvents MemoDelete As Button
    Friend WithEvents urlButton As Button
End Class
