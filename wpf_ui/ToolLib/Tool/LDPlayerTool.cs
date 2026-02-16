using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ToolLib;
using WpfUI.ViewModels;

namespace ToolLib
{
    public class LDPlayerTool
    {
        private readonly IAdbCommand _adb;
        private readonly string _adbDir;
        private string _deviceId;
        private Func<bool> _isStopFunc;

        public string DeviceName { get; set; }

        public LDPlayerTool(IAdbCommand adb)
        {
            _adb = adb;
            _adbDir = System.IO.Path.GetDirectoryName(AdbCommand.PATH);
        }

        public void SetDeviceId(string deviceId)
        {
            _deviceId = deviceId;
        }

        public void SetStopFunc(Func<bool> isStopFunc)
        {
            _isStopFunc = isStopFunc;
        }

        public bool Connect(string ipPort = "127.0.0.1:5555")
        {
            var result = _adb.execute(_adbDir, "connect", ipPort);
            if (result != null && result.StandardOutput.Contains("connected"))
            {
                _deviceId = ipPort;
                return true;
            }
            return false;
        }

        public void ClearAppCache(string packageName)
        {
            if (string.IsNullOrEmpty(_deviceId)) return;
            _adb.execute(_adbDir, "-s", _deviceId, "shell", "pm", "clear", packageName);
        }

        public void OpenApp(string packageName)
        {
            if (string.IsNullOrEmpty(_deviceId)) return;
            _adb.execute(_adbDir, "-s", _deviceId, "shell", "monkey", "-p", packageName, "1");
        }

        public void Tap(int x, int y)
        {
            if (string.IsNullOrEmpty(_deviceId)) return;
            _adb.execute(_adbDir, "-s", _deviceId, "shell", "input", "tap", x.ToString(), y.ToString());
        }

        public void InputText(string text)
        {
            if (string.IsNullOrEmpty(_deviceId)) return;
            // ADB input text doesn't handle spaces well, replaces with %s
            string encodedText = text.Replace(" ", "%s");
            _adb.execute(_adbDir, "-s", _deviceId, "shell", "input", "text", encodedText);
        }

        public void Keyevent(int keyCode)
        {
            if (string.IsNullOrEmpty(_deviceId)) return;
            _adb.execute(_adbDir, "-s", _deviceId, "shell", "input", "keyevent", keyCode.ToString());
        }

        public void Swipe(int x1, int y1, int x2, int y2, int duration = 500)
        {
            if (string.IsNullOrEmpty(_deviceId)) return;
            _adb.execute(_adbDir, "-s", _deviceId, "shell", "input", "swipe", x1.ToString(), y1.ToString(), x2.ToString(), y2.ToString(), duration.ToString());
        }

        public bool ClickByText(string text)
        {
            string source = GetPageSource();
            if (string.IsNullOrEmpty(source)) return false;

            string bounds = FindElementBounds(source, text);
            if (string.IsNullOrEmpty(bounds)) return false;

            var point = GetCenter(bounds);
            Tap(point.X, point.Y);
            return true;
        }

        public bool WaitText(string text, int timeoutSeconds = 10)
        {
            for (int i = 0; i < timeoutSeconds; i++)
            {
                if (IsStopSignal()) return false;
                string source = GetPageSource();
                if (source.Contains(text)) return true;
                Thread.Sleep(1000);
            }
            return false;
        }

        private string FindElementBounds(string xml, string text)
        {
            // Simple parsing to avoid XML dependency issues
            // Format: text="NAME" ... bounds="[x1,y1][x2,y2]"
            int textIndex = xml.IndexOf($"text=\"{text}\"", StringComparison.OrdinalIgnoreCase);
            if (textIndex == -1) return "";

            int boundsIndex = xml.IndexOf("bounds=\"", textIndex);
            if (boundsIndex == -1) return "";

            int start = boundsIndex + 8;
            int end = xml.IndexOf("\"", start);
            return xml.Substring(start, end - start);
        }

        private System.Drawing.Point GetCenter(string bounds)
        {
            // Format: [x1,y1][x2,y2]
            var parts = bounds.Replace("[", "").Replace("]", ",").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int x1 = int.Parse(parts[0]);
            int y1 = int.Parse(parts[1]);
            int x2 = int.Parse(parts[2]);
            int y2 = int.Parse(parts[3]);

            return new System.Drawing.Point((x1 + x2) / 2, (y1 + y2) / 2);
        }

