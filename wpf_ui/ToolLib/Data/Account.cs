using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolLib.Data
{
    public class Account
    {
        public int StoreId { get; set; }

        public long Id { get; set; }
        public string UID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string TwoFA { get; set; }
        public string Gender { get; set; }
        public string DOB { get; set; }
        public long Birthday { get; set; }

        public string Token { get; set; }
        public string Cookie { get; set; }
        public int Join { get; set; }
        public int Leave { get; set; }
        public int Share { get; set; }
        public int TotalFriend { get; set; }
        public int TotalRequest { get; set; }
        public int TotalGroup { get; set; }
        public int TotalPage { get; set; }
        public string PageIds { get; set; }

        public string Description { get; set; }
        public int Status { get; set; }
        public long Working { get; set; }
        public string PendingJoin { get; set; }
        public long UpdatedAt { get; set; }
        public int ShareTypeId { get; set; }


        public string Proxy { get; set; }
        public string UserAgent { get; set; }
        public int IsLogin { get; set; }
        public int FriendsRequest { get; set; }
        public int IsActive { get; set; }
        public int TotalShareGroup { get; set; }
        public int TotalShareTimeline { get; set; }
        public string GroupIDs { get; set; }
        public string OldGroupIDs { get; set; }
        public string Login { get; set; }
        public int IsVerify { get; set; }
        public string Verify { get; set; }
        public int IsTwoFA { get; set; }
        public string TempName { get; set; }
        public string Note { get; set; }
        public string ReelSourceVideo { get; set; }
        public string MailPass { get; set; }
        public string PrimaryLocation { get; set; }
        public string TimelineSource { get; set; }
        public string CreationDate { get; set; }

        public static Account from(DataRow row)
        {
            long id = Convert.ToInt64(row["id"]);
            string uid = row["uid"].ToString().Trim();
            string name = row["name"] + "";
            string password = row["password"].ToString().Trim();
            string email = row["email"] + "";
            string gender = row["gender"] + "";
            long birthday = (long)row["birthday"];

            string twofa = row["twofa"].ToString().Trim();
            
            string token = row["token"] + "";
            string proxy = row["proxy"] + "";
            string pendingJoin = row["pending_join"] + "";
            string userAgent = row["user_agent"] + "";
            string dob = row["dob"] + "";
            string groupIDs = row["group_ids"] + "";
            string cookie = row["cookie"] + "";

            long updatedAt = (long)row["updated_at"];

            string description = row["description"] + "";
            int status = Convert.ToInt32(row["status"]);
            int isLeave = Convert.ToInt32(row["is_leave"]);
            int storeId = Convert.ToInt32(row["store_id"]);
            var totalGroup = Convert.ToInt32(row["total_group"]);
            var totalFriend = Convert.ToInt32(row["total_friend"]);
            var isShare = Convert.ToInt32(row["is_share"]);
            var isLogin = Convert.ToInt32(row["is_login"]);
            var isActive = Convert.ToInt32(row["is_active"]);
            var totalShareGroup = Convert.ToInt32(row["total_share_group"]);
            var totalShareTimeline = Convert.ToInt32(row["total_share_timeline"]);
            var isTwoFA = Convert.ToInt32(row["is_twofa"]);
            var friendsRequest = 0;

            var isVerify = Convert.ToInt32(row["is_verify"]);
            string tempName = "", pageIds= "", primaryLocation= "";
            int total_page = 0;
            try
            {
                total_page = Convert.ToInt32(row["total_page"]);
            }
            catch (Exception) { }
            try
            {
                pageIds = row["page_ids"].ToString().Trim();
            }
            catch (Exception) { }
            try
            {
                friendsRequest= Convert.ToInt32(row["friends_request"]);
            }
            catch (Exception) { }
            try
            {
                tempName = row["temp_name"].ToString().Trim();
            }
            catch (Exception) { }
            string reel_source_video = "";
            try
            {
                reel_source_video = row["reel_source_video"].ToString().Trim();
            }
            catch (Exception) { }
            string note = "";
            try
            {
                note = row["note"].ToString().Trim();
            }
            catch (Exception) { }
            string mailPass = "";
            try
            {
                mailPass = row["mailPass"].ToString().Trim();
            }
            catch (Exception) { }
            try
            {
                primaryLocation = row["primaryLocation"].ToString().Trim();
            }
            catch (Exception) { }
            string timeline_source = "";
            try
            {
                timeline_source = row["timeline_source"].ToString().Trim();
            }
            catch (Exception) { }
            string creation_date = "";
            try
            {
                creation_date = row["creation_date"].ToString().Trim();
            }
            catch (Exception) { }
            string old_group_ids = "";
            try
            {
                old_group_ids = row["old_group_ids"].ToString().Trim();
            }
            catch (Exception) { }

            var shareTypeId = Convert.ToInt32(row["share_type_id"]);

            var acc = new Account()
            {
                Id = id,
                Name = name,
                Password = password,
                UID = uid,
                Email = email,
                Birthday = birthday,
                Gender = gender,
                ShareTypeId = shareTypeId,
                DOB = dob,
                GroupIDs = groupIDs,

                TempName = tempName,
                OldGroupIDs = old_group_ids,

                FriendsRequest = friendsRequest,
                Cookie = cookie,

                TwoFA = twofa,
                Token = token,
                Proxy = proxy,
                TotalGroup = totalGroup,
                TotalFriend = totalFriend,
                TotalPage = total_page,
                PageIds = pageIds,

                Leave = isLeave,
                Share = isShare,
                IsLogin = isLogin,
                IsActive = isActive,

                TotalShareGroup = totalShareGroup,
                TotalShareTimeline = totalShareTimeline,

                IsTwoFA = isTwoFA,
                IsVerify = isVerify,
                UpdatedAt = updatedAt,

                UserAgent = userAgent,
                Description = description,
                Status = status,
                StoreId = storeId,
                PendingJoin = pendingJoin,

                Note= note,
                MailPass= mailPass,
                PrimaryLocation= primaryLocation,
                ReelSourceVideo= reel_source_video,

                TimelineSource= timeline_source,
                CreationDate= creation_date,
            };

            return acc;
        }
    }    
}
