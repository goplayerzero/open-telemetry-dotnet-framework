Imports Microsoft.Extensions.Logging

Public Class _Default
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        If Global_asax.Logger IsNot Nothing Then
            Global_asax.Logger.LogInformation("Page_Load: Default.aspx loaded")
            Global_asax.Logger.LogError("LogError")
            Global_asax.Logger.LogCritical("LogCritical")
        End If
    End Sub

    Protected Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Label1.Text = "Button Clicked!"
        Global_asax.Logger.LogCritical("Button Click error")
        ' Example of accessing the current activity
        Dim currentActivity = Activity.Current
        If currentActivity IsNot Nothing Then
            ' Use the current activity
            System.Diagnostics.Debug.WriteLine("CurrentActivity")

        Else
            Global_asax.Logger.LogCritical("Current Activity not found.")
            Global_asax.Logger.LogInformation("setting trace state string")
            Global_asax.Logger.LogCritical("Just an error from vb.net")

        End If


    End Sub

    Protected Sub btnLogin_Click(ByVal sender As Object, ByVal e As EventArgs)
        Dim username As String = txtUsername.Text
        Dim password As String = txtPassword.Text

        Dim currentActivity = Activity.Current

        Debug.WriteLine($"currentActivity {currentActivity.TraceId}")

        ' Here you would typically validate the credentials against a database or other data source
        If username = "admin" AndAlso password = "password" Then
            ' Authentication successful
            Response.Redirect("About.aspx")
        Else
            ' Authentication failed
            Label1.Text = "Invalid username or password."
            Global_asax.Logger.LogCritical($"Invalid username or password. {username}")
        End If
    End Sub
End Class