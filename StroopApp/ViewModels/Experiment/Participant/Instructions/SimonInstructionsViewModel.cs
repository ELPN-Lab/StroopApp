using StroopApp.Core;
using StroopApp.Models.Simon;
using StroopApp.Resources;
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
    public partial class SimonInstructionsViewModel : InstructionsViewModelBase
    {
        protected List<Action> _pageBuilders;
        protected LocalizedStrings _loc;
        protected SimonProfile _profile;
        protected SimonResponseMappings _simonMappings;
        protected List<Action> PageBuilders => _pageBuilders ??= BuildPages();
        protected override int TotalPages => PageBuilders.Count;

        private TextBlock _textBlock = new();
        public SimonInstructionsViewModel(ExperimentSettingsViewModel settings,
                                          INavigationService participantWindowNavigationService,
                                          Func<Page> nextPageFactory)
            : base(settings, participantWindowNavigationService, nextPageFactory)
        { }

        protected List<Action> BuildPages()
        {
            _simonMappings = _settings.KeyMappings.Simon;
            _profile = (SimonProfile)_settings.CurrentProfile;
            _loc = new LocalizedStrings();
            List<Action> pages = [];
            pages.Add(BuildIntroPage);
            pages.Add(BuildFixationCrossPage);
            pages.Add(BuildLeftRulePage);
            pages.Add(BuildRightRulePage);
            if (_profile.ReversalCuePresentation is not ReversalCuePresentation.None)
            {
                pages.Add(() => BuildPrimePage(step: 0));
                pages.Add(() => BuildPrimePage(step: 1));
                pages.Add(() => BuildPrimePage(step: 2));
            }
            pages.Add(BuildQuestionsPage);
            return pages;
        }

        private bool IsReversalCueColorModality =>
            _profile.StimulusMode is (SimonStimulusMode.Shape or SimonStimulusMode.Arrow)
            && _profile.ReversalCuePresentation is ReversalCuePresentation.Integrated;

        private void BuildIntroPage()
        {
            AddTextLine(_loc["Simon_Page1_Intro"], true);
            AddTextLine(_loc["Simon_Page1_Display"]);
            string stimulusModality = _profile.StimulusMode switch
            {
                SimonStimulusMode.Arrow => _loc["Simon_Word_ArrowDirection"],
                SimonStimulusMode.Shape => _loc["Simon_Word_Shape"],
                _ => _loc["Simon_Word_Color"],
            };
            if(_profile.AnswerMode == SimonAnswerMode.LeftRight)
            {
                AddTextLine(string.Format(_loc["Simon_Page1_Display2"], _simonMappings.Left.Key, _simonMappings.Right.Key, stimulusModality));
            }
            else
            {
                AddTextLine(string.Format(_loc["Simon_Page1_Display3"], _simonMappings.Left.Key, stimulusModality));
            }
        }

        private void BuildFixationCrossPage()
        {
            AddTextLine(_loc["Simon_FixationCrossPage_1"]);
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            panel.Children.Add(new Rectangle { Width = 30 });
            panel.Children.Add(CreateFixationCross());
            _textBlock.Inlines.Add(panel);
        }

        private void BuildPrimePage(int step)
        {
            if (_profile.StimulusMode is SimonStimulusMode.Color)
            {
                if (_profile.ReversalCuePresentation is ReversalCuePresentation.Before)
                    AddTextLine(_loc["Simon_PrimePage_ColorBefore"]);
                else if (_profile.ReversalCuePresentation is ReversalCuePresentation.Around)
                    AddTextLine(_loc["Simon_PrimePage_ColorAround"]);
                else
                    AddTextLine(_loc["Simon_PrimePage_ColorIntegrated"]);
            }
            else if (_profile.StimulusMode is SimonStimulusMode.Shape)
            {
                if (_profile.ReversalCuePresentation is ReversalCuePresentation.Before)
                    AddTextLine(_loc["Simon_PrimePage_ShapeBefore"]);
                else if (_profile.ReversalCuePresentation is ReversalCuePresentation.Around)
                    AddTextLine(_loc["Simon_PrimePage_ShapeAround"]);
                else
                    AddTextLine(_loc["Simon_PrimePage_ShapeIntegrated"]);
            }
            else if (_profile.StimulusMode is SimonStimulusMode.Arrow)
            {
                if (_profile.ReversalCuePresentation is ReversalCuePresentation.Before)
                    AddTextLine(_loc["Simon_PrimePage_ArrowBefore"]);
                else if (_profile.ReversalCuePresentation is ReversalCuePresentation.Around)
                    AddTextLine(_loc["Simon_PrimePage_ArrowAround"]);
                else
                    AddTextLine(_loc["Simon_PrimePage_ArrowIntegrated"]);
            }

            if (step < 1) return;

            var rowPanel = new StackPanel() { Orientation = Orientation.Horizontal };

            var firstColPanel = new WrapPanel() { Width = 700, Orientation = Orientation.Vertical };
            if (IsReversalCueColorModality)
            {
                firstColPanel.Children.Add(new TextBlock() { TextWrapping = TextWrapping.Wrap, Text = string.Format(_loc["Simon_PrimePage_StandardCase"], _loc["Simon_Word_Color"]) });
                firstColPanel.Children.Add(new Rectangle() { Height = 30 });
                firstColPanel.Children.Add(CreateShape(_profile.StandardShape, _profile.StandardColor));
                firstColPanel.Children.Add(new Rectangle() { Height = 30 });
                firstColPanel.Children.Add(new TextBlock() { TextWrapping = TextWrapping.Wrap, Text = _loc["Simon_PrimePage_StandardCaseDescription"] });
            }
            else
            {
                firstColPanel.Children.Add(new TextBlock() { TextWrapping = TextWrapping.Wrap, Text = string.Format(_loc["Simon_PrimePage_StandardCase"], _loc["Simon_Word_Shape"]) });
                firstColPanel.Children.Add(new Rectangle() { Height = 30 });
                firstColPanel.Children.Add(CreateOutlineCue(_profile.StandardShape, "#FFFFFF"));
                firstColPanel.Children.Add(new Rectangle() { Height = 30 });
                firstColPanel.Children.Add(new TextBlock() { TextWrapping = TextWrapping.Wrap, Text = _loc["Simon_PrimePage_StandardCaseDescription"] });
            }
            rowPanel.Children.Add(firstColPanel);

            if (step < 2)
            {
                _textBlock.Inlines.Add(rowPanel);
                return;
            }
            var secondColPanel = new WrapPanel() { Width = 700, Orientation = Orientation.Vertical };
            if (IsReversalCueColorModality)
            {
                secondColPanel.Children.Add(new TextBlock() { TextWrapping = TextWrapping.Wrap, Text = string.Format(_loc["Simon_PrimePage_ReversedCase"], _loc["Simon_Word_Color"]) });
                secondColPanel.Children.Add(new Rectangle() { Height = 30 });
                secondColPanel.Children.Add(CreateShape(_profile.StandardShape, _profile.ReversedColor));
                secondColPanel.Children.Add(new Rectangle() { Height = 30 });
                secondColPanel.Children.Add(new TextBlock() { TextWrapping = TextWrapping.Wrap, Text = _loc["Simon_PrimePage_ReversedCaseDescription"] });

            }
            else
            {
                secondColPanel.Children.Add(new TextBlock() { TextWrapping = TextWrapping.Wrap, Text = string.Format(_loc["Simon_PrimePage_ReversedCase"], _loc["Simon_Word_Shape"]) });
                secondColPanel.Children.Add(new Rectangle() { Height = 30 });
                secondColPanel.Children.Add(CreateOutlineCue(_profile.ReversedShape, "#FFFFFF"));
                secondColPanel.Children.Add(new Rectangle() { Height = 30 });
                secondColPanel.Children.Add(new TextBlock() { TextWrapping = TextWrapping.Wrap, Text = _loc["Simon_PrimePage_ReversedCaseDescription"] });
            }
            rowPanel.Children.Add(secondColPanel);

            _textBlock.Inlines.Add(rowPanel);
        }


        private void BuildLeftRulePage()
        {
            if (_profile.StimulusMode == SimonStimulusMode.Shape)
                AddTextLine(string.Format(_loc["Simon_Stimulus_Instruction_Shape"],_profile.LeftShape));
            else if(_profile.StimulusMode == SimonStimulusMode.Arrow)
                AddTextLine(_loc["Simon_Stimulus_Instruction_Arrow"]);
            else
                AddTextLine(_loc["Simon_Stimulus_Instruction_Color"]);
                
            if (_profile.AnswerMode == SimonAnswerMode.GoNoGo)
            {
                var panel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                var shape = GetRuleShape(isLeftRule: true);
                var color = GetRuleColor(isLeftRule: true);
                _textBlock.Inlines.Add(CreateShape(shape, color));
                panel.Children.Add(new Rectangle { Width = 30 });
                panel.Children.Add(CreateHandClickIcon());

                _textBlock.Inlines.Add(panel);

                AddBlankLine();

                AddTextLine(string.Format(_loc["Simon_GoKey_Instruction"], _simonMappings.Left.Key));
            }
            else
            {
                var shape = GetRuleShape(isLeftRule: true);
                var color = GetRuleColor(isLeftRule: true);
                _textBlock.Inlines.Add(CreateShape(shape, color));
                AddBlankLine();
                AddTextLine(string.Format(_loc["Simon_LeftKey_Instruction"], _simonMappings.Left.Key));
            }
        }

        private void BuildRightRulePage()
        {
            if(_profile.StimulusMode == SimonStimulusMode.Shape)
            AddTextLine(string.Format(_loc["Simon_Stimulus_Instruction_Shape"], _profile.LeftShape));
            else if (_profile.StimulusMode == SimonStimulusMode.Arrow)
                AddTextLine(_loc["Simon_Stimulus_Instruction_Arrow"]);
            else
                AddTextLine(_loc["Simon_Stimulus_Instruction_Color"]);
            if (_profile.AnswerMode == SimonAnswerMode.GoNoGo)
            {
                var panel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                var shape = GetRuleShape(isLeftRule: false);
                var color = GetRuleColor(isLeftRule: false);
                _textBlock.Inlines.Add(CreateShape(shape, color));
                panel.Children.Add(new Rectangle { Width = 30 });
                panel.Children.Add(CreateNoSymbolIcon());
                _textBlock.Inlines.Add(panel);

                AddBlankLine();
                AddTextLine(string.Format(_loc["Simon_NoGoKey_Instruction"]));
            }
            else
            {
                var shape = GetRuleShape(isLeftRule: false);
                var color = GetRuleColor(isLeftRule: false);
                _textBlock.Inlines.Add(CreateShape(shape, color));
                AddBlankLine();
                _textBlock.Inlines.Add(new Run(string.Format(_loc["Simon_RightKey_Instruction"], _simonMappings.Right.Key)));
                AddBlankLine();
            }
        }
        private void BuildQuestionsPage()
        {
            AddTextLine(_loc["Page3_Questions"]);
        }


        /// <summary>
        /// Generates localized instruction content based on page index and experiment configuration.
        /// Handles dynamic instruction based on congruence percentage and key mappings.
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
                var simon = _settings.KeyMappings.Simon;
                var answerMode = ((SimonProfile)_settings.CurrentProfile).AnswerMode;

                _textBlock = CreateNewTextBlock();
                PageBuilders[page]();
                return _textBlock;
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
                Thread.CurrentThread.CurrentUICulture = originalUICulture;
            }
        }

        /// <summary>
        /// Initialize the global container for any content included in the instruction screens
        /// </summary>
        /// <returns>Returns a Black TextBlock element, White Foreground, Fontsize=36, Alignment Center in both directions</returns>
        private TextBlock CreateNewTextBlock()
        {
            return new TextBlock()
            {
                TextWrapping = TextWrapping.Wrap,
                Background = Brushes.Black,
                Foreground = Brushes.White,
                FontSize = 36,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
        }
        private void AddTextLine(string text, bool isBold = false)
        {
            _textBlock.Inlines.Add(new Run(text)
            {
                FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal
            });
            AddBlankLine();
        }

        private void AddBlankLine()
        {
            _textBlock.Inlines.Add(new LineBreak());
            _textBlock.Inlines.Add(new LineBreak());
        }

        private Path CreateShape(SimonStimulusShape shape, string color)
        {
            Brush fill;
            try
            {
                fill = (Brush)new BrushConverter().ConvertFromString(color)!;
            }
            catch
            {
                fill = Brushes.White;
            }
            return new Path()
            {
                Data = ShapeToGeometry(shape),
                Width = 120,
                Height = 120,
                Stretch = Stretch.Uniform,
                Fill = fill,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }
        private Path CreateOutlineCue(SimonStimulusShape shape, string color)
        {
            Brush fill;
            try
            {
                fill = (Brush)new BrushConverter().ConvertFromString(color)!;
            }
            catch
            {
                fill = Brushes.White;
            }
            return new Path()
            {
                Data = ShapeToGeometry(shape),
                Width = 120,
                Height = 120,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Fill = Brushes.Transparent,
                Stroke = Brushes.White,
                StrokeThickness = 6,
            };
        }
        private SimonStimulusShape GetRuleShape(bool isLeftRule)
        {
            if (_profile.StimulusMode is SimonStimulusMode.Arrow)
                return isLeftRule ? SimonStimulusShape.LeftArrow : SimonStimulusShape.RightArrow;
            if (_profile.StimulusMode is SimonStimulusMode.Shape)
                return isLeftRule ? _profile.LeftShape : _profile.RightShape;
            return _profile.ReversalCuePresentation is ReversalCuePresentation.Integrated
                ? _profile.StandardShape : _profile.BaseShape;
        }

        private string GetRuleColor(bool isLeftRule)
        {
            if (_profile.StimulusMode is SimonStimulusMode.Color)
                return isLeftRule ? _profile.LeftColor : _profile.RightColor;
            return _profile.ReversalCuePresentation is ReversalCuePresentation.Integrated
               ? _profile.StandardColor : _profile.BaseColor;
        }

        private static Geometry ShapeToGeometry(SimonStimulusShape shape) => shape switch
        {
            SimonStimulusShape.Square => ShapesGeometries.Square,
            SimonStimulusShape.Triangle => ShapesGeometries.Triangle,
            SimonStimulusShape.LeftArrow => ShapesGeometries.LeftArrow,
            SimonStimulusShape.RightArrow => ShapesGeometries.RightArrow,
            _ => ShapesGeometries.Circle,

        };

        private Ellipse CreateEllipse(string color = "#FFFFFF")
        {
            return new Ellipse
            {
                Width = 120,
                Height = 120,
                Fill = (Brush)new BrushConverter().ConvertFromString(color)!,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        /// <summary>
        /// Creates an HandClick Icon with a path
        /// </summary>
        /// <returns>A Path of 120 x 120 pixels reprensenting an hand and a click</returns>
        private Path CreateHandClickIcon()
        {
            return new Path
            {
                Data = new GeometryGroup
                {
                    Children = [
                                    Geometry.Parse("M 69.416 43.298 H 68.97 c -0.983 0 -1.917 0.216 -2.756 0.603 c -0.644 -2.975 -3.295 -5.21 -6.459 -5.21 h -0.447 c -1.128 0 -2.19 0.284 -3.12 0.784 c -1 -2.379 -3.355 -4.054 -6.094 -4.054 h -0.447 c -0.925 0 -1.807 0.191 -2.606 0.536 v -9.458 c 0 -3.644 -2.964 -6.607 -6.608 -6.607 h -0.447 c -3.643 0 -6.607 2.964 -6.607 6.607 v 24.261 l -3.005 2.281 c -2.394 1.817 -3.911 4.461 -4.273 7.444 c -0.362 2.984 0.479 5.914 2.37 8.251 l 9.378 11.594 v 4.608 c 0 2.791 2.271 5.063 5.062 5.063 h 23.411 c 2.791 0 5.063 -2.271 5.063 -5.063 l 0.001 -4.375 c 2.996 -3.766 4.639 -8.438 4.639 -13.242 V 49.905 C 76.023 46.262 73.06 43.298 69.416 43.298 z M 72.023 67.32 c 0 4.102 -1.478 8.088 -4.159 11.224 c -0.311 0.362 -0.48 0.823 -0.48 1.3 v 5.094 c 0 0.586 -0.477 1.063 -1.063 1.063 H 42.911 c -0.585 0 -1.062 -0.477 -1.062 -1.063 v -5.316 c 0 -0.458 -0.157 -0.902 -0.445 -1.258 L 31.581 66.22 c -1.204 -1.488 -1.74 -3.354 -1.509 -5.254 c 0.23 -1.899 1.197 -3.583 2.721 -4.74 l 0.586 -0.444 v 5.495 c 0 1.104 0.896 2 2 2 s 2 -0.896 2 -2 v -9.506 c 0 -0.014 0 -0.026 0 -0.04 V 26.498 c 0 -1.438 1.169 -2.607 2.607 -2.607 h 0.447 c 1.438 0 2.607 1.17 2.607 2.607 v 15.53 c 0 0 0 0 0 0.001 s 0 0 0 0.001 l 0.01 12.917 c 0.001 1.104 0.896 1.998 2 1.998 c 0 0 0.001 0 0.001 0 c 1.104 -0.001 1.999 -0.896 1.998 -2.002 L 47.04 42.028 c 0 -1.438 1.169 -2.607 2.606 -2.607 h 0.447 c 1.438 0 2.607 1.17 2.607 2.607 v 3.27 c 0 0 0 0.001 0 0.001 c 0 0 0 0.001 0 0.001 l 0.009 9.647 c 0.001 1.104 0.896 1.998 2 1.998 c 0.001 0 0.001 0 0.002 0 c 1.104 -0.001 1.999 -0.897 1.998 -2.002 l -0.009 -9.646 c 0 -1.438 1.169 -2.607 2.606 -2.607 h 0.447 c 1.438 0 2.607 1.17 2.607 2.607 v 4.088 c -0.006 0.066 -0.02 0.13 -0.02 0.197 l 0.01 5.366 c 0.002 1.104 0.897 1.996 2 1.996 c 0.001 0 0.002 0 0.004 0 c 1.104 -0.002 1.998 -0.899 1.996 -2.004 l -0.009 -4.852 c 0.006 -0.062 0.019 -0.121 0.019 -0.184 c 0 -1.438 1.17 -2.607 2.607 -2.607 h 0.446 c 1.438 0 2.607 1.17 2.607 2.607 V 67.32 z"),
                                    Geometry.Parse("M 63.994 25.511 H 51.79 c -1.104 0 -2 -0.896 -2 -2 s 0.896 -2 2 -2 h 12.204 c 1.104 0 2 0.896 2 2 S 65.099 25.511 63.994 25.511 z"),
                                    Geometry.Parse("M 39.985 16.204 c -1.104 0 -2 -0.896 -2 -2 V 2 c 0 -1.104 0.896 -2 2 -2 s 2 0.896 2 2 v 12.204 C 41.985 15.309 41.09 16.204 39.985 16.204 z"),
                                    Geometry.Parse("M 48.558 18.441 c -0.512 0 -1.023 -0.195 -1.414 -0.586 c -0.781 -0.781 -0.781 -2.047 0 -2.828 l 8.63 -8.629 c 0.781 -0.781 2.047 -0.781 2.828 0 c 0.781 0.781 0.781 2.047 0 2.828 l -8.63 8.629 C 49.581 18.246 49.069 18.441 48.558 18.441 z"),
                                    Geometry.Parse("M 28.181 25.511 H 15.977 c -1.104 0 -2 -0.896 -2 -2 s 0.896 -2 2 -2 h 12.204 c 1.104 0 2 0.896 2 2 S 29.285 25.511 28.181 25.511 z"),
                                    Geometry.Parse("M 31.413 18.441 c -0.512 0 -1.024 -0.195 -1.414 -0.586 L 21.37 9.226 c -0.781 -0.781 -0.781 -2.047 0 -2.828 c 0.78 -0.781 2.048 -0.781 2.828 0 l 8.629 8.629 c 0.781 0.781 0.781 2.047 0 2.828 C 32.437 18.246 31.925 18.441 31.413 18.441 z"),
                                    ]
                },
                Width = 120,
                Height = 120,
                Stretch = Stretch.Uniform,
                Fill = Brushes.White,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Miter,
                StrokeMiterLimit = 10,
                Opacity = 1,
            };
        }

        /// <summary>
        /// Creates a NoSymbol icon
        /// </summary>
        /// <returns>Returns a NoSymbol path of 120 x 120 pixels</returns>
        private Path CreateNoSymbolIcon()
        {
            return new Path
            {
                //Data = Geometry.Parse("M6110 12744 c-596 -38 -982 -101 -1470 -239 -1731 -489 -3191 -1708\r\n-3990 -3330 -344 -698 -548 -1432 -627 -2250 -31 -331 -23 -928 18 -1275 142\r\n-1207 609 -2323 1367 -3265 203 -253 555 -617 792 -821 979 -840 2132 -1354\r\n3395 -1513 303 -38 442 -46 815 -45 391 0 565 12 900 60 1108 157 2152 601\r\n3037 1291 420 327 847 761 1171 1191 684 906 1107 1964 1241 3102 41 347 49\r\n944 18 1275 -21 214 -58 479 -93 660 -371 1922 -1618 3572 -3379 4470 -728\r\n371 -1499 592 -2340 670 -158 15 -727 28 -855 19z m523 -2039 c731 -27 1532\r\n-304 2162 -747 160 -112 329 -249 319 -257 -9 -8 -2869 -2091 -5476 -3989\r\n-659 -480 -1202 -872 -1206 -872 -18 0 -133 381 -181 600 -230 1045 -58 2185\r\n472 3110 300 525 716 995 1192 1345 591 434 1263 702 1990 790 139 17 444 33\r\n535 29 30 -2 117 -6 193 -9z m3775 -2892 c282 -801 317 -1705 97 -2558 -123\r\n-476 -330 -935 -607 -1345 -251 -371 -574 -716 -933 -995 -962 -750 -2225\r\n-1044 -3415 -794 -527 110 -1011 314 -1465 615 -154 102 -360 259 -360 274 0\r\n8 6642 4858 6656 4859 4 1 15 -25 27 -56z"),
                Data = Geometry.Parse("M6035 12789 c-1890 -112 -3621 -1041 -4757 -2554 -1087 -1447 -1509\r\n-3313 -1153 -5090 405 -2014 1749 -3711 3615 -4565 1872 -856 4051 -757 5837\r\n266 1606 920 2734 2485 3098 4299 303 1511 46 3094 -721 4432 -971 1696 -2665\r\n2858 -4589 3148 -444 67 -908 89 -1330 64z m815 -1355 c785 -76 1522 -319\r\n2184 -720 129 -78 328 -213 405 -274 l32 -25 -3544 -3543 -3544 -3544 -65 89\r\nc-480 657 -800 1447 -922 2278 -49 331 -65 816 -37 1110 63 670 227 1256 516\r\n1840 494 999 1281 1786 2280 2280 648 320 1319 493 2050 529 130 6 489 -5 645\r\n-20z m3697 -2143 c438 -622 741 -1396 857 -2186 49 -331 65 -816 37 -1110 -63\r\n-670 -227 -1256 -516 -1840 -643 -1303 -1793 -2245 -3215 -2636 -659 -181\r\n-1365 -221 -2050 -118 -812 123 -1596 443 -2243 917 l-89 65 3544 3544 3543\r\n3544 25 -32 c14 -17 62 -83 107 -148z"),
                Width = 120,
                Height = 120,
                Stretch = Stretch.Uniform,
                Fill = Brushes.White,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Miter,
                StrokeMiterLimit = 10,
                Opacity = 1,
            };
        }

        /// <summary>
        /// Generates a TickIcon created form a path, might be usefull for future conditions or other tasks
        /// </summary>
        /// <returns>Returns a TickIcon Path of 120 x 120</returns>
        private Path CreateTickIcon()
        {
            return new Path
            {
                Data = Geometry.Parse("M4.89163 13.2687L9.16582 17.5427L18.7085 8"),
                Width = 120,
                Height = 120,
                Stretch = Stretch.Uniform,
                Stroke = Brushes.Lime,
                StrokeThickness = 16,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
            };
        }

        private Path CreateFixationCross()
        {
            var geometryGroup = new GeometryGroup { FillRule = FillRule.Nonzero };
            geometryGroup.Children.Add(Geometry.Parse(
                "M 45,90 C 40.582,90 37,86.418 37,82 L 37,8 C 37,3.582 40.582,0 45,0 " +
                "C 49.418,0 53,3.582 53,8 L 53,82 C 53,86.418 49.418,90 45,90 Z"));
            geometryGroup.Children.Add(Geometry.Parse(
                "M 82,53 L 8,53 C 3.582,53 0,49.418 0,45 C 0,40.582 3.582,37 8,37 " +
                "L 82,37 C 86.418,37 90,40.582 90,45 C 90,49.418 86.418,53 82,53 Z"));

            return new Path
            {
                Data = geometryGroup,
                Fill = Brushes.White,
                Stretch = Stretch.Uniform,
                Width = 50,
                Height = 50,
            };
        }
    }
}
