using System;
using System.Net.Http;
using Serilog;

namespace DataAccess
{
    public class UserService
    {
        private readonly HttpClient _httpClient;

        public UserService()
        {
            _httpClient = new HttpClient();
        }

        public string GetUser(int id)
        {
            try
            {
                var response = _httpClient.GetAsync("https://reqres.in/api/users/" + id).Result;

                Log.Information($"Calling api for user {id}");

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = response.Content.ReadAsStringAsync().Result;
                    return responseBody;
                }
                else
                {
                    Log.Error($"Error: {response.StatusCode} {id}");
                    Console.WriteLine($"Error: {response.StatusCode}");
                    return null;
                }
            }
            catch (HttpRequestException e)
            {
                Log.Error($"Request error: {e.Message}");
                Console.WriteLine($"Request error: {e.Message}");
                return null;
            }
        }
    }

}
