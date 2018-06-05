Imports System.Text
Imports System.Xml
Imports System.ComponentModel

Public Class Main
    Dim itemId As Integer
    Dim currentProcess As String
    Dim currentWinName As String
    Public newItem_winName As String
    Public newItem_procName As String
    Public newItem_remindEvery As Integer
    Dim Second As Integer
    Dim WindowsTitle() As String
    Dim ProcessName() As String
    Dim StartTime() As String
    Dim ProcessForRemind() As String

    Dim enabled As Boolean
    Private objMutex As System.Threading.Mutex

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load

        objMutex = New System.Threading.Mutex(False, "AuoSave Reminder")
        If objMutex.WaitOne(0, False) = False Then
            objMutex.Close()
            objMutex = Nothing
            MessageBox.Show("Another instance is already running!")
            End
        End If

        NotifyIcon1.Visible = True
        getApplications()
        readFromXml()
        listRunningApps()
        enabled = My.Settings.Enabled

        CheckBox1.Checked = enabled
        Panel1.Enabled = CheckBox1.CheckState

        'Me.Visible = True
        'Dim x As Integer
        'Dim y As Integer
        'x = Screen.PrimaryScreen.WorkingArea.Width
        'y = Screen.PrimaryScreen.WorkingArea.Height - Me.Height

        'Do Until x = Screen.PrimaryScreen.WorkingArea.Width - Me.Width
        '    x = x - 1
        '    Me.Location = New Point(x, y)
        'Loop
    End Sub

    Public Shared Function getApplications() As String
        Dim a As New StringBuilder()
        Dim b As New Process()
        Dim s As String
        Dim e As String
        Dim t As String
        For Each b In Process.GetProcesses(".")
            Try
                If b.MainWindowTitle.Length > 0 Then
                    a.Append("Window Title:  " + b.MainWindowTitle.ToString() + Environment.NewLine)
                    a.Append("Process Name:  " + b.ProcessName.ToString() + Environment.NewLine)
                    a.Append(Environment.NewLine)
                    t &= b.StartTime.ToString + "|"
                    s &= b.MainWindowTitle.ToString + "|"
                    e &= b.ProcessName.ToString + "|"

                End If
            Catch
            End Try
        Next
        Main.WindowsTitle = s.Split("|"c)
        Main.ProcessName = e.Split("|"c)
        Main.ProcessForRemind = e.Split("|"c)
        Main.StartTime = t.Split("|"c)

        Return a.ToString()
    End Function

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        If My.Settings.Enabled = True Then
            Second = Second + 1
            If Second >= 5 Then
                checkIfNeedToRemind()
                Second = 0
            End If
        End If
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If CheckBox1.Checked = False Then
            Panel1.Enabled = False
            enabled = False
        Else
            Panel1.Enabled = True
            enabled = True
        End If
    End Sub

    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        listRunningApps()
    End Sub

    Private Sub saveToXml()
        getApplications()

        xmlData_programs.Tables("Programs").Clear()
        Dim filename As String
        Dim counter As Integer = 0
        Dim bol As Boolean = False
        Dim num As String = "00:10:00"
        For Each filename In WindowsTitle
            counter += 1
            xmlData_programs.Tables("Programs").Rows.Add(New Object() {filename, num, bol, ProcessName(counter - 1), StartTime(counter - 1), "/"})
        Next filename
        xmlData_programs.Tables("Programs").Rows.RemoveAt((WindowsTitle.Count - 1))
        xmlData_programs.Tables("Programs").WriteXml("listOfPrograms.xml", XmlWriteMode.IgnoreSchema)

    End Sub

    Public Sub readFromXml()
        ListView1.Items.Clear()
        ListView1.Columns.Clear()

        ListView1.Columns.Add("Process Name")
        ListView1.Columns.Add("Remind every")
        ListView1.Columns.Add("Activated")
        ListView1.Columns.Add("Windows Name")
        ListView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize)
        Dim ProgramList As XDocument = XDocument.Load("listOfPrograms.xml")
        For Each Program As XElement In ProgramList...<Programs>

            Dim programName As String = Program.Element("Name")
            Dim processName As String = Program.Element("Process")
            Dim remind_every As String = Program.Element("RemindEvery")
            Dim activated As Boolean = Program.Element("Activated")

            Dim Item As New ListViewItem(processName)
            Item.SubItems.Add(remind_every.ToString + " min")
            Item.SubItems.Add(activated)
            Item.SubItems.Add(programName)
            ListView1.Items.Add(Item)

        Next Program

    End Sub

    Private Sub ListView1_MouseDown(sender As Object, e As MouseEventArgs) Handles ListView1.MouseDown
        Dim info As ListViewHitTestInfo = ListView1.HitTest(e.X, e.Y)
        Dim item As ListViewItem = info.Item

        If item IsNot Nothing Then
            itemId = item.Index.ToString

            Dim subitem As ListViewItem.ListViewSubItem = info.SubItem

            currentProcess = item.Text
        End If
    End Sub

    Private Sub RemoveToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RemoveToolStripMenuItem.Click

        Dim node As XElement

        Dim ProgramList As XDocument = XDocument.Load("listOfPrograms.xml")
        For Each Program As XElement In ProgramList...<Programs>

            Dim processName As String = Program.Element("Process")

            If processName = currentProcess Then
                node = Program
            End If

        Next Program

        node.Remove()
        ProgramList.Save("listOfPrograms.xml")
        readFromXml()

    End Sub

    Private Sub EnableDisableToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EnableDisableToolStripMenuItem.Click

        Dim element As XElement

        Dim ProgramList As XDocument = XDocument.Load("listOfPrograms.xml")
        For Each Program As XElement In ProgramList...<Programs>

            Dim processName As String = Program.Element("Process")
            Dim activated As Boolean = Program.Element("Activated")

            If processName = currentProcess Then
                element = Program.Element("Activated")
                element.ReplaceAll(togleBolean(activated).ToString)
            End If

        Next Program

        ProgramList.Save("listOfPrograms.xml")
        readFromXml()
    End Sub

    Private Function togleBolean(input As Boolean)
        Dim output As Boolean

        If input = True Then
            output = False
        Else
            output = True
        End If

        Return output
    End Function

    Private Sub listRunningApps()

        ListView2.Items.Clear()
        ListView2.Columns.Clear()

        ListView2.Columns.Add("Process Name")
        ListView2.Columns.Add("Start Time")
        ListView2.Columns.Add("Windows Title")

        Dim b As New Process()
        For Each b In Process.GetProcesses(".")
            Try
                If b.MainWindowTitle.Length > 0 Then

                    Dim Item As New ListViewItem(b.ProcessName.ToString)
                    Item.SubItems.Add(b.StartTime.ToString)
                    Item.SubItems.Add(b.MainWindowTitle.ToString)

                    ListView2.Items.Add(Item)
                End If
            Catch
            End Try
        Next
        ListView2.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize)
        ListView2.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent)
    End Sub

    Private Sub ListView2_MouseDown(sender As Object, e As MouseEventArgs) Handles ListView2.MouseDown
        Dim info As ListViewHitTestInfo = ListView2.HitTest(e.X, e.Y)
        Dim item As ListViewItem = info.Item

        If item IsNot Nothing Then
            itemId = item.Index.ToString
            Dim subitem As ListViewItem.ListViewSubItem = item.SubItems.Item(2)
            currentProcess = item.Text
            currentWinName = subitem.Text
        End If
    End Sub

    Private Sub CreateReminderToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CreateReminderToolStripMenuItem.Click
        createReminder()
    End Sub

    Private Sub createReminder()
        Dim processes As New StringBuilder()

        Dim ProgramList As XDocument = XDocument.Load("listOfPrograms.xml")
        For Each Program As XElement In ProgramList...<Programs>

            Dim processName As String = Program.Element("Process")

            processes.Append(processName + Environment.NewLine)

        Next Program

        If processes.ToString.Contains(currentProcess) Then
            MsgBox("Podsijetnik već postoji..")
        Else
            NewItem.Show()

            NewItem.TextBox1.Text = currentWinName
            NewItem.TextBox2.Text = currentProcess
        End If
    End Sub

    Public Sub addAppToRemind(_name As String, _process As String, _remind_every As String, _msg As String, _activated As Boolean)

        Dim ProgramList As New XmlDocument()
        ProgramList.Load("listOfPrograms.xml")

        Dim programs As XmlElement = ProgramList.CreateElement("Programs")

        Dim name As XmlElement = ProgramList.CreateElement("Name")
        name.InnerText = _name
        Dim process As XmlElement = ProgramList.CreateElement("Process")
        process.InnerText = _process
        Dim remind_every As XmlElement = ProgramList.CreateElement("RemindEvery")
        remind_every.InnerText = _remind_every
        Dim activated As XmlElement = ProgramList.CreateElement("Activated")
        activated.InnerText = _activated
        Dim message As XmlElement = ProgramList.CreateElement("Message")
        message.InnerText = _msg
        Dim remindet As XmlElement = ProgramList.CreateElement("RemindetAt")
        remindet.InnerText = DateTime.Now.ToString
        programs.AppendChild(name)
        programs.AppendChild(process)
        programs.AppendChild(remind_every)
        programs.AppendChild(message)
        programs.AppendChild(activated)
        programs.AppendChild(remindet)
        ProgramList.DocumentElement.AppendChild(programs)

        ProgramList.Save("listOfPrograms.xml")
    End Sub

    Private Sub checkIfNeedToRemind()
        Dim save As Boolean = False
        Dim ProgramList As XDocument = XDocument.Load("listOfPrograms.xml")

        Dim remindeTime As String
        Dim remindeProc As String
        Dim remindeActive As Boolean
        Dim remindeName As String

        Dim remindeRemindetAt As String
        Dim startTime As String
        Dim element As XElement
        Dim c As Integer = 0
        For Each program As XElement In ProgramList...<Programs>

            remindeTime = program.Element("RemindEvery")
            startTime = program.Element("StartTime")
            remindeProc = program.Element("Process")
            remindeActive = program.Element("Activated")
            remindeName = program.Element("Name")
            remindeRemindetAt = program.Element("RemindetAt")

            If remindeActive Then

                Dim b As New Process()
                For Each b In Process.GetProcesses(".")
                    Try
                        If b.MainWindowTitle.Length > 0 Then
                            If remindeProc = b.ProcessName.ToString Then

                                Dim customMessage As String = program.Element("Message")

                                If customMessage Is String.Empty Then
                                    customMessage = "Sejvaj to što radiš !"
                                End If
                                Dim _elapsedtime As TimeSpan

                                If remindeRemindetAt = "/" Then
                                    _elapsedtime = DateTime.Parse(DateTime.Now).Subtract(DateTime.Parse(b.StartTime.ToString))
                                    If _elapsedtime > TimeSpan.Parse(remindeTime) Then
                                        element = program.Element("RemindetAt")
                                        element.ReplaceAll(DateTime.Now.ToString)
                                        reminde(remindeProc, remindeName, customMessage)
                                        save = True
                                    End If
                                Else
                                    _elapsedtime = DateTime.Parse(DateTime.Now).Subtract(DateTime.Parse(remindeRemindetAt))
                                    If _elapsedtime > TimeSpan.Parse(remindeTime) Then
                                        element = program.Element("RemindetAt")
                                        element.ReplaceAll(DateTime.Now.ToString)
                                        reminde(remindeProc, remindeName, customMessage)
                                        save = True
                                    End If
                                End If
                            End If
                        End If
                    Catch
                    End Try
                Next
            End If
        Next program
        If save Then
            ProgramList.Save("listOfPrograms.xml")
        End If
    End Sub

    Private Sub reminde(process As String, winname As String, message As String)
        NotifyIcon1.BalloonTipTitle = process & " - " & winname
        NotifyIcon1.BalloonTipText = message
        NotifyIcon1.ShowBalloonTip(1000)
    End Sub

    Private Sub RefreshToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RefreshToolStripMenuItem.Click
        readFromXml()
    End Sub

    Private Sub EditToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EditToolStripMenuItem.Click
        Dim message As String
        Dim remindeTime As String
        Dim activated As Boolean
        Dim process As String

        Dim ProgramList As XDocument = XDocument.Load("listOfPrograms.xml")
        For Each Program As XElement In ProgramList...<Programs>

            Dim processName As String = Program.Element("Process")

            If processName = currentProcess Then
                process = processName
                message = Program.Element("Message")
                remindeTime = Program.Element("RemindEvery")
                activated = Program.Element("Activated")

                Edit.proc = processName
                Edit.TextBox3.Text = message
                Edit.TextBox1.Text = remindeTime
                Edit.CheckBox1.Checked = activated
                Edit.Show()
                Exit For
            End If
        Next Program
    End Sub

    Public Sub editProgramForReminde(proc As String, msg As String, active As Boolean, time As String)
        Dim _active As XElement
        Dim _msg As XElement
        Dim _time As XElement

        Dim ProgramList As XDocument = XDocument.Load("listOfPrograms.xml")
        For Each Program As XElement In ProgramList...<Programs>

            Dim processName As String = Program.Element("Process")
            Dim activated As Boolean = Program.Element("Activated")

            If processName = proc Then
                _active = Program.Element("Activated")
                _active.ReplaceAll(togleBolean(activated).ToString)

                _msg = Program.Element("Message")
                _msg.ReplaceAll(msg)

                _time = Program.Element("RemindEvery")
                _time.ReplaceAll(time)
            Else

            End If

        Next Program

        ProgramList.Save("listOfPrograms.xml")
        readFromXml()
    End Sub

    Private Sub ListView1_DoubleClick(sender As Object, e As EventArgs) Handles ListView1.DoubleClick
        Dim message As String
        Dim remindeTime As String
        Dim activated As Boolean
        Dim process As String

        Dim ProgramList As XDocument = XDocument.Load("listOfPrograms.xml")
        For Each Program As XElement In ProgramList...<Programs>

            Dim processName As String = Program.Element("Process")

            If processName = currentProcess Then
                process = processName
                message = Program.Element("Message")
                remindeTime = Program.Element("RemindEvery")
                activated = Program.Element("Activated")

                Edit.proc = processName
                Edit.TextBox3.Text = message
                Edit.TextBox1.Text = remindeTime
                Edit.CheckBox1.Checked = activated
                Edit.Show()
                Exit For
            End If
        Next Program
    End Sub

    Private Sub ListView2_DoubleClick(sender As Object, e As EventArgs) Handles ListView2.DoubleClick
        createReminder()
    End Sub

    Private Sub QuitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles QuitToolStripMenuItem.Click
        My.Settings.Save()
        End
    End Sub

    Private Sub Main_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        My.Settings.Enabled = enabled
        My.Settings.Save()
    End Sub

    Private Sub OpenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenToolStripMenuItem.Click
        Me.Show()
        Me.WindowState = FormWindowState.Normal
    End Sub

    Private Sub Main_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        If Me.WindowState = FormWindowState.Minimized Then
            NotifyIcon1.Visible = True
            NotifyIcon1.ShowBalloonTip(3000, "AutoSave Reminder", "DoubleClick on the Icon to restore the application.", ToolTipIcon.Info)
            Me.Hide()
        End If
    End Sub

    Private Sub NotifyIcon1_DoubleClick(sender As Object, e As EventArgs) Handles NotifyIcon1.DoubleClick
        Me.Show()
        Me.WindowState = FormWindowState.Normal
    End Sub
End Class