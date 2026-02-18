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
using WpfUI.ViewModels;
using WpfUI;
using OpenPop.Mime;

namespace ToolKHBrowser.Views
{
    /// <summary>
    /// Interaction logic for pgNewsFeed.xaml
    /// </summary>
    public partial class pgNewsFeed : Page
    {
        ICacheViewModel cacheViewModel = DIConfig.Get<ICacheViewModel>();

        public pgNewsFeed()
        {
            InitializeComponent();

            LoadData();
        }
        //public void LoadData()
        //{
        //    var cache = cacheViewModel.GetCacheDao().Get("newsfeed:config");
        //    if (cache != null && cache.Value != null)
        //    {
        //        string str = cache.Value.ToString();
        //        if (!string.IsNullOrEmpty(str))
        //        {
        //            NewsFeedConfig newsfeedObj = JsonConvert.DeserializeObject<NewsFeedConfig>(str);

        //            try
        //            {
        //                txtPlayTimeStart.Value = Int32.Parse(newsfeedObj.NewsFeed.PlayTime.NumberStart.ToString());
        //                txtPlayTimeEnd.Value = Int32.Parse(newsfeedObj.NewsFeed.PlayTime.NumberEnd.ToString());
        //            }
        //            catch (Exception) { }
        //            try
        //            {
        //                chbReachNone.IsChecked = newsfeedObj.NewsFeed.React.None;
        //                chbReachLike.IsChecked = newsfeedObj.NewsFeed.React.Like;
        //                chbReachComment.IsChecked = newsfeedObj.NewsFeed.React.Comment;
        //                chbReachRandom.IsChecked = newsfeedObj.NewsFeed.React.Random;

