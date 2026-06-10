using FusionEdge.Data;
using FusionEdge.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FusionEdge.Components.Services
{
    internal class OraclePrimaveraCloudService : IOraclePrimaveraCloudService
    {

        private readonly HttpClient _httpClient;
        public OraclePrimaveraCloudService()
        {
            _httpClient = new HttpClient();
        }
        public async Task<bool> AuthenticateOPCCredentials(string username, string password)
        {
            try
            {
                var serverUrl = "https://primavera-au1.oraclecloud.com";

                var authToken = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"{username}:{password}")
                );

                _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Basic", authToken);

                var response = await _httpClient.PostAsync(
                    $"{serverUrl}/primediscovery/apitoken/request?scope=http://primavera-au1.oraclecloud.com/api",
                    null
                );

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();

                    using JsonDocument doc = JsonDocument.Parse(result);
                    var token = doc.RootElement.GetProperty("accessToken").GetString();
                    var hashPasword = BCrypt.Net.BCrypt.HashPassword(password);

                    using var db = new AppDbContext();

                    // await db.Database.EnsureDeletedAsync();


                    if (token != null)
                    {
                        await db.Database.EnsureCreatedAsync();

                        var userData = new User
                        {
                            Name = username,
                            Email = username,
                            PasswordHash = hashPasword,
                            // AccessToken = token,

                        };

                        var savingDataStatus = db.User.Add(userData);

                        await db.SaveChangesAsync();

                        return true;
                    }
                    return false;

                }

                return false;


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
