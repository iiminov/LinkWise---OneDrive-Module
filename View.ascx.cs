/*
' Copyright (c) 2015  LinkWise
'  All rights reserved.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' 
*/

using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LinkWise.Modules.OneDrive
{
    public partial class View : OneDriveModuleBase//, System.Web.UI.Page
    {
        private FileOperations _fileOperations = new FileOperations();
        private ApplicationDbContext _db = new ApplicationDbContext();
        private string cacheKey = "#O365:pid=" + PortalSettings.Current.PortalId.ToString() + ":uid=" + PortalSettings.Current.UserId.ToString();

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                errors.Text = string.Format("cache key: {0}", cacheKey);
                errors.Text += string.Format("<br>{0} {1} in cache", cacheKey, _db.GetUserTokenCacheList(cacheKey) != null ? "exists" : "does not exist");
                if (!IsPostBack)
                {
                    //if (User.Identity.IsAuthenticated || Request.IsAuthenticated)
                    if (_db.GetUserTokenCacheList(cacheKey) != null)
                    {
                        StatusText.Text = string.Format("You're signed in as {0}!!", /*User.Identity.GetUserName()*/PortalSettings.Current.UserInfo.DisplayName);
                        LoginStatus.Visible = true;
                        SignOutButton.Visible = true;
                        Files.Visible = true;
                    }
                    else
                    {
                        SignInButton.Visible = true;
                    }
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void SignIn(object sender, EventArgs e)
        {
            errors.Text += "<br>Atempting user sign in";
            //if (!Request.IsAuthenticated)
            if (_db.GetUserTokenCacheList(cacheKey) == null)
            {
                errors.Text += "<br>Redirecting to sign in page";
                //return usl is configured as http://devsite1.me/OneDrive
                HttpContext.Current.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = "" }, OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }

        protected void SignOut(object sender, EventArgs e)
        {
            //return usl is configured as http://devsite1.me/OneDrive
            HttpContext.Current.GetOwinContext().Authentication.SignOut(new AuthenticationProperties { RedirectUri = "" }, OpenIdConnectAuthenticationDefaults.AuthenticationType, CookieAuthenticationDefaults.AuthenticationType);
        }

        protected async void GetFiles(object sender, EventArgs e)
        {
            var files = await _fileOperations.GetMyFilesAsync();
            MyFiles.DataSource = files;
            MyFiles.DataBind();
        }

        protected async void CommandBtn_Click(Object sender, CommandEventArgs e)
        {
            switch ((String)e.CommandName)
            {
                case "Download":
                    var success = await _fileOperations.DownloadFileAsync((String)e.CommandArgument);

                    if (success)
                    {
                        DownloadSuccess.Visible = true;
                    }
                    else
                    {
                        failMessage.Text = "Unable to identify file. FileID cannot be an empty string.";
                        DownloadFail.Visible = true;
                    }
                    break;

                case "Open":
                    var files = await _fileOperations.GetMyFilesAsync((String)e.CommandArgument);
                    MyFiles.DataSource = files;
                    MyFiles.DataBind();
                    break;
            }
        }
    }
}