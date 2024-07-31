Imports System.IO
Imports System.Net.Http
Imports ActaDumpperBot.Core
Imports Guna.UI2.WinForms
Imports System.Text.Json
Imports System.Collections.Specialized.BitVector32

Public Class Form1

    Public PThread As ThreadPlus = Nothing
    Dim CSVPath As String = String.Empty

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim SavePath As String = My.Settings.SavePath

        If String.IsNullOrWhiteSpace(SavePath) Then
            SavePath = IO.Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Dumps")
            CSVPath = IO.Path.Combine(SavePath, "Record.csv")
        End If

        Guna2TextBox1.Text = SavePath
        StartValueTextbox.Text = My.Settings.CurrentDNI
        EndValueTextbox.Text = My.Settings.LastDNI

    End Sub


#Region " UI "

    Private Sub Guna2Button4_Click(sender As Object, e As EventArgs) Handles Guna2Button4.Click
        My.Settings.Reset()
        Process.Start(Application.ExecutablePath)
        Process.GetCurrentProcess.Kill()
        Me.Close()
    End Sub


    Private Sub Guna2Button3_Click(sender As Object, e As EventArgs) Handles Guna2Button3.Click
        Dim FolderSelector As New FolderBrowserDialog With {.Description = "Select Folder to Save Dumps"}
        If FolderSelector.ShowDialog(Me) = DialogResult.OK Then
            Dim Path As String = FolderSelector.SelectedPath
            Guna2TextBox1.Text = Path
            CSVPath = IO.Path.Combine(Path, "Record.csv")
            My.Settings.SavePath = Path
            My.Settings.Save()
        End If
    End Sub

    Private Sub OnlyNumbers_KeyPress(sender As Object, e As KeyPressEventArgs) Handles StartValueTextbox.KeyPress, EndValueTextbox.KeyPress
        If Not Char.IsControl(e.ToString) AndAlso Not Char.IsDigit(e.KeyChar) Then
            e.Handled = True
        End If
    End Sub

    Private Sub ParseStringNumbers_Leave(sender As Object, e As EventArgs) Handles StartValueTextbox.TextChanged, EndValueTextbox.TextChanged, StartValueTextbox.Leave, EndValueTextbox.Leave
        Dim tb As Guna2TextBox = DirectCast(sender, Guna2TextBox)

        If String.IsNullOrEmpty(tb.Text) Then
            If tb Is StartValueTextbox Then
                tb.Text = 3911001
            ElseIf tb Is EndValueTextbox Then
                'tb.Text = 3911001 + 1
            End If
        Else
            Dim text As String = tb.Text.Replace(".", "")
            Dim value As Decimal
            If Decimal.TryParse(text, value) Then
                tb.Text = String.Format("{0:N0}", value).Replace(",", ".")
            End If
        End If
    End Sub

    Private Sub Guna2Button1_Click(sender As Object, e As EventArgs) Handles Guna2Button1.Click
        If PThread Is Nothing Then
            DumpFolder = Guna2TextBox1.Text
            StartDNI = Val(RemoveNonDigitCharacters(StartValueTextbox.Text))
            OnStopDNI = Val(RemoveNonDigitCharacters(EndValueTextbox.Text))

            If StartDNI = 0 Then
                WriteStatus("The start value must be greater than 3911001", Color.Red)
                Exit Sub
            End If

            If String.IsNullOrWhiteSpace(EndValueTextbox.Text) = False Then
                If OnStopDNI > StartDNI Then
                    WriteStatus("The start value must be less than the end value.", Color.Red)
                    Exit Sub
                End If
            End If

            PThread = New ThreadPlus(AddressOf RunThread)
            SetBoost()
            Guna2Button1.Text = "Pause"
            PThread.Start()
            Guna2Button2.Enabled = True
        Else
            If Guna2Button1.Text.Equals("Pause", StringComparison.OrdinalIgnoreCase) Then
                Guna2Button1.Text = "Resume"
                PThread.PauseThread()
            Else
                Guna2Button1.Text = "Pause"
                PThread.ResumeThread()
            End If
        End If
    End Sub

    Private Sub Guna2Button2_Click(sender As Object, e As EventArgs) Handles Guna2Button2.Click
        ResetUI()
    End Sub

    Private Sub ResetUI()
        Guna2Button2.Enabled = False
        Guna2Button1.Enabled = False
        Guna2Button1.Text = "Cancelling..."
        If PThread IsNot Nothing Then PThread.Dispose() : PThread = Nothing
        Guna2Button1.Text = "Start"
        Guna2Button1.Enabled = True
        ResetLog()
    End Sub

    Private Sub WriteLog(ByVal Message As String)
        Me.BeginInvoke(Sub()
                           LogTextBox.Text += Message & Environment.NewLine
                       End Sub)
    End Sub

    Private Sub ResetLog()
        Me.BeginInvoke(Sub()
                           LogTextBox.Text = ""
                       End Sub)
    End Sub

    Private Sub WriteStatus(ByVal Message As String, ByVal Color As Color)
        Me.BeginInvoke(Sub()
                           LabelStatus.Text = Message
                           LabelStatus.BackColor = Color
                       End Sub)
    End Sub

    Private Sub UpdateDNI(ByVal DNI As String)
        Me.BeginInvoke(Sub()
                           StartValueTextbox.Text = DNI
                           My.Settings.CurrentDNI = DNI
                           My.Settings.LastDNI = EndValueTextbox.Text
                           My.Settings.Save()
                       End Sub)
    End Sub

