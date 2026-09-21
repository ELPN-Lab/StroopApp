using CommunityToolkit.Mvvm.ComponentModel;
using StroopApp.Core;
using StroopApp.Models;
using StroopApp.Services.Navigation;
using StroopApp.ViewModels.State;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using StroopApp.ViewModels.Experiment.Participant.Common;
namespace StroopApp.ViewModels.Experiment.Participant.Stroop
{
    /// <summary>
    /// Manages Stroop test trial execution, timing, and participant input handling.
    /// Orchestrates the trial sequence (fixation → cue → stimulus → response) with validated ±8ms timing precision.
    /// </summary>
    public partial class StroopViewModel : ViewModelBase, IDisposable
    {
        /// <summary>
        /// Experiment settings provided via constructor.
        /// Initialized before ViewModel usage.
        /// </summary>
        public ExperimentSettingsViewModel Settings
        {
            get; set;
        }

        /// <summary>
        /// Current step ViewModel displayed to the participant.
        /// Its type determines which DataTemplate is rendered.
        /// Initialized during trial execution.
        /// </summary>
        [ObservableProperty]
        private object? _currentStepViewModel;

        /// <summary>
        /// Task completion source for participant input.
        /// Initialized in StartResponseTimer() or StartTrials().
        /// </summary>
        private TaskCompletionSource<double> _inputTcs = null!;
        private readonly Stopwatch _responseTime;
        private readonly Stopwatch _wordTimer;
        /// <summary>
        /// Navigation service provided via constructor dependency injection.
        /// Guaranteed non-null during ViewModel lifecycle.
        /// </summary>
        private readonly INavigationService _participantWindowNavigationService = null!;
        private CancellationTokenSource _cancellationTokenSource = null!;
        private bool _isDisposed = false;

        public StroopViewModel(ExperimentSettingsViewModel settings, INavigationService participantWindowNavigationService)
        {
            Settings = settings;
            _participantWindowNavigationService = participantWindowNavigationService;
            _responseTime = new Stopwatch();
            _wordTimer = new Stopwatch();
            _cancellationTokenSource = new CancellationTokenSource();

            StartTrials();
        }

        public void StartResponseTimer()
        {
            _responseTime.Restart();
            _wordTimer.Restart();
            _inputTcs = new TaskCompletionSource<double>();
        }

        /// <summary>
        /// Executes the complete trial sequence for the current block.
        /// Handles timing for fixation cross, visual cue (if enabled), stimulus presentation, and response collection.
        /// Supports cancellation and stores reaction times with trial results.
        /// </summary>
        public async void StartTrials()
        {
            try
            {
                if (Settings.ExperimentContext.CurrentBlock is null)
                    throw new InvalidOperationException("CurrentBlock is not initialized");

                foreach (StroopTrial trial in Settings.ExperimentContext.CurrentBlock.TrialRecords)
                {
                    if (Settings.ExperimentContext.IsTaskStopped || _cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        HandleTaskStopped();
                        return;
                    }
                    Settings.ExperimentContext.CurrentTrial = trial;

                    CurrentStepViewModel = new FixationCrossViewModel();
                    await Task.Delay(Settings.CurrentProfile.FixationDuration, _cancellationTokenSource.Token);
                    if (Settings.ExperimentContext.IsTaskStopped || _cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        HandleTaskStopped();
                        return;
                    }
                    if (Settings.CurrentProfile.HasVisualCue)
                    {
                        CurrentStepViewModel = new VisualCueControlViewModel(trial.VisualCue);
                        await Task.Delay(Settings.CurrentProfile.VisualCueDuration, _cancellationTokenSource.Token);

                        if (Settings.ExperimentContext.IsTaskStopped || _cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            HandleTaskStopped();
                            return;
                        }
                    }

                    var wordViewModel = new WordControlViewModel(trial.Stimulus.Text, trial.Stimulus.Color);
                    CurrentStepViewModel = wordViewModel;

                    await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (!_isDisposed)
                        {
                            _responseTime.Restart();
                            _wordTimer.Restart();
                            _inputTcs = new TaskCompletionSource<double>();
                        }
                    }), DispatcherPriority.Render);

