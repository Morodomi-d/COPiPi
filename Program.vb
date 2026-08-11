Imports System.Threading
Imports System.Windows.Forms

Module Program


    Sub Main()
            Dim createdNew As Boolean
            Dim mutex As New Mutex(True, "COPiPiMutex", createdNew)

            If Not createdNew Then
                MessageBox.Show("COPiPi はすでに起動しています。")
                Return
            End If

            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New TrayAppContext())

        mutex.ReleaseMutex()
        End Sub

End Module