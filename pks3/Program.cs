using System;
using System.Net;
using System.Windows;

namespace PKS3
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var app = new Application();
            app.Run(new MainWindow());
        }
    }
}
