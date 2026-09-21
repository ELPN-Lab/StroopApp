namespace StroopApp.XUnitTests;

using ClosedXML.Excel;
using Moq;
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
}
