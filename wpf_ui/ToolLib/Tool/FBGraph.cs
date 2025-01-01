using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using ToolKHBrowser.ViewModels;
using Cookie = System.Net.Cookie;

namespace ToolKHBrowser.ToolLib.Tool
{
    public class FBGraph
    {
        public string fb_dtsg = "";
        public string uid = "";
        public string referer = "https://www.facebook.com";
        public string contentType = "application/x-www-form-urlencoded";
        public string accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
        public string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.60 Safari/537.36";
        public CookieContainer container = null;
        public CookieContainer container2 = new CookieContainer();

        public void SetUID(string uid)
        {
            this.uid = uid;
        }
        public void SetContainer(CookieContainer c)
        {
            container = c;
            container2 = c;
        }
        public int IsGroupPending(string groupId)
        {
            var res = HTTPWebRequest("GET", "https://m.facebook.com/groups/" + groupId + "/madminpanel/pending");
            if (res.Contains("Pending Posts (0)"))
            {
                return 1;
            }
            if (res.Contains("No pending posts"))
            {
                return 0;
            }
            if (res.Contains("temporarily blocked"))
            {
                return -1;
            }

            return 1;
        }
        public string GetBackupPage(string accessToken)
        {
            string pageIds = "";

            string url = "https://graph.facebook.com/v14.0/me?fields=accounts.limit(1000){id,name,access_token}&access_token=" + accessToken;

            string a = HTTPWebRequest("GET", url);

            var b = JsonConvert.DeserializeObject<FacebookAccounts>(a.ToString());
            if (b.accounts == null)
            {
                return "";
            }
            foreach (var d in b.accounts.data)
            {
                var p = JsonConvert.DeserializeObject<Pages>(d.ToString());
                if (p.id.Trim().Contains(uid.Trim()))
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(pageIds))
                {
                    pageIds += ',';
                }
                pageIds += p.id + "|" + p.access_token;
            }

