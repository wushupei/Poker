using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
namespace ZhiPai
{
    public class Cell_Backs : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData) //弹起时调用
        {
            MainGame.Instance.ResetCell_Backs();
        }
    }
}