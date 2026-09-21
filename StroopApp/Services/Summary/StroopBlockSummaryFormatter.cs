using StroopApp.Models;
using StroopApp.Services.Language;

namespace StroopApp.Services.Summary
{
    public class StroopBlockSummaryFormatter : BlockSummaryFormatter
    {
        public StroopBlockSummaryFormatter(ILanguageService languageService): base(languageService) { }
        public override TaskType TaskType => TaskType.Stroop;
        public override List<BlockSummaryColumn> GetColumns()
        {
            var cols = GetCommonColumns().ToList();
            cols.Add(new(GetString("Header_Congruence_Percent"), ProfilePath(nameof(StroopProfile.CongruencePercent))));
            cols.Add(new(GetString("Header_VisualCue"), ProfilePath(nameof(StroopProfile.HasVisualCue)))
            { Type = BlockSummaryCellType.BooleanGlyph});
            cols.Add(new(GetString("Header_Switch"), ProfilePath(nameof(StroopProfile.SwitchPercent))));
            return cols;
        }
    }
}
