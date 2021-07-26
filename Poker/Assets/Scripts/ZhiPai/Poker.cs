using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace ZhiPai
{
    public class Poker : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerDownHandler
    {
        public PokerInfo info;
        Image image;
        Transform oriParent;
        Vector3 oriPosition;
        float lastTime = 0; //双击计时器
        public void InitPoker(PokerInfo _info)
        {
            info = _info;
            image = GetComponent<Image>();
        }
        public void SetSprite(bool _isUp)
        {
            if (_isUp)
            {
                image.sprite = info.up;
                name = info.pokerType + "-" + info.number;
            }
            else
            {
                image.sprite = info.back;
                name = "???";
            }
        }

        public void OnBeginDrag(PointerEventData eventData) //拖拽开始时调用一次
        {
            if (!MainGame.Instance.canControl) return;
            //面朝上且最顶上的牌才能拖动
            if (image.sprite == info.back || transform.GetSiblingIndex() != transform.parent.childCount - 1) return;

            //记录原父物体和坐标
            oriParent = transform.parent;
            oriPosition = transform.position;
            //如果身上有连续的牌,则将身上的牌都设为自身子物体,方便拖动
            int index = transform.GetSiblingIndex();
            int count = transform.parent.childCount - 1 - index;
            for (int i = 0; i < count; i++)
            {
                transform.parent.GetChild(index + 1).parent = transform;
            }
            //直接成为Canvas的子物体,排列在所有其它其物体之下
            transform.SetParent(MainGame.Instance.canvas);
        }
        public void OnDrag(PointerEventData eventData) //拖拽时持续调用
        {
            if (!MainGame.Instance.canControl) return;
            if (image.sprite == info.back || transform.GetSiblingIndex() != transform.parent.childCount - 1) return;

            transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData) //拖拽结束时调用一次
        {
            if (!MainGame.Instance.canControl) return;
            if (image.sprite == info.back || transform.GetSiblingIndex() != transform.parent.childCount - 1) return;

            GetIntoSlot();
        }

        public void OnPointerClick(PointerEventData eventData) //弹起时调用
        {
            if (!MainGame.Instance.canControl) return;
            //点击上方牌堆顶上的牌进行翻牌
            if (transform.GetComponentInParent<Cell_Backs>() && transform.GetSiblingIndex() == transform.parent.childCount - 1)
                MainGame.Instance.OpenPoker();
            //点击下方牌堆顶上的牌进行翻牌
            if (transform.GetComponentInParent<Cell_Common>() && transform.GetSiblingIndex() == transform.parent.childCount - 1)
                SetSprite(true);
        }

        public void OnPointerDown(PointerEventData eventData) //点击时调用
        {
            if (!MainGame.Instance.canControl) return;
            if (transform.GetSiblingIndex() != transform.parent.childCount - 1) return;

            if (Time.realtimeSinceStartup - lastTime < 0.2f)
            {
                MainGame.Instance.QuickPlacement(this);
                lastTime = 0;
                return;
            }
            lastTime = Time.realtimeSinceStartup;
        }
        void GetIntoSlot() //放入小格子
        {
            List<Transform> slots = new List<Transform>(); //可拖入的格子或Poker
                                                           //在左上格子中找寻可拖入位置(左上格子和牌堆不允许放入一串牌)
            if (transform.childCount == 0)
            {
                Cell_Type[] cell_Type = FindObjectsOfType<Cell_Type>();
                foreach (var item in cell_Type)
                {
                    //如果是空格子,且拖动的牌只有一张
                    if (item.transform.childCount == 0)
                    {
                        //如果花色对对上,且点数是A,表示为可放入的格子
                        if (item.pokerType == info.pokerType && info.number == 1)
                            slots.Add(item.transform);
                    }
                    else //不是空格子就找到顶上的牌判断是否可放入
                    {
                        Poker topPoker = item.transform.GetChild(item.transform.childCount - 1).GetComponent<Poker>();
                        //如果花色相同,且点数是连续的,则表示可放入
                        if (info.pokerType == topPoker.info.pokerType && info.number - topPoker.info.number == 1)
                            slots.Add(topPoker.transform);
                    }
                }
            }
            //在下方格子中找寻可拖入位置
            Cell_Common[] cell_Common = FindObjectsOfType<Cell_Common>();
            foreach (var item in cell_Common)
            {
                //如果是空格子
                if (item.transform.childCount == 0)
                {
                    //如果点数是K,表示为可放入的格子
                    if (info.number == 13)
                        slots.Add(item.transform);
                }
                else //不是空格子就找到顶上的牌判断是否可放入
                {
                    Transform topPoker = item.transform.GetChild(item.transform.childCount - 1);
                    Poker poker = GetTopPoker(topPoker).GetComponent<Poker>();
                    //如果牌面朝上,花色相反,且点数是连续的,则表示可放入
                    if (poker.image.sprite == poker.info.up && (int)info.pokerType % 2 != (int)poker.info.pokerType % 2 && poker.info.number - info.number == 1)
                        slots.Add(poker.transform);
                }
            }
            //遍历所有可放置位置
            for (int i = 0; i < slots.Count; i++)
            {
                //获取当前位置
                RectTransform slot = slots[i].GetComponent<RectTransform>();
                //如果鼠标进入该位置的范围
                if (slot.rect.Contains(Input.mousePosition - slot.position))
                {
                    if (slot.GetComponent<Poker>()) //如果是Poker
                    {
                        //如果是下面的Poker,则需要隔出距离,认上一张牌作父物体,方便拖动
                        if (slots[i].GetComponentInParent<Cell_Common>())
                        {
                            transform.position = slots[i].position + Vector3.down * 40;
                            transform.SetParent(slots[i]);
                        }
                        else //左上的Poker则直接覆盖位置,和上一张牌在一个层级
                        {
                            transform.position = slots[i].position;
                            transform.SetParent(slots[i].parent);
                        }
                    }
                    else //如果是格子
                    {
                        transform.SetParent(slots[i]); //将图片放入该小格子
                        transform.localPosition = Vector3.zero; //居中
                    }
                    MainGame.Instance.CheckWin();
                    return; //立刻结束该方法的调用
                }
            }
            //返回原位置
            transform.SetParent(oriParent);
            transform.position = oriPosition;
        }
        Transform GetTopPoker(Transform topPoker) //获取牌串最顶上的牌(最里层子物体)
        {
            if (topPoker.childCount > 0)
                return GetTopPoker(topPoker.GetChild(0));
            else
                return topPoker;
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
