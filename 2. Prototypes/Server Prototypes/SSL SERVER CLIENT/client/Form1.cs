using System;
using System.Windows.Forms;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Web.Script.Serialization;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;

namespace client
{

    public partial class Form1 : Form
    {
        public static SslStream sslStream;
        delegate void MessageReciever(serverMessage newMsg);
        delegate void AddMEssageDelegates(string message);

        event MessageReciever eventMessage;

        public static bool ValidateCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }
            return false;
        }
        class serverMessage
        {

            public string message;
        }
        class information
        {
            public string identifier;
            public string message;
        }
        TcpClient clientSocket;
        NetworkStream serverStream = default(NetworkStream);
        Thread listeningThread = null;

        public Form1()
        {
            InitializeComponent();
            eventMessage += new MessageReciever(RecievedMessage);
        }

        private void RunClient()
        {
            TcpClient client = new TcpClient("192.168.0.23", 8888);
            sslStream = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateCertificate), null);
            try
            {
                sslStream.AuthenticateAsClient("InstantMessengerServer");
            }
            catch (AuthenticationException e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
                if (e.InnerException != null)
                {
                    Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                }
                Console.WriteLine("Authentication failed - closing the connection.");
                client.Close();
                return;
            }

            listeningThread = new Thread(new ThreadStart(listen));
            listeningThread.Start();
        }

        private void listen()
        {
            while (true)
            {
                byte[] buffer = new byte[2048];
                StringBuilder message = new StringBuilder();
                int bytes = -1;
                do
                {
                    bytes = sslStream.Read(buffer, 0, buffer.Length);

                    Decoder decoder = Encoding.UTF8.GetDecoder();
                    char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                    decoder.GetChars(buffer, 0, bytes, chars, 0);
                    message.Append(chars);
                    // Check for EOF.
                    if (message.ToString().IndexOf("<EOF>") != -1)
                    {
                        break;
                    }
                } while (bytes != 0);

                AddMessage(message.ToString());

            }
        }

        private void AddMessage(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new AddMEssageDelegates(AddMessage), new object[] { message });
            }
            else
            {
                //Deserialiing the JSON message into a TelemeteryUpdate instance 
                JavaScriptSerializer Deserializer = new JavaScriptSerializer();
                serverMessage newMsg = Deserializer.Deserialize<serverMessage>(message);
                eventMessage?.Invoke(newMsg); 
                
            }
        }

        private void RecievedMessage(serverMessage servermsg)
        {
            lblRecieved.Text = servermsg.message;
        }
        private void txtRecieveMsg_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnSend_Click(object sender, EventArgs e)
        {
  
            information info = new information();
            info.identifier = txtIdentifer.Text.ToString();
            info.message = txtMessage.Text.ToString();
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            string jsonString = Serializer.Serialize(info);
            NetworkStream stream = clientSocket.GetStream();
            byte[] rawData = Encoding.ASCII.GetBytes(jsonString);
            serverStream.Write(rawData, 0, rawData.Length);
            serverStream.Flush();
        }

        private void txtSendMsg_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            RunClient();
        }

        private void btnreset_Click(object sender, EventArgs e)
        {
            lblRecieved.Text = "";
        }

        private void txtIdentifer_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtMessage_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
