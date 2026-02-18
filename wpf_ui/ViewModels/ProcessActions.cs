using Newtonsoft.Json;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfUI.ViewModels;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace ToolKHBrowser.ViewModels
{

    public class ProcessActions
    {
        public int Thread { get; set; }
        public int ResetIP { get; set; }

        public bool IsShareToTimeline { get; set; }
        public bool IsShareToGroup { get; set; }
        public bool IsShareProfilePage { get; set; }
        public bool IsShareWebsite { get; set; }

        public bool IsLeaveGroup { get; set; }
        public bool IsJoinGroup { get; set; }
        public bool IsViewGroup { get; set; }
        public bool AutoScrollGroup { get; set; }
        public bool IsBackupGroup { get; set; }

        public bool PagePost { get; set; }
        public bool GroupPost { get; set; }

        public bool EnglishUS { get; set; }
        public bool LockTime { get; set; }
        public bool Token { get; set; }
        public bool PrimaryLocation { get; set; }
        public bool CreationDate { get; set; }
        public bool LogoutAlDevices { get; set; }
        public bool AutoUnlockCheckpoint { get; set; }
        public bool UserData { get; set; }

        public bool ContactPrimary { get; set; }
        public bool ContactRemoveMail { get; set; }
        public bool ContactRemovePhone { get; set; }
        public bool ContactRemoveInstragram { get; set; }

        public bool ReadNotification { get; set; }
        public bool ReadMessenger { get; set; }
        public bool PostTimeline { get; set; }
        public bool PlayNewsFeed { get; set; }
        public bool AutoScroll { get; set; }

        public bool CreatePage { get; set; }
        public bool FollowPage { get; set; }
        public bool BackupPage { get; set; }
        public bool AutoScrollPage { get; set; }
        public bool PageCreateReel { get; set; }

        public bool AddFriends { get; set; }
        public bool AcceptFriends { get; set; }
        public bool AddFriendsByUID { get; set; }
        public bool BackupFriends { get; set; }

        public bool ProfileDeleteData { get; set; }
        public bool ActivtiyLog { get; set; }
        public bool NewInfo { get; set; }
        public bool PublicPost { get; set; }
        public bool TurnOnTwoFA { get; set; }
        public bool NewPassword { get; set; }
        public bool GetInfo { get; set; }
        public bool CheckIn { get; set; }
        public bool Marketplace { get; set; }
        public bool TurnOnPM { get; set; }
        public bool IsCheckReelInvite { get; set; }
        public bool NoSwitchPage { get; set; }
        public bool IsShareByGraph { get; set; }

        public bool RecoveryPhoneNumber { get; set; }

        public Sharer Share { get; set; }
        public GroupConfig GroupConfig { get; set; }
        public NewsFeedConfig NewsFeed { get; set; }

        public PageConfig PageConfig { get; set; }
        public PagePostConfig PagePostConfig { get; set; }
        public FriendsConfig FriendsConfig { get; set; }
        public ProfileConfig ProfileConfig { get; set; }
        
        public ContactConfig ContactConfig { get; set; }
        public RecoveryConfig RecoveryConfig { get; set; }
        public OtherConfig OtherConfig { get; set; }

        public bool WorkingOnPage { get; set; }
        public bool IsPageInvite { get; set; }
        public bool IsPageRemoveAdmin { get; set; }
    }
    public class OtherConfig
    {
        public bool WatchTime { get; set; }
        public bool ReelPlay { get; set; }
        public bool ChangeLanguage { get; set; }
    }
    public class RecoveryConfig
    {
        public bool FiveSime { get; set; }
        public string Password { get; set; }    
        public RecoveryPhoneNumber PhoneNumber { get; set; }
    }
    public class RecoveryPhoneNumber
    {
        public FiveSim FiveSim { get; set; }
    }
    public class FiveSim
    {
        public string APIKey { get; set; }
        public string Country { get; set; }
        public string Opterator { get; set; }
        public string Product { get; set; }
    }
    public class YandexVerify
    {
        public bool Status { get; set; }
        public string MailPrimary { get; set; }
        public string Code { get; set; }
        public IWebDriver Driver { get; set; }
    }
    public class ContactConfig
    {
        public bool Yandex { get; set; }
        public bool Hotmail { get; set; }
        public bool NewLayout { get; set; }
        public string MailList { get; set; }
        public YandexConfig YandexConfig { get; set; }
        public HotmailConfig HotmailConfig { get; set; }
    }
    public class HotmailConfig
    {
        public string ApiKey { get; set; }
        public string DomainName { get; set; }
    }
    public class YandexConfig
    {
        public int Index { get; set; }
        public string TextFix { get; set; }
        public string Mail { get; set; }
        public string Password { get; set; }
        public string Protocol { get; set; }
    }
    public class ProfileConfig
    {
        public DeleteProfileData DeleteData { get; set; }
        public NewInfo NewInfo { get; set; }
        public TwoFA TwoFA { get; set; }
        public Password Password { get; set; }
        public ActivityLog ActivityLog { get; set; }
    }
    public class ActivityLog
    {
        public int GroupPostsAndComments { get; set; }
    }
    public class DeleteProfileData
    {
        public int Unfriend { get; set; }
        public int Suggest { get; set; }
        public int Request { get; set; }
        public bool Profile { get; set; }
        public bool Cover { get; set; }
        public bool Picture { get; set; }
        public bool Tag { get; set; }
        public bool Phone { get; set; }
    }
    public class NewInfo
    {
        public string SourceProfile { get; set; }
        public string SourceCover { get; set; }
        public string Bio { get; set; }
        public string School { get; set; }
        public string College { get; set; }
        public string City { get; set; }
        public string Hometown { get; set; }

        public string CheckIn { get; set; }
        public string Marketplace { get; set; }
        public string Password { get; set; }
    }

    public class TwoFA
    {
        public bool Web { get; set; }
        public bool WebII { get; set; }
        public bool Mbasic { get; set; }
        public bool EnterPassword { get; set; }
    }
    public class Password
    {
        public string Value { get; set; }
        public bool RunOnWeb { get; set; }
        public bool RunOnWeb2 { get; set; }
        public bool RunOnMbasic { get; set; }
        public bool RunOnNewLayOut { get; set; }
    }
    public class FriendsConfig
    {
        public int AddNumber { get; set; }
        public int AcceptNumber { get; set; }
        public FriendsByUID FriendsByUID { get; set; }
    }
    public class FriendsByUID
    {
        public int AddNumber { get; set; }
        public string UIDs { get; set; }
    }
    //public class PageConfig
    //{
    //    public string PageUrls { get; set; }
    //    public CreatePageConfig CreatePage { get; set; }
    //    public CreateReelConfig CreateReel { get; set; }

    //}

    public class PageConfig
    {
        public string PageUrls { get; set; }
        public CreatePageConfig CreatePage { get; set; }
        public CreateReelConfig CreateReel { get; set; }

        // ✅ ALIAS for code that expects PageConfig.Create
        [JsonIgnore]
        public CreatePageConfig Create => CreatePage;

        // ✅ ALIAS for code that expects PageConfig.Follow.Value
        [JsonIgnore]
        public PageFollowConfig Follow => new PageFollowConfig { Value = PageUrls };
    }
    public class PageFollowConfig
    {
        public string Value { get; set; }
    }
    public class PagePostConfig
    {
        public string PageIds { get; set; }
        public int MinPosts { get; set; }
        public int MaxPosts { get; set; }
        public string SourceFolder { get; set; }
        public string Captions { get; set; }
    }
    public class CreateReelConfig
    {
        public int CreateNumber { get; set; }
        public string SourceFolder { get; set; }
        public string Hashtag { get; set; }
        public string Captions { get; set; }
        public string Caption => Captions;
    }
    public class CreatePageConfig
    {
        public int CreateNumber { get; set; }
        public string Names { get; set; }
        public string Categies { get; set; }
        public string Bio { get; set; }
    }
    public class NewsFeedConfig
    {
        public PostTimelineConfig Timeline { get; set; }
        public PlayOnNewsFeedConfig NewsFeed { get; set; }
        public Messenger Messenger { get; set; }
        public PagePostConfig PagePost { get; set; }
    }
    public class PostTimelineConfig
    {
        public string SourceFolder { get; set; }
        public string Captions { get; set; }
        public int MinNumber { get; set; }
        public int MaxNumber { get; set; }
        public bool DeleteAfterPost { get; set; }
    }
    public class PlayOnNewsFeedConfig
    {
        public React React { get; set; }
        public NumberRank PlayTime { get; set; }
        public string Comments { get; set; }
    }
    public class Messenger
    {
        public MessageCallSound MessageCallSound { get; set; }
        public MessageSound MessageSound { get; set; }
        public MessagePopup MessagePopup { get; set; }
        public ActiveStatus ActiveStatus { get; set; }
    }
    public class MessageCallSound
    {
        public bool None { get; set; }
        public bool On { get; set; }
        public bool Off { get; set; }
    }
    public class MessageSound
    {
        public bool None { get; set; }
        public bool On { get; set; }
        public bool Off { get; set; }
    }
    public class MessagePopup
    {
        public bool None { get; set; }
        public bool On { get; set; }
        public bool Off { get; set; }
    }
    public class ActiveStatus
    {
        public bool None { get; set; }
        public bool On { get; set; }
        public bool Off { get; set; }
    }
    public class GroupConfig
    {
        public GroupConfigJoin Join { get; set; }
        public GroupConfigLeave Leave { get; set; }
        public GroupConfigView View { get; set; }
        public GroupConfigBackup Backup { get; set; }
    }
    public class GroupConfigJoin
    {
        //public string GroupIDs { get; set; }
        public int NumberOfJoin { get; set; }
        public string Answers { get; set; }
        public bool IsJoinOnlyGroupNoPending { get; set; }  
    }
    public class GroupConfigLeave
    {
        public bool IsMembership { get; set; }
        public bool IsID { get; set; }
        public bool LeavePendingOnly { get; set; }
        public bool LeavePendingScriptDetect { get; set; }
    }
    public class GroupConfigView
    {
        public string Captions { get; set; }
        public string Comments { get; set; }
        public string SourceFolder { get; set; }

        public GroupConfigViewTime ViewTime { get; set; }
        public GroupConfigNumber GroupNumber { get; set; }
        public GroupConfigViewReact React { get; set; }
    }
    public class GroupConfigBackup
    {
        public bool IsToken { get; set; }
        public bool IsBrowser { get; set; }
        public bool BackupNewGroup { get; set; }
    }
    public class GroupConfigViewReact
    {
        public bool Like { get; set; }
        public bool None { get; set; }
        public bool Random { get; set; }
    }
    public class GroupConfigNumber
    {
        public int NumberStart { get; set; }
        public int NumberEnd { get; set; }
    }
    public class GroupConfigViewTime
    {
        public int NumberStart { get; set; }
        public int NumberEnd { get; set; }
    }

    public class Sharer
    {
        public string Urls { get; set; }
        public string Captions { get; set; }
        public string Hashtag { get; set; }
        public string Comments { get; set; }
        public bool CheckPending { get; set; }
        public bool MixContent { get; set; }
        public bool OneContent { get; set; }        

        public ShareWebsite Website { get; set; }

        public ShareToGroups Groups { get; set; }
        public ShareProfilePage ProfilePage { get; set; }
        public ShareDelayEachShare DelayEachShare { get; set; }
        public ShareGroupNumber GroupNumber { get; set; }
    }
    public class ShareWebsite
    {
        public bool Page { get; set; }
        public bool Group { get; set; }
        public bool CommentLink { get; set; }
        public bool PastLink { get; set; }
        public bool RandomPicture { get; set; }
        public bool RandomContent { get; set; }
        public bool GroupWithoutJoin { get; set; }
        public string Folder { get; set; }
    }
    public class ShareToGroups
    {
        public int WatchBeforeShare { get; set; }
        public int WatchAfterShare { get; set; }
        public ShareReact React { get; set; }
        public ShareGroupFilter GroupFilter { get; set; }
    }
    public class ShareProfilePage
    {
        public bool IsToken { get; set; }
        public string CommentObject { get; set; }
        public string GroupIDs { get; set; }
    }
    public class ShareGroupFilter
    {
        public bool Random { get; set; }
        public bool GroupName { get; set; }
    }
    public class ShareDelayEachShare
    {
        public int DelayStart { get; set; }
        public int DelayEnd { get; set; }
    }
    public class ShareGroupNumber
    {
        public int NumberStart { get; set; }
        public int NumberEnd { get; set; }
    }
    public class ShareReact
    {
        public bool Like { get; set; }
        public bool None { get; set; }
        public bool Random { get; set; }
    }
    public class React
    {
        public bool Like { get; set; }
        public bool Comment { get; set; }
        public bool None { get; set; }
        public bool Random { get; set; }
    }
    public class NumberRank
    {
        public int NumberStart { get; set; }
        public int NumberEnd { get; set; }
    }

    public class FacebookAccounts
    {
        [JsonProperty("accounts")]
        public Data accounts { get; set; }
    }
    public class Data
    {
        [JsonProperty("data")]
        public object[] data { get; set; }
    }
    public class Pages
    {
        [JsonProperty("id")]
        public string id { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("access_token")]
        public string access_token { get; set; }
        public override string ToString()
        {
            return " [ id : " + id + " , name :" + name + " , access_token :" + access_token + " ]";

        }
    }
    public class FiveSimSMS
    {
        public Code[] code { get; set; }
    }
    public class Code
    {
        [JsonProperty("code")]
        public string code { get; set; }
        public override string ToString()
        {
            return " [ code : " + code + "]";
        }
    }
    public class HotmailboxBuyMail
    {
        [JsonProperty("code")]
        public int code { get; set; }
        [JsonProperty("Message")]
        public string Message { get; set; }
        [JsonProperty("Data")]
        public HotmailboxBuyMailData Data { get; set; }
    }
    public class HotmailboxBuyMailData
    {
        [JsonProperty("Product")]
        public string Product { get; set; }
        public HotmailboxBuyMailDataEmails[] emails { get; set; }
    }
    public class HotmailboxBuyMailDataEmails
    {
        [JsonProperty("Email")]
        public string Email { get; set; }
        [JsonProperty("Password")]
        public string Password { get; set; }
        public override string ToString()
        {
            return " [ Email : " + Email + " , Password :" + Password + " ]";
        }
    }
}
