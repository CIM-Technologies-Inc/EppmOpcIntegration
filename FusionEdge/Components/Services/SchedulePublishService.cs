using FusionEdge.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionEdge.Components.Services
{
    internal class SchedulePublishService
    {
        private readonly IFileTransferService _fileTransferService;
        public SchedulePublishService(IFileTransferService fileTransferService)
        {
            _fileTransferService = fileTransferService;
        }

        public async Task<bool> TriggerSchedulePublish()
        {

            var sourceFile = @"C:\Users\MaechaelGregoreElchi\source\repos\FusionEdge\FusionEdge\Resources\Files\xer\A.test";

            var destination = @"C:\Users\MaechaelGregoreElchi\DC\ACCDocs\ACC - CIM Techsupport\20230627 - ACC Demo Project\Project Files\Schedule tool files\newProjectFolder";

            using var db = new AppDbContext();

            await db.Database.EnsureCreatedAsync();

            var latestSchedule = await db.ScheduleSettings.OrderByDescending(s => s.Id).FirstOrDefaultAsync();

            if (latestSchedule == null)
                return false;

            var timeDiff = DateTime.Now - latestSchedule.DateTimePublish;

            if (timeDiff.TotalMinutes <= 30 && timeDiff.TotalMinutes >= 0)
            {
                await _fileTransferService.MoveFileAsync(sourceFile, destination);
               
            }

            return false; 

        }

    }
}