                    if (Settings.ExperimentContext.IsTaskStopped || _cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        HandleTaskStopped();
                        return;
                    }

                    var delayTask = Task.Delay(Settings.CurrentProfile.MaxReactionTime, _cancellationTokenSource.Token);
                    var completed = await Task.WhenAny(_inputTcs.Task, delayTask);

                    if (completed == _inputTcs.Task && !_inputTcs.Task.IsCanceled)
                    {
                        _responseTime.Stop();
                        trial.ReactionTime = _inputTcs.Task.Result;
                        Settings.ExperimentContext.CurrentBlock.TrialTimes.Add(trial.ReactionTime);
                        Settings.ExperimentContext.ReactionPoints.Add(new ReactionTimePoint(trial.TrialNumber, trial.ReactionTime, trial.IsValidResponse));
                        CurrentStepViewModel = new FixationCrossViewModel();
                    }
                    else if (!delayTask.IsCanceled)
                    {
                        _responseTime.Stop();
                        _inputTcs.TrySetCanceled();
                        trial.IsValidResponse = false;
                        Settings.ExperimentContext.CurrentBlock.TrialTimes.Add(null);
                        Settings.ExperimentContext.ReactionPoints.Add(new ReactionTimePoint(trial.TrialNumber, double.NaN, trial.IsValidResponse));
                    }

                    if (Settings.ExperimentContext.IsTaskStopped || _cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        HandleTaskStopped();
                        return;
                    }

                    double remaining = Settings.CurrentProfile.MaxReactionTime - _wordTimer.Elapsed.TotalMilliseconds;
                    if (remaining > 0)
                        await Task.Delay((int)remaining, _cancellationTokenSource.Token);

                    _wordTimer.Stop();
                }

                EndBlock();
            }
            catch (OperationCanceledException)
            {
                HandleTaskStopped();
            }
        }
        public void StopTask()
        {
            Settings.ExperimentContext.IsTaskStopped = true;
            _cancellationTokenSource?.Cancel();
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                StopTask();
                _cancellationTokenSource?.Dispose();
            }
        }
        private void HandleTaskStopped()
        {
            _responseTime.Stop();
            _wordTimer.Stop();
            _inputTcs?.TrySetCanceled();

            if (Settings.ExperimentContext.CurrentBlock != null)
            {
                Settings.ExperimentContext.CurrentBlock.CalculateValues();
            }

            Settings.ExperimentContext.CurrentTrial = null;
            Settings.ExperimentContext.IsBlockFinished = true;
        }
        public void EndBlock()
        {
            if (Settings.ExperimentContext.CurrentBlock is null)
                return;

            Settings.ExperimentContext.CurrentBlock.CalculateValues();
            Settings.ExperimentContext.CurrentTrial = null;
            Settings.ExperimentContext.IsBlockFinished = true;
        }

        /// <summary>
        /// Processes keyboard input and maps keys to color responses based on current key mappings.
        /// Validates response correctness and records reaction time.
        /// </summary>
        public void ProcessInput(Key key)
        {
            if (_inputTcs == null || _inputTcs.Task.IsCompleted)
                return;

            string? answer = key == Settings.KeyMappings.Stroop.Red.Key ? Settings.KeyMappings.Stroop.Red.Color
                           : key == Settings.KeyMappings.Stroop.Blue.Key ? Settings.KeyMappings.Stroop.Blue.Color
                           : key == Settings.KeyMappings.Stroop.Green.Key ? Settings.KeyMappings.Stroop.Green.Color
                           : key == Settings.KeyMappings.Stroop.Yellow.Key ? Settings.KeyMappings.Stroop. Yellow.Color
                           : null;

            if (answer != null)
            {
                var trial = Settings.ExperimentContext.CurrentTrial as StroopTrial;
                if (trial is null)
                    return;

                trial.GivenAnswer = answer;
                trial.IsValidResponse = string.Equals(trial.ExpectedAnswer, answer, StringComparison.OrdinalIgnoreCase);
                _inputTcs.TrySetResult(_responseTime.Elapsed.TotalMilliseconds);
            }
        }
    }
}
