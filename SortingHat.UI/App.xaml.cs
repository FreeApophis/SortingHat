using System.Globalization;
using System.Windows;
using Autofac;

namespace SortingHat.UI;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");

        using var container = new CompositionRoot().Build();

        container.Resolve<MainWindow>().ShowDialog();
    }
}
