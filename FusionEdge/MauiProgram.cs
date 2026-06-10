using FusionEdge.Components.Services;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;

namespace FusionEdge
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            //framework Services
            builder.Services.AddMudServices();

            //Custom Service
            builder.Services.AddSingleton<IFileTransferService, FileTransferService>();
            builder.Services.AddSingleton<IEmailService, EmailService>();
            builder.Services.AddSingleton<IWorkspaceService, WorkspaceService>();
            builder.Services.AddSingleton<SchedulePublishService>();
            builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
            builder.Services.AddSingleton<UserService>();
            builder.Services.AddSingleton<ScheduleRunnerService>();
            builder.Services.AddSingleton<SessionService>();
            builder.Services.AddSingleton<SchedulePublishService>();
            builder.Services.AddSingleton<ISourceConfigurationService, SourceConfigurationService>();
            builder.Services.AddSingleton<IOraclePrimaveraCloudService, OraclePrimaveraCloudService>();


            SQLitePCL.Batteries_V2.Init();

            return builder.Build();
        }
    }
}
