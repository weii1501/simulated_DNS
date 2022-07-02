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

namespace DNSTest
{
    public partial class ForeignName_Server : Form
    {
        private MongoClient dbclient;
        private IMongoDatabase Database;
        private IMongoCollection<BsonDocument> collation;

        public ForeignName_Server()
        {
            InitializeComponent();
            this.dataGridView1.DefaultCellStyle.Font = new Font("Tahoma", 12);
            dbclient = new MongoClient("mongodb+srv://meone:letpk9741@cluster0.vye0x.mongodb.net/DNS?retryWrites=true&w=majority");
            Database = dbclient.GetDatabase("DNS");
            collation = Database.GetCollection<BsonDocument>("ForeignServer");
            showdatabase();
        }
        string text;
        void recei()
        {
            // giá trị Any của IPAddress tương ứng với Ip của tất cả các giao diện mạng trên máy
            var localIp = IPAddress.Any;
            // tiến trình server sẽ sử dụng cổng 1308
            var localPort = 6000;
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

            while (true)
            {
                // biến này về sau sẽ chứa địa chỉ của tiến trình client nào gửi gói tin tới
                EndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
                // khi nhận được gói tin nào sẽ lưu lại địa chỉ của tiến trình client
                var length = socket.ReceiveFrom(receiveBuffer, ref remoteEndpoint);
                 text = Encoding.UTF8.GetString(receiveBuffer, 0, length);
                string[] dev = text.Split('|');
                text = dev[0];
                if (dev[1] == "1")
                    richTextBox1.Text += $"Nhận từ Resolve_Server: {text}" + "\n";
                else
                    richTextBox1.Text += $"Nhận từ Resolve_Server1: {text}" + "\n";
                string look = null;
                //truy van 

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

                if (look != null)//neu tim thay tra ve IP
                {
                    var sendBuffer = Encoding.UTF8.GetBytes(look);
                    // gửi kết quả lại cho resolver
                    socket.SendTo(sendBuffer, remoteEndpoint);

                    Array.Clear(receiveBuffer, 0, size);
                }
                else////neu khong tim thay tra ve du lieu ban dau
                {
                    var sendBuffer = Encoding.UTF8.GetBytes("Host Not Found");
                    // gửi kết quả lại cho resolver
                    socket.SendTo(sendBuffer, remoteEndpoint);

                    Array.Clear(receiveBuffer, 0, size);
                }
            }
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

        private void button1_Click(object sender, EventArgs e)
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
                    BsonDocument userData = new BsonDocument()
                        .Add("Dn", textBox1.Text)
                        .Add("Ip", textBox2.Text);
                    collation.InsertOne(userData);
                    showdatabase1();
                }
                else
                    MessageBox.Show("Dữ liệu đã tồn tại trong database");
            }
            else
                MessageBox.Show("Nhập lại địa chỉ IP");
        }

        private void foreiserver_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            Thread m = new Thread(new ThreadStart(recei));
            m.Start();
        }

        private void button2_Click(object sender, EventArgs e)
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

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

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
                MessageBox.Show("IP khong dung");
        }
    }
}
