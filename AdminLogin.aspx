<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminLogin.aspx.cs" Inherits="AdminLogin" %>
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <title>Admin Login</title>
    <link rel="stylesheet" href="p29.css" />
    <style>
        .login-form { max-width:420px; margin:40px auto; }
        .form-status { margin-top:8px; color: #b22222; }
    </style>
</head>
<body>
    <main class="container">
        <section class="login-form">
            <h1>Admin Login</h1>
            <asp:Label ID="lblStatus" runat="server" CssClass="form-status"></asp:Label>
            <asp:Panel runat="server">
                <div>
                    <label for="txtUser">Username</label>
                    <asp:TextBox ID="txtUser" runat="server" CssClass="form-control" />
                </div>
                <div>
                    <label for="txtPass">Password</label>
                    <asp:TextBox ID="txtPass" runat="server" TextMode="Password" CssClass="form-control" />
                </div>
                <div>
                    <asp:CheckBox ID="chkRemember" runat="server" /> <label for="chkRemember">Remember me</label>
                </div>
                <div style="margin-top:12px;">
                    <asp:Button ID="btnLogin" runat="server" Text="Sign In" OnClick="btnLogin_Click" CssClass="btn btn-primary" />
                </div>
            </asp:Panel>
        </section>
    </main>
</body>
</html>
