using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PoEMonitor.Models
{
    public enum MessageType
    {
        Trade = 0,
        Party,
        Private,
        Global,
        Other
    }
    public class ParsedMessage
    {
        public ParsedMessage(string logline)
        {
            const string pattern = @"(\d{4}/\d{2}/\d{2}\s\d{2}:\d{2}:\d{2})[^\]]+\]\s([$@%#])([^:]+):\s(.*)";

            var matches = Regex.Matches(logline, pattern, RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.IgnoreCase);

            if (matches.Count != 1)
            {
                Type = MessageType.Other;
                return;
            }

            MessageDate = DateTime.ParseExact(matches[0].Groups[1].Value, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            User = matches[0].Groups[3].Value;
            Message = matches[0].Groups[4].Value;

            var type = matches[0].Groups[2].Value;

            switch (type)
            {
                case "$":
                    Type = MessageType.Trade;
                    break;
                case "@":
                    Type = MessageType.Private;
                    break;
                case "%":
                    Type = MessageType.Party;
                    break;
                case "#":
                    Type = MessageType.Global;
                    break;
                default:
                    Type = MessageType.Other;
                    break;
            }
        }

        public string User { get; set; }

        public DateTime MessageDate { get; set; }

        public MessageType Type { get; set; }

        public string Message { get; set; }

        public List<MatchItem> Match()
        {
            switch (Type)
            {
                case MessageType.Party:
                    return new List<MatchItem>() { new MatchItem() { EntryDate = this.MessageDate, UserName = this.User, Message = this.Message, MatchingRuleName = "Party Chat" } };

                case MessageType.Private:
                    return new List<MatchItem>() { new MatchItem() { EntryDate = this.MessageDate, UserName = this.User, Message = this.Message, MatchingRuleName = "Private Chat" } };

                case MessageType.Global:
                    var globalMatchingRules = Properties.Settings.Default.Rules.Where(m => m.Match(this.Message)).ToList();

                    if (!globalMatchingRules.Any())
                    {
                        return new List<MatchItem>();
                    }

                    var globalMatchItems =
                        globalMatchingRules.Select(
                            matchingRule => new MatchItem() { EntryDate = this.MessageDate, UserName = this.User, Message = this.Message, MatchingRuleName = matchingRule.Name }).
                            ToList();
                    return globalMatchItems;

                case MessageType.Trade:
                    var matchingRules = Properties.Settings.Default.Rules.Where(m => m.Match(this.Message)).ToList();

                    if (!matchingRules.Any())
                    {
                        return new List<MatchItem>();
                    }

                    var matchItems =
                        matchingRules.Select(
                            matchingRule => new MatchItem() { EntryDate = this.MessageDate, UserName = this.User, Message = this.Message, MatchingRuleName = matchingRule.Name }).
                            ToList();
                    return matchItems;

                default:
                    return new List<MatchItem>();

            }
        }
    }
}
