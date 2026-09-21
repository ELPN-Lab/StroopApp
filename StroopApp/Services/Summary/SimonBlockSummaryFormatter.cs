using StroopApp.Models;
using StroopApp.Models.Simon;
using StroopApp.Services.Language;

namespace StroopApp.Services.Summary
{
    public class SimonBlockSummaryFormatter : BlockSummaryFormatter
    {
        public SimonBlockSummaryFormatter(ILanguageService languageService) : base(languageService) { }
        public override TaskType TaskType => TaskType.Simon;
        public override List<BlockSummaryColumn> GetColumns()
        {
            var cols = GetCommonColumns().ToList();
            cols.Add(new(GetString("Header_Congruence_Percent"), ProfilePath(nameof(SimonProfile.CongruencePercent))));
            cols.Add(new(GetString("Header_ReversedMappingPercent"), ProfilePath(nameof(SimonProfile.ReversedMappingPercent))));
            return cols;
        }

    }
}
