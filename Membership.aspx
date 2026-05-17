<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Membership.aspx.cs" Inherits="Membership" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Membership Form (ASP.NET)</title>
    <link rel="stylesheet" href="p29.css" />
</head>
<body class="membership-page">
    <form id="form1" runat="server">
        <header class="page-hero">
            <div class="container page-hero-inner">
                <a href="Default.aspx" class="back-link">Back to ASP Home</a>
                <p class="eyebrow">ASP.NET Membership</p>
                <h1>Save Membership Info to Database</h1>
            </div>
        </header>

        <main class="container form-page-wrap">
            <div class="membership-form form-page-form">
                <div class="form-grid">
                    <div class="form-row">
                        <label for="txtFullName">Full Name</label>
                        <asp:TextBox ID="txtFullName" runat="server" CssClass="asp-input" />
                    </div>
                    <div class="form-row">
                        <label for="ddlMemberType">Member Type</label>
                        <asp:DropDownList ID="ddlMemberType" runat="server" CssClass="asp-input">
                            <asp:ListItem Text="Student" Value="Student" />
                            <asp:ListItem Text="Alumni" Value="Alumni" />
                        </asp:DropDownList>
                    </div>
                    <div class="form-row">
                        <label for="txtDepartment">Department</label>
                        <asp:TextBox ID="txtDepartment" runat="server" CssClass="asp-input" />
                    </div>
                    <div class="form-row">
                        <label for="txtRollId">Roll / ID</label>
                        <asp:TextBox ID="txtRollId" runat="server" CssClass="asp-input" />
                    </div>
                    <div class="form-row">
                        <label for="txtBatch">Batch</label>
                        <asp:TextBox ID="txtBatch" runat="server" CssClass="asp-input" />
                    </div>
                    <div class="form-row">
                        <label for="txtMailbox">Mailbox</label>
                        <asp:TextBox ID="txtMailbox" runat="server" CssClass="asp-input" />
                    </div>
                    <div class="form-row">
                        <label for="txtPhoneNumber">Phone Number</label>
                        <asp:TextBox ID="txtPhoneNumber" runat="server" CssClass="asp-input" />
                    </div>
                    <div class="form-row">
                        <label for="ddlPassport">Has Passport</label>
                        <asp:DropDownList ID="ddlPassport" runat="server" CssClass="asp-input">
                            <asp:ListItem Text="Yes" Value="1" />
                            <asp:ListItem Text="No" Value="0" />
                        </asp:DropDownList>
                    </div>
                    <div class="form-row full-width">
                        <label for="txtMessage">Message</label>
                        <asp:TextBox ID="txtMessage" runat="server" TextMode="MultiLine" Rows="4" CssClass="asp-input" />
                    </div>
                </div>

                <div class="form-actions">
                    <asp:Button ID="btnSubmit" runat="server" CssClass="btn btn-primary form-submit" Text="Submit Membership" OnClick="btnSubmit_Click" />
                    <a href="p29.html" class="btn btn-ghost form-cancel">Back to Homepage</a>
                </div>

                <asp:Label ID="lblStatus" runat="server" CssClass="form-status" />
            </div>
        </main>
    </form>
</body>
</html>
