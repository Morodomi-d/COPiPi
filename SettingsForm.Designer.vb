<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class SettingsForm
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(SettingsForm))
        Label1 = New Label()
        GroupBox1 = New GroupBox()
        rbURL = New RadioButton()
        rbOriginal = New RadioButton()
        rbLocalCopy = New RadioButton()
        btnOK = New Button()
        btnCancel = New Button()
        GroupBox1.SuspendLayout()
        SuspendLayout()
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(17, 14)
        Label1.Name = "Label1"
        Label1.Size = New Size(267, 15)
        Label1.TabIndex = 0
        Label1.Text = "COPiPi設定【添付ファイルの保存先を設定してください】"
        ' 
        ' GroupBox1
        ' 
        GroupBox1.Controls.Add(rbURL)
        GroupBox1.Controls.Add(rbOriginal)
        GroupBox1.Controls.Add(rbLocalCopy)
        GroupBox1.Location = New Point(26, 41)
        GroupBox1.Name = "GroupBox1"
        GroupBox1.Size = New Size(523, 108)
        GroupBox1.TabIndex = 1
        GroupBox1.TabStop = False
        GroupBox1.Text = "COPiPiでは付箋にファイルを添付することが可能です。"
        ' 
        ' rbURL
        ' 
        rbURL.AutoSize = True
        rbURL.Location = New Point(13, 78)
        rbURL.Name = "rbURL"
        rbURL.Size = New Size(232, 19)
        rbURL.TabIndex = 2
        rbURL.TabStop = True
        rbURL.Text = "URL（OneDrive）も使う（クラウドリンク）"
        rbURL.UseVisualStyleBackColor = True
        ' 
        ' rbOriginal
        ' 
        rbOriginal.AutoSize = True
        rbOriginal.Location = New Point(13, 52)
        rbOriginal.Name = "rbOriginal"
        rbOriginal.Size = New Size(497, 19)
        rbOriginal.TabIndex = 1
        rbOriginal.TabStop = True
        rbOriginal.Text = "添付したファイルをそのまま使います（リンクとして扱います。添付元ファイルは動かさないでくださいね）"
        rbOriginal.UseVisualStyleBackColor = True
        ' 
        ' rbLocalCopy
        ' 
        rbLocalCopy.AutoSize = True
        rbLocalCopy.Location = New Point(13, 25)
        rbLocalCopy.Name = "rbLocalCopy"
        rbLocalCopy.Size = New Size(365, 19)
        rbLocalCopy.TabIndex = 0
        rbLocalCopy.TabStop = True
        rbLocalCopy.Text = "添付されたファイルをコピーして保存します（専用フォルダに保存）【推奨】"
        rbLocalCopy.UseVisualStyleBackColor = True
        ' 
        ' btnOK
        ' 
        btnOK.Location = New Point(474, 155)
        btnOK.Name = "btnOK"
        btnOK.Size = New Size(75, 23)
        btnOK.TabIndex = 2
        btnOK.Text = "OK"
        btnOK.UseVisualStyleBackColor = True
        ' 
        ' btnCancel
        ' 
        btnCancel.Location = New Point(393, 155)
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New Size(75, 23)
        btnCancel.TabIndex = 3
        btnCancel.Text = "キャンセル"
        btnCancel.UseVisualStyleBackColor = True
        ' 
        ' SettingsForm
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(573, 183)
        Controls.Add(btnCancel)
        Controls.Add(btnOK)
        Controls.Add(GroupBox1)
        Controls.Add(Label1)
        FormBorderStyle = FormBorderStyle.FixedSingle
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        MaximizeBox = False
        MinimizeBox = False
        Name = "SettingsForm"
        Text = "設定"
        GroupBox1.ResumeLayout(False)
        GroupBox1.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents rbLocalCopy As RadioButton
    Friend WithEvents rbURL As RadioButton
    Friend WithEvents rbOriginal As RadioButton
    Friend WithEvents btnOK As Button
    Friend WithEvents btnCancel As Button
End Class
