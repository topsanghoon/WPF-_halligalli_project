using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HalliGalli_Server
{
    // 플레이어 클래스
    public class Player
    {
        public CardDeck cardDeck = new CardDeck();
        public int playerId;
        public string username = "없음";
        public bool isAlive = true;
        public bool isTurn = false;

        public TcpClient tcpClient;
        public NetworkStream stream;
        public StreamReader reader;
        public StreamWriter writer;
        public Card frontCard { get; set; }

        public Player(int id, NetworkStream stream, TcpClient client)
        {
            this.playerId = id;
            this.stream = stream;
            this.reader = new StreamReader(stream);
            this.writer = new StreamWriter(stream); 
            this.tcpClient = client;
            frontCard = new Card();
            //Todo: 유저이름 입력받기(회의 필요)
        }

        public void ReceiveUserInfo(MessageCliToServer msg)
        {
            // 유저정보를 수신함
            switch (msg.key)
            {
                case 1: // 
                    PlayCard(msg);
                    break;
                case 2:
                    RingBell(msg);
                    break;
            }
        }

        private void PlayCard(MessageCliToServer msg)
        {
            // Json으로 받은 정보가 카드 내기일 경우
            //if (!isTurn)
            //{
            //    return;
            //}
            Table.Instance.PlayCard(msg.name); // 테이블에서 카드 내기 로직 호출
        }
        private void RingBell(MessageCliToServer msg)
        {

            if (msg.penalty == true)
            {
                Table.Instance.ApplyPenalty(msg.name);
                Broadcaster.Instance.BroadcastToAll(new MessageServerToCli(3));
                return;
            }
            if (msg.time_dif == null) return;

            //Todo: 타임스탬프 밀리초단위 시간차 받아서
            int value = (int)msg.time_dif;

            Table.Instance.bell.Ring(msg.name, value);
            // Json으로 받은 정보가 종 울리기일 경우
        }

        public T ReceiveJson<T>()
        {
            string str = reader.ReadLine();
            Console.WriteLine("받은 json: "+str);
            try
            {
                var options = new JsonSerializerOptions
                {
                    IncludeFields = true,
                    PropertyNameCaseInsensitive = true
                };

                T data = JsonSerializer.Deserialize<T>(str,options);
                Console.WriteLine("=== 수신된 원본 JSON ===");
                Console.WriteLine(data.ToString());
                Console.WriteLine("========================");
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine("파싱 실패: " + ex.Message);
                return default(T); // Ensure all code paths return a value
            }
        }

        //public T ReceiveJson<T>()
        //{
        //    byte[] lengthBytes = new byte[4];
        //    stream.Read(lengthBytes, 0, 4);
        //    int length = BitConverter.ToInt32(lengthBytes, 0);

        //    byte[] data = new byte[length];
        //    int readBytes = 0;
        //    while (readBytes < length)
        //    {
        //        int r = stream.Read(data, readBytes, length - readBytes);
        //        if (r == 0) throw new IOException("Disconnected");
        //        readBytes += r;
        //    }

        //    string json = Encoding.UTF8.GetString(data);

        //    //테스트
        //    Console.WriteLine("=== 수신된 원본 JSON ===");
        //    Console.WriteLine(json);
        //    Console.WriteLine("========================");

        //    return JsonSerializer.Deserialize<T>(json);
        //}



        
    }

}
