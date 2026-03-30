using System;
using System.Windows;

namespace PKSAnalyzer
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            var app = new Application();
            app.Run(new MainWindow());
        }
    }
}
