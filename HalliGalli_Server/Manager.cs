using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

//1. 승리, 2.패배, 3.패널티 4.입장  5.퇴장 6.이름 중복 7.게임시작 




namespace HalliGalli_Server
{
    // 관리 클래스 (싱글톤)
    public class Manager
    {
        private readonly object threadLock = new object();
        private const int MAX_THREAD = 4;
        private int currentThreadCount = 0;
        TcpListener server;
        bool gamestart = false;

        bool[] idBox = {false,false,false,false};

        private Manager()
        {

            server = new TcpListener(IPAddress.Any, 1234);

            Console.WriteLine("서버 시작");
            server.Start();
        }
        public static Manager Instance { get; } = new Manager();


        public void Init()
        {

            Thread thread = new Thread(new ThreadStart(ServerOpen));
            thread.Start();

        }

        public void ServerOpen()
        {
            while (true)
            {

                TcpClient client = server.AcceptTcpClient();
                if (client != null && CheckUserAvailable())
                {
                    Thread ClientThread = new Thread(new ParameterizedThreadStart(UserTCPStart));
                    lock (threadLock)
                    {
                        currentThreadCount++;
                    }
                    ClientThread.Start(client);

                }
            }
        }
        public void UserTCPStart(object obj) // 유저 추가시 
        {
            TcpClient Client = (TcpClient)obj;
            NetworkStream stream = Client.GetStream();

            int playerId = getPlayerId();

            Player player = new Player(playerId, stream, Client); 
            //MessageCliToServer msg = player.ReceiveJson<MessageCliToServer>();
            MessageCliToServer msg = player.ReceiveJson<MessageCliToServer>();
            if (msg == null)
            {
                Console.WriteLine("초기 메시지 수신 실패, 연결 종료");
                return; 
            }
            try
            {
                //Console.WriteLine("msg.id: "+msg.id+"msg.name:"+msg.name+"msg.key:"+msg.key);
                if (string.IsNullOrWhiteSpace(msg.name))
                    throw new Exception("이름이 비어 있습니다.");

                if (Table.Instance.players.ContainsKey(msg.name))
                    throw new Exception("이름 중복");
                player.username = msg.name;

                Table.Instance.AddPlayer(player); // 수정
                MessageServerToCli callBack = new MessageServerToCli(
                    player.playerId,
                    player.username,
                    4
                 );
                Console.WriteLine(callBack.ToString());

                Broadcaster.Instance.BroadcastToAll(callBack);
                Broadcaster.Instance.BroadcastEnternece(player);

                while (true)
                {

                    msg = player.ReceiveJson<MessageCliToServer>();
                    if (msg != null) {

                        if(!gamestart && msg.key == 3) // p->3
                        {
                            gamestart = true;
                            Table.Instance.StartGame();
                            continue;
                        }

                        if (!gamestart) continue;
                        player.ReceiveUserInfo(msg);// 메세지를 받았을때 로직을 처리
                                                    // 상태 기준 판단
                                                    // 상대가 누른 키를 기준으로 판단


                        // 테스트(쓰레기값 주기)
                        // Broadcaster.Instance.BroadcastToAll(new MessageServerToCli());
                    }


                }
            }
            catch (IOException)
            {
                Console.WriteLine($"{player.playerId} 연결 종료");
                Broadcaster.Instance.BroadcastToAll(new MessageServerToCli(player.playerId, player.username, 9)); // 9 -> 퇴장
                RemoveUser(player);
            }
            catch (Exception e)
            {
                if(e.Message=="이름 중복")
                {
                    Broadcaster.Instance.BroadcastToAll(new MessageServerToCli(6)); // 6 -> 이름 중복
                }else if (e.Message == "낼 카드가 없음")
                {
                    
                } else
                {
                    Console.WriteLine("유저 연결: 예상치 못한 오류" + e.Message);
                }
            }

        }
        public bool CheckUserAvailable()
        {
            lock (threadLock)
            {
                return (!gamestart && currentThreadCount < MAX_THREAD);
            }

        }
        public void RemoveUser(Player player)
        {

            lock (threadLock)
            {
                idBox[player.playerId] = false;
                currentThreadCount--;
            }

            Table.Instance.PlayerDeath(player.username);
            Table.Instance.players.Remove(player.username);
            player.stream.Close();
            player.tcpClient.Close();
        }

        private int getPlayerId()
        {
            int playerid = 0;
            for (int i = 0; i < 4; i++)
            {
                if (!idBox[i])
                {
                    lock (threadLock)
                    {
                        idBox[i] = true;
                    }
                    playerid = i;
                    break;
                }
            }
            return playerid;
        }

        
    }
}
