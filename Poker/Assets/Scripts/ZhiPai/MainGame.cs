using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZhiPai
{
    public class MainGame : MonoBehaviour
    {
        public static MainGame Instance;
        Transform cell_Backs, cell_Ups, cell_Stars;
        Transform DownCells;
        [HideInInspector] public Transform canvas;
        [HideInInspector] public bool canControl;
        Dictionary<PokerType, Transform> cell_TypeDic = new Dictionary<PokerType, Transform>();
        void Start()
        {
            InitPoker();
        }
        void InitPoker()
        {
            Instance = this;
            //获取格子
            cell_Backs = GameObject.Find("Cell_Backs").transform;
            cell_Ups = GameObject.Find("Cell_Ups").transform;
            cell_Stars = GameObject.Find("Cell_Stars").transform;
            DownCells = GameObject.Find("DownCells").transform;
            canvas = GameObject.Find("Canvas").transform;
            Cell_Type[] cell_Types = FindObjectsOfType<Cell_Type>();
            foreach (var item in cell_Types)
            {
                cell_TypeDic[item.pokerType] = item.transform;
            }

            //加载扑克模型,所有正面图片,背面图片
            Sprite[] ups = Resources.LoadAll<Sprite>("PokerSprites/Up");
            Sprite back = Resources.Load<Sprite>("PokerSprites/Back");
            Poker poker = Resources.Load<Poker>("PokerPrefabs/Poker");
            List<Poker> pokerList = new List<Poker>();
            for (int i = 0; i < ups.Length; i++)
            {
                PokerInfo info = new PokerInfo();
                info.up = ups[i];
                info.back = back;
                //根据图片名称后缀字母判断Poker花色
                switch (ups[i].name.Substring(ups[i].name.Length - 1))
                {
                    case "s":
                        info.pokerType = PokerType.Spade;
                        break;
                    case "h":
                        info.pokerType = PokerType.Heart;
                        break;
                    case "c":
                        info.pokerType = PokerType.Club;
                        break;
                    case "d":
                        info.pokerType = PokerType.Diamond;
                        break;
                }
                //根据图片名称前面数字判断Poker点数
                info.number = int.Parse(ups[i].name.Substring(0, ups[i].name.Length - 1));

                //创建扑克获取信息
                Poker pokerObj = Instantiate(poker);
                pokerObj.InitPoker(info);
                pokerList.Add(pokerObj);
            }
            //洗牌,将洗好的牌放到指定位置
            pokerList = ShuffleTheCards(pokerList);
            for (int i = 0; i < pokerList.Count; i++)
            {
                pokerList[i].transform.parent = cell_Stars;
                pokerList[i].transform.position = cell_Stars.position + new Vector3(0.5f, -0.5f, 0) * cell_Stars.childCount;
                pokerList[i].SetSprite(false);
            }
            GameObject.Find("StarBut").GetComponent<Button>().onClick.AddListener(() =>
            {
                StartCoroutine(GameBegin());
                GameObject.Find("ControlPanel").SetActive(false);
            });
        }
        IEnumerator GameBegin()
        {
            //将牌放到相应位置,先放下面
            for (int i = 0; i < DownCells.childCount; i++)
            {
                for (int j = i; j < DownCells.childCount; j++)
                {
                    yield return new WaitForSecondsRealtime(0.05f);
                    Transform poker = cell_Stars.GetChild(cell_Stars.childCount - 1);
                    StartCoroutine(PokerMoveToCell(poker, DownCells.GetChild(j).position + Vector3.down * 10 * i, 3000, DownCells.GetChild(j)));
                    //顶上的牌面朝上,可拖动
                    if (j == i)
                        poker.GetComponent<Poker>().SetSprite(true);
                }
            }
            //剩下的放上面牌堆
            while (cell_Stars.childCount > 0)
            {
                yield return new WaitForSecondsRealtime(0.02f);
                Transform poker = cell_Stars.GetChild(cell_Stars.childCount - 1);
                StartCoroutine(PokerMoveToCell(poker, cell_Backs.position + new Vector3(0.5f, -0.5f, 0) * cell_Backs.childCount, 3000, cell_Backs));
            }
            canControl = true;
        }
        List<Poker> ShuffleTheCards(List<Poker> _pokerList) //把扑克打乱顺序
        {
            List<Poker> tempPokers = new List<Poker>();
            while (_pokerList.Count > 0)
            {
                int randIndex = Random.Range(0, _pokerList.Count);
                tempPokers.Add(_pokerList[randIndex]);
                _pokerList.RemoveAt(randIndex);
            }
            return tempPokers;
        }
        public void OpenPoker() //翻上方牌堆的顶上牌
        {
            Transform topPoker = cell_Backs.GetChild(cell_Backs.childCount - 1);
            topPoker.position = cell_Ups.position + new Vector3(0.5f, -0.5f, 0) * cell_Ups.childCount;
            topPoker.parent = cell_Ups;
            //顶上的牌面朝上,可拖动
            topPoker.GetComponent<Poker>().SetSprite(true);
        }
        public void ResetCell_Backs() //牌堆的牌翻完后重置
        {
            if (cell_Backs.childCount == 0)
            {
                while (cell_Ups.childCount > 0)
                {
                    Transform topPoker = cell_Ups.GetChild(cell_Ups.childCount - 1);
                    topPoker.position = cell_Backs.position + new Vector3(0.5f, -0.5f, 0) * cell_Backs.childCount;
                    topPoker.parent = cell_Backs;
                    topPoker.GetComponent<Poker>().SetSprite(false);
                }
            }
        }
        public void QuickPlacement(Poker poker) //快速归入左上格子(双击触发)
        {
            if (poker.transform.childCount > 0) return; //牌串不能快速归入
            Transform cell = cell_TypeDic[poker.info.pokerType];
            if (poker.info.number - cell.childCount == 1)
            {
                StartCoroutine(PokerMoveToCell(poker.transform, cell.position, 5000));
                poker.transform.parent = cell;
                CheckWin();
            }
        }
        public void CheckWin() //检测是否胜利
        {
            int count = 0;
            foreach (var item in cell_TypeDic.Values)
            {
                count += item.childCount;
            }
            if (count >= 52)
            {
                canControl = false;
                GameObject.Find("WinPanel").transform.Find("Text").gameObject.SetActive(true);
            }
        }
        IEnumerator PokerMoveToCell(Transform poker, Vector3 pos, float speed, Transform parent = null)
        {
            poker.parent = canvas;
            while (true)
            {
                yield return null;
                poker.position = Vector3.MoveTowards(poker.position, pos, Time.deltaTime * speed);
                if ((poker.position - pos).sqrMagnitude < 0.1f)
                {
                    poker.position = pos;
                    if (parent != null)
                        poker.parent = parent;
                    break;
                }
            }
        }
    }
}
