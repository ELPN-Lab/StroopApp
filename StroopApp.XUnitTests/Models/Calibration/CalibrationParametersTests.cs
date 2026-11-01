using StroopApp.Models.Calibration;
using Xunit;

namespace StroopApp.XUnitTests.Models.Calibration
{
    /// <summary>
    /// Unit tests for CalibrationParameters defaults, cloning and validation rules.
    /// </summary>
    public class CalibrationParametersTests
    {
        private static bool HasError(CalibrationParameters parameters, CalibrationParametersError error)
        {
            return parameters.GetValidationErrors().Contains(error);
        }

        /// <summary>
        /// Default values match the calibration specification.
        /// </summary>
        [Fact]
        public void Constructor_Defaults_MatchSpecification()
        {
            var parameters = new CalibrationParameters();

            Assert.Equal(2000, parameters.StartingTime);
            Assert.Equal(140, parameters.LowerBound);
            Assert.Equal(5000, parameters.UpperBound);
            Assert.Equal(0.85, parameters.TargetAccuracy);
            Assert.Equal(0.25, parameters.Gamma);
            Assert.Equal(0.01, parameters.Delta);
            Assert.Equal(2.0, parameters.Beta);
            Assert.Equal(20, parameters.Phase1TrialCount);
            Assert.Equal(5, parameters.MinCorrectPhase1Trials);
            Assert.Equal(40, parameters.MinPhase2TrialCount);
            Assert.Equal(100, parameters.MaxPhase2TrialCount);
            Assert.Equal(2.0, parameters.PriorWidthFactor);
            Assert.Equal(0.20, parameters.ConvergedThreshold);
            Assert.Equal(0.40, parameters.NearlyConvergedThreshold);
        }

        /// <summary>
        /// Default values pass every validation rule.
        /// </summary>
        [Fact]
        public void GetValidationErrors_Defaults_ReturnsEmpty()
        {
            var parameters = new CalibrationParameters();

            Assert.Empty(parameters.GetValidationErrors());
        }

        /// <summary>
        /// A clone holds the same values as the original.
        /// </summary>
        [Fact]
        public void Clone_CopiesAllValues()
        {
            var original = new CalibrationParameters
            {
                StartingTime = 5000,
                LowerBound = 200,
                UpperBound = 6000,
                TargetAccuracy = 0.8,
                Delta = 0.02,
                Beta = 1.85,
                Phase1TrialCount = 10,
                MinCorrectPhase1Trials = 3,
                MinPhase2TrialCount = 30,
                MaxPhase2TrialCount = 80,
                PriorWidthFactor = 3.0,
                ConvergedThreshold = 0.15,
                NearlyConvergedThreshold = 0.35
            };

            var clone = original.Clone();

            Assert.NotSame(original, clone);
            Assert.Equivalent(original, clone);
        }

        /// <summary>
        /// Modifying a clone leaves the original unchanged.
        /// </summary>
        [Fact]
        public void Clone_ModifyingClone_DoesNotChangeOriginal()
        {
            var original = new CalibrationParameters();

            var clone = original.Clone();
            clone.StartingTime = 5000;
            clone.Beta = 1.85;

            Assert.Equal(2000, original.StartingTime);
            Assert.Equal(2.0, original.Beta);
        }

        /// <summary>
        /// The lower bound must be strictly positive.
        /// </summary>
        [Theory]
        [InlineData(0, true)]
        [InlineData(-1, true)]
        [InlineData(1, false)]
        public void GetValidationErrors_LowerBound_FlagsNonPositive(int lowerBound, bool expectedError)
        {
            var parameters = new CalibrationParameters { LowerBound = lowerBound };

            Assert.Equal(expectedError, HasError(parameters, CalibrationParametersError.LowerBoundNotPositive));
        }

        /// <summary>
        /// The lower bound must be strictly below the upper bound.
        /// </summary>
        [Theory]
        [InlineData(5000, 5000, true)]
        [InlineData(5001, 5000, true)]
        [InlineData(4999, 5000, false)]
        public void GetValidationErrors_Bounds_FlagsLowerNotBelowUpper(int lowerBound, int upperBound, bool expectedError)
        {
            var parameters = new CalibrationParameters { LowerBound = lowerBound, UpperBound = upperBound };

            Assert.Equal(expectedError, HasError(parameters, CalibrationParametersError.LowerBoundNotBelowUpperBound));
        }

        /// <summary>
        /// The starting time must lie within the bounds, bounds included.
        /// </summary>
        [Theory]
        [InlineData(139, true)]
        [InlineData(140, false)]
        [InlineData(5000, false)]
        [InlineData(5001, true)]
        public void GetValidationErrors_StartingTime_FlagsOutOfBounds(int startingTime, bool expectedError)
        {
            var parameters = new CalibrationParameters { StartingTime = startingTime };

            Assert.Equal(expectedError, HasError(parameters, CalibrationParametersError.StartingTimeOutOfBounds));
        }

