Imports System.Diagnostics
Imports System.Net.Http
Imports System.Threading.Tasks
Imports Microsoft.Extensions.Logging
Imports Newtonsoft.Json

Partial Public Class About
    Inherits Page


    Protected Async Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            Await LoadDataAsync()
        End If
    End Sub

    Private Async Function LoadDataAsync() As Task
        Using client As New HttpClient()
            Try
                Dim response As HttpResponseMessage = Await client.GetAsync("https://reqres.in/api/users")
                response.EnsureSuccessStatusCode()
                Dim jsonString As String = Await response.Content.ReadAsStringAsync()
                Dim data As ApiResponse = JsonConvert.DeserializeObject(Of ApiResponse)(jsonString)

                If data IsNot Nothing AndAlso data.Data IsNot Nothing Then
                    GridView1.DataSource = data.Data
                    GridView1.DataBind()
                End If
            Catch ex As Exception
                ' Handle errors (e.g., log them)

            End Try
        End Using
    End Function

    ' Class to deserialize the API response
    Public Class ApiResponse
        Public Property Data As List(Of User)
    End Class

    Public Class User
        Public Property Id As Integer
        Public Property Email As String
        Public Property First_name As String
        Public Property Last_name As String
        Public Property Avatar As String
    End Class

    Protected Async Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Await LoadUserDataAsync()
    End Sub

    Private Async Function LoadUserDataAsync() As Task
        Using client As New HttpClient()
            Try
                Dim response As HttpResponseMessage = Await client.GetAsync("https://reqres.in/api/users/" + TextBox1.Text)
                response.EnsureSuccessStatusCode()
                Dim jsonString As String = Await response.Content.ReadAsStringAsync()
                Dim data As ApiResponse = JsonConvert.DeserializeObject(Of ApiResponse)(jsonString)

                If data IsNot Nothing Then
                    Debug.WriteLine("user data {data}")
                End If
            Catch ex As Exception
                ' Handle errors (e.g., log them)

            End Try
        End Using
    End Function
End Class