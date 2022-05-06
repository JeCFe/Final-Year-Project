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
using log4net;

[assembly: log4net.Config.XmlConfigurator(Watch = true)] //Watch ensure if the config is changed then the program will react in realtime

namespace Forms_SSL_Client
{
    public partial class Form1 : Form
    {
        //Static assigned SERVER IP and SERVER PORT
        //This value need to be changed to the IP and Port of the server machine 
        //These values can also be changed if theres no server to connect too
        public string ServerIP = "81.101.122.94";
        public int ServerPort = 2556;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Main Program");

        Authenticator auth = new Authenticator();

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

        private bool connected = false; //this variable handles if the client is connected to the server

        public Form1()
        {
            //Loads all elemenets for the GUI
            InitializeComponent();
            InitaliseServerConnection();
        }

        //This function is used to initalise the connection to the server
        //Establishes the TCP and SSL connections for the secure connections 
        //Starts the listening thread to listen for messages from the server
        //If the server connection times out then a window to allow a new IP and port open
        private void InitaliseServerConnection()
        {
            bool connectionEstablished = false;

            while (!connectionEstablished)
            {
                try
                {
                    MessageBox.Show("Press OK to begin connecting to the Server\nPlease wait for either login screen or IP/PORT screen");
                    client = new TcpClient(ServerIP, ServerPort);
                    netStream = client.GetStream();
                    sslStream = new SslStream(netStream, false, new RemoteCertificateValidationCallback(ValidateCertificate));
                    sslStream.AuthenticateAsClient("SecureChat");
                    reader = new StreamReader(sslStream, Encoding.Unicode);
                    writer = new StreamWriter(sslStream, Encoding.Unicode);
                    connected = true;
                    log.Info("Connected to server");
                    listeningThread = new Thread(new ThreadStart(ListenForMessages));
                    listeningThread.Start();
                    connectionEstablished = true;
                    ElementVisability(false);
                }
                catch (Exception e) {
                    log.Error(e.ToString());
                    connectionEstablished = true;
                    TakeNewIPAndPort();
                }
            }
        }

        //Alerts the usert there was an error connecting the the server
        //Opens the window with the IP and Port number to connect to a different server
        private void TakeNewIPAndPort()
        {
            MessageBox.Show("Error connecting to server, please enter IP and Port");
            ElementVisability(false, 1);
        }

        //Infinate look to listen to messages 
        //When message is recieved to send that message 
        private void ListenForMessages()
        {
            while (connected)
            {
                try
                {
                    //When message is recieved pass onto DecodeMessage
                    string recievedMessage = reader.ReadLine();
                    log.Info("Message recieved");
                    DecodeMessage(recievedMessage);
                }
                catch (Exception)
                {
                    //If clinet is disconnected by server closing 
                    //Closes the lisening thread
                    //Logs and displays the event
                    connected = false;
                    listeningThread.Abort();
                    string logMessage = "Unknown error with server connection";
                    MessageBox.Show(logMessage);
                    log.Fatal(logMessage);
                }

            }
        }

        //This function is used whenever the server recieves a message from the server
        //A check is first if there is a null message 
        //  This can happen if theres an error with the server or a message or a ddos attack
        //  If there is a null message then a log is made and the appliation is logout and closed
        //A delegate is needed as the message is coming from another thread (listening thread)
        //The message is deserialised and a switch case is used to determine the approperiate function for the incoming message
        private void DecodeMessage(string message)
        {
            if (message == null)
            {
                string logMessage = "Recieved Null Message";
                log.Error(logMessage);
                MessageBox.Show(logMessage); //Potential DDOS Close client 
                Logout();
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

                    decryptedMessage = auth.DecryptMessage(message);

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

        //This function is used when a standard message is recieved
        //Decrypts the recieved message and appends to the message box object on GUI
        private void CommonCommunications(Message M) //Handles Basic messages
        {
            JavaScriptSerializer Deserializer = new JavaScriptSerializer();
            SM = Deserializer.Deserialize<StandardMessage>(M.message);
            txtMessageBox.AppendText(auth.DecryptMessage(SM.message) + Environment.NewLine);
        }

        //This functions should be used to check validity of SSL certificate
        //Due this project only having a self signed this function is more of a placer for when a CA certificate is used
        public static bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; // Allow untrusted certificates.
        }

        //This function handles the encryption handsake
        //Recieving the RSA encryption key 
        //Decrypting the AES key and IV key and then serialising and sending this back to the server
        private void HandShakeManager(Message M)
        {
            JavaScriptSerializer Deserializer = new JavaScriptSerializer();
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            HSM = Deserializer.Deserialize<HandShakeMessage>(M.message);
            if (HSM.stage == "1")
            {
                HSM.EncryptedAESIV = auth.GetEncryptedAesIV(HSM.RSAPublicKey);
                HSM.EncryptedAESKey = auth.GetEncryptedAesKey(HSM.RSAPublicKey);
                HSM.test = auth.EncryptMessage("Test");
                HSM.stage = "1";
                string HSMmessage = Serializer.Serialize(HSM);
                M.id = "0";
                M.message = HSMmessage;
                string mMessage = Serializer.Serialize(M);
                writer.WriteLine(mMessage);
                writer.Flush();
            }
        }

        //This function takes the message the user wants to send another user and sends this to the server
        private void SendMessage() 
        {
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            string msg = txtMsgToSend.Text;
            SM.message = msg;
            string SMstring = Serializer.Serialize(SM);
            M.id = "3";
            M.message = SMstring;
            string Mstring = Serializer.Serialize(M);
            SendDataToServer(Mstring);
            txtMsgToSend.Text = "";
        }

        //This function sends the message to be encrypted then writes the message on the stream to the server
        //If any errors are thrown this is logged
        private void SendDataToServer(string message)
        {
            try
            {
                writer.WriteLine(auth.EncryptMessage(message));
                writer.Flush();
                log.Info("Message sent to server");
            }
            catch (Exception)
            {
                log.Error("Error sending message to server");
            }
        }

        //This function is exact to that on the server
        //Uses PKBDF2 to hash a string, in this case a password
        //Returns the string value of the hashing
        private string PreformPBKDF2Hash(string password, byte[] salt)
        {
            int iterations = 10000;
            Rfc2898DeriveBytes hash = new Rfc2898DeriveBytes(password, salt, iterations);
            return Convert.ToBase64String(hash.GetBytes(128));
        }

        #region Login and Logout
        //This function handles the stages of login
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
                case "5":
                    ServerLogout();
                    break;
                default:
                    break;
            }
        }
        //This function send the server the email address entered by the user
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

