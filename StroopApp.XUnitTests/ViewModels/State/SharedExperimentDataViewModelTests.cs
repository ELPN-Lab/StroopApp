using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Moq;
using SkiaSharp;
using StroopApp.Core;
using StroopApp.Models;
using StroopApp.Services.Charts;
using StroopApp.ViewModels.State;
using Xunit;

namespace StroopApp.ViewModels.State.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="SharedExperimentDataViewModel"/> class.
    /// </summary>
    public partial class SharedExperimentDataViewModelTests
    {

        private Block CreateBlock()
        {
            return new Block(new StroopProfile { ProfileName = "TestProfile" }, blockNumber: 1);
        }

        /// <summary>
        /// Tests that BlockSeries property returns the same reference as the underlying model's BlockSeries.
        /// Ensures the property correctly exposes the model's BlockSeries collection.
        /// </summary>
        /// 
        [Fact]
        public void BlockSeries_WhenAccessed_ReturnsSameReferenceAsModel()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);

            // Act
            ObservableCollection<ISeries> result = viewModel.BlockSeries;

            // Assert
            Assert.Same(model.BlockSeries, result);
        }

        /// <summary>
        /// Tests that BlockSeries property returns an empty collection when the model's BlockSeries is empty.
        /// Verifies that the property correctly exposes the initial state of the model's BlockSeries.
        /// </summary>
        [Fact]
        public void BlockSeries_WhenModelHasEmptyCollection_ReturnsEmptyCollection()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);

            // Act
            ObservableCollection<ISeries> result = viewModel.BlockSeries;

            // Assert
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that BlockSeries property returns the collection with items when the model's BlockSeries contains items.
        /// Verifies that the property correctly exposes the model's BlockSeries with its contents.
        /// </summary>
        [Fact]
        public void BlockSeries_WhenModelHasItems_ReturnsCollectionWithItems()
        {
            // Arrange
            var model = new SharedExperimentData();
            var mockSeries = new Moq.Mock<ISeries>();
            model.BlockSeries.Add(mockSeries.Object);
            var viewModel = new SharedExperimentDataViewModel(model);

            // Act
            ObservableCollection<ISeries> result = viewModel.BlockSeries;

            // Assert
            Assert.Single(result);
            Assert.Same(mockSeries.Object, result[0]);
        }

        /// <summary>
        /// Tests that BlockSeries property always returns the same instance on multiple accesses.
        /// Verifies reference stability and that the property doesn't create new instances.
        /// </summary>
        [Fact]
        public void BlockSeries_WhenAccessedMultipleTimes_ReturnsSameInstance()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);

            // Act
            ObservableCollection<ISeries> firstAccess = viewModel.BlockSeries;
            ObservableCollection<ISeries> secondAccess = viewModel.BlockSeries;

            // Assert
            Assert.Same(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that BlockSeries property reflects changes made to the model's BlockSeries after construction.
        /// Verifies that the property is a live reference to the model's collection.
        /// </summary>
        [Fact]
        public void BlockSeries_WhenModelCollectionIsModified_ReflectsChanges()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);
            var mockSeries = new Moq.Mock<ISeries>();

            // Act
            model.BlockSeries.Add(mockSeries.Object);
            ObservableCollection<ISeries> result = viewModel.BlockSeries;

            // Assert
            Assert.Single(result);
            Assert.Same(mockSeries.Object, result[0]);
        }

        /// <summary>
        /// Tests that the Blocks property returns the same ObservableCollection instance as the underlying model's Blocks collection.
        /// </summary>
        [Fact]
        public void Blocks_WhenAccessed_ReturnsSameReferenceAsModel()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);

            // Act
            var result = viewModel.Blocks;

            // Assert
            Assert.Same(model.Blocks, result);
        }

        /// <summary>
        /// Tests that the Blocks property returns a non-null ObservableCollection when the ViewModel is properly initialized.
        /// </summary>
        [Fact]
        public void Blocks_WhenAccessed_ReturnsNonNullCollection()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);

            // Act
            var result = viewModel.Blocks;

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that the Blocks property consistently returns the same reference across multiple accesses.
        /// </summary>
        [Fact]
        public void Blocks_WhenAccessedMultipleTimes_ReturnsConsistentReference()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);

            // Act
            var firstAccess = viewModel.Blocks;
            var secondAccess = viewModel.Blocks;

            // Assert
            Assert.Same(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that the Blocks property returns an empty collection when no blocks have been added to the model.
        /// </summary>
        [Fact]
        public void Blocks_WhenModelHasNoBlocks_ReturnsEmptyCollection()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);

            // Act
            var result = viewModel.Blocks;

            // Assert
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that the Blocks property reflects changes when blocks are added to the underlying model's collection.
        /// </summary>
        [Fact]
        public void Blocks_WhenBlocksAddedToModel_ReflectsChanges()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);
            var block = CreateBlock();

            // Act
            model.Blocks.Add(block);
            var result = viewModel.Blocks;

            // Assert
            Assert.Single(result);
            Assert.Same(block, result[0]);
        }

        /// <summary>
        /// Tests that the ColumnSerie getter returns the value from the underlying model.
        /// </summary>
        [Fact]
        public void ColumnSerie_Get_ReturnsValueFromModel()
        {
            // Arrange
            var model = new SharedExperimentData();
            var expectedCollection = new ObservableCollection<ISeries>();
            var dummySeries = new ColumnSeries<int> { Values = new[] { 1, 2, 3 } };
            expectedCollection.Add(dummySeries);
            model.ColumnSerie = expectedCollection;
            var viewModel = new SharedExperimentDataViewModel(model);

            // Act
            var result = viewModel.ColumnSerie;

            // Assert
            Assert.Same(expectedCollection, result);
        }

        /// <summary>
        /// Tests that setting ColumnSerie with a different value updates the model and raises PropertyChanged event.
        /// </summary>
        [Fact]
        public void ColumnSerie_SetWithDifferentValue_UpdatesModelAndRaisesPropertyChanged()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);
            var newCollection = new ObservableCollection<ISeries>();
            var propertyChangedRaised = false;
            string? changedPropertyName = null;

            viewModel.PropertyChanged += (sender, args) =>
            {
                propertyChangedRaised = true;
                changedPropertyName = args.PropertyName;
            };

            // Act
            viewModel.ColumnSerie = newCollection;

            // Assert
            Assert.Same(newCollection, model.ColumnSerie);
            Assert.Same(newCollection, viewModel.ColumnSerie);
            Assert.True(propertyChangedRaised);
            Assert.Equal("ColumnSerie", changedPropertyName);
        }

        /// <summary>
        /// Tests that setting ColumnSerie with the same reference does not raise PropertyChanged event.
        /// </summary>
        [Fact]
        public void ColumnSerie_SetWithSameReference_DoesNotRaisePropertyChanged()
        {
            // Arrange
            var model = new SharedExperimentData();
            var existingCollection = new ObservableCollection<ISeries>();
            // Add a mock item to prevent the constructor from replacing the collection
            existingCollection.Add(Mock.Of<ISeries>());
            model.ColumnSerie = existingCollection;
            var viewModel = new SharedExperimentDataViewModel(model);
            var propertyChangedRaised = false;

            viewModel.PropertyChanged += (sender, args) =>
            {
                propertyChangedRaised = true;
            };

            // Act
            viewModel.ColumnSerie = existingCollection;

            // Assert
            Assert.False(propertyChangedRaised);
            Assert.Same(existingCollection, model.ColumnSerie);
        }

        /// <summary>
        /// Tests that setting ColumnSerie to null updates the model and raises PropertyChanged event.
        /// </summary>
        [Fact]
        public void ColumnSerie_SetToNull_UpdatesModelAndRaisesPropertyChanged()
        {
            // Arrange
            var model = new SharedExperimentData();
            var initialCollection = new ObservableCollection<ISeries>();
            model.ColumnSerie = initialCollection;
            var viewModel = new SharedExperimentDataViewModel(model);
            var propertyChangedRaised = false;
            string? changedPropertyName = null;

            viewModel.PropertyChanged += (sender, args) =>
            {
                propertyChangedRaised = true;
                changedPropertyName = args.PropertyName;
            };

            // Act
            viewModel.ColumnSerie = null;

            // Assert
            Assert.Null(model.ColumnSerie);
            Assert.Null(viewModel.ColumnSerie);
            Assert.True(propertyChangedRaised);
            Assert.Equal("ColumnSerie", changedPropertyName);
        }

        /// <summary>
        /// Tests that setting ColumnSerie from null to a non-null value updates the model and raises PropertyChanged event.
        /// </summary>
        [Fact]
        public void ColumnSerie_SetFromNullToValue_UpdatesModelAndRaisesPropertyChanged()
        {
            // Arrange
            var model = new SharedExperimentData();
            model.ColumnSerie = null;
            var viewModel = new SharedExperimentDataViewModel(model);
            var newCollection = new ObservableCollection<ISeries>();
            var propertyChangedRaised = false;
            string? changedPropertyName = null;

            viewModel.PropertyChanged += (sender, args) =>
            {
                propertyChangedRaised = true;
                changedPropertyName = args.PropertyName;
            };

            // Act
            viewModel.ColumnSerie = newCollection;

            // Assert
            Assert.Same(newCollection, model.ColumnSerie);
            Assert.Same(newCollection, viewModel.ColumnSerie);
            Assert.True(propertyChangedRaised);
            Assert.Equal("ColumnSerie", changedPropertyName);
        }

        /// <summary>
        /// Tests that setting ColumnSerie to null when already null does not raise PropertyChanged event.
        /// </summary>
        [Fact]
        public void ColumnSerie_SetToNullWhenAlreadyNull_DoesNotRaisePropertyChanged()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);
            viewModel.ColumnSerie = null; // Ensure it's null first
            var propertyChangedRaised = false;

            viewModel.PropertyChanged += (sender, args) =>
            {
                propertyChangedRaised = true;
            };

            // Act
            viewModel.ColumnSerie = null;

            // Assert
            Assert.False(propertyChangedRaised);
            Assert.Null(model.ColumnSerie);
        }

        /// <summary>
        /// Verifies that the constructor throws an exception when the model parameter is null.
        /// </summary>
        [Fact]
        public void Constructor_WithNullModel_ThrowsNullReferenceException()
        {
            // Arrange
            SharedExperimentData? model = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => new SharedExperimentDataViewModel(model!));
        }

        /// <summary>
        /// Verifies that the constructor initializes ColumnSerie when the model's ColumnSerie is null.
        /// </summary>
        [Fact]
        public void Constructor_WithNullColumnSerie_InitializesColumnSerie()
        {
            // Arrange
            var model = new SharedExperimentData
            {
                ColumnSerie = null
            };

            // Act
            var viewModel = new SharedExperimentDataViewModel(model);

            // Assert
            Assert.NotNull(viewModel.ColumnSerie);
            Assert.NotNull(model.ColumnSerie);
        }

        /// <summary>
        /// Verifies that the constructor initializes ColumnSerie when the model's ColumnSerie is empty.
        /// </summary>
        [Fact]
        public void Constructor_WithEmptyColumnSerie_InitializesColumnSerie()
        {
            // Arrange
            var model = new SharedExperimentData();
            model.ColumnSerie!.Clear();

            // Act
            var viewModel = new SharedExperimentDataViewModel(model);

            // Assert
            Assert.NotNull(viewModel.ColumnSerie);
            Assert.Single(viewModel.ColumnSerie);
        }

        /// <summary>
        /// Verifies that the constructor does not reinitialize ColumnSerie when it already contains items.
        /// </summary>
        [Fact]
        public void Constructor_WithPopulatedColumnSerie_DoesNotReinitializeColumnSerie()
        {
            // Arrange
            var model = new SharedExperimentData();
            var existingSeries = Mock.Of<ISeries>();
            model.ColumnSerie!.Add(existingSeries);
            var originalColumnSerie = model.ColumnSerie;

            // Act
            var viewModel = new SharedExperimentDataViewModel(model);

            // Assert
            Assert.Same(originalColumnSerie, viewModel.ColumnSerie);
            Assert.Single(viewModel.ColumnSerie);
            Assert.Same(existingSeries, viewModel.ColumnSerie[0]);
        }

        /// <summary>
        /// Verifies that the constructor initializes all boolean properties to their default values
        /// when the model uses default settings.
        /// </summary>
        [Fact]
        public void Constructor_WithDefaultModel_InitializesDefaultBooleanProperties()
        {
            // Arrange
            var model = new SharedExperimentData();

            // Act
            var viewModel = new SharedExperimentDataViewModel(model);

            // Assert
            Assert.False(viewModel.IsBlockFinished);
            Assert.False(viewModel.IsTaskStopped);
            Assert.True(viewModel.IsParticipantSelectionEnabled);
            Assert.True(viewModel.HasUnsavedExports);
        }

        /// <summary>
        /// Verifies that the constructor initializes nullable properties to null when the model has null values.
        /// </summary>
        [Fact]
        public void Constructor_WithNullBlockAndTrial_InitializesNullablePropertiesToNull()
        {
            // Arrange
            var model = new SharedExperimentData
            {
                CurrentBlock = null,
                CurrentTrial = null
            };
            model.ColumnSerie!.Add(Mock.Of<ISeries>());

            // Act
            var viewModel = new SharedExperimentDataViewModel(model);

            // Assert
            Assert.Null(viewModel.CurrentBlock);
            Assert.Null(viewModel.CurrentTrial);
        }

        /// <summary>
        /// Verifies that the ReactionPoints property returns the model's ReactionPoints collection.
        /// </summary>
        [Fact]
        public void ReactionPoints_ReturnsModelReactionPoints()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);

            // Act
            var reactionPoints = viewModel.ReactionPoints;

            // Assert
            Assert.Same(model.ReactionPoints, reactionPoints);
        }

        /// <summary>
        /// Verifies that the BlockSeries property returns the model's BlockSeries collection.
        /// </summary>
        [Fact]
        public void BlockSeries_ReturnsModelBlockSeries()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);

            // Act
            var blockSeries = viewModel.BlockSeries;

            // Assert
            Assert.Same(model.BlockSeries, blockSeries);
        }

        /// <summary>
        /// Verifies that the Sections property returns the model's Sections collection.
        /// </summary>
        [Fact]
        public void Sections_ReturnsModelSections()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);

            // Act
            var sections = viewModel.Sections;

            // Assert
            Assert.Same(model.Sections, sections);
        }

        /// <summary>
        /// Tests that AddNewSerie throws ArgumentNullException when settings parameter is null.
        /// </summary>
        [Fact]
        public void AddNewSerie_NullSettings_ThrowsArgumentNullException()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => viewModel.AddNewSerie(null!));
            Assert.Equal("settings", exception.ParamName);
        }

        /// <summary>
        /// Tests that AddNewSerie correctly adds a new block, series, and section with valid settings.
        /// Verifies that CurrentBlock is set, collections are updated, and model properties are modified.
        /// </summary>
        [Fact]
        public void AddNewSerie_ValidSettings_AddsBlockSeriesAndSection()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);

            var profile = new StroopProfile
            {
                WordCount = 10,
                ProfileName = "TestProfile",
                CongruencePercent = 50,
                SwitchPercent = 30,
                HasVisualCue = true
            };

            var settings = new ExperimentSettings();
            var settingsViewModel = new ExperimentSettingsViewModel(settings);
            settingsViewModel.CurrentProfile = profile;
            settingsViewModel.Block = 1;

            int initialBlocksCount = viewModel.Blocks.Count;
            int initialBlockSeriesCount = viewModel.BlockSeries.Count;
            int initialSectionsCount = viewModel.Sections.Count;

            // Act
            viewModel.AddNewSerie(settingsViewModel);

            // Assert
            Assert.NotNull(viewModel.CurrentBlock);
            Assert.Equal(initialBlocksCount + 1, viewModel.Blocks.Count);
            Assert.Equal(initialBlockSeriesCount + 1, viewModel.BlockSeries.Count);
            Assert.Equal(initialSectionsCount + 1, viewModel.Sections.Count);
            Assert.Equal(1, model.ColorIndex);
            Assert.Equal(11, model.CurrentBlockStart);
            Assert.Equal(10, model.CurrentBlockEnd);
        }

        /// <summary>
        /// Tests that AddNewSerie adds the correct Block to the Blocks collection.
        /// Verifies Block properties are set from settings.
        /// </summary>
        [Fact]
        public void AddNewSerie_ValidSettings_AddsCorrectBlockToCollection()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);

            var profile = new StroopProfile
            {
                WordCount = 15,
                ProfileName = "TestProfile",
                CongruencePercent = 70,
                SwitchPercent = 40,
                HasVisualCue = true
            };

            var experimentSettings = new ExperimentSettings();
            var settingsViewModel = new ExperimentSettingsViewModel(experimentSettings);
            settingsViewModel.CurrentProfile = profile;
            settingsViewModel.Block = 3;

            // Act
            viewModel.AddNewSerie(settingsViewModel);

            // Assert
            Assert.Single(viewModel.Blocks);
            var addedBlock = viewModel.Blocks[0];
            Assert.Equal("TestProfile", addedBlock.Profile.ProfileName);
            Assert.Equal(3, addedBlock.BlockNumber);
            Assert.Equal(70, addedBlock.Profile.CongruencePercent);
            Assert.Equal(40, addedBlock.Profile.SwitchPercent);
            Assert.True(addedBlock.Profile.HasVisualCue);
            Assert.NotSame(profile, addedBlock.Profile);
        }

        /// <summary>
        /// Verifies that the Reset method raises PropertyChanged events for all required properties:
        /// Blocks, BlockSeries, Sections, ReactionPoints, and ColumnSerie.
        /// </summary>
        [Fact]
        public void Reset_RaisesPropertyChangedForAllRequiredProperties()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);
            var raisedProperties = new System.Collections.Generic.List<string>();

            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != null)
                {
                    raisedProperties.Add(args.PropertyName);
                }
            };

            // Act
            viewModel.Reset();

            // Assert
            Assert.Contains(nameof(viewModel.Blocks), raisedProperties);
            Assert.Contains(nameof(viewModel.BlockSeries), raisedProperties);
            Assert.Contains(nameof(viewModel.Sections), raisedProperties);
            Assert.Contains(nameof(viewModel.ReactionPoints), raisedProperties);
            Assert.Contains(nameof(viewModel.ColumnSerie), raisedProperties);
        }

        /// <summary>
        /// Verifies that the Reset method recreates the ColumnSerie property.
        /// The ColumnSerie should not be null after Reset is called.
        /// </summary>
        [Fact]
        public void Reset_RecreatesColumnSerie()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);

            // Act
            viewModel.Reset();

            // Assert
            Assert.NotNull(viewModel.ColumnSerie);
        }

        /// <summary>
        /// Verifies that the Reset method refreshes ViewModel properties from the model.
        /// After reset, the ViewModel properties should match the model's reset state.
        /// </summary>
        [Fact]
        public void Reset_RefreshesPropertiesFromModel()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);

            // Modify state before reset
            viewModel.IsBlockFinished = true;
            viewModel.IsTaskStopped = true;

            // Act
            viewModel.Reset();

            // Assert
            Assert.False(viewModel.IsTaskStopped);
            Assert.False(viewModel.IsBlockFinished);
            Assert.Null(viewModel.CurrentBlock);
            Assert.Null(viewModel.CurrentTrial);
        }

        /// <summary>
        /// Verifies that Reset works correctly when the model is already in its initial state.
        /// Should still raise all PropertyChanged events even when no data needs to be cleared.
        /// </summary>
        [Fact]
        public void Reset_WhenAlreadyInInitialState_StillRaisesEvents()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);
            var raisedProperties = new System.Collections.Generic.List<string>();

            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != null)
                {
                    raisedProperties.Add(args.PropertyName);
                }
            };

            // Act
            viewModel.Reset();

            // Assert
            Assert.Contains(nameof(viewModel.Blocks), raisedProperties);
            Assert.Contains(nameof(viewModel.BlockSeries), raisedProperties);
            Assert.Contains(nameof(viewModel.Sections), raisedProperties);
            Assert.Contains(nameof(viewModel.ReactionPoints), raisedProperties);
            Assert.Contains(nameof(viewModel.ColumnSerie), raisedProperties);
        }

        /// <summary>
        /// Verifies that Reset properly handles the ColumnSerie property when it already exists.
        /// After reset, ColumnSerie should be recreated and not be the same instance.
        /// </summary>
        [Fact]
        public void Reset_WhenColumnSerieExists_RecreatesIt()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);
            var originalColumnSerie = viewModel.ColumnSerie;

            // Act
            viewModel.Reset();

            // Assert
            Assert.NotNull(viewModel.ColumnSerie);
            Assert.NotSame(originalColumnSerie, viewModel.ColumnSerie);
        }

        /// <summary>
        /// Verifies that Reset synchronizes the HasUnsavedExports property from the model.
        /// After reset, HasUnsavedExports should be set to true per the model's Reset behavior.
        /// </summary>
        [Fact]
        public void Reset_SetsHasUnsavedExportsToTrue()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);
            viewModel.HasUnsavedExports = false;

            // Act
            viewModel.Reset();

            // Assert
            Assert.True(viewModel.HasUnsavedExports);
        }

        /// <summary>
        /// Verifies that Reset clears the CurrentBlock property to null.
        /// </summary>
        [Fact]
        public void Reset_ClearsCurrentBlock()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);
            var block = CreateBlock();

            // Act
            viewModel.Reset();

            // Assert
            Assert.Null(viewModel.CurrentBlock);
        }

        /// <summary>
        /// Verifies that Reset clears the CurrentTrial property to null.
        /// </summary>
        [Fact]
        public void Reset_ClearsCurrentTrial()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);
            viewModel.CurrentTrial = new StroopTrial();

            // Act
            viewModel.Reset();

            // Assert
            Assert.Null(viewModel.CurrentTrial);
        }

        /// <summary>
        /// Tests that RefreshFromModel correctly syncs properties when nullable reference type properties
        /// (CurrentBlock and CurrentTrial) are null.
        /// </summary>
        [Fact]
        public void RefreshFromModel_WithNullablePropertiesNull_UpdatesViewModelWithNullValues()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);

            model.CurrentBlock = null;
            model.CurrentTrial = null;
            model.IsBlockFinished = false;
            model.IsTaskStopped = false;
            model.IsParticipantSelectionEnabled = true;
            model.HasUnsavedExports = false;

            // Act
            viewModel.RefreshFromModel();

            // Assert
            Assert.Null(viewModel.CurrentBlock);
            Assert.Null(viewModel.CurrentTrial);
            Assert.False(viewModel.IsBlockFinished);
            Assert.False(viewModel.IsTaskStopped);
            Assert.True(viewModel.IsParticipantSelectionEnabled);
            Assert.False(viewModel.HasUnsavedExports);
        }

        /// <summary>
        /// Tests that RefreshFromModel correctly updates properties when model state changes
        /// from initial values to different values.
        /// </summary>
        [Fact]
        public void RefreshFromModel_AfterModelStateChanges_ReflectsNewModelState()
        {
            // Arrange
            var model = new SharedExperimentData();
            var block1 = new Block(new StroopProfile
            {
                ProfileName = "TestProfile",
                CongruencePercent = 50,
                SwitchPercent= 30,
                HasVisualCue = true,
            }, blockNumber: 1);

            var trial1 = new StroopTrial();
            model.CurrentBlock = block1;
            model.CurrentTrial = trial1;
            model.IsBlockFinished = true;
            model.IsTaskStopped = false;
            model.IsParticipantSelectionEnabled = false;
            model.HasUnsavedExports = true;

            var viewModel = new SharedExperimentDataViewModel(model);
            var block2 = new Block(
                new StroopProfile
                {
                    ProfileName = "TestProfile",
                    CongruencePercent = 70,
                    SwitchPercent = 40,
                    HasVisualCue = false,
                },
                blockNumber : 2);
            var trial2 = new StroopTrial();

            model.CurrentBlock = block2;
            model.CurrentTrial = trial2;
            model.IsBlockFinished = false;
            model.IsTaskStopped = true;
            model.IsParticipantSelectionEnabled = true;
            model.HasUnsavedExports = false;

            // Act
            viewModel.RefreshFromModel();

            // Assert
            Assert.Equal(block2, viewModel.CurrentBlock);
            Assert.Equal(trial2, viewModel.CurrentTrial);
            Assert.False(viewModel.IsBlockFinished);
            Assert.True(viewModel.IsTaskStopped);
            Assert.True(viewModel.IsParticipantSelectionEnabled);
            Assert.False(viewModel.HasUnsavedExports);
        }

        /// <summary>
        /// Tests that RefreshFromModel updates properties correctly when transitioning
        /// from non-null to null values for nullable properties.
        /// </summary>
        [Fact]
        public void RefreshFromModel_TransitioningToNullValues_UpdatesViewModelCorrectly()
        {
            // Arrange
            var model = new SharedExperimentData();
            var block = CreateBlock();
            var trial = new StroopTrial();

            model.CurrentBlock = block;
            model.CurrentTrial = trial;

            var viewModel = new SharedExperimentDataViewModel(model);

            model.CurrentBlock = null;
            model.CurrentTrial = null;

            // Act
            viewModel.RefreshFromModel();

            // Assert
            Assert.Null(viewModel.CurrentBlock);
            Assert.Null(viewModel.CurrentTrial);
        }

        /// <summary>
        /// Tests that RefreshFromModel correctly syncs all boolean flag combinations.
        /// Verifies each boolean property independently updates to match the model.
        /// </summary>
        [Theory]
        [InlineData(true, true, true, true)]
        [InlineData(false, false, false, false)]
        [InlineData(true, false, true, false)]
        [InlineData(false, true, false, true)]
        [InlineData(true, true, false, false)]
        [InlineData(false, false, true, true)]
        public void RefreshFromModel_WithVariousBooleanCombinations_UpdatesAllBooleanPropertiesCorrectly(
            bool isBlockFinished,
            bool isTaskStopped,
            bool isParticipantSelectionEnabled,
            bool hasUnsavedExports)
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);

            model.IsBlockFinished = isBlockFinished;
            model.IsTaskStopped = isTaskStopped;
            model.IsParticipantSelectionEnabled = isParticipantSelectionEnabled;
            model.HasUnsavedExports = hasUnsavedExports;

            // Act
            viewModel.RefreshFromModel();

            // Assert
            Assert.Equal(isBlockFinished, viewModel.IsBlockFinished);
            Assert.Equal(isTaskStopped, viewModel.IsTaskStopped);
            Assert.Equal(isParticipantSelectionEnabled, viewModel.IsParticipantSelectionEnabled);
            Assert.Equal(hasUnsavedExports, viewModel.HasUnsavedExports);
        }

        /// <summary>
        /// Tests that the Sections property returns the same collection reference
        /// as the underlying model's Sections property.
        /// </summary>
        [Fact]
        public void Sections_WhenAccessed_ReturnsSameReferenceAsModel()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);

            // Act
            var result = viewModel.Sections;

            // Assert
            Assert.Same(model.Sections, result);
        }

        /// <summary>
        /// Tests that the Sections property returns a non-null collection
        /// when the model is initialized with default constructor.
        /// </summary>
        [Fact]
        public void Sections_WhenModelInitializedWithDefaultConstructor_ReturnsNonNullCollection()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);

            // Act
            var result = viewModel.Sections;

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that the Sections property returns an empty collection
        /// when the model is freshly initialized.
        /// </summary>
        [Fact]
        public void Sections_WhenModelFreshlyInitialized_ReturnsEmptyCollection()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);

            // Act
            var result = viewModel.Sections;

            // Assert
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that the Sections property reflects changes made to the model's Sections collection.
        /// </summary>
        [Fact]
        public void Sections_WhenModelSectionsCollectionIsModified_ReflectsChanges()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);
            var section = new RectangularSection();

            // Act
            model.Sections.Add(section);
            var result = viewModel.Sections;

            // Assert
            Assert.Single(result);
            Assert.Same(section, result[0]);
        }

        /// <summary>
        /// Tests that the Sections property returns the updated collection reference
        /// when the model's Sections property is replaced with a new collection.
        /// </summary>
        [Fact]
        public void Sections_WhenModelSectionsPropertyIsReplaced_ReturnsNewCollection()
        {
            // Arrange
            var model = new SharedExperimentData();
            var viewModel = new SharedExperimentDataViewModel(model);
            var newSections = new ObservableCollection<RectangularSection>();
            var section = new RectangularSection();
            newSections.Add(section);

            // Act
            model.Sections = newSections;
            var result = viewModel.Sections;

            // Assert
            Assert.Same(newSections, result);
            Assert.Single(result);
            Assert.Same(section, result[0]);
        }

        /// <summary>
        /// Tests that the Sections property returns a collection with multiple items
        /// when the model's Sections collection contains multiple RectangularSection instances.
        /// </summary>
        [Fact]
        public void Sections_WhenModelContainsMultipleSections_ReturnsCollectionWithAllItems()
        {
            // Arrange
            var model = new SharedExperimentData();
            var section1 = new RectangularSection();
            var section2 = new RectangularSection();
            var section3 = new RectangularSection();
            model.Sections.Add(section1);
            model.Sections.Add(section2);
            model.Sections.Add(section3);
            var viewModel = new SharedExperimentDataViewModel(model);

            // Act
            var result = viewModel.Sections;

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Same(section1, result[0]);
            Assert.Same(section2, result[1]);
            Assert.Same(section3, result[2]);
        }
    }

    /// <summary>
    /// Unit tests for the <see cref="SharedExperimentDataViewModel"/> constructor.
    /// </summary>
    public partial class SharedExperimentDataViewModelConstructorTests
    {
        /// <summary>
        /// Verifies that the constructor correctly initializes the ViewModel with non-default boolean property values.
        /// Tests that all boolean properties set to true in the model are correctly initialized in the ViewModel.
        /// </summary>
        [Fact]
        public void Constructor_WithAllBooleanPropertiesTrue_InitializesPropertiesCorrectly()
        {
            // Arrange
            var model = new SharedExperimentData
            {
                IsBlockFinished = true,
                IsTaskStopped = true,
                IsParticipantSelectionEnabled = true,
                HasUnsavedExports = true
            };

            // Act
            var viewModel = new SharedExperimentDataViewModel(model);

            // Assert
            Assert.True(viewModel.IsBlockFinished);
            Assert.True(viewModel.IsTaskStopped);
            Assert.True(viewModel.IsParticipantSelectionEnabled);
            Assert.True(viewModel.HasUnsavedExports);
        }

        /// <summary>
        /// Verifies that the constructor correctly initializes the ViewModel with mixed boolean property values.
        /// Tests that boolean properties are independently initialized from their model values.
        /// </summary>
        [Fact]
        public void Constructor_WithMixedBooleanProperties_InitializesPropertiesCorrectly()
        {
            // Arrange
            var model = new SharedExperimentData
            {
                IsBlockFinished = true,
                IsTaskStopped = false,
                IsParticipantSelectionEnabled = false,
                HasUnsavedExports = true
            };

            // Act
            var viewModel = new SharedExperimentDataViewModel(model);

            // Assert
            Assert.True(viewModel.IsBlockFinished);
            Assert.False(viewModel.IsTaskStopped);
            Assert.False(viewModel.IsParticipantSelectionEnabled);
            Assert.True(viewModel.HasUnsavedExports);
        }

        /// <summary>
        /// Verifies that the constructor correctly handles the case where CurrentBlock is null but CurrentTrial is non-null.
        /// Tests mixed nullable property initialization.
        /// </summary>
        [Fact]
        public void Constructor_WithNullBlockAndNonNullTrial_InitializesPropertiesCorrectly()
        {
            // Arrange
            var trial = new StroopTrial { TrialNumber = 3 };
            var model = new SharedExperimentData
            {
                CurrentBlock = null,
                CurrentTrial = trial
            };

            // Act
            var viewModel = new SharedExperimentDataViewModel(model);

            // Assert
            Assert.Null(viewModel.CurrentBlock);
            Assert.NotNull(viewModel.CurrentTrial);
            Assert.Same(trial, viewModel.CurrentTrial);
        }

        /// <summary>
        /// Verifies that the constructor correctly exposes all model collections through ViewModel properties.
        /// Tests that Blocks, ReactionPoints, BlockSeries, and Sections properties reference the same collections as the model.
        /// </summary>
        [Fact]
        public void Constructor_InitializesAllCollectionPropertiesFromModel()
        {
            // Arrange
            var model = new SharedExperimentData();

            // Act
            var viewModel = new SharedExperimentDataViewModel(model);

            // Assert
            Assert.Same(model.Blocks, viewModel.Blocks);
            Assert.Same(model.ReactionPoints, viewModel.ReactionPoints);
            Assert.Same(model.BlockSeries, viewModel.BlockSeries);
            Assert.Same(model.Sections, viewModel.Sections);
        }

        /// <summary>
        /// Verifies that the constructor initializes ColumnSerie correctly when the model has a non-empty ColumnSerie
        /// with multiple items. Ensures that no reinitialization occurs and the original collection is preserved.
        /// </summary>
        [Fact]
        public void Constructor_WithMultipleItemsInColumnSerie_PreservesOriginalCollection()
        {
            // Arrange
            var model = new SharedExperimentData();
            var series1 = Mock.Of<ISeries>();
            var series2 = Mock.Of<ISeries>();
            model.ColumnSerie!.Add(series1);
            model.ColumnSerie.Add(series2);
            var originalColumnSerie = model.ColumnSerie;

            // Act
            var viewModel = new SharedExperimentDataViewModel(model);

            // Assert
            Assert.Same(originalColumnSerie, viewModel.ColumnSerie);
            Assert.Equal(2, viewModel.ColumnSerie.Count);
            Assert.Same(series1, viewModel.ColumnSerie[0]);
            Assert.Same(series2, viewModel.ColumnSerie[1]);
        }

        /// <summary>
        /// Verifies that the constructor with ColumnSerie containing a single item preserves the collection
        /// and does not reinitialize it.
        /// </summary>
        [Fact]
        public void Constructor_WithSingleItemInColumnSerie_PreservesCollection()
        {
            // Arrange
            var model = new SharedExperimentData();
            var series = Mock.Of<ISeries>();
            model.ColumnSerie!.Add(series);
            var originalColumnSerie = model.ColumnSerie;
            var originalCount = model.ColumnSerie.Count;

            // Act
            var viewModel = new SharedExperimentDataViewModel(model);

            // Assert
            Assert.Same(originalColumnSerie, viewModel.ColumnSerie);
            Assert.Equal(originalCount, viewModel.ColumnSerie.Count);
            Assert.Single(viewModel.ColumnSerie);
        }

        /// <summary>
        /// Verifies that the constructor initializes ColumnSerie when model's ColumnSerie is null
        /// and updates the model's ColumnSerie property.
        /// </summary>
        [Fact]
        public void Constructor_WithNullColumnSerie_UpdatesModelColumnSerie()
        {
            // Arrange
            var model = new SharedExperimentData
            {
                ColumnSerie = null
            };

            // Act
            var viewModel = new SharedExperimentDataViewModel(model);

            // Assert
            Assert.NotNull(model.ColumnSerie);
            Assert.Same(model.ColumnSerie, viewModel.ColumnSerie);
        }

        /// <summary>
        /// Verifies that when the constructor initializes ColumnSerie from an empty collection,
        /// the resulting ColumnSerie contains exactly one series item (created by NewColumnSerie).
        /// </summary>
        [Fact]
        public void Constructor_WithEmptyColumnSerie_CreatesNewSerieWithOneItem()
        {
            // Arrange
            var model = new SharedExperimentData();
            model.ColumnSerie!.Clear();

            // Act
            var viewModel = new SharedExperimentDataViewModel(model);

            // Assert
            Assert.NotNull(viewModel.ColumnSerie);
            Assert.Single(model.ColumnSerie);
            Assert.Single(viewModel.ColumnSerie);
        }
    }
}