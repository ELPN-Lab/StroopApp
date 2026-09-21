using ClosedXML.Excel;
using StroopApp.Models;
using StroopApp.Services.Language;

namespace StroopApp.Services.Exportation
{
    public class StroopTrialExportFormatter : TrialExportFormatter
    {
        public StroopTrialExportFormatter(ILanguageService languageService) : base(languageService) { }

        public override TaskType TaskType => TaskType.Stroop;

        public override List<string> GetColumnHeaders()
        {
            var headers = GetCommonPrefixHeaders();
            headers.Add(LanguageService.GetLocalizedString("Header_Congruence"));
            headers.Add(LanguageService.GetLocalizedString("Header_VisualCue"));
            headers.Add(LanguageService.GetLocalizedString("Header_Expected_Answer"));
            headers.Add(LanguageService.GetLocalizedString("Header_Given_Answer"));
            headers.AddRange(GetCommonSuffixHeaders());
            return headers;
        }

        public override void WriteRow(IXLWorksheet worksheet, int row, ITrial trial, string profileName, int blockNumber)
        {
            WriteCommonPrefix(worksheet, row, trial, profileName, blockNumber);
            var stroopTrial = (StroopTrial)trial;
            int col = CommonPrefixCount + 1;
            worksheet.Cell(row, col++).Value = stroopTrial.IsCongruent;
            worksheet.Cell(row, col++).Value = stroopTrial.VisualCue switch
            {
                VisualCueType.Square => LanguageService.GetLocalizedString("Label_Square"),
                VisualCueType.Round => LanguageService.GetLocalizedString("Label_Circle"),
                _ => " "
            };
            worksheet.Cell(row, col++).Value = stroopTrial.ExpectedAnswer;
            worksheet.Cell(row, col++).Value = stroopTrial.GivenAnswer;
            WriteCommonSuffix(worksheet, row, col, trial);
        }
    }
}
