<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="View.ascx.cs" Inherits="LinkWise.Modules.OneDrive.View" %>

        <div>
            <p><asp:Literal runat="server" ID="errors" /></p>
            <hr />
            <asp:PlaceHolder runat="server" ID="LoginStatus" Visible="false">
                <p><asp:Literal runat="server" ID="StatusText" /></p>
            </asp:PlaceHolder>

            <asp:PlaceHolder runat="server" ID="SignInButton" Visible="false">
                <p>Sign in using your Office365 account</p>
                <div style="margin-bottom: 10px;">
                    <div>
                        <asp:Button runat="server" OnClick="SignIn" Text="Sign in" />
                    </div>
                </div>
            </asp:PlaceHolder>

            <asp:PlaceHolder runat="server" ID="SignOutButton" Visible="false">
                <p>When finished you can sign out</p>
                <div style="margin-bottom: 10px;">
                    <div>
                        <asp:Button runat="server" OnClick="SignOut" Text="Sign out" />
                    </div>
                </div>
            </asp:PlaceHolder>

            <asp:PlaceHolder runat="server" ID="Files" Visible="false">
                <div style="margin-bottom: 10px;">
                    <div>
                        <asp:Button runat="server" OnClick="GetFiles" Text="My Files" />
                    </div>
                </div>
                <asp:PlaceHolder runat="server" ID="DownloadSuccess" Visible="false">
                    <div style="margin-bottom: 10px;">
                        <p>File downloaded successfully.</p>
                    </div>
                </asp:PlaceHolder>
                <asp:PlaceHolder runat="server" ID="DownloadFail" Visible="false">
                    <div style="margin-bottom: 10px;">
                        <asp:Label runat="server" ID="failMessage" />
                    </div>
                </asp:PlaceHolder>
                <div style="margin-bottom: 10px;">
                    <asp:Repeater runat="server" ID="MyFiles">
                        <HeaderTemplate>
                            <table>
                                <thead>
                                    <tr>
                                        <th>#</th>
                                        <th>Name</th>
                                        <th>Size</th>
                                        <th>Type</th>
                                        <th>URL</th>
                                        <th>Actions</th>
                                    </tr>
                                </thead>
                                <tbody>
                        </HeaderTemplate>
                        <ItemTemplate>
                                    <tr>
                                        <td><%# Eval("ItemID") %></td>
                                        <td><%# Eval("ItemName") %></td>
                                        <td><%# Eval("ItemSize") %> bytes</td>
                                        <td><%# Eval("ItemType") %></td>
                                        <td><asp:hyperlink runat="server" Text="Open in OneDrive" NavigateUrl='<%# Eval("ItemURL") %>' Target="_blank" /></td>
                                        <td><asp:Button runat="server" Text="Open" Visible='<%# Convert.ToBoolean(Eval("ItemType").ToString() == "Folder") %>' CommandArgument='<%# Eval("ItemID") %>' CommandName="Open" OnCommand="CommandBtn_Click" /><asp:Button runat="server" Text="Download" Visible='<%# Convert.ToBoolean(Eval("ItemType").ToString() == "File") %>' CommandArgument='<%# Eval("ItemID") %>' CommandName="Download" OnCommand="CommandBtn_Click" /></td>
                                    </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                                </tbody>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                </div>
            </asp:PlaceHolder>
        </div>
