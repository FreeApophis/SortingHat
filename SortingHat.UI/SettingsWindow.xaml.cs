using System;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro;
using MahApps.Metro.Controls;

namespace SortingHat.UI
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : MetroWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();

            InitializerThemeSettings();
        }

        private void InitializerThemeSettings()
        {
            //var currentStyle = ThemeManager.DetectAppStyle(Application.Current);

            //foreach (var theme in ThemeManager.AppThemes) {
            //    var index = ApplicationTheme.Items.Add(theme.Name);

            //    if (theme == currentStyle.Item1) {
            //        ApplicationTheme.SelectedIndex = index;
            //    }
            //}

            //foreach (var accent in ThemeManager.Accents) {
            //    var index = AccentColor.Items.Add(accent.Name);

            //    if (accent == currentStyle.Item2) {
            //        AccentColor.SelectedIndex = index;
            //    }

            //}
        }

        private void OnThemeChanged(object sender, SelectionChangedEventArgs e)
        {
            //var currentStyle = ThemeManager.DetectAppStyle(Application.Current);

            //if (ApplicationTheme.SelectedItem is string applicationTheme) {
            //    ThemeManager.ChangeAppStyle(Application.Current, currentStyle.Item2, ThemeManager.GetAppTheme(applicationTheme));
            //}
        }

        private void OnAccentChanged(object sender, SelectionChangedEventArgs e)
        {
            //var currentStyle = ThemeManager.DetectAppStyle(Application.Current);

            //if (AccentColor.SelectedItem is string accentColor) {
            //    ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(accentColor), currentStyle.Item1);
            //}
        }
    }
}
