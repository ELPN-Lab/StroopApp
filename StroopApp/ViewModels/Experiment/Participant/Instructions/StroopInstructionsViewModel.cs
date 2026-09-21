using StroopApp.Core;
using StroopApp.Services.Navigation;
using StroopApp.ViewModels.State;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace StroopApp.ViewModels.Experiment.Participant.Instructions
{
    /// <summary>
    /// Manages multilingual instruction pages displayed before Stroop test execution.
    /// Generates dynamic instructions based on experiment configuration (congruence, visual cues).
    /// </summary>
    public partial class StroopInstructionsViewModel : InstructionsViewModelBase
    {
        protected override int TotalPages => 3;

        public StroopInstructionsViewModel(
            ExperimentSettingsViewModel settings,
            INavigationService participantWindowNavigationService,
            Func<Page> nextPageFactory) 
            : base(settings, participantWindowNavigationService, nextPageFactory)
        { }

        
        /// <summary>
        /// Generates localized instruction content based on page index and experiment configuration.
        /// Handles 6 different instruction cases based on congruence percentage and visual cue presence.
        /// </summary>
        protected override UIElement GenerateInstructionPage(int page)
        {
            var originalCulture = Thread.CurrentThread.CurrentCulture;
            var originalUICulture = Thread.CurrentThread.CurrentUICulture;
            try
            {
                var targetCulture = new CultureInfo(_settings.CurrentProfile.TaskLanguage ?? "en");
                Thread.CurrentThread.CurrentCulture = targetCulture;
                Thread.CurrentThread.CurrentUICulture = targetCulture;

                var loc = new LocalizedStrings();
                var profile = _settings.CurrentProfile;
                var congruence = profile.CongruencePercent;
                bool hasCue = profile.HasVisualCue;
                string key = hasCue switch
                {
                    false when congruence == 0 => "Case1",
                    false when congruence == 100 => "Case2",
                    false => "Case3",
                    true when congruence == 0 => "Case4",
                    true when congruence == 100 => "Case5",
                    true => "Case6",
                };

                var tb = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    Background = Brushes.Black,
                    Foreground = Brushes.White,
                    FontSize = 36,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                };

                switch (page)
                {
                    case 0:
                        tb.Inlines.Add(new Run(loc["Page1_Intro"]) { FontWeight = FontWeights.Bold });
                        tb.Inlines.Add(new LineBreak());
                        tb.Inlines.Add(new LineBreak());
                        if (hasCue)
                        {
                            tb.Inlines.Add(new Run(loc["Page1_Display_WithCue"]));
                        }
                        else
                        {
                            tb.Inlines.Add(new Run(loc["Page1_Display_NoCue"]));
                        }
                        tb.Inlines.Add(new LineBreak());
                        tb.Inlines.Add(new LineBreak());
                        tb.Inlines.Add(new Run(loc["Page1_WordsList"]));
                        tb.Inlines.Add(new LineBreak());
                        tb.Inlines.Add(new LineBreak());
                        tb.Inlines.Add(new Run(loc[$"{key}_P1_Congruence"]));
                        tb.Inlines.Add(new LineBreak());
                        tb.Inlines.Add(new LineBreak());
                        if (!hasCue)
                        {
                            tb.Inlines.Add(new Run(loc["Page1_Example"]));
                            tb.Inlines.Add(new LineBreak());
                            tb.Inlines.Add(new LineBreak());
                            if (key == "Case3")
                            {
                                tb.Inlines.Add(new Run(loc["Word_RED"]) { Foreground = Brushes.Red, FontSize = 78 });
                                tb.Inlines.Add(new Run("         "));
                                tb.Inlines.Add(new Run(loc["Word_RED"]) { Foreground = Brushes.Blue, FontSize = 78 });
                            }
                            else
                            {
                                var exampleColor = (key == "Case2") ? Brushes.Red : Brushes.Blue;
                                tb.Inlines.Add(new Run(loc["Word_RED"]) { Foreground = exampleColor, FontSize = 78 });
                            }
                        }
                        break;

                    case 1:
                        tb.Inlines.Add(new Run(loc[$"{key}_P2_Instructions"]));

                        if (hasCue)
                        {
                            tb.Inlines.Add(new LineBreak());
                            tb.Inlines.Add(new LineBreak());
                            var mainPanel = new StackPanel
                            {
                                Orientation = Orientation.Vertical,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center,
                            };
                            var incongruentSquareGrid = CreateCueGrid(false, false);
                            var incongruentCircleGrid = CreateCueGrid(true, false);
                            var congruentSquareGrid = CreateCueGrid(false, true);
                            var congruentCircleGrid = CreateCueGrid(true, true);

                            switch (key)
                            {
                                case "Case4":
                                    var case4Panel = new StackPanel
                                    {
                                        Orientation = Orientation.Horizontal,
                                        HorizontalAlignment = HorizontalAlignment.Center,
                                        VerticalAlignment = VerticalAlignment.Center
                                    };
                                    case4Panel.Children.Add(incongruentSquareGrid);
                                    case4Panel.Children.Add(incongruentCircleGrid);
                                    mainPanel.Children.Add(case4Panel);
                                    break;

                                case "Case5":
                                    var case5Panel = new StackPanel
                                    {
                                        Orientation = Orientation.Horizontal,
                                        HorizontalAlignment = HorizontalAlignment.Center,
                                        VerticalAlignment = VerticalAlignment.Center
                                    };
                                    case5Panel.Children.Add(congruentSquareGrid);
                                    case5Panel.Children.Add(congruentCircleGrid);
                                    mainPanel.Children.Add(case5Panel);
                                    break;

                                case "Case6":
                                    var firstRowPanel = new StackPanel
                                    {
                                        Orientation = Orientation.Horizontal,
                                        HorizontalAlignment = HorizontalAlignment.Center,
                                        VerticalAlignment = VerticalAlignment.Center
                                    };
                                    firstRowPanel.Children.Add(incongruentSquareGrid);
                                    firstRowPanel.Children.Add(incongruentCircleGrid);

                                    var secondRowPanel = new StackPanel
                                    {
                                        Orientation = Orientation.Horizontal,
                                        HorizontalAlignment = HorizontalAlignment.Center,
                                        VerticalAlignment = VerticalAlignment.Center
                                    };
                                    secondRowPanel.Children.Add(congruentSquareGrid);
                                    secondRowPanel.Children.Add(congruentCircleGrid);

                                    mainPanel.Children.Add(firstRowPanel);
                                    mainPanel.Children.Add(secondRowPanel);
                                    break;
                            }

                            tb.Inlines.Add(new InlineUIContainer(mainPanel) { BaselineAlignment = BaselineAlignment.Center });
                            tb.Inlines.Add(new LineBreak());
                            tb.Inlines.Add(new Run(loc[$"{key}_P2_InstructionsWithCue"]));
                        }
                        break;

                    case 2:
                        tb.Inlines.Add(new Run(loc["Page3_Questions"]));
                        break;
                }
                return tb;
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
                Thread.CurrentThread.CurrentUICulture = originalUICulture;
            }
        }

        /// <summary>
        /// Creates a visual grid showing fixation cross, cue shape (circle/square), and stimulus word.
        /// </summary>
        Grid CreateCueGrid(bool isCircle, bool isCongruent)
        {
            var loc = new LocalizedStrings();
            var grid = new Grid
            {
                Width = 500,
                Height = 200,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20)
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(250) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(250) });
            var shapeGrid = new Grid
            {
                Width = 250,
                Height = 250,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(shapeGrid, 0);
            shapeGrid.Children.Add(new Rectangle
            {
                Width = 6,
                Height = 50,
                Fill = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
            shapeGrid.Children.Add(new Rectangle
            {
                Width = 50,
                Height = 6,
                Fill = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
            if (isCircle)
            {
                shapeGrid.Children.Add(new Ellipse
                {
                    Width = 150,
                    Height = 150,
                    Stroke = Brushes.White,
                    StrokeThickness = 6,
                    Fill = Brushes.Transparent,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                });
            }
            else
            {
                shapeGrid.Children.Add(new Rectangle
                {
                    Width = 150,
                    Height = 150,
                    Stroke = Brushes.White,
                    StrokeThickness = 6,
                    Fill = Brushes.Transparent,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                });
            }
            var wordColor = isCongruent ? Brushes.Red : Brushes.Blue;
            var wordBlock = new TextBlock
            {
                Text = loc["Word_RED"],
                Foreground = wordColor,
                FontSize = 78,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(wordBlock, 1);

            grid.Children.Add(shapeGrid);
            grid.Children.Add(wordBlock);

            return grid;
        }
    }
}
