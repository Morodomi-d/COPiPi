Public Class TrayAppContext
    Inherits ApplicationContext

    Private manager As DataViewForm

    Public Sub New()
        manager = New DataViewForm()

        ' ★ 初期化処理を必ず呼ぶ
        manager.Init()

        manager.ShowInTaskbar = False
        manager.Visible = False
    End Sub
End Class
