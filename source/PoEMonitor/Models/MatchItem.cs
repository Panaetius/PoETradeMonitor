using System;

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
    }
}