            return pageIds;
        }
        public FBProfile GetInfo(string accessToken)
        {
            string url = "https://graph.facebook.com/v16.0/me?fields=id,name,gender,birthday,email&access_token=" + accessToken;

            string res = HTTPWebRequest("GET", url);
            var fba = new FBProfile();

            if (!string.IsNullOrEmpty(res))
            {
                try
                {
                    var jobj = JObject.Parse(res.ToString());

                    fba = JsonConvert.DeserializeObject<FBProfile>(jobj.ToString());
                }
                catch (Exception) { }
            }

            return fba;
        }
        public ObservableCollection<FBGroupNodes> GetGroupByUID(string uid, string accessToken)
        {
            string url = "https://graph.facebook.com/graphql?q=nodes(" + uid + "){groups{nodes{id,name,viewer_post_status,visibility,group_member_profiles{count}}}}&format=json&access_token=" + accessToken;

            string res = HTTPWebRequest("GET", url);
            var fbNodes = new ObservableCollection<FBGroupNodes>();

            if (!string.IsNullOrEmpty(res))
            {
                try
                {
                    var jobj = JObject.Parse(res.ToString());

                    var g = JsonConvert.DeserializeObject<FBProfileGroup>(jobj.First.First.ToString());
                    fbNodes = g.groups.nodes;
                }
                catch (Exception) { }
            }

            return fbNodes;
        }
        public bool CheckLiveUID(string uid)
        {
            try
            {
                string text = new WebClient().DownloadString("https://graph.facebook.com/" + uid + "/picture?redirect=false");
                if (!string.IsNullOrEmpty(text))
                {
                    if (text.Contains("height") && text.Contains("width"))
                    {
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }
        public string CommentOnPost(string obj_id, string accessToken, string message)
        {
            return HTTPWebRequest("POST", "https://graph.facebook.com/v15.0/" + obj_id + "/comments", "message=" + message + "&access_token=" + accessToken);
        }
        public void GetFB_DTSG()
        {
            var aa = HTTPWebRequest("GET", "https://www.facebook.com");
            aa = HTTPWebRequest("GET", "https://www.facebook.com/me");
            var str_fb_dtsg1 = "|DTSGInitialData|,[],{|token|:|";
            var str_fb_dtsg2 = "|},";
            str_fb_dtsg1 = str_fb_dtsg1.Replace('|', '"');
            str_fb_dtsg2 = str_fb_dtsg2.Replace('|', '"');

            fb_dtsg = GetStringBetween(aa, str_fb_dtsg1, str_fb_dtsg2);

            Console.WriteLine("FB_DTSG: " + fb_dtsg);
        }
        public bool LoginByCookie(string cookie)
        {
            SetHttpWebRequestCookies(cookie);

            var aa = HTTPWebRequest("GET", "https://www.facebook.com");
            aa = HTTPWebRequest("GET", "https://www.facebook.com/me");
            var str_fb_dtsg1 = "|DTSGInitialData|,[],{|token|:|";
            var str_fb_dtsg2 = "|},";
            str_fb_dtsg1 = str_fb_dtsg1.Replace('|', '"');
            str_fb_dtsg2 = str_fb_dtsg2.Replace('|', '"');

            fb_dtsg = GetStringBetween(aa, str_fb_dtsg1, str_fb_dtsg2);

            return true;
        }
        public void RequestNW()
        {
            HTTPWebRequest("GET", "https://www.facebook.com/nw/", "");
        }
        public void CreateProfilePage()
        {

        }
        public void ProfilePageShareToGroup(string pageId, string groupId, string videoId)
        {
            string composerSessionId = "6307147e-759d-4167-a2d8-9784d39201c8";// GetWebSession();
            string variables = "{|input|:{" +
                "|composer_entry_point|:|inline_composer|," +
                "|composer_source_surface|:|group|," +
                "|composer_type|:|group|," +
                "|logging|:{|composer_session_id|:|" + composerSessionId + "|}," +
                "|source|:|www|," +
                "|attachments|:[{|link|:{|share_scrape_data|:|{|share_type|:37,|share_params|:[" + videoId + "]}|}}]," +
                "|message|:{|ranges|:[],|text|:||}," +
                "|with_tags_ids|:[]," +
                "|inline_activities|:[]," +
                "|explicit_place_id|:|0|," +
                "|text_format_preset_id|:|0|," +
                "|navigation_data|:{|attribution_id_v2|:|CometGroupDiscussionRoot.react,comet.group,unexpected,1673197154585,882471,2361831622,;GroupsCometCrossGroupFeedRoot.react,comet.groups.feed,tap_tabbar,1673197152641,830641,2361831622,|}," +
                "|tracking|:[null]," +
                "|audience|:{|to_id|:|" + groupId + "|}," +
                "|actor_id|:|" + pageId + "|," +
                "|client_mutation_id|:|3|}," +
                "|displayCommentsFeedbackContext|:null," +
                "|displayCommentsContextEnableComment|:null," +
                "|displayCommentsContextIsAdPreview|:null," +
                "|displayCommentsContextIsAggregatedShare|:null," +
                "|displayCommentsContextIsStorySet|:null," +
                "|feedLocation|:|GROUP|," +
                "|feedbackSource|:0," +
                "|focusCommentID|:null," +
                "|gridMediaWidth|:null," +
                "|groupID|:null," +
                "|scale|:1," +
                "|privacySelectorRenderLocation|:|COMET_STREAM|," +
                "|renderLocation|:|group|," +
                "|useDefaultActor|:false," +
                "|inviteShortLinkKey|:null," +
                "|isFeed|:false," +
                "|isFundraiser|:false," +
                "|isFunFactPost|:false," +
                "|isGroup|:true," +
                "|isEvent|:false," +
                "|isTimeline|:false," +
                "|isSocialLearning|:false," +
                "|isPageNewsFeed|:false," +
                "|isProfileReviews|:false," +
                "|isWorkSharedDraft|:false," +
                "|UFI2CommentsProvider_commentsKey|:|CometGroupDiscussionRootSuccessQuery|," +
                "|hashtag|:null," +
                "|canUserManageOffers|:false," +
                "}";


            // code below now clean
            string param = "av=" + uid;
            param += "&__usid=" + uid;
            param += "&__a=1";
            param += "&dpr=1";
            param += "&fb_dtsg=" + fb_dtsg;

            param += "&fb_api_caller_class=RelayModern";
            param += "&fb_api_req_friendly_name=ComposerStoryCreateMutation";

            param += "&server_timestamps=true";
            param += "&doc_id=7170838572990679";
            param += "&fb_api_analytics_tags=[|qpl_active_flow_ids=431626709|]";
            param += "&variables=" + variables;

            param += "&__req=2m";
            param += "&__hs=19365.HYP:comet_pkg.2.1.0.2.1";
            param += "&__ccg=GOOD";
            param += "&dpr=1";

            param += "&__s=dyyf75:825dhk:ix8jm0";
            param += "&__hsi=7186326980095757894";
            param += "&__comet_req=15";
            param += "&jazoest=25447";
            param += "&lsd=s4sYCISD9Lyi6lg70nmEue";
            param += "&__spin_r=1005611264";
            param += "&__spin_b=trunk";
            param += "&__spin_t=1654085852";

            param += "&__dyn=7AzHxqUW13xt0mUyEqxenFwLBwopU98nwgU765QdwSxucyU8EW1twYwJyEiwsobo6u3y4o11U2nwb-q7oc81xoswIK1Rwwwg8a8465o-cwfG12wOKdwGxu782lwv89kbxS2218wc61axe3S1lwlE-U2exi4UaEW2au1jxS6FobrwKxm5oe8cE8K5pUfEdE7am7-8wywdqUuBwJwSyES0Io88cA0z8c84qifxe3u364UrwFg662S26";
            param += "&__csr=gaQ4Qah4n25H7iNYBk-D48jTFcj4bOjilsDEyliHHOeGnfJvFbqHtnQVHiVILhf9jaFVFuy5CFkhbmQmZ4jgnGtbmhQAV5AKGgB924-QuiEZeWGWpH_yoyiAd-FrzESGUHiDLzqgyazTgJ2EOejK2WUiK7U8EoV4ui4RnJ1G6t6CUGqivUWiinWGh6zFXyECUgzUGAazUgBhpA2q6FUycxm68G8x-6ofWUO8xe78Si1cx62Gi9Awxxaiq2i9UnxJ2EOq6F8oGi5Amt2o-1exS2C48kxSdK9x2qi9wBx21wxe3ydAxm2e1ywOUrxifAAUuwi8c89o19E0bYU1LFK0vu04d802e_w0Jzw2tzw4pw2281V80Bm0OUhw2cA0bJw0yzw6aw16C0ava0edo72E0h08hz8V16A3S03mzBkgCW80F81gyBAzEowooW0AU0z506NiJzoOE1tE2r80qW440TU6q7UW1tw";

            HTTPWebRequest("POST", "https://www.facebook.com/api/graphql/", param);
        }

        public void RequestBNZAI()
        {
            string webSession = GetWebSession();
            string appID = GetAppID();
            string variables = "[{|app_id|:|" + appID + "|,|posts|:|0|,|user|:|" + uid + "|,|webSessionId|:|" + webSession + "|,|trigger|:|falco:video_player_www|,|send_method|:|ajax|,|compression|:|deflate|,|snappy_ms|:1}]";


            // code below now clean
            string param = "av=" + uid;
            param += "&__usid=6-Trcstro1ktf2k4:Prcsst88ns09m:0-Arcstro1fb1fv4-RV=6:F=";
            param += "&__a=1";
            param += "&dpr=1";
            param += "&fb_dtsg=" + fb_dtsg;
            param += "&fb_api_caller_class=RelayModern";
            param += "&fb_api_req_friendly_name=CometPageLikePageAsPageMutation";

            param += "&server_timestamps=true";
            param += "&doc_id=7170838572990679";
            param += "&fb_api_analytics_tags=[|qpl_active_flow_ids=431626192|]";
            param += "&variables=" + variables;

            param += "&__req=53";
            param += "&__ccg=GOOD";
            param += "&__rev=1005611264";
            param += "&__s=k9j3u7:ivnpu3:5x266r";
            param += "&__hsi=7104244640658143481-0";
            param += "&__comet_req=1";
            param += "&jazoest=22097";
            param += "&lsd=s4sYCISD9Lyi6lg70nmEue";
            param += "&__spin_r=1005611264";
            param += "&__spin_b=trunk";
            param += "&__spin_t=1654085852";
            param += "&__dyn=7AzHK4HwBwIxt0mUyEqxenFw9uu2i5U4e1Nxt3odE98K2aew9G2Saxa1NwJwpUe8hwaG1sw9u0LVEtwMw65xO321Rwwwg8a8465o-cwfG12wOKdwGxu782ly87e2l2Utwwwi831wiEjwZxy3O1mzXxG1Pxi4UaEW2G1jxS6FobrwKxm5oe8cEW4-5pUfEe88o4Wm7-8wywfCm2Sq2-azo2NwwwOg2cwMwhF8-4UdU";

            param += "&__csr=g9cbSyNssB7P8lfkj4EHN4IBssDFijGXJJlLnMAAzGKy8yGAADnFGRF9lV4a-RjAFR9qm4-jSLiARHmi8HiBKrJ5h9k-iHjjGV4jhfiyuhyHF7t24aBx7Cnr_jxW9yqG8CyXl2HGENAmXnyXCDyFqyA-4EWFGjJ2poKnGiWwYyoKUaA8Kt91Gcx2UqVVojzpu32XzFoK44awLxibwGwwwOz8W7Kq2Sm3um22dyEaaxC2Oq2Ccy8hwkbDwh8fawsEows8bocoW4awNwrVoco4q7EG0gi2i2m1Vg0gqw62w9y1Bw5Sw4Ww9y0VopwKwto1h8W0FE7W09Tw3_E3Uwee054Q0mucx20iuu2WE5C0eAw0xGo8EdE1sU0WOeBwHw21E0zO0pm0bGw62wpES641rwvpEmGujnwa-i48Sew9y19wh9U1-EbUVBy8loK0uaubwl8pxe1FU5S1yyo1CH-UeAfhofE4i9gqCUgyWK5bzu8yA4GxW0tPwcO0d4zo1vU09goow1QW0jC78b811oyU4i2i6Elx25Umw29E3qwvm2O0lS4EO0axg4u0Ubxe7U5aUa8ao2Qxi08Iw11G08DwhE";

            HTTPWebRequest("POST", "https://www.facebook.com/api/graphql/", param);
        }
        public string GetWebSession()
        {
            return "32sriw:2e7e0n:7v1aau";
        }
        public string GetAppID()
        {
            return "";
        }
        public string GetVideoURL(string videURL)
        {
            var aa = HTTPWebRequest("GET", videURL);
            var str_fb_dtsg1 = "|base_url|:|";
            var str_fb_dtsg2 = "|,|bandwidth|:";
            str_fb_dtsg1 = str_fb_dtsg1.Replace('|', '"');
            str_fb_dtsg2 = str_fb_dtsg2.Replace('|', '"');

            string url = GetStringBetween(aa, str_fb_dtsg1, str_fb_dtsg2);

            return url;
        }
        public void LikePage(string pageId, string actor_id = "")
        {
            if (string.IsNullOrEmpty(actor_id))
            {
                actor_id = uid;
            }
            //uid = actor_id;
            HTTPWebRequest("POST", "https://www.facebook.com/api/graphql/", "av=" + uid + "&__user=" + uid + "&__a=1&dpr=1&fb_dtsg=" + fb_dtsg + "&fb_api_caller_class=RelayModern&fb_api_req_friendly_name=CometPageLikeCommitMutation&server_timestamps=true&doc_id=4895534987233816&fb_api_analytics_tags=[|qpl_active_flow_ids=30605361|]&variables=%7B%22input%22%3A%7B%22attribution_id_v2%22%3A%22CometSinglePageHomeRoot.react%2Ccomet.page%2Cunexpected%2C1648971418307%2C150641%2C250100865708545%3BCometHomeRoot.react%2Ccomet.home%2Cvia_cold_start%2C1648971407454%2C265249%2C4748854339%22%2C%22is_tracking_encrypted%22%3Atrue%2C%22page_id%22%3A%22" + pageId + "%22%2C%22source%22%3A%22unknown%22%2C%22tracking%22%3A%5B%5D%2C%22actor_id%22%3A%22" + actor_id + "%22%2C%22client_mutation_id%22%3A%221%22%7D%2C%22isAdminView%22%3Afalse%7D");
        }
        public void GetViewVideo(string url, long bytestart, long byteend)
        {
            HTTPWebRequest("GET", url + "&bytestart=" + bytestart + "&byteend=" + byteend);
        }
        public void ViewNW()
        {
            HTTPWebRequest("GET", "https://facebook.com/nw/");
        }
        public string JoinGroup(string groupId)
        {
            string variables = "{|group_id|:|" + groupId + "|}";
            string param = "__user=" + uid;
            param += "&v=" + uid;
            param += "&__user=" + uid;
            param += "&__a=1";
            param += "&__dyn=7AzHxqU5a5Q1ryaxG4VuC0BVU98nwgU765QdwSwAyU8EW0CEboG4E762S1DwUx60gu0BU2_CxS320om78c87m2210wEwgolzUO0-E4a3aUS2G5Usw9m1YwBgK7o884y0Mo4G4Ufo5m1mzXw8W58jwGzEaE5e7oqBwJK2W5olwUwlu5pUfE2FBx_y88E3VBwJwSyES0Io88cA0z8c84qifxe3u364UrwFg662S261rw";
            param += "&__csr=gbc8ha599NcRjh9dNsyWv-DfimhjkhIGCFH49JFO49iTaQGSQRZ9sz88iFmVbiAybXWirz9vXCXp9mqXBFWiWABjymi9Cl7hUyXz48AmeKitpqGiuqEO8zExqVk8xmuu49F8-r9g9alDzppbHCDzeeye224e9UO798ngK48CqUeWxqdxi4axe2aq9KiazWG8CUgxl0hEdE456zK4UVxi2Si6okxeawgo4KfwGx-Ud9olwa259HyojwGwwxymfzF8661jz85S5o2qwMxmvyEc8nxi02Jy055E28ypo2JxCq063E4G00MeUfo0Be04ho5i0iy210ySow1IE14E1_U0kywcS0ePwb6Oiw3x80HME0xu03eAw0Na0nS0HE0Ue0jC15w2QE1N40Xo0Dm3a0SW83e1sw";
            param += "&__req=1o";
            param += "&__ccg=EXCELLENT";
            param += "&fb_dtsg=" + fb_dtsg;
            param += "&__comet_req=15";
            param += "&__hs=19315.HYP%3Acomet_pkg.2.1.1.2.1";

            param += "&__hs=19377.HYP:comet_pkg.2.1.0.2.1";
            param += "&dpr=1";
            param += "&__hsi=7120671559589022268";
            param += "&__rev=1006840155";

            param += "&variables=" + variables;

            param += "&__comet_req=1";
            param += "&jazoest=25368";
            param += "&lsd=s4sYCISD9Lyi6lg70nmEue";
            param += "&__spin_r=1005611264";
            param += "&__spin_b=trunk";
            param += "&__spin_t=1674190730";
            param += "&lsd=1t1KuJvIiJAK1rNqkzvdRx";
            param += "&ph=C3";
            param += "&fb_api_caller_class=RelayModern";
            param += "&fb_api_req_friendly_name=useGroupsCometHeaderControlsQuery";
            param += "&server_timestamps=true";

            return HTTPWebRequest("POST", "https://www.facebook.com/api/graphql/", param);
        }
        public void RequestBNZAI(long ts)
        {
            string variables = "[{|app_id|:|2220391788200892|,|posts|:0,|user|:|" + uid + "|,|webSessionId|:|x6lyho:8joelm:1b7i6q|},|trigger|:|gk2_exposure|,|send_method|:|ajax|,|compression|:|deflate|,|snappy_ms|:|2|}]";
            string param = "__user=" + uid;
            param += "&__a=1";
            param += "&__ccg=EXCELLENT";
            param += "&fb_dtsg=" + fb_dtsg;
            param += "&__comet_req=15";
            param += "&__hs=19315.HYP%3Acomet_pkg.2.1.1.2.1";

            param += "&__hsi=7120671559589022268";
            param += "&__req=35";
            param += "&__rev=1005852782";

            param += "&ts=" + ts;
            param += "&q=" + variables;
            ; ;
            param += "&__comet_req=1";
            param += "&jazoest=25368";
            param += "&lsd=s4sYCISD9Lyi6lg70nmEue";
            param += "&__spin_r=1005611264";
            param += "&__spin_b=trunk";
            param += "&__spin_t=1654085852";
            param += "&lsd=1t1KuJvIiJAK1rNqkzvdRx";
            param += "&ph=C3";

            HTTPWebRequest("POST", "https://web.facebook.com/ajax/bnzai", param);
        }
        public void LikePageAsPage(string targe_page_id, string source_page_id = "")
        {
            string actor_id = uid;
            string variables = "{|input|:{|source_page_id|:|" + source_page_id + "|,|targe_page_id|:|" + targe_page_id + "|,|actor_id|:|" + actor_id + "|,|client_mutation_id|:|17|},|targetPageId|:|" + targe_page_id + "|,|isAdminView|:true}";
            string param = "av=" + uid;
            param += "&__usid=6-Trcstro1ktf2k4:Prcsst88ns09m:0-Arcstro1fb1fv4-RV=6:F=";
            param += "&__a=1";
            param += "&dpr=1";
            param += "&fb_dtsg=" + fb_dtsg;
            param += "&fb_api_caller_class=RelayModern";
            param += "&fb_api_req_friendly_name=CometPageLikePageAsPageMutation";

            param += "&server_timestamps=true";
            param += "&doc_id=7170838572990679";
            param += "&fb_api_analytics_tags=[|qpl_active_flow_ids=431626192|]";
            param += "&variables=" + variables;

            param += "&__req=53";
            param += "&__ccg=GOOD";
            param += "&__rev=1005611264";
            param += "&__s=k9j3u7:ivnpu3:5x266r";
            param += "&__hsi=7104244640658143481-0";
            param += "&__comet_req=1";
            param += "&jazoest=22097";
            param += "&lsd=s4sYCISD9Lyi6lg70nmEue";
            param += "&__spin_r=1005611264";
            param += "&__spin_b=trunk";
            param += "&__spin_t=1654085852";
            param += "&__dyn=7AzHK4HwBwIxt0mUyEqxenFw9uu2i5U4e1Nxt3odE98K2aew9G2Saxa1NwJwpUe8hwaG1sw9u0LVEtwMw65xO321Rwwwg8a8465o-cwfG12wOKdwGxu782ly87e2l2Utwwwi831wiEjwZxy3O1mzXxG1Pxi4UaEW2G1jxS6FobrwKxm5oe8cEW4-5pUfEe88o4Wm7-8wywfCm2Sq2-azo2NwwwOg2cwMwhF8-4UdU";

            param += "&__csr=g9cbSyNssB7P8lfkj4EHN4IBssDFijGXJJlLnMAAzGKy8yGAADnFGRF9lV4a-RjAFR9qm4-jSLiARHmi8HiBKrJ5h9k-iHjjGV4jhfiyuhyHF7t24aBx7Cnr_jxW9yqG8CyXl2HGENAmXnyXCDyFqyA-4EWFGjJ2poKnGiWwYyoKUaA8Kt91Gcx2UqVVojzpu32XzFoK44awLxibwGwwwOz8W7Kq2Sm3um22dyEaaxC2Oq2Ccy8hwkbDwh8fawsEows8bocoW4awNwrVoco4q7EG0gi2i2m1Vg0gqw62w9y1Bw5Sw4Ww9y0VopwKwto1h8W0FE7W09Tw3_E3Uwee054Q0mucx20iuu2WE5C0eAw0xGo8EdE1sU0WOeBwHw21E0zO0pm0bGw62wpES641rwvpEmGujnwa-i48Sew9y19wh9U1-EbUVBy8loK0uaubwl8pxe1FU5S1yyo1CH-UeAfhofE4i9gqCUgyWK5bzu8yA4GxW0tPwcO0d4zo1vU09goow1QW0jC78b811oyU4i2i6Elx25Umw29E3qwvm2O0lS4EO0axg4u0Ubxe7U5aUa8ao2Qxi08Iw11G08DwhE";

            HTTPWebRequest("POST", "https://www.facebook.com/api/graphql/", param);
        }
        public void RequestVideo(int bytesstart, int byteend)
        {
            //string url = "https://z-p3-video.fpnh18-1.fna.fbcdn.net/v/t42.1790-2/277655792_814010323322718_5062150426527674311_n.mp4?_nc_cat=102&ccb=1-5&_nc_sid=5aebc0&efg=eyJ2ZW5jb2RlX3RhZyI6ImRhc2hfYXVkaW9fYWFjcF80OF9mcmFnXzJfYXVkaW8ifQ==&_nc_eui2=AeFwSYf80WiodbWdj-753xgXE8RbqTd61CcTxFupN3rUJ6HeRHbMB4T6r4A-76admNy0EntuAy3Awf29ttReuPJW&_nc_ohc=IFzfBdLRjz0AX93xPom&_nc_ht=z-p3-video.fpnh18-1.fna&oh=00_AT_F8aN8vyHra4_CbXHPWLiupMp9bUvIYkH0FCiyO_z1TQ&oe=624E78BB&bytestart="+bytesstart+"&byteend="+byteend;
            //HTTPWebRequest("GET", "https://web.facebook.com/video/unified_cvc/", param);
        }
        public void ViewCVC(int pf1, int pf2, string videoID)
        {
            string pps = "null";
            if (pf1 > 0 || true)
            {
                pps = "{|m|:false,|pf|:" + pf1 + ",|s|:|playing|,|sa|:567}";
            }
            string d = "{|pps|:" + pps + ",|ps|:{|m|:false,|pf|:" + pf2 + ",|s|:|playing|,|sa|:567},|si|:|fd12391d1fb4d8|,|so|:|inline::entry_point|,|vi|:|" + videoID + "|,|tk|:|ztMVXtxTzw6vXdGOO+rPUjVgSsIk0uudybSSseyD4rYpa1Z1FrMP9EAo0znXghO2|}";
            string param = "__user=" + uid;
            param += "&__a=1";
            param += "&fb_dtsg=" + fb_dtsg;
            param += "&d=" + d;

            param += "&__req=h";
            param += "&__hs=19088.HYP:comet_pkg.2.1.0.2.";
            param += "&__ccg=EXCELLENT";
            param += "&__rev=1005306012";
            param += "&__s=j9n5nn:rgxgfj:dhtmio";
            param += "&__hsi=7083332981746944976-0";
            param += "&__comet_req=1";
            param += "&jazoest=22035";
            param += "&lsd=CEZlz55kxVRq27at7mngEQ";
            param += "&__spin_r=1005306012";
            param += "&__spin_b=trunk";
            param += "&__spin_t=1649216977";

            param += "&__dyn=7AzHK4HwkEng5KbxG4VuC0BVU98nwgU29gS3q2ibwyzE2qwJxS1NwJwkEkwUx60gu0luq7oc81xoszU2mwwwg8a8465o-cwfG12wOKdwGwQw9m8wsU9kbxS2218wIw9i1axe3S68f85qfK6E7e58jwGzEaE5e7oqBwJK2W5olwUwgojUlDw-wAxe1MBx_y88E6a0BFobodEGdwb6223908O3216AzUjw";
            param += "&__csr=gN4MhlERgHtNBlOqiZ9EDniq9N7t_OWRvquJH8htRhdpGqH_nVkhQhaqAgxbFASHhpnKjHRqG_XhdqDV5QRBmKqVKAbDypF8VF4G4HUFabDy8gDBz9F8Gu8GHiABih8S8y8qAG5ohStoCi5et1aV9ki2m6oOmHxufGbguxCawAyV9FojwJwPwQw_HwIK6EpKQ1bzQGwSwb21hU6mUrCK8geE8E6mdwAwpU2kwf2fwgVEpwqU5S1DwHwoo0w63pw0-Iw2g802jIwaK211gwao4a4U0BO3fg0B204fo0mXw0G7w0Dpw288S";


            HTTPWebRequest("POST", "https://web.facebook.com/video/unified_cvc/", param);
        }
        public void ViewCVC_dsfds(int pf1, int pf2, string videoID)
        {
            string pps = "null";
            if (pf1 > 0 || true)
            {
                pps = "{|m|:false,|pf|:" + pf1 + ",|s|:|playing|,|sa|:567}";
            }
            string d = "{|pps|:" + pps + ",|ps|:{|m|:false,|pf|:" + pf2 + ",|s|:|playing|,|sa|:567},|si|:|fd12391d1fb4d8|,|so|:|inline::entry_point|,|vi|:|" + videoID + "|,|tk|:|ztMVXtxTzw6vXdGOO+rPUjVgSsIk0uudybSSseyD4rYpa1Z1FrMP9EAo0znXghO2|}";
            string param = "__user=" + uid;
            param += "&__a=1";
            param += "&fb_dtsg=" + fb_dtsg;
            param += "&d=" + d;

            param += "&__req=h";
            param += "&__hs=19088.HYP:comet_pkg.2.1.0.2.";
            param += "&__ccg=EXCELLENT";
            param += "&__rev=1005306012";
            param += "&__s=j9n5nn:rgxgfj:dhtmio";
            param += "&__hsi=7083332981746944976-0";
            param += "&__comet_req=1";
            param += "&jazoest=22035";
            param += "&lsd=CEZlz55kxVRq27at7mngEQ";
            param += "&__spin_r=1005306012";
            param += "&__spin_b=trunk";
            param += "&__spin_t=1649216977";

            param += "&__dyn=7AzHK4HwkEng5KbxG4VuC0BVU98nwgU29gS3q2ibwyzE2qwJxS1NwJwkEkwUx60gu0luq7oc81xoszU2mwwwg8a8465o-cwfG12wOKdwGwQw9m8wsU9kbxS2218wIw9i1axe3S68f85qfK6E7e58jwGzEaE5e7oqBwJK2W5olwUwgojUlDw-wAxe1MBx_y88E6a0BFobodEGdwb6223908O3216AzUjw";
            param += "&__csr=gN4MhlERgHtNBlOqiZ9EDniq9N7t_OWRvquJH8htRhdpGqH_nVkhQhaqAgxbFASHhpnKjHRqG_XhdqDV5QRBmKqVKAbDypF8VF4G4HUFabDy8gDBz9F8Gu8GHiABih8S8y8qAG5ohStoCi5et1aV9ki2m6oOmHxufGbguxCawAyV9FojwJwPwQw_HwIK6EpKQ1bzQGwSwb21hU6mUrCK8geE8E6mdwAwpU2kwf2fwgVEpwqU5S1DwHwoo0w63pw0-Iw2g802jIwaK211gwao4a4U0BO3fg0B204fo0mXw0G7w0Dpw288S";


            HTTPWebRequest("POST", "https://web.facebook.com/video/unified_cvc/", param);
        }
        public void CreatePageClassic(string pageName, string categoriesID)
        {
            string variables = "{|input|:{|categories|:[|" + categoriesID.Trim() + "|],|description|:||,|name|:|" + pageName.Trim() + "|,|publish|:true,|ref|:|launch_point|,|actor_id|:|" + uid + "|,|client_mutation_id|:|3|}}";
            string param = "av=" + uid;
            param += "&__user=" + uid;
            param += "&__a=1";
            param += "&dpr=1";
            param += "&fb_dtsg=" + fb_dtsg;
            param += "&fb_api_caller_class=RelayModern";
            param += "&fb_api_req_friendly_name=CometPageCreateMutation";
            param += "&server_timestamps=true";
            param += "&doc_id=6015849741773814";
            //param += "&fb_api_analytics_tags=[|qpl_active_flow_ids=30605361|]";
            param += "&x-fb-lsd=|mIFD7W9Fq-HtjKSIXKePTA|";
            param += "&variables=" + variables;

            param += "&__req=2c";
            param += "&__ccg=EXCELLENT";
            param += "&__rev=1005290467";
            param += "&__s=y69n0q:n2vngq:2hpij5";
            param += "&__hsi=7082724547208442634-0";
            param += "&__comet_req=1";
            param += "&jazoest=21891";
            param += "&lsd=l5_BHihe5PkwxJ1DkdhBuy";
            param += "&__spin_r=1005290467";
            param += "&__spin_b=trunk";
            param += "&__spin_t=1649075315";
            param += "&__dyn=7AzHK4HwBwIxt0mUyEqxenFw9uu2i5U4e0ykdwSwAyU8EW0CEboG4E762S1DwUx60GE3Qwb-q7oc81xoswMwto886C11xmfz83WwgEcHzoaEd82ly87e2l2Utwwwi831wiEjwZxy3O1mzXxG1Pxi4UaEW2G1jxS6FobrwKxm5oe8cEW4-5pUfEe88o4Wm7-8wywfCm2S3qazo2NwwwOg2cwMwhF8-4U";

            param += "&__csr=gx3AcYQ8W97vtsj9Fiha999kJsGsOtSx7QBbh4HkJpviGyenGJilt9bqp4jh-iF99FC9hKuWxnGdBhEWcDiVER2Aiuh3p9FeFWCAG-mUqybyESmiVuqcBzpFoy5EoximczErCKaG58aomxOQfxW4oaonxC2u2y5EW4oeUszHyA9gO1Owhoao6Cm5UbUbE7GU6u2e2i6U2Fz84S5VU6m1Fw8O2e1vCxq1wy87C1PyE3IweG1Kxi363W0sO0UEC1bG3yE5a7U0vFw9C9w3e825w6iyUnw1Gq079E0Cp02go2vw67w2s81aokwduew11i086yE2dK78pwjUXAyQ3p3o6i17wSw9m14g7678G2C1eLCw_wOUW488Q8yi1PwyjGUrwBwiE8EbedxK2O0AUsw4CwIw8W0xU26g2ywTU9EdU0xS0fEwZwBwfa02860ry0rq0fkyV8aofEC7E-i0xpUyU5S1hw8Kawju0Vo6K7823wey9w3ZolAU0H605lo";

            SetReferer("https://web.facebook.com/pages/creation/?ref_type=launch_point");
            HTTPWebRequest("POST", "https://www.facebook.com/api/graphql/", param);
        }
        public void SetReferer(string refe)
        {
            referer = refe;
        }
        public CookieContainer GetCookieContainerFromDriver(IWebDriver driver)
        {
            var cc = new CookieContainer();
            try
            {
                foreach (var cookie in driver.Manage().Cookies.AllCookies)
                {
                    System.Net.Cookie netcookie = new System.Net.Cookie()
                    {
                        Domain = cookie.Domain,
                        HttpOnly = cookie.IsHttpOnly,
                        Name = cookie.Name,
                        Path = cookie.Path,
                        Secure = cookie.Secure,
                        Value = cookie.Value,
                    };
                    if (cookie.Name == "c_user")
                    {
                        uid = cookie.Value;
                    }
                    if (cookie.Expiry.HasValue)
                    {
                        netcookie.Expires = cookie.Expiry.Value;
                    }
                    cc.Add(netcookie);
                }
            }
            catch (Exception) { }
            container2 = cc;
            userAgent = (string)((IJavaScriptExecutor)driver).ExecuteScript("return navigator.userAgent;");


            return cc;
        }
        public string HTTPWebRequest(string requestType, string url, string req = "")
        {
            try
            {
                //ServicePointManager.Expect100Continue = false;
                //ServicePointManager.UseNagleAlgorithm = false;

                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                request.Proxy = null;
                //request.ServicePoint.Expect100Continue = false;
                //request.ServicePoint.UseNagleAlgorithm = false;

                request.AllowAutoRedirect = true;
                request.KeepAlive = true;

                request.ContentType = contentType;

                if (container2 != null)
                {
                    request.CookieContainer = container2;
                }
                accept = "*/*";
                request.Referer = referer;
                request.Accept = accept;
                request.UserAgent = userAgent;

                //request.Headers.Add("Sec-Fetch-Dest", "empty");
                //request.Headers.Add("Sec-Fetch-Mode", "navigate");
                //request.Headers.Add("Sec-Fetch-Site", "none");
                //request.Headers.Add("Sec-Fetch-User", "21");
                //request.Headers.Add("Upgrade-Insecure-Requests", "1");

                req = req.Replace('|', '"');

                if (requestType == "POST")
                {
                    var postData = req;
                    var data = Encoding.ASCII.GetBytes(postData);
                    request.ContentLength = data.Length;
                    request.Method = "POST";
                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }
                else
                {
                    request.Method = "GET";
                }
                var response = (HttpWebResponse)request.GetResponse();
                var resStream = response.GetResponseStream();

                var responseString = new StreamReader(resStream).ReadToEnd();

                resStream.Close();
                response.Close();

                return responseString;
            }
            catch (Exception e) { return e.Message; }

            return "";
        }
        public string GetStringBetween(string text, string start, string end)
        {
            int p1 = text.IndexOf(start) + start.Length;
            int p2 = text.IndexOf(end, p1);

            if (end == "")
            {
                return (text.Substring(p1));
            }
            else
            {
                return text.Substring(p1, p2 - p1);
            }

            return "";
        }
        public string[] GetVideosRequestWatchTime(string html)
        {
            html = html.Replace('"', '|');
            //html = html.Replace("/", "k");
            string[] arr = html.Replace("z-p3-video.fpnh18-3.fna.fbcdn.net", "!").Split('!');
            string[] arrRes = null;

            for (int i = 0; i < arr.Length; i++)
            {
                try
                {
                    Console.WriteLine(arr[i]);
                    arrRes[i] = "https://z-p3-video.fpnh18-3.fna.fbcdn.net/v/" + GetStringBetween(arr[i], "/v/", "|,|");
                    Console.WriteLine("=========");
                    Console.WriteLine(arrRes[i]);
                    Console.WriteLine("=========");
                }
                catch (Exception) { }
            }

            return arrRes;
        }
        public void SetHttpWebRequestCookies(string browserCookies, string domainx = ".facebook.com")
        {
            string[] tmpCookies = browserCookies.Split(';');

            foreach (string cookies in tmpCookies)
            {
                string[] cookie = cookies.Split('=');
                Cookie tmpCookiex = new Cookie();

                try
                {
                    tmpCookiex.Name = cookie[0];
                    tmpCookiex.Value = cookie[1];
                    tmpCookiex.HttpOnly = false;
                    tmpCookiex.Secure = true;
                    //tmpCookiex.Expires = DateTime.Now.AddYears(5);
                    tmpCookiex.Path = "/";
                    tmpCookiex.Domain = domainx;
                }
                catch (Exception) { }

                if (tmpCookiex.Name == "c_user")
                {
                    uid = tmpCookiex.Value;
                }
                if (domainx.Contains(".facebook.com"))
                {
                    try
                    {
                        container2.Add(new Uri("https://www.facebook.com"), tmpCookiex);
                    }
                    catch (Exception) { }
                    //try
                    //{
                    //    container2.Add(new Uri("https://web.facebook.com"), tmpCookiex);
                    //}
                    //catch (Exception) { }
                }
                else
                {
                    try
                    {
                        container2.Add(new Uri("http://www" + domainx), tmpCookiex);
                    }
                    catch (Exception) { }
                }
            }
        }
    }
    public class FBProfile
    {
        [JsonProperty("id")]
        public string id { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("gender")]
        public string gender { get; set; }
        [JsonProperty("email")]
        public string email { get; set; }
        [JsonProperty("birthday")]
        public string birthday { get; set; }
        public override string ToString()
        {
            return " [ id : " + id + " , name :" + name + " , gender :" + gender + " , email :" + email + " , birthday :" + birthday + " ]";
        }
    }
    public class FBProfileGroup
    {
        [JsonProperty("groups")]
        public FBGroup groups { get; set; }
        public override string ToString()
        {
            return " [ groups : " + groups + " ]";
        }
    }
    public class FBGroup
    {
        [JsonProperty("nodes")]
        public ObservableCollection<FBGroupNodes> nodes { get; set; }
        public override string ToString()
        {
            return " [ nodes : " + nodes + " ]";
        }
    }
    public class FBGroupNodes
    {
        [JsonProperty("id")]
        public string id { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("viewer_post_status")]
        public string viewer_post_status { get; set; }
        [JsonProperty("visibility")]
        public string visibility { get; set; }
        [JsonProperty("group_member_profiles")]
        public FBGroupMemberProfiles group_member_profiles { get; set; }
        public override string ToString()
        {
            return " [ id : " + id + " , name :" + name + " , viewer_post_status :" + viewer_post_status + " , viewer_post_status :" + visibility + " , group_member_profiles :" + group_member_profiles + " ]";
        }
    }
    public class FBGroupMemberProfiles
    {
        [JsonProperty("count")]
        public int count { get; set; }

    }
}
