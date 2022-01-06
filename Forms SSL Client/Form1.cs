using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace Forms_SSL_Client
{
   

    public partial class Form1 : Form
    {
        public string ServerIP { get { return "192.168.0.23"; } }
        public int ServerPort { get { return 2000; } }

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

            client = new TcpClient(ServerIP, ServerPort);
            netStream = client.GetStream();
            sslStream = new SslStream(netStream, false, new RemoteCertificateValidationCallback(ValidateCertificate));
            sslStream.AuthenticateAsClient("SecureChat");
            reader = new StreamReader(sslStream, Encoding.Unicode);
            writer = new StreamWriter(sslStream, Encoding.Unicode);
            listeningThread = new Thread(new ThreadStart(listenForMessages));
            listeningThread.Start();
        
        }
        delegate void DecodeMessageDelegate(string message);
        private void listenForMessages()
        {
            while (true)
            {
                string recievedMessage = reader.ReadLine();
                DecodeMessage(recievedMessage);
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
                        break;
                    case "1": //Login
                        LoginManager(M);
                        break;
                    case "2"://Registration
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
                    PasswordHasher(LINFO);
                    break;
                case "3":
                    ConfimationValidator(LINFO.confirmation);
                    break;
                default:
                    break;
            }
        }

        private void PasswordHasher(LoginInformation LINFO) //Client stage 2
        {
            if (LINFO.confirmation == "true")
            {
                string plainTextPassword = txtPassword.Text;
                //Hash with accountSalt
                //Hash with sessionSalt 
                LINFO.passwordHash = plainTextPassword;
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

        private void ConfimationValidator(string confirmation)
        {
            if (confirmation == "true")
            {
                MessageBox.Show("ACCOUNT VALID");
            }
            else
            {
                MessageBox.Show("YA WRONG");
            }
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


    }
    class Message //Default message recieved 
    {
        public string id;
        public string message; //Additional serialised Message
    }
    class HandShakeMessage //ID CODE 0
    {
        public string RSAPublicKey;
        public string EncryptedAESKey;
        public string EncryptedAESIV;
        public bool Confirmation;
        public string SessionSalt;
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
    }
    class StandardMessage //ID CODE 3
    {
        public string message;
    }
}
