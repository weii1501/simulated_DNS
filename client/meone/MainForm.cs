using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

namespace meone
{
    public partial class MainForm : Form
    {
        private MongoClient dbclient;
        private IMongoDatabase Database;
        private IMongoCollection<BsonDocument> collation;
        public MainForm()
        {
            InitializeComponent();
            dbclient = new MongoClient("mongodb+srv://meone:letpk9741@cluster0.vye0x.mongodb.net/DNS?retryWrites=true&w=majority");
            Database = dbclient.GetDatabase("DNS");
            collation = Database.GetCollection<BsonDocument>("Client_List");
        }

       
        private void Form1_Load(object sender, EventArgs e)
        {
            panel1.BackColor = Color.FromArgb(100, 0, 0, 0);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!!!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                string pass = null;
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("User", textBox1.Text);
                var docs = collation.Find(filter).ToList();
                foreach (var doc in docs)
                {
                    pass = doc.GetValue("Pass").AsString;
                }
                if (pass != null)
                {
                    if (textBox2.Text == pass)
                    {
                        
                        //MessageBox.Show("Đăng nhập thành công!!!");
                        Client f = new Client();
                        f.Show();
                        
                        
                    }
                    else
                    {
                        MessageBox.Show("Tài khoản hoặc mật khẩu không đúng!!!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    DialogResult dialogResult = MessageBox.Show("Tài khoản nhập sai hoặc không tồn tại!!!\n Bạn có muốn đăng ký tài khoản ?", "Đăng ký", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dialogResult == DialogResult.Yes)
                    {
                        signup d = new signup();
                        d.Show();
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        
                    }
                }
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
