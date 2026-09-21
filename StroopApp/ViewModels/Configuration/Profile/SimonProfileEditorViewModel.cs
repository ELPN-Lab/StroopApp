using CommunityToolkit.Mvvm.ComponentModel;
using StroopApp.Models;
using StroopApp.Models.Simon;
using StroopApp.Resources;
using StroopApp.Services.Profile;
using System.Collections.ObjectModel;

namespace StroopApp.ViewModels.Configuration.Profile
{
    public partial class SimonProfileEditorViewModel : ProfileEditorViewModelBase
    {

        public List<EnumOption<SimonStimulusMode>> StimulusModes { get; } =
            [
            new () { Value= SimonStimulusMode.Color, DisplayName= Strings.SimonStimulusMode_Color},
            new () { Value= SimonStimulusMode.Shape, DisplayName= Strings.SimonStimulusMode_Shape},
            new () { Value= SimonStimulusMode.Arrow, DisplayName= Strings.SimonStimulusMode_Arrow},
            ];
        public List<EnumOption<SimonStimulusPositionMode>> StimulusPositionModes { get; } =
            [
            new () { Value= SimonStimulusPositionMode.LeftRight, DisplayName= Strings.SimonStimulusPositionMode_LeftRight},
            new () { Value= SimonStimulusPositionMode.Center, DisplayName= Strings.SimonStimulusPositionMode_Center},
            ];
        public List<EnumOption<SimonAnswerMode>> AnswerModes { get; } =
            [
            new () { Value= SimonAnswerMode.GoNoGo, DisplayName= Strings.SimonAnswerMode_GoNoGo},
            new () { Value= SimonAnswerMode.LeftRight, DisplayName= Strings.SimonAnswerMode_LeftRight},
            ];

        public List<EnumOption<SimonStimulusShape>> StimulusShapes { get; } =
            [
                new () { Value= SimonStimulusShape.Circle, DisplayName= Strings.SimonStimulusShape_Circle},
                new () { Value= SimonStimulusShape.Square, DisplayName= Strings.SimonStimulusShape_Square},
                new () { Value= SimonStimulusShape.Triangle, DisplayName= Strings.SimonStimulusShape_Triangle},
            ];
        public List<EnumOption<SimonStimulusShape>> CueShapes { get; } =
            [
                new () { Value= SimonStimulusShape.Circle, DisplayName= Strings.SimonStimulusShape_Circle},
                new () { Value= SimonStimulusShape.Square, DisplayName= Strings.SimonStimulusShape_Square},
            ];

        public List<ColorOption> StimulusColors { get; } =
            [
                new () { Hex= "#FF0000", DisplayName= Strings.StimulusColor_Red},
                new () { Hex= "#00FF00", DisplayName= Strings.StimulusColor_Green},
                new () { Hex= "#0000FF", DisplayName= Strings.StimulusColor_Blue},
                new () { Hex= "#0072B2", DisplayName= Strings.StimulusColor_BlueWong},
                new () { Hex= "#E69F00", DisplayName= Strings.StimulusColor_OrangeWong},
                new () { Hex= "#FF00F7", DisplayName= Strings.StimulusColor_Pink},
                new () { Hex= "#FFFFFF", DisplayName= Strings.StimulusColor_White},
            ];

