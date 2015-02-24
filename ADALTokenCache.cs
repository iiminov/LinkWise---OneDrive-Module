using DotNetNuke.Entities.Portals;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Data.Entity;
using System.Linq;

namespace LinkWise.Modules.OneDrive
{
    public class ADALTokenCache : TokenCache
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        string User;
        UserTokenCache Cache;
        string cacheKey = "#O365:pid=" + PortalSettings.Current.PortalId.ToString() + ":uid=" + PortalSettings.Current.UserId.ToString();

        // constructor
        public ADALTokenCache(string user)
        {
            // associate the cache to the current user of the web app
            User = user;
            this.AfterAccess = AfterAccessNotification;
            this.BeforeAccess = BeforeAccessNotification;
            this.BeforeWrite = BeforeWriteNotification;

            // look up the entry in the DB
            //Cache = db.UserTokenCacheList.FirstOrDefault(c => c.webUserUniqueId == User);
            Cache = db.GetUserTokenCacheList(PortalSettings.Current.UserId.ToString());
            // place the entry in memory
            this.Deserialize((Cache == null) ? null : Cache.cacheBits);
        }

        // clean up the DB
        public override void Clear()
        {
            base.Clear();
            db.RemoveUserTokenCacheList(cacheKey);

            //foreach (var cacheEntry in db.UserTokenCacheList)
                //db.UserTokenCacheList.Remove(cacheEntry);
            //db.SaveChanges();
        }

        // Notification raised before ADAL accesses the cache.
        // This is your chance to update the in-memory copy from the DB, if the in-memory version is stale
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            if (Cache == null)
            {
                // first time access
                //Cache = db.UserTokenCacheList.FirstOrDefault(c => c.webUserUniqueId == User);
                Cache = db.GetUserTokenCacheList(cacheKey);
            }
            else
            {   // retrieve last write from the DB
                //var status = from e in db.UserTokenCacheList
                //             where (e.webUserUniqueId == User)
                //             select new
                //             {
                //                 LastWrite = e.LastWrite
                //             };
                var status = db.GetUserTokenCacheList(cacheKey);
                // if the in-memory copy is older than the persistent copy
                //if (status.First().LastWrite > Cache.LastWrite)
                if (status.LastWrite > Cache.LastWrite)
                //// read from from storage, update in-memory copy
                {
                    //Cache = db.UserTokenCacheList.FirstOrDefault(c => c.webUserUniqueId == User);
                    Cache = db.GetUserTokenCacheList(cacheKey);
                }
            }
            this.Deserialize((Cache == null) ? null : Cache.cacheBits);
        }

        // Notification raised after ADAL accessed the cache.
        // If the HasStateChanged flag is set, ADAL changed the content of the cache
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if state changed
            if (this.HasStateChanged)
            {
                Cache = new UserTokenCache
                {
                    UserTokenCacheId = PortalSettings.Current.UserId,
                    webUserUniqueId = User,
                    cacheBits = this.Serialize(),
                    LastWrite = DateTime.Now
                };
                //// update the DB and the lastwrite                
                //db.Entry(Cache).State = Cache.UserTokenCacheId == 0 ? EntityState.Added : EntityState.Modified;
                //db.SaveChanges();
                db.SetUserTokenCacheList(cacheKey, Cache);
                this.HasStateChanged = false;
            }
        }

        void BeforeWriteNotification(TokenCacheNotificationArgs args)
        {
            // if you want to ensure that no concurrent write take place, use this notification to place a lock on the entry
        }
    }
}