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
using System.IO;
using System.Threading;

namespace TCPClient
{
    public partial class Form1: Form
    {
        TcpClient Client;

        NetworkStream Stream;
        StreamReader Reader;
        StreamWriter Writer;

        Thread receiveThread;

        bool Connected;

        private delegate void AddTextDelegate(string strText);
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string IP = "127.0.0.1";
            int port = 9001;

            Client = new TcpClient(); // Client 소켓은 1개 (서버는 2개)
            Client.Connect(IP, port);

            // 접속이 되었다면
            Stream = Client.GetStream();
            Connected = true;

            txtView.AppendText("서버에 접속 성공!" + Environment.NewLine);

            Reader = new StreamReader(Stream);
            Writer = new StreamWriter(Stream);

            // 데이터 수신을 위한 스레드 (언제올지 모르니 대기)
            ThreadStart ts = new ThreadStart(Receive);
            Thread rcvthread = new Thread(ts);
            rcvthread.Start();
        }

        private void Receive()
        {
            AddTextDelegate AddText = new AddTextDelegate(txtView.AppendText);
            // 무한루프
            while (Connected)
            {
                if (Stream.CanRead)
                {
                    string tempStr = Reader.ReadLine();
                    if (tempStr.Length > 0)
                    {
                        Invoke(AddText, "상대방 : " + tempStr + Environment.NewLine); // 상대방 메시지는 temp 안에 있음
                    }
                }
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            txtView.AppendText("나 : " + txtInput.Text + Environment.NewLine); // 스레드 사용 안 함
            Writer.WriteLine(txtInput.Text); // 내가 보내는거니까 Writer 객체 사용. 보내기
            Writer.Flush(); // Flush란 밀어버리는 용도. Buffer를 밀어서 깨끗하게 만들기. 버퍼에 저장된 데이터를 강제로 내보냄.
            txtInput.Clear(); // 내가 쓴 텍스트 정리
        }
    }
}