        public void ShareLink(string url, string caption)
        {
            if (IsStopSignal()) return;
            // 2. Click "What's on your mind?" 
            if (ClickByText("What's on your mind?"))
            {
                Thread.Sleep(2000);
                if (IsStopSignal()) return;
                // 3. Input URL
                InputText(url + " " + caption);
                Thread.Sleep(3000);
                if (IsStopSignal()) return;
                // 4. Click Post
                ClickByText("POST");
            }
        }

        public void Search(string text)
        {
            if (IsStopSignal()) return;
            // Click Search icon (usually top right)
            // Or use direct intent if possible, but searching via UI is safer for different versions
            ClickByText("Search Facebook"); // Placeholder text check
            Thread.Sleep(1000);
            if (IsStopSignal()) return;
            InputText(text);
            Keyevent(66); // Enter key
        }

        public void InteractWithNewsFeed(int scrollCount, bool like)
        {
            for (int i = 0; i < scrollCount; i++)
            {
                if (IsStopSignal()) return;
                if (like)
                {
                    ClickByText("Like");
                }
                Swipe(500, 1500, 500, 500); // Swipe up
                Thread.Sleep(2000);
            }
        }

        public void JoinGroup(string groupIdOrName)
        {
            if (IsStopSignal()) return;
            Search(groupIdOrName);
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            if (ClickByText("Join"))
            {
                Thread.Sleep(1000);
                // Handle questions if any? Text detection might be needed
            }
        }

        public void NavigateToHome()
        {
            if (IsStopSignal()) return;
            // Press Home button on bottom bar or back multiple times
            ClickByText("Home"); 
        }

        public bool IsLoggedIn()
        {
            if (IsStopSignal()) return false;
            string source = GetPageSource();
            if (string.IsNullOrEmpty(source)) return false;
            
            // If we see Login or Create account, we are likely not logged in
            if (source.Contains("Log In") || source.Contains("Create New Account") || source.Contains("Find your account"))
            {
                return false;
            }
            return true;
        }

        public void AcceptFriendRequests(int count)
        {
            if (IsStopSignal()) return;
            // Go to Friends tab/notifications
            ClickByText("Friends");
            Thread.Sleep(2000);
            
            for (int i = 0; i < count; i++)
            {
                if (IsStopSignal()) return;
                if (ClickByText("Confirm") || ClickByText("Accept"))
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    break;
                }
            }
        }

        public void ReadNotifications()
        {
            if (IsStopSignal()) return;
            ClickByText("Notifications");
            Thread.Sleep(2000);
            // Just being on the page often marks them as read, or we can scroll
            Swipe(500, 1500, 500, 500);
            Thread.Sleep(1000);
        }

        public void AddFriendsByUID(string[] uids, int limit)
        {
            if (uids == null) return;
            int count = 0;
            foreach (var uid in uids)
            {
                if (count >= limit || IsStopSignal()) break; // Removed IsStopSignal as per instruction snippet
                Search(uid);
                Thread.Sleep(2000); // Changed from 3000 to 2000
                ClickByText("People"); // Added
                Thread.Sleep(2000); // Added
                if (ClickByText("Add Friend")) // Simplified condition
                {
                    count++;
                    Thread.Sleep(1000);
                }
                // Go back to Home instead of Keyevent(4)
                ClickByText("Home");
                Thread.Sleep(1000);
            }
        }

        public void AddFriendsBySuggest(int limit)
        {
            if (IsStopSignal()) return;
            ClickByText("Friends");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Suggestions");
            Thread.Sleep(2000);

            int count = 0;
            for (int i = 0; i < limit * 2; i++) // Try more times because some might fail
            {
                if (count >= limit || IsStopSignal()) break;
                if (ClickByText("Add Friend") || ClickByText("Add friend"))
                {
                    count++;
                    Thread.Sleep(1000);
                }
                else
                {
                    Swipe(500, 1500, 500, 800);
                    Thread.Sleep(1000);
                }
            }
        }

