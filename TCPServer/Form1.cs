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

namespace TCPServer
{
    public partial class Form1: Form
    {
        TcpListener Server; // 서버 소켓 생성
        TcpClient Client; // 클라이언트 소켓 생성

        NetworkStream Stream; // 소켓이 연결되고 나서 서버와 클라이언트가 데이터를 주고받기 위함
        StreamReader Reader;
        StreamWriter Writer;

        // 데이터가 언제 들어올지 모르니 대기상태로. 무한반복하면 비효율적. 스레드 사용.
        Thread receiveThread;

        bool Connected; // 연결 여부 확인 변수 선언

        private delegate void AddTextDelegate(string strText); // 출력하고 싶은 문자열을 넣으면 delegate의 타입과 동일한 메소드가 동작할 것임.
        public Form1()
        {
            InitializeComponent();
        }

        private void btnSend_Click(object sender, EventArgs e) // 내가 보내는 경우
        {
            txtView.AppendText("나 : " + txtInput.Text + Environment.NewLine); // 스레드 사용 안 함
            Writer.WriteLine(txtInput.Text); // 내가 보내는거니까 Writer 객체 사용. 보내기
            Writer.Flush(); // Flush란 밀어버리는 용도. Buffer를 밀어서 깨끗하게 만들기. 버퍼에 저장된 데이터를 강제로 내보냄.
            txtInput.Clear(); // 내가 쓴 텍스트 정리
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 서버 창이 열리면 스레드 먼저 동작하게끔
            ThreadStart ts = new ThreadStart(Listen); // 스레드로 돌릴 메소드 등록
            Thread thread = new Thread(ts);
            thread.Start();
        }

        // 스레드로 사용할 임의의 함수(메소드)
        private void Listen() // 스레드를 돌면서 외부의 어떤 클라이언트로부터 접속 대기
        {
            AddTextDelegate AddText = new AddTextDelegate(txtView.AppendText); // 텍스트박스에 텍스트 추가

            // 소켓 생성
            IPAddress addr = new IPAddress(0);
            int port = 9001; // 포트 번호는 1000 이상부터는 자유롭게

            Server = new TcpListener(addr, port); // 생성 및 바인딩. 서버가 받아줌.
            Server.Start(); // 서버 시작

            // 메인 텍스트 뷰로 메시지 전달(충돌 위험때문에 위에 delegate 생성)
            Invoke(AddText, "서버 시작" + Environment.NewLine); // 서버 시작 출력 후 개행

            Client = Server.AcceptTcpClient(); // 연결 수락

            Connected = true; // 연결 성공

            Invoke(AddText, "클라이언트와 연결" + Environment.NewLine);

            // 이제 서버와 클라이언트 서로 데이터 주고 받기
            Stream = Client.GetStream(); // 얘를 가지고 통신할 것임. 데이터 주고받을 준비 완료
            Reader = new StreamReader(Stream);
            Writer = new StreamWriter(Stream);

            // 데이터 수신을 위한 스레드 (언제올지 모르니 대기)
            ThreadStart ts = new ThreadStart(Receive); // 수신을 위한 Receive 라는 메소드 생성 (Listen 말고)
            Thread rcvthread = new Thread(ts);
            rcvthread.Start();
        }

        private void Receive()
        {
            AddTextDelegate AddText = new AddTextDelegate(txtView.AppendText);
            // 여기서는 무한루프를 돌아야 함
            while(Connected)
            {
                if(Stream.CanRead) // Stream이 읽을 수 있는 상태가 됐다면
                {
                    string temp = Reader.ReadLine(); // 무조건 string으로 들어옴
                    if(temp.Length > 0) // 0이라면 데이터가 없다는 뜻 -> 끝까지 보냈다는 뜻. 1 이상은 문자가 있다는 뜻.
                    {
                        Invoke(AddText, "상대방 : " + temp + Environment.NewLine); // 상대방 메시지는 temp 안에 있음
                    }
                }
            }
        }
    }
}
