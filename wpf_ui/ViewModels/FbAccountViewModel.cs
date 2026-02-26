using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolLib;
using ToolLib.Data;

namespace ToolKHBrowser.ViewModels
{
    public interface IFbAccountViewModel
    {
        ObservableCollection<FbAccount> fbAccounts(int groupDeviceId, string keyword="", bool isTempStore= false, int statusId=-1);
        ObservableCollection<FbAccount> populate(string text,bool igNoreFirstLine=true, int groupDeviceId=0);
        IAccountDao getAccountDao();
        string updateAccountAction(string uid, int actionResult, string actionType);

        int GetTotalAccount();
        int GetTotalAccountLive();
        int GetTotalAccountDie();
    }
    public class FbAccountViewModel : IFbAccountViewModel
    {
        private IAccountDao accountDao;
        private int totalAccount;
        private int totalAccountLive;
        private int totalAccountDie;

        public FbAccountViewModel(IAccountDao accountDao)
        {
            this.accountDao = accountDao;
        }
        public IAccountDao getAccountDao()
        {
            return accountDao;
        }
        public string updateAccountAction(string uid, int actionResult, string actionType)
        {
            return accountDao.updateAccountAction(uid, actionResult, actionType);
        }
        public int GetTotalAccount()
        {
            return totalAccount;
        }
        public int GetTotalAccountLive()
        {
            return totalAccountLive;
        }
        public int GetTotalAccountDie()
        {
            return totalAccountDie;
        }
        public ObservableCollection<FbAccount> fbAccounts(int storeId, string keyword="", bool isTempStore= false, int statusId=-1)
        {
            var items = new ObservableCollection<FbAccount>();
            var accounts = accountDao.listAccount(storeId, keyword, isTempStore);
            int key = 1;

            totalAccount = 0;
            totalAccountLive = 0;
            totalAccountDie = 0;

            foreach (var a in accounts)
            {
                bool isAdd = true;
                if (statusId != -1)
                {
                    isAdd = false;
                    if(statusId == a.Status)
                    {
                        isAdd = true;
                    }
                }
                if (isAdd)
                {
                    var fbAccount = new FbAccount()
                    {
                        Id = Convert.ToInt64(a.Id.ToString()),
                        StoreId = a.StoreId,
                        Key = key,
                        UID = a.UID,
                        Name = a.Name,
                        Password = a.Password,
                        TotalFriend = a.TotalFriend,
                        FriendsRequest = a.FriendsRequest,
                        Gender = a.Gender,
                        Email = a.Email,
                        Token = a.Token,
                        Proxy = a.Proxy,
                        IsActive = a.IsActive,
                        PendingJoin = a.PendingJoin,
                        GroupIDs = a.GroupIDs,
                        TwoFA = GetStrTwoFA(a.TwoFA, a.IsTwoFA),
                        Status = GetAccountStatus(a.Status),
                        Join = GetStrJoin(a.Join),
                        Verify = GetStrJoin(a.IsVerify),
                        Leave = GetStrLeave(a.Leave),
                        Share = GetStrShare(a.Share),
                        Active = GetStrActive(a.IsActive),
                        Login = GetStrLogin(a.IsLogin),
                        Description = a.Description,
                        IsLogin = a.IsLogin,
                        IsVerify = a.IsVerify,
                        IsTwoFA = a.IsTwoFA,
                        IsLeave = a.Leave,
                        IsShare = a.Share,
                        Cookie = a.Cookie,
                        DOB = a.DOB,
                        UserAgent = a.UserAgent,

                        TotalGroup = a.TotalGroup,
                        TotalPage = a.TotalPage,
                        PageIds = a.PageIds,

                        TempName = a.TempName,
                        StoreName = !string.IsNullOrWhiteSpace(a.StoreName) ? a.StoreName : a.TempName,
                        Note = a.Note,
                        ReelSourceVideo = a.ReelSourceVideo,
                        MailPass = a.MailPass,
                        PrimaryLocation = a.PrimaryLocation,
                        TimelineSource = a.TimelineSource,

                        TotalShareGroup = a.TotalShareGroup,
                        TotalShareTimeline = a.TotalShareTimeline,

                        LastUpdate = GetDate(a.UpdatedAt),

                        CreationDate = a.CreationDate,
                        OldGroupIds = a.OldGroupIDs,
                    };

                    items.Add(fbAccount);
                    key++;

                    totalAccount++;
                    if(fbAccount.Status == "Live")
                    {
                        totalAccountLive++;
                    } else
                    {
                        totalAccountDie++;
                    }
                }
            }

            return items;
        }
        public string GetDate(long date)
        {
            if (date > 0)
            {
                return DateTimeOffset.FromFileTime(date).ToString("dd/MM/yyyy HH:mm:ss");
            }

            return "...";
        }
        public ObservableCollection<FbAccount> populate(string text, bool igNoreFirstLine = true, int groupDeviceId = 0)
        {
            var items = new ObservableCollection<FbAccount>();
            int start = 0;
            if (igNoreFirstLine)
            {
                start = 1;
            }
            var arr = text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = start; i < arr.Length; i++)
            {
                var l = arr[i];
                var c = l.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

                string uid = "", password = "", twofa = "", token = "";
                int length = c.Length;
                if (length >= 2)
                {
                    uid = c[0];
                    password = c[1];
                    if (length >= 3)
                    {
                        twofa = c[2];
                    }
                    if (length >= 4)
                    {
                        token = c[3];
                    }
                    var fbAcc = new FbAccount()
                    {
                        Key = i,
                        UID = uid,
                        Password = password,
                        TwoFA = twofa,
                        Token = token,
                        Description = "New Account"
                    };

                    items.Add(fbAcc);
                }
            }

            return items;
        }
        public string GetAccountStatus(int status)
        {
            string s = "";

            if (status == 1)
            {
                s = "Live";
            }
            else if (status == 0)
            {
                s = "Die";
            }

            return s;
        }
        public string GetStrTwoFA(string twofa, int num)
        {            
            if(string.IsNullOrEmpty(twofa)) {
                //twofa = "Block";
                //if(num==1)
                //{
                //    twofa = "Allow";
                //}
            }

            return twofa;
        }
        public string GetStrLogin(int num)
        {
            string s = "Block";
            if (num == 0)
            {
                s = "Allow";
            }

            return s;
        }
        public string GetStrVerify(int num)
        {
            string s = "Allow";
            if (num == 0)
            {
                s = "Block";
            }

            return s;
        }
        public string GetStrJoin(int num)
        {
            string s = "Allow";
            if(num==0)
            {
                s = "Block";
            }

            return s;
        }
        public string GetStrActive(int num)
        {
            string s = "Allow";
            if (num == 0)
            {
                s = "Block";
            }

            return s;
        }
        public string GetStrShare(int num)
        {
            string s = "Allow";
            if (num == 0)
            {
                s = "Block";
            } 

            return s;
        }
        public string GetStrLeave(int num)
        {
            string s = "Allow";
            if (num == 0)
            {
                s = "Block";
            }

            return s;
        }
    }
}
