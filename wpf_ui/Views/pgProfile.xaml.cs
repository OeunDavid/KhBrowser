using CsQuery.ExtensionMethods.Internal;
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
    /// Interaction logic for pgProfile.xaml
    /// </summary>
    public partial class pgProfile : Page
    {
        ICacheViewModel cacheViewModel = DIConfig.Get<ICacheViewModel>();
        public pgProfile()
        {
            InitializeComponent();

            LoadData();
        }
        public void LoadData()
        {
            var cache = cacheViewModel.GetCacheDao().Get("profile:config");
            if (cache != null && cache.Value != null)
            {
                var str = cache.Value.ToString();
                if (!string.IsNullOrEmpty(str))
                {
                    ProfileConfig profileObj = JsonConvert.DeserializeObject<ProfileConfig>(str);
                if (profileObj == null) return;

                if (profileObj.DeleteData != null)
                {
                    txtUnfriends.Value = profileObj.DeleteData.Unfriend;
                    txtSuggest.Value = profileObj.DeleteData.Suggest;
                    txtRequest.Value = profileObj.DeleteData.Request;

                    chbProfilePicture.IsChecked = profileObj.DeleteData.Profile;
                    chbCoverPicture.IsChecked = profileObj.DeleteData.Cover;
                    chbAllPicture.IsChecked = profileObj.DeleteData.Picture;
                    chbDeletePostTag.IsChecked = profileObj.DeleteData.Tag;
                    chbRemoveMobilePhone.IsChecked = profileObj.DeleteData.Phone;
                }

                if (profileObj.NewInfo != null)
                {
                    txtProfileSource.Text = profileObj.NewInfo.SourceProfile;
                    txtCoverSource.Text = profileObj.NewInfo.SourceCover;
                    txtBio.Text = profileObj.NewInfo.Bio;
                    txtSchool.Text = profileObj.NewInfo.School;
                    txtCollege.Text = profileObj.NewInfo.College;
                    txtCity.Text = profileObj.NewInfo.City;
                    txtHometown.Text = profileObj.NewInfo.Hometown;
                }

                chbTurnOnTwofaByWeb.IsChecked = profileObj.TwoFA.Web;
                chbTurnOnTwofaByWebII.IsChecked = profileObj.TwoFA.WebII;
                chbTurnOnTwofaByMbasic.IsChecked = profileObj.TwoFA.Mbasic;
                chbEnterPassword.IsChecked = profileObj.TwoFA.EnterPassword;

                txtNewPassword.Text = profileObj.Password.Value;
                chbPasswordWeb.IsChecked = profileObj.Password.RunOnWeb;
                chbPasswordMbasic.IsChecked = profileObj.Password.RunOnMbasic;
                try
                {
                    chbPasswordNewLayout.IsChecked = profileObj.Password.RunOnNewLayOut;
                }
                catch (Exception) { }
                try
                {
                    chbPasswordWeb2.IsChecked = profileObj.Password.RunOnWeb2;
                }
                catch (Exception) { }

                if (profileObj.ActivityLog != null)
                {
                    try
                    {
                        txtDeleteActivityLogGroups.Value =  profileObj.ActivityLog.GroupPostsAndComments;
                    }
                    catch (InvalidCastException) { }
                    catch (Exception) { }
                }
                try
                {
                    txtCheckIn.Text = profileObj.NewInfo.CheckIn;
                } catch(Exception) { }
                try
                {
                    txtMarketplace.Text = profileObj.NewInfo.Marketplace;
                }
                catch (Exception) { }
                }
            }
        }

        private void btnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            ProfileConfig profileObj = new ProfileConfig();

            DeleteProfileData deleteObj = new DeleteProfileData();
            deleteObj.Unfriend = Int32.Parse(txtUnfriends.Value.ToString());
            deleteObj.Suggest = Int32.Parse(txtSuggest.Value.ToString());
            deleteObj.Request = Int32.Parse(txtRequest.Value.ToString());
            deleteObj.Profile = chbProfilePicture.IsChecked.Value;
            deleteObj.Cover = chbCoverPicture.IsChecked.Value;
            deleteObj.Picture = chbAllPicture.IsChecked.Value;
            deleteObj.Tag = chbDeletePostTag.IsChecked.Value;
            deleteObj.Phone = chbRemoveMobilePhone.IsChecked.Value;

            NewInfo infoObj = new NewInfo();
            infoObj.SourceProfile = txtProfileSource.Text;
            infoObj.SourceCover = txtCoverSource.Text;
            infoObj.Bio = txtBio.Text;
            infoObj.School = txtSchool.Text;
            infoObj.College = txtCollege.Text;
            infoObj.City= txtCity.Text;
            infoObj.Hometown = txtHometown.Text;

            infoObj.CheckIn = txtCheckIn.Text;
            infoObj.Marketplace = txtMarketplace.Text;

            ActivityLog activityLogObj = new ActivityLog();
            activityLogObj.GroupPostsAndComments = Int32.Parse(txtDeleteActivityLogGroups.Value.ToString());

            TwoFA twofaObj = new TwoFA();
            twofaObj.Web = chbTurnOnTwofaByWeb.IsChecked.Value;
            twofaObj.WebII = chbTurnOnTwofaByWebII.IsChecked.Value;
            twofaObj.Mbasic = chbTurnOnTwofaByMbasic.IsChecked.Value;
            twofaObj.EnterPassword = chbEnterPassword.IsChecked.Value;

            Password passObj = new Password();
            passObj.Value = txtNewPassword.Text;
            passObj.RunOnWeb = chbPasswordWeb.IsChecked.Value;
            passObj.RunOnWeb2 = chbPasswordWeb2.IsChecked.Value;
            passObj.RunOnMbasic = chbPasswordMbasic.IsChecked.Value;
            passObj.RunOnNewLayOut = chbPasswordNewLayout.IsChecked.Value;

            profileObj.DeleteData = deleteObj;
            profileObj.ActivityLog = activityLogObj;
            profileObj.NewInfo = infoObj;
            profileObj.TwoFA = twofaObj;
            profileObj.Password = passObj;


            string output = JsonConvert.SerializeObject(profileObj);

            cacheViewModel.GetCacheDao().Set("profile:config", output);

            MessageBox.Show("Your config has been save successfully.");
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
