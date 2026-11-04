using ClosedXML.Excel;
using StroopApp.Models.Calibration;
using StroopApp.Services.Language;

namespace StroopApp.Services.Exportation
{
    /// <summary>
    /// Writes the "Calibration" worksheet: one row per calibration block, with its result and parameters.
    /// </summary>
    public class CalibrationExportFormatter
    {
        public const string WorksheetName = "Calibration";

        private readonly ILanguageService _languageService;

        public CalibrationExportFormatter(ILanguageService languageService)
        {
            _languageService = languageService;
        }

        /// <summary>
        /// Localized column headers, in the order used by <see cref="WriteRow"/>.
        /// </summary>
        public List<string> GetColumnHeaders() => new()
        {
            GetString("Header_ParticipantId"),
            GetString("Header_ProfileName"),
            GetString("Header_BlockNumber"),
            GetString("Header_CalibrationStatus"),
            GetString("Header_CalibrationStopReason"),
            GetString("Header_CalibrationT50"),
            GetString("Header_CalibrationT5"),
            GetString("Header_CalibrationT95"),
            GetString("Header_CalibrationR"),
            GetString("Header_CalibrationPhase1TrialCount"),
            GetString("Header_CalibrationPhase2TrialCount"),
            GetString("Header_CalibrationStartingPoint"),
            GetString("Header_CalibrationSliderAccuracy"),
            GetString("Header_CalibrationSliderPresentationTime"),
            GetString("Header_CalibrationStartingTime"),
            GetString("Header_CalibrationLowerBound"),
            GetString("Header_CalibrationUpperBound"),
            GetString("Header_CalibrationTargetAccuracy"),
            GetString("Header_CalibrationGamma"),
            GetString("Header_CalibrationDelta"),
            GetString("Header_CalibrationBeta"),
            GetString("Header_CalibrationPriorWidthFactor"),
            GetString("Header_CalibrationMinCorrectPhase1Trials"),
            GetString("Header_CalibrationMinPhase2TrialCount"),
            GetString("Header_CalibrationMaxPhase2TrialCount"),
            GetString("Header_CalibrationConvergedThreshold"),
            GetString("Header_CalibrationNearlyConvergedThreshold")
        };

        /// <summary>
        /// Writes the result of a finished calibration block; accuracies are written as percentages.
        /// </summary>
        public void WriteRow(IXLWorksheet worksheet, int row, string participantId, CalibrationBlock block)
        {
            var result = block.Result ?? throw new InvalidOperationException("The calibration block has no result.");
            var parameters = result.Parameters;
            int col = 1;

            worksheet.Cell(row, col++).Value = participantId;
            worksheet.Cell(row, col++).Value = block.Profile.ProfileName;
            worksheet.Cell(row, col++).Value = block.BlockNumber;
            worksheet.Cell(row, col++).Value = GetString($"Label_CalibrationStatus_{result.Status}");
            worksheet.Cell(row, col++).Value = GetString($"Label_CalibrationStopReason_{result.StopReason}");
            worksheet.Cell(row, col++).Value = result.T50;
            worksheet.Cell(row, col++).Value = result.T5;
            worksheet.Cell(row, col++).Value = result.T95;
            worksheet.Cell(row, col++).Value = result.R;
            worksheet.Cell(row, col++).Value = result.Phase1TrialCount;
            worksheet.Cell(row, col++).Value = result.Phase2TrialCount;
            worksheet.Cell(row, col++).Value = result.StartingPoint;
            worksheet.Cell(row, col++).Value = result.SliderAccuracy * 100;
            worksheet.Cell(row, col++).Value = result.SliderPresentationTime;
            worksheet.Cell(row, col++).Value = parameters.StartingTime;
            worksheet.Cell(row, col++).Value = parameters.LowerBound;
            worksheet.Cell(row, col++).Value = parameters.UpperBound;
            worksheet.Cell(row, col++).Value = parameters.TargetAccuracy * 100;
            worksheet.Cell(row, col++).Value = parameters.Gamma;
            worksheet.Cell(row, col++).Value = parameters.Delta;
            worksheet.Cell(row, col++).Value = parameters.Beta;
            worksheet.Cell(row, col++).Value = parameters.PriorWidthFactor;
            worksheet.Cell(row, col++).Value = parameters.MinCorrectPhase1Trials;
            worksheet.Cell(row, col++).Value = parameters.MinPhase2TrialCount;
            worksheet.Cell(row, col++).Value = parameters.MaxPhase2TrialCount;
            worksheet.Cell(row, col++).Value = parameters.ConvergedThreshold;
            worksheet.Cell(row, col).Value = parameters.NearlyConvergedThreshold;
        }

        private string GetString(string key) => _languageService.GetLocalizedString(key);
    }
}
