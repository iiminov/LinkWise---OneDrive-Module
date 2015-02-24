using DotNetNuke.Common.Utilities;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace LinkWise.Modules.OneDrive
{
    // Has been mdified to store User Tokens in DNN Cache
    public class ApplicationDbContext
    {
        public UserTokenCache GetUserTokenCacheList(string _key)
        {
            return (UserTokenCache)DataCache.GetCache(_key);
        }

        public void SetUserTokenCacheList(string _key, UserTokenCache _object)
        {
            DataCache.SetCache(_key, _object);
        }

        public void RemoveUserTokenCacheList(string _key)
        {
            DataCache.RemoveCache(_key);
        }
    }

    public class UserTokenCache
    {
        [Key]
        public int UserTokenCacheId { get; set; }
        public string webUserUniqueId { get; set; }
        public byte[] cacheBits { get; set; }
        public DateTime LastWrite { get; set; }
    }
}