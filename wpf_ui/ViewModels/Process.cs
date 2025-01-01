using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolKHBrowser.ViewModels
{
    public class ProcessAction
    {
        public int account { set; get; }
        public int tab { set; get; }
        public int round { set; get; }
        public int type { set; get; }
        public General general { get; set; }
        public Newfeed newfeed { get; set; }
        public Friend friend { get; set; }
        public ActiveGroup group { get; set; }
        public Leave leave { get; set; }
        public TwoFAa twofa { get; set; }
        public Share share { get; set; }
    }
    public class General
    {
        public bool isUseImage { get; set; }
        public bool isUseCookie { get; set; }
        public int screen { get; set; }
        public string type { get; set; }
    }
    public class Share
    {
        public int scroll { get; set; }
        public bool isNewfeedComment { get; set; }
        public int shareType { get; set; }
        public int watchTime { get; set; }
        public int watchLike { get; set; }
        public bool isWatchComment { get; set; }
        public int groupNumber { get; set; }
        public int watchScroll { get; set; }
        public int postDelay { get; set; }
        public bool autoLeave { get; set; }
        public bool isMobile { get; set; }
        public bool isOneByOne { get; set; }
        public bool isTimeline { get; set; }
        public bool isAuto { get; set; }
        public bool isCaption { get; set; }
    }
    public class Newfeed
    {
        public int scroll { get; set; }
        public int postWaiting { get; set; }
        public int submitWaiting { get; set; }
        public bool isSource { get; set; }
        public bool isStory { get; set; }
        public bool isCaption { get; set; }
        public bool isComment { get; set; }
        public bool isNotification { get; set; }
        public bool isMessenger { get; set; }
    }
    public class Friend
    {
        public int add { get; set; }
        public int confirm { get; set; }
        public int suggest { get; set; }
        public bool check { get; set; }
    }
    public class ActiveGroup
    {
        public int number { get; set; }
        public int scroll { get; set; }
        public int numberPost { get; set; }
        public int postWaiting { get; set; }
        public int submitWaiting { get; set; }
        public bool isSource { get; set; }
        public bool isCaption { get; set; }
        public bool isComment { get; set; }
        public bool isCount { get; set; }
        public bool isLeave { get; set; }
    }
    public class Leave
    {
        public bool leave1 { get; set; }
        public bool leave2 { get; set; }
        public int group { get; set; }
    }
    public class TwoFAa
    {
        public bool EnterPassword { get; set; }
        public bool IsLogic2 { get; set; }
    }
}