        public List<EnumOption<ReversalCuePresentation>> ReversalCuePresentations { get; } =
            [
                new () { Value= ReversalCuePresentation.Before, DisplayName= Strings.SimonReversalCuePresentation_Before},
                new () { Value= ReversalCuePresentation.Around, DisplayName= Strings.SimonReversalCuePresentation_Around},
                new () { Value= ReversalCuePresentation.Integrated, DisplayName= Strings.SimonReversalCuePresentation_Integrated},
                new () { Value= ReversalCuePresentation.None, DisplayName= Strings.SimonReversalCuePresentation_None},
            ];

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsStimulusModeShape))]
        [NotifyPropertyChangedFor(nameof(IsStimulusModeColor))]
        [NotifyPropertyChangedFor(nameof(IsReversalCueModalityShape))]
        [NotifyPropertyChangedFor(nameof(IsReversalCueModalityColor))]
        [NotifyPropertyChangedFor(nameof(IsStimulusModeShapeOrArrow))]
        [NotifyPropertyChangedFor(nameof(IsBaseColorUsed))]
        [NotifyPropertyChangedFor(nameof(IsBaseShapeUsed))]
        private EnumOption<SimonStimulusMode> _selectedStimulusMode;
        
        [ObservableProperty]
        private EnumOption<SimonStimulusPositionMode> _selectedStimulusPositionMode;

        [ObservableProperty]
        private EnumOption<SimonAnswerMode> _selectedAnswerMode;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsReversalCuePresentationBefore))]
        [NotifyPropertyChangedFor(nameof(ShowReversalSettings))]
        [NotifyPropertyChangedFor(nameof(IsReversalCueModalityShape))]
        [NotifyPropertyChangedFor(nameof(IsReversalCueModalityColor))]
        [NotifyPropertyChangedFor(nameof(IsReversalCuePresentationNotIntegrated))]
        [NotifyPropertyChangedFor(nameof(IsReversalCuePresentationIntegrated))]
        [NotifyPropertyChangedFor(nameof(IsBaseColorUsed))]
        [NotifyPropertyChangedFor(nameof(IsBaseShapeUsed))]
        private EnumOption<ReversalCuePresentation> _selectedReversalCuePresentation;

        partial void OnSelectedReversalCuePresentationChanged(EnumOption<ReversalCuePresentation> value)
        {
            if (_isUpdating) return;
            if (value?.Value != ReversalCuePresentation.Before)
                VisualCueDuration = 0;
        }

        [ObservableProperty]
        private ColorOption _selectedBaseColor;
        [ObservableProperty]
        private EnumOption<SimonStimulusShape> _selectedBaseShape;

        [ObservableProperty]
        private EnumOption<SimonStimulusShape> _selectedLeftShape;
        [ObservableProperty]
        private EnumOption<SimonStimulusShape> _selectedRightShape;
        [ObservableProperty]
        private ColorOption _selectedLeftColor;
        [ObservableProperty]
        private ColorOption _selectedRightColor;
        
        [ObservableProperty]
        private EnumOption<SimonStimulusShape> _selectedStandardShape;
        [ObservableProperty]
        private EnumOption<SimonStimulusShape> _selectedReversedShape;
        [ObservableProperty]
        private ColorOption _selectedStandardColor;
        [ObservableProperty]
        private ColorOption _selectedReversedColor;

        [ObservableProperty]
        private int _reversedMappingPercent;
        [ObservableProperty]
        private int _visualCueDuration;

        public bool IsStimulusModeShape => SelectedStimulusMode?.Value == SimonStimulusMode.Shape;
        public bool IsStimulusModeColor => SelectedStimulusMode?.Value == SimonStimulusMode.Color;
        public bool IsStimulusModeShapeOrArrow => !IsStimulusModeColor;
        public bool IsBaseShapeUsed => IsStimulusModeColor && IsReversalCuePresentationNotIntegrated;
        public bool IsBaseColorUsed => (!IsStimulusModeColor) && IsReversalCuePresentationNotIntegrated;
        public bool IsReversalCuePresentationBefore => SelectedReversalCuePresentation?.Value == ReversalCuePresentation.Before;
        public bool ShowReversalSettings => SelectedReversalCuePresentation?.Value is not null and not ReversalCuePresentation.None;
        public bool IsReversalCuePresentationNotIntegrated => !IsReversalCuePresentationIntegrated;
        public bool IsReversalCuePresentationIntegrated => SelectedReversalCuePresentation?.Value is ReversalCuePresentation.Integrated;

        public bool IsReversalCueModalityShape => !IsReversalCueModalityColor;

        public bool IsReversalCueModalityColor =>
                    SelectedReversalCuePresentation?.Value == ReversalCuePresentation.Integrated
                    && (SelectedStimulusMode?.Value is SimonStimulusMode.Shape or SimonStimulusMode.Arrow);

        

        protected override void InitializeTaskSpecificFromProfile()
        {
            var profile = (SimonProfile)Profile;
            SelectedStimulusMode = StimulusModes.First(stimulusMode => stimulusMode.Value == profile.StimulusMode);
            SelectedStimulusPositionMode = StimulusPositionModes.First(positionMode => positionMode.Value == profile.StimulusPositionMode);
            SelectedAnswerMode = AnswerModes.First(answer => answer.Value == profile.AnswerMode);
            SelectedReversalCuePresentation = ReversalCuePresentations.First(cuePresentation => cuePresentation.Value == profile.ReversalCuePresentation);
            
            SelectedBaseShape = StimulusShapes.First(stimulusShape => stimulusShape.Value == profile.BaseShape);
            SelectedBaseColor = StimulusColors.First(stimulusColor => stimulusColor.Hex == profile.BaseColor);

            SelectedLeftShape = StimulusShapes.First(stimulusShape => stimulusShape.Value == profile.LeftShape);
            SelectedRightShape = StimulusShapes.First(stimulusShape => stimulusShape.Value == profile.RightShape);
            SelectedStandardShape = CueShapes.First(stimulusShape => stimulusShape.Value == profile.StandardShape);
            SelectedReversedShape = CueShapes.First(stimulusShape => stimulusShape.Value == profile.ReversedShape);
            
            SelectedLeftColor = StimulusColors.First(stimulusColor => stimulusColor.Hex == profile.LeftColor);
            SelectedRightColor = StimulusColors.First(stimulusColor => stimulusColor.Hex == profile.RightColor);
            SelectedStandardColor = StimulusColors.First(stimulusColor => stimulusColor.Hex == profile.StandardColor);
            SelectedReversedColor = StimulusColors.First(stimulusColor => stimulusColor.Hex == profile.ReversedColor);
            
            ReversedMappingPercent = profile.ReversedMappingPercent;
            VisualCueDuration = profile.VisualCueDuration;
        }

        protected override void SyncTaskSpecificToProfile()
        {
            var profile = (SimonProfile)Profile;
            profile.StimulusMode = SelectedStimulusMode.Value;
            profile.StimulusPositionMode = SelectedStimulusPositionMode.Value;
            profile.AnswerMode = SelectedAnswerMode.Value;
            profile.ReversalCuePresentation = SelectedReversalCuePresentation.Value;
            profile.BaseColor = SelectedBaseColor.Hex;
            profile.BaseShape = SelectedBaseShape.Value;
            profile.LeftShape = SelectedLeftShape.Value;
            profile.RightShape = SelectedRightShape.Value;
            profile.LeftColor = SelectedLeftColor.Hex;
            profile.RightColor = SelectedRightColor.Hex;
            profile.StandardShape = SelectedStandardShape.Value;
            profile.ReversedShape = SelectedReversedShape.Value;
            profile.StandardColor = SelectedStandardColor.Hex;
            profile.ReversedColor = SelectedReversedColor.Hex;
            profile.ReversedMappingPercent = ReversedMappingPercent;
            profile.VisualCueDuration = IsReversalCuePresentationBefore ? VisualCueDuration : 0;
        }
        public SimonProfileEditorViewModel(SimonProfile profile, ObservableCollection<ExperimentProfile> profiles, IProfileService profileService) : base(profile, profiles, profileService)
        { }
    }   
}
