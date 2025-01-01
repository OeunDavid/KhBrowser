using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ToolKHBrowser.ViewModels;
using WpfUI;
using WpfUI.ViewModels;

namespace ToolKHBrowser.Views
{
    /// <summary>
    /// Interaction logic for pgGroups.xaml
    /// </summary>
    public partial class pgGroups : Page
    {
        ICacheViewModel cacheViewModel = DIConfig.Get<ICacheViewModel>();

        public pgGroups()
        {
            InitializeComponent();

            LoadData();
        }
        public void LoadData()
        {
            var str = cacheViewModel.GetCacheDao().Get("group:config").Value.ToString();
            if (!string.IsNullOrEmpty(str))
            {
                GroupConfig groupObj = JsonConvert.DeserializeObject<GroupConfig>(str);
                var groupIds = cacheViewModel.GetCacheDao().Get("group:config:group_ids").Value.ToString();

                try
                {
                    txtGroupJoinOfNumber.Value = Int32.Parse(groupObj.Join.NumberOfJoin.ToString());
                    txtGroupAnswer.Text = groupObj.Join.Answers;
                    txtGroupIDs.Text = groupIds;
                }
                catch (Exception) { }
                try
                {

                    chbJoinOnlyGroupNoPending.IsChecked = groupObj.Join.IsJoinOnlyGroupNoPending;
                }
                catch (Exception) { }
                try
                {
                    chbLeaveGroupMembership.IsChecked = groupObj.Leave.IsMembership;
                    chbLeaveGroupByID.IsChecked = groupObj.Leave.IsID;
                    chbLeaveGroupPending.IsChecked = groupObj.Leave.LeavePendingOnly;
                }
                catch (Exception) { }
                try
                {
                    chbLeaveGroupPendingDetectByScript.IsChecked = groupObj.Leave.LeavePendingScriptDetect;
                }
                catch (Exception) { }
                try
                {
                    chbReachNone.IsChecked = groupObj.View.React.None;
                    chbReachLike.IsChecked = groupObj.View.React.Like;
                    chbReachRandom.IsChecked = groupObj.View.React.Random;
                }
                catch (Exception) { }

                try
                {
                    chbBackupGroupByToken.IsChecked = groupObj.Backup.IsToken ;
                    chbBackupGroupOnBrowser.IsChecked = groupObj.Backup.IsBrowser;
                }
                catch (Exception) { }
                try
                {
                    chbBackupNewGroup.IsChecked = groupObj.Backup.BackupNewGroup;
                }
                catch (Exception) { }
                try
                {
                    txtViewGroupNumberStart.Value = Int32.Parse(groupObj.View.GroupNumber.NumberStart.ToString());
                    txtViewGroupNumberEnd.Value = Int32.Parse(groupObj.View.GroupNumber.NumberEnd.ToString());
                }
                catch (Exception) { }
                try
                {
                    txtViewGroupTimeStart.Value = Int32.Parse(groupObj.View.ViewTime.NumberStart.ToString());
                    txtViewGroupTimeEnd.Value = Int32.Parse(groupObj.View.ViewTime.NumberEnd.ToString());
                }
                catch (Exception) { }
                try
                {
                    txtGroupSourceFolder.Text = groupObj.View.SourceFolder;
                    txtGroupCaption.Text = groupObj.View.Captions;
                    txtViewGroupComments.Text = groupObj.View.Comments;
                }
                catch (Exception) { }
            }
        }

        private void btnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            var sourceFolder = txtGroupSourceFolder.Text.Trim();
            var comment = txtViewGroupComments.Text;
            var caption = txtGroupCaption.Text;

            var groupNumberStart = Int32.Parse(txtViewGroupNumberStart.Value.ToString());
            var groupNumberEnd = Int32.Parse(txtViewGroupNumberEnd.Value.ToString());

            var timeStart = Int32.Parse(txtViewGroupTimeStart.Value.ToString());
            var timeEnd = Int32.Parse(txtViewGroupTimeEnd.Value.ToString());

            var numberOfJoin = Int32.Parse(txtGroupJoinOfNumber.Value.ToString());
            var answers = txtGroupAnswer.Text;
            var groupIds = txtGroupIDs.Text;
            var isJoinOnlyGroupNoPending = chbJoinOnlyGroupNoPending.IsChecked.Value;

            var reactShareNone = chbReachNone.IsChecked.Value;
            var reactShareLike = chbReachLike.IsChecked.Value;
            var reactShareRandom = chbReachRandom.IsChecked.Value;

            var isToken = chbBackupGroupByToken.IsChecked.Value;
            var isBrowser = chbBackupGroupOnBrowser.IsChecked.Value;
            var backupNewGroup = chbBackupNewGroup.IsChecked.Value;

            var isLeaveMembership = chbLeaveGroupMembership.IsChecked.Value;
            var isLeaveById = chbLeaveGroupByID.IsChecked.Value;
            var isLeavePendingOnly = chbLeaveGroupPending.IsChecked.Value;
            var isLeavePendingScriptDetect = chbLeaveGroupPendingDetectByScript.IsChecked.Value;

            GroupConfig groupObj = new GroupConfig();
            GroupConfigJoin joinObj = new GroupConfigJoin();
            GroupConfigLeave leaveObj = new GroupConfigLeave();
            GroupConfigView viewObj = new GroupConfigView();
            GroupConfigBackup backupObj = new GroupConfigBackup();
            GroupConfigViewTime timeObj = new GroupConfigViewTime();
            GroupConfigNumber numberObj = new GroupConfigNumber();
            GroupConfigViewReact reactObj = new GroupConfigViewReact();

            joinObj.NumberOfJoin = numberOfJoin;
            joinObj.Answers = answers;
            joinObj.IsJoinOnlyGroupNoPending = isJoinOnlyGroupNoPending;

            leaveObj.IsMembership = isLeaveMembership;
            leaveObj.IsID = isLeaveById;
            leaveObj.LeavePendingOnly = isLeavePendingOnly;
            leaveObj.LeavePendingScriptDetect = isLeavePendingScriptDetect;

            timeObj.NumberStart = timeStart;
            timeObj.NumberEnd = timeEnd;
            viewObj.ViewTime = timeObj;

            numberObj.NumberStart = groupNumberStart;
            numberObj.NumberEnd = groupNumberEnd;
            viewObj.GroupNumber = numberObj;

            reactObj.Random = reactShareRandom;
            reactObj.Like = reactShareLike;
            reactObj.None = reactShareNone;
            viewObj.React = reactObj;

            viewObj.SourceFolder = sourceFolder;
            viewObj.Captions = caption;
            viewObj.Comments = comment;

            backupObj.IsToken = isToken;
            backupObj.IsBrowser = isBrowser;
            backupObj.BackupNewGroup = backupNewGroup;

            groupObj.Join = joinObj;
            groupObj.Leave = leaveObj;
            groupObj.Backup = backupObj;
            groupObj.View = viewObj;

            string output = JsonConvert.SerializeObject(groupObj);

            cacheViewModel.GetCacheDao().Set("group:config", output);
            cacheViewModel.GetCacheDao().Set("group:config:group_ids", groupIds);

            MessageBox.Show("Your config has been save successfully.");
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
