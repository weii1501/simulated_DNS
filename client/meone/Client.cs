using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
namespace meone
{
    public partial class Client : Form
    {
        public Client()
        {
            InitializeComponent();
            ShowDatabaseOfCache();
            
        }
        string dblink = @"Data Source=.\SQLEXPRESS;Initial Catalog=doanltm1;Integrated Security=True"; // thay đổi path sql ngay chỗ này
        string key = "b14ca5898a4e4133bbce2ea2315a1916";
        byte[] iv = new byte[16];
        byte[] array;
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
        void cache()
        {
            //truy vấn
            string look = null;
            //tìm vào database trước

            //truy van trong case
            using (SqlConnection connection = new SqlConnection(dblink))
            {
                connection.Open();
                string getcmd = $"select ipadd from Table_2 where domain='{textBox1.Text}'";
                SqlCommand command = new SqlCommand(getcmd, connection);
                look = (string)command.ExecuteScalar();
                connection.Close();
            }

            if (look != null)
            {
                show(look, textBox1.Text);
            }
            else
            {
                send();
                ShowDatabaseOfCache();
            }
        }

        void send()
        {
            try
            {
                // chuyển đổi chuỗi ký tự thành object thuộc kiểu IPAddress
                var serverIp = IPAddress.Parse("25.17.126.199");

                // yêu cầu người dùng nhập cổng của server

                // chuyển chuỗi ký tự thành biến kiểu int
                var serverPort = int.Parse("5000");
                // mỗi endpoint chứa ip của host và port của tiến trình
                var serverEndpoint = new IPEndPoint(serverIp, serverPort);

                var size = 1024; // kích thước của bộ đệm
                var receiveBuffer = new byte[size]; // mảng byte làm bộ đệm            


                // yêu cầu người dùng nhập một chuỗi
                var text = textBox1.Text;

                // khởi tạo object của lớp socket để sử dụng dịch vụ Udp
                // lưu ý SocketType của Udp là Dgram (datagram) 
                var socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
                string encrypted;

                using (Aes myAes = Aes.Create())
                {
                    // Encrypt the string to an array of bytes.
                    encrypted = EncryptString(key, text);
                }

                var sendBuffer = Encoding.ASCII.GetBytes(encrypted);
                // biến đổi chuỗi thành mảng byte

                // gửi mảng byte trên đến tiến trình server
                socket.SendTo(sendBuffer, serverEndpoint);

                // endpoint này chỉ dùng khi nhận dữ liệu
                EndPoint dummyEndpoint = new IPEndPoint(IPAddress.Any, 0);

                // do đó dummyEndpoint không có giá trị sử dụng 
                var length = socket.ReceiveFrom(receiveBuffer, ref dummyEndpoint);
                // chuyển đổi mảng byte về chuỗi
                var decode = Encoding.ASCII.GetString(receiveBuffer, 0, length);
                // xóa bộ đệm (để lần sau sử dụng cho yên tâm)
                var result = DecryptString(key, decode);
                Array.Clear(receiveBuffer, 0, size);

                // đóng socket và giải phóng tài nguyên
                socket.Close();




                //ghi vao cache
                string look = null;
                //truy van trong case
                show(result, text);
                using (SqlConnection connection = new SqlConnection(dblink))
                {
                    connection.Open();
                    string getcmd = $"select ipadd from Table_2 where domain='{textBox1.Text}'";
                    SqlCommand command = new SqlCommand(getcmd, connection);
                    look = (string)command.ExecuteScalar();
                    connection.Close();
                }
                if (look == null)
                {
                    if (result.IndexOf("Host Not Found") == -1)
                    {
                        insert(text, repairip(result));
                    }
                }
            }
            catch
            {
                MessageBox.Show("server chưa bật ", "warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        public void Chrome(string link)
        {
            string url = "";
            if (link == "Host Not Found")
            {
                return;
            }
            else
            {
                if (!string.IsNullOrEmpty(link))
                {
                    if (link.Contains('.'))
                    {
                        url = link;
                    }
                    else
                    {
                        url = "https://www.google.com/search?q=" + link.Replace(" ", "+");
                    }
                }
                try
                {
                    Process.Start("chrome.exe", url + " --incognito");
                }
                catch (System.ComponentModel.Win32Exception e)
                {
                    MessageBox.Show("Unable to find Google Chrome...",
                        "chrome.exe not found!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btn_Search_Click(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            // có xử dụng cache thì thay send = cache
            Thread m = new Thread(new ThreadStart(cache));
            m.Start();
        }

        void show(string ip, string domain)
        {
            string[] arr = new string[2];
            ListViewItem item;
            //thêm Item vào ListView
            arr[1] = domain;
            arr[0] = ip;
            item = new ListViewItem(arr);
            listView1.Items.Add(item);
        }

        private void Client_Load(object sender, EventArgs e)
        {
            listView1.View = View.Details;
            listView1.Columns.Add("IP", 300);
            listView1.Columns.Add("Domain name", 300);
        }
        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            if (listView1.SelectedItems[0].Text.IndexOf("Host Not Found") == -1)
            {
                Chrome(repairip(listView1.SelectedItems[0].Text));
            }

            else
                MessageBox.Show("Không tìm thấy địa chỉ IP",  "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Client_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(dblink))
                {
                    connection.Open();
                    string getcmd = $"delete from Table_2";
                    SqlCommand command = new SqlCommand(getcmd, connection);
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        int check=999;
        void ShowDatabaseOfCache()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(dblink))
                {

                    if (listView2 != null)
                    {
                        
                        connection.Open();
                        string query = "select * from Table_2";
                        SqlDataAdapter adapt = new SqlDataAdapter("select * from Table_2", connection);
                        DataSet ds = new DataSet();
                        adapt.Fill(ds, "table");
                        DataTable dt = new DataTable();
                        dt = ds.Tables["table"];
                        if (check!=dt.Rows.Count)
                        {
                            listView2.Clear();
                            listView2.GridLines = true;
                            listView2.View = View.Details;
                            listView2.Columns.Add("Domain", 200);
                            listView2.Columns.Add("IP", 200);
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                if (listView2 != null)
                                {
                                    listView2.Items.Add(dt.Rows[i].ItemArray[0].ToString());
                                    listView2.Items[i].SubItems.Add(dt.Rows[i].ItemArray[1].ToString());
                                }

                            }
                            check = dt.Rows.Count;
                        }
                        
                        connection.Close();
                    }
                    
                }
            }
            catch
            {
                MessageBox.Show("lỗi kết nối database", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        void insert(string domain, string ip)
        {
            
                using (SqlConnection connection = new SqlConnection(dblink))
                {
                    connection.Open();
                    string ins = "insert into Table_2(domain,ipadd) values(@domain,@ipadd)";
                    SqlCommand cmd = new SqlCommand(ins, connection);
                    cmd.Parameters.AddWithValue("@domain", domain);
                    cmd.Parameters.AddWithValue("@ipadd", ip);
                    cmd.ExecuteNonQuery();
                    connection.Close();
                    DisplayData();
                }
            
        }
        private string repairip(string result)
        {
            string[] ip = result.Split('-');
            return ip[0];
        }
        private void DisplayData()
        {
            using (SqlConnection connection = new SqlConnection(dblink))
            {
                connection.Open();
                DataTable dt = new DataTable();
                SqlDataAdapter adapt = new SqlDataAdapter("select * from Table_2",connection);
                adapt.Fill(dt);
                connection.Close();
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