        //                txtComments.Text = newsfeedObj.NewsFeed.Comments;
        //            }
        //            catch (Exception) { }
        //            try
        //            {
        //                txtSourceFolder.Text = newsfeedObj.Timeline.SourceFolder;
        //                txtCaption.Text = newsfeedObj.Timeline.Captions;
        //            }
        //            catch (Exception) { }
        //            try
        //            {
        //                txtTimelineMinNumber.Value = Int32.Parse(newsfeedObj.Timeline.MinNumber.ToString());
        //                txtTimelineMaxNumber.Value = Int32.Parse(newsfeedObj.Timeline.MaxNumber.ToString());
        //                chbDeleteAfterPost.IsChecked = newsfeedObj.Timeline.DeleteAfterPost;
        //            }
        //            catch (Exception) { }
        //            try
        //            {
        //                chbMessageSoundNone.IsChecked = newsfeedObj.Messenger.MessageSound.None;
        //                chbMessageSoundOn.IsChecked = newsfeedObj.Messenger.MessageSound.On;
        //                chbMessageSoundOff.IsChecked = newsfeedObj.Messenger.MessageSound.Off;
        //            }
        //            catch (Exception) { }
        //            try
        //            {
        //                chbCallSoundNone.IsChecked = newsfeedObj.Messenger.MessageCallSound.None;
        //                chbCallSoundOn.IsChecked = newsfeedObj.Messenger.MessageCallSound.On;
        //                chbCallSoundOff.IsChecked = newsfeedObj.Messenger.MessageCallSound.Off;
        //            }
        //            catch (Exception) { }
        //            try
        //            {
        //                chbPopUpSoundNone.IsChecked = newsfeedObj.Messenger.MessagePopup.None;
        //                chbPopUpSoundOn.IsChecked = newsfeedObj.Messenger.MessagePopup.On;
        //                chbPopUpSoundOff.IsChecked = newsfeedObj.Messenger.MessagePopup.Off;
        //            }
        //            catch (Exception) { }
        //            try
        //            {
        //                chbActiveStatusNone.IsChecked = newsfeedObj.Messenger.ActiveStatus.None;
        //                chbActiveStatusOn.IsChecked = newsfeedObj.Messenger.ActiveStatus.On;
        //                chbActiveStatusOff.IsChecked = newsfeedObj.Messenger.ActiveStatus.Off;
        //            }
        //            catch (Exception) { }
        //            try
        //            {
        //                txtPagePostIds.Text = newsfeedObj.PagePost.PageIds;
        //                txtPagePostSourceFolder.Text = newsfeedObj.PagePost.SourceFolder;
        //                txtPagePostCaption.Text = newsfeedObj.PagePost.Captions;
        //                txtPagePostMinNumber.Value = Int32.Parse(newsfeedObj.PagePost.MinPosts.ToString());
        //                txtPagePostMaxNumber.Value = Int32.Parse(newsfeedObj.PagePost.MaxPosts.ToString());
        //            }
        //            catch (Exception) { }
        //        }
        //    }
        //}
        public void LoadData()
        {
            if (cacheViewModel?.GetCacheDao() == null) return; // or log

            var cache = cacheViewModel.GetCacheDao().Get("newsfeed:config");
            var str = cache?.Value?.ToString();
            if (string.IsNullOrWhiteSpace(str)) return;

            NewsFeedConfig newsfeedObj;
            try
            {
                newsfeedObj = JsonConvert.DeserializeObject<NewsFeedConfig>(str);
            }
            catch
            {
                return; // invalid JSON
            }

            if (newsfeedObj == null) return;

            // Helper local function
            int ToInt(object v, int def = 0) => int.TryParse(v?.ToString(), out var x) ? x : def;

            // NEWSFEED
            var nf = newsfeedObj.NewsFeed;
            if (nf?.PlayTime != null)
            {
                txtPlayTimeStart.Value = ToInt(nf.PlayTime.NumberStart);
                txtPlayTimeEnd.Value = ToInt(nf.PlayTime.NumberEnd);
            }

            if (nf?.React != null)
            {
                chbReachNone.IsChecked = nf.React.None;
                chbReachLike.IsChecked = nf.React.Like;
                chbReachComment.IsChecked = nf.React.Comment;
                chbReachRandom.IsChecked = nf.React.Random;
            }
            txtComments.Text = nf?.Comments ?? "";

            // TIMELINE
            var tl = newsfeedObj.Timeline;
            if (tl != null)
            {
                txtSourceFolder.Text = tl.SourceFolder ?? "";
                txtCaption.Text = tl.Captions ?? "";
                txtTimelineMinNumber.Value = ToInt(tl.MinNumber);
                txtTimelineMaxNumber.Value = ToInt(tl.MaxNumber);
                chbDeleteAfterPost.IsChecked = tl.DeleteAfterPost;
            }

            // MESSENGER
            var ms = newsfeedObj.Messenger;
            if (ms?.MessageSound != null)
            {
                chbMessageSoundNone.IsChecked = ms.MessageSound.None;
                chbMessageSoundOn.IsChecked = ms.MessageSound.On;
                chbMessageSoundOff.IsChecked = ms.MessageSound.Off;
            }

            if (ms?.MessageCallSound != null)
            {
                chbCallSoundNone.IsChecked = ms.MessageCallSound.None;
                chbCallSoundOn.IsChecked = ms.MessageCallSound.On;
                chbCallSoundOff.IsChecked = ms.MessageCallSound.Off;
            }

            if (ms?.MessagePopup != null)
            {
                chbPopUpSoundNone.IsChecked = ms.MessagePopup.None;
                chbPopUpSoundOn.IsChecked = ms.MessagePopup.On;
                chbPopUpSoundOff.IsChecked = ms.MessagePopup.Off;
            }

            if (ms?.ActiveStatus != null)
            {
                chbActiveStatusNone.IsChecked = ms.ActiveStatus.None;
                chbActiveStatusOn.IsChecked = ms.ActiveStatus.On;
                chbActiveStatusOff.IsChecked = ms.ActiveStatus.Off;
            }

            // PAGE POST
            var pp = newsfeedObj.PagePost;
            if (pp != null)
            {
                txtPagePostIds.Text = pp.PageIds ?? "";
                txtPagePostSourceFolder.Text = pp.SourceFolder ?? "";
                txtPagePostCaption.Text = pp.Captions ?? "";
                txtPagePostMinNumber.Value = ToInt(pp.MinPosts);
                txtPagePostMaxNumber.Value = ToInt(pp.MaxPosts);
            }
        }


        private void btnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            NumberRank numberObj = new NumberRank();
            numberObj.NumberStart = Int32.Parse(txtPlayTimeStart.Value.ToString());
            numberObj.NumberEnd = Int32.Parse(txtPlayTimeEnd.Value.ToString());

            React reactObj = new React();
            reactObj.None = chbReachNone.IsChecked.Value;
            reactObj.Like = chbReachLike.IsChecked.Value;
            reactObj.Comment = chbReachComment.IsChecked.Value;
            reactObj.Random = chbReachRandom.IsChecked.Value;