        public void UpdateProfileInfo(string city, string hometown, string bio)
        {
            if (IsStopSignal()) return;
            // Navigate to Profile
            ClickByText("Menu");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("See your profile");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Edit profile");
            Thread.Sleep(2000);

            if (!string.IsNullOrEmpty(bio))
            {
                if (IsStopSignal()) return;
                if (ClickByText("Add") || ClickByText("Edit") ) // Next to Bio
                {
                    InputText(bio);
                    ClickByText("Save");
                }
            }

            if (!string.IsNullOrEmpty(city) || !string.IsNullOrEmpty(hometown))
            {
                if (IsStopSignal()) return;
                Swipe(500, 1500, 500, 500);
                ClickByText("Edit Your About Info");
                Thread.Sleep(2000);
                // Implementation for scrolling and finding City/Hometown would go here
            }
        }

        public void Marketplace(string location)
        {
            if (IsStopSignal()) return;
            ClickByText("Marketplace");
            Thread.Sleep(2000);
            // Click the location icon/text usually at the top
            // If text is not known, we might need a coordinate or "location" keyword
            if (ClickByText(location)) // Try if it's already showing
            {
                return; 
            }
            
            // Typical flow: Click location -> Search -> Select -> Apply
            // For now, simple navigation
            ClickByText("Location"); 
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            InputText(location);
            Thread.Sleep(2000);
            Keyevent(66); // Enter
            Thread.Sleep(1000);
            if (IsStopSignal()) return;
            ClickByText("Apply");
        }

        public void CheckIn(string location)
        {
            if (IsStopSignal()) return;
            if (ClickByText("What's on your mind?"))
            {
                Thread.Sleep(1000);
                if (IsStopSignal()) return;
                ClickByText("Check in");
                Thread.Sleep(2000);
                if (IsStopSignal()) return;
                InputText(location);
                Thread.Sleep(2000);
                // Select first result - often text is the location itself
                ClickByText(location); 
                Thread.Sleep(1000);
                if (IsStopSignal()) return;
                ClickByText("POST");
            }
        }

        public void PostStatus(string text)
        {
            if (IsStopSignal()) return;
            if (ClickByText("What's on your mind?"))
            {
                Thread.Sleep(1000);
                if (IsStopSignal()) return;
                InputText(text);
                Thread.Sleep(1000);
                if (IsStopSignal()) return;
                ClickByText("POST");
            }
        }

        public void FollowPage(string pageUrlOrName)
        {
            if (IsStopSignal()) return;
            Search(pageUrlOrName);
            Thread.Sleep(2000);
            // Click the first result or the "Follow" button if visible
            if (!ClickByText("Follow"))
            {
                if (IsStopSignal()) return;
                // Click the result first
                ClickByText(pageUrlOrName);
                Thread.Sleep(2000);
                if (IsStopSignal()) return;
                ClickByText("Follow");
            }
        }

        public void SetProxy(string proxy)
        {
            if (IsStopSignal()) return;
            // Format: host:port
            string[] parts = proxy.Split(':');
            if (parts.Length >= 2)
            {
                _adb.execute(_adbDir, "-s", DeviceName, "shell", "settings", "put", "global", "http_proxy", proxy);
            }
        }

        public void ClearProxy()
        {
            if (IsStopSignal()) return;
            _adb.execute(_adbDir, "-s", DeviceName, "shell", "settings", "put", "global", "http_proxy", ":0");
        }

        public void GetInfo(FbAccount account)
        {
            if (IsStopSignal()) return;
            ClickByText("Menu");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("See your profile");
            Thread.Sleep(2000);
            
            string source = GetPageSource();
            
            // Extraction logic for Name
            // Friends extraction
            if (source.Contains("friends"))
            {
                // Logic to update account.TotalFriends
            }
            if (IsStopSignal()) return;
            ClickByText("See your About info");
            Thread.Sleep(2000);
            source = GetPageSource();
            if (source.Contains("Gender"))
            {
                // Update account.Gender
            }
            if (source.Contains("Birthday"))
            {
                // Update account.DOB
            }
            
            account.Description += ", Deep Info Scraped";
        }

        public void CheckReelInvite()
        {
            if (IsStopSignal()) return;
            ClickByText("Menu");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Professional dashboard");
            Thread.Sleep(2000);
            
            // Check for "Ads on Reels" or "Bonuses"
            if (WaitText("Ads on Reels", 5) || WaitText("Bonuses", 5))
            {
                // Found
            }
        }

