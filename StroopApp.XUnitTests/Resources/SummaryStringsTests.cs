using StroopApp.Resources;
using System.Globalization;
using Xunit;

namespace StroopApp.XUnitTests.Resources
{
    /// <summary>
    /// Tests that the headers of the mean response time columns are translated in English and French.
    /// Cultures are passed explicitly so the tests do not depend on the current UI culture.
    /// </summary>
    public class SummaryStringsTests
    {
        private static string Get(string key, string culture)
            => Strings.ResourceManager.GetString(key, new CultureInfo(culture))!;

        [Theory]
        [InlineData("Header_MeanResponseTime", "en", "Mean response time (all answers)")]
        [InlineData("Header_MeanCorrectResponseTime", "en", "Mean response time (correct answers)")]
        [InlineData("Label_MeanCorrect", "en", "Mean (correct)")]
        [InlineData("Header_MeanResponseTime", "fr", "Temps moyen de réponse (toutes réponses)")]
        [InlineData("Header_MeanCorrectResponseTime", "fr", "Temps moyen de réponse (bonnes réponses)")]
        [InlineData("Label_MeanCorrect", "fr", "Moyenne (bonnes rép.)")]
        public void MeanHeaders_AreTranslated(string key, string culture, string expected)
        {
            Assert.Equal(expected, Get(key, culture));
        }

        [Theory]
        [InlineData("Header_MeanResponseTime")]
        [InlineData("Header_MeanCorrectResponseTime")]
        [InlineData("Label_MeanCorrect")]
        public void MeanHeaders_DifferBetweenEnglishAndFrench(string key)
        {
            Assert.NotEqual(Get(key, "en"), Get(key, "fr"));
        }

        /// <summary>
        /// The two summary headers must be distinguishable in each language.
        /// </summary>
        [Theory]
        [InlineData("en")]
        [InlineData("fr")]
        public void SummaryMeanHeaders_AreDistinct(string culture)
        {
            Assert.NotEqual(Get("Header_MeanResponseTime", culture), Get("Header_MeanCorrectResponseTime", culture));
        }
    }
}
