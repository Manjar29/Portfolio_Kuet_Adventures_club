using System;
using System.Configuration;
using System.Data.SqlClient;

public partial class Membership : System.Web.UI.Page
{
    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtFullName.Text) ||
            string.IsNullOrWhiteSpace(txtDepartment.Text) ||
            string.IsNullOrWhiteSpace(txtRollId.Text) ||
            string.IsNullOrWhiteSpace(txtBatch.Text) ||
            string.IsNullOrWhiteSpace(txtMailbox.Text) ||
            string.IsNullOrWhiteSpace(txtPhoneNumber.Text) ||
            string.IsNullOrWhiteSpace(txtMessage.Text))
        {
            lblStatus.Text = "Please fill all fields.";
            lblStatus.ForeColor = System.Drawing.Color.Firebrick;
            return;
        }

        var connectionString = ConfigurationManager.ConnectionStrings["KuetDb"]?.ConnectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            lblStatus.Text = "Database connection is missing in web.config.";
            lblStatus.ForeColor = System.Drawing.Color.Firebrick;
            return;
        }

        const string sql = @"INSERT INTO MembershipApplications
            (FullName, MemberType, Department, RollId, Batch, Mailbox, PhoneNumber, HasPassport, Message, SubmittedAtUtc)
            VALUES
            (@FullName, @MemberType, @Department, @RollId, @Batch, @Mailbox, @PhoneNumber, @HasPassport, @Message, SYSUTCDATETIME());";

        try
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@FullName", txtFullName.Text.Trim());
                command.Parameters.AddWithValue("@MemberType", ddlMemberType.SelectedValue.Trim());
                command.Parameters.AddWithValue("@Department", txtDepartment.Text.Trim());
                command.Parameters.AddWithValue("@RollId", txtRollId.Text.Trim());
                command.Parameters.AddWithValue("@Batch", txtBatch.Text.Trim());
                command.Parameters.AddWithValue("@Mailbox", txtMailbox.Text.Trim());
                command.Parameters.AddWithValue("@PhoneNumber", txtPhoneNumber.Text.Trim());
                command.Parameters.AddWithValue("@HasPassport", ddlPassport.SelectedValue == "1");
                command.Parameters.AddWithValue("@Message", txtMessage.Text.Trim());

                connection.Open();
                command.ExecuteNonQuery();
            }

            lblStatus.Text = "Membership info saved successfully.";
            lblStatus.ForeColor = System.Drawing.Color.ForestGreen;

            txtFullName.Text = string.Empty;
            txtDepartment.Text = string.Empty;
            txtRollId.Text = string.Empty;
            txtBatch.Text = string.Empty;
            txtMailbox.Text = string.Empty;
            txtPhoneNumber.Text = string.Empty;
            txtMessage.Text = string.Empty;
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Save failed: " + ex.Message;
            lblStatus.ForeColor = System.Drawing.Color.Firebrick;
        }
    }
}
