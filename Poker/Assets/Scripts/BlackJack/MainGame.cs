using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZhiPai;
namespace BlackJack
{
    public class MainGame : MonoBehaviour
    {
        void Start()
        {
            Poker poker = Resources.Load<Poker>("BlackJack/PokerPrefab/Poker");

            Sprite[] sprite = Resources.LoadAll<Sprite>("BlackJack/PokerSprites/Poker");
            List<Poker> pokerList = new List<Poker>();
            for (int i = 0; i < sprite.Length - 1; i++)
            {
                int num = int.Parse(sprite[i].name.Split('_')[1]);
                PokerInfo info = new PokerInfo();
                info.pokerType = (PokerType)(num / 13);
                info.number = num % 13 + 1;

                print(info.pokerType + "-" + info.number);
            }
        }
    }
}