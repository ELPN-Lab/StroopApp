namespace StroopApp.Models
{
    /// <summary>
    /// Shared statistics rules for response times, so that every table applies the same definition of a correct answer.
    /// </summary>
    public static class ResponseTimeStatistics
    {
        /// <summary>
        /// Computes the mean reaction time of the correct answers only.
        /// A correct answer is a trial whose <c>IsValidResponse</c> is <c>true</c> and whose reaction time is neither <c>null</c> nor <c>NaN</c>.
        /// </summary>
        /// <returns>The mean in milliseconds, or <c>null</c> when no trial qualifies.</returns>
        public static double? MeanOfCorrectAnswers(IEnumerable<(bool? IsValidResponse, double? ReactionTime)> trials)
        {
            var correctTimes = trials
                .Where(t => t.IsValidResponse == true && t.ReactionTime.HasValue && !double.IsNaN(t.ReactionTime.Value))
                .Select(t => t.ReactionTime!.Value)
                .ToList();

            return correctTimes.Count > 0 ? correctTimes.Average() : null;
        }
    }
}
