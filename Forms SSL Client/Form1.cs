using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;


using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace Forms_SSL_Client
{


    public partial class Form1 : Form
    {

        Authenticator auth = new Authenticator();
        //public string ServerIP { get { return "192.168.0.23"; } }
        public string ServerIP { get { return "82.28.214.91"; } }
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
                catch (Exception) {
                    MessageBox.Show("FUCK");
                }

            }
        }
        delegate void DecodeMessageDelegate(string message);
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
            txtMessageBox.AppendText(SM.message + Environment.NewLine);
        }

        private void DecodeMessage(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new DecodeMessageDelegate(DecodeMessage), new object[] { message });
            }
            else
            {
                Message M = new Message();

                JavaScriptSerializer Deserializer = new JavaScriptSerializer();
                M = Deserializer.Deserialize<Message>(message);
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
            SM.message = msg;
            string SMstring = Serializer.Serialize(SM);
            M.id = "3";
            M.message = SMstring;
            string Mstring = Serializer.Serialize(M);
            writer.WriteLine(Mstring);
            writer.Flush();
            txtMsgToSend.Text = "";
        }


        private void LoginBegin() //Begins login steps 
        {
            LINFO.Email = txtUserEmail.Text;
            LINFO.stage = "1";
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            string LINFOmessage = Serializer.Serialize(LINFO);
            M.id = "1";
            M.message = LINFOmessage;
            string message = Serializer.Serialize(M);
            writer.WriteLine(message);
            writer.Flush();

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
                    LogInFinalStage(LINFO.confirmation);
                    break;
                default:
                    break;
            }
        }
        private string PreformPBKDF2Hash(string accountHashedPassword, byte[] salt)
        {
            int iterations = 10000;
            Rfc2898DeriveBytes hash = new Rfc2898DeriveBytes(accountHashedPassword, salt, iterations);
            return Convert.ToBase64String(hash.GetBytes(128));
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
                writer.WriteLine(message);
                writer.Flush();
            }
            else
            {
                MessageBox.Show("NO FOUND EMAIL");
            }

        }
        private void LogInFinalStage(string confirmation)
        {
            if (confirmation == "true")
            {
                MessageBox.Show("Successfully Logged in");
                ElementVisability();
            }
            else
            {
                MessageBox.Show("Unsuccessful");
            }
        }//Client stage 3

        private void RegistrationBegin() //Request salt from server
        {
            RINFO.stage = "1";
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string RINFOmessage = serializer.Serialize(RINFO);
            M.id = "2";
            M.message = RINFOmessage;
            string message = serializer.Serialize(M);
            writer.WriteLine(message);
            writer.Flush();
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
                writer.WriteLine(message);
                writer.Flush();
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
        private void ElementVisability()
        {
            txtMessageBox.Visible = true;
            txtMsgToSend.Visible = true;
            btnSend.Visible = true;

            gBXLogin.Visible = false;
            gBXRegistraion.Visible = false;
            Form1 form1 = this;
            form1.Height = 527;
            form1.Width = 395;

        }
        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessage();

        }

        private void txtMessageBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnlogin_Click(object sender, EventArgs e)
        {
            LoginBegin();

        }

        private void btnReg_Click(object sender, EventArgs e)
        {
            RegistrationBegin();
        }



    }
    class Authenticator
    {
        Aes aesKey;
        public Authenticator()
        {
            aesKey = Aes.Create();
        }
        public string encryptMessage(string message)
        {
            try
            {
                byte[] encryptedData;
                string encryptedString = "";
                ICryptoTransform encryptor = aesKey.CreateEncryptor();
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(message);
                        }
                        encryptedData = msEncrypt.ToArray();
                    }
                }
                encryptedString = Encoding.Unicode.GetString(encryptedData);
                return encryptedString;
            }
            catch (Exception)
            {

                throw;
            }

        }
        public string decryptMessage(string message)
        {
            try
            {
                string decryptedString = "";
                byte[] encryptedData = Encoding.Unicode.GetBytes(message);
                ICryptoTransform decryptor = aesKey.CreateDecryptor();
                using (MemoryStream msDecrypt = new MemoryStream(encryptedData))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            decryptedString = srDecrypt.ReadToEnd();
                        }
                    }
                }
                return decryptedString;
            }
            catch (Exception)
            {

                throw;
            }

        }
        public byte[] getEncryptedAesKey(string key)
        {
            var pubCSP = new RSACryptoServiceProvider();
            pubCSP.FromXmlString(key);
            byte[] byteEncryptedAES = pubCSP.Encrypt(aesKey.Key, RSAEncryptionPadding.Pkcs1);
            //key = Encoding.Unicode.GetString(byteEncryptedAES);
            return byteEncryptedAES;
        }
        public byte[] getEncryptedAesIV(string key)
        {
            string iv;
            var pubCSP = new RSACryptoServiceProvider();
            pubCSP.FromXmlString(key);
           byte[] byteEncryptedAES = pubCSP.Encrypt(aesKey.IV, RSAEncryptionPadding.Pkcs1);


            //iv = Encoding.Unicode.GetString(byteEncryptedAES);
            return byteEncryptedAES;
        }
    }


    class Message //Default message recieved 
    {
        public string id;
        public string message; //Additional serialised Message
    }
    class HandShakeMessage //ID CODE 0
    {
        public string stage;
        public string test;
        public string RSAPublicKey;
        public byte[] EncryptedAESKey;
        public byte[] EncryptedAESIV;
        public bool Confirmation;

    }
    class LoginInformation //ID CODE 1
    {
        public string confirmation;
        public string stage;
        public string Email;
        public string accountSalt;
        public string sessionSalt;
        public string passwordHash;
    }
    class RegistrationInformation //ID CODE 2
    {
        public string Name;
        public string Email;
        public string PasswordHash;
        public string AccountSalt;
        public string stage;
        public string confirmation;
    }
    class StandardMessage //ID CODE 3
    {
        public string message;
    }
}
