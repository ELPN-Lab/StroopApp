using StroopApp.Services.Window;
using StroopApp.ViewModels.State;

namespace StroopApp.XUnitTests.TestDummies
{
    public class DummyWindowManager : IWindowManager
    {
        public bool ShowCalled;
        public void ShowParticipantWindow(ExperimentSettingsViewModel settings) => ShowCalled = true;
        public void ShowSimonParticipantWindow(ExperimentSettingsViewModel settings) => ShowCalled = true;
        public void CloseParticipantWindow()
        {
        }
        public void CloseSimonParticipantWindow()
        {
        }
    }
}
