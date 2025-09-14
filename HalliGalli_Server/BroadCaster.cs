using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HalliGalli_Server
{
    // 브로드캐스터

    public class Broadcaster
    {
        public static Broadcaster Instance { get; } = new Broadcaster();

        //public void SendJson<T>(T obj, NetworkStream stream)
        //{
        //    var options = new JsonSerializerOptions
        //    {
        //        IncludeFields = true, //  필드 포함
        //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // JSON 네이밍 정책
        //        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never,
        //        WriteIndented = true
        //    };

        //    string json = JsonSerializer.Serialize(obj, options);
        //    byte[] data = Encoding.UTF8.GetBytes(json);
        //    byte[] dataLength = BitConverter.GetBytes(data.Length);

        //    stream.Write(dataLength, 0, 4);
        //    stream.Write(data, 0, data.Length);
        //    stream.Flush();

        //    Console.WriteLine("보낸 JSON:\n" + json);
        //}

        public void SendJson<T>(T obj, NetworkStream stream, string name)
        {
            if (stream == null || !stream.CanWrite)
            {
                Console.WriteLine("스트림이 null이거나 쓰기 불가능");
                return;
            }

            string json = JsonSerializer.Serialize(obj);
            try
            {
                using (StreamWriter writer = new StreamWriter(stream, leaveOpen: true))
                {
                    writer.WriteLine(json);
                    writer.Flush();
                    Console.WriteLine(name+"에게 보낸 JSON: " + json);
                    Thread.Sleep(30);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("전송 실패: " + e.Message);
            }
        }

        // 수정된 BroadcastToAll 메서드
        public void BroadcastToAll(MessageServerToCli message)
        {

            foreach (var kvp in Table.Instance.players)
            {
                Player player = kvp.Value; // KeyValuePair에서 Player 객체를 가져옴
                SendJson(message, player.stream, player.username);
            }
        }
        public void BroadcastWinner(string winnerName)
        {
            foreach (var kvp in Table.Instance.players)
            {
                Player player = kvp.Value; // KeyValuePair에서 Player 객체를 가져옴

                if (player.playerId == null) return;
                int userState = 0;
                if (player.username.Equals(winnerName))
                {
                    userState = 1;
                }
                else
                {
                    userState = 2;
                    // Handle other cases if necessary
                }
                // Explicitly cast 'int?' to 'int' after checking for null
                MessageServerToCli msg = new MessageServerToCli(
                    player.playerId, // Fix: Use .Value to access the underlying int
                    player.username,
                    (player.playerId==Table.Instance.currentTurnPlayerId),
                    userState
                );

                
                SendJson(msg, player.stream, player.username);
            }
        }

        // 처음 입장했을 때 이미 참여한 사람들 정보를 뿌림
        public void BroadcastEnternece(Player target)
        {
            Console.WriteLine("타겟 플레이어: "+target.username);
            foreach (var kvp in Table.Instance.players)
            {
                Player player = kvp.Value; // KeyValuePair에서 Player 객체를 가져옴
                MessageServerToCli message = new MessageServerToCli(
                        player.playerId,
                        player.username,
                        4
                    );
                
                SendJson(message, target.stream, target.username);
                Thread.Sleep(500);
            }
            Console.WriteLine("---끝---");
        }


        public void BroadcastNextTurn(MessageCard[] messageCards, int currentTurnPlayerId)
        {
            foreach (var kvp in Table.Instance.players)
            {
                Player player = kvp.Value; // KeyValuePair에서 Player 객체를 가져옴

                MessageServerToCli msg = new MessageServerToCli
                {
                    PlayerId = player.playerId,
                    PlayerName = player.username,
                    IsTurnActive = (player.playerId == currentTurnPlayerId),
                    OpenCards = messageCards,
                    UserState = 0,
                    RemainingCardCounts = Table.Instance.GetAllPlayerCardCounts()
                };

                if (player.playerId == currentTurnPlayerId)
                {
                    msg.IsTurnActive = true;
                }
                else
                {
                    msg.IsTurnActive = false;
                }

                SendJson(msg, player.stream, player.username);
            }
        }
    }
}
