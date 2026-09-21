using StroopApp.Models;
using StroopApp.Services.KeyMapping;

namespace StroopApp.XUnitTests.TestDummies
{
	public class DummyKeyMappingService : IKeyMappingService
	{
		public Task<ExperimentKeyMappings> LoadKeyMappings() => Task.FromResult(new ExperimentKeyMappings());
		public Task SaveKeyMappings(ExperimentKeyMappings mappings) => Task.CompletedTask;
	}
}
