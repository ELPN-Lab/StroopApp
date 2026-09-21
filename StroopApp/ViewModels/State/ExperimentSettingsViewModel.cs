using CommunityToolkit.Mvvm.ComponentModel;
using StroopApp.Core;
using StroopApp.Models;

namespace StroopApp.ViewModels.State
{
    public partial class ExperimentSettingsViewModel : ViewModelBase
    {
        private readonly ExperimentSettings _model;

        public SharedExperimentDataViewModel ExperimentContext { get; }

        [ObservableProperty]
        private int _block;

        [ObservableProperty]
        private Participant _participant = null!;

        [ObservableProperty]
        private ExperimentProfile _currentProfile = null!;

        [ObservableProperty]
        private ExperimentKeyMappings _keyMappings = null!;

        [ObservableProperty]
        private string _exportFolderPath = string.Empty;

        public ExperimentSettingsViewModel(ExperimentSettings model)
        {
            _model = model;
            ExperimentContext = new SharedExperimentDataViewModel(model.ExperimentContext);

            _block = model.Block;
            _participant = model.Participant;
            _currentProfile = model.CurrentProfile;
            _keyMappings = model.KeyMappings;
            _exportFolderPath = model.ExportFolderPath;
        }

        partial void OnBlockChanged(int value) => _model.Block = value;
        partial void OnParticipantChanged(Participant value) => _model.Participant = value;
        partial void OnCurrentProfileChanged(ExperimentProfile value) => _model.CurrentProfile = value;
        partial void OnKeyMappingsChanged(ExperimentKeyMappings value) => _model.KeyMappings = value;
        partial void OnExportFolderPathChanged(string value) => _model.ExportFolderPath = value;

        public void Reset()
        {
            _model.Reset();
            ExperimentContext.RefreshFromModel();

            Block = _model.Block;
            Participant = _model.Participant;
            CurrentProfile = _model.CurrentProfile;
            KeyMappings = _model.KeyMappings;
            ExportFolderPath = _model.ExportFolderPath;
        }
    }
}