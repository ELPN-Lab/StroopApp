using StroopApp.Models.Calibration;
using StroopApp.Services.Calibration;
using Xunit;

namespace StroopApp.XUnitTests.Services.Calibration
{
    /// <summary>
    /// Compares CalibrationCalculator with values computed by the independent Python script Reference/quest_reference.py.
    /// </summary>
    public class CalibrationReferenceTests
    {
        private const double Tolerance = 1e-9;
        private const bool C = true;
        private const bool E = false;

        private static (List<int> Times, double T5, double T50, double T95, double R) Run(CalibrationParameters parameters, double startingPoint, bool[] answers)
        {
            var grid = CalibrationCalculator.CreateGrid(parameters);
            var candidates = CalibrationCalculator.CreateCandidates(grid);
            var probabilityTable = CalibrationCalculator.CreateProbabilityTable(grid, candidates, parameters);
            var logWeights = CalibrationCalculator.CreatePrior(grid, startingPoint, parameters);
            var times = new List<int>();

            foreach (bool isCorrect in answers)
            {
                int time = CalibrationCalculator.SelectNextPresentationTime(logWeights, candidates, probabilityTable);
                times.Add(time);
                logWeights = CalibrationCalculator.Update(logWeights, grid, time, isCorrect, parameters);
            }

            double t5 = CalibrationCalculator.Quantile(logWeights, grid, 0.05);
            double t50 = CalibrationCalculator.Quantile(logWeights, grid, 0.5);
            double t95 = CalibrationCalculator.Quantile(logWeights, grid, 0.95);
            return (times, t5, t50, t95, CalibrationCalculator.ComputeR(t5, t50, t95));
        }

        /// <summary>
        /// Sequence A: default parameters, start at 800 ms, mostly correct answers.
        /// </summary>
        [Fact]
        public void Run_SequenceA_MatchesPythonReference()
        {
            var result = Run(new CalibrationParameters(), 800, [C, C, E, C, C, C, E, C, C, C]);

            Assert.Equal([684, 576, 490, 822, 742, 676, 631, 832, 785, 750], result.Times);
            Assert.Equal(494.58097238730403, result.T5, Tolerance);
            Assert.Equal(733.3956053647017, result.T50, Tolerance);
            Assert.Equal(1163.0481897570764, result.T95, Tolerance);
            Assert.Equal(0.9114688068486014, result.R, Tolerance);
        }

        /// <summary>
        /// Sequence B: default parameters, start at 800 ms, only errors.
        /// </summary>
        [Fact]
        public void Run_SequenceB_MatchesPythonReference()
        {
            var result = Run(new CalibrationParameters(), 800, [E, E, E, E, E, E, E, E]);

            Assert.Equal([684, 1230, 1797, 2368, 2846, 3230, 3541, 3794], result.Times);
            Assert.Equal(3596.5062151031407, result.T5, Tolerance);
            Assert.Equal(4534.299868725684, result.T50, Tolerance);
            Assert.Equal(4985.191220993236, result.T95, Tolerance);
            Assert.Equal(0.30626227777043125, result.R, Tolerance);
        }

        /// <summary>
        /// Sequence C: custom parameters (200 to 3000 ms, 80 %, beta 1.85, width 3), start at 1200 ms, alternating answers.
        /// </summary>
        [Fact]
        public void Run_SequenceC_MatchesPythonReference()
        {
            var parameters = new CalibrationParameters
            {
                LowerBound = 200,
                UpperBound = 3000,
                TargetAccuracy = 0.80,
                Beta = 1.85,
                PriorWidthFactor = 3.0
            };

            var result = Run(parameters, 1200, [C, E, C, E, C, E, C, E, C, E, C, E]);

            Assert.Equal([793, 683, 1184, 1106, 1524, 1473, 1811, 1770, 2031, 2008, 2226, 2201], result.Times);
            Assert.Equal(1780.0246756641461, result.T5, Tolerance);
            Assert.Equal(2474.807873685081, result.T50, Tolerance);
            Assert.Equal(2960.396175037145, result.T95, Tolerance);
            Assert.Equal(0.4769548019965613, result.R, Tolerance);
        }
    }
}
