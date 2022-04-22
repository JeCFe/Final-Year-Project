namespace Forms_SSL_Client
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.txtMessageBox = new System.Windows.Forms.TextBox();
            this.txtMsgToSend = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.txtUserEmail = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.btnlogin = new System.Windows.Forms.Button();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.txtRegEmail = new System.Windows.Forms.TextBox();
            this.txtRegPassword = new System.Windows.Forms.TextBox();
            this.btnReg = new System.Windows.Forms.Button();
            this.gBXLogin = new System.Windows.Forms.GroupBox();
            this.lblLoginPassword = new System.Windows.Forms.Label();
            this.lblLoginEmail = new System.Windows.Forms.Label();
            this.gBXRegistraion = new System.Windows.Forms.GroupBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.lblEmail = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.RegError = new System.Windows.Forms.ErrorProvider(this.components);
            this.LoginError = new System.Windows.Forms.ErrorProvider(this.components);
            this.btnLogout = new System.Windows.Forms.Button();
            this.gBXLogin.SuspendLayout();
            this.gBXRegistraion.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RegError)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LoginError)).BeginInit();
            this.SuspendLayout();
            // 
            // txtMessageBox
            // 
            this.txtMessageBox.Location = new System.Drawing.Point(12, 12);
            this.txtMessageBox.Multiline = true;
            this.txtMessageBox.Name = "txtMessageBox";
            this.txtMessageBox.ReadOnly = true;
            this.txtMessageBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMessageBox.Size = new System.Drawing.Size(356, 358);
            this.txtMessageBox.TabIndex = 0;
            this.txtMessageBox.Visible = false;
            // 
            // txtMsgToSend
            // 
            this.txtMsgToSend.Location = new System.Drawing.Point(12, 377);
            this.txtMsgToSend.Multiline = true;
            this.txtMsgToSend.Name = "txtMsgToSend";
            this.txtMsgToSend.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMsgToSend.Size = new System.Drawing.Size(356, 60);
            this.txtMsgToSend.TabIndex = 1;
            this.txtMsgToSend.Visible = false;
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(12, 443);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(283, 39);
            this.btnSend.TabIndex = 2;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Visible = false;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // txtUserEmail
            // 
            this.txtUserEmail.Location = new System.Drawing.Point(70, 19);
            this.txtUserEmail.Name = "txtUserEmail";
            this.txtUserEmail.Size = new System.Drawing.Size(100, 20);
            this.txtUserEmail.TabIndex = 3;
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(70, 45);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(100, 20);
            this.txtPassword.TabIndex = 4;
            // 
            // btnlogin
            // 
            this.btnlogin.Location = new System.Drawing.Point(70, 71);
            this.btnlogin.Name = "btnlogin";
            this.btnlogin.Size = new System.Drawing.Size(100, 23);
            this.btnlogin.TabIndex = 5;
            this.btnlogin.Text = "Login";
            this.btnlogin.UseVisualStyleBackColor = true;
            this.btnlogin.Click += new System.EventHandler(this.btnlogin_Click);
            // 
            // txtUsername
            // 
            this.txtUsername.Location = new System.Drawing.Point(75, 15);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(100, 20);
            this.txtUsername.TabIndex = 6;
            // 
            // txtRegEmail
            // 
            this.txtRegEmail.Location = new System.Drawing.Point(75, 44);
            this.txtRegEmail.Name = "txtRegEmail";
            this.txtRegEmail.Size = new System.Drawing.Size(100, 20);
            this.txtRegEmail.TabIndex = 7;
            // 
            // txtRegPassword
            // 
            this.txtRegPassword.Location = new System.Drawing.Point(75, 74);
            this.txtRegPassword.Name = "txtRegPassword";
            this.txtRegPassword.PasswordChar = '*';
            this.txtRegPassword.Size = new System.Drawing.Size(100, 20);
            this.txtRegPassword.TabIndex = 8;
            // 
            // btnReg
            // 
            this.btnReg.Location = new System.Drawing.Point(75, 100);
            this.btnReg.Name = "btnReg";
            this.btnReg.Size = new System.Drawing.Size(100, 23);
            this.btnReg.TabIndex = 9;
            this.btnReg.Text = "Register";
            this.btnReg.UseVisualStyleBackColor = true;
            this.btnReg.Click += new System.EventHandler(this.btnReg_Click);
            // 
            // gBXLogin
            // 
            this.gBXLogin.Controls.Add(this.lblLoginPassword);
            this.gBXLogin.Controls.Add(this.lblLoginEmail);
            this.gBXLogin.Controls.Add(this.txtUserEmail);
            this.gBXLogin.Controls.Add(this.txtPassword);
            this.gBXLogin.Controls.Add(this.btnlogin);
            this.gBXLogin.Location = new System.Drawing.Point(27, 29);
            this.gBXLogin.Name = "gBXLogin";
            this.gBXLogin.Size = new System.Drawing.Size(200, 100);
            this.gBXLogin.TabIndex = 10;
            this.gBXLogin.TabStop = false;
            this.gBXLogin.Text = "Login Controls";
            // 
            // lblLoginPassword
            // 
            this.lblLoginPassword.AutoSize = true;
            this.lblLoginPassword.Location = new System.Drawing.Point(11, 48);
            this.lblLoginPassword.Name = "lblLoginPassword";
            this.lblLoginPassword.Size = new System.Drawing.Size(53, 13);
            this.lblLoginPassword.TabIndex = 7;
            this.lblLoginPassword.Text = "Password";
            // 
            // lblLoginEmail
            // 
            this.lblLoginEmail.AutoSize = true;
            this.lblLoginEmail.Location = new System.Drawing.Point(11, 22);
            this.lblLoginEmail.Name = "lblLoginEmail";
            this.lblLoginEmail.Size = new System.Drawing.Size(32, 13);
            this.lblLoginEmail.TabIndex = 6;
            this.lblLoginEmail.Text = "Email";
            // 
            // gBXRegistraion
            // 
            this.gBXRegistraion.Controls.Add(this.lblPassword);
            this.gBXRegistraion.Controls.Add(this.lblEmail);
            this.gBXRegistraion.Controls.Add(this.lblName);
            this.gBXRegistraion.Controls.Add(this.txtUsername);
            this.gBXRegistraion.Controls.Add(this.txtRegEmail);
            this.gBXRegistraion.Controls.Add(this.btnReg);
            this.gBXRegistraion.Controls.Add(this.txtRegPassword);
            this.gBXRegistraion.Location = new System.Drawing.Point(269, 29);
            this.gBXRegistraion.Name = "gBXRegistraion";
            this.gBXRegistraion.Size = new System.Drawing.Size(200, 129);
            this.gBXRegistraion.TabIndex = 11;
            this.gBXRegistraion.TabStop = false;
            this.gBXRegistraion.Text = "Registration Controls";
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(6, 74);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(53, 13);
            this.lblPassword.TabIndex = 11;
            this.lblPassword.Text = "Password";
            // 
            // lblEmail
            // 
            this.lblEmail.AutoSize = true;
            this.lblEmail.Location = new System.Drawing.Point(7, 18);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new System.Drawing.Size(55, 13);
            this.lblEmail.TabIndex = 10;
            this.lblEmail.Text = "Username";
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(6, 44);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(32, 13);
            this.lblName.TabIndex = 8;
            this.lblName.Text = "Email";
            // 
            // RegError
            // 
            this.RegError.ContainerControl = this;
            // 
            // LoginError
            // 
            this.LoginError.ContainerControl = this;
            // 
            // btnLogout
            // 
            this.btnLogout.Location = new System.Drawing.Point(301, 443);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(67, 39);
            this.btnLogout.TabIndex = 12;
            this.btnLogout.Text = "Logout";
            this.btnLogout.UseVisualStyleBackColor = true;
            this.btnLogout.Visible = false;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(496, 169);
            this.Controls.Add(this.btnLogout);
            this.Controls.Add(this.gBXRegistraion);
            this.Controls.Add(this.gBXLogin);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.txtMsgToSend);
            this.Controls.Add(this.txtMessageBox);
            this.Name = "Form1";
            this.Text = "Chat Client";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.gBXLogin.ResumeLayout(false);
            this.gBXLogin.PerformLayout();
            this.gBXRegistraion.ResumeLayout(false);
            this.gBXRegistraion.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RegError)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LoginError)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtMessageBox;
        private System.Windows.Forms.TextBox txtMsgToSend;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TextBox txtUserEmail;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnlogin;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.TextBox txtRegEmail;
        private System.Windows.Forms.TextBox txtRegPassword;
        private System.Windows.Forms.Button btnReg;
        private System.Windows.Forms.GroupBox gBXLogin;
        private System.Windows.Forms.Label lblLoginEmail;
        private System.Windows.Forms.Label lblLoginPassword;
        private System.Windows.Forms.GroupBox gBXRegistraion;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.ErrorProvider RegError;
        private System.Windows.Forms.ErrorProvider LoginError;
        private System.Windows.Forms.Button btnLogout;
    }
}

