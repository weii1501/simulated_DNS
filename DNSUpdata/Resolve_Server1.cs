using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Diagnostics;
using System.Security.Cryptography;
using System.IO;
namespace DNSTest
{
    public partial class Resolve_Server1 : Form
    {
        private MongoClient dbclient;
        private IMongoDatabase Database;
        private IMongoCollection<BsonDocument> collation;
        public Resolve_Server1()
        {
            InitializeComponent();
            dbclient = new MongoClient("mongodb+srv://meone:letpk9741@cluster0.vye0x.mongodb.net/DNS?retryWrites=true&w=majority");
            Database = dbclient.GetDatabase("DNS");
            collation = Database.GetCollection<BsonDocument>("Database");
        }
        byte[] iv = new byte[16];
        byte[] array;
        string EncryptString(string key, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(array);
        }
        string DecryptString(string key, string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
        string text;
        int check = 0;
        string key = "b14ca5898a4e4133bbce2ea2315a1916";
        string result = "";
        bool ValidateIP = false;
        bool ValidateIP1 = false;
        //int count = 0;
        public Resolve_Server1( string text2): this()
        {
            text = text2;
        }
        //nhan data va xu li
        void recei()
        {
            // giá trị Any của IPAddress tương ứng với Ip của tất cả các giao diện mạng trên máy
            var localIp = IPAddress.Any;
            // tiến trình server sẽ sử dụng cổng 1308
            var localPort = 10000;
            // biến này sẽ chứa "địa chỉ" của tiến trình server trên mạng
            var localEndPoint = new IPEndPoint(localIp, localPort);
            // yêu cầu hệ điều hành cho phép chiếm dụng cổng 1308
            // server sẽ nghe trên tất cả các mạng mà máy tính này kết nối tới
            // chỉ cần gói tin udp đến cổng 1308, tiến trình server sẽ nhận được

            // một overload khác của hàm tạo Socket
            // InterNetwork là họ địa chỉ dành cho IPv4
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(localEndPoint);
            //richTextBox1.Text += localEndPoint + "\n";

            var size = 1024;
            var receiveBuffer = new byte[size];
            //truy vấn  
            string look = null;

            while (true)
            {
                // biến này về sau sẽ chứa địa chỉ của tiến trình client nào gửi gói tin tới
                EndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);

                // khi nhận được gói tin nào sẽ lưu lại địa chỉ của tiến trình client
                var length = socket.ReceiveFrom(receiveBuffer, ref remoteEndpoint);
                text = Encoding.UTF8.GetString(receiveBuffer, 0, length);
                //tach lay ip va text 
                string[] tach = text.Split('|');
                text = tach[0];
                string[] iptach = tach[1].Split(':');
                IPAddress ipaddress1 = IPAddress.Parse(iptach[0]);
             
                // biến này về sau sẽ chứa địa chỉ của tiến trình client nào gửi gói tin tới
                EndPoint remoteEndpoint1 = new IPEndPoint(ipaddress1, int.Parse(iptach[1]));
                richTextBox1.Text += $"Nhận từ client: {iptach[0]} thông qua resolver: {text}" + "\n";
                string Ip = null;
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("Dn", text);
                var docs = collation.Find(filter).ToList();
                foreach (var doc in docs)
                {
                    Ip = doc.GetValue("Ip").AsString;
                }
                richTextBox1.Text += Ip + "\n";
                look = Ip;

                //truy van du lieu
                if (look == null)//tìm trong resolver hong có thì gởi qua foreign sv
                {
                    text += "|2";
                    sendforeisever();
                    if (check == 0)
                    {
                        //gởi tới name server
                        sendnamesever();
                    }
                    int lentext = text.Length;
                    text = text.Substring(0, lentext - 2);
                    //gui du lieu sau khi truy van ve client
                    if (ValidateIP == true || result == "Host Not Found" || ValidateIP1 == true)
                    {

                        string encrypted;
                        // Create a new instance of the Aes
                        // class.  This generates a new key and initialization
                        // vector (IV).
                        using (Aes myAes = Aes.Create())
                        {
                            result += "--Load Balancing--";
                             // Encrypt the string to an array of bytes.
                             encrypted = EncryptString(key, result);
                        }
                        var sendBuffer = Encoding.ASCII.GetBytes(encrypted);
                        // gửi kết quả lại cho client
                        socket.SendTo(sendBuffer, remoteEndpoint1);
                        Array.Clear(receiveBuffer, 0, size);
                    }
                }
                else
                {
                    look += "--Load balancing from Resolve Server 1--";
                    //code gởi về client
                    string encrypted;
                    // Create a new instance of the Aes
                    // class.  This generates a new key and initialization
                    // vector (IV).
                    using (Aes myAes = Aes.Create())
                    {
                        // Encrypt the string to an array of bytes.
                        encrypted = EncryptString(key, look);
                        //textBox1.Text = encrypted;
                    }
                    var sendBuffer = Encoding.ASCII.GetBytes(encrypted);
                    // gửi kết quả lại cho client
                    socket.SendTo(sendBuffer, remoteEndpoint1);
                    //count--;
                    Array.Clear(receiveBuffer, 0, size);
                }

            }
            //đóng socket
            socket.Close();
        }

