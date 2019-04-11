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

            new CompositionRoot()
                .Build()
                .Resolve<MainWindow>()
                .ShowDialog();
        }

    }
}
