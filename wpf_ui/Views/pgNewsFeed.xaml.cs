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
using ToolKHBrowser.ViewModels;
using ToolKHBrowser;
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
                chbReachRandom.IsChecked = nf.React.Random;
                chbReachLike.IsChecked = !nf.React.Random && nf.React.Like;
                chbReachComment.IsChecked = !nf.React.Random && !nf.React.Like && nf.React.Comment;
                chbReachNone.IsChecked = !nf.React.Random && !nf.React.Like && !nf.React.Comment;
            }
            txtComments.Text = nf?.Comments ?? "";
            txtMinComments.Value = ToInt(nf?.MinComments, 1);
            txtMaxComments.Value = ToInt(nf?.MaxComments, 1);

            if (nf?.CommentPost != null)
            {
                txtCommentPostVideoUrls.Text = nf.CommentPost.VideoUrls ?? "";
                txtCommentPostComments.Text = nf.CommentPost.Comments ?? "";

                var cpReact = nf.CommentPost.React;
                bool cpLike = cpReact?.Like == true;
                bool cpComment = cpReact?.Comment == true;
                chbCommentPostLikeComment.IsChecked = cpLike && cpComment;
                chbCommentPostLikeOnly.IsChecked = cpLike && !cpComment;
                chbCommentPostCommentOnly.IsChecked = !cpLike && cpComment;

                if (cpReact == null || (!cpLike && !cpComment))
                {
                    chbCommentPostLikeComment.IsChecked = true;
                }
            }

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

            bool isReactRandom = chbReachRandom.IsChecked == true;
            bool isReactLike = chbReachLike.IsChecked == true;
            bool isReactComment = chbReachComment.IsChecked == true;

            React reactObj = new React();
            reactObj.Random = isReactRandom;
            reactObj.Like = isReactLike;
            reactObj.Comment = isReactComment;
            reactObj.None = !reactObj.Random && !reactObj.Like && !reactObj.Comment;

            bool isCommentPostLikeComment = chbCommentPostLikeComment.IsChecked == true;
            bool isCommentPostLikeOnly = chbCommentPostLikeOnly.IsChecked == true;
            bool isCommentPostCommentOnly = chbCommentPostCommentOnly.IsChecked == true;

            React commentPostReactObj = new React();
            commentPostReactObj.Random = false;
            commentPostReactObj.Like = isCommentPostLikeComment || isCommentPostLikeOnly;
            commentPostReactObj.Comment = isCommentPostLikeComment || isCommentPostCommentOnly;
            commentPostReactObj.None = !commentPostReactObj.Like && !commentPostReactObj.Comment;

            CommentPostByUrlConfig commentPostObj = new CommentPostByUrlConfig();
            commentPostObj.VideoUrls = txtCommentPostVideoUrls.Text;
            commentPostObj.Comments = txtCommentPostComments.Text;
            commentPostObj.React = commentPostReactObj;

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
            playObj.MinComments = Int32.Parse(txtMinComments.Value.ToString());
            playObj.MaxComments = Int32.Parse(txtMaxComments.Value.ToString());
            playObj.CommentPost = commentPostObj;

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