            MessageCallSound callSoundObj = new MessageCallSound();
            callSoundObj.None = chbCallSoundNone.IsChecked.Value;
            callSoundObj.On = chbCallSoundOn.IsChecked.Value;
            callSoundObj.Off = chbCallSoundOff.IsChecked.Value;

            MessageSound soundObj = new MessageSound();
            soundObj.None = chbMessageSoundNone.IsChecked.Value;
            soundObj.On = chbMessageSoundOn.IsChecked.Value;
            soundObj.Off = chbMessageSoundOff.IsChecked.Value;

            MessagePopup popupObj = new MessagePopup();
            popupObj.None = chbPopUpSoundNone.IsChecked.Value;
            popupObj.On = chbPopUpSoundOn.IsChecked.Value;
            popupObj.Off = chbPopUpSoundOff.IsChecked.Value;

            ActiveStatus activeObj = new ActiveStatus();
            activeObj.None= chbActiveStatusNone.IsChecked.Value;
            activeObj.On = chbActiveStatusOn.IsChecked.Value;
            activeObj.Off = chbActiveStatusOff.IsChecked.Value;

            PostTimelineConfig timelineObj = new PostTimelineConfig();
            timelineObj.SourceFolder = txtSourceFolder.Text;
            timelineObj.Captions = txtCaption.Text;
            timelineObj.MinNumber = Int32.Parse(txtTimelineMinNumber.Value.ToString());
            timelineObj.MaxNumber = Int32.Parse(txtTimelineMaxNumber.Value.ToString());
            timelineObj.DeleteAfterPost = chbDeleteAfterPost.IsChecked.Value;

            PlayOnNewsFeedConfig playObj = new PlayOnNewsFeedConfig();
            playObj.React = reactObj;
            playObj.PlayTime = numberObj;
            playObj.Comments = txtComments.Text;

            Messenger messengerObj = new Messenger();
            messengerObj.MessageCallSound = callSoundObj;
            messengerObj.MessageSound = soundObj;
            messengerObj.MessagePopup = popupObj;
            messengerObj.ActiveStatus= activeObj;

            PagePostConfig pagePostObj = new PagePostConfig();
            pagePostObj.PageIds = txtPagePostIds.Text;
            pagePostObj.SourceFolder = txtPagePostSourceFolder.Text;
            pagePostObj.Captions = txtPagePostCaption.Text;
            pagePostObj.MinPosts = Int32.Parse(txtPagePostMinNumber.Value.ToString());
            pagePostObj.MaxPosts = Int32.Parse(txtPagePostMaxNumber.Value.ToString());

            NewsFeedConfig newsfeedObj = new NewsFeedConfig();
            newsfeedObj.NewsFeed = playObj;
            newsfeedObj.Timeline = timelineObj;
            newsfeedObj.Messenger = messengerObj;
            newsfeedObj.PagePost = pagePostObj;

            string output = JsonConvert.SerializeObject(newsfeedObj);

            cacheViewModel.GetCacheDao().Set("newsfeed:config", output);

            MessageBox.Show("Your config has been save successfully.");
        }
        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.Title = "Select Media File (Image/Video) to pick Source Folder";
                dialog.Filter = "Media Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.mp4;*.avi;*.mov;*.mkv;*.wmv|All Files|*.*";
                dialog.CheckFileExists = true;
                dialog.CheckPathExists = true;
                
                if (!string.IsNullOrEmpty(txtSourceFolder.Text) && System.IO.Directory.Exists(txtSourceFolder.Text))
                {
                    dialog.InitialDirectory = txtSourceFolder.Text;
                }

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txtSourceFolder.Text = dialog.FileName;
                }
            }
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void btnPagePostBrowse_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.Title = "Select Media File (Image/Video) to pick Source Folder";
                dialog.Filter = "Media Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.mp4;*.avi;*.mov;*.mkv;*.wmv|All Files|*.*";
                dialog.CheckFileExists = true;
                dialog.CheckPathExists = true;
                
                if (!string.IsNullOrEmpty(txtPagePostSourceFolder.Text) && System.IO.Directory.Exists(txtPagePostSourceFolder.Text))
                {
                    dialog.InitialDirectory = txtPagePostSourceFolder.Text;
                }

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txtPagePostSourceFolder.Text = dialog.FileName;
                }
            }
        }
    }
}
