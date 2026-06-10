using FusionEdge.Data;
using FusionEdge.Data.DTOs;
using FusionEdge.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
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
                {
                    return true;
                }

                var baseUrl = "http://192.168.8.128:8206";

                var raw = $"{username}:{password}";

                var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));

                _httpClient.DefaultRequestHeaders.Clear();

                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authToken", authToken);

                var response = await _httpClient.PostAsync(
                    $"{baseUrl}/p6ws/restapi/login?DatabaseName=CIMEPPM", null
                );

                var result = await response.Content.ReadAsStringAsync();

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

                // Check if config already exists for this user
                var existing = await db.SourceConfigurations
                    .FirstOrDefaultAsync(s => s.UserId == dto.UserId);

                if (existing != null)
                {
                    // Update existing
                    existing.SelectedSource = dto.SelectedSource;
                    existing.Domain = dto.Domain ?? string.Empty;
                    existing.Username = dto.Username;
                    existing.PlainPass = dto.PlainPass;
                    existing.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                    db.SourceConfigurations.Update(existing);
                }
                else
                {
                    // Insert new
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
    }
}
