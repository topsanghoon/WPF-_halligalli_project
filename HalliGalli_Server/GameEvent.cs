using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalliGalli_Server
{
    public enum GameEvent
    {
        None = 0,
        WIN, //1
        LOSE, //2 
        PENALTY,//3
        ENTER,//4
        EXIT,//5
        DUP_NAME, //6
        GAME_START,//7
        GAME_WIN, //8
        GAME_LOSE //9
    }
    public enum Commend
    {
        K = 1,
        SPACE,
        P
    }
}
  
