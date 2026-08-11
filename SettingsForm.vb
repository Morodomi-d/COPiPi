Public Class SettingsForm

    Public Property FileAttachMode As String

    Private ownerForm As DataViewForm

    Public Sub New(parentForm As DataViewForm)
        InitializeComponent()
        ownerForm = parentForm
    End Sub

    Private Sub SettingsForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim mode = ownerForm.LoadSetting("FileAttachMode")

        Select Case mode
            Case "LocalCopy" : rbLocalCopy.Checked = True
            Case "UseOriginal" : rbOriginal.Checked = True
            Case "UseURL" : rbURL.Checked = True
        End Select
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        If rbLocalCopy.Checked Then FileAttachMode = "LocalCopy"
        If rbOriginal.Checked Then FileAttachMode = "UseOriginal"
        If rbURL.Checked Then FileAttachMode = "UseURL"

        ownerForm.SetFileAttachMode(FileAttachMode)
        ownerForm.SaveSetting("FileAttachMode", FileAttachMode)
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

End Class
