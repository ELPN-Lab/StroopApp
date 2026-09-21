using StroopApp.Services.Exportation;
using StroopApp.Services.Navigation;
using StroopApp.Services.Session;
using StroopApp.Services.Summary;
using StroopApp.Services.Window;
using StroopApp.ViewModels.Experiment.Experimenter.End;
using StroopApp.ViewModels.State;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StroopApp.Views.Experiment.Experimenter
{
    public partial class EndExperimentPage : Page, INavigationAware
    {
        private readonly ExperimentSettingsViewModel _settings;
        private readonly IExportationService _exportationService;
        private readonly IWindowManager _windowManager;
        private readonly IExperimentSessionService _sessionService;
        private readonly IEnumerable<BlockSummaryFormatter> _summaryFormatters;

        INavigationService INavigationAware.NavigationService
        {
            set => Initialize(value);
        }

        public EndExperimentPage(
            ExperimentSettingsViewModel settings, 
            IExportationService exportationService, 
            IWindowManager windowManager,
            IExperimentSessionService sessionService,
            IEnumerable<BlockSummaryFormatter> summaryFormatters)
        {
            InitializeComponent();
            _settings = settings;
            _exportationService = exportationService;
            _windowManager = windowManager;
            _sessionService = sessionService;
            _summaryFormatters = summaryFormatters;
            Unloaded += (s, e) => (DataContext as IDisposable)?.Dispose();
        }

        private void BuildSummaryColumns(IReadOnlyList<BlockSummaryColumn> columns)
        {
            BlockSummaryGrid.Columns.Clear();
            foreach (var col in columns)
            {
                var binding = new Binding(col.BindingPath)
                {
                    Mode = BindingMode.OneWay,
                    TargetNullValue = string.Empty
                };
                if (!string.IsNullOrEmpty(col.StringFormat))
                    binding.StringFormat = col.StringFormat;

                var isGlyph = col.Type == BlockSummaryCellType.BooleanGlyph;
                if (isGlyph)
                {
                    binding.Converter = (IValueConverter)FindResource("BoolToStringConverter");
                    binding.ConverterParameter = "✅|❎";
                }

                BlockSummaryGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = col.Header,
                    Binding = binding,
                    ElementStyle = (Style)FindResource(isGlyph ? "GlyphCellStyle" : "CenteredCellStyle")
                });
            }
        }

        private void Initialize(INavigationService navigationService)
        {
            var viewModel = new EndExperimentPageViewModel(
                _settings, _exportationService, navigationService, _windowManager, _sessionService, _summaryFormatters);
            DataContext = viewModel;
            BuildSummaryColumns(viewModel.SummaryColumns);
        }
    }
}