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
    public partial class History : Form
    {
        private MongoClient dbclient;
        private IMongoDatabase Database;
        private IMongoCollection<BsonDocument> collotion;
        public History()
        {
            InitializeComponent();
            dbclient = new MongoClient("mongodb+srv://meone:letpk9741@cluster0.vye0x.mongodb.net/DNS?retryWrites=true&w=majority");
            Database = dbclient.GetDatabase("DNS");
            collotion = Database.GetCollection<BsonDocument>("TimeLine");
            this.dataGridView1.DefaultCellStyle.Font = new Font("Tahoma", 11);
        }

        private void daily_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            Thread m = new Thread(new ThreadStart(showdatabase1));
            m.Start();
        }
        void showdatabase1()
        {
            var documents = collotion.Find(new BsonDocument()).ToList();
            //tao table
            dataGridView1.Refresh();
            dataGridView1.DataSource = null;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            DataTable dt = new DataTable();
            dt.Columns.Add("Time");
            dt.Columns.Add("IP");
            dt.Columns.Add("Domain");
            DataRow row = dt.NewRow();
            //them data
            foreach (BsonDocument doc in documents)
            {
                dt.Rows.Add(doc["Time"], doc["Ip"], doc["Dn"]);
            }
            //them vao gridview
            dataGridView1.DataSource = dt;
        }
    }
}
