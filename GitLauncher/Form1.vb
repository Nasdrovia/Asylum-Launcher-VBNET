Imports System.Security.Principal
Imports System.IO
Imports System.Net

Public Class Form1
    Dim path As String
    Dim fullPath As String = "Empty"
    Dim pathSaveFile As String = Application.StartupPath & "\path.txt"
    Dim updateNeeded As Boolean = False
    Dim i As Integer = 0
    Dim o As Integer = 0
    Public WithEvents wc As New WebClient()
    Dim SW As Stopwatch
    Dim identity = WindowsIdentity.GetCurrent()
    Dim principal = New WindowsPrincipal(identity)
    Dim isElevated As Boolean = principal.IsInRole(WindowsBuiltInRole.Administrator)
    Dim INI_Remote = Application.StartupPath & "\remote.ini"
    Public fs As FileStream
    Dim steamPATHREG = My.Computer.Registry.GetValue(
    "HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", Nothing) + "/"
    Dim something As String = "something"
    Dim gameDir As String = "SteamApps/common/Game"
    Dim downloading As Boolean = False
    Dim updateURL As String
    Dim varsURL As String
    Dim updateCounter As Integer

    Public Sub downloadData()
        Dim Lines() As String
        Dim stringSeparators() As String = {vbCrLf}
        Dim data As String = wc.DownloadString("http://localhost/vars.txt")
        Lines = data.Split(stringSeparators, StringSplitOptions.None)
        For Each s As String In Lines
            ListBox1.Items.Add(s)
        Next
    End Sub
    Private Shared Function CreateMD5StringFromFile(ByVal Filename As String) As String

        Dim MD5 = System.Security.Cryptography.MD5.Create
        Dim Hash As Byte()
        Dim sb As New System.Text.StringBuilder


        Using st As New IO.FileStream(Filename, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
            Hash = MD5.ComputeHash(st)
        End Using

        For Each b In Hash
            sb.Append(b.ToString("X2"))
        Next

        Return sb.ToString
    End Function
    Public Sub checkUpdate()
        If fullPath = "Empty" Or fullPath = "" Then
            MsgBox("Select a folder first")
            ChooseFolder()
        Else
            updateNeeded = False
            downloadMD5List()
            Dim INI_RemoteCreate As New IniFile(Application.StartupPath & "\remote.ini")
            Dim INI_LocalCreate As New IniFile(Application.StartupPath & "\local.ini")
            Dim iniValueMD5L As String = "0"
            Dim iniValueMD5R As String = "0"
            Dim iniValueNameL As String = "0"
            Dim iniValueNameR As String = "0"
            Dim iniValueCountR As Integer = INI_RemoteCreate.GetString(0, "count", "(none)")
            Dim localCount As Integer = IO.Directory.GetFiles(fullPath).Length
            Dim md5String As String
            i = 0
            ListBox2.Items.Clear()
            For index As Integer = 1 To iniValueCountR
               
                If My.Computer.FileSystem.FileExists(fullPath + "\" + INI_RemoteCreate.GetString(index, "fileName", "(none)")) = False Then
                    ListBox2.Items.Add(INI_RemoteCreate.GetString(index, "fileName", "(none)"))
                    md5String = "Empty"
                    updateNeeded = True
                Else
                    md5String = CreateMD5StringFromFile(fullPath + "\" + INI_RemoteCreate.GetString(index, "fileName", "(none)"))
                    
                End If
                INI_LocalCreate.WriteString(index, "md5", md5String)
                INI_LocalCreate.WriteString(index, "fileName", INI_RemoteCreate.GetString(index, "fileName", "(none)"))
            Next
            For index As Integer = 1 To iniValueCountR
                iniValueMD5L = INI_LocalCreate.GetString(index, "md5", "(none)")
                iniValueNameL = INI_LocalCreate.GetString(index, "fileName", "(none)")
                iniValueMD5R = INI_RemoteCreate.GetString(index, "md5", "(none)")
                iniValueNameR = INI_RemoteCreate.GetString(index, "fileName", "(none)")
                If My.Computer.FileSystem.FileExists(fullPath + "\" + INI_RemoteCreate.GetString(index, "fileName", "(none)")) = True Then
                    If (iniValueMD5L <> iniValueMD5R And iniValueNameL = iniValueNameR) = True Then
                        ListBox2.Items.Add(INI_RemoteCreate.GetString(index, "fileName", "(none)"))
                        i = i + 1
                        updateNeeded = True
                    End If
                End If
                
            Next
            updateButtons()
        End If
    End Sub
    Public Sub downloadMD5List()
        My.Computer.Network.DownloadFile(
   "http://localhost/filesList.ini",
   INI_Remote, String.Empty, String.Empty, False, 100000, True)
    End Sub
    Public Sub updateButtons()
        If updateNeeded = True Then
            Button1.Text = "Update"
            TextBox11.Text = CStr(ListBox2.Items.Count) + " updates"
            MsgBox("New update")
        Else
            Button1.Text = "Go"
            TextBox11.Text = "Ready"
            MsgBox("Nothing to update")
        End If
    End Sub
    Public Sub ChooseFolder()
        If FolderBrowserDialog1.ShowDialog() = DialogResult.OK Then
            TextBox1.Text = FolderBrowserDialog1.SelectedPath + "\" + something
            My.Computer.FileSystem.WriteAllText(pathSaveFile, TextBox1.Text, False)
            If My.Computer.FileSystem.DirectoryExists(TextBox1.Text + "\something") Then
                fullPath = TextBox1.Text + "\something"
            Else
                My.Computer.FileSystem.CreateDirectory(TextBox1.Text + "\something")
                fullPath = TextBox1.Text + "\something"
            End If
            checkUpdate()
        End If
    End Sub
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        ChooseFolder()
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If isElevated = False Then
            MsgBox("Please launch the launcher as an Administrator !")
            End
            Application.Exit()
        End If
        downloadData()
        'Web_update.Main()
        updateURL = ListBox1.Items(3).ToString()
        TextBox5.Text = ListBox1.Items(0).ToString()
        TextBox7.Text = ListBox1.Items(1).ToString()
        TextBox9.Text = ListBox1.Items(2).ToString()
        If My.Computer.FileSystem.FileExists(pathSaveFile) Then
            If My.Computer.FileSystem.DirectoryExists(TextBox1.Text + "/something") Then
                fullPath = TextBox1.Text + "/something"
            Else
                My.Computer.FileSystem.CreateDirectory(TextBox1.Text + "/something")
                fullPath = TextBox1.Text + "/something"
            End If
            path = My.Computer.FileSystem.ReadAllText(pathSaveFile)
            If path = "" Then
                ChooseFolder()
            End If
            TextBox1.Text = path
            fullPath = path + "\something"
        Else
            fs = File.Create(pathSaveFile)
            fs.Close()
            Dim message As Integer
            message = MsgBox("Folder = " + steamPATHREG + "SteamApps/common/Game" + " ?", MsgBoxStyle.YesNo)
            If message = MsgBoxResult.Yes Then
                TextBox1.Text = steamPATHREG + "SteamApps/common/Game"
                My.Computer.FileSystem.WriteAllText(pathSaveFile, TextBox1.Text, False)
                If My.Computer.FileSystem.DirectoryExists(TextBox1.Text + "/something") Then
                    fullPath = TextBox1.Text + "/something"
                Else
                    My.Computer.FileSystem.CreateDirectory(TextBox1.Text + "/something")
                    fullPath = TextBox1.Text + "/something"
                End If
            Else
                ChooseFolder()
            End If
        End If
        checkUpdate()
        TextBox4.Text = "0.00 MB / 0.00 MB ( 0 % )"
        TextBox3.Text = "0.00 KB/s"
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        checkUpdate()
    End Sub
    Private Sub Form1_Closing(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        If My.Computer.FileSystem.FileExists(Application.StartupPath + "\client_version.txt") Then
            File.Delete(Application.StartupPath + "\client_version.txt")
        End If
        If My.Computer.FileSystem.FileExists(Application.StartupPath + "\local.ini") Then
            File.Delete(Application.StartupPath + "\local.ini")
        End If
        If My.Computer.FileSystem.FileExists(Application.StartupPath + "\remote.ini") Then
            File.Delete(Application.StartupPath + "\remote.ini")
        End If
    End Sub
    Private Sub downloadFiles()
        If o >= ListBox2.Items.Count Then
            TextBox3.Text = "0.00 KB/s"
            Button1.Enabled = True
            Button2.Enabled = True
            Button3.Enabled = True
            wc.CancelAsync()
            checkUpdate()
            Return
        End If
        Try
            If downloading = False Then
                Button1.Enabled = False
                Button2.Enabled = False
                Button3.Enabled = False
                SW = Stopwatch.StartNew
                wc = New WebClient
                wc.DownloadFileAsync(New Uri(updateURL + ListBox2.Items.Item(o)), fullPath + "\" + ListBox2.Items.Item(o))
                downloading = True
            End If
        Catch ex As Exception
            MsgBox(ex)
        End Try
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If updateNeeded Then
            If TextBox1.Text = "" Then
                MsgBox("Select a folder")
                ChooseFolder()
            Else
                o = 0
                updateCounter = ListBox2.Items.Count
                downloadFiles()
            End If
        Else
            MsgBox("Running the game")
        End If
    End Sub
    Private Sub wc_DownloadFileCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.AsyncCompletedEventArgs) Handles wc.DownloadFileCompleted
        o = o + 1
        downloading = False
        updateCounter = updateCounter - 1
        TextBox11.Text = CStr(updateCounter) + " updates"
        downloadFiles()
    End Sub
    Private Sub wc_DownloadProgressChanged(ByVal sender As Object, ByVal e As System.Net.DownloadProgressChangedEventArgs) Handles wc.DownloadProgressChanged
        Try
            ProgressBar1.Value = e.ProgressPercentage
            TextBox4.Text = (e.BytesReceived / 1024 / 1024).ToString("0.00") & " MB / " & (e.TotalBytesToReceive / 1024 / 1024).ToString("0.00") & " MB " & "( " & e.ProgressPercentage & " %" & " )"
            TextBox3.Text = (e.BytesReceived / SW.ElapsedMilliseconds).ToString("0.00") & " KB/s"
        Catch ex As Exception
            wc.CancelAsync()
            MsgBox(ex)
        End Try
    End Sub
End Class
