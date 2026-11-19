using StroopApp.Models;
using StroopApp.Services.Summary;
using StroopApp.XUnitTests.TestDummies;
using Xunit;

namespace StroopApp.XUnitTests.Services.Summary
{
    /// <summary>
    /// Unit tests for the columns of the block summary table shown on the end page.
    /// The dummy language service returns the resource key as the header, so headers identify the columns.
    /// </summary>
    public class BlockSummaryFormatterTests
    {
        public static IEnumerable<object[]> Formatters()
        {
            var language = new DummyLanguageService();
            yield return new object[] { new StroopBlockSummaryFormatter(language) };
            yield return new object[] { new SimonBlockSummaryFormatter(language) };
        }

        /// <summary>
        /// The summary has two mean columns, the overall one bound to ResponseTimeMean and the correct one bound to CorrectResponseTimeMean.
        /// </summary>
        [Theory]
        [MemberData(nameof(Formatters))]
        public void GetColumns_ReturnsOverallAndCorrectMeanColumns(BlockSummaryFormatter formatter)
        {
            var columns = formatter.GetColumns();

            var overall = Assert.Single(columns, c => c.Header == "Header_MeanResponseTime");
            var correct = Assert.Single(columns, c => c.Header == "Header_MeanCorrectResponseTime");
            Assert.Equal(nameof(Block.ResponseTimeMean), overall.BindingPath);
            Assert.Equal(nameof(Block.CorrectResponseTimeMean), correct.BindingPath);
        }

        /// <summary>
        /// The correct mean column comes right after the overall one and both show milliseconds with two decimals.
        /// </summary>
        [Theory]
        [MemberData(nameof(Formatters))]
        public void GetColumns_CorrectMeanFollowsOverallMeanWithSameFormat(BlockSummaryFormatter formatter)
        {
            var columns = formatter.GetColumns();

            int overallIndex = columns.FindIndex(c => c.Header == "Header_MeanResponseTime");
            Assert.Equal("Header_MeanCorrectResponseTime", columns[overallIndex + 1].Header);
            Assert.Equal("{0:F2} ms", columns[overallIndex].StringFormat);
            Assert.Equal("{0:F2} ms", columns[overallIndex + 1].StringFormat);
        }

        /// <summary>
        /// A correct mean with no value is displayed as "NA" in the summary.
        /// </summary>
        [Theory]
        [MemberData(nameof(Formatters))]
        public void GetColumns_CorrectMeanColumn_DisplaysNAWhenNoValue(BlockSummaryFormatter formatter)
        {
            var correct = formatter.GetColumns().Single(c => c.Header == "Header_MeanCorrectResponseTime");

            Assert.Equal("NA", correct.NullDisplay);
        }

        /// <summary>
        /// Columns other than the means keep an empty cell when their value is null.
        /// </summary>
        [Theory]
        [MemberData(nameof(Formatters))]
        public void GetColumns_OtherColumns_HaveNoNullDisplay(BlockSummaryFormatter formatter)
        {
            var others = formatter.GetColumns()
                .Where(c => c.Header != "Header_MeanResponseTime" && c.Header != "Header_MeanCorrectResponseTime");

            Assert.All(others, c => Assert.Null(c.NullDisplay));
        }
    }
}
