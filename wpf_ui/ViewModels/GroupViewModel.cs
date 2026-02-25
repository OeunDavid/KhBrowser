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
using ToolKHBrowser;
using ToolKHBrowser.ToolLib.Data;
using ToolKHBrowser.ViewModels;
using ToolKHBrowser.Views;
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
        void PostGroup();
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
        //public void Start(frmMain form,IWebDriver driver, FbAccount data)
        //{
        //    this.form = form;
        //    this.data = data;
        //    this.driver = driver;
        //    this.processActionData = this.form.processActionsData;
        //    sourceFolderFileIndex = 0;
        //    try
        //    {
        //        sourceFolderFileIndex = Int32.Parse(this.form.cacheViewModel.GetCacheDao().Get("group:view:source_index").Value.ToString());
        //    }
        //    catch (Exception) { }

        //    random = new Random();
        //    try
        //    {
        //        joinAnswerArr = this.processActionData.GroupConfig.Join.Answers.Split('\n');
        //    }
        //    catch (Exception) { }
        //    try
        //    {
        //        commentArr = this.processActionData.GroupConfig.View.Comments.Split('\n');
        //    }
        //    catch (Exception) { }
        //    try
        //    {
        //        captionArr = this.processActionData.GroupConfig.View.Captions.Split('\n');
        //    }
        //    catch (Exception) { }
        //}

        public void Start(frmMain form, IWebDriver driver, FbAccount data)
        {
            if (form == null)
                throw new ArgumentNullException("form");

            this.form = form;
            this.data = data;
            this.driver = driver;

            sourceFolderFileIndex = 0;
            joinAnswerArr = new string[0];
            commentArr = new string[0];
            captionArr = new string[0];

            random = new Random();

            this.processActionData = this.form.processActionsData;

            // SAFE cacheDao
            var cacheDao = this.form.cacheViewModel != null
                ? this.form.cacheViewModel.GetCacheDao()
                : null;

            try
            {
                if (cacheDao != null)
                {
                    var item = cacheDao.Get("group:view:source_index");
                    if (item != null && item.Value != null)
                    {
                        int idx;
                        if (int.TryParse(item.Value.ToString(), out idx))
                            sourceFolderFileIndex = idx;
                    }
                }
            }
            catch { }

            // SAFE join answers
            try
            {
                if (this.processActionData != null &&
                    this.processActionData.GroupConfig != null &&
                    this.processActionData.GroupConfig.Join != null &&
                    !string.IsNullOrWhiteSpace(this.processActionData.GroupConfig.Join.Answers))
                {
                    joinAnswerArr = this.processActionData.GroupConfig.Join.Answers
                        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            catch { }

            // SAFE comments
            try
            {
                if (this.processActionData != null &&
                    this.processActionData.GroupConfig != null &&
                    this.processActionData.GroupConfig.View != null &&
                    !string.IsNullOrWhiteSpace(this.processActionData.GroupConfig.View.Comments))
                {
                    commentArr = this.processActionData.GroupConfig.View.Comments
                        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            catch { }

            // SAFE captions
            try
            {
                if (this.processActionData != null &&
                    this.processActionData.GroupConfig != null &&
                    this.processActionData.GroupConfig.View != null &&
                    !string.IsNullOrWhiteSpace(this.processActionData.GroupConfig.View.Captions))
                {
                    captionArr = this.processActionData.GroupConfig.View.Captions
                        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            catch { }
        }


        public void LeaveGroup()
        {
            if (processActionData == null || processActionData.GroupConfig == null || processActionData.GroupConfig.Leave == null)
                return;

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
                                driver.Navigate().GoToUrl(FBTool.GetSafeGroupUrl(Constant.FB_WEB_URL, groupId));
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
                            driver.Navigate().GoToUrl(FBTool.GetSafeGroupUrl(Constant.FB_WEB_URL, groupId));
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
                            driver.Navigate().GoToUrl(FBTool.GetSafeGroupUrl(Constant.FB_WEB_URL, groupId));
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
                string url = FBTool.GetSafeGroupUrl("https://m.facebook.com", groupId);
                if (!url.EndsWith("/")) url += "/";
                driver.Navigate().GoToUrl(url + "madminpanel/pending");
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
            if (processActionData == null || processActionData.GroupConfig == null || processActionData.GroupConfig.Join == null)
                return;

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
                            driver.Navigate().GoToUrl(FBTool.GetSafeGroupUrl(Constant.FB_WEB_URL, groupId));
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
                            driver.Navigate().GoToUrl(FBTool.GetSafeGroupUrl(Constant.FB_MOBILE_URL, groupId));
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
        public void PostGroup()
        {
            if (processActionData == null || processActionData.GroupConfig == null)
                return;

            string runType = ConfigData.GetRunType().ToLower().Trim();
            var groupRecords = groupsDao.GetRecordsByUID(data.UID);
            foreach (DataRow row in groupRecords.Rows)
            {
                if (IsStop()) break;

                string groupId = "";
                try
                {
                    groupId = row["group_id"] + "";
                }
                catch (Exception) { }

                if (string.IsNullOrEmpty(groupId)) continue;

                string caption = GetCaption();
                // string source = GetSourceFile(); 

                if (runType == "web" || true)
                {
                    try
                    {
                        driver.Navigate().GoToUrl(FBTool.GetSafeGroupUrl(Constant.FB_WEB_URL, groupId));
                    }
                    catch (Exception) { }
                    FBTool.WaitingPageLoading(driver);
                    Thread.Sleep(2000);
                    
                    if (!string.IsNullOrEmpty(caption))
                    {
                        WebFBTool.PostInGroup(driver, caption);
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
            if (processActionData == null || processActionData.GroupConfig == null || processActionData.GroupConfig.View == null)
            {
                try
                {
                    data.Description = "View Group: missing group view config";
                    form.SetGridDataRowStatus(data);
                }
                catch { }
                return;
            }

            var viewCfg = processActionData.GroupConfig.View;
            int sN = 1, eN = 1;
            int sT = 1, eT = 1;
            try
            {
                if (viewCfg.GroupNumber != null)
                {
                    sN = viewCfg.GroupNumber.NumberStart;
                    eN = viewCfg.GroupNumber.NumberEnd;
                }
            }
            catch { }
            try
            {
                if (viewCfg.ViewTime != null)
                {
                    sT = viewCfg.ViewTime.NumberStart;
                    eT = viewCfg.ViewTime.NumberEnd;
                }
            }
            catch { }

            int viewGroupNumber = GetRankNumber(sN, eN);
            if(viewGroupNumber <= 0)
            {
                return;
            }

            var sourceFolder = viewCfg.SourceFolder;
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
            var targetGroupIds = new List<string>();
            try
            {
                var groupRecords = groupsDao.GetRecordsByUID(data.UID);
                if (groupRecords != null)
                {
                    foreach (DataRow row in groupRecords.Rows)
                    {
                        try
                        {
                            var gid = (row["group_id"] + "").Trim();
                            if (!string.IsNullOrWhiteSpace(gid))
                                targetGroupIds.Add(gid);
                        }
                        catch { }
                    }
                }
            }
            catch { }

            // Fallback to Groups config textbox list when account DB has no backed-up groups yet.
            if (targetGroupIds.Count == 0)
            {
                try
                {
                    var cfgIds = this.form.GetJoinGroupIDArr();
                    if (cfgIds != null)
                    {
                        foreach (var gid in cfgIds)
                        {
                            if (!string.IsNullOrWhiteSpace(gid))
                                targetGroupIds.Add(gid.Trim());
                        }
                    }
                }
                catch { }
            }

            if (targetGroupIds.Count == 0)
            {
                try
                {
                    data.Description = "View Group: no targets (backup groups or Group IDs list)";
                    form.SetGridDataRowStatus(data);
                }
                catch { }
                return;
            }

            int processed = 0;
            foreach (var groupIdRaw in targetGroupIds)
            {
                if (IsStop() || processed >= viewGroupNumber)
                {
                    break;
                }

                string groupId = groupIdRaw;
                if (string.IsNullOrEmpty(groupId))
                {
                    continue;
                }
                processed++;

                string caption = GetCaption();
                int time = GetRankNumber(sT, eT);
                if (runType != "mobile")
                {
                    try
                    {
                        data.Description = "View Group: " + groupId;
                        form.SetGridDataRowStatus(data);
                    }
                    catch { }

                    try
                    {
                        driver.Navigate().GoToUrl(FBTool.GetSafeGroupUrl(Constant.FB_WEB_URL, groupId));
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
                        data.Description = "View Group(m): " + groupId;
                        form.SetGridDataRowStatus(data);
                    }
                    catch { }

                    try
                    {
                        driver.Navigate().GoToUrl(FBTool.GetSafeGroupUrl(Constant.FB_MOBILE_URL, groupId));
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
                        if (IsStop()) break;
                        if(!isLike)
                        {
                            isLike = true;
                            if(viewCfg.React != null && viewCfg.React.Like)
                            {
                                WebFBTool.LikePost(driver);
                            } else if(viewCfg.React != null && viewCfg.React.Random && random.Next(0,2) == 1)
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
            if (min < 0) min = 0;
            if (max < min) max = min;
            if (min == max) return min;
            return random != null ? random.Next(min, max + 1) : new Random().Next(min, max + 1);
        }
        public string GetComment()
        {
            string str = "";
            try
            {
                if (commentArr != null && commentArr.Length > 0)
                {
                    str = commentArr[random.Next(0, commentArr.Length)];
                }
            }
            catch (Exception) { }

            return str;
        }
        public string GetCaption()
        {
            string str = "";
            try
            {
                if (captionArr != null && captionArr.Length > 0)
                {
                    str = captionArr[random.Next(0, captionArr.Length)];
                }
            }
            catch (Exception) { }

            return str;
        }
        public string GetJoinAnswer()
        {
            string answer = "";
            try
            {
                if (joinAnswerArr != null && joinAnswerArr.Length > 0)
                {
                    answer = joinAnswerArr[random.Next(0, joinAnswerArr.Length)];
                }
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
