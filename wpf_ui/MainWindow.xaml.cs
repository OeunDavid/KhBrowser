using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToolLib;
using ToolLib.Data;
using ToolLib.Http;
using WpfUI.ViewModels;
using WpfUI.Views;

namespace WpfUI
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    /// 
  
    public partial class MainWindow : Window
    {
        private IDataDao dataDao = ToolDiConfig.Get<IDataDao>();
        private int licenseDay;

        public MainWindow()
        {
            InitializeComponent();
            licenseDay = 0;

            try
            {
                dataDao.ensureDatabaseReady();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing database: " + ex.Message);
            }

            //RunSQL();

            detectPermission();
        }

        private string appId = "193278124048833"; // Replace with your App ID
        private string profileId, fbCode, profileName, profileAccessToken, accessToken;
        private string currentUrl;
        private CookieContainer container = new CookieContainer();
        private string ConnectionStatusString = "https://mbasic.facebook.com/login/device-based/regular/login/?refsrc=deprecated&lwv=100&refid=8";

        public async Task loginAsync(string email, string password)
        {

            using (HttpClient client = new HttpClient())
            {
                var parameters = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("email", email),
                    new KeyValuePair<string, string>("pass", password)
                });

                HttpResponseMessage response = await client.PostAsync("https://www.facebook.com/login.php", parameters);

                if (response.IsSuccessStatusCode)
                {
                    // Check if login was successful by analyzing the  
                    string responseBody = await response.Content.ReadAsStringAsync();
                    if (responseBody.Contains("Log Out"))
                    {
                        Console.WriteLine("Login successful!");
                    }
                    else
                    {
                        Console.WriteLine("Login failed!");
                    }
                }
                else
                {
                    Console.WriteLine($"Failed to log in. Status Code: {response.StatusCode}");
                }
            }


            HttpRequest("get", "https://mbasic.facebook.com/login/device-based/regular/login/?refsrc=deprecated&lwv=100&refid=8", "");
            string getUrl = "https://mbasic.facebook.com/login/device-based/regular/login/?refsrc=deprecated&lwv=100&refid=8";
            string postData = String.Format("email={0}&pass={1}", email, password);
            HttpWebRequest getRequest = (HttpWebRequest)WebRequest.Create(getUrl);
            getRequest.CookieContainer = new CookieContainer();
            getRequest.CookieContainer = container; //recover cookies First request
            getRequest.Method = WebRequestMethods.Http.Post;
            getRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36";
            getRequest.AllowWriteStreamBuffering = true;
            getRequest.ProtocolVersion = HttpVersion.Version11;
            getRequest.AllowAutoRedirect = true;
            getRequest.ContentType = "application/x-www-form-urlencoded";

            byte[] byteArray = Encoding.ASCII.GetBytes(postData);
            getRequest.ContentLength = byteArray.Length;
            Stream newStream = getRequest.GetRequestStream(); //open connection
            newStream.Write(byteArray, 0, byteArray.Length); // Send the data.
            newStream.Close();

            HttpWebResponse getResponse = (HttpWebResponse)getRequest.GetResponse();
            using (StreamReader sr = new StreamReader(getResponse.GetResponseStream()))
            {
                container.Add(getResponse.Cookies);
                string sourceCode = sr.ReadToEnd();
            }

            string profileResponse = HttpRequest("get", "https://facebook.com/" + "me", "");
            profileId = GetStringBetween(profileResponse, "{\"USER_ID\":\"", "\",\"ACCOUNT_ID");
            fbCode = GetStringBetween(profileResponse, "fb_dtsg\" value=\"", "\" autocomplete");
            profileName = GetStringBetween(profileResponse, "id:\"" + profileId + "\",name:\"", "\",firstName:\"");
        }
        public bool LoginFb(string email, string password)
        {
            //string loginResponse1 = HttpRequest("POST", ConnectionStatusString + "login.php", "lsd=AVphHiRP&email=" + email + "&pass=" + password + "&lgnrnd=093439_-52j&lgnjs=1380213284&locale=en_GB");
            //string loginResponse2 = HttpRequest("POST", ConnectionStatusString + "login.php", "lsd=AVphHiRP&email=" + email + "&pass=" + password + "&lgnrnd=093439_-52j&lgnjs=1380213284&locale=en_GB"); string loginResponse1 = HttpRequest("POST", ConnectionStatusString + "login.php", "lsd=AVphHiRP&email=" + email + "&pass=" + password + "&lgnrnd=093439_-52j&lgnjs=1380213284&locale=en_GB");
            string loginResponse1 = HttpRequest("post", ConnectionStatusString, "lsd=AVr2R6Ru8oU&jazoest=2902&m_ts=1710401730&li=wqjyZRmKBaP88BT5TKkZdl1b&try_number=0&unrecognized_tries=0&email="+ email +"&pass="+  password + "&login=Log+in&bi_xrwh=0");
            string loginResponse2 = HttpRequest("post", ConnectionStatusString, "lsd=AVr2R6Ru8oU&jazoest=2902&m_ts=1710401730&li=wqjyZRmKBaP88BT5TKkZdl1b&try_number=0&unrecognized_tries=0&email="+ email +"&pass="+  password + "&login=Log+in&bi_xrwh=0");
            string profileResponse = HttpRequest("get", "https://facebook.com/" + "me", "");
            profileId = GetStringBetween(profileResponse, "{\"USER_ID\":\"", "\",\"ACCOUNT_ID");
            fbCode = GetStringBetween(profileResponse, "fb_dtsg\" value=\"", "\" autocomplete");
            profileName = GetStringBetween(profileResponse, "id:\"" + profileId + "\",name:\"", "\",firstName:\"");
            profileAccessToken = GetStringBetween(profileResponse, "access_token:", ",").Replace("\"", "");
            if (!string.IsNullOrEmpty(profileId) && profileId != "ERROR" && profileId != "0")
            {
                string accessTokenResponse = HttpRequest("POST", ConnectionStatusString + "v1.0/dialog/oauth/confirm", $"fb_dtsg={fbCode}&app_id={appId}&redirect_uri=fbconnect%3A%2F%2Fsuccess&display=page&access_token=&from_post=1&return_format=access_token&domain=&sso_device=ios&__CONFIRM__=1&__user={profileId}&scope=user_birthday,user_religion_politics,user_relationships,user_relationship_details,user_hometown,user_location,user_likes,user_education_history,user_work_history,user_website,user_events,user_photos,user_videos,user_friends,user_about_me,user_status,user_games_activity,user_tagged_places,user_posts,rsvp_event,email,read_insights,publish_actions,read_audience_network_insights,read_custom_friendlists,user_actions.books,user_actions.music,user_actions.video,user_actions.news,user_actions.fitness,user_managed_groups,manage_pages,pages_manage_cta,pages_manage_instant_articles,pages_show_list,publish_pages,read_page_mailboxes,ads_management,ads_read,business_management,pages_messaging,pages_messaging_phone_number,pages_messaging_subscriptions,pages_messaging_payments,instagram_basic,instagram_manage_comments,instagram_manage_insights,public_profile");
                accessToken = GetStringBetween(accessTokenResponse, "access_token=", "&expires_in");
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool LogoutFb()
        {
            container = new CookieContainer();
            profileId = "";
            profileName = "";
            fbCode = "";
            return true;
        }

        private string HttpRequest(string requestType, string url, string requestData)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.KeepAlive = true;
                request.CookieContainer = container;
                request.ContentType = "application/x-www-form-urlencoded";
                request.Timeout = 100000;
                request.AllowAutoRedirect = true;
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36";

                if (requestType.ToLower() == "post")
                {
                    request.Method = "POST";
                    byte[] bytes = Encoding.UTF8.GetBytes(requestData);
                    request.ContentLength = bytes.Length;
                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }
                }
                else
                {
                    request.Method = "GET";
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    container.Add(response.Cookies);
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        currentUrl = response.ResponseUri.ToString();

                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle or log the error
                return null;
            }
        }

        private string GetStringBetween(string source, string start, string end)
        {
            int startIndex = source.IndexOf(start) + start.Length;
            int endIndex = source.IndexOf(end, startIndex);
            if (startIndex < start.Length || endIndex == -1)
            {
                return "";
            }
            return source.Substring(startIndex, endIndex - startIndex);
        }

        public bool isAllow()
        {
            return true;
        }
        public void detectPermission()
        {
            string version = "8.4";

            if (true)
            {
                //lblDisplayLicense.Text = "PC name: " + user.PCName + ", Your license: " + user.LicenseDay + " Days";
                this.Title = "Tool FB - Version: " + version;

                copyFile();

                GridPrincipal.Children.Clear();
                GridPrincipal.Children.Add(new frmMain());
            }
        }
        public void copyFile()
        {
            // This was for SQLite file backup. SQL Server backups should be handled via SQL Server management or scripts.
            /*
            string prefix = DateTime.Now.ToFileTime().ToString();
            string path = System.Environment.CurrentDirectory;
            string sou = path + "\\core_db.db";
            string des = path + "\\backup\\" + prefix + "_core_db.db";
            string[] files = Directory.GetFiles(path + "\\backup");

            //for (int i = 0; i < files.Length; i++)
            //{
            //    try
            //    {
            //        File.Delete(files[i]);
            //    }
            //    catch (Exception) { }
            //}
            try
            {
                File.Copy(sou, des, true);
            }
            catch (Exception) { }
            */
        }
        private void ButtonFechar_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
