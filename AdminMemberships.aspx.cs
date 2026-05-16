using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

public partial class AdminMemberships : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            LoadMemberships();
        }
    }

    private void LoadMemberships()
    {
        var connectionString = ConfigurationManager.ConnectionStrings["KuetDb"]?.ConnectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            lblAdminStatus.Text = "Database connection is missing in web.config.";
            lblAdminStatus.ForeColor = System.Drawing.Color.Firebrick;
            return;
        }

        const string sql = @"SELECT Id, FullName, MemberType, Department, RollId, Batch, Mailbox, PhoneNumber, HasPassport, SubmittedAtUtc
                             FROM MembershipApplications
                             ORDER BY SubmittedAtUtc DESC;";

        try
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(sql, connection))
            using (var adapter = new SqlDataAdapter(command))
            {
                var table = new DataTable();
                adapter.Fill(table);
                gvMemberships.DataSource = table;
                gvMemberships.DataBind();
            }

            lblAdminStatus.Text = "Loaded membership records.";
            lblAdminStatus.ForeColor = System.Drawing.Color.ForestGreen;
        }
        catch (Exception ex)
        {
            lblAdminStatus.Text = "Load failed: " + ex.Message;
            lblAdminStatus.ForeColor = System.Drawing.Color.Firebrick;
        }
    }
}
