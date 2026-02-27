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
            var cache = cacheViewModel.GetCacheDao().Get("page:config");
            if (cache != null && cache.Value != null)
            {
                var str = cache.Value.ToString();
                if (!string.IsNullOrEmpty(str))
                {
                    PageConfig pageObj = JsonConvert.DeserializeObject<PageConfig>(str);
                    if (pageObj == null) return;

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

                    if (pageObj.AutoScroll != null)
                    {
                        try
                        {
                            txtPageAutoScrollComments.Text = pageObj.AutoScroll.Comments ?? "";
                        }
                        catch (Exception) { }

                        try
                        {
                            var r = pageObj.AutoScroll.React;
                            bool isRandom = r?.Random == true;
                            bool isLike = r?.Like == true;
                            bool isComment = r?.Comment == true;

                            chbPageAutoScrollReactRandom.IsChecked = isRandom;
                            chbPageAutoScrollReactLikeComment.IsChecked = !isRandom && isLike && isComment;
                            chbPageAutoScrollReactLike.IsChecked = !isRandom && isLike && !isComment;
                            chbPageAutoScrollReactComment.IsChecked = !isRandom && !isLike && isComment;
                            chbPageAutoScrollReactNone.IsChecked = !isRandom && !isLike && !isComment;
                        }
                        catch (Exception) { }
                    }

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

            bool pageAutoRandom = chbPageAutoScrollReactRandom.IsChecked == true;
            bool pageAutoLikeComment = chbPageAutoScrollReactLikeComment.IsChecked == true;
            bool pageAutoLike = chbPageAutoScrollReactLike.IsChecked == true;
            bool pageAutoComment = chbPageAutoScrollReactComment.IsChecked == true;

            React pageAutoReactObj = new React();
            pageAutoReactObj.Random = pageAutoRandom;
            pageAutoReactObj.Like = pageAutoLikeComment || pageAutoLike;
            pageAutoReactObj.Comment = pageAutoLikeComment || pageAutoComment;
            pageAutoReactObj.None = !pageAutoReactObj.Random && !pageAutoReactObj.Like && !pageAutoReactObj.Comment;

            PageAutoScrollConfig pageAutoScrollObj = new PageAutoScrollConfig();
            pageAutoScrollObj.React = pageAutoReactObj;
            pageAutoScrollObj.Comments = txtPageAutoScrollComments.Text;

            PageConfig pageObj = new PageConfig();
            pageObj.CreatePage = createPageObj;
            pageObj.CreateReel = reelObj;
            pageObj.PageUrls = txtPageUrl.Text;
            pageObj.AutoScroll = pageAutoScrollObj;

            string output = JsonConvert.SerializeObject(pageObj);

            cacheViewModel.GetCacheDao().Set("page:config", output);

            MessageBox.Show("Your config has been save successfully.");
        }
        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
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
                var currentPath = (txtCreateReelSourceFolder.Text ?? "").Trim();
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
                txtCreateReelSourceFolder.Text = dialog.FileName;
            }
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
