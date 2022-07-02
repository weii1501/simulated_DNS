using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Diagnostics;

namespace DNSTest
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
            collation = Database.GetCollection<BsonDocument>("Database");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var d = new Resolve_Server();
            d.Show();
            ForeignName_Server n = new ForeignName_Server();
            n.Show();
            Name_Server h = new Name_Server();
            h.Show();
            var z = new Resolve_Server1();
            z.Show();
            button1.Enabled = false;
        }

        private void menu_Load(object sender, EventArgs e)
        {
           
        }

        
       
    }
}
