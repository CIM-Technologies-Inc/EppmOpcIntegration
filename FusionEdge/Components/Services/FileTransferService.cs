using FusionEdge.Data;
using FusionEdge.Data.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FusionEdge.Components.Services
{
    internal class FileTransferService : IFileTransferService
    {
        private readonly IEmailService _emailService;

        public FileTransferService(IEmailService emailService)
        {
            _emailService = emailService;
        }

        private string CleanPathName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            return name.Trim();
        }

        public async Task<string> ExportAndSave(string Workspace, long projectId, SourceConfiguration config, string projectName, int UserId)
        {
            string fileName = "";
            string fullPath = "";
            projectName = projectName.Replace("_", " ").Trim();

            using var db = new AppDbContext();

            var emailNotif = await db.EmailNotifications
                .Where(x => x.ProjectId == projectId.ToString())
                .ToListAsync();

            try
            {
                var baseUrl = config.Domain;

                var raw = $"{config.Username}:{config.PlainPass}";

                var authToken = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(raw)
                );

                // COOKIE SESSION
                var cookieContainer = new CookieContainer();

                var handler = new HttpClientHandler
                {
                    CookieContainer = cookieContainer,
                    ServerCertificateCustomValidationCallback =
                        (msg, cert, chain, errors) => true
                };

                using var client = new HttpClient(handler);

                client.DefaultRequestHeaders.Clear();

                client.DefaultRequestHeaders.Add(
                    "authToken",
                    authToken
                );

                // LOGIN
                var loginResponse = await client.PostAsync(
                    $"{baseUrl}/p6ws/restapi/login?DatabaseName=CIMEPPM",
                    null
                );

                if (!loginResponse.IsSuccessStatusCode)
                {
                    throw new Exception("Login failed.");
                }

                // EXPORT REQUEST BODY
                var requestBody = JsonSerializer.Serialize(new
                {
                    ProjectObjectId = new[] { projectId },
                    FileType = "XER"
                });

                // EXPORT REQUEST
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"{baseUrl}/p6ws/restapi/export/exportXERProject"
                );

                request.Content = new StringContent(
                    requestBody,
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();

                    throw new Exception(
                        $"Export failed: {err}"
                    );
                }

                // FILE CONTENT
                var fileBytes =
                    await response.Content.ReadAsByteArrayAsync();

                // FILE EXTENSION
                var contentType =
                    response.Content.Headers.ContentType?.MediaType ?? "";

                var extension =
                    contentType.Contains("zip")
                        ? ".zip"
                        : ".xer";

                // ROOT FOLDER
                var rootFolder =
                    @"C:\Users\apple\DC\ACCDocs\ACC - CIM Techsupport\20230627 - ACC Demo Project\Project Files\Schedule tool files\";

                // NEW PROJECTS FOLDER
                var newProjectFolder =
                    Path.Combine(rootFolder, "newProjectFolder");

                // ENSURE FOLDERS EXIST
                Directory.CreateDirectory(rootFolder);
                Directory.CreateDirectory(newProjectFolder);


                // EXISTING PROJECT PATH
                var existingProjectPath =
                    Path.Combine(
                        rootFolder,
                        CleanPathName(projectName)
                    );

                // NEW PROJECT PATH
                var newProjectPath =
                    Path.Combine(
                        newProjectFolder,
                        projectName
                    );

                string targetFolder;

                bool exists = await db.EmailNotifications
                            .AnyAsync(x => x.ProjectId == projectId.ToString());

                // CHECK IF PROJECT EXISTS IN ROOT
                if (Directory.Exists(existingProjectPath))
                {
                  
                    if (!exists)
                    {
                        return "updateEmail";
                    }

                    // SAVE TO EXISTING PROJECT FOLDER
                    targetFolder = existingProjectPath;
                }
                else
                {
                    if (!Directory.Exists(newProjectPath)) 
                    {
                        if (!exists)
                        {
                            return "setEmail";
                        }
                    }
                                       
                    // CREATE NEW PROJECT FOLDER
                    targetFolder = newProjectPath;

                    Directory.CreateDirectory(targetFolder);
                }

                fileName = $"{projectName}{extension}";

                fullPath =
                    Path.Combine(targetFolder, fileName);


                // SAVE NEW FILE
                await File.WriteAllBytesAsync(
                    fullPath,
                    fileBytes
                );

                if (emailNotif.Any())
                {

                    foreach (var r in emailNotif)
                    {
                        var receiverEmails = await db.EmailReceivers
                            .FirstOrDefaultAsync(x => x.Id == r.EmailId);

                        await _emailService.SendSuccessEmailAsync(
                            receiverEmails.Email,
                            true,
                            fileName,
                            projectName,
                            fullPath,
                            r.EmailTemplate
                        );
                    }
                }

                return fullPath;
            }
            catch (Exception ex)
            {
                // FAILED EMAIL
                if (emailNotif.Any())
                {

                    foreach (var r in emailNotif)
                    {
                        var receiverEmails = await db.EmailReceivers
                            .FirstOrDefaultAsync(x => x.Id == r.EmailId);

                        await _emailService.SendSuccessEmailAsync(
                            receiverEmails.Email,
                            false,
                            fileName ?? "Unknown",
                            projectName,
                            fullPath,
                            "Failed"
                        );
                    }
                }

                throw new Exception(ex.Message);
            }
        }
        public async Task<string> MoveFileAsync(string sourcePath,string destinationFolder)
        {
            string destinationPath = "";
            string fileName = "";

            try
            {
                fileName = Path.GetFileName(sourcePath);

                if (!File.Exists(sourcePath))
                {
                    throw new FileNotFoundException(
                        "Source file not found."
                    );
                }

                Directory.CreateDirectory(destinationFolder);

                destinationPath = Path.Combine(
                    destinationFolder,
                    fileName
                );

                await Task.Run(() =>
                    File.Copy(sourcePath, destinationPath, true)
                );

                //await _emailService.SendSuccessEmailAsync(
                //    "appleshamdra@gmail.com",
                //    true,
                //    fileName,
                //    destinationFolder,
                //    true
                //);

                return destinationPath;
            }
            catch (Exception ex)
            {
                //await _emailService.SendSuccessEmailAsync(
                //    "appleshamdra@gmail.com",
                //    false,
                //    fileName ?? "Unknown",
                //    destinationFolder,
                //    false
                //);

                throw;
            }
        }
    }
}
