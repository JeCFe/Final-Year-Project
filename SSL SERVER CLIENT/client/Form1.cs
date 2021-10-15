using System;
using System.Windows.Forms;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Web.Script.Serialization;

namespace client
{
    public partial class Form1 : Form
    {
        delegate void MessageReciever(serverMessage newMsg);
        delegate void AddMEssageDelegates(string message);

        event MessageReciever eventMessage;
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

        private void listen()
        {
            NetworkStream stream = clientSocket.GetStream();
            while (true)
            {
                byte[] buffer = new byte[256];
                int num_bytes = stream.Read(buffer, 0, 256);
                if (num_bytes>0)
                {
                    string message = Encoding.ASCII.GetString(buffer, 0, num_bytes);
                    AddMessage(message);
                }
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
            clientSocket = new TcpClient();
            IPAddress ip = IPAddress.Parse("192.168.0.23");
            int port = 8888;
            clientSocket.Connect(ip, port);
            serverStream = clientSocket.GetStream();
            
            
            listeningThread = new Thread(new ThreadStart(listen));
            listeningThread.Start();
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