        //If an account exists then this function will hash the entered password and send this to the server  
        //If an account doesnt exists then an error provider will be used to notify this
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
                log.Info("No email found");
                LoginError.SetError(this.txtUserEmail, "No email found");
            }

        }

        //If the server has successfully loged the used in then the main window is shown and the title is updated
        //Else a log event is made and the user is notified via error provider
        private void LogInFinalStage(LoginInformation LINFO)
        {
            if (LINFO.confirmation == "true")
            {
                ElementVisability(true);
                log.Info("User logged in");
                Form1.ActiveForm.Text = "You are logged in as: " + LINFO.name;
            }
            else
            {
                log.Info("Issue logging user in");
                LoginError.SetError(txtPassword, "Invalid password or this account is already logged in");
            }
        }//Client stage 3

        //When the client logs out, this can happen via button or closing the program
        //If the user closes the application the logout.name is set to close to tell the server to full disconnect
        //Else will just notify the server to mark the user as logged out 
        //Will go back to the login menu
        private void Logout(bool disconnect = true)
        {
            ElementVisability(false);
            LoginInformation logout = new LoginInformation();
            Message M = new Message();
            logout.stage = "5";
            if (disconnect) { logout.name = "close"; }
            M.id = "1";
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            M.message = Serializer.Serialize(logout);
            string MainMessage = Serializer.Serialize(M);
            SendDataToServer(MainMessage);
            Form1.ActiveForm.Text = "Chat Client";
            LINFO = new LoginInformation();
        }

        //If theres a server disconnect then the user will be notified and the applicaiton will close
        private void ServerLogout()
        {
            string logMessage = "Server has stopped";
            log.Info(logMessage);
            MessageBox.Show(logMessage);
            Application.Exit();
        }
        #endregion


        #region Registration 
        //This function handles the stages of registraion
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

        //This will request a salt from the server
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

        //This function hashes the password using the salt provided
        //Then send the server all registraion details 
        //If theres an error then the user will be notified
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
            else {
                string logMessage = "Unknown error occurred, please try again";
                log.Error(logMessage);
                MessageBox.Show(logMessage);
            }


        }

        //If the server has correctly register then the client will be notified 
        private void RegistrationFinalStage(RegistrationInformation RINFO)
        {
            if (RINFO.confirmation == "true")
            {
                log.Info("Server successfully added account");
                MessageBox.Show("Account added");
            }
            else {
                string logMessage = "Error with registrating account";
                log.Error(logMessage);
                MessageBox.Show(logMessage); 
            }
        }
        #endregion

        #region Forms Interaction

        //Show main true shows the chat window
        //Show main false show the login menu
        //Level is optional, if the level is anything but 0 this means to show the IP/Port menu
        private void ElementVisability(bool showMain, int level = 0)
        {
            Form1 form1 = this;
            if (level == 0)
            {
                gbxIPandPort.Visible = false;
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
            else
            {
                gBXLogin.Visible = false;
                gBXRegistraion.Visible = false;

                gbxIPandPort.Visible = true; 
                form1.Height = 208;
                form1.Width = 283;
            }

        }

        //When user presses send calls the SendMessage function
        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        //When the user presses login
        //Input validate  the email address and password
        //If both pass then start the login process
        //Else user error providers to notify the user
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

        //When the user presses register
        //Input validate  the email address, password, adn username
        //If both pass then start the registraion process
        //Else user error providers to notify the user
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

        //When the user closes the client window
        //Logout the client, cancel the listening thread and then close the application
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            log.Info("User logging out");
            Logout();
            connected = false;
            try{ listeningThread.Abort(); }
            catch (Exception){}
            Application.Exit();
        }

        //When client presses logout call the logout function
        private void btnLogout_Click(object sender, EventArgs e){ Logout(false); }

        //When the client cant connect to the server
        //Input validate the ip and port, if both correct try InitaliserServerConnection function again
        //Else keep asking for ip adn port until correct format
        private void btnConnect_Click(object sender, EventArgs e)
        {
            bool error = false;
            //Validates IP Address
            InputValidation validate = new InputValidation();
            if (validate.IPValidation(txtIPaddress.Text)) { }
            else
            {
                error = true;
                IPError.SetError(txtIPaddress, "Incorrect IP Format");
            }
            int portNumber = 0;
            if (int.TryParse(txtPort.Text, out portNumber)) { }
            else
            {
                error = true;
                IPError.SetError(txtPort, "Incorrect Port Format");
            }

            if (!error)
            {
                ServerIP = txtIPaddress.Text;
                ServerPort = portNumber;
                InitaliseServerConnection();
            }
        }

        #endregion


    }
}
