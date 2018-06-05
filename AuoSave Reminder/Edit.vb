Public Class Edit
    Dim message As String
    Public proc As String
    Private Sub Edit_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        Main.Enabled = True
    End Sub

    Private Sub Edit_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Main.Enabled = False
        message = TextBox3.Text
        Me.Text = "Edit - " & proc
    End Sub

    Private Sub MaskedTextBox1_MouseDown(sender As Object, e As MouseEventArgs) Handles MaskedTextBox1.MouseDown
        MaskedTextBox1.Select(0, 0)
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.Close()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim time As String
        If MaskedTextBox1.MaskCompleted = True Then
            time = MaskedTextBox1.Text
        Else
            time = TextBox1.Text
        End If

        If Not TextBox3.Text = message Then
            message = TextBox3.Text
        End If

        Main.editProgramForReminde(proc, message, CheckBox1.Checked, time)
        Me.Close()
    End Sub
End Class