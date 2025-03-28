using IronworksTranslator.Core;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Windows.Media;

namespace IronworksTranslator.Settings
{
    public sealed class ChatSettings
    {
        public ChatSettings()
        {
            Echo = new Channel(ChatCode.Echo);
            Emote = new Channel(ChatCode.Emote);
            Say = new Channel(ChatCode.Say);
            Yell = new Channel(ChatCode.Yell);
            Shout = new Channel(ChatCode.Shout);
            Tell = new Channel(ChatCode.Tell);
            Party = new Channel(ChatCode.Party);
            Alliance = new Channel(ChatCode.Alliance);
            LinkShell1 = new Channel(ChatCode.LinkShell1);
            LinkShell2 = new Channel(ChatCode.LinkShell2);
            LinkShell3 = new Channel(ChatCode.LinkShell3);
            LinkShell4 = new Channel(ChatCode.LinkShell4);
            LinkShell5 = new Channel(ChatCode.LinkShell5);
            LinkShell6 = new Channel(ChatCode.LinkShell6);
            LinkShell7 = new Channel(ChatCode.LinkShell7);
            LinkShell8 = new Channel(ChatCode.LinkShell8);
            CWLinkShell1 = new Channel(ChatCode.CWLinkShell1);
            CWLinkShell2 = new Channel(ChatCode.CWLinkShell2);
            CWLinkShell3 = new Channel(ChatCode.CWLinkShell3);
            CWLinkShell4 = new Channel(ChatCode.CWLinkShell4);
            CWLinkShell5 = new Channel(ChatCode.CWLinkShell5);
            CWLinkShell6 = new Channel(ChatCode.CWLinkShell6);
            CWLinkShell7 = new Channel(ChatCode.CWLinkShell7);
            CWLinkShell8 = new Channel(ChatCode.CWLinkShell8);
            FreeCompany = new Channel(ChatCode.FreeCompany);
            Novice = new Channel(ChatCode.Novice);
            System = new Channel(ChatCode.System);
            Notice = new Channel(ChatCode.Notice);
            Error = new Channel(ChatCode.Error);
            NPCDialog = new Channel(ChatCode.NPCDialog);
            NPCAnnounce = new Channel(ChatCode.NPCAnnounce);
            MarketSold = new Channel(ChatCode.MarketSold);
            Recruitment = new Channel(ChatCode.Recruitment);

            Party.Color = Colors.LightSkyBlue;
            Say.Color = Colors.White;
            Shout.Color = Colors.Orange;
            Yell.Color = Colors.Yellow;
            Alliance.Color = Colors.MediumPurple;
            
            LinkShell1.Color = Colors.LightGreen;
            LinkShell2.Color = Colors.LightGreen;
            LinkShell3.Color = Colors.LightGreen;
            LinkShell4.Color = Colors.LightGreen;
            LinkShell5.Color = Colors.LightGreen;
            LinkShell6.Color = Colors.LightGreen;
            LinkShell7.Color = Colors.LightGreen;
            LinkShell8.Color = Colors.LightGreen;
            
            CWLinkShell1.Color = Colors.LightBlue;
            CWLinkShell2.Color = Colors.LightBlue;
            CWLinkShell3.Color = Colors.LightBlue;
            CWLinkShell4.Color = Colors.LightBlue;
            CWLinkShell5.Color = Colors.LightBlue;
            CWLinkShell6.Color = Colors.LightBlue;
            CWLinkShell7.Color = Colors.LightBlue;
            CWLinkShell8.Color = Colors.LightBlue;
            
            FreeCompany.Color = Colors.LightSeaGreen;
            Novice.Color = Colors.LightCoral;
            
            System.Color = Colors.Gray;
            Notice.Color = Colors.LightYellow;
            Error.Color = Colors.Red;
            NPCDialog.Color = Colors.LightGoldenrodYellow;
            NPCAnnounce.Color = Colors.LightSalmon;
            MarketSold.Color = Colors.Gold;
            Recruitment.Color = Colors.Violet;
            
            Party.TranslateSpeaker = false;
            Say.TranslateSpeaker = false;
            Shout.TranslateSpeaker = false;
            Yell.TranslateSpeaker = false;
            Alliance.TranslateSpeaker = false;

            ChannelVisibility = new Dictionary<ChatCode, bool>() {
                {ChatCode.Echo, Echo.Show },
                {ChatCode.Emote, Echo.Show },
                {ChatCode.Say, Say.Show },
                {ChatCode.Yell, Yell.Show },
                {ChatCode.Shout, Shout.Show },
                {ChatCode.Tell, Tell.Show },
                {ChatCode.Party, Party.Show },
                {ChatCode.Alliance, Alliance.Show },
                {ChatCode.LinkShell1, LinkShell1.Show },
                {ChatCode.LinkShell2, LinkShell2.Show },
                {ChatCode.LinkShell3, LinkShell3.Show },
                {ChatCode.LinkShell4, LinkShell4.Show },
                {ChatCode.LinkShell5, LinkShell5.Show },
                {ChatCode.LinkShell6, LinkShell6.Show },
                {ChatCode.LinkShell7, LinkShell7.Show },
                {ChatCode.LinkShell8, LinkShell8.Show },
                {ChatCode.CWLinkShell1, CWLinkShell1.Show },
                {ChatCode.CWLinkShell2, CWLinkShell2.Show },
                {ChatCode.CWLinkShell3, CWLinkShell3.Show },
                {ChatCode.CWLinkShell4, CWLinkShell4.Show },
                {ChatCode.CWLinkShell5, CWLinkShell5.Show },
                {ChatCode.CWLinkShell6, CWLinkShell6.Show },
                {ChatCode.CWLinkShell7, CWLinkShell7.Show },
                {ChatCode.CWLinkShell8, CWLinkShell8.Show },
                {ChatCode.FreeCompany, FreeCompany.Show },
                {ChatCode.Novice, Novice.Show },
                {ChatCode.System, System.Show },
                {ChatCode.Notice, Notice.Show },
                {ChatCode.Error, Error.Show },
                {ChatCode.NPCDialog, NPCDialog.Show },
                {ChatCode.NPCAnnounce, NPCAnnounce.Show },
                {ChatCode.MarketSold, MarketSold.Show },
                {ChatCode.Recruitment, Recruitment.Show },
            };

            ChannelLanguage = new Dictionary<ChatCode, ClientLanguage>(){
                {ChatCode.Echo, Echo.MajorLanguage },
                {ChatCode.Emote, Emote.MajorLanguage },
                {ChatCode.Say, Say.MajorLanguage },
                {ChatCode.Yell, Yell.MajorLanguage },
                {ChatCode.Shout, Shout.MajorLanguage },
                {ChatCode.Tell, Tell.MajorLanguage },
                {ChatCode.Party, Party.MajorLanguage },
                {ChatCode.Alliance, Alliance.MajorLanguage },
                {ChatCode.LinkShell1, LinkShell1.MajorLanguage },
                {ChatCode.LinkShell2, LinkShell2.MajorLanguage },
                {ChatCode.LinkShell3, LinkShell3.MajorLanguage },
                {ChatCode.LinkShell4, LinkShell4.MajorLanguage },
                {ChatCode.LinkShell5, LinkShell5.MajorLanguage },
                {ChatCode.LinkShell6, LinkShell6.MajorLanguage },
                {ChatCode.LinkShell7, LinkShell7.MajorLanguage },
                {ChatCode.LinkShell8, LinkShell8.MajorLanguage },
                {ChatCode.CWLinkShell1, CWLinkShell1.MajorLanguage },
                {ChatCode.CWLinkShell2, CWLinkShell2.MajorLanguage },
                {ChatCode.CWLinkShell3, CWLinkShell3.MajorLanguage },
                {ChatCode.CWLinkShell4, CWLinkShell4.MajorLanguage },
                {ChatCode.CWLinkShell5, CWLinkShell5.MajorLanguage },
                {ChatCode.CWLinkShell6, CWLinkShell6.MajorLanguage },
                {ChatCode.CWLinkShell7, CWLinkShell7.MajorLanguage },
                {ChatCode.CWLinkShell8, CWLinkShell8.MajorLanguage },
                {ChatCode.FreeCompany, FreeCompany.MajorLanguage },
                {ChatCode.Novice, Novice.MajorLanguage },
                {ChatCode.System, System.MajorLanguage },
                {ChatCode.Notice, Notice.MajorLanguage },
                {ChatCode.Error, Error.MajorLanguage },
                {ChatCode.NPCDialog, NPCDialog.MajorLanguage },
                {ChatCode.NPCAnnounce, NPCAnnounce.MajorLanguage },
                {ChatCode.MarketSold, MarketSold.MajorLanguage },
                {ChatCode.Recruitment, Recruitment.MajorLanguage },
            };

            SpeakerTranslation = new Dictionary<ChatCode, bool>()
            {
                {ChatCode.Echo, Echo.TranslateSpeaker },
                {ChatCode.Emote, Emote.TranslateSpeaker },
                {ChatCode.Say, Say.TranslateSpeaker },
                {ChatCode.Yell, Yell.TranslateSpeaker },
                {ChatCode.Shout, Shout.TranslateSpeaker },
                {ChatCode.Tell, Tell.TranslateSpeaker },
                {ChatCode.Party, Party.TranslateSpeaker },
                {ChatCode.Alliance, Alliance.TranslateSpeaker },
                {ChatCode.LinkShell1, LinkShell1.TranslateSpeaker },
                {ChatCode.LinkShell2, LinkShell2.TranslateSpeaker },
                {ChatCode.LinkShell3, LinkShell3.TranslateSpeaker },
                {ChatCode.LinkShell4, LinkShell4.TranslateSpeaker },
                {ChatCode.LinkShell5, LinkShell5.TranslateSpeaker },
                {ChatCode.LinkShell6, LinkShell6.TranslateSpeaker },
                {ChatCode.LinkShell7, LinkShell7.TranslateSpeaker },
                {ChatCode.LinkShell8, LinkShell8.TranslateSpeaker },
                {ChatCode.CWLinkShell1, CWLinkShell1.TranslateSpeaker },
                {ChatCode.CWLinkShell2, CWLinkShell2.TranslateSpeaker },
                {ChatCode.CWLinkShell3, CWLinkShell3.TranslateSpeaker },
                {ChatCode.CWLinkShell4, CWLinkShell4.TranslateSpeaker },
                {ChatCode.CWLinkShell5, CWLinkShell5.TranslateSpeaker },
                {ChatCode.CWLinkShell6, CWLinkShell6.TranslateSpeaker },
                {ChatCode.CWLinkShell7, CWLinkShell7.TranslateSpeaker },
                {ChatCode.CWLinkShell8, CWLinkShell8.TranslateSpeaker },
                {ChatCode.FreeCompany, FreeCompany.TranslateSpeaker },
                {ChatCode.Novice, Novice.TranslateSpeaker },
                {ChatCode.System, System.TranslateSpeaker },
                {ChatCode.Notice, Notice.TranslateSpeaker },
                {ChatCode.Error, Error.TranslateSpeaker },
                {ChatCode.NPCDialog, NPCDialog.TranslateSpeaker },
                {ChatCode.NPCAnnounce, NPCAnnounce.TranslateSpeaker },
                {ChatCode.MarketSold, MarketSold.TranslateSpeaker },
                {ChatCode.Recruitment, Recruitment.TranslateSpeaker },
            };

            ChannelColor = new Dictionary<ChatCode, Color>()
            {
                {ChatCode.Echo, Echo.Color },
                {ChatCode.Emote, Emote.Color },
                {ChatCode.Say, Say.Color },
                {ChatCode.Yell, Yell.Color },
                {ChatCode.Shout, Shout.Color },
                {ChatCode.Tell, Tell.Color },
                {ChatCode.Party, Party.Color },
                {ChatCode.Alliance, Alliance.Color },
                {ChatCode.LinkShell1, LinkShell1.Color },
                {ChatCode.LinkShell2, LinkShell2.Color },
                {ChatCode.LinkShell3, LinkShell3.Color },
                {ChatCode.LinkShell4, LinkShell4.Color },
                {ChatCode.LinkShell5, LinkShell5.Color },
                {ChatCode.LinkShell6, LinkShell6.Color },
                {ChatCode.LinkShell7, LinkShell7.Color },
                {ChatCode.LinkShell8, LinkShell8.Color },
                {ChatCode.CWLinkShell1, CWLinkShell1.Color },
                {ChatCode.CWLinkShell2, CWLinkShell2.Color },
                {ChatCode.CWLinkShell3, CWLinkShell3.Color },
                {ChatCode.CWLinkShell4, CWLinkShell4.Color },
                {ChatCode.CWLinkShell5, CWLinkShell5.Color },
                {ChatCode.CWLinkShell6, CWLinkShell6.Color },
                {ChatCode.CWLinkShell7, CWLinkShell7.Color },
                {ChatCode.CWLinkShell8, CWLinkShell8.Color },
                {ChatCode.FreeCompany, FreeCompany.Color },
                {ChatCode.Novice, Novice.Color },
                {ChatCode.System, System.Color },
                {ChatCode.Notice, Notice.Color },
                {ChatCode.Error, Error.Color },
                {ChatCode.NPCDialog, NPCDialog.Color },
                {ChatCode.NPCAnnounce, NPCAnnounce.Color },
                {ChatCode.MarketSold, MarketSold.Color },
                {ChatCode.Recruitment, Recruitment.Color },
            };

            Echo.OnSettingsChanged += Channel_OnSettingsChanged;
            Emote.OnSettingsChanged += Channel_OnSettingsChanged;
            Say.OnSettingsChanged += Channel_OnSettingsChanged;
            Yell.OnSettingsChanged += Channel_OnSettingsChanged;
            Shout.OnSettingsChanged += Channel_OnSettingsChanged;
            Tell.OnSettingsChanged += Channel_OnSettingsChanged;
            Party.OnSettingsChanged += Channel_OnSettingsChanged;
            Alliance.OnSettingsChanged += Channel_OnSettingsChanged;
            LinkShell1.OnSettingsChanged += Channel_OnSettingsChanged;
            LinkShell2.OnSettingsChanged += Channel_OnSettingsChanged;
            LinkShell3.OnSettingsChanged += Channel_OnSettingsChanged;
            LinkShell4.OnSettingsChanged += Channel_OnSettingsChanged;
            LinkShell5.OnSettingsChanged += Channel_OnSettingsChanged;
            LinkShell6.OnSettingsChanged += Channel_OnSettingsChanged;
            LinkShell7.OnSettingsChanged += Channel_OnSettingsChanged;
            LinkShell8.OnSettingsChanged += Channel_OnSettingsChanged;
            CWLinkShell1.OnSettingsChanged += Channel_OnSettingsChanged;
            CWLinkShell2.OnSettingsChanged += Channel_OnSettingsChanged;
            CWLinkShell3.OnSettingsChanged += Channel_OnSettingsChanged;
            CWLinkShell4.OnSettingsChanged += Channel_OnSettingsChanged;
            CWLinkShell5.OnSettingsChanged += Channel_OnSettingsChanged;
            CWLinkShell6.OnSettingsChanged += Channel_OnSettingsChanged;
            CWLinkShell7.OnSettingsChanged += Channel_OnSettingsChanged;
            CWLinkShell8.OnSettingsChanged += Channel_OnSettingsChanged;
            FreeCompany.OnSettingsChanged += Channel_OnSettingsChanged;
            Novice.OnSettingsChanged += Channel_OnSettingsChanged;
            System.OnSettingsChanged += Channel_OnSettingsChanged;
            Notice.OnSettingsChanged += Channel_OnSettingsChanged;
            Error.OnSettingsChanged += Channel_OnSettingsChanged;
            NPCDialog.OnSettingsChanged += Channel_OnSettingsChanged;
            NPCAnnounce.OnSettingsChanged += Channel_OnSettingsChanged;
            MarketSold.OnSettingsChanged += Channel_OnSettingsChanged;
            Recruitment.OnSettingsChanged += Channel_OnSettingsChanged;
        }

