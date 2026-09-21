using ClosedXML.Excel;
using StroopApp.Models;
using StroopApp.Services.Language;

namespace StroopApp.Services.Exportation
{
    public abstract class TrialExportFormatter
    {
        protected readonly ILanguageService LanguageService;
        protected TrialExportFormatter(ILanguageService languageService)
        {
            LanguageService = languageService;
        }

        public abstract TaskType TaskType { get;  }
        public abstract List<string> GetColumnHeaders();
        public abstract void WriteRow(IXLWorksheet worksheet, int row, ITrial trial, string profileName, int blockNumber);
        protected static int CommonPrefixCount => 4;
        protected List<string> GetCommonPrefixHeaders() => new()
        {
            LanguageService.GetLocalizedString("Header_ParticipantId"),
            LanguageService.GetLocalizedString("Header_ProfileName"),
            LanguageService.GetLocalizedString("Header_BlockNumber"),
            LanguageService.GetLocalizedString("Header_Trials")
        };

        protected List<string> GetCommonSuffixHeaders() => new()
        {
            LanguageService.GetLocalizedString("Header_Response_Validity"),
            LanguageService.GetLocalizedString("Header_ResponseTime")
        };

        protected void WriteCommonPrefix(IXLWorksheet worksheet, int row, ITrial trial, string profileName, int blockNumber)
        {
            worksheet.Cell(row, 1).Value = trial.ParticipantId;
            worksheet.Cell(row, 2).Value = profileName;
            worksheet.Cell(row, 3).Value = blockNumber;
            worksheet.Cell(row, 4).Value = trial.TrialNumber;
        }

        protected void WriteCommonSuffix(IXLWorksheet worksheet, int row, int col, ITrial trial)
        {
            var validCell = worksheet.Cell(row, col);
            if (trial.IsValidResponse.HasValue)
                validCell.Value = trial.IsValidResponse.Value;
            else
                validCell.Clear();

            worksheet.Cell(row, col+1).Value = trial.ReactionTime;
        }
    }
}
