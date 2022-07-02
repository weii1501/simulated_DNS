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
    public partial class Resolve_Server : Form
    {
        private MongoClient dbclient;
        private IMongoDatabase Database;
        private IMongoCollection<BsonDocument> collation;
        private IMongoCollection<BsonDocument> collotion;

        public Resolve_Server()
        {
            InitializeComponent();
            this.dataGridView1.DefaultCellStyle.Font = new Font("Tahoma", 12);
            dbclient = new MongoClient("mongodb+srv://meone:letpk9741@cluster0.vye0x.mongodb.net/DNS?retryWrites=true&w=majority");
            Database = dbclient.GetDatabase("DNS");
            collation = Database.GetCollection<BsonDocument>("Database");
            collotion = Database.GetCollection<BsonDocument>("TimeLine");
            showdatabase();
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
        string result = "";
        bool ValidateIP = false;
        bool ValidateIP1 = false;
        int count = 0;
        string key = "b14ca5898a4e4133bbce2ea2315a1916";
        //nhan data va xu li
        void recei()
        {

            //truy vấn  
            string look = null;
            //tìm vào database trước

            // giá trị Any của IPAddress tương ứng với Ip của tất cả các giao diện mạng trên máy
            var localIp = IPAddress.Any;
            var localPort = 5000;
            // biến này sẽ chứa "địa chỉ" của tiến trình server trên mạng
            var localEndPoint = new IPEndPoint(localIp, localPort);
            // yêu cầu hệ điều hành cho phép chiếm dụng cổng 5000
            // server sẽ nghe trên tất cả các mạng mà máy tính này kết nối tới
            // chỉ cần gói tin udp đến cổng 5000, tiến trình server sẽ nhận được

            // một overload khác của hàm tạo Socket
            // InterNetwork là họ địa chỉ dành cho IPv4
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(localEndPoint);
            //richTextBox1.Text += localEndPoint + "\n";

            var size = 1024;
            var receiveBuffer = new byte[size];

            while (true)
            {
                Thread.Sleep(30);
                // biến này về sau sẽ chứa địa chỉ của tiến trình client nào gửi gói tin tới
                EndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
                // khi nhận được gói tin nào sẽ lưu lại địa chỉ của tiến trình client
                var length = socket.ReceiveFrom(receiveBuffer, ref remoteEndpoint);
                text = Encoding.UTF8.GetString(receiveBuffer, 0, length);
                //richTextBox1.Text += $"Nhận từ client: {text}" + "\n";
                text = DecryptString(key, text);
                text = text.Replace('|', ' ');
                string[] remote = remoteEndpoint.ToString().Split(':');
                //add vao database daily
                richTextBox1.Text += $"Nhận từ client {remote[0]}: {text}" + "\n";
                DateTime aDateTime = DateTime.Now;
                string time = aDateTime.ToString();
                adddaily(time, remote[0], text);
                if (text != null)
                    count++;
                //=>>>>>>
                //chuyen huong sang Resolver khac
                if ((count % 2) == 1)
                {
                    CheckForIllegalCrossThreadCalls = false;
                    Thread thread = new Thread(() => sendresolveserver1(remoteEndpoint.ToString()));
                    thread.Start();
                    //truyen du lieu sang resolve 1
                }
                else
                {
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
                        text += "|1";
                        sendforeisever();
                        if (check == 0)
                        {
                            //gởi tới name server
                            sendnamesever();
                        }
                        int lentext = text.Length;
                        text = text.Substring(0,lentext-2);
                        //gui du lieu sau khi truy van ve client
                        if (ValidateIP == true || result == "Host Not Found" || ValidateIP1 == true)
                        {
                            string encrypted;
                            // Create a new instance of the Aes
                            // class.  This generates a new key and initialization
                            // vector (IV).
                            using (Aes myAes = Aes.Create())
                            {
                                // Encrypt the string to an array of bytes.
                                encrypted = EncryptString(key, result);
                            }
                            var sendBuffer = Encoding.ASCII.GetBytes(encrypted);
                            // gửi kết quả lại cho client
                            socket.SendTo(sendBuffer, remoteEndpoint);
                            Array.Clear(receiveBuffer, 0, size);
                        }
                    }
                    else
                    {
                        //code gởi về client
                        string encrypted;
                        // Create a new instance of the Aes
                        // class.  This generates a new key and initialization
                        // vector (IV).
                        using (Aes myAes = Aes.Create())
                        {
                            // Encrypt the string to an array of bytes.
                            encrypted = EncryptString(key, look);
                        }
                        var sendBuffer = Encoding.ASCII.GetBytes(encrypted);
                        // gửi kết quả lại cho client
                        socket.SendTo(sendBuffer, remoteEndpoint);
                        
                        Array.Clear(receiveBuffer, 0, size);
                    }
                }

            }
            //đóng socket
            socket.Close();
        }
        //truyền dữ liệu đến resolve_server1
        void sendresolveserver1(string localIp)
        {
            // yêu cầu người dùng nhập ip của server

            // chuyển đổi chuỗi ký tự thành object thuộc kiểu IPAddress
            var serverIp = IPAddress.Parse("127.0.0.7");

            // yêu cầu người dùng nhập cổng của server

            // chuyển chuỗi ký tự thành biến kiểu int
            var serverPort = int.Parse("10000");

            // đây là "địa chỉ" của tiến trình server trên mạng
            // mỗi endpoint chứa ip của host và port của tiến trình
            var serverEndpoint = new IPEndPoint(serverIp, serverPort);

            var size = 1024; // kích thước của bộ đệm
            var receiveBuffer = new byte[size]; // mảng byte làm bộ đệm            

            // khởi tạo object của lớp socket để sử dụng dịch vụ Udp
            // lưu ý SocketType của Udp là Dgram (datagram) 
            var socket = new Socket(SocketType.Dgram, ProtocolType.Udp);

            // biến đổi chuỗi thành mảng byte
            text += "|" + localIp;
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
                richTextBox1.Text +=  result + "\n";
            }
            // xóa bộ đệm (để lần sau sử dụng cho yên tâm)
            Array.Clear(receiveBuffer, 0, size);
            // đóng socket và giải phóng tài nguyên
            socket.Close();
        }

        //Truy vấn Foreignserver
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
                richTextBox1.Text +=  result + "\n";
            }
            // xóa bộ đệm (để lần sau sử dụng cho yên tâm)
            Array.Clear(receiveBuffer, 0, size);

            // đóng socket và giải phóng tài nguyên
            socket.Close();

        }

        //Truy vấn Nameserver
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


        // Tạo luồng thực thi
        private void resolver_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            Thread m = new Thread(new ThreadStart(recei));
            m.Start();
        }

        // Hàm kiểm tra IP
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

        void showdatabase()
        {
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            var documents = collation.Find(new BsonDocument()).ToList();
            //tao table
            DataTable dt = new DataTable();
            dt.Columns.Add("IP");
            dt.Columns.Add("Domain");
            DataRow row = dt.NewRow();
            //them data
            foreach (BsonDocument doc in documents)
            {
                dt.Rows.Add(doc["Ip"], doc["Dn"]);
            }
            //them vao gridview
            dataGridView1.DataSource = dt;
        }

        void showdatabase1()
        {
            try
            {
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                var documents = collation.Find(new BsonDocument()).ToList();
                //tao table
                dataGridView1.Refresh();
                dataGridView1.DataSource = null;
                DataTable dt = new DataTable();
                dt.Columns.Add("IP");
                dt.Columns.Add("Domain");
                DataRow row = dt.NewRow();
                //them data
                foreach (BsonDocument doc in documents)
                {
                    dt.Rows.Add(doc["Ip"], doc["Dn"]);
                }
                //them vao gridview
                dataGridView1.DataSource = dt;
            }
            catch
            {
               
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (ValidateIPv4(textBox2.Text))
            {
                string Ip = null;
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("Dn", textBox1.Text);
                var docs = collation.Find(filter).ToList();
                foreach (var doc in docs)
                {
                    Ip = doc.GetValue("Ip").AsString;
                }
                if (Ip == null)
                {
                    add(textBox1.Text, textBox2.Text);
                    showdatabase1();
                }
                else
                    MessageBox.Show("Dữ liệu đã tồn tại trong database");
            }
            else
                MessageBox.Show("Nhập lại địa chỉ IP");
        }
        void add(string dm, string ip)
        {
            BsonDocument userData = new BsonDocument()
                        .Add("Dn", dm)
                        .Add("Ip", ip);
            collation.InsertOne(userData);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim() != "")
            {
                var deleteFilter = Builders<BsonDocument>.Filter.Eq("Dn", textBox1.Text);
                string Ip = null;
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("Dn", textBox1.Text);
                var docs = collation.Find(filter).ToList();
                foreach (var doc in docs)
                {
                    Ip = doc.GetValue("Ip").AsString;
                }
                if (Ip != null)
                {
                    collation.DeleteOne(deleteFilter);
                    showdatabase1();
                }
                else
                    MessageBox.Show("Dữ liệu không tồn tại trong database");
            }
            else
                MessageBox.Show("Nhập Domain name để xóa");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (ValidateIPv4(textBox2.Text))
            {
                var filter = Builders<BsonDocument>.Filter.Eq("Dn", textBox1.Text);
                var update = Builders<BsonDocument>.Update.Set("Ip", textBox2.Text);
                collation.UpdateOne(filter, update);
                showdatabase1();
            }
            else
                MessageBox.Show("IP không đúng!!! Vui lòng nhập lại IP!!!");
        }
        void adddaily(string time, string ip, string dm)
        {
            BsonDocument userData = new BsonDocument()
                        .Add("Time", time)
                        .Add("Ip", ip)
                        .Add("Dn", dm);
            collotion.InsertOne(userData);
        }
        private void button4_Click(object sender, EventArgs e)
        {
            History h = new History();
            h.Show();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }
    }
}
