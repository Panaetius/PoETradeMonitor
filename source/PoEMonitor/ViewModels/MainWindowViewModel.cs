using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

using PoEMonitor.Helpers;
using PoEMonitor.Models;

using Application = System.Windows.Application;
using Timer = System.Timers.Timer;

namespace PoEMonitor.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region constructors
        public MainWindowViewModel()
        {
            _trayIcon = new NotifyIcon { Icon = new Icon("Resources\\PoE.ico") };

            LogFilePath = Properties.Settings.Default.LogFilePath;
            Rules = Properties.Settings.Default.Rules ?? new TrulyObservableCollection<MatchingRule>();
            PlaySoundEnabled = Properties.Settings.Default.PlaySound;
            SystemTrayEnabled = Properties.Settings.Default.SystemTrayEnabled;
            IgnoreLinkedItemsEnabled = Properties.Settings.Default.IgnoreLinkedItemsEnabled;
            IgnoreDuplicateMatches = Properties.Settings.Default.IgnoreDuplicateMatches;
            NotifyPartyEnabled = Properties.Settings.Default.NotifyPartyEnabled;
            NotifyPrivateEnabled = Properties.Settings.Default.NotifyPrivateEnabled;

            Matches = new ObservableCollection<MatchItem>();

            this.OpenLogFile();

            this.InitializeCommands();
        }

        /// <summary>
        /// Initializes all command bindings
        /// </summary>
        public void InitializeCommands()
        {
            this.SelectClientFileCommand = new RelayCommand(this.SelectClientFile);
            this.ClearBlacklistCommand = new RelayCommand(this.ClearBlacklist);
            this.ClearResultsCommand = new RelayCommand(this.ClearResults);
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

        private bool _ignoreLinkedItemsEnabled;

        private WindowState _currentWindowState;

        private bool _ignoreDuplicateMatches;

        private System.Windows.Controls.UserControl _currentView;

        private bool _notifyPartyEnabled;

        private bool _notifyPrivateEnabled;

        #endregion

        #region properties
        /// <summary>
        /// The list of matching rules
        /// </summary>
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

        /// <summary>
        /// True if duplicate matches/chat messages should be ignore. Only the most recent match will be displayed
        /// </summary>
        public bool IgnoreDuplicateMatches
        {
            get
            {
                return this._ignoreDuplicateMatches;
            }
            set
            {
                this._ignoreDuplicateMatches = value;
                this.OnPropertyChanged("IgnoreDuplicateMatches");
                Properties.Settings.Default.IgnoreDuplicateMatches = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// The path of the log file
        /// </summary>
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

        /// <summary>
        /// Flag that indicates whether a sound should be played upon finding a match
        /// </summary>
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

        /// <summary>
        /// Flag that indicates whether the system tray icon is enabled
        /// </summary>
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
                if (value)
                {
                    _trayIcon.DoubleClick += this.ShowWindow;
                }
                else
                {
                    _trayIcon.DoubleClick -= this.ShowWindow;
                }

            }
        }

        /// <summary>
        /// Flag that indicates whether messages with linked items (containing a "_") should be ignored
        /// </summary>
        public bool IgnoreLinkedItemsEnabled
        {
            get
            {
                return this._ignoreLinkedItemsEnabled;
            }
            set
            {
                this._ignoreLinkedItemsEnabled = value;
                this.OnPropertyChanged("IgnoreLinkedItemsEnabled");
                Properties.Settings.Default.IgnoreLinkedItemsEnabled = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Should a notification be sent when the user receives a party message
        /// </summary>
        public bool NotifyPartyEnabled
        {
            get
            {
                return this._notifyPartyEnabled;
            }
            set
            {
                this._notifyPartyEnabled = value;
                this.OnPropertyChanged("NotifyPartyEnabled");
                Properties.Settings.Default.NotifyPartyEnabled = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Should a notification be sent when the user receives a private message
        /// </summary>
        public bool NotifyPrivateEnabled
        {
            get
            {
                return this._notifyPrivateEnabled;
            }
            set
            {
                this._notifyPrivateEnabled = value;
                this.OnPropertyChanged("NotifyPrivateEnabled");
                Properties.Settings.Default.NotifyPrivateEnabled = value;
                Properties.Settings.Default.Save();
            }
        }

        public WindowState CurrentWindowState
        {
            get
            {
                return this._currentWindowState;
            }
            set
            {
                this._currentWindowState = value;
                this.OnPropertyChanged("CurrentWindowState");
            }
        }

        public System.Windows.Controls.UserControl CurrentView
        {
            get
            {
                return this._currentView;
            }
            set
            {
                this._currentView = value;
                this.OnPropertyChanged("CurrentView");
            }
        }

        public ObservableCollection<MatchItem> Matches { get; set; }
        #endregion

        #region Commands

        public ICommand SelectClientFileCommand { get; set; }

        public ICommand ClearBlacklistCommand { get; set; }

        public ICommand ClearResultsCommand { get; set; }

        /// <summary>
        /// A method to show the windows file selection dialog to select the PoE Client.txt file
        /// </summary>
        public void SelectClientFile(object param)
        {
            // Create OpenFileDialog 
            var dlg = new Microsoft.Win32.OpenFileDialog { InitialDirectory = Path.GetDirectoryName(this.LogFilePath), DefaultExt = ".txt", FileName = "Client.txt" };

            // Set filter for file extension and default file extension 

            // Display OpenFileDialog by calling ShowDialog method 
            var result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                LogFilePath = dlg.FileName;

                if (_checkTimer != null)
                {
                    _checkTimer.Stop();
                }

                this.OpenLogFile();
            }
        }

        public void ClearBlacklist(object param)
        {
            Properties.Settings.Default.Blacklist = new List<string>();
            Properties.Settings.Default.Save();
        }

        public void ClearResults(object param)
        {
            this.Matches.Clear();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Opens the log file stream and reads to end
        /// </summary>
        private void OpenLogFile()
        {
            if (File.Exists(this.LogFilePath))
            {
                this._logReader = new StreamReader(new FileStream(this.LogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.GetEncoding("windows-1252"));
                
#if RELEASE //reads the whole file in debug mode, so the game doesn't have to be running to get information
                this._logReader.ReadToEnd();
#endif

                this._checkTimer = new Timer(1000);
                this._checkTimer.Elapsed += this.CheckLogElapsed;
                this._checkTimer.Start();
            }
        }

        /// <summary>
        /// Saves the rule config when changes are made
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RulesChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            var collection = new TrulyObservableCollection<MatchingRule>();
            var filteredRules = this._rules.Where(r => !string.IsNullOrEmpty(r.Pattern));//we don't want to save empty patterns!

            foreach (var filteredRule in filteredRules)
            {
                collection.Add(filteredRule);
            }

            Properties.Settings.Default.Rules = collection;
            Properties.Settings.Default.Save();
        }

        private void ShowWindow(object sender, EventArgs args)
        {
            CurrentWindowState = WindowState.Normal;
        }

        /// <summary>
        /// Checks for new log lines when the timer elapses
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Checks a Log line for a match
        /// </summary>
        /// <param name="line">the line to match</param>
        private void HandleLogLine(string line)
        {
            var parsedMessage = new ParsedMessage(line);

            if (parsedMessage.Type == MessageType.Other)
            {
                return;
            }

            if (!NotifyPartyEnabled && parsedMessage.Type == MessageType.Party)
            {
                return;
            }

            if (!NotifyPrivateEnabled && parsedMessage.Type == MessageType.Private)
            {
                return;
            }

            if (Properties.Settings.Default.Blacklist != null && Properties.Settings.Default.Blacklist.Contains(parsedMessage.User))
            {
                return;
            }

            if (this.IgnoreLinkedItemsEnabled && parsedMessage.Message.Contains("_"))
            {
                return;
            }

            var matchItems = parsedMessage.Match();

            if (matchItems.Count > 0)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => this.AddMatches(matchItems)));
            }
        }

        /// <summary>
        /// Adds a list of match to the list of matches. Removes identical previous matches if the "IgnoreDuplicates" option is set.
        /// </summary>
        /// <param name="matches">the <see cref="IEnumerable{MatchItem}"/> list to add to the Matches </param>
        private void AddMatches(IEnumerable<MatchItem> matches)
        {
            bool isDuplicate = false;

            var matchItems = matches as List<MatchItem> ?? matches.ToList();

            foreach (var item in matchItems)
            {
                if (IgnoreDuplicateMatches)
                {//remove duplicates of any exist already, so only the new entry is in the list
                    var duplicates = Matches.Where(m => m == item).ToList();

                    if (duplicates.Any())
                    {
                        isDuplicate = true;

                        foreach (var duplicate in duplicates)
                        {
                            Matches.Remove(duplicate);
                        }
                    }
                }

                Matches.Add(item);
            }
            
            if (!isDuplicate)
            {//only notify if this isn't a duplicate or IgnoreDuplicateMatches is false. 
             //isDuplicate is true if any of the matches is a duplicate. Because all matches are for the same message (but possibly different rules), one duplicate means the whole list of matches is a duplicate
                if (SystemTrayEnabled)
                {
                    _trayIcon.ShowBalloonTip(5000, "Found new Matches!", string.Join("\r\n", matchItems.Select(m => "Rule: " + m.MatchingRuleName + " User: " + m.UserName + " Message: " + m.Message)), ToolTipIcon.Info);
                }

                if (PlaySoundEnabled)
                {
                    Console.Beep(500, 300);
                }
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