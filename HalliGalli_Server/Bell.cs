using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalliGalli_Server
{

    public class PlayerName_TimeDIf
    {
        public string PlayerName { get; set; }
        public int TimeDif { get; set; }

        public PlayerName_TimeDIf()
        {
            this.PlayerName = "없음";
            this.TimeDif = -1;
        }

        public PlayerName_TimeDIf(string playerName, int timeDif)
        {
            this.PlayerName = playerName;
            this.TimeDif = timeDif;
        }
    }

    // 종 클래스
    public class Bell
    {
        //public bool isActive = false; // 과일 5개가 모였을 때 활성화
                                      // 활성화가 되지 않으면 패널티
                                      // 활성화가 되어있으면 버퍼에 정보저장 후 승리자 판별

        public bool isDeciding = false;

        public List<PlayerName_TimeDIf> TimeDifList;

        private readonly object lockObj = new();

        public Bell()
        {
            this.TimeDifList = new List<PlayerName_TimeDIf>();
        }

        public void Ring(string playerName, int timeDif)
        {

            if (!isDeciding)
            {
                isDeciding = true;
                new Thread(new ThreadStart(StartDecision)).Start();
            }
            TimeDifList.Add(new PlayerName_TimeDIf(playerName, timeDif));
        }

        public void StartDecision()
        {

            Thread.Sleep(3500);
            string winner = DecideWinner();
            if (winner == "없음")
            {
                Broadcaster.Instance.BroadcastToAll(new MessageServerToCli(9)); // 9 -> 우승자없음
            }

            // 쓰레드 종료
            TimeDifList.Clear();
            foreach(var kvs in Table.Instance.players)
            {
                Player ply = kvs.Value;
                ply.frontCard = new Card();
            }
            isDeciding = false;

            //Todo: 테이블 지움
            Broadcaster.Instance.BroadcastWinner(winner);
            Table.Instance.MergeDeck(winner);
            Table.Instance.CheckWinner(winner);

            return;
        }

        public string DecideWinner()
        {
            int MinDif = int.MaxValue;
            string CurWinner = "없음";

            foreach(PlayerName_TimeDIf pt in TimeDifList)
            {
                if(pt.TimeDif < MinDif)
                {
                    MinDif = pt.TimeDif;
                    CurWinner = pt.PlayerName;
                }
            }


            return CurWinner;
        }
    }
}
