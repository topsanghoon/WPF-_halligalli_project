using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalliGalli_Server
{
    using System.Text.Json.Serialization;

    public class MessageServerToCli
    {
        [JsonPropertyName("id")]
        public int? PlayerId { get; set; }

        [JsonPropertyName("name")]
        public string PlayerName { get; set; } = "";

        [JsonPropertyName("turn")]
        public bool IsTurnActive { get; set; }
        //[JsonPropertyName("card")]
        //public Card Card { get; set; }

        // 펼쳐진 카드 목록 → JSON의 "카드정보"
        [JsonPropertyName("card")]
        public MessageCard[]? OpenCards { get; set; }

        [JsonPropertyName("user_status")]
        public int? UserState { get; set; }

        // 남은 카드 개수 배열 → JSON의 "남은카드개수"
        [JsonPropertyName("remaining_card_count")]
        public MessageCardCount[]? RemainingCardCounts { get; set; }

        public MessageServerToCli() { }

        public MessageServerToCli(int userState)
        {
            IsTurnActive = false;
            OpenCards = (!Table.Instance.gameStart)
                ? Array.Empty<MessageCard>()
                : Table.Instance.getMessageCards();
            UserState = userState;
            RemainingCardCounts = Table.Instance.gameStart ? Table.Instance.GetAllPlayerCardCounts() : Array.Empty<MessageCardCount>();
        }
        public MessageServerToCli(int playerId, string playerName, int userState)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            IsTurnActive = false;
            OpenCards = (!Table.Instance.gameStart)
                ? Array.Empty<MessageCard>()
                : Table.Instance.getMessageCards();
            UserState = userState;
            RemainingCardCounts = Table.Instance.gameStart ? Table.Instance.GetAllPlayerCardCounts() : Array.Empty<MessageCardCount>();


        }
        public MessageServerToCli(int playerId, string playerName, bool turn,int userState)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            IsTurnActive = turn;
            //Card = card;
            OpenCards = (!Table.Instance.gameStart || userState == 1 || userState == 2)
                ? Array.Empty<MessageCard>()
                : Table.Instance.getMessageCards();
            UserState = userState;
            RemainingCardCounts = Table.Instance.gameStart ? Table.Instance.GetAllPlayerCardCounts() : Array.Empty<MessageCardCount>();
        }
    }

    public class MessageCliToServer
    {
        public int id;
        public string name;
        public int key;
        public int? time_dif;
        public bool penalty; 

        public MessageCliToServer()
        {
            this.id = 0;
            this.name = "";
            this.key = 0;
            this.time_dif = 0;
            this.penalty = false;
        }

        public MessageCliToServer(int playerId, string playerName, int key, bool penalty)
        {
            this.id = playerId;
            this.name = playerName;
            this.key = key;
            this.penalty = penalty;
        }

        public MessageCliToServer(int playerId, string playerName, int key, int time_dif, bool penalty)
        {
            this.id = playerId;
            this.name = playerName;
            this.key = key;
            this.time_dif = time_dif;
            this.penalty = penalty;
        }
    }

    public class MessageCard
    {
        [JsonPropertyName("ID")]
        public int ID { get; set; }

        [JsonPropertyName("num")]
        public int Num { get; set; }

        public MessageCard(int id, int num)
        {
            ID = id;
            Num = num;
        }
    }

    public class MessageCardCount
    {
        [JsonPropertyName("ID")]
        public int ID { get; set; }

        [JsonPropertyName("card_count")]
        public int CardCount { get; set; }

        public MessageCardCount(int id, int cardCount)
        {
            ID = id;
            CardCount = cardCount;
        }
    }

}
