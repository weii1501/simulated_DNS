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

namespace DNSTest
{
    public partial class client : Form
    {
        public client()
        {
            InitializeComponent();
        }
        void send()
        {
            

            // yêu cầu người dùng nhập ip của server
            
            // chuyển đổi chuỗi ký tự thành object thuộc kiểu IPAddress
            var serverIp = IPAddress.Parse("127.0.0.1");

            // yêu cầu người dùng nhập cổng của server
            
            // chuyển chuỗi ký tự thành biến kiểu int
            var serverPort = int.Parse("5000");

            // đây là "địa chỉ" của tiến trình server trên mạng
            // mỗi endpoint chứa ip của host và port của tiến trình
            var serverEndpoint = new IPEndPoint(serverIp, serverPort);

            var size = 1024; // kích thước của bộ đệm
            var receiveBuffer = new byte[size]; // mảng byte làm bộ đệm            

            
                // yêu cầu người dùng nhập một chuỗi
                
                var text = textBox1.Text;

                // khởi tạo object của lớp socket để sử dụng dịch vụ Udp
                // lưu ý SocketType của Udp là Dgram (datagram) 
                var socket = new Socket(SocketType.Dgram, ProtocolType.Udp);

                // biến đổi chuỗi thành mảng byte
                var sendBuffer = Encoding.ASCII.GetBytes(text);
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
                var result = Encoding.ASCII.GetString(receiveBuffer, 0, length);
                // xóa bộ đệm (để lần sau sử dụng cho yên tâm)
                Array.Clear(receiveBuffer, 0, size);

                // đóng socket và giải phóng tài nguyên
                socket.Close();

                // in kết quả ra màn hình
                richTextBox1.Text += result + "\n";
            
        } 
        private void button1_Click(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            Thread m = new Thread(new ThreadStart(send));
            m.Start();
        }
    }
}
