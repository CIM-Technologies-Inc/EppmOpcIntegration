using FusionEdge.Data;
using FusionEdge.Data.DTOs;
using FusionEdge.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FusionEdge.Components.Services
{
    internal class AuthenticationService : IAuthenticationService
    {
        private readonly HttpClient _httpClient;
        private bool _isAuthenticated = false;
        private string _lastUsername = string.Empty;
        private string _lastPassword = string.Empty;
        private readonly string _baseUrl = "http://192.168.8.128:8206";

        public AuthenticationService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<bool> RegisterAsync(CreateUserDto dto)
        {
            try
            {
                using var db = new AppDbContext();
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();

                var exists = await db.User.AnyAsync(u => u.Email == dto.Email);
                if (exists) return false;

                var user = new User
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    CreatedAt = DateTime.UtcNow
                };

                db.User.Add(user);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<User?> Login(string username, string password)
        {
            try
            {
                using var db = new AppDbContext();
                var user = await db.User.FirstOrDefaultAsync(u => u.Email == username);
                if (user == null) return null;
                return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash) ? user : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<bool> AuthenticateCredential(string username, string password)
        {
            try
            {
                if (_isAuthenticated && _lastUsername == username && _lastPassword == password)
                    return true;

                var raw = $"{username}:{password}";
                var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authToken", authToken);

                var response = await _httpClient.PostAsync(
                    $"{_baseUrl}/p6ws/restapi/login?DatabaseName=CIMEPPM", null
                );

                if (response.IsSuccessStatusCode)
                {
                    _isAuthenticated = true;
                    _lastUsername = username;
                    _lastPassword = password;
                    return true;
                }
                else
                {
                    _isAuthenticated = false;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _isAuthenticated = false;
                return false;
            }
        }

        public async Task<bool> SaveSourceConfiguration(SourceSettingDto dto)
        {
            try
            {
                using var db = new AppDbContext();
                await db.Database.EnsureCreatedAsync();

                var existing = await db.SourceConfigurations
                    .FirstOrDefaultAsync(s => s.UserId == dto.UserId);

                if (existing != null)
                {
                    existing.SelectedSource = dto.SelectedSource;
                    existing.Domain = dto.Domain ?? string.Empty;
                    existing.Username = dto.Username;
                    existing.PlainPass = dto.PlainPass;
                    existing.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                    db.SourceConfigurations.Update(existing);
                }
                else
                {
                    db.SourceConfigurations.Add(new SourceConfiguration
                    {
                        UserId = dto.UserId,
                        SelectedSource = dto.SelectedSource,
                        Domain = dto.Domain ?? string.Empty,
                        Username = dto.Username,
                        PlainPass = dto.PlainPass,
                        Password = BCrypt.Net.BCrypt.HashPassword(dto.Password)
                    });
                }

                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<List<string>> GetProjectEmails(string projectId)
        {
            var emails = new List<string>();

            if (!_isAuthenticated)
            {
                Console.WriteLine("Not authenticated. Please authenticate first.");
                return emails;
            }

            try
            {
                // HARDCODED FOR TESTING — replace with projectId when confirmed working
                var testProjectId = projectId;

                Console.WriteLine($"[DEBUG] GetProjectEmails — testing with projectId = {testProjectId}");

                var resourceIds = await GetResourceIdsByProject(testProjectId);

                Console.WriteLine($"[DEBUG] Found {resourceIds.Count} resource(s): {string.Join(", ", resourceIds)}");

                foreach (var resourceId in resourceIds)
                {
                    var email = await GetResourceEmail(resourceId);
                    if (!string.IsNullOrEmpty(email) && !emails.Contains(email))
                        emails.Add(email);
                }

                Console.WriteLine($"[DEBUG] Collected emails: {string.Join(", ", emails)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching project emails: {ex.Message}");
            }

            return emails;
        }

        private async Task<HttpClient> BuildEppmClientAsync()
        {
            var authToken = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_lastUsername}:{_lastPassword}")
            );

            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };

            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.TryAddWithoutValidation("authToken", authToken);

            await client.PostAsync(
                $"{_baseUrl}/p6ws/restapi/login?DatabaseName=CIMEPPM", null
            );

            return client;
        }

        private async Task<List<string>> GetResourceIdsByProject(string projectId)
        {
            var resourceIds = new List<string>();
            try
            {
                var client = await BuildEppmClientAsync();

                var url = $"{_baseUrl}/p6ws/restapi/projectResource" +
                          $"?Filter=ProjectObjectId:eq:{projectId}" +
                          $"&Fields=ProjectObjectId,ResourceObjectId";

                Console.WriteLine($"[DEBUG] GetResourceIdsByProject url: {url}");

                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[DEBUG] GetResourceIdsByProject response: {content}");

                if (response.IsSuccessStatusCode)
                {
                    var items = JsonSerializer.Deserialize<List<JsonElement>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (items != null)
                        foreach (var item in items)
                            if (item.TryGetProperty("ResourceObjectId", out var idProp))
                            {
                                var id = idProp.GetString();
                                if (!string.IsNullOrEmpty(id) && !resourceIds.Contains(id))
                                    resourceIds.Add(id);
                            }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] GetResourceIdsByProject error: {ex.Message}");
            }
            return resourceIds;
        }

        private async Task<string> GetResourceEmail(string resourceId)
        {
            try
            {
                var client = await BuildEppmClientAsync();

                var url = $"{_baseUrl}/p6ws/restapi/resource" +
                          $"?Filter=ObjectId:eq:{resourceId}" +
                          $"&Fields=ObjectId,Name,EmailAddress";

                Console.WriteLine($"[DEBUG] GetResourceEmail url: {url}");

                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[DEBUG] GetResourceEmail response: {content}");

                if (response.IsSuccessStatusCode)
                {
                    var resources = JsonSerializer.Deserialize<List<JsonElement>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (resources != null && resources.Count > 0 &&
                        resources[0].TryGetProperty("EmailAddress", out var emailProp))
                        return emailProp.GetString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] GetResourceEmail error: {ex.Message}");
            }
            return null;
        }
    }
}