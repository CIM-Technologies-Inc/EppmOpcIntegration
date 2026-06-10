using FusionEdge.Data;
using FusionEdge.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FusionEdge.Components.Services
{
    public class ScheduleRunnerService : IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UserService _userService;
        private Timer? _timer;
        private bool _isRunning;

        public event Action<ScheduleRunResult>? OnScheduleResult;
        public ScheduleRunnerService(IServiceProvider serviceProvider, UserService userService)
        {
            _serviceProvider = serviceProvider;
            _userService = userService;
        }

        public void Start()
        {
            _timer = new Timer(
                async _ => await SafeCheckSchedule(),
                null,
                TimeSpan.Zero,
                TimeSpan.FromMinutes(1));
        }

        private async Task SafeCheckSchedule()
        {
            if (_isRunning)
                return;
            try
            {
                _isRunning = true;
                await CheckSchedule();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Scheduler Error: {ex}");
            }
            finally
            {
                _isRunning = false;
            }
        }

        private async Task CheckSchedule()
        {
            using var scope = _serviceProvider.CreateScope();

            var fileTransfer = scope.ServiceProvider.GetRequiredService<IFileTransferService>();

            using var db = new AppDbContext();

            var schedules = await db.ScheduleSettings
                .Where(x =>
                    !x.IsExecuted &&
                    x.ScheduleType != "Never")
                .ToListAsync();

            if (!schedules.Any()) {
                return;
            }
            
            //var config = await db.SourceConfigurations
            //    .FirstOrDefaultAsync(x => x.UserId == schedule.UserId);

            //if (config == null)
            //    return;

            foreach (var schedule in schedules)
            {
                try
                {
                    bool shouldRun = false;

                    var config = await db.SourceConfigurations
                        .FirstOrDefaultAsync(x => x.UserId == schedule.UserId);

                    if (config == null) {
                        return;
                    }
                        

                    switch (schedule.ScheduleType)
                    {
                        case "Custom":

                            shouldRun = schedule.DateTimePublish <= DateTime.Now;
                            break;

                        case "Daily":

                            if (DateTime.Now.TimeOfDay < schedule.Time)
                                break;

                            if (schedule.LastExecuted?.Date == DateTime.Today)
                                break;

                            shouldRun = true;
                            break;

                        case "Weekly":

                            if (string.IsNullOrWhiteSpace(schedule.Days))
                                break;

                            var selectedDays = schedule.Days
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => x.Trim())
                                .ToList();

                            var today = DateTime.Now.DayOfWeek.ToString();

                            if (!selectedDays.Contains(today))
                                break;

                            var scheduledTime = schedule.Time;

                            if (DateTime.Now.TimeOfDay < scheduledTime)
                                break;

                            if (schedule.LastExecuted?.Date == DateTime.Today)
                                break;

                            shouldRun = true;
                            break;
                    }
                    
                    if (!shouldRun)
                        continue;

                    long projectId = long.Parse(schedule.Project);


                    var savePath = await fileTransfer.ExportAndSave(projectId, config, schedule.Projectname);

                    OnScheduleResult?.Invoke(new ScheduleRunResult
                    {
                        ScheduleId = schedule.UserId,
                        Project = schedule.Project,
                        Success = true,
                        Message = $"Project exported successfully:\n{savePath}"
                    });

                    schedule.LastExecuted = DateTime.Now;
                    
                    if (schedule.ScheduleType == "Custom")
                    {
                        schedule.IsExecuted = true;
                    }

                }
                catch (Exception ex)
                {
                    OnScheduleResult?.Invoke(new ScheduleRunResult
                    {
                        ScheduleId = schedule.Id,
                        Success = false,
                        Message = ex.Message
                    });
                }
            }

            await db.SaveChangesAsync();
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}