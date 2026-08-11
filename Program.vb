Imports System.Windows.Forms

Module Program
    Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New TrayAppContext())
    End Sub
End Module