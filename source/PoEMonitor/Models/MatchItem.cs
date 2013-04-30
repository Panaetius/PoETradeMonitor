using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

using PoEMonitor.Helpers;

namespace PoEMonitor.Models
{
    public class MatchItem
    {
        /// <summary>
        /// The date of the message
        /// </summary>
        public DateTime EntryDate { get; set; }

        /// <summary>
        /// The user who wrote the message
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// the name of the <see cref="MatchingRule"/> that matched
        /// </summary>
        public string MatchingRuleName { get; set; }

        /// <summary>
        /// The message that was matched
        /// </summary>
        public string Message { get; set; }

        public ICommand CopyUsernameCommand
        {
            get
            {
                return new RelayCommand(_ => this.CopyUsername());
            }
        }

        public ICommand BlacklistCommand
        {
            get
            {
                return new RelayCommand(_ => this.AddToBlacklist());
            }
        }

        private void CopyUsername()
        {
            Clipboard.SetText(this.UserName);
        }

        private void AddToBlacklist()
        {
            if(Properties.Settings.Default.Blacklist == null)
            {
                Properties.Settings.Default.Blacklist = new List<string>();
            }

            Properties.Settings.Default.Blacklist.Add(this.UserName);
            Properties.Settings.Default.Save();
        }

        #region Equality members
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((MatchItem)obj);
        }

        protected bool Equals(MatchItem other)
        {
            return string.Equals(this.UserName, other.UserName) && string.Equals(this.MatchingRuleName, other.MatchingRuleName) && string.Equals(this.Message, other.Message);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (this.UserName != null ? this.UserName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.MatchingRuleName != null ? this.MatchingRuleName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.Message != null ? this.Message.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(MatchItem left, MatchItem right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MatchItem left, MatchItem right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}