        void sendnamesever()
        {
            // yêu cầu người dùng nhập ip của server

            // chuyển đổi chuỗi ký tự thành object thuộc kiểu IPAddress
            var serverIp = IPAddress.Parse("127.0.0.3");

            // yêu cầu người dùng nhập cổng của server

            // chuyển chuỗi ký tự thành biến kiểu int
            var serverPort = int.Parse("7000");

            // đây là "địa chỉ" của tiến trình server trên mạng
            // mỗi endpoint chứa ip của host và port của tiến trình
            var serverEndpoint = new IPEndPoint(serverIp, serverPort);

            var size = 1024; // kích thước của bộ đệm
            var receiveBuffer = new byte[size]; // mảng byte làm bộ đệm            


            // yêu cầu người dùng nhập một chuỗi

            // khởi tạo object của lớp socket để sử dụng dịch vụ Udp
            // lưu ý SocketType của Udp là Dgram (datagram) 
            var socket = new Socket(SocketType.Dgram, ProtocolType.Udp);

            // biến đổi chuỗi thành mảng byte
            var sendBuffer = Encoding.UTF8.GetBytes(text);
            // gửi mảng byte trên đến tiến trình server
            socket.SendTo(sendBuffer, serverEndpoint);

            // endpoint này chỉ dùng khi nhận dữ liệu
            EndPoint dummyEndpoint = new IPEndPoint(IPAddress.Any, 0);
            // nhận mảng byte từ dịch vụ Udp và lưu vào bộ đệm
            // biến dummyEndpoint có nhiệm vụ lưu lại địa chỉ của tiến trình nguồn
            // tuy nhiên, ở đây chúng ta đã biết tiến trình nguồn là Server
            // do đó dummyEndpoint không có giá trị sử dụng 
            var length = socket.ReceiveFrom(receiveBuffer, ref dummyEndpoint);
            // chuyển đổi mảng byte về chuỗi
            result = Encoding.UTF8.GetString(receiveBuffer, 0, length);
            // xóa bộ đệm (để lần sau sử dụng cho yên tâm)
            Array.Clear(receiveBuffer, 0, size);

            // đóng socket và giải phóng tài nguyên
            socket.Close();

            // in kết quả ra màn hình
            richTextBox1.Text += result + "\n";

            //Kiểm tra dữ liệu trả về từ Nameserver có phải IP hay không
            ValidateIP1 = ValidateIPv4(result);
        }
        public bool ValidateIPv4(string ipString)
        {
            if (String.IsNullOrWhiteSpace(ipString))
            {
                return false;
            }
            string[] splitValues = ipString.Split('.');
            if (splitValues.Length != 4)
            {
                return false;
            }
            byte tempForParsing;
            return splitValues.All(r => byte.TryParse(r, out tempForParsing));
        }
        //Truy vấn Nameserver
        void sendforeisever()
        {
            // yêu cầu người dùng nhập ip của server

            // chuyển đổi chuỗi ký tự thành object thuộc kiểu IPAddress
            var serverIp = IPAddress.Parse("127.0.0.2");

            // yêu cầu người dùng nhập cổng của server

            // chuyển chuỗi ký tự thành biến kiểu int
            var serverPort = int.Parse("6000");

            // đây là "địa chỉ" của tiến trình server trên mạng
            // mỗi endpoint chứa ip của host và port của tiến trình
            var serverEndpoint = new IPEndPoint(serverIp, serverPort);

            var size = 1024; // kích thước của bộ đệm
            var receiveBuffer = new byte[size]; // mảng byte làm bộ đệm            

            // khởi tạo object của lớp socket để sử dụng dịch vụ Udp
            // lưu ý SocketType của Udp là Dgram (datagram) 
            var socket = new Socket(SocketType.Dgram, ProtocolType.Udp);

            // biến đổi chuỗi thành mảng byte
            var sendBuffer = Encoding.UTF8.GetBytes(text);
            // gửi mảng byte trên đến tiến trình server
            socket.SendTo(sendBuffer, serverEndpoint);//gởi đến foreign server

            //nhận dữ liệu từ foreign server

            // endpoint này chỉ dùng khi nhận dữ liệu
            EndPoint dummyEndpoint = new IPEndPoint(IPAddress.Any, 0);
            // nhận mảng byte từ dịch vụ Udp và lưu vào bộ đệm
            // biến dummyEndpoint có nhiệm vụ lưu lại địa chỉ của tiến trình nguồn
            // tuy nhiên, ở đây chúng ta đã biết tiến trình nguồn là Server
            // do đó dummyEndpoint không có giá trị sử dụng 
            var length = socket.ReceiveFrom(receiveBuffer, ref dummyEndpoint);
            // chuyển đổi mảng byte về chuỗi
            result = Encoding.UTF8.GetString(receiveBuffer, 0, length);
            result = result.Trim();
            //IPAddress ip;
            //kiem tra du lieu tra ve cua Foreiserver co phai dia chi ip hay khong
            ValidateIP = ValidateIPv4(result);
            if (ValidateIP)
            {
                check = 1;
                richTextBox1.Text += result + "\n";
            }
            // xóa bộ đệm (để lần sau sử dụng cho yên tâm)
            Array.Clear(receiveBuffer, 0, size);

            // đóng socket và giải phóng tài nguyên
            socket.Close();
        }
        // Hàm kiểm tra IP
        // Tạo luồng thực thi
        private void Resolve_Server1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            Thread m = new Thread(new ThreadStart(recei));
            m.Start();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
        void add(string dm, string ip)
        {
            BsonDocument userData = new BsonDocument()
                        .Add("Dn", dm)
                        .Add("Ip", ip);
            collation.InsertOne(userData);
        }
    }
}
