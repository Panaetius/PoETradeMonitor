using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;

using PoEMonitor.Helpers;
using PoEMonitor.Models;

using Application = System.Windows.Application;
using Timer = System.Timers.Timer;

namespace PoEMonitor.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region constructores
        public MainWindowViewModel()
        {
            _trayIcon = new NotifyIcon();
            _trayIcon.Icon = new Icon("Resources\\PoE.ico");

            LogFilePath = Properties.Settings.Default.LogFilePath;
            Rules = Properties.Settings.Default.Rules ?? new TrulyObservableCollection<MatchingRule>();
            PlaySoundEnabled = Properties.Settings.Default.PlaySound;
            SystemTrayEnabled = Properties.Settings.Default.SystemTrayEnabled;
            FilterLinkedItemsEnabled = Properties.Settings.Default.FilterLinkedItemsEnabled;
            Matches = new ObservableCollection<MatchItem>();

            if (File.Exists(LogFilePath))
            {
                _logReader = new StreamReader(new FileStream(LogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                _logReader.ReadToEnd();

                _checkTimer = new Timer(1000);
                _checkTimer.Elapsed += this.CheckLogElapsed;
                _checkTimer.Start();
            }

            InitializeCommands();
        }

        public void InitializeCommands()
        {
            this.SelectClientFileCommand = new RelayCommand(this.SelectClientFile);
        }
        #endregion

        #region fields
        private TrulyObservableCollection<MatchingRule> _rules;

        private string _logFilePath;

        private StreamReader _logReader;

        private Timer _checkTimer;

        private bool _playSoundEnabled;

        private bool _systemTrayEnabled;

        private NotifyIcon _trayIcon;

        private bool _filterLinkedItemsEnabled;

        #endregion

        #region properties
        public TrulyObservableCollection<MatchingRule> Rules
        {
            get
            {
                return this._rules;
            }
            set
            {
                this._rules = value;
                this.OnPropertyChanged("Rules");
                this._rules.CollectionChanged += this.RulesChangedHandler;
            }
        }

        public string LogFilePath
        {
            get
            {
                return this._logFilePath;
            }
            set
            {
                this._logFilePath = value;
                this.OnPropertyChanged("LogFilePath");
                Properties.Settings.Default.LogFilePath = value;
                Properties.Settings.Default.Save();
            }
        }

        public bool PlaySoundEnabled
        {
            get
            {
                return this._playSoundEnabled;
            }
            set
            {
                this._playSoundEnabled = value;
                this.OnPropertyChanged("PlaySoundEnabled");
                Properties.Settings.Default.PlaySound = value;
                Properties.Settings.Default.Save();
            }
        }

        public bool SystemTrayEnabled
        {
            get
            {
                return this._systemTrayEnabled;
            }
            set
            {
                this._systemTrayEnabled = value;
                this.OnPropertyChanged("SystemTrayEnabled");
                Properties.Settings.Default.SystemTrayEnabled = value;
                Properties.Settings.Default.Save();

                _trayIcon.Visible = value;
            }
        }

        public bool FilterLinkedItemsEnabled
        {
            get
            {
                return this._filterLinkedItemsEnabled;
            }
            set
            {
                this._filterLinkedItemsEnabled = value;
                this.OnPropertyChanged("FilterLinkedItemsEnabled");
                Properties.Settings.Default.FilterLinkedItemsEnabled = value;
                Properties.Settings.Default.Save();
            }
        }

        public ObservableCollection<MatchItem> Matches { get; set; }
        #endregion

        #region Commands

        public ICommand SelectClientFileCommand { get; set; }

        public void SelectClientFile()
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.InitialDirectory = Path.GetDirectoryName(this.LogFilePath);


            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".txt";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                LogFilePath = dlg.FileName;

                if (_checkTimer != null)
                {
                    _checkTimer.Stop();
                }

                if (File.Exists(LogFilePath))
                {
                    _logReader = new StreamReader(new FileStream(LogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                    _logReader.ReadToEnd();

                    _checkTimer = new Timer(1000);
                    _checkTimer.Elapsed += this.CheckLogElapsed;
                    _checkTimer.Start();
                }
            }
        }

        #endregion

        #region Methods
        private void RulesChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            var collection = new TrulyObservableCollection<MatchingRule>();
            var filteredRules = this._rules.Where(r => !string.IsNullOrEmpty(r.Pattern));

            foreach (var filteredRule in filteredRules)
            {
                collection.Add(filteredRule);
            }

            Properties.Settings.Default.Rules = collection;
            Properties.Settings.Default.Save();
        }

        void CheckLogElapsed(object sender, ElapsedEventArgs e)
        {
            if (_logReader != null)
            {
                _checkTimer.Stop();

                string line;

                while ((line = _logReader.ReadLine()) != null)
                {
                    HandleLogLine(line);
                }

                _checkTimer.Start();
            }
        }

        private void HandleLogLine(string line)
        {
            var pattern = @"(\d{4}/\d{2}/\d{2}\s\d{2}:\d{2}:\d{2})[^\]]+\]\s\$([^:]+):\s(.*)";

            var matches = Regex.Matches(line, pattern, RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.IgnoreCase);

            if (matches.Count != 1)
            {
                return;
            }

            var date = DateTime.ParseExact(matches[0].Groups[1].Value, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            var user = matches[0].Groups[2].Value;
            var message = matches[0].Groups[3].Value;

            if (FilterLinkedItemsEnabled && message.Contains("_"))
            {
                return;
            }

            var matchingRules = Rules.Where(m => m.Match(message));

            if (!matchingRules.Any())
            {
                return;
            }

            var matchItems = new List<MatchItem>();

            foreach (var matchingRule in matchingRules)
            {
                var item = new MatchItem
                    { EntryDate = date, MatchingRuleName = matchingRule.Name, UserName = user, Message = message };

                matchItems.Add(item);

                Application.Current.Dispatcher.BeginInvoke(new Action(() => Matches.Add(item)));
            }

            if (SystemTrayEnabled)
            {
                _trayIcon.ShowBalloonTip(5000, "Found new Matches!", string.Join("\r\n", matchItems.Select(m => "Rule: " + m.MatchingRuleName + " User: "+m.UserName+" Message: " + m.Message)), ToolTipIcon.Info);
            }

            if (PlaySoundEnabled)
            {
                Console.Beep(500, 300);
            }
        }

        #endregion

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}