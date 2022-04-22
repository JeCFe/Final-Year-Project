using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;


namespace Forms_SSL_Client
{
    public partial class Form1 : Form
    {
        Authenticator auth = new Authenticator();
        public string ServerIP { get { return "81.101.122.94"; } }
        public int ServerPort { get { return 2556; } }

        TcpClient client;
        NetworkStream netStream;
        SslStream sslStream;
        Thread listeningThread;
        StreamReader reader;
        StreamWriter writer;

        //All types of messages that can be recieved
        Message M = new Message();
        HandShakeMessage HSM = new HandShakeMessage();
        LoginInformation LINFO = new LoginInformation();
        RegistrationInformation RINFO = new RegistrationInformation();
        StandardMessage SM = new StandardMessage();
        delegate void DecodeMessageDelegate(string message);

        public Form1()
        {

            InitializeComponent();

            InitaliseServerConnection();
        }
        private void InitaliseServerConnection()
        {
            bool connectionEstablished = false;
            while (!connectionEstablished)
            {
                try
                {
                    client = new TcpClient(ServerIP, ServerPort);
                    netStream = client.GetStream();
                    sslStream = new SslStream(netStream, false, new RemoteCertificateValidationCallback(ValidateCertificate));
                    sslStream.AuthenticateAsClient("SecureChat");
                    reader = new StreamReader(sslStream, Encoding.Unicode);
                    writer = new StreamWriter(sslStream, Encoding.Unicode);
                    listeningThread = new Thread(new ThreadStart(listenForMessages));
                    listeningThread.Start();
                    connectionEstablished = true;
                }
                catch (Exception e) {
                    MessageBox.Show(e.ToString());
                }
            }
        }
        private void listenForMessages()
        {
            bool connected = true;
            while (connected)
            {
                try
                {
                    string recievedMessage = reader.ReadLine();
                    DecodeMessage(recievedMessage);
                }
                catch (Exception)
                {
                    //Set up reconnection processes
                    connected = false;
                    MessageBox.Show("Unknown error with server connection");
                }

            }
        }
        private void CommonCommunications(Message M) //Handles Basic messages
        {
            JavaScriptSerializer Deserializer = new JavaScriptSerializer();
            SM = Deserializer.Deserialize<StandardMessage>(M.message);
            txtMessageBox.AppendText(auth.decryptMessage(SM.message) + Environment.NewLine);
        }
        private void DecodeMessage(string message)
        {
            if (message == null)
            {
                MessageBox.Show("Null message"); //Potential DDOS Close client 
                //Send message to server to log out 
                Application.Exit();
            }
            else
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new DecodeMessageDelegate(DecodeMessage), new object[] { message });
                }
                else
                {
                    string decryptedMessage = message;

                    decryptedMessage = auth.decryptMessage(message);

                    Message M = new Message();
                    JavaScriptSerializer Deserializer = new JavaScriptSerializer();
                    M = Deserializer.Deserialize<Message>(decryptedMessage);
                    switch (M.id)
                    {
                        case "0": //HandShake
                            HandShakeManager(M);
                            break;
                        case "1": //Login
                            LoginManager(M);
                            break;
                        case "2"://Registration
                            RegistrationManager(M);
                            break;
                        case "3": //Standard Message
                            CommonCommunications(M);
                            break;
                        default:
                            break;

                    }
                }
            }
        }
        public static bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; // Allow untrusted certificates.
        }
        private void HandShakeManager(Message M)
        {
            JavaScriptSerializer Deserializer = new JavaScriptSerializer();
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            HSM = Deserializer.Deserialize<HandShakeMessage>(M.message);
            if (HSM.stage == "1")
            {
                HSM.EncryptedAESIV = auth.getEncryptedAesIV(HSM.RSAPublicKey);
                HSM.EncryptedAESKey = auth.getEncryptedAesKey(HSM.RSAPublicKey);
                HSM.test = auth.encryptMessage("Test");
                HSM.stage = "1";
                string HSMmessage = Serializer.Serialize(HSM);
                M.id = "0";
                M.message = HSMmessage;
                string mMessage = Serializer.Serialize(M);
                writer.WriteLine(mMessage);
                writer.Flush();
            }
        }
        private void SendMessage() //Sends basic message
        {
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            string msg = txtMsgToSend.Text;
            // SM.message = auth.encryptMessage(msg);
            SM.message = msg;
            string SMstring = Serializer.Serialize(SM);
            M.id = "3";
            M.message = SMstring;
            string Mstring = Serializer.Serialize(M);
            SendDataToServer(Mstring);
            txtMsgToSend.Text = "";
        }
        private void SendDataToServer(string message)
        {
            writer.WriteLine(auth.encryptMessage(message));
            //writer.WriteLine(message);
            writer.Flush();
        }
        private string PreformPBKDF2Hash(string accountHashedPassword, byte[] salt)
        {
            int iterations = 10000;
            Rfc2898DeriveBytes hash = new Rfc2898DeriveBytes(accountHashedPassword, salt, iterations);
            return Convert.ToBase64String(hash.GetBytes(128));
        }

        #region Login and Logout
        private void LoginBegin() //Begins login steps 
        {
            LINFO.Email = txtUserEmail.Text;
            LINFO.stage = "1";
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            string LINFOmessage = Serializer.Serialize(LINFO);
            M.id = "1";
            M.message = LINFOmessage;
            string message = Serializer.Serialize(M);
            SendDataToServer(message);
        }
        private void LoginManager(Message M)
        {
            JavaScriptSerializer Deserializer = new JavaScriptSerializer();
            LINFO = Deserializer.Deserialize<LoginInformation>(M.message);
            switch (LINFO.stage)
            {
                case "2":
                    LogInSecondStage(LINFO);
                    break;
                case "3":
                    LogInFinalStage(LINFO);
                    break;
                default:
                    break;
            }
        }
        private void LogInSecondStage(LoginInformation LINFO) //Client stage 2
        {
            if (LINFO.confirmation == "true")
            {
                string plainTextPassword = txtPassword.Text;
                string accountPasswordHash = PreformPBKDF2Hash(plainTextPassword, Convert.FromBase64String(LINFO.accountSalt));
                string authenticationToken = PreformPBKDF2Hash(accountPasswordHash, Convert.FromBase64String(LINFO.sessionSalt));
                //Hash with accountSalt
                //Hash with sessionSalt 
                LINFO.passwordHash = authenticationToken;
                LINFO.stage = "2";
                JavaScriptSerializer Serializer = new JavaScriptSerializer();
                string LINFOmessage = Serializer.Serialize(LINFO);
                M.id = "1";
                M.message = LINFOmessage;
                string message = Serializer.Serialize(M);
                SendDataToServer(message);
            }
            else
            {
                MessageBox.Show("NO FOUND EMAIL");
                LoginError.SetError(this.txtUserEmail, "No email found");
            }

        }
        private void LogInFinalStage(LoginInformation LINFO)
        {
            if (LINFO.confirmation == "true")
            {
                ElementVisability(true);
                Form1.ActiveForm.Text = "You are logged in as: " + LINFO.name;
            }
            else
            {
                MessageBox.Show("Unsuccessful");
            }
        }//Client stage 3
        private void Logout()
        {
            ElementVisability(false);
            LoginInformation logout = new LoginInformation();
            Message M = new Message();
            logout.stage = "5";
            M.id = "1";
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            M.message = Serializer.Serialize(logout);
            string MainMessage = Serializer.Serialize(M);
            SendDataToServer(MainMessage);
        }
        #endregion

        #region Registration 
        private void RegistrationBegin() //Request salt from server
        {
            RINFO.stage = "1";
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string RINFOmessage = serializer.Serialize(RINFO);
            M.id = "2";
            M.message = RINFOmessage;
            string message = serializer.Serialize(M);
            SendDataToServer(message);
        }
        private void RegistrationManager(Message M)
        {
            JavaScriptSerializer Deserialiser = new JavaScriptSerializer();
            RINFO = Deserialiser.Deserialize<RegistrationInformation>(M.message);
            switch (RINFO.stage)
            {
                case "1": //Recieve either account salt or confirmation that email has been taken
                    RegistrationSecondStage(RINFO);
                    break;
                case "2": //Recieve confirmation account has been created
                    RegistrationFinalStage(RINFO);
                    break;
            }
        }
        private void RegistrationSecondStage(RegistrationInformation RINFO)
        {
            Message M = new Message();
            if (RINFO.confirmation == "true") //Can continue with registraion
            {
                //Ensures all important information is encrypted
                string hashedPassword = PreformPBKDF2Hash(txtRegPassword.Text, Convert.FromBase64String(RINFO.AccountSalt));
                RINFO.Email = txtRegEmail.Text;
                RINFO.Name = txtUsername.Text;
                RINFO.PasswordHash = hashedPassword;
                RINFO.stage = "2";
                JavaScriptSerializer Serializer = new JavaScriptSerializer();
                string RINFOmessage = Serializer.Serialize(RINFO);
                M.id = "2";
                M.message = RINFOmessage;
                string message = Serializer.Serialize(M);
                SendDataToServer(message);
            }
            else { MessageBox.Show("Unknown error occurred, please try again"); }


        }
        private void RegistrationFinalStage(RegistrationInformation RINFO)
        {
            if (RINFO.confirmation == "true")
            {
                MessageBox.Show("Account added");
            }
            else { MessageBox.Show("Error with registring account"); }
        }
        #endregion

        #region Forms Interaction
        private void ElementVisability(bool showMain)
        {
            Form1 form1 = this;
            if (showMain)
            {
                txtMessageBox.Visible = true;
                txtMsgToSend.Visible = true;
                btnSend.Visible = true;
                btnLogout.Visible = true;

                gBXLogin.Visible = false;
                gBXRegistraion.Visible = false;

                form1.Height = 527;
                form1.Width = 395;
            }
            else
            {
                txtMessageBox.Visible = false;
                txtMsgToSend.Visible = false;
                btnSend.Visible = false;
                btnLogout.Visible = false;

                gBXLogin.Visible = true;
                gBXRegistraion.Visible = true;

                form1.Height = 209;
                form1.Width = 512;
            }
            txtMessageBox.Clear();
            txtMsgToSend.Clear();
        }
        private void btnSend_Click(object sender, EventArgs e)
        {
            //input validation
            SendMessage();
        }
        private void btnlogin_Click(object sender, EventArgs e)
        {
            LoginError.Clear();
            InputValidation valid = new InputValidation();
            if (valid.EmailValidation(txtUserEmail.Text))
            {
                if (valid.PasswordValidation(txtPassword.Text))
                {
                    LoginError.Clear();
                    LoginBegin();
                }
                else { LoginError.SetError(this.txtPassword, "Incorrect password format"); }
            }
            else { LoginError.SetError(this.txtUserEmail, "Incorrect email format"); }
        }
        private void btnReg_Click(object sender, EventArgs e)
        {
            RegError.Clear();
            InputValidation valid = new InputValidation();
            if (valid.NameValidation(txtUsername.Text))
            {
                if (valid.EmailValidation(txtRegEmail.Text))
                {
                    if (valid.PasswordValidation(txtRegPassword.Text))
                    {
                        RegError.Clear();
                        RegistrationBegin();
                    }
                    else { RegError.SetError(this.txtRegPassword, "Incorrect password format"); }
                }
                else { RegError.SetError(this.txtRegEmail, "Incorrect email format"); }
            }
            else { RegError.SetError(this.txtUsername, "Incorrect username format"); }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Logout();
            Application.Exit();
        }
        private void btnLogout_Click(object sender, EventArgs e){ Logout(); }

        #endregion


    }
}
