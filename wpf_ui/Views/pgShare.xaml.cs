using CsQuery.EquationParser.Implementation.Functions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace ToolKHBrowser.Views
{
    /// <summary>
    /// Interaction logic for pgShare.xaml
    /// </summary>
    public partial class pgShare : Page
    {
        ICacheViewModel cacheViewModel = DIConfig.Get<ICacheViewModel>();
        public pgShare()
        {
            InitializeComponent();

            LoadData();
        }
        public void LoadData()
        {
            var str= cacheViewModel.GetCacheDao().Get("share:config").Value.ToString();
            if(!string.IsNullOrEmpty(str))
            {
                Sharer shareObj = JsonConvert.DeserializeObject<Sharer>(str);

                txtLinkURL.Text = shareObj.Urls;
                txtComments.Text = shareObj.Comments;
                txtCaptions.Text = shareObj.Captions;
                try
                {
                    txtHashtag.Text = shareObj.Hashtag;
                }
                catch (Exception) { }

                try
                {
                    txtDelayAfterEachShareStart.Value = Int32.Parse(shareObj.DelayEachShare.DelayStart.ToString());
                    txtDelayAfterEachShareEnd.Value = Int32.Parse(shareObj.DelayEachShare.DelayEnd.ToString());
                }
                catch (Exception) { }
                try
                {
                    chbShareToGroupNoPending.IsChecked = shareObj.CheckPending;
                }
                catch (Exception) { }
                try
                {
                    chbShareMixContent.IsChecked = shareObj.MixContent;
                }
                catch (Exception) { }
                try
                {
                    chbShareSingleContent.IsChecked = shareObj.OneContent;
                }
                catch (Exception) { }
                try
                {
                    txtViewBeforeShare.Value = Int32.Parse(shareObj.Groups.WatchBeforeShare.ToString());
                    txtWatchAfterShare.Value = Int32.Parse(shareObj.Groups.WatchAfterShare.ToString());
                }
                catch (Exception) { }
                try
                {
                    txtNumberOfShareStart.Value = Int32.Parse(shareObj.GroupNumber.NumberStart.ToString());
                    txtNumberOfShareEnd.Value = Int32.Parse(shareObj.GroupNumber.NumberEnd.ToString());
                }
                catch (Exception) { }
                try
                {
                    chbReachNone.IsChecked = shareObj.Groups.React.None;
                    chbReachLike.IsChecked = shareObj.Groups.React.Like;
                    chbReachRandom.IsChecked = shareObj.Groups.React.Random;
                }
                catch (Exception) { }
                try
                {
                    chbShareGroupRandom.IsChecked = shareObj.Groups.GroupFilter.Random;
                    chbShareGroupByName.IsChecked = shareObj.Groups.GroupFilter.GroupName;
                }
                catch (Exception) { }

                try
                {
                    chbShareProfilePageNewToken.IsChecked = shareObj.ProfilePage.IsToken;
                    txtProfilePageCommentObjectId.Text = shareObj.ProfilePage.CommentObject;
                    txtGroupID.Text = shareObj.ProfilePage.GroupIDs;
                }
                catch (Exception) { }

                if (shareObj.Website != null)
                {
                    try
                    {
                        chbShareWebsiteToPage.IsChecked = shareObj.Website.Page;
                        chbShareWebsiteToGroup.IsChecked = shareObj.Website.Group;
                        chbShareWebsiteCommentLink.IsChecked = shareObj.Website.CommentLink;
                        chbShareWebsitePastLink.IsChecked = shareObj.Website.PastLink;
                        chbShareWebsiteRandomImage.IsChecked = shareObj.Website.RandomPicture;
                        txtShareWebsiteFolderImage.Text = shareObj.Website.Folder;
                    }
                    catch (Exception) { }
                    try
                    {
                        chbShareWebsiteRandomContent.IsChecked = shareObj.Website.RandomContent;
                    }
                    catch (Exception) { }
                    try
                    {
                        chbShareWebsiteWithoutJoinGroups.IsChecked = shareObj.Website.GroupWithoutJoin;
                    }
                    catch (Exception) { }
                }
            }
        }

        private void btnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            var url = txtLinkURL.Text.Trim();
            var comment = txtComments.Text;
            var caption = txtCaptions.Text;
            var hashtag = txtHashtag.Text;

            var eachShareStart = Int32.Parse(txtDelayAfterEachShareStart.Value.ToString());
            var eachShareEnd = Int32.Parse(txtDelayAfterEachShareEnd.Value.ToString());

            var groupNumberStart = Int32.Parse(txtNumberOfShareStart.Value.ToString());
            var groupNumberEnd = Int32.Parse(txtNumberOfShareEnd.Value.ToString());

            var checkPending = chbShareToGroupNoPending.IsChecked.Value;
            var mixContent = chbShareMixContent.IsChecked.Value;
            var oneContent = chbShareSingleContent.IsChecked.Value;

            var viewBeforeShare = Int32.Parse(txtViewBeforeShare.Value.ToString());
            var viewAfterShere = Int32.Parse(txtWatchAfterShare.Value.ToString());

            var reactShareNone = chbReachNone.IsChecked.Value;
            var reactShareLike = chbReachLike.IsChecked.Value;
            var reactShareRandom = chbReachRandom.IsChecked.Value;

            var groupShareRandom = chbShareGroupRandom.IsChecked.Value;
            var groupShareName = chbShareGroupByName.IsChecked.Value;

            var object_id = txtProfilePageCommentObjectId.Text.Trim();
            var groupIds = txtGroupID.Text.Trim();
            var pageNewToken = chbShareProfilePageNewToken.IsChecked.Value;

            var page = chbShareWebsiteToPage.IsChecked.Value;
            var group = chbShareWebsiteToGroup.IsChecked.Value;
            var commentLink = chbShareWebsiteCommentLink.IsChecked.Value;
            var PastLink = chbShareWebsitePastLink.IsChecked.Value;
            var randomPicture = chbShareWebsiteRandomImage.IsChecked.Value;
            var randomContent = chbShareWebsiteRandomContent.IsChecked.Value;
            var groupWithoutJoin = chbShareWebsiteWithoutJoinGroups.IsChecked.Value;
            var folder = txtShareWebsiteFolderImage.Text.Trim();

            Sharer shareObj = new Sharer();
            ShareToGroups toGroupObj = new ShareToGroups();
            ShareProfilePage profilePageObj = new ShareProfilePage();
            ShareGroupNumber groupNumberObj = new ShareGroupNumber();
            ShareDelayEachShare delayEachShareObj = new ShareDelayEachShare();
            ShareGroupFilter groupFilter = new ShareGroupFilter();
            ShareReact reactObj = new ShareReact();

            ShareWebsite websiteObj = new ShareWebsite();

            websiteObj.Page = page;
            websiteObj.Group = group;
            websiteObj.CommentLink= commentLink;
            websiteObj.PastLink = PastLink;
            websiteObj.RandomPicture = randomPicture;
            websiteObj.Folder = folder;
            websiteObj.RandomContent = randomContent;
            websiteObj.GroupWithoutJoin = groupWithoutJoin;

            delayEachShareObj.DelayStart = eachShareStart;
            delayEachShareObj.DelayEnd = eachShareEnd;

            groupFilter.Random = groupShareRandom;
            groupFilter.GroupName = groupShareName;

            reactObj.Random = reactShareRandom;
            reactObj.Like = reactShareLike;
            reactObj.None = reactShareNone;

            toGroupObj.WatchBeforeShare = viewBeforeShare;
            toGroupObj.WatchAfterShare = viewAfterShere;
            toGroupObj.GroupFilter = groupFilter;
            toGroupObj.React = reactObj;

            groupNumberObj.NumberStart = groupNumberStart;
            groupNumberObj.NumberEnd = groupNumberEnd;

            profilePageObj.CommentObject = object_id;
            profilePageObj.GroupIDs= groupIds;
            profilePageObj.IsToken = pageNewToken;

            shareObj.Urls = url;
            shareObj.Comments = comment;
            shareObj.Captions = caption;
            shareObj.Hashtag = hashtag;
            shareObj.CheckPending = checkPending;
            shareObj.MixContent = mixContent;
            shareObj.OneContent = oneContent;

            shareObj.Groups = toGroupObj;
            shareObj.ProfilePage = profilePageObj;
            shareObj.GroupNumber = groupNumberObj;
            shareObj.DelayEachShare = delayEachShareObj;

            shareObj.Website = websiteObj;

            string output = JsonConvert.SerializeObject(shareObj);

            cacheViewModel.GetCacheDao().Set("share:config", output);

            MessageBox.Show("Your config has been save successfully.");
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
