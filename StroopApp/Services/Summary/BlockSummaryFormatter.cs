using StroopApp.Models;
using StroopApp.Services.Language;

namespace StroopApp.Services.Summary
{
    public abstract class BlockSummaryFormatter
    {
        protected readonly ILanguageService LanguageService;
        protected BlockSummaryFormatter(ILanguageService languageService)
            => LanguageService = languageService;

        public abstract TaskType TaskType { get; }
        public abstract List<BlockSummaryColumn> GetColumns();
        protected string GetString(string key) => LanguageService.GetLocalizedString(key);
        protected string ProfilePath(string member) => $"{nameof(Block.Profile)}.{member}";
        protected IEnumerable<BlockSummaryColumn> GetCommonColumns()

            =>
            [
                new BlockSummaryColumn(GetString("Header_ExperimentProfile"),
                    ProfilePath(nameof(ExperimentProfile.ProfileName))),
                new BlockSummaryColumn(GetString("Header_BlockNumber"), nameof(Block.BlockNumber))
                    { StringFormat = $"{GetString("Header_BlockNumber")} {{0}}" },
                new BlockSummaryColumn(GetString("Header_TrialsPerBlock"), nameof(Block.TrialsPerBlock)),
                new BlockSummaryColumn(GetString("Header_Accuracy"), nameof(Block.Accuracy))
                    { StringFormat = "{0:F2}%" },
                new BlockSummaryColumn(GetString("Header_MeanResponseTime"), nameof(Block.ResponseTimeMean))
                    { StringFormat = "{0:F2} ms" },
            ];


    }
}
