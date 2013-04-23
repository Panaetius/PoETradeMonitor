using System;

namespace PoEMonitor.Models
{
    public class MatchItem
    {
        public DateTime EntryDate { get; set; }

        public string UserName { get; set; }

        public string MatchingRuleName { get; set; }

        public string Message { get; set; }
    }
}