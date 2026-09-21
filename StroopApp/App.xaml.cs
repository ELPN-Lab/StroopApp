using Microsoft.Extensions.DependencyInjection;
using StroopApp.Models;
using StroopApp.Services.Charts;
using StroopApp.Services.Exportation;
using StroopApp.Services.KeyMapping;
using StroopApp.Services.Language;
using StroopApp.Services.Navigation.PageFactory;
using StroopApp.Services.Participant;
using StroopApp.Services.Profile;
using StroopApp.Services.Session;
using StroopApp.Services.Summary;
using StroopApp.Services.Trial;
using StroopApp.Services.Window;
using StroopApp.ViewModels.State;
using StroopApp.Views;
using StroopApp.Views.Configuration;
using StroopApp.Views.Experiment.Experimenter;
using StroopApp.Views.Home;
using System.IO;
using System.Windows;

namespace StroopApp
{
    public partial class App : Application
    {
        /// <summary>
        /// Global service provider for the application's DI container.
        /// </summary>
        internal static IServiceProvider ServiceProvider { get; private set; } = null!;
        internal static IWindowManager WindowManager { get; private set; } = null!;
        internal static ILanguageService LanguageService { get; private set; } = null!;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            LanguageService = ServiceProvider.GetRequiredService<ILanguageService>();
            WindowManager = ServiceProvider.GetRequiredService<IWindowManager>();
            var sessionService = ServiceProvider.GetRequiredService<IExperimentSessionService>();
            var pageFactory = ServiceProvider.GetRequiredService<IPageFactory>();

            var expWin = new ExperimentWindow(pageFactory, LanguageService, sessionService);
            expWin.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var configDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "StroopApp");

            services.AddSingleton(new AppConfiguration { ConfigDirectory = configDir });

            services.AddSingleton<ExperimentChartFactory>();

            services.AddSingleton<ILanguageService, LanguageService>();
            services.AddSingleton<IWindowManager, WindowManager>();

            services.AddSingleton<ExperimentSettings>(sp =>
            {
                var settings = new ExperimentSettings
                {
                    ExperimentContext = new SharedExperimentData()
                };
                return settings;
            });

            services.AddSingleton<ExperimentSettingsViewModel>(sp =>
                new ExperimentSettingsViewModel(sp.GetRequiredService<ExperimentSettings>()));

            services.AddSingleton<IPageFactory, PageFactory>();

            services.AddSingleton<IProfileService, ProfileService>();
            services.AddSingleton<IParticipantService, ParticipantService>();
            services.AddSingleton<IKeyMappingService, KeyMappingService>();
            services.AddSingleton<TrialExportFormatter,StroopTrialExportFormatter>();
            services.AddSingleton<TrialExportFormatter,SimonTrialExportFormatter>();
            services.AddSingleton<IExportationService, ExportationService>();
            services.AddSingleton<BlockSummaryFormatter, StroopBlockSummaryFormatter>();
            services.AddSingleton<BlockSummaryFormatter, SimonBlockSummaryFormatter>();
            services.AddTransient<ITrialGenerationService, TrialGenerationService>();
            services.AddTransient<ISimonTrialGenerationService, SimonTrialGenerationService>();
            services.AddSingleton<IExperimentSessionService, ExperimentSessionService>();
            

            services.AddTransient<HomePage>();
            services.AddTransient<ConfigurationPage>();
            services.AddTransient<SimonConfigurationPage>();
            services.AddTransient<EndExperimentPage>();
            services.AddTransient<ExperimentDashBoardPage>();
        }
    }
}
