using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
namespace BlackJack
{
    public class Poker : MonoBehaviour, IPointerDownHandler
    {
        [HideInInspector] public PokerInfo info;
        public void InitPoker(PokerInfo _info)
        {
            info = _info;
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            print(info.pokerType + "-" + info.number);
        }
    }
    public class PokerInfo
    {
        public Sprite up;
        public Sprite back;
        public PokerType pokerType;
        public int number;
    }
    public enum PokerType
    {
        Spade,
        Heart,
        Club,
        Diamond
    }
}