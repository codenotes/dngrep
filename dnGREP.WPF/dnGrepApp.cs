﻿using System.Windows;
using dnGREP.Common;
using NLog;
using System;

namespace dnGREP.WPF
{
    public class dnGrepApp : Application
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public MainForm MainForm { get; private set; }

        public dnGrepApp()
            : base()
        {
            this.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(dnGrepApp_DispatcherUnhandledException);
            Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("Styles.xaml", UriKind.Relative) });
        }

        void dnGrepApp_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            logger.LogException(LogLevel.Error, e.Exception.Message, e.Exception);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Utils.DeleteTempFolder();
            MainForm = new MainForm();
            ProcessArgs(e.Args);
        }

        public void ProcessArgs(string[] args)
        {
            //Process Command Line Arguments Here
            if (args != null && args.Length > 0)
            {
                string searchPath = args[0];

                if (searchPath == "/warmUp")
                {
                    MainWindow.Visibility = Visibility.Hidden;
                    MainWindow.ShowInTaskbar = false;
                    MainWindow.Loaded += new RoutedEventHandler(MainWindow_Loaded);
                }

                if (searchPath.EndsWith(":\""))
                    searchPath = searchPath.Substring(0, searchPath.Length - 1) + "\\";
                GrepSettings.Instance.Set<string>(GrepSettings.Key.SearchFolder, searchPath);
                MainForm.UpdateState();
            }
            //MainForm.Show();
            MainForm.Activate();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MainForm.Close();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            Utils.DeleteTempFolder();
        }
    }
}
