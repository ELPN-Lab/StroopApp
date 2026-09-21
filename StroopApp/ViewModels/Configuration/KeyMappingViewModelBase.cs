using StroopApp.Core;
using StroopApp.Models;
using StroopApp.Resources;
using StroopApp.Services.KeyMapping;

namespace StroopApp.ViewModels.Configuration
{
    public abstract partial class KeyMappingViewModelBase : ViewModelBase
    {
        protected readonly IKeyMappingService _keyMappingService;
        protected ExperimentKeyMappings _fullKeyMappings = new();

        protected KeyMappingViewModelBase(IKeyMappingService keyMappingService)
        {
            _keyMappingService = keyMappingService;
        }

        protected abstract void ApplyLoaderMappings(ExperimentKeyMappings fullKeyMappings);
        protected abstract void PersistMappings(ExperimentKeyMappings fullKeyMappings);

        protected async Task LoadAsync()
        {
            try
            {
                _fullKeyMappings = await _keyMappingService.LoadKeyMappings();
                ApplyLoaderMappings(_fullKeyMappings);
            }
            catch (Exception exception)
            {
                await ShowErrorDialogAsync($"{Strings.Error_Title}: {exception.Message}");
            }
        }

        protected async Task SaveAsync()
        {
            try
            {
                PersistMappings(_fullKeyMappings);
                await _keyMappingService.SaveKeyMappings(_fullKeyMappings);
            }
            catch (Exception exception)
            {
                await ShowErrorDialogAsync($"{Strings.Error_Title}: {exception.Message}");
            }
        }
    }
}
