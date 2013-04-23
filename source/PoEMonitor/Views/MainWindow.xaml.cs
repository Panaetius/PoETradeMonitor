using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;

using PoEMonitor.ViewModels;

namespace PoEMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            (this.DataContext as MainWindowViewModel).PropertyChanged += this.OnStateChanged;
        }

        public NotifyIcon TrayIcon { get; set; }

        private void OnStateChanged(object o, PropertyChangedEventArgs e)
        {
            var modelWindowState =  (this.DataContext as MainWindowViewModel).CurrentWindowState;
            if ( e.PropertyName == "CurrentWindowState")
            {
                if (modelWindowState == WindowState.Minimized && (this.DataContext as MainWindowViewModel).SystemTrayEnabled)
                {
                    //this.WindowState = modelWindowState;

                    this.Hide();
                    //(this.DataContext as MainWindowViewModel).CurrentWindowState = WindowState.Minimized;
                }
                else if ((modelWindowState == WindowState.Normal || modelWindowState == WindowState.Maximized) && (this.DataContext as MainWindowViewModel).SystemTrayEnabled)
                {
                    this.Show();
                    this.WindowState = modelWindowState;
                }

                base.OnStateChanged(e);
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                (this.DataContext as MainWindowViewModel).CurrentWindowState = WindowState.Minimized;
            }
        }
    }
}
