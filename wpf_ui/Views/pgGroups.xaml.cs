using Newtonsoft.Json;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToolKHBrowser.ViewModels;
using ToolKHBrowser;
using ToolKHBrowser.ViewModels;

namespace ToolKHBrowser.Views
{
    /// <summary>
    /// Interaction logic for pgGroups.xaml
    /// </summary>
    public partial class pgGroups : Page
    {
        private readonly ICacheViewModel cacheViewModel = DIConfig.Get<ICacheViewModel>();

        public pgGroups()
        {
            InitializeComponent();
            LoadData();
        }

        public void LoadData()
        {
            try
            {
                var cache = cacheViewModel.GetCacheDao().Get("group:config");
                if (cache == null || cache.Value == null) return;

                var str = cache.Value.ToString();
                if (string.IsNullOrEmpty(str)) return;

                GroupConfig groupObj = null;
                try { groupObj = JsonConvert.DeserializeObject<GroupConfig>(str); }
                catch { }

                if (groupObj == null) return;

                var cacheIds = cacheViewModel.GetCacheDao().Get("group:config:group_ids");
                var groupIds = cacheIds != null && cacheIds.Value != null ? cacheIds.Value.ToString() : "";

                // JOIN
                if (groupObj.Join != null)
                {
                    try
                    {
                        txtGroupJoinOfNumber.Value = Int32.Parse(groupObj.Join.NumberOfJoin.ToString());
                        txtGroupAnswer.Text = groupObj.Join.Answers;
                        txtGroupIDs.Text = groupIds;
                        chbJoinOnlyGroupNoPending.IsChecked = groupObj.Join.IsJoinOnlyGroupNoPending;
                    }
                    catch { }
                }
                else
                {
                    // still load ids if join is null
                    txtGroupIDs.Text = groupIds;
                }

                // LEAVE
                if (groupObj.Leave != null)
                {
                    try
                    {
                        chbLeaveGroupMembership.IsChecked = groupObj.Leave.IsMembership;
                        chbLeaveGroupByID.IsChecked = groupObj.Leave.IsID;
                        chbLeaveGroupPending.IsChecked = groupObj.Leave.LeavePendingOnly;
                    }
                    catch { }

                    try
                    {
                        chbLeaveGroupPendingDetectByScript.IsChecked = groupObj.Leave.LeavePendingScriptDetect;
                    }
                    catch { }
                }

                // VIEW
                if (groupObj.View != null)
                {
                    try
                    {
                        if (groupObj.View.React != null)
                        {
                            chbReachNone.IsChecked = groupObj.View.React.None;
                            chbReachLike.IsChecked = groupObj.View.React.Like;
                            chbReachRandom.IsChecked = groupObj.View.React.Random;
                        }
                    }
                    catch { }

                    try
                    {
                        if (groupObj.View.GroupNumber != null)
                        {
                            txtViewGroupNumberStart.Value = Int32.Parse(groupObj.View.GroupNumber.NumberStart.ToString());
                            txtViewGroupNumberEnd.Value = Int32.Parse(groupObj.View.GroupNumber.NumberEnd.ToString());
                        }
                    }
                    catch { }

                    try
                    {
                        if (groupObj.View.ViewTime != null)
                        {
                            txtViewGroupTimeStart.Value = Int32.Parse(groupObj.View.ViewTime.NumberStart.ToString());
                            txtViewGroupTimeEnd.Value = Int32.Parse(groupObj.View.ViewTime.NumberEnd.ToString());
                        }
                    }
                    catch { }

                    try
                    {
                        txtGroupSourceFolder.Text = groupObj.View.SourceFolder;
                        txtGroupCaption.Text = groupObj.View.Captions;
                        txtViewGroupComments.Text = groupObj.View.Comments;
                    }
                    catch { }
                }

                // BACKUP
                if (groupObj.Backup != null)
                {
                    try
                    {
                        chbBackupGroupByToken.IsChecked = groupObj.Backup.IsToken;
                        chbBackupGroupOnBrowser.IsChecked = groupObj.Backup.IsBrowser;
                    }
                    catch { }

                    try
                    {
                        chbBackupNewGroup.IsChecked = groupObj.Backup.BackupNewGroup;
                    }
                    catch { }
                }
            }
            catch
            {
                // ignore any unexpected UI/cache errors
            }
        }

        private void btnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sourceFolder = (txtGroupSourceFolder.Text ?? "").Trim();
                var comment = txtViewGroupComments.Text ?? "";
                var caption = txtGroupCaption.Text ?? "";

                var groupNumberStart = SafeInt(txtViewGroupNumberStart.Value);
                var groupNumberEnd = SafeInt(txtViewGroupNumberEnd.Value);

                var timeStart = SafeInt(txtViewGroupTimeStart.Value);
                var timeEnd = SafeInt(txtViewGroupTimeEnd.Value);

                var numberOfJoin = SafeInt(txtGroupJoinOfNumber.Value);
                var answers = txtGroupAnswer.Text ?? "";

                // ✅ Normalize & clean group list before saving
                var groupIdsRaw = txtGroupIDs.Text ?? "";
                var groupIdsClean = NormalizeGroupIdsMultiline(groupIdsRaw);

                var isJoinOnlyGroupNoPending = chbJoinOnlyGroupNoPending.IsChecked == true;

