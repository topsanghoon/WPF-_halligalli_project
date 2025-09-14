using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HalliGalli_Server
{
    // 관리 클래스 (싱글톤)
    public class Sk_Manager
    {
        public static Sk_Manager Instance { get; } = new Sk_Manager();

        private int maxThread;
        private int currentThread;
        private object server;

        public void AddUser() { }
        public void CheckUserAvailable() { }
        public void RemoveUser(int playerId) { }
    }

    // 브로드캐스터
    public class Sk_Broadcaster
    {
        public void SendJson(int playerId, string message) { }

        public void SendJson(int playerId, bool isTurnActive, Sk_Card card, int userState, Sk_Card[] openedCards) { }
    }

    // 테이블 클래스
    public class Sk_Table
    {
        public List<Sk_Player> players = new();
        public List<Sk_Card> openedCards = new();
        public Dictionary<string, int> fruitCardCount = new();

        public Queue<Sk_Card> tableDeck = new();
        public int currentTurnPlayerId;
        public bool isActive;
        public Sk_Bell bell = new();

        public void StartGame() { }
        public void PlayCard(int playerId) { }
        public Sk_Card[] ShowCurrentResult() { return null; }
        public void MergeDeck(int playerId) { }
        public void MoveTurn() { }
        public void ApplyPenalty(int playerId) { }
        public void PlayerDeath(int playerId) { }
        public int CheckWinner() { return -1; }
    }

    // 카드 클래스
    public class Sk_Card
    {
        public string fruitType;
        public int count;

        public Sk_Card(string fruitType, int count)
        {
            this.fruitType = fruitType;
            this.count = count;
        }
    }

    // 카드덱 클래스
    public class Sk_CardDeck
    {
        public Queue<Sk_Card> deck = new();

        public void MergeDeck(Sk_CardDeck otherDeck) { }
        public Sk_Card DrawCard() { return null; }
        public void AddCard(Sk_Card card) { }
        public int GetCardCount() { return deck.Count; }
    }

    // 플레이어 클래스
    public class Sk_Player
    {
        public Sk_CardDeck deck = new();
        public int playerId;
        public string username;
        public bool isAlive = true;
        public bool isTurn = false;

        public TcpClient tcpClient;
        public StreamReader reader;
        public StreamWriter writer;

        public void ReceiveUserInfo() { }
        public void ReceiveJson() { }
        public void PlayCard() { }
        public void RingBell() { }
    }

    // 종 클래스
    public class Sk_Bell
    {
        public bool isActive = false;
        public Dictionary<int, double> buffer = new();

        public void Ring(int playerId) { }
        public void Activate() { isActive = true; }
        public void Deactivate() { isActive = false; }
        public int DecideWinner() { return -1; }
    }
}
