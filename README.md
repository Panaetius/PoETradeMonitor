Path of Exile Trade Monitor
===============

![Screenshot](https://raw.github.com/Panaetius/PoETradeMonitor/master/img/Screenshot.PNG)

A monitoring tool for Path of Exile Trade Chat.

This tools monitors the Client.txt log of the free-to-play game [Path of Exile](http://www.pathofexile.com) for trade-chat messages ($ ones) and allows you to specify filter rules to filter messages you want to be alerted to.

It doesn't interact or modify your game in any way, instead if only monitors the Client.txt file for changes. It doesn't need or read your login details or any other personal information.

[Download](https://github.com/Panaetius/PoETradeMonitor/raw/master/binaries/PoEMonitor%20v0.1.zip)

Features
--------
- Create filter rules with wildcards to match trade rules
- Sound notifications on match (beep)
- System-Tray notifications on match
- Displays a log of all matches with username, date of message and the message itself

Prerequisites
-------------
- .Net Framework 4.5

Known Issues
------------
- You can't match items that were linked to trade-chat, as those only show up as "_" in a trade message and there's currently no way to get linked item information from the Client.txt file. This will probably never be fixed/supported, unless Grinding Gear Games changes the way the Client.txt works

Usage
-----
Select the Client.txt file from your Path of Exile game folder (Usually "C:\Program Files (x86)\Grinding Gear Games\Path of Exile\logs" for 64-bit and "C:\Program Files\Grinding Gear Games\Path of Exile\logs" for 32-bit Windows).

Choose how you want to be notified of matches (Sound alert, tray notification or no notification).

Choose if you want to filter out messages with linked items in them.

To create a new filter rule, simply click on the empty row in the Rules-list, add a name for the rule (optional) and enter a string matching pattern in the Pattern field.

Patterns support wildcards. "*" to match multiple characters, "?" to match a single character, put a string in quotes (e.g. "GCP") to only match whole words. All matches are case-insesitive.

Examples:

> WTT*GCP

matches

> WTT chaos orbs for gcp

> WTT GCPs for alchemy

but not

>WTB 6L bow for 1 GCP
 
 
and
>"GCP"

matches

>WTB some stuff for 5 gcp

but not

>WTB some stuff for 5 gcps

(Notice the "s" in "gcps")
