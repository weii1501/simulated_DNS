using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace meone
{
    public partial class signup : Form
    {
        private MongoClient dbclient;
        private IMongoDatabase Database;
        private IMongoCollection<BsonDocument> collation;
        public signup()
        {
            InitializeComponent();
            dbclient = new MongoClient("mongodb+srv://meone:letpk9741@cluster0.vye0x.mongodb.net/DNS?retryWrites=true&w=majority");
            Database = dbclient.GetDatabase("DNS");
            collation = Database.GetCollection<BsonDocument>("Client_List");
        }
        void add()
        {
            BsonDocument userData = new BsonDocument()
                        .Add("User", usertext.Text)
                        .Add("Pass", passtext.Text);
            collation.InsertOne(userData);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(usertext.Text) || string.IsNullOrWhiteSpace(passtext.Text) || string.IsNullOrWhiteSpace(confirmpasstext.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!!!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                try
                {
                    string user = null;
                    var builder = Builders<BsonDocument>.Filter;
                    var filter = builder.Eq("User", usertext.Text);
                    var docs = collation.Find(filter).ToList();
                    foreach (var doc in docs)
                    {
                        user = doc.GetValue("User").AsString;
                    }
                    if (user == null)
                    {
                        if (passtext.Text == confirmpasstext.Text)
                        {
                            add();
                            Close();
                            MessageBox.Show("Đăng ký thành công!!!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                            MessageBox.Show("Nhập lại mật khẩu!!!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                        MessageBox.Show("Dữ liệu đã tồn tại trong database");
                }
                catch (Exception w)
                {
                    MessageBox.Show(w.ToString());
                }
            }
        }
    }
}
