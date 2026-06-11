using FusionEdge.Data;
using FusionEdge.Data.DTOs;
using FusionEdge.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace FusionEdge.Components.Services
{
    internal class EmailService : IEmailService
    {
        public async Task<string> SendSuccessEmailAsync(string toEmail, bool isSuccess, string fileName, string folderName, string EmailTemplate)
        {

            var uploadTimeUtc = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC";
            var statusText = isSuccess ? "Published Successfully" : "Not Published";
            var statusColor = isSuccess ? "#008000" : "#ff0000"; 
            var subject = isSuccess ? "File Transfer Successful" : "File Transfer Failed";
            var messageIfNew = EmailTemplate == "Success" ? "Transfer Successful" : EmailTemplate == "Update" ? "Updated Schedule Published Successfully" : EmailTemplate == "Failed" ? "File Transfer Failed" : "New Schedule Uploaded (Action Required)";
            var subHeaderMessage = EmailTemplate == "Success" ? $"{fileName} has been uploaded to:"
                                 : EmailTemplate == "Update" ? $"There is an update to the {fileName}."
                                 : EmailTemplate == "Failed" ? "Transfer failed" : $"A New schedule({fileName}) has been successfully uploaded!:";

            var folderRow = EmailTemplate == "New Schedule"
                                ? $@"
                                     <tr style='background-color:#f2f2f2;'>
                                        <td style='border:1px solid #dddddd;'><strong>Folder</strong></td>
                                        <td style='border:1px solid #dddddd;'>
                                            <a href='' target='_blank' style='color:#1a73e8; text-decoration:none;'>
                                                {folderName}
                                            </a>
                                        </td>
                                    </tr>"
                                : "";

            var htmlBody = $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                          <meta charset='UTF-8' />
                          <meta name='viewport' content='width=device-width, initial-scale=1.0' />
                          <title>Schedule Upload Notification</title>
                        </head>
                        <body style='margin:0; padding:0; background-color:#f4f6f8; font-family: Arial, sans-serif;'>

                          <div style='max-width:600px; margin:20px auto; background:#ffffff; border-radius:10px; overflow:hidden; box-shadow:0 2px 6px rgba(0,0,0,0.08);'>
    
                            <div style='background:#1e1e2e; padding:16px 20px;'>
                              <h2 style='margin:0; color:#ffffff; font-size:18px;'> {messageIfNew}</h2>
                            </div>

                            <div style='padding:20px; color:#333333; font-size:14px; line-height:1.6;'>
      
                              <p style='margin-top:0;'>
                                {subHeaderMessage}
                              </p>

                              <table border='0' cellspacing='0' cellpadding='10' style='border-collapse:collapse; width:100%; border:1px solid #dddddd; font-size:14px;'>
                                {folderRow}

                                <tr>
                                  <td style='border:1px solid #dddddd;'><strong>Published Time</strong></td>
                                  <td style='border:1px solid #dddddd;'>{uploadTimeUtc}</td>
                                </tr>
                                <tr style='background-color:#f9f9f9;'>
                                  <td style='border:1px solid #dddddd;'><strong>Status</strong></td>
                                  <td style='border:1px solid #dddddd;'>
                                    <span style='display:inline-block; color:#ffffff; font-weight:bold; background:{statusColor}; padding:4px 10px; font-size:12px; border-radius:12px;'>
                                      {statusText}
                                    </span>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='border:1px solid #dddddd;'><strong>Logs</strong></td>
                                  <td style='border:1px solid #dddddd;'>
                                    <a href='' target='_blank' style='color:#1a73e8; text-decoration:none; font-weight:bold;'>View Logs</a>
                                  </td>
                                </tr>
                              </table>

                              <p style='margin-top:16px; color:#000;'>
                                <strong>Important:</strong> You must publish this schedule in <strong>Schedule</strong> before it becomes available.
                              </p>

                              <p style='margin-top:16px; color:#666; font-size:12px;'>
                                This is an automated notification — please do not reply.
                              </p>

                            </div>
                          </div>

                        </body>
                        </html>
                        ";
            var message = new MailMessage
            {
                From = new MailAddress("appleshamdra@gmail.com"),
                Subject = isSuccess ? "File Transfer Successful" : "File Transfer Failed",
                Body = isSuccess ? htmlBody : "",
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(
                    "appleshamdra@gmail.com",
                    "ucmktwixzpkfaoli"
                ),
                EnableSsl = true
            };

            await smtp.SendMailAsync(message);

            return "Success email sent";
        }

        public async Task<string> SaveEmailReceiverAsync(EmailReceiverDto dto)
        {
            try
            {
                using var db = new AppDbContext();

                await db.Database.EnsureCreatedAsync();

                var receiver = new EmailReceiver
                {
                    Email = dto.Email,
                    // ProjectId = dto.ProjectId
                };

                db.EmailReceivers.Add(receiver);

                await db.SaveChangesAsync();

                return "Success Saved";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }


        public async Task<int> ReturnAndFetchEmailId(string email)
        {
            try
            {
                using var db = new AppDbContext();
                await db.Database.EnsureCreatedAsync();

                var existing = await db.EmailReceivers
                    .FirstOrDefaultAsync(e => e.Email == email);

                if (existing != null)
                    return existing.Id;

                // NOT FOUND — save it and return new Id
                var receiver = new EmailReceiver { Email = email };
                db.EmailReceivers.Add(receiver);
                await db.SaveChangesAsync();

                return receiver.Id;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<string> SaveEmailNotificationReceiverAsync(EmailNotificationDto dto)
        {
            try
            {
                using var db = new AppDbContext();

                await db.Database.EnsureCreatedAsync();

                //var emailId = await ReturnAndFetchEmailId(dto.EmailId.ToString());

                var emailNotification = new EmailNotification
                {
                    EmailTemplate = dto.EmailTemplate,
                    EmailId = dto.EmailId,
                    ProjectId = dto.ProjectId
                };

                db.EmailNotifications.Add(emailNotification);

                await db.SaveChangesAsync();

                return "Saved Successfully";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
    }

}

