using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Lab07_Bai03
{
    public partial class Form1 : Form
    {
        private static int pingstart, pingstop, elapsedtime;
        private static Thread pinger;

        private void button1_Click(object sender, EventArgs e)
        {
            pinger = new Thread(new ThreadStart(sendPing));
            pinger.IsBackground = true;
            pinger.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            pinger.Abort();
            this.Invoke((MethodInvoker)(() => listBox1.Items.Add("Ping stopped")));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            sock.Close();
            this.Close();
        }

        private Socket sock;
        public Form1()
        {
            InitializeComponent();
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 3000);
        }
        void sendPing()
        {
            IPHostEntry iphe = Dns.Resolve(textBox1.Text);
            IPEndPoint iep = new IPEndPoint(iphe.AddressList[0], 0);
            EndPoint ep = (EndPoint)iep;
            ICMP packet = new ICMP();
            int recv, i = 1;
            packet.Type = 0x08;
            packet.Code = 0x00;
            Buffer.BlockCopy(BitConverter.GetBytes(1), 0, packet.Message, 0, 2);
            byte[] data = Encoding.ASCII.GetBytes(textBox2.Text);
            Buffer.BlockCopy(data, 0, packet.Message, 4, data.Length);
            packet.MessageSize = data.Length + 4;
            int packetsize = packet.MessageSize + 4;
            this.Invoke((MethodInvoker)(() => listBox1.Items.Add("Pinging " + textBox1.Text)));
            while (true)
            {
                packet.Checksum = 0;
                Buffer.BlockCopy(BitConverter.GetBytes(i), 0, packet.Message, 2, 2);
                UInt16 chcksum = packet.getChecksum();
                packet.Checksum = chcksum;
                pingstart = Environment.TickCount;
                sock.SendTo(packet.getBytes(), packetsize, SocketFlags.None, iep);
                try
                {
                    data = new byte[1024];
                    recv = sock.ReceiveFrom(data, ref ep);
                    pingstop = Environment.TickCount;
                    elapsedtime = pingstop - pingstart;
                    this.Invoke((MethodInvoker)(() => listBox1.Items.Add("reply from: " + ep.ToString() + ", seq: " + i + ", time = " + elapsedtime + "ms")));
                }
                catch (SocketException)
                {
                    this.Invoke((MethodInvoker)(() => listBox1.Items.Add("no reply from host")));
                }
                i++;
                Thread.Sleep(3000);
            }
        }
    }
}
