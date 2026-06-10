using FusionEdge.Data.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionEdge.Components.Services
{
    internal interface IEmailService
    {
        Task<string> SendSuccessEmailAsync(string toEmail, bool isSuccess, string fileName, string folderName, bool isNew);

        Task<string> SaveEmailReceiverAsync(EmailReceiverDto dto);

    }
}
