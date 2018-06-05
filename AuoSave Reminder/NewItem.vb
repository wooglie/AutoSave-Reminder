Public Class NewItem
    Private Sub NewItem_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Main.Enabled = False
    End Sub

    Private Sub NewItem_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        Main.Enabled = True
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim str As String
        If TextBox4.Text = "" Then
            str = "Sejvaj to što radiš !"
        Else
            str = TextBox4.Text.ToString
        End If
        If IsNumeric(TextBox3.Text) Then
            If Not TextBox1.Text = "" And Not TextBox3.Text = "" Then
                Dim remindEvery As String = DateTime.Parse("00:00:00").AddMinutes(TextBox3.Text).ToLongTimeString
                Main.addAppToRemind(TextBox1.Text, TextBox2.Text, remindEvery, str, True)
                Me.Close()
            Else
                If TextBox1.Text = "" Then
                    MsgBox("Windows name ne može biti prazno!")
                Else
                    MsgBox("Unesi period svakih koliko minute će te podsijetiti!")
                End If
            End If
        Else
            MsgBox("Minute nisu valjane..")

        End If

        Main.readFromXml()


    End Sub
End Class