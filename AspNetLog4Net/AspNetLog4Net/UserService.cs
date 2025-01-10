using log4net;
using System;
using System.Net.Http;

namespace AspNetLog4Net
{
    public class UserService
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(UserService));

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
                
                if (response.IsSuccessStatusCode)
                {
                    logger.Info($"Calling for user id {id}");
                    var responseBody = response.Content.ReadAsStringAsync().Result;
                    return responseBody;
                }
                else
                {
                    logger.Error($"Error: {response.StatusCode}");
                    Console.WriteLine($"Error: {response.StatusCode}");
                    return null;
                }
            }
            catch (HttpRequestException e)
            {
                logger.Error($"Request error: {e.Message}");
                Console.WriteLine($"Request error: {e.Message}");
                return null;
            }
        }
    }
}