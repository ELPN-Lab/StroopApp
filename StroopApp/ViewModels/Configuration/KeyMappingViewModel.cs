using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModernWpf.Controls;
using StroopApp.Models;
using StroopApp.Resources;
using StroopApp.Services.KeyMapping;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace StroopApp.ViewModels.Configuration
{
    /// <summary>
    /// ViewModel for managing keyboard key mappings for color responses.
    /// Handles key assignment with validation to prevent duplicate mappings.
    /// </summary>
    public partial class KeyMappingViewModel : KeyMappingViewModelBase
    {

        [ObservableProperty]
        private KeyMappings _mappings = new();

        [ObservableProperty]
        private KeyMapping? _editingMapping;

        public KeyMappingViewModel(IKeyMappingService keyMappingService) :base (keyMappingService)
        {
            _ = LoadAsync();
        }

        protected override void ApplyLoaderMappings(ExperimentKeyMappings fullKeyMappings)
        {
            Mappings = fullKeyMappings.Stroop;
        }
        protected override void PersistMappings(ExperimentKeyMappings fullKeyMappings)
        {
            fullKeyMappings.Stroop = Mappings;
        }

        private void RefreshMappingsBindings()
        {
            OnPropertyChanged(nameof(Mappings));
        }

        [RelayCommand]
        private void StartEditing(string color)
        {
            EditingMapping = color switch
            {
                "Red" => Mappings.Red,
                "Blue" => Mappings.Blue,
                "Green" => Mappings.Green,
                "Yellow" => Mappings.Yellow,
                _ => null
            };
        }

        [RelayCommand]
        private async Task OpenKeyMappingEditorAsync(string color)
        {
            try
            {
                StartEditing(color);

                string originalMessage = Strings.Message_KeyMapping;
                var originalText = new TextBlock
                {
                    Text = originalMessage,
                    TextWrapping = TextWrapping.Wrap
                };

                var errorPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Opacity = 0,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                var errorIcon = new FontIcon
                {
                    Glyph = "\uE7BA",
                    Foreground = new SolidColorBrush(Colors.Red),
                    FontSize = 24,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var errorText = new TextBlock
                {
                    Text = Strings.Error_KeyMappingKeyUsed,
                    Foreground = new SolidColorBrush(Colors.Red),
                    Margin = new Thickness(8, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                errorPanel.Children.Add(errorIcon);
                errorPanel.Children.Add(errorText);

                var grid = new Grid();
                grid.Children.Add(originalText);
                grid.Children.Add(errorPanel);

                var dialog = new ContentDialog
                {
                    Title = string.Format(Strings.Title_KeyMappingDialog, color),
                    Content = grid,
                    PrimaryButtonText = string.Empty,
                    SecondaryButtonText = string.Empty
                };

                dialog.KeyDown += async (s, e) =>
                {
                    if (e.Key == Key.Escape)
                    {
                        EditingMapping = null;
                        dialog.Hide();
                        e.Handled = true;
                    }
                    else
                    {
                        bool keyAlreadyUsed =
                            Mappings.Red.Key == e.Key && EditingMapping != Mappings.Red ||
                            Mappings.Blue.Key == e.Key && EditingMapping != Mappings.Blue ||
                            Mappings.Green.Key == e.Key && EditingMapping != Mappings.Green ||
                            Mappings.Yellow.Key == e.Key && EditingMapping != Mappings.Yellow;

                        if (keyAlreadyUsed)
                        {
                            var fadeIn = new DoubleAnimation
                            {
                                From = 0,
                                To = 1,
                                Duration = TimeSpan.FromMilliseconds(300)
                            };
                            var fadeOut = new DoubleAnimation
                            {
                                From = 1,
                                To = 0,
                                BeginTime = TimeSpan.FromMilliseconds(1500),
                                Duration = TimeSpan.FromMilliseconds(300)
                            };

                            var sb = new Storyboard();
                            sb.Children.Add(fadeIn);
                            sb.Children.Add(fadeOut);
                            Storyboard.SetTarget(fadeIn, errorPanel);
                            Storyboard.SetTargetProperty(fadeIn, new PropertyPath("Opacity"));
                            Storyboard.SetTarget(fadeOut, errorPanel);
                            Storyboard.SetTargetProperty(fadeOut, new PropertyPath("Opacity"));
                            sb.Begin();

                            dialog.Focus();
                            e.Handled = true;
                        }
                        else if (EditingMapping != null)
                        {
                            EditingMapping.Key = e.Key;
                            RefreshMappingsBindings();
                            await SaveAsync();
                            dialog.Hide();
                            e.Handled = true;
                        }
                    }
                };

                dialog.Loaded += (s, e) => dialog.Focus();

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync($"{Strings.Error_Title}: {ex.Message}");
            }
        }

        [RelayCommand]
        private void OnKeyPressed(Key key)
        {
            if (EditingMapping == null)
                return;

            if (key == Key.Escape)
            {
                EditingMapping = null;
                return;
            }

            EditingMapping.Key = key;
            RefreshMappingsBindings();
            EditingMapping = null;
        }
    }
}