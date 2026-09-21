using StroopApp.Services.Session;
using StroopApp.Services.Trial;

namespace StroopApp.XUnitTests.TestDummies
{
    internal class DummyExperimentSessionService : IExperimentSessionService
    {
        public void AbortSession()
        {
            throw new NotImplementedException();
        }

        public void CompleteBlock()
        {
            throw new NotImplementedException();
        }

        public void PrepareNextBlock()
        {
            throw new NotImplementedException();
        }

        public void ResetForNewExperiment()
        {
            throw new NotImplementedException();
        }

        public void StartBlock(ITrialGenerationService trialGenerationService)
        {
            throw new NotImplementedException();
        }
    }
}
