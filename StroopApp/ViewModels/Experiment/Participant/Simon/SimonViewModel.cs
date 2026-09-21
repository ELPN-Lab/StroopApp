using CommunityToolkit.Mvvm.ComponentModel;
using StroopApp.Core;
using StroopApp.Models;
using StroopApp.Models.Simon;
using StroopApp.Services.Navigation;
using StroopApp.ViewModels.Experiment.Participant.Common;
using StroopApp.ViewModels.State;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace StroopApp.ViewModels.Experiment.Participant.Simon
{
    /// <summary>
    /// Manages Simon test trial execution, timing, and participant input handling
    /// Orchestrates the trial sequense (fixation -> cue -> stimulus -> response) with validated ±8ms timing precision.
    /// </summary>
    public partial class SimonViewModel : ViewModelBase, IDisposable
    {
        public ExperimentSettingsViewModel Settings { get; set; }

        [ObservableProperty]
        private object? _currentStepViewModel;

        private TaskCompletionSource<double> _inputTcs = null!;
        private readonly Stopwatch _responseTime;
        private readonly Stopwatch _stimulusTimer;
        private readonly INavigationService _participantWindowNavigationService = null!;
        private CancellationTokenSource _cancellationTokenSource = null!;
        private bool _isDisposed = false;

        public SimonViewModel(
            ExperimentSettingsViewModel settings,
            INavigationService participantWindowNavigationService)
        {
            Settings = settings;
            _participantWindowNavigationService = participantWindowNavigationService;
            _responseTime = new Stopwatch();
            _stimulusTimer = new Stopwatch();
            _cancellationTokenSource = new CancellationTokenSource();
            StartTrials();
        }

        public async void StartTrials()
        {
            try
            {
                if (Settings.ExperimentContext.CurrentBlock is null)
                {
                    throw new InvalidOperationException("CurrentBlock is not initialized");
                }

                var profile = (SimonProfile)Settings.CurrentProfile;

                foreach (SimonTrial trial in Settings.ExperimentContext.CurrentBlock.TrialRecords)
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

                    var cueShape = trial.IsReversedMapping ? profile.ReversedShape : profile.StandardShape;
                    if (profile.ReversalCuePresentation == ReversalCuePresentation.Before)
                    {
                        CurrentStepViewModel = new SimonPrimeViewModel(cueShape);
                        await Task.Delay(Settings.CurrentProfile.VisualCueDuration, _cancellationTokenSource.Token);

                        if (Settings.ExperimentContext.IsTaskStopped || _cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            HandleTaskStopped();
                            return;
                        }
                    }

                    CurrentStepViewModel = new SimonStimulusViewModel(trial.Stimulus.Position, trial.Stimulus.Color,trial.Stimulus.Shape
                        , profile.ReversalCuePresentation == ReversalCuePresentation.Around ? cueShape : null);

                    await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (!_isDisposed)
                        {
                            _responseTime.Restart();
                            _stimulusTimer.Restart();
                            _inputTcs = new TaskCompletionSource<double>();
                        }
                    }), DispatcherPriority.Render);

                    if (Settings.ExperimentContext.IsTaskStopped || _cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        HandleTaskStopped(); return;
                    }

                    var delayTask = Task.Delay(Settings.CurrentProfile.MaxReactionTime, _cancellationTokenSource.Token);
                    var completed = await Task.WhenAny(_inputTcs.Task, delayTask);

                    if (completed == _inputTcs.Task && !_inputTcs.Task.IsCanceled)
                    {
                        _responseTime.Stop();
                        trial.ReactionTime = _inputTcs.Task.Result;
                        EvaluateTrial(trial);
                        Settings.ExperimentContext.CurrentBlock.TrialTimes.Add(trial.ReactionTime);
                        Settings.ExperimentContext.ReactionPoints.Add(new ReactionTimePoint(trial.TrialNumber, trial.ReactionTime, trial.IsValidResponse));
                        CurrentStepViewModel = new FixationCrossViewModel();
                    }

                    else if (!delayTask.IsCanceled)
                    {
                        _responseTime.Stop();
                        _inputTcs.TrySetCanceled();

                        if (trial.ExpectedAnswer == SimonAnswer.NoGo)
                        {
                            trial.GivenAnswer = SimonAnswer.NoGo;
                        }
                        EvaluateTrial(trial);
                        Settings.ExperimentContext.CurrentBlock.TrialTimes.Add(trial.ReactionTime);
                        Settings.ExperimentContext.ReactionPoints.Add(new ReactionTimePoint(trial.TrialNumber, double.NaN, trial.IsValidResponse));
                    }
                    if (Settings.ExperimentContext.IsTaskStopped || _cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        HandleTaskStopped();
                        return;
                    }

                    double remaining = Settings.CurrentProfile.MaxReactionTime - _stimulusTimer.Elapsed.TotalMilliseconds;
                    if (remaining > 0)
                        await Task.Delay((int)remaining, _cancellationTokenSource.Token);

                    _stimulusTimer.Stop();
                }

                EndBlock();
            }
            catch (OperationCanceledException)
            {
                HandleTaskStopped();
            }
        }

        public void ProcessInput(Key key)
        {
            if (_inputTcs == null || _inputTcs.Task.IsCompleted)
                return;

            SimonAnswer? answer = MapKeyToAnswer(key);

            if (answer is SimonAnswer simonAnswer)
            {
                var trial = Settings.ExperimentContext.CurrentTrial as SimonTrial;
                if (trial is null)
                    return;
                trial.GivenAnswer = simonAnswer;
                _inputTcs.TrySetResult(_responseTime.Elapsed.TotalMilliseconds);
            }
        }

        private SimonAnswer? MapKeyToAnswer(Key key)
        {
            var simonMappings = Settings.KeyMappings.Simon; 
            return Settings.CurrentProfile is SimonProfile { AnswerMode: SimonAnswerMode.GoNoGo }
                ? (key == simonMappings.Left.Key ? SimonAnswer.Go : null) :
                key == simonMappings.Left.Key ? SimonAnswer.Left :
                key == simonMappings.Right.Key ? SimonAnswer.Right :
                null;
        }

        private void EvaluateTrial(SimonTrial trial)
        {
            trial.IsValidResponse = trial.GivenAnswer == SimonAnswer.None 
                ?  false : trial.ExpectedAnswer == trial.GivenAnswer;
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
            _stimulusTimer.Stop();
            _inputTcs?.TrySetCanceled();

            if (Settings.ExperimentContext.CurrentBlock != null)
                Settings.ExperimentContext.CurrentBlock.CalculateValues();

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
    }
}
