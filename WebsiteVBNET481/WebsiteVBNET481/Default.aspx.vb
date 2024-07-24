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
            currentActivity.TraceStateString = "userid=1234"
            System.Diagnostics.Debug.WriteLine("CurrentActivity")

        Else
            Global_asax.Logger.LogCritical("Current Activity not found.")
            Global_asax.Logger.LogInformation("setting trace state string")
            Global_asax.Logger.LogCritical("Just an error from vb.net")

        End If


    End Sub
End Class