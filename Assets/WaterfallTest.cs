using UnityEngine;
using System.Collections;

namespace com.tencent.pandora
{
    public class WaterfallTest : MonoBehaviour
    {
        public UIButton clearButton;
        public UIButton refill;

        private WaterfallScrollViewHelper_NGUI _waterfall;

        // Use this for initialization
        void Start()
        {
            EventDelegate.Add(clearButton.onClick, WaterFallClear);
            EventDelegate.Add(refill.onClick, ReFill);

            _waterfall = transform.Find("WaterfallScrollView").GetComponent<WaterfallScrollViewHelper_NGUI>();
            RunWaterfall();
        }

        private void RunWaterfall()
        {
            _waterfall.ItemCount = 5;
            _waterfall.onReachTop.AddListener(OnReachTop);
            _waterfall.onReachBottom.AddListener(OnReachBottom);
            _waterfall.SetItemFillDelegate(FillItem);
            _waterfall.SetItemRecycleDelegate(RecycleItem);
            _waterfall.Fill();
        }

        private void FillItem(GameObject item, int index)
        {
            Debug.Log("FillItem ...");
            //绑定按钮事件
            Transform itemTrans = item.transform;
            GameObject switchButton = itemTrans.Find("Button").gameObject;
            UIEventListener.Get(switchButton).onClick = SwitchTextDisplayState;

        }

        private void SwitchTextDisplayState(GameObject buttonGameobject)
        {
            Transform buttonTrans = buttonGameobject.transform;
            GameObject textGameobject = buttonTrans.parent.Find("Label").gameObject;
            if (textGameobject.activeInHierarchy == true)
            {
                textGameobject.SetActive(false);
            }
            else
            {
                textGameobject.SetActive(true);
            }

            string name = buttonTrans.parent.name;
            int startIndex = name.IndexOf("_");
            string index = name.Substring(startIndex + 1);
            _waterfall.RefreshLayout(System.Convert.ToInt32(index));
        }
        private void RecycleItem(GameObject item)
        {
            Debug.Log("RecycleItem ...");

        }

        private void OnReachTop()
        {
            PrintColorLog("aa2200", "OnReachTop...");
        }

        private void OnReachBottom()
        {
            PrintColorLog("0022ff", "OnReachBottom...");
            AddItemCount();
        }

        private void PrintColorLog(string color, string logContent)
        {
            Debug.Log("<color=#" + color + ">" + logContent + "</color>");
        }

        private void AddItemCount()
        {
            _waterfall.ItemCount = 10;
        }

        private void WaterFallClear()
        {
            _waterfall.Clear();
        }

        private void ReFill()
        {
            _waterfall.Fill();
        }


    }
}