#End Region

#Region " Methods "

    Dim DumpFolder As String = String.Empty
    Dim StartDNI As Integer = 0
    Dim OnStopDNI As Integer = 0

    Public Async Sub RunThread()
        Dim headers As New List(Of String) From {"DNI", "Url", "File"}
        Dim csv As New CsvCreator(headers)



        Dim currentNumber As Integer = StartDNI
        Dim Retry As Integer = 0

        While True

            Try
                ResetLog()
                UpdateDNI(currentNumber)
                If Not Directory.Exists(DumpFolder) Then Directory.CreateDirectory(DumpFolder)

                Dim CurrentDNI As String = String.Format("{0:N0}", currentNumber).Replace(",", ".")
                Dim Message As String = "GET: V-" & CurrentDNI

                WriteStatus(Message, Color.LightGray)

                Dim GetURL As String = Core.Globals.MakeUrlRequest(currentNumber)
                Using Client As HttpBotClient = HttpBotClient.SetupSession(GetURL)

                    'Console.WriteLine("GET " & GetURL)
                    WriteLog("Sending " & Message)
                    Client.Timeout = TimeSpan.FromSeconds(10)
                    Dim Response As HttpResponseMessage = Await Client.GetAsync(GetURL)

                    WriteLog("Response Code: " & Response.StatusCode)
                    WriteLog("IsSuccessStatusCode: " & Response.IsSuccessStatusCode)

                    If Response.IsSuccessStatusCode Then

                        Dim Content As String = Await Response.Content.ReadAsStringAsync()

                        WriteLog("Conten Length: " & Content.Length)

                        WriteStatus(Message.Replace("GET", "Parsing..."), Color.LightGray)

                        Dim jsonDoc As JsonDocument = ParseJson(Content)
                        If jsonDoc IsNot Nothing Then
                            Dim UrlElement As JsonElement = Nothing
                            If jsonDoc.RootElement.TryGetProperty("url", UrlElement) Then

                                Dim url As String = UrlElement.ToString()
                                Dim ImageName As String = Path.GetFileName(url)
                                Dim SaveImagePath As String = Path.Combine(DumpFolder, ImageName)

                                If File.Exists(SaveImagePath) = False Then
                                    WriteLog("Getting Acta.... Please Wait!")
                                    WriteStatus("Downloading Acta... Please Wait!", Color.Lime)
                                Try
                                    Dim imageResponse As HttpResponseMessage = Await Client.GetAsync(url)
                                    If imageResponse.StatusCode = Net.HttpStatusCode.OK Then
                                        Dim imageBytes As Byte() = Await imageResponse.Content.ReadAsByteArrayAsync()
                                        File.WriteAllBytes(SaveImagePath, imageBytes)
                                        WriteStatus("Acta successfully saved!", Color.Lime)
                                        csv.AddRow(New Dictionary(Of String, String) From {{"DNI", CurrentDNI}, {"Url", url}, {"File", SaveImagePath}})
                                    Else
                                        WriteStatus("Downloading Acta Failed, Saving Link on log File!", Color.Yellow)
                                        csv.AddRow(New Dictionary(Of String, String) From {{"DNI", CurrentDNI}, {"Url", url}, {"File", "null"}})
                                    End If
                                Catch ex As Exception
                                    WriteStatus("Downloading Acta Failed, Saving Link on log File, Please Use VPN!", Color.Yellow)
                                    csv.AddRow(New Dictionary(Of String, String) From {{"DNI", CurrentDNI}, {"Url", url}, {"File", "null"}})
                                End Try

                            Else
                                    WriteStatus("Acta successfully saved!", Color.Lime)
                                    csv.AddRow(New Dictionary(Of String, String) From {{"DNI", CurrentDNI}, {"Url", url}, {"File", SaveImagePath}})
                                End If
                                csv.SaveToFile(CSVPath)
                            Else
                                WriteLog("Invalid json ---------->>>>>" & Environment.NewLine & Content)
                                WriteStatus("Error: Invalid json data", Color.Red)
                            End If

                            If OnStopDNI = 0 Then
                                currentNumber += 1
                            Else
                                If currentNumber >= OnStopDNI Then
                                    Exit While
                                End If
                            End If
                        ElseIf Response.StatusCode = Net.HttpStatusCode.BadGateway OrElse Response.StatusCode = Net.HttpStatusCode.RequestTimeout OrElse Response.StatusCode = Net.HttpStatusCode.GatewayTimeout Then
                            WriteLog("User Not Found, Respose ---------->>>>>" & Environment.NewLine & Content)
                            WriteStatus("Error: Invalid Response", Color.Yellow)
                            If OnStopDNI = 0 Then
                                currentNumber += 1
                            Else
                                If currentNumber >= OnStopDNI Then
                                    Exit While
                                End If
                            End If
                        Else
                            Console.WriteLine("Error On GET: V-" & CurrentDNI & " Respose Code: " & Response.StatusCode.ToString())
                            WriteLog("Invalid Response ---------->>>>>" & Environment.NewLine & Content)
                            WriteStatus("Error: Invalid Response", Color.Red)
                        End If

                    Else
                        WriteStatus("Error On GET: V-" & String.Format("{0:N0}", currentNumber).Replace(",", ".") & " Code: " & Response.StatusCode.ToString(), Color.Red)
                    End If

                End Using

                Retry += 1

                If Retry >= 4 Then
                    Retry = 0
                    If OnStopDNI = 0 Then
                        currentNumber += 1
                    Else
                        If currentNumber >= OnStopDNI Then
                            Exit While
                        End If
                    End If
                End If

            Catch ex As Exception
            WriteLog("Message ---------->>>>>" & Environment.NewLine & ex.StackTrace)
            WriteStatus("Error: " & ex.Message, Color.Red)
            If OnStopDNI = 0 Then
                currentNumber += 1
            Else
                If currentNumber >= OnStopDNI Then
                    Exit While
                End If
            End If
            End Try



            Application.DoEvents()
        End While

        Me.BeginInvoke(Sub()
                           ResetUI()
                       End Sub)
    End Sub

    Public Function ParseJson(ByVal value As String) As JsonDocument
        Try
            Dim jsonDoc As JsonDocument = JsonDocument.Parse(value)
            Return jsonDoc
        Catch
            Return Nothing
        End Try
    End Function

    Public Function RemoveNonDigitCharacters(S As String) As String
        Return New String(S.Where(Function(x As Char) System.Char.IsDigit(x)).ToArray)
    End Function

    Private Sub Guna2TrackBar1_Scroll(sender As Object, e As ScrollEventArgs) Handles Guna2TrackBar1.Scroll
        SetBoost()
    End Sub

    Private Sub SetBoost()
        Try
            Dim CurrentVal As Integer = Guna2TrackBar1.Value
            Dim Process As Process = Process.GetCurrentProcess
            Select Case CurrentVal
                Case 1
                    Label9.Text = "CPU (Normal)"
                    Process.PriorityClass = ProcessPriorityClass.AboveNormal
                    If PThread IsNot Nothing Then PThread.Thread.Priority = System.Threading.ThreadPriority.Normal
                Case 2
                    Label9.Text = "CPU (High)"
                    Process.PriorityClass = ProcessPriorityClass.High
                    If PThread IsNot Nothing Then PThread.Thread.Priority = System.Threading.ThreadPriority.AboveNormal
                Case 3
                    Label9.Text = "CPU (Extreme)"
                    Process.PriorityClass = ProcessPriorityClass.RealTime
                    If PThread IsNot Nothing Then PThread.Thread.Priority = System.Threading.ThreadPriority.Highest
            End Select
        Catch ex As Exception : End Try
    End Sub


#End Region


End Class
