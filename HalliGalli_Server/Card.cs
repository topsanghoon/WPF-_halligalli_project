using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalliGalli_Server
{
    // 카드 클래스
    public class Card
    {
        public string fruitType;
        public int count;

        public Card()
        {
            this.fruitType = "없음";
            this.count = 0;
        }
        public Card(string fruitType, int count)
        {
            this.fruitType = fruitType;
            this.count = count;
        }
     
        public int getNum()
        {
            int res = 0;
            switch (fruitType) {
                case "apple":
                    res = 0;
                    break;
                case "banana":
                    res = 5;
                    break;
                case "grape":
                    res = 10;
                    break;
                case "watermelon":
                    res = 15;
                    break;
                
                default:
                    res = -5;
                    break;
            
            }
            res += count;
            return res;
        }
        

    }
}