        /// <summary>
        /// The target accuracy must lie strictly between gamma and 1 minus delta.
        /// </summary>
        [Theory]
        [InlineData(0.25, true)]
        [InlineData(0.2501, false)]
        [InlineData(0.98, false)]
        [InlineData(0.995, true)]
        public void GetValidationErrors_TargetAccuracy_FlagsOutOfRange(double targetAccuracy, bool expectedError)
        {
            var parameters = new CalibrationParameters { TargetAccuracy = targetAccuracy };

            Assert.Equal(expectedError, HasError(parameters, CalibrationParametersError.TargetAccuracyOutOfRange));
        }

        /// <summary>
        /// The slope must be strictly positive.
        /// </summary>
        [Theory]
        [InlineData(0.0, true)]
        [InlineData(-1.0, true)]
        [InlineData(0.01, false)]
        public void GetValidationErrors_Beta_FlagsNonPositive(double beta, bool expectedError)
        {
            var parameters = new CalibrationParameters { Beta = beta };

            Assert.Equal(expectedError, HasError(parameters, CalibrationParametersError.BetaNotPositive));
        }

        /// <summary>
        /// Delta must be strictly positive and strictly below 1 minus gamma.
        /// </summary>
        [Theory]
        [InlineData(0.0, true)]
        [InlineData(-0.01, true)]
        [InlineData(0.0001, false)]
        [InlineData(0.7499, false)]
        [InlineData(0.75, true)]
        public void GetValidationErrors_Delta_FlagsOutOfRange(double delta, bool expectedError)
        {
            var parameters = new CalibrationParameters { Delta = delta };

            Assert.Equal(expectedError, HasError(parameters, CalibrationParametersError.DeltaOutOfRange));
        }

        /// <summary>
        /// The prior width factor must be strictly above 1.
        /// </summary>
        [Theory]
        [InlineData(1.0, true)]
        [InlineData(0.5, true)]
        [InlineData(1.01, false)]
        public void GetValidationErrors_PriorWidthFactor_FlagsNotAboveOne(double priorWidthFactor, bool expectedError)
        {
            var parameters = new CalibrationParameters { PriorWidthFactor = priorWidthFactor };

            Assert.Equal(expectedError, HasError(parameters, CalibrationParametersError.PriorWidthFactorNotAboveOne));
        }

        /// <summary>
        /// The minimum number of correct phase 1 answers must be between 2 and the phase 1 trial count.
        /// </summary>
        [Theory]
        [InlineData(1, true)]
        [InlineData(2, false)]
        [InlineData(20, false)]
        [InlineData(21, true)]
        public void GetValidationErrors_MinCorrectPhase1Trials_FlagsOutOfRange(int minCorrect, bool expectedError)
        {
            var parameters = new CalibrationParameters { MinCorrectPhase1Trials = minCorrect };

            Assert.Equal(expectedError, HasError(parameters, CalibrationParametersError.MinCorrectOutOfRange));
        }

        /// <summary>
        /// The maximum number of phase 2 trials must be strictly positive.
        /// </summary>
        [Theory]
        [InlineData(0, true)]
        [InlineData(-1, true)]
        [InlineData(1, false)]
        public void GetValidationErrors_MaxPhase2TrialCount_FlagsNonPositive(int maxPhase2, bool expectedError)
        {
            var parameters = new CalibrationParameters { MaxPhase2TrialCount = maxPhase2 };

            Assert.Equal(expectedError, HasError(parameters, CalibrationParametersError.MaxPhase2NotPositive));
        }

        /// <summary>
        /// The minimum number of phase 2 trials must not exceed the maximum.
        /// </summary>
        [Theory]
        [InlineData(101, 100, true)]
        [InlineData(100, 100, false)]
        [InlineData(40, 100, false)]
        public void GetValidationErrors_Phase2TrialCounts_FlagsMinAboveMax(int minPhase2, int maxPhase2, bool expectedError)
        {
            var parameters = new CalibrationParameters { MinPhase2TrialCount = minPhase2, MaxPhase2TrialCount = maxPhase2 };

            Assert.Equal(expectedError, HasError(parameters, CalibrationParametersError.MinPhase2AboveMax));
        }

        /// <summary>
        /// The converged threshold must not exceed the nearly converged threshold.
        /// </summary>
        [Theory]
        [InlineData(0.41, 0.40, true)]
        [InlineData(0.40, 0.40, false)]
        [InlineData(0.20, 0.40, false)]
        public void GetValidationErrors_Thresholds_FlagsConvergedAboveNearlyConverged(double converged, double nearlyConverged, bool expectedError)
        {
            var parameters = new CalibrationParameters { ConvergedThreshold = converged, NearlyConvergedThreshold = nearlyConverged };

            Assert.Equal(expectedError, HasError(parameters, CalibrationParametersError.ConvergedAboveNearlyConverged));
        }
    }
}