                var reactShareNone = chbReachNone.IsChecked == true;
                var reactShareLike = chbReachLike.IsChecked == true;
                var reactShareRandom = chbReachRandom.IsChecked == true;

                var isToken = chbBackupGroupByToken.IsChecked == true;
                var isBrowser = chbBackupGroupOnBrowser.IsChecked == true;
                var backupNewGroup = chbBackupNewGroup.IsChecked == true;

                var isLeaveMembership = chbLeaveGroupMembership.IsChecked == true;
                var isLeaveById = chbLeaveGroupByID.IsChecked == true;
                var isLeavePendingOnly = chbLeaveGroupPending.IsChecked == true;
                var isLeavePendingScriptDetect = chbLeaveGroupPendingDetectByScript.IsChecked == true;

                GroupConfig groupObj = new GroupConfig();

                GroupConfigJoin joinObj = new GroupConfigJoin();
                GroupConfigLeave leaveObj = new GroupConfigLeave();
                GroupConfigView viewObj = new GroupConfigView();
                GroupConfigBackup backupObj = new GroupConfigBackup();
                GroupConfigViewTime timeObj = new GroupConfigViewTime();
                GroupConfigNumber numberObj = new GroupConfigNumber();
                GroupConfigViewReact reactObj = new GroupConfigViewReact();

                // JOIN
                joinObj.NumberOfJoin = numberOfJoin;
                joinObj.Answers = answers;
                joinObj.IsJoinOnlyGroupNoPending = isJoinOnlyGroupNoPending;

                // LEAVE
                leaveObj.IsMembership = isLeaveMembership;
                leaveObj.IsID = isLeaveById;
                leaveObj.LeavePendingOnly = isLeavePendingOnly;
                leaveObj.LeavePendingScriptDetect = isLeavePendingScriptDetect;

                // VIEW TIME
                timeObj.NumberStart = timeStart;
                timeObj.NumberEnd = timeEnd;
                viewObj.ViewTime = timeObj;

                // GROUP NUMBER
                numberObj.NumberStart = groupNumberStart;
                numberObj.NumberEnd = groupNumberEnd;
                viewObj.GroupNumber = numberObj;

                // REACT
                reactObj.Random = reactShareRandom;
                reactObj.Like = reactShareLike;
                reactObj.None = reactShareNone;
                viewObj.React = reactObj;

                // VIEW CONTENT
                viewObj.SourceFolder = sourceFolder;
                viewObj.Captions = caption;
                viewObj.Comments = comment;

                // BACKUP
                backupObj.IsToken = isToken;
                backupObj.IsBrowser = isBrowser;
                backupObj.BackupNewGroup = backupNewGroup;

                groupObj.Join = joinObj;
                groupObj.Leave = leaveObj;
                groupObj.Backup = backupObj;
                groupObj.View = viewObj;

                string output = JsonConvert.SerializeObject(groupObj);

                cacheViewModel.GetCacheDao().Set("group:config", output);
                cacheViewModel.GetCacheDao().Set("group:config:group_ids", groupIdsClean);

                // ✅ update textbox to cleaned list (so user sees correct data)
                txtGroupIDs.Text = groupIdsClean;

                MessageBox.Show("Your config has been save successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Save Config Error");
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private int SafeInt(object value)
        {
            if (value == null) return 0;
            int n;
            if (int.TryParse(value.ToString(), out n)) return n;
            return 0;
        }

        private void btnGroupSourceFolderBrowse_Click(object sender, RoutedEventArgs e)
        {
            BrowseFolderInto(txtGroupSourceFolder);
        }

        private void BrowseFolderInto(TextBox target)
        {
            if (target == null) return;

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select source file",
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
                Filter = "Media files|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.mp4;*.mov;*.avi;*.mkv;*.webm|All files|*.*"
            };

            try
            {
                var currentPath = (target.Text ?? "").Trim();
                if (System.IO.File.Exists(currentPath))
                {
                    dialog.InitialDirectory = System.IO.Path.GetDirectoryName(currentPath);
                    dialog.FileName = System.IO.Path.GetFileName(currentPath);
                }
                else if (System.IO.Directory.Exists(currentPath))
                {
                    dialog.InitialDirectory = currentPath;
                }
            }
            catch { }

            if (dialog.ShowDialog() == true)
            {
                target.Text = dialog.FileName;
            }
        }

        // ✅ Convert multiline input into normalized urls (one per line)
        private string NormalizeGroupIdsMultiline(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";

            var sb = new StringBuilder();
            var lines = raw.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length; i++)
            {
                var url = NormalizeFacebookGroupUrl(lines[i]);
                if (string.IsNullOrEmpty(url)) continue;

                if (sb.Length > 0) sb.AppendLine();
                sb.Append(url);
            }

            return sb.ToString();
        }

        // ✅ Your Normalize method (kept + used on Save)
        private string NormalizeFacebookGroupUrl(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            var s = input.Trim();

            // If full URL
            if (s.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                int q = s.IndexOf('?');
                if (q > 0)
                    s = s.Substring(0, q);

                // Extract only /groups/{id}
                var match = System.Text.RegularExpressions.Regex.Match(
                    s,
                    @"facebook\.com/groups/([^/]+)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    var id = match.Groups[1].Value;
                    return "https://www.facebook.com/groups/" + id + "/";
                }

                return s;
            }

            // Only ID
            return "https://www.facebook.com/groups/" + s + "/";
        }

    }
}
