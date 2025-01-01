using Emgu.CV.Structure;
using OpenPop.Mime;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using ToolKHBrowser.ToolLib.Data;
using ToolKHBrowser.ToolLib.Tool;
using ToolLib.Data;
using ToolLib.Tool;
using WpfUI;
using WpfUI.ToolLib.Data;
using WpfUI.ViewModels;
using WpfUI.Views;
using static Emgu.CV.Stitching.Stitcher;

namespace ToolKHBrowser.ViewModels
{
    public interface IGroupViewModel
    {
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void Start(frmMain form, IWebDriver driver, FbAccount data);
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void LeaveGroup();
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void JoinGroup();
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void BackupGroup();
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void ViewGroup();
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        IGroupsDao GetGroupsDao();
    }
    public class GroupViewModel : IGroupViewModel
    {
        private IAccountDao accountDao;
        private ICacheDao cacheDao;
        private IGroupsDao groupsDao;
        private frmMain form;
        private FbAccount data;
        private IWebDriver driver;
        private ProcessActions processActionData;
        private string[] joinAnswerArr;
        private string[] commentArr;
        private string[] captionArr;
        private string[] soureFolderFileArr;
        private Random random;
        private int sourceFolderFileIndex;

        public GroupViewModel(IAccountDao accountDao, ICacheDao cacheDao, IGroupsDao groupsDao)
        {
            this.accountDao = accountDao;
            this.cacheDao = cacheDao;
            this.groupsDao = groupsDao;
        }
        public IGroupsDao GetGroupsDao()
        {
            return this.groupsDao;
        }
        public void Start(frmMain form,IWebDriver driver, FbAccount data)
        {
            this.form = form;
            this.data = data;
            this.driver = driver;
            this.processActionData = this.form.processActionsData;
            sourceFolderFileIndex = 0;
            try
            {
                sourceFolderFileIndex = Int32.Parse(this.form.cacheViewModel.GetCacheDao().Get("group:view:source_index").Value.ToString());
            }
            catch (Exception) { }

            random = new Random();
            try
            {
                joinAnswerArr = this.processActionData.GroupConfig.Join.Answers.Split('\n');
            }
            catch (Exception) { }
            try
            {
                commentArr = this.processActionData.GroupConfig.View.Comments.Split('\n');
            }
            catch (Exception) { }
            try
            {
                captionArr = this.processActionData.GroupConfig.View.Captions.Split('\n');
            }
            catch (Exception) { }
        }
        public void LeaveGroup()
        {
            if(processActionData.GroupConfig.Leave.IsMembership)
            {
                WebFBTool.LeaveGroupII(driver);
            }
            if(processActionData.GroupConfig.Leave.IsID)
            {
                string runType = ConfigData.GetRunType().ToLower().Trim();
                var groupRecords = groupsDao.GetRecordsByUID(data.UID);

                //var graph = new FBGraph();
                //if (processActionData.GroupConfig.Leave.LeavePendingScriptDetect)
                //{
                //    graph.GetCookieContainerFromDriver(driver);
                //}                

                foreach (DataRow row in groupRecords.Rows)
                {
                    if (IsStop())
                    {
                        break;
                    }
                    string groupId = "";
                    try
                    {
                        groupId = row["group_id"] + "";
                    }
                    catch (Exception) { }
                    if (string.IsNullOrEmpty(groupId))
                    {
                        continue;
                    }
                    int isPending = 0;
                    if(processActionData.GroupConfig.Leave.LeavePendingScriptDetect)
                    {
                        isPending = IsGrouupPending(driver,groupId);
                        if(isPending == 1)
                        {
                            try
                            {
                                driver.Navigate().GoToUrl(Constant.FB_WEB_URL + "/" + groupId);
                            }
                            catch (Exception) { }
                            FBTool.WaitingPageLoading(driver);
                            Thread.Sleep(500);
                            WebFBTool.LeaveGroup(driver);
                            Thread.Sleep(1000);
                        }
                    } 
                    else if (processActionData.GroupConfig.Leave.LeavePendingOnly)
                    {
                        try
                        {
                            driver.Navigate().GoToUrl(Constant.FB_WEB_URL + "/" + groupId);
                        }
                        catch (Exception) { }
                        FBTool.WaitingPageLoading(driver);
                        Thread.Sleep(500);
                        WebFBTool.LeaveGroup(driver, processActionData.GroupConfig.Leave.LeavePendingOnly);
                        Thread.Sleep(1000);
                    } else
                    {
                        try
                        {
                            driver.Navigate().GoToUrl(Constant.FB_WEB_URL + "/" + groupId);
                        }
                        catch (Exception) { }
                        FBTool.WaitingPageLoading(driver);
                        Thread.Sleep(500);
                        WebFBTool.LeaveGroup(driver);
                        Thread.Sleep(1000);
                    }

                    
                    if (runType == "web" || true)
                    {
                        
                        
                    }
                    else
                    {
                        try
                        {
                            driver.Navigate().GoToUrl(Constant.FB_MOBILE_URL + "/" + groupId);
                        }
                        catch (Exception) { }
                        FBTool.WaitingPageLoading(driver);
                        FBTool.Scroll(driver, 1000, false);
                        MobileFBTool.LeaveGroup(driver);
                        Thread.Sleep(1000);
                    }
                }
            }
        }
        public int IsGrouupPending(IWebDriver driver, string groupId)
        {
            try
            {
                driver.Navigate().GoToUrl("https://m.facebook.com/groups/" + groupId + "/madminpanel/pending");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(500);
            string source = "";
            try
            {
                source = driver.PageSource;
                if(source.ToString().Contains("Pending Posts"))
                {
                    return 1;
                }
                else if(source.ToString().ToLower().Contains("no pending posts"))
                {
                    return 0;
                }
            } catch(Exception) { }

            return 1;
        }
        public void JoinGroup()
        {
            int joinGroup = processActionData.GroupConfig.Join.NumberOfJoin;
            if (joinGroup > 0)
            {
                string runType = ConfigData.GetRunType().ToLower().Trim();
                bool joinOnlyGroupNoPending = false;
                try
                {
                    joinOnlyGroupNoPending = processActionData.GroupConfig.Join.IsJoinOnlyGroupNoPending;
                } catch(Exception) { }
                for (int i = 0; i < joinGroup; i++)
                {
                    if(IsStop())
                    {
                        break;
                    }

                    var groupId = GetGroupId();
                    if(string.IsNullOrEmpty(groupId) )
                    {
                        break;
                    }
                    if (joinOnlyGroupNoPending)
                    {
                        int isPending = 0;

                        do
                        {
                            isPending = IsGrouupPending(driver, groupId);
                            if(isPending == 1)
                            {
                                groupId = GetGroupId();
                                Thread.Sleep(500);
                            }
                        } while (!IsStop() && !string.IsNullOrEmpty(groupId) && isPending == 1);
                        if(isPending == 1)
                        {
                            break;
                        }
                    }

                    var joinAnswer = GetJoinAnswer();
                    if (runType == "web")
                    {
                        try
                        {
                            driver.Navigate().GoToUrl(Constant.FB_WEB_URL + "/" + groupId);
                        }
                        catch (Exception) { }
                        FBTool.WaitingPageLoading(driver);
                        Thread.Sleep(500);
                        WebFBTool.JoinGroup(driver, joinAnswer);
                    }
                    else
                    {
                        try
                        {
                            driver.Navigate().GoToUrl(Constant.FB_MOBILE_URL + "/" + groupId);
                        }
                        catch (Exception) { }
                        FBTool.WaitingPageLoading(driver);
                        FBTool.Scroll(driver, 1000, false);
                        MobileFBTool.JoinGroup(driver, joinAnswer);
                    }
                    if (!FBTool.IsLogin(driver))
                    {
                        var message = FBTool.LoggedIn(driver, data, false);
                        FBTool.WaitingPageLoading(driver);
                        Thread.Sleep(1000);
                        if (message != "success")
                        {
                            data.Status = "Die";
                            form.SetGridDataRowStatus(data);
                            break;
                        }
                    }
                }
            }
        }
        public void BackupGroup()
        {
            this.form.BackupGroups(driver, data);
        }
        public void ViewGroup()
        {
            string des = data.Description;

            int sN = this.form.processActionsData.GroupConfig.View.GroupNumber.NumberStart;
            int eN = this.form.processActionsData.GroupConfig.View.GroupNumber.NumberEnd;

            int viewGroupNumber = GetRankNumber(sN, eN);
            if(viewGroupNumber <= 0)
            {
                return;
            }
            int sT = this.form.processActionsData.GroupConfig.View.ViewTime.NumberStart;
            int eT = this.form.processActionsData.GroupConfig.View.ViewTime.NumberEnd;
            int time = GetRankNumber(sT, eT);

            var sourceFolder = processActionData.GroupConfig.View.SourceFolder;
            string source = "";
            bool isSourceFolderFile = false;
            if (!string.IsNullOrEmpty(sourceFolder))
            {
                try
                {
                    soureFolderFileArr = Directory.GetFiles(sourceFolder);
                    isSourceFolderFile = true;
                }
                catch (Exception) { }
            }
            string runType = ConfigData.GetRunType().ToLower().Trim();
            var groupRecords = groupsDao.GetRecordsByUID(data.UID);
            foreach (DataRow row in groupRecords.Rows)
            {
                string groupId = "";
                try
                {
                    groupId = row["group_id"] + "";
                }
                catch (Exception) { }
                if (string.IsNullOrEmpty(groupId))
                {
                    continue;
                }
                string caption = GetCaption();
                if (runType == "web" || true)
                {
                    try
                    {
                        driver.Navigate().GoToUrl(Constant.FB_WEB_URL + "/" + groupId);
                    }
                    catch (Exception) { }
                    FBTool.WaitingPageLoading(driver);
                    Thread.Sleep(500);
                    if(isSourceFolderFile)
                    {
                        source = GetSourceFile();
                    }
                    if (!string.IsNullOrEmpty(caption) || !string.IsNullOrEmpty(source))
                    {
                        WebFBTool.PostInGroup(driver, caption, source);
                    }
                }
                else
                {
                    try
                    {
                        driver.Navigate().GoToUrl(Constant.FB_MOBILE_URL + "/" + groupId);
                    }
                    catch (Exception) { }
                    FBTool.WaitingPageLoading(driver);
                    FBTool.Scroll(driver, 1000, false);

                }

                if (time > 0)
                {
                    string comment = GetComment();
                    bool isLike = false;
                    do
                    {
                        if(!isLike)
                        {
                            isLike = true;
                            if(processActionData.GroupConfig.View.React.Like)
                            {
                                WebFBTool.LikePost(driver);
                            } else if(processActionData.GroupConfig.View.React.Random && random.Next(0,2) == 1)
                            {
                                WebFBTool.LikePost(driver);
                            }
                        }
                        if(!string.IsNullOrEmpty(comment))
                        {
                            WebFBTool.PostComment(driver, comment);
                            comment = "";
                        }
                        time--;
                        FBTool.Scroll(driver, 1000,false);
                    } while (!IsStop() && time > 0);
                }               
            }
        }
        public string GetSourceFile()
        {
            string str = "";
            try
            {
                if (sourceFolderFileIndex >= soureFolderFileArr.Length)
                {
                    sourceFolderFileIndex = 0;
                }
                str = soureFolderFileArr[sourceFolderFileIndex];
                sourceFolderFileIndex++;

                this.form.cacheViewModel.GetCacheDao().Set("group:view:source_index", sourceFolderFileIndex + "");
            }
            catch (Exception) { }

            return str;
        }
        public int GetRankNumber(int min, int max)
        {
            return new Random().Next(min, max);
        }
        public string GetComment()
        {
            string str = "";
            try
            {
                str = commentArr[random.Next(0, commentArr.Length - 1)];
            }
            catch (Exception) { }

            return str;
        }
        public string GetCaption()
        {
            string str = "";
            try
            {
                str = captionArr[random.Next(0, captionArr.Length - 1)];
            }
            catch (Exception) { }

            return str;
        }
        public string GetJoinAnswer()
        {
            string answer = "";
            try
            {
                answer = joinAnswerArr[random.Next(0, joinAnswerArr.Length - 1)];
            }
            catch (Exception) { }

            return answer;
        }
        public string GetGroupId()
        {
            return this.form.GetJoinGroupID();
        }
        public bool IsStop()
        {
            return this.form.IsStop();
        }
    }
}
