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

namespace ToolKHBrowser.Views
{
    /// <summary>
    /// Interaction logic for pgPages.xaml
    /// </summary>
    public partial class pgPages : Page
    {
        ICacheViewModel cacheViewModel = DIConfig.Get<ICacheViewModel>();
        public pgPages()
        {
            InitializeComponent();

            LoadData();
        }
        public void LoadData()
        {
            var str = cacheViewModel.GetCacheDao().Get("page:config").Value.ToString();
            if (!string.IsNullOrEmpty(str))
            {
                PageConfig pageObj = JsonConvert.DeserializeObject<PageConfig>(str);

                try
                {
                    txtPageCreate.Value = Int32.Parse(pageObj.CreatePage.CreateNumber.ToString());
                }
                catch (Exception) { }
                try
                {
                    txtPageNames.Text = pageObj.CreatePage.Names;
                }
                catch (Exception) { }
                try
                {
                    txtPageCategories.Text = pageObj.CreatePage.Categies;
                }
                catch (Exception) { }
                try
                {
                    txtBio.Text = pageObj.CreatePage.Bio;
                }
                catch (Exception) { }
                try
                {
                    txtPageUrl.Text = pageObj.PageUrls;
                }
                catch (Exception) { }

                if (pageObj.CreateReel != null)
                {
                    try
                    {
                        txtPageCreateReel.Value = Int32.Parse(pageObj.CreateReel.CreateNumber.ToString());
                        txtCreateReelSourceFolder.Text = pageObj.CreateReel.SourceFolder;
                        txtReelCaptions.Text = pageObj.CreateReel.Captions;
                        txtReelHashtag.Text = pageObj.CreateReel.Hashtag;
                    }
                    catch (Exception) { }
                }
            }
        }

        private void btnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            CreatePageConfig createPageObj = new CreatePageConfig();
            createPageObj.Names = txtPageNames.Text;
            createPageObj.Categies = txtPageCategories.Text;
            createPageObj.Bio = txtBio.Text;
            createPageObj.CreateNumber = Int32.Parse(txtPageCreate.Value.ToString());

            CreateReelConfig reelObj = new CreateReelConfig();
            reelObj.SourceFolder = txtCreateReelSourceFolder.Text;
            reelObj.Hashtag = txtReelHashtag.Text;
            reelObj.Captions = txtReelCaptions.Text;
            reelObj.CreateNumber = Int32.Parse(txtPageCreateReel.Value.ToString());

            PageConfig pageObj = new PageConfig();
            pageObj.CreatePage = createPageObj;
            pageObj.CreateReel = reelObj;
            pageObj.PageUrls = txtPageUrl.Text;

            string output = JsonConvert.SerializeObject(pageObj);

            cacheViewModel.GetCacheDao().Set("page:config", output);

            MessageBox.Show("Your config has been save successfully.");
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
