Imports System.IO
Imports System.Net
Imports System.Text
Class Web_update
    Dim version As String
    Public Shared Downuri As String



    Public Shared Sub Main()
        Dim Client As New Net.WebClient()
        Dim URI As String
        Dim htmlURL As String = Form1.ListBox1.Items(4).ToString()
        Dim exeURL As String = Form1.ListBox1.Items(5).ToString()
        URI = htmlURL
        Downuri = exeURL

        Dim wr As HttpWebRequest = CType(WebRequest.Create(URI.ToString), HttpWebRequest)
        Dim ws As HttpWebResponse = CType(wr.GetResponse(), HttpWebResponse)
        Dim str As Stream = ws.GetResponseStream()
        Dim inBuf(100000) As Byte
        Dim bytesToRead As Integer = CInt(inBuf.Length)
        Dim bytesRead As Integer = 0
        While bytesToRead > 0
            Dim n As Integer = str.Read(inBuf, bytesRead, bytesToRead)
            If n = 0 Then
                Exit While
            End If
            bytesRead += n
            bytesToRead -= n
        End While
        Dim fstr As New FileStream("client_version.txt", FileMode.OpenOrCreate, FileAccess.Write)
        fstr.Write(inBuf, 0, bytesRead)
        str.Close()
        fstr.Close()
        Dim sr As StreamReader = New System.IO.StreamReader("client_version.txt")
        Dim version As Integer = CInt(sr.ReadToEnd.Replace(".", "").Substring(0, 4))
        sr.Close()
        If version > CInt(Application.ProductVersion.Replace(".", "")) Then
            File.Move(Application.StartupPath & "\GitLauncher.exe", _
                       Application.StartupPath & "\" & _
                       "GitLauncher.exe.old")
            Client.DownloadFile(Web_update.Downuri, _
Application.StartupPath & "\GitLauncher.exe")
            Process.Start(Application.StartupPath & _
            "\GitLauncher.exe")
            Application.Exit()
        End If
    End Sub
End Class