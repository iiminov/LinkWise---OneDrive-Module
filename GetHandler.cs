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

using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using Newtonsoft.Json;
using System;
using System.Web;

namespace LinkWise.Modules.OneDrive
{
    public class GetHandler : IHttpHandler
    {
        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            HttpResponse response = context.Response;
            response.ContentType = "application/json";
            var written = false;

            //because we're coming into a URL that isn't being handled by DNN we need to figure out the PortalId
            SetPortalId(context.Request);

            context.Response.Write("<H1>This is an HttpHandler Test.</H1>");
            context.Response.Write("<p>Your Browser:</p>");
            context.Response.Write("Type: " + context.Request.Browser.Type + "<br>");
            context.Response.Write("Version: " + context.Request.Browser.Version);

            //if (context.Request.Url.LocalPath.Contains("/svc/time"))
            if (context.Request.Url.AbsolutePath.IndexOf("time") > 0 && written == false)
            {
                response.Write(JsonConvert.SerializeObject(DateTime.Now));
                written = true;
            }

            //get all roles for a portal
            if (context.Request.Url.AbsolutePath.IndexOf("roles") > 0 && written == false)
            {
                response.Write(GetRolesJson(PortalId));
                written = true;
            }

            //get back a listing of 
            if (context.Request.Url.AbsolutePath.IndexOf("users") > 0 && written == false)
            {
                response.Write(GetUsersJson(PortalId));
                written = true;
            }

            //get back a single user
            if (context.Request.Url.AbsolutePath.IndexOf("user") > 0 && written == false)
            {
                //get the username to lookup 
                response.Write(GetUserJson(PortalId, context.Request));
                written = true;
            }

        }

        ///<summary>
        /// Return all security roles for a portal in formatted json string
        /// </summary>
        /// <param name="portalId">portalid</param>
        private static string GetRolesJson(int portalId)
        {
            //get a list of all roles and return it in json
            var rc = new RoleController();

            //getroles returns all roles, we will need to filter roles for this portalid
            var roles = rc.GetRoles(portalId);
            var rolesOutput = string.Empty;
            foreach (RoleInfo r in roles)
            {
                //filter on the current portalid
                if (r.PortalID == PortalId)
                {
                    var output = JsonConvert.SerializeObject(r, Formatting.Indented);
                    rolesOutput += output;
                }
            }
            return rolesOutput;
        }

        ///<summary>
        /// Return all users for a portal in formatted json string
        /// </summary>
        /// <param name="portalId">portalid</param>
        private static string GetUsersJson(int portalId)
        {
            //get a list of all roles and return it in json
            var uc = new UserController();
            var users = UserController.GetUsers(portalId);
            var usersOutput = string.Empty;
            foreach (var t in users)
            {
                var output = JsonConvert.SerializeObject(t, Formatting.Indented);
                usersOutput += output;

            }
            return usersOutput;
        }

        ///<summary>
        /// Return a single user object in formatted json string
        /// </summary>
        /// <param name="portalId">portalid</param>
        /// <param name="userId">userId</param>
        private static string GetUserJson(int portalId, HttpRequest request)
        {
            var url = request.Url.AbsoluteUri;
            var userOutput = string.Empty;
            var usernameLoc = url.IndexOf("/user/") + 6;
            var username = url.Substring(usernameLoc);
            var ui = UserController.GetUserByName(PortalId, username);
            if (ui != null)
            {
                var userId = ui.UserID;

                //get a list of all roles and return it in json
                var uc = new UserController();
                var user = UserController.GetUserById(portalId, userId);

                if (user != null)
                {
                    var output = JsonConvert.SerializeObject(user, Formatting.Indented);
                    userOutput += output;
                }
            }
            return userOutput;
        }

        ///<summary>
        /// Set the portalid, taking the current request and locating which portal is being called based on this request.
        /// </summary>
        /// <param name="request">request</param>
        private void SetPortalId(HttpRequest request)
        {
            string domainName = DotNetNuke.Common.Globals.GetDomainName(request, true);

            string portalAlias = domainName.Substring(0, domainName.IndexOf("/svc"));
            PortalAliasInfo pai = PortalAliasController.Instance.GetPortalAlias(portalAlias);
            if (pai != null)
            {
                PortalId = pai.PortalID;
            }
        }

        public static int PortalId { get; set; }

        #endregion
    }
}