<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminMemberships.aspx.cs" Inherits="AdminMemberships" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Admin Memberships (ASP.NET)</title>
    <link rel="stylesheet" href="p29.css" />
</head>
<body class="membership-page">
    <form id="form1" runat="server">
        <header class="page-hero">
            <div class="container page-hero-inner">
                <a href="Default.aspx" class="back-link">Back to ASP Home</a>
                <p class="eyebrow">Admin</p>
                <h1>Membership Records</h1>
            </div>
        </header>

        <main class="container form-page-wrap">
            <div class="membership-form form-page-form">
                <asp:GridView ID="gvMemberships" runat="server" AutoGenerateColumns="False" CssClass="asp-grid" GridLines="Both">
                    <Columns>
                        <asp:BoundField DataField="Id" HeaderText="ID" />
                        <asp:BoundField DataField="FullName" HeaderText="Name" />
                        <asp:BoundField DataField="MemberType" HeaderText="Type" />
                        <asp:BoundField DataField="Department" HeaderText="Department" />
                        <asp:BoundField DataField="RollId" HeaderText="Roll/ID" />
                        <asp:BoundField DataField="Batch" HeaderText="Batch" />
                        <asp:BoundField DataField="Mailbox" HeaderText="Mailbox" />
                        <asp:BoundField DataField="PhoneNumber" HeaderText="Phone" />
                        <asp:BoundField DataField="HasPassport" HeaderText="Passport" />
                        <asp:BoundField DataField="SubmittedAtUtc" HeaderText="Submitted (UTC)" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                    </Columns>
                </asp:GridView>
                <asp:Label ID="lblAdminStatus" runat="server" CssClass="form-status" />
            </div>
        </main>
    </form>
</body>
</html>