        private void Channel_OnSettingsChanged(object sender, string name, object value)
        {
            var channel = sender as Channel;
            switch (name)
            {
                case "Show":
                    ChannelVisibility[channel.Code] = (bool)value;
                    break;
                case "MajorLanguage":
                    ChannelLanguage[channel.Code] = (ClientLanguage)value;
                    break;
                case "TranslateSpeaker":
                    SpeakerTranslation[channel.Code] = (bool)value;
                    break;
                case "Color":
                    ChannelColor[channel.Code] = (Color)value;
                    break;
                default:
                    break;
            }
        }

        [JsonIgnore]
        public Dictionary<ChatCode, bool> ChannelVisibility;
        [JsonIgnore]
        public Dictionary<ChatCode, ClientLanguage> ChannelLanguage;
        [JsonIgnore]
        public Dictionary<ChatCode, bool> SpeakerTranslation;
        [JsonIgnore]
        public Dictionary<ChatCode, Color> ChannelColor;

        public Channel Echo;
        public Channel Emote;
        public Channel Say;
        public Channel Yell;
        public Channel Shout;
        public Channel Tell;
        public Channel Party;
        public Channel Alliance;
        public Channel LinkShell1;
        public Channel LinkShell2;
        public Channel LinkShell3;
        public Channel LinkShell4;
        public Channel LinkShell5;
        public Channel LinkShell6;
        public Channel LinkShell7;
        public Channel LinkShell8;
        public Channel CWLinkShell1;
        public Channel CWLinkShell2;
        public Channel CWLinkShell3;
        public Channel CWLinkShell4;
        public Channel CWLinkShell5;
        public Channel CWLinkShell6;
        public Channel CWLinkShell7;
        public Channel CWLinkShell8;
        public Channel FreeCompany;
        public Channel Novice;
        public Channel System;
        public Channel Notice;
        public Channel Error;
        public Channel NPCDialog;
        public Channel NPCAnnounce;
        public Channel MarketSold;
        public Channel Recruitment;
    }
}
