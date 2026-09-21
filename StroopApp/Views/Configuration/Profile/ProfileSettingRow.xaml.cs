using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace StroopApp.Views.Configuration.Profile
{
    /// <summary>
    /// Logique d'interaction pour ProfileSettingRow.xaml
    /// </summary>
    [ContentProperty(nameof(SettingContent))]
    public partial class ProfileSettingRow : UserControl
    {
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(ProfileSettingRow), new PropertyMetadata(""));
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(ProfileSettingRow), new PropertyMetadata(""));
        public static readonly DependencyProperty SettingContentProperty =
            DependencyProperty.Register(nameof(SettingContent), typeof(object), typeof(ProfileSettingRow), new PropertyMetadata(null));
        public static readonly DependencyProperty DetailContentProperty =
            DependencyProperty.Register(nameof(DetailContent), typeof(object), typeof(ProfileSettingRow), new PropertyMetadata(null));
        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }
        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }
        public object? SettingContent
        {
            get => GetValue(SettingContentProperty);
            set => SetValue(SettingContentProperty, value);
        }
        public object? DetailContent
        {
            get => GetValue(DetailContentProperty);
            set => SetValue(DetailContentProperty, value);
        }

        public ProfileSettingRow()
        {
            InitializeComponent();
        }
    }
}
