using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalliGalli_Server
{
    // 카드덱 클래스
    public class CardDeck
    {
        public Queue<Card> deck = new Queue<Card>(); // 수정: C# 7.3 호환성을 위해 명시적으로 형식 지정  

        public void MergeDeck(CardDeck otherDeck)
        {
            //Todo: 다른 카드 덱을 합치기  
            while (otherDeck.deck.Count > 0)
            {
                deck.Enqueue(otherDeck.deck.Dequeue());
            }
        }
        public Card DrawCard()
        {
            if (deck.Count == 0) return null;
            return deck.Dequeue();
        }
        public void AddCard(Card card)
        {
            //Todo: 자신의 덱에 카드를 추가함  
            if (card != null) deck.Enqueue(card);
        }
        public int GetCardCount()
        {
            //Todo: 저장된 카드 갯수를 호출  
            return deck.Count;
        }
    }
}
