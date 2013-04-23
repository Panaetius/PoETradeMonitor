using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace PoEMonitor.Models
{
    public class MatchingRule:INotifyPropertyChanged
    {
        private string _pattern;

        private string _name;

        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
                this.OnPropertyChanged("Name");
            }
        }

        public string Pattern
        {
            get
            {
                return this._pattern;
            }
            set
            {
                this._pattern = value;
                this.OnPropertyChanged("Pattern");
            }
        }

        public bool Match(string message)
        {
            if (!string.IsNullOrEmpty(Pattern))
            {
                var regexPattern = GetRegexPatternFromPattern(Pattern);

                return Regex.IsMatch(message, regexPattern, RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
            }

            return false;
        }

        private string GetRegexPatternFromPattern(string pattern)
        {
            var result = Regex.Escape(pattern);

            result = result.Replace(@"\*", ".*");
            result = result.Replace(@"\?", ".");
            result = Regex.Replace(result, @"""([^""]+)""", "\\b$1\\b");

            return result;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}