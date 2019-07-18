using System.Globalization;
using System.Windows;
using Autofac;

namespace SortingHat.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");

            new CompositionRoot()
                .Build()
                .Resolve<MainWindow>()
                .ShowDialog();
        }

    }
}
