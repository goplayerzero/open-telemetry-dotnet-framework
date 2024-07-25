Partial Class _Default
    Inherits System.Web.UI.Page

    Protected Sub btnLogin_Click(sender As Object, e As EventArgs)
        Dim username As String = txtUsername.Text.Trim()
        Dim password As String = txtPassword.Text.Trim()

        ' Replace with your actual validation logic, e.g., database check
        If username = "admin" AndAlso password = "password" Then
            lblMessage.Text = "Login successful!"
            lblMessage.ForeColor = Drawing.Color.Green
            ' Redirect to another page or perform other actions
            Response.Redirect("About.aspx")
        Else
            lblMessage.Text = "Invalid username or password."
        End If
    End Sub
End Class
