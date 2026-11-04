namespace StroopApp.XUnitTests;

using ClosedXML.Excel;
using Moq;
using StroopApp.Models;
using StroopApp.Models.Calibration;
using StroopApp.Models.Simon;
using StroopApp.Services.Exportation;
using StroopApp.Services.Language;
using Xunit;

public class ExportFormatterTests 
{
    [Fact]
    public void SimonFormatter_HeadersAndRowValuesStayAligned()
    {
        // arrange
        var mockLanguageService = new Mock<ILanguageService>();
        mockLanguageService.Setup(ls => ls.GetLocalizedString(It.IsAny<string>(), It.IsAny<string?>()))
            .Returns((string key, string? _) => key);
        var formatter = new SimonTrialExportFormatter(mockLanguageService.Object);

        var trial = new SimonTrial()
        {
            ParticipantId = "999",
            TrialNumber = 10,
            CongruencePercent = 11,
            ReversedMappingPercent = 22,
            IsAnswerCongruent = true,
            IsReversedMapping = true,
            ExpectedAnswer = SimonAnswer.Right,
            GivenAnswer = SimonAnswer.Left,
            IsValidResponse = false,
            ReactionTime = 333.5,
            Stimulus = new SimonStimulus("#123456",StimulusPosition.Left,SimonStimulusShape.Triangle),
        };

        using var workbook = new XLWorkbook();

        var ws = workbook.AddWorksheet("test");
        var headers = formatter.GetColumnHeaders();
        for (int i = 0; i < headers.Count; i++)
            ws.Cell(1, i + 1).Value = headers[i];

        // act
        formatter.WriteRow(ws, 2, trial, "profileTest", 3);

        // assert
        Assert.Equal(headers.Count, ws.Row(2).LastCellUsed().Address.ColumnNumber);

        int Col(string key) => headers.IndexOf(key) + 1;
        Assert.Equal(11, ws.Cell(2, Col("Header_Congruence_Percent")).GetValue<int>());
        Assert.Equal(22, ws.Cell(2, Col("Header_ReversedMappingPercent")).GetValue<int>());
        Assert.True(ws.Cell(2, Col("Header_IsAnswerCongruent")).GetValue<bool>());
        Assert.False(ws.Cell(2, Col("Header_IsSpatialCongruent")).GetValue<bool>());
        Assert.True(ws.Cell(2, Col("Header_IsReversedMapping")).GetValue<bool>());
        Assert.Equal("Left", ws.Cell(2, Col("Header_StimulusPosition")).GetString());
        Assert.Equal("#123456", ws.Cell(2, Col("Header_StimulusColor")).GetString());
        Assert.Equal("Triangle", ws.Cell(2, Col("Header_StimulusShape")).GetString());
        Assert.Equal("Right", ws.Cell(2, Col("Header_Expected_Answer")).GetString());
        Assert.Equal("Left", ws.Cell(2, Col("Header_Given_Answer")).GetString());
    }

    [Fact]
    public void StroopFormatter_CalibrationColumns_AreWrittenAtTheEnd()
    {
        // arrange
        var formatter = new StroopTrialExportFormatter(CreateKeyLanguageService());
        var trial = new StroopTrial
        {
            ParticipantId = "999",
            TrialNumber = 4,
            ReactionTime = 420,
            IsValidResponse = true,
            PresentationTime = 1250,
            CalibrationPhase = 2
        };

        using var workbook = new XLWorkbook();
        var ws = workbook.AddWorksheet("test");
        var headers = formatter.GetColumnHeaders();

        // act
        formatter.WriteRow(ws, 2, trial, "profileTest", 1);

        // assert
        Assert.Equal("Header_PresentationTime", headers[^2]);
        Assert.Equal("Header_CalibrationPhase", headers[^1]);
        Assert.Equal(headers.Count, ws.Row(2).LastCellUsed().Address.ColumnNumber);
        Assert.Equal(1250, ws.Cell(2, headers.Count - 1).GetValue<int>());
        Assert.Equal(2, ws.Cell(2, headers.Count).GetValue<int>());
    }