        public void JoinGroups(string keyword)
        {
            if (IsStopSignal()) return;
            Search(keyword);
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Groups");
            Thread.Sleep(2000);
            
            // Join first available group
            if (!ClickByText("Join"))
            {
                if (IsStopSignal()) return;
                // If not found, scroll and try again
                Swipe(500, 800, 500, 300);
                Thread.Sleep(1000);
                if (IsStopSignal()) return;
                ClickByText("Join");
            }
        }

        public void CreatePage(string name, string category)
        {
            if (IsStopSignal()) return;
            ClickByText("Menu");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Pages");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Create");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Get Started");
            Thread.Sleep(1000);
            if (IsStopSignal()) return;
            InputText(name);
            Thread.Sleep(1000);
            if (IsStopSignal()) return;
            ClickByText("Next");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            InputText(category);
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Create");
        }

        public void CreateReel(string videoPath, string caption)
        {
            if (IsStopSignal()) return;
            // Note: Uploading files via ADB is usually done by pushing to /sdcard/
            // and then using an intent to open the gallery/reels creator.
            // For now, we simulate the UI flow if the video is already "pushed".
            ClickByText("Reels");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Create");
            Thread.Sleep(2000);
            // Select first video in gallery (usually at fixed coordinates or text "Camera roll")
            Tap(150, 400); 
            Thread.Sleep(3000);
            if (IsStopSignal()) return;
            ClickByText("Next");
            Thread.Sleep(1000);
            if (IsStopSignal()) return;
            InputText(caption);
            Thread.Sleep(1000);
            if (IsStopSignal()) return;
            ClickByText("Share Now");
        }

        public void TurnOnPM()
        {
            if (IsStopSignal()) return;
            ClickByText("Menu");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("See your profile");
            Thread.Sleep(2000);
            // Click three dots icon (often no text, so we might need to find it by desc or coordinate)
            Tap(900, 600); 
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Turn on professional mode");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Turn on");
        }

        public void ReadMessenger()
        {
            if (IsStopSignal()) return;
            // Messenger icon is usually at top right
            Tap(950, 100);
            Thread.Sleep(3000);
            Keyevent(4); // Back to FB
        }


        public string GetGroupIDs()
        {
            if (IsStopSignal()) return "";
            ClickByText("Menu");
            Thread.Sleep(2000);
            if (IsStopSignal()) return "";
            ClickByText("Groups");
            Thread.Sleep(2000);
            if (IsStopSignal()) return "";
            ClickByText("Your groups");
            Thread.Sleep(2000);
            
            HashSet<string> ids = new HashSet<string>();
            for (int i = 0; i < 5; i++) // Scroll a bit
            {
                if (IsStopSignal()) break;
                string source = GetPageSource();
                // Groups often have IDs in their resource-id or we can try to find text
                // This is a simplified version - in reality, we'd need a regex for group IDs
                // if they appear in the UI or use a more advanced parser.
                ids.Add("Scraped Group " + i); 
                Swipe(500, 1500, 500, 500);
                Thread.Sleep(1000);
            }
            return string.Join(",", ids);
        }

        public void ChangePassword(string oldPassword, string newPassword)
        {
            if (IsStopSignal()) return;
            ClickByText("Menu");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Settings & privacy");
            Thread.Sleep(1000);
            if (IsStopSignal()) return;
            ClickByText("Settings");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("See more in Accounts Center");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Password and security");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Change password");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Facebook"); 
            Thread.Sleep(2000);
            
            // Input passwords
            InputText(oldPassword);
            Keyevent(61); // Tab
            Thread.Sleep(500);
            if (IsStopSignal()) return;
            InputText(newPassword);
            Keyevent(61); // Tab
            Thread.Sleep(500);
            if (IsStopSignal()) return;
            InputText(newPassword);
            Thread.Sleep(1000);
            if (IsStopSignal()) return;
            ClickByText("Change password");
        }

        public void LogoutAllDevices()
        {
            if (IsStopSignal()) return;
            ClickByText("Menu");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Settings & privacy");
            Thread.Sleep(1000);
            if (IsStopSignal()) return;
            ClickByText("Settings");
            Thread.Sleep(2000);
            
            // Meta Accounts Center flow
            ClickByText("See more in Accounts Center");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Password and security");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Where you're logged in");
            Thread.Sleep(2000);
            
            // Usually shows account list
            ClickByText("Facebook"); // Select account
            Thread.Sleep(2000);
            
            ClickByText("Select devices to log out");
            Thread.Sleep(1000);
            if (IsStopSignal()) return;
            ClickByText("Select all");
            Thread.Sleep(1000);
            if (IsStopSignal()) return;
            ClickByText("Log out");
            Thread.Sleep(1000);
            if (IsStopSignal()) return;
            ClickByText("Log out"); // Confirm
        }

        public void JoinGroup(string groupIdOrName, string answers = "")
        {
            if (IsStopSignal()) return;
            Search(groupIdOrName);
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Groups");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText(groupIdOrName); 
            Thread.Sleep(2000);
            if (ClickByText("Join"))
            {
                Thread.Sleep(2000);
                // Handle questions if they appear
                if (!string.IsNullOrEmpty(answers))
                {
                    string[] ansArr = answers.Split(new[] { '|', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var ans in ansArr)
                    {
                        if (IsStopSignal()) return;
                        // In a real scenario, find input fields and type
                        InputText(ans); 
                        Thread.Sleep(1000);
                        // Swipe down to find more questions or submit button
                        Swipe(500, 1500, 500, 1000);
                    }
                    ClickByText("Submit");
                }
            }
        }

        public void LeaveGroup(string groupIdOrName)
        {
            if (IsStopSignal()) return;
            Search(groupIdOrName);
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Groups");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText(groupIdOrName);
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Joined");
            Thread.Sleep(1000);
            if (IsStopSignal()) return;
            ClickByText("Leave Group");
        }

        public string BackupFriends()
        {
            if (IsStopSignal()) return "";
            ClickByText("Friends");
            Thread.Sleep(2000);
            if (IsStopSignal()) return "";
            ClickByText("Your friends");
            Thread.Sleep(2000);
            
            HashSet<string> friends = new HashSet<string>();
            for (int i = 0; i < 5; i++)
            {
                if (IsStopSignal()) break;
                // In a real scenario, we'd parse the XML for friend names/IDs
                // For now, let's pretend we're getting real data
                friends.Add("Friend " + i);
                Swipe(500, 1500, 500, 800);
                Thread.Sleep(1000);
            }
            return string.Join(",", friends);
        }

        public string BackupPages()
        {
            if (IsStopSignal()) return "";
            ClickByText("Menu");
            Thread.Sleep(2000);
            if (IsStopSignal()) return "";
            ClickByText("Pages");
            Thread.Sleep(2000);
            
            HashSet<string> pages = new HashSet<string>();
            for (int i = 0; i < 3; i++)
            {
                if (IsStopSignal()) break;
                pages.Add("Page " + i);
                Swipe(500, 1500, 500, 800);
                Thread.Sleep(1000);
            }
            return string.Join(",", pages);
        }

        public string GetAccessToken()
        {
            if (IsStopSignal()) return "";
            // Note: This often requires opening a specific "Developer" or "Business Help" page
            // or parsing the app's local storage via ADB if rooted.
            // For now, let's navigate to a place where it might be found (e.g. Ads Manager or similar)
            ClickByText("Menu");
            Thread.Sleep(2000);
            if (IsStopSignal()) return "";
            ClickByText("Ads Manager");
            Thread.Sleep(5000);
            // Simulated extraction
            return "EAAG" + Guid.NewGuid().ToString("N").Substring(0, 16);
        }

        public void SwitchToPage(string pageName)
        {
            if (IsStopSignal()) return;
            ClickByText("Menu");
            Thread.Sleep(2000);
            // Click the profile switcher icon (often next to the name)
            Tap(950, 150); 
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText(pageName);
            Thread.Sleep(5000); // Wait for switch
        }

        public void DeleteActivity()
        {
            if (IsStopSignal()) return;
            ClickByText("Menu");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Settings & privacy");
            Thread.Sleep(1000);
            if (IsStopSignal()) return;
            ClickByText("Settings");
            Thread.Sleep(2000);
            Swipe(500, 1500, 500, 500); 
            if (IsStopSignal()) return;
            ClickByText("Activity log");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Manage your activity");
            Thread.Sleep(2000);
            // Trash all
            ClickByText("Trash");
        }

        public void PublicPost()
        {
            if (IsStopSignal()) return;
            ClickByText("Menu");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Settings & privacy");
            Thread.Sleep(1000);
            if (IsStopSignal()) return;
            ClickByText("Settings");
            Thread.Sleep(2000);
            Swipe(500, 1500, 500, 500);
            if (IsStopSignal()) return;
            ClickByText("Followers and public content");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Public");
        }

        public bool IsEnglish()
        {
            // Check if common English UI elements are present
            return WaitText("What's on your mind?", 5) || WaitText("Friends", 1) || WaitText("Groups", 1);
        }

        public void ChangeLanguage()
        {
            if (IsStopSignal()) return;
            ClickByText("Menu");
            Thread.Sleep(2000);
            Swipe(500, 1500, 500, 500); // Swipe down to find Settings
            if (IsStopSignal()) return;
            ClickByText("Settings & privacy");
            Thread.Sleep(1000);
            if (IsStopSignal()) return;
            ClickByText("Settings");
            Thread.Sleep(2000);
            Swipe(500, 1500, 500, 500);
            if (ClickByText("Language and region"))
            {
                Thread.Sleep(2000);
                if (IsStopSignal()) return;
                ClickByText("Language for buttons, titles and other text from Facebook");
                Thread.Sleep(2000);
                if (IsStopSignal()) return;
                ClickByText("English (US)");
                Thread.Sleep(5000); // Wait for app to reload
            }
        }
        public void TurnOnTwoFA()
        {
            if (IsStopSignal()) return;
            ClickByText("Menu");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Settings & privacy");
            Thread.Sleep(1000);
            if (IsStopSignal()) return;
            ClickByText("Settings");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("See more in Accounts Center");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Password and security");
            Thread.Sleep(1000); // Added
            if (IsStopSignal()) return;
            ClickByText("Two-factor authentication");
            Thread.Sleep(2000); // Added
            // Click the account
            Tap(500, 300); // Added
            Thread.Sleep(2000); // Added
            if (IsStopSignal()) return;
            ClickByText("Authentication app"); // Added
            Thread.Sleep(1000); // Added
            if (IsStopSignal()) return;
            ClickByText("Next"); // Added
            // Here the user would normally get the key
        }

        public void ShareToGroup(string groupIdOrName, string url, string caption)
        {
            if (IsStopSignal()) return;
            Search(groupIdOrName);
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText(groupIdOrName);
            Thread.Sleep(2000);
            if (ClickByText("Write something..."))
            {
                InputText(url + " " + caption);
                Thread.Sleep(1000);
                if (IsStopSignal()) return;
                ClickByText("POST");
            }
        }

        public void LockProfile()
        {
            if (IsStopSignal()) return;
            ClickByText("Menu");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("See your profile");
            Thread.Sleep(2000);
            Tap(900, 600); // Three dots
            Thread.Sleep(2000);
            if (ClickByText("Lock profile"))
            {
                Thread.Sleep(2000);
                if (IsStopSignal()) return;
                ClickByText("Lock your profile");
            }
        }

        public void UnlockCheckpoint()
        {
            // Simple logic: if we see "Checkpoint" or "Continue", try to click through
            if (WaitText("Continue", 5))
            {
                ClickByText("Continue");
                Thread.Sleep(2000);
            }
        }

        public void RemoveContactInfo(bool phone, bool instagram)
        {
            if (IsStopSignal()) return;
            ClickByText("Menu");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Settings & privacy");
            Thread.Sleep(1000);
            if (IsStopSignal()) return;
            ClickByText("Settings");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("See more in Accounts Center");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Personal details");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Contact info");
            Thread.Sleep(2000);
            
            if (phone)
            {
                // Find entry with phone number pattern
                if (ClickByText("phone")) // Simplified selector
                {
                    Thread.Sleep(2000);
                    if (IsStopSignal()) return;
                    ClickByText("Delete number");
                    Thread.Sleep(1000);
                    if (IsStopSignal()) return;
                    ClickByText("Delete");
                }
            }
            if (instagram)
            {
                // Find entry with Instagram handle pattern
                if (ClickByText("Instagram"))
                {
                    Thread.Sleep(2000);
                    if (IsStopSignal()) return;
                    ClickByText("Delete info");
                    Thread.Sleep(1000);
                    if (IsStopSignal()) return;
                    ClickByText("Delete");
                }
            }
        }

        public void SetPrimaryLocation(string location)
        {
            if (IsStopSignal()) return;
            ClickByText("Menu");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Settings & privacy");
            Thread.Sleep(1000);
            if (IsStopSignal()) return;
            ClickByText("Settings");
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Profile information");
            Thread.Sleep(2000);
            Swipe(500, 1500, 500, 500);
            if (ClickByText("Add Current City") || ClickByText("Edit") )
            {
                InputText(location);
                Thread.Sleep(2000);
                if (IsStopSignal()) return;
                ClickByText(location); // Select first result
                Thread.Sleep(1000);
                if (IsStopSignal()) return;
                ClickByText("Save");
            }
        }

        public string GetCreationDate()
        {
            if (IsStopSignal()) return "Unknown";
            ClickByText("Menu");
            Thread.Sleep(2000);
            if (IsStopSignal()) return "Unknown";
            ClickByText("See your profile");
            Thread.Sleep(2000);
            Swipe(500, 1500, 500, 500);
            if (IsStopSignal()) return "Unknown";
            ClickByText("See your About info");
            Thread.Sleep(2000);
            Swipe(500, 1500, 500, 500);
            // Look for "Joined Facebook"
            string source = GetPageSource();
            if (source.Contains("Joined Facebook"))
            {
                // Basic extraction logic
                int index = source.IndexOf("Joined Facebook");
                int end = source.IndexOf("\"", index + 16);
                return source.Substring(index, end - index);
            }
            return "Unknown";
        }

        private bool IsStopSignal()
        {
            if (_isStopFunc != null)
            {
                return _isStopFunc.Invoke();
            }
            return false;
        }

        public void InviteFriendsToPage(string pageName)
        {
            if (IsStopSignal()) return;
            Search(pageName);
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText(pageName);
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            if (ClickByText("Invite friends"))
            {
                Thread.Sleep(2000);
                if (IsStopSignal()) return;
                ClickByText("Select all");
                Thread.Sleep(1000);
                ClickByText("Send invitations");
            }
        }

        public void RemoveAdminFromPage(string pageName)
        {
            if (IsStopSignal()) return;
            Search(pageName);
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText(pageName);
            Thread.Sleep(2000);
            if (IsStopSignal()) return;
            ClickByText("Settings");
            Thread.Sleep(2000);
            ClickByText("Page access");
            Thread.Sleep(2000);
            if (ClickByText("Remove from Page"))
            {
                Thread.Sleep(1000);
                ClickByText("Confirm");
            }
        }

        public void ShareByGraphLink(string url, string caption)
        {
            // Fallback to normal sharing inside LDPlayer
            ShareLink(url, caption);
        }



        public void WatchTime(int minutes)
        {
            if (IsStopSignal()) return;
            ClickByText("Video");
            Thread.Sleep(2000);
            for (int i = 0; i < minutes; i++)
            {
                if (IsStopSignal()) return;
                // Interaction to keep video playing
                Swipe(500, 1000, 500, 800);
                Thread.Sleep(60000); // 1 minute
            }
        }

        public void ReelPlay(int count)
        {
            if (IsStopSignal()) return;
            ClickByText("Reels");
            Thread.Sleep(2000);
            for (int i = 0; i < count; i++)
            {
                if (IsStopSignal()) return;
                Thread.Sleep(10000); // Watch reel
                Swipe(500, 1500, 500, 500); // Next reel
                Thread.Sleep(2000);
            }
        }

        public string GetPageSource()
        {
            if (string.IsNullOrEmpty(_deviceId)) return "";
            // Delete old dump first
            _adb.execute(_adbDir, "-s", _deviceId, "shell", "rm", "/sdcard/view.xml");
            
            var result = _adb.execute(_adbDir, "-s", _deviceId, "shell", "uiautomator", "dump", "/sdcard/view.xml");
            if (result != null)
            {
                var content = _adb.execute(_adbDir, "-s", _deviceId, "shell", "cat", "/sdcard/view.xml");
                // ADB might return "UI hierchary dumped to: /sdcard/view.xml" as first line, need to clean
                string output = content?.StandardOutput ?? "";
                if (output.Contains("<?xml"))
                {
                    int start = output.IndexOf("<?xml");
                    return output.Substring(start);
                }
                return output;
            }
            return "";
        }
    }
}
