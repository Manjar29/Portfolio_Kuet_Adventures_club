using System;
using System.Configuration;

public partial class AdminLogin : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            // If already authenticated, redirect back
            if (Session["IsAdmin"] is bool isAdmin && isAdmin)
            {
                var returnUrl = Request.QueryString["returnUrl"] ?? "AdminMemberships.aspx";
                Response.Redirect(returnUrl);
            }
        }
    }

    protected void btnLogin_Click(object sender, EventArgs e)
    {
        var user = txtUser.Text?.Trim();
        var pass = txtPass.Text ?? string.Empty;

        // Read credentials from web.config appSettings if present
        var cfgUser = ConfigurationManager.AppSettings["AdminUser"] ?? "admin";
        var cfgPass = ConfigurationManager.AppSettings["AdminPass"] ?? "password";

        if (string.Equals(user, cfgUser, StringComparison.Ordinal) && pass == cfgPass)
        {
            Session["IsAdmin"] = true;

            var cookie = new System.Web.HttpCookie("IsAdmin", "1") { HttpOnly = true };
            if (chkRemember.Checked)
            {
                cookie.Expires = DateTime.Now.AddDays(7);
            }
            Response.Cookies.Add(cookie);

            var returnUrl = Request.QueryString["returnUrl"];
            if (!string.IsNullOrEmpty(returnUrl))
                Response.Redirect(returnUrl);
            else
                Response.Redirect("AdminMemberships.aspx");
        }
        else
        {
            lblStatus.Text = "Invalid credentials.";
        }
    }
}