    [Fact]
    public void CalibrationFormatter_HeadersAndRowValuesStayAligned()
    {
        // arrange
        var formatter = new CalibrationExportFormatter(CreateKeyLanguageService());
        var block = new CalibrationBlock(new StroopProfile { ProfileName = "calib" }, 3, new CalibrationParameters());
        for (int i = 0; i < block.Parameters.Phase1TrialCount + 5; i++)
            block.RecordTrial(new StroopTrial { IsValidResponse = i % 3 != 0, ReactionTime = 500 + 10 * i });
        block.StopByExperimenter();
        var result = block.Result!;
        result.SliderAccuracy = 0.75;
        result.SliderPresentationTime = 900;

        using var workbook = new XLWorkbook();
        var ws = workbook.AddWorksheet(CalibrationExportFormatter.WorksheetName);
        var headers = formatter.GetColumnHeaders();

        // act
        formatter.WriteRow(ws, 2, "999", block);

        // assert
        Assert.Equal(headers.Count, ws.Row(2).LastCellUsed().Address.ColumnNumber);

        int Col(string key) => headers.IndexOf(key) + 1;
        Assert.Equal("calib", ws.Cell(2, Col("Header_ProfileName")).GetString());
        Assert.Equal(3, ws.Cell(2, Col("Header_BlockNumber")).GetValue<int>());
        Assert.Equal($"Label_CalibrationStatus_{result.Status}", ws.Cell(2, Col("Header_CalibrationStatus")).GetString());
        Assert.Equal("Label_CalibrationStopReason_StoppedByExperimenter", ws.Cell(2, Col("Header_CalibrationStopReason")).GetString());
        Assert.Equal(result.T50!.Value, ws.Cell(2, Col("Header_CalibrationT50")).GetValue<double>(), 6);
        Assert.Equal(5, ws.Cell(2, Col("Header_CalibrationPhase2TrialCount")).GetValue<int>());
        Assert.Equal(75, ws.Cell(2, Col("Header_CalibrationSliderAccuracy")).GetValue<double>(), 6);
        Assert.Equal(900, ws.Cell(2, Col("Header_CalibrationSliderPresentationTime")).GetValue<double>());
        Assert.Equal(85, ws.Cell(2, Col("Header_CalibrationTargetAccuracy")).GetValue<double>(), 6);
        Assert.Equal(0.25, ws.Cell(2, Col("Header_CalibrationGamma")).GetValue<double>());
    }

    [Fact]
    public void CalibrationFormatter_PhaseOneStop_LeavesEstimateCellsEmpty()
    {
        // arrange
        var formatter = new CalibrationExportFormatter(CreateKeyLanguageService());
        var block = new CalibrationBlock(new StroopProfile(), 1, new CalibrationParameters());
        block.RecordTrial(new StroopTrial { IsValidResponse = true, ReactionTime = 500 });
        block.StopByExperimenter();

        using var workbook = new XLWorkbook();
        var ws = workbook.AddWorksheet(CalibrationExportFormatter.WorksheetName);
        var headers = formatter.GetColumnHeaders();

        // act
        formatter.WriteRow(ws, 2, "999", block);

        // assert
        int Col(string key) => headers.IndexOf(key) + 1;
        Assert.Equal("Label_CalibrationStatus_EstimationImpossible", ws.Cell(2, Col("Header_CalibrationStatus")).GetString());
        Assert.True(ws.Cell(2, Col("Header_CalibrationT50")).IsEmpty());
        Assert.True(ws.Cell(2, Col("Header_CalibrationSliderAccuracy")).IsEmpty());
        Assert.Equal(headers.Count, ws.Row(2).LastCellUsed().Address.ColumnNumber);
    }

    private static ILanguageService CreateKeyLanguageService()
    {
        var mockLanguageService = new Mock<ILanguageService>();
        mockLanguageService.Setup(ls => ls.GetLocalizedString(It.IsAny<string>(), It.IsAny<string?>()))
            .Returns((string key, string? _) => key);
        return mockLanguageService.Object;
    }
}
