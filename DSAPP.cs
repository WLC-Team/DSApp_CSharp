using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace DSAPP
{
    public partial class DSApp : Form
    {
        private SQLiteConnection sql_con;
        private SQLiteCommand sql_cmd;
        private SQLiteDataAdapter DB;
        private DataSet DS = new DataSet();
        private DataTable DT = new DataTable();
        public const string MatchEmailPattern =
@"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
+ @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?
				[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
+ @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?
				[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
+ @"([a-zA-Z0-9]+[\w-]+\.)+[a-zA-Z]{1}[a-zA-Z0-9-]{1,23})$";

        public DSApp()
        {
            InitializeComponent();
            LoadData();

        }

        private void DSOp_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selitem = DSOp.SelectedItem.ToString();
            switch (selitem)
            {
                case "CreateUser":
                    CreateUserPanel.Show();
                    ShareFilePanel.Hide();
                    SearchUserPanel.Hide();
                    CLearCreateUSerContent();
                    break;
                case "SearchUser":
                    CreateUserPanel.Hide();
                    ShareFilePanel.Hide();
                    SearchUserPanel.Show();
                    CLearSearchUSerContent();
                    break;
                case "ShareFile":
                    CreateUserPanel.Hide();
                    ShareFilePanel.Show();
                    SearchUserPanel.Hide();
                    break;
                default:
                    break;
            }
        }

        private void CreateUser_Click(object sender, EventArgs e)
        {
            string firstNametxt = FirstNameCU.Text;
            string lastNametxt = LastNameCU.Text;
            string emailIDtxt = EmailCU.Text;
            string celltxt = CellCU.Text;
            UInt64 phoneNo = 0;
            if (!string.IsNullOrEmpty(firstNametxt) && !string.IsNullOrEmpty(lastNametxt) && !string.IsNullOrEmpty(emailIDtxt) && !string.IsNullOrEmpty(celltxt))
            {

                if (!string.IsNullOrEmpty(celltxt))
            {
                phoneNo = Convert.ToUInt64(CellCU.Text);
            }
            else
            {
                MessageBox.Show("Please enter phone number!");
            }

                if (IsEmailvalid(emailIDtxt))
                {
                    if (sql_con.State != ConnectionState.Open)
                        sql_con.Open();
                    SQLiteCommand cmd = new SQLiteCommand();
                    cmd.CommandText = "SELECT COUNT(*) FROM users WHERE email=" + "'" + emailIDtxt.Trim() + "'";
                    cmd.Connection = sql_con;
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    if ((count == 0) && IsEmailvalid(emailIDtxt))
                    {
                        string txtSQLQuery = "insert into  users (firstname, lastname, email, phonenum) values ('" + firstNametxt + "','" + lastNametxt + "', '" + emailIDtxt + "', '" + phoneNo + "' )";
                        //sql_con.Open();
                        SQLiteCommand command = new SQLiteCommand(txtSQLQuery, sql_con);
                       int success= command.ExecuteNonQuery();
                        if(success !=0)
                        {
                            MessageBox.Show("User successfully created!");

                        }
                        string systemDrive = System.IO.Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));
                        int index = emailIDtxt.IndexOf('@');
                        string filename = null;
                        if (index > 0)
                        {
                            filename = emailIDtxt.Substring(0, index);
                        }
                        string folderLocation = systemDrive + filename;
                        bool exists = System.IO.Directory.Exists(folderLocation);
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(folderLocation);
                        }
                    }
                    else
                    {
                        MessageBox.Show("User already exists!");
                    }
                }
                else
                {
                    MessageBox.Show("Email Address is invalid!");
                }
            }
            else
            {
                MessageBox.Show("Some fields are missing!");
            }

           }
        private void DSApp_Load(object sender, EventArgs e)
        {
            LoadData();
        }
        private void LoadData()
        {
            SetConnection();
            sql_con.Open();

            string sql = "CREATE TABLE IF NOT EXISTS users (firstname varchar(20), lastname varchar(20), email varchar(40) PRIMARY KEY , phonenum UInt64)";
            sql_cmd = new SQLiteCommand(sql, sql_con);
            sql_cmd.ExecuteNonQuery();
            sql_con.Close();

        }
        private void SetConnection()
        {

            sql_con = new SQLiteConnection("Data Source=DemoT.db;Version=3;New=False;Compress=True;");

        }

        private void SearchUserSU_Click(object sender, EventArgs e)
        {
            string firstNamesrchtxt = FirstNameTBSU.Text;
            string lastNamesrchtxt = LastNameTBSU.Text;
            string emailIDsrchtxt = EmailTBSU.Text;
            string searchquery;
            if (!string.IsNullOrEmpty(firstNamesrchtxt) && !string.IsNullOrEmpty(lastNamesrchtxt))
            {
                searchquery = "select firstname, lastname, email, phonenum from users where firstname = '" + firstNamesrchtxt.Trim() + "' AND lastname = '" + lastNamesrchtxt.Trim() + "'";

            }
            else
            {
                searchquery = "select firstname, lastname, email, phonenum from users where firstname = '" + firstNamesrchtxt.Trim() + "' OR lastname = '" + lastNamesrchtxt.Trim() + "' OR email = '" + emailIDsrchtxt.Trim() + "'";
            }
            DataTable dt = new DataTable();
            if (sql_con.State != ConnectionState.Open)
                sql_con.Open();
            //SQLiteCommand command = new SQLiteCommand(searchquery, sql_con);
            SQLiteDataAdapter da = new SQLiteDataAdapter(searchquery, sql_con);
            da.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                dataGridView1.DataSource = null;
                dataGridView1.Rows.Clear();
                dataGridView1.DataSource = dt;
                dataGridView1.Refresh();
            }
            else
            {
                MessageBox.Show("User not Found!");
            }
            sql_con.Close();


        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int rowIndex = e.RowIndex;
            DataGridViewRow row = dataGridView1.Rows[rowIndex];
            SearchUserPanel.Hide();
            ShareFilePanel.Show();
            SelectedEmailList.Items.Add(row.Cells[2].Value.ToString());

        }
        public static bool IsEmailvalid(string email)
        {
            if (email != null) return Regex.IsMatch(email, MatchEmailPattern);
            else return false;
        }

        private void CreateUserPanel_Enter(object sender, EventArgs e)
        {
            CLearCreateUSerContent();
        }
        void CLearCreateUSerContent()
        {
            FirstNameCU.Clear();
            LastNameCU.Clear();
            EmailCU.Clear();
            CellCU.Clear();
        }
        void CLearSearchUSerContent()
        {
            FirstNameTBSU.Clear();
            LastNameTBSU.Clear();
            EmailTBSU.Clear();
            dataGridView1.DataSource = null;
            dataGridView1.Rows.Clear();
        }
        private void ShareBtn_Click(object sender, EventArgs e)
        {
            if (sendToNetworkFolder.Checked == true)
            {                 
                string emailIDtxt = emailToTextBox.Text;
                if (string.IsNullOrWhiteSpace(emailIDtxt))
                {
                    MessageBox.Show("Please enter emailID/email folder !", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    emailToTextBox.Focus(); //Not working
                }
                else if (string.IsNullOrWhiteSpace(FilePath.Text))
                {
                    MessageBox.Show("Please browse file ! ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    FilePath.Focus(); //Not working
                }
                else
                {
                    if (emailIDtxt.Contains('@'))
                    {
                        string[] strEmailID = emailIDtxt.Split('@');
                        emailIDtxt = strEmailID[strEmailID.Length - 2];
                    }
                    string systemDrive = System.IO.Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));
                    string foldername = emailIDtxt;
                    string folderLocation = systemDrive + foldername;
                    bool exists = System.IO.Directory.Exists(folderLocation);
                    string File_Path = FilePath.Text;
                    string[] strFileParts = File_Path.Split('\\');
                    string fileName = null;
                    
                    if (strFileParts.Length > 0)
                    {
                        fileName = strFileParts[strFileParts.Length - 1];
                    }
                    string sourceFile = File_Path;
                    string destFile = System.IO.Path.Combine(folderLocation, fileName);
                    if (!exists)
                    {
                        MessageBox.Show("Location does not exists !");
                    }
                    else
                    {
                        if (!(System.IO.File.Exists(fileName)))
                        {
                            System.IO.File.Copy(sourceFile, destFile, true);
                            MessageBox.Show(" File copied to folder successfully !");
                            clearTabs();
                        }
                        else
                        {
                            MessageBox.Show(" File already exists with same name !");
                        }
                    }
                }
            }
                
            else if (sendEmailRadioButton.Checked == true) 
            {
                try
                {   
                    //verify email address is valid
                    if (IsEmailvalid(emailToTextBox.Text))
                    {
                        MailMessage mail = new MailMessage();
                        SmtpClient SmtpServer = new SmtpClient("smtp.office365.com");

                        mail.From = new MailAddress("shefali.shakya@wipro.com");
                        mail.To.Add(emailToTextBox.Text);
                        mail.Subject = "Digital Send - Test Mail";
                        mail.Body = "This is for testing SMTP mail \n " + emailBodyText.Text;

                        SmtpServer.Port = 587;
                        SmtpServer.EnableSsl = true;
                        SmtpServer.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;

                        SmtpServer.Credentials = new System.Net.NetworkCredential("sh248947@wipro.com", "14Aug@14");
                        
                        //Add attachments, if any
                        if (!(string.IsNullOrWhiteSpace(FilePath.Text)))
                        {
                            mail.Attachments.Add(new Attachment(FilePath.Text));
                        }

                        //Send email
                        SmtpServer.Send(mail);
                        MessageBox.Show("Email Sent Successfully ! ");
                        clearTabs();
                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid email address !");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            else
            {
                MessageBox.Show("Please choose an option: Send Email or Send to Network Folder !");
            }
        }

        private void clearTabs()
        {
            emailBodyText.Clear();
            emailToTextBox.Clear();
            FilePath.Clear();
            SelectedEmailList.ClearSelected();
            SelectedEmailList.Refresh();
            sendEmailRadioButton.Show();
            sendToNetworkFolder.Show();
            sendEmailRadioButton.Checked = false;
            sendEmailRadioButton.Checked = false;
            selectEmailLabel.Show();
            SelectedEmailList.Show();
            emailBody.Show();
            emailBodyText.Show();

        }

        private void emailToTextBox_TextChanged(object sender, EventArgs e)
        {
            
        }


        private void emailBody_Click(object sender, EventArgs e)
        {

        }

        private void sendEmailRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (sendEmailRadioButton.Checked == true)
            {
                sendToNetworkFolder.Hide();
            }
        }

        private void sendToNetworkFolder_CheckedChanged(object sender, EventArgs e)
        {
            if (sendToNetworkFolder.Checked == true)
            {
                sendEmailRadioButton.Hide();
                emailBody.Hide();
                emailBodyText.Hide();
            }

        }

        private void emailTo_Click(object sender, EventArgs e)
        {

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void ShareFilePanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void FilePath_TextChanged(object sender, EventArgs e)
        {

        }        

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void browseFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            DialogResult dr = ofd.ShowDialog();
            if (dr == DialogResult.OK)
            {
                FilePath.Text = ofd.FileName;
            }
        }

        private void selectEmailLabel_Click_1(object sender, EventArgs e)
        {

        }

        private void SelectedEmailList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(emailToTextBox.Text))
            {
                emailToTextBox.Text = SelectedEmailList.Text;
            }
        }

        private void emailBodyText_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void FilePath_TextChanged_1(object sender, EventArgs e)
        {

        }       

        private void emailBody_Click_1(object sender, EventArgs e)
        {

        }

        private void enterEmailId_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void SearchUserPanel_Enter(object sender, EventArgs e)
        {
            CLearSearchUSerContent();
        }

        private void resetFields_Click(object sender, EventArgs e)
        {
            clearTabs();
        }
    }
}
