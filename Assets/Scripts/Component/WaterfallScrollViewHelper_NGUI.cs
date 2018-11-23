using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

namespace com.tencent.pandora
{
    //[CustomLuaClass]
    public class WaterfallScrollViewHelper_NGUI : MonoBehaviour
    {

        public GameObject itemTemplate;
        public GameObject itemPool;

        //滑动到顶端时触发的事件，可能会多次触发，业务层需要做防重入处理
        public UnityEvent onReachTop = new UnityEvent();
        public UnityEvent onReachBottom = new UnityEvent();

        private Transform _itemParentTrans;

        private Action<GameObject, int> _itemFillDelegate;
        private Action<GameObject> _itemRecycleDelegate;

        private static int MOVE_DELTA = 10;
        private UIScrollView _scrollView;
        private float _upBoundary = 0;
        private float _bottomBoundary = 0;
        private int _startIndex = 0;
        private int _endIndex = -1;
        private int _itemCount;
        private float _contentTopPositionY;
        private bool _hasSetTopPositionY = false;
        private float _lastContentPositionY;
        private float _viewportHeight = 0;

        //在ScrollView区的item Dictionary，key 为索引值
        private Dictionary<int, GameObject> _onAirItemDict;

        private void Awake()
        {
            _onAirItemDict = new Dictionary<int, GameObject>();
            _scrollView = transform.GetComponent<UIScrollView>();
            _scrollView.onDragFinished = OnDragFinished;
            InitTemplate();
            InitPool();
        }

        public void OnDragFinished()
        {
            if (GetContentPositionY() < GetContentTopPositionY())
            {
                onReachTop.Invoke();
            }
            if ((GetContentPositionY() - GetContentTopPositionY()) > (GetContentHeight() - GetViewportHeight()))
            {
                onReachBottom.Invoke();
            }
        }

        private float GetContentTopPositionY()
        {
            if (_hasSetTopPositionY == false)
            {
                _hasSetTopPositionY = true;
                _scrollView.ResetPosition();
                _contentTopPositionY = transform.localPosition.y;
            }
            return _contentTopPositionY;
        }

        private float GetContentPositionY()
        {
            return transform.localPosition.y;
        }

        private void InitTemplate()
        {
            if (itemTemplate == null)
            {
                Transform templateTrans = transform.Find("itemTemplate");
                if (templateTrans != null)
                {
                    itemTemplate = templateTrans.gameObject;
                }
            }
            if (itemTemplate == null)
            {
                Debug.LogError("item模版没有设置");
                return;
            }

            itemTemplate.SetActive(false);
        }

        private void InitPool()
        {
            if (itemPool == null)
            {
                Transform poolTrans = transform.Find("Container_pool");
                if (poolTrans != null)
                {
                    itemPool = poolTrans.gameObject;
                }
            }
            if (itemPool == null)
            {
                Debug.LogError("item资源池没有设置");
                return;
            }

            itemPool.SetActive(false);
        }

        public void SetItemFillDelegate(Action<GameObject, int> itemFillDelegate)
        {
            _itemFillDelegate = itemFillDelegate;
        }

        public void SetItemRecycleDelegate(Action<GameObject> itemRecycleDelegate)
        {
            _itemRecycleDelegate = itemRecycleDelegate;
        }

        public int ItemCount
        {
            get
            {
                return _itemCount;
            }
            set
            {
                _itemCount = value;
            }
        }

        public void Fill()
        {
            _startIndex = 0;
            _endIndex = -1;
            _upBoundary = 0;
            _bottomBoundary = 0;
            _onAirItemDict.Clear();
            _scrollView.ResetPosition();
            _lastContentPositionY = GetContentPositionY();
            GetContentTopPositionY();
            Forward();
        }

        private void Forward()
        {
            while ((_endIndex < this.ItemCount - 1) && (_bottomBoundary + GetContentPositionY() > -GetViewportHeight()))
            {
                _endIndex += 1;
                GameObject item = CreateItem(_endIndex);
                _onAirItemDict.Add(_endIndex, item);
                SetItemPosition(item, 0f, _bottomBoundary);
                RefreshBottomBoundary();
                //标记bounds需重新计算，不用自己设置scrollview的区域
                _scrollView.InvalidateBounds();
            }
            RecycleTop();
        }

        private void RefreshBottomBoundary()
        {
            GameObject endItem = _onAirItemDict[_endIndex];
            Transform trans = endItem.transform;

            float itemHeight = CalculateRelativeWidgetBounds(trans).size.y;
            _bottomBoundary = trans.localPosition.y - itemHeight;
        }
        private void RecycleTop()
        {
            if (GetRangeItemHeight(_startIndex + 1, _endIndex) < GetViewportHeight())
            {
                return;
            }
            for (int i = _startIndex; i <= _endIndex; i++)
            {
                GameObject item = _onAirItemDict[i];
                if (IsItemUpOfViewport(item) == true)
                {
                    _onAirItemDict.Remove(i);
                    RecycleItem(item);
                }
                else
                {
                    _startIndex = i;
                    break;
                }
                _scrollView.InvalidateBounds();
            }
            RefreshUpBoundary();
        }

        private float GetRangeItemHeight(int startIndex, int endIndex)
        {
            float result = 0f;
            for (int i = startIndex; i <= endIndex; i++)
            {
                GameObject item = _onAirItemDict[i];
                Transform trans = item.transform;
                result += CalculateRelativeWidgetBounds(trans).size.y;
            }
            return result;
        }
        private float GetViewportHeight()
        {
            if (_viewportHeight == 0)
            {
                _viewportHeight = transform.GetComponent<UIPanel>().finalClipRegion.w;
            }
            return _viewportHeight;
        }

        private bool IsItemUpOfViewport(GameObject item)
        {
            Transform trans = item.transform;
            float itemHeight = CalculateRelativeWidgetBounds(trans).size.y;
            float positionY = trans.localPosition.y - itemHeight + GetContentPositionY();

            if (positionY > GetContentTopPositionY())
            {
                return true;
            }
            return false;
        }

        private void RecycleItem(GameObject item)
        {
            if (_itemRecycleDelegate == null)
            {
                Debug.LogError("itemRecycleDelegate 未设置");
                return;
            }
            _itemRecycleDelegate(item);
            item.transform.SetParent(itemPool.transform);
        }

        // 边界  
        private void RefreshUpBoundary()
        {
            GameObject startItem = _onAirItemDict[_startIndex];
            Transform trans = startItem.transform;
            _upBoundary = trans.localPosition.y;
        }

        //手指下滑
        private void Backward()
        {
            while (_startIndex > 0 && (_upBoundary + GetContentPositionY()) < GetContentTopPositionY())
            {
                _startIndex -= 1;
                GameObject item = CreateItem(_startIndex);
                _onAirItemDict.Add(_startIndex, item);
                float itemHeight = GetItemHeight(item);
                SetItemPosition(item, 0, _upBoundary + itemHeight);
                RefreshUpBoundary();
                _scrollView.InvalidateBounds();
            }
            RecycleBottom();
        }
        private float GetItemHeight(GameObject item)
        {
            return CalculateRelativeWidgetBounds(item.transform).size.y;
        }
        private void RecycleBottom()
        {
            if (GetRangeItemHeight(_startIndex, _endIndex - 1) < GetViewportHeight())
            {
                return;
            }

            for (int i = _endIndex; i >= _startIndex; i--)
            {
                GameObject item = _onAirItemDict[i];
                if (IsItemBottomOfViewport(item) == true)
                {
                    _onAirItemDict.Remove(i);
                    RecycleItem(item);
                }
                else
                {
                    _endIndex = i;
                    break;
                }
                _scrollView.InvalidateBounds();
            }
            RefreshBottomBoundary();
        }

        private bool IsItemBottomOfViewport(GameObject item)
        {
            Transform trans = item.transform;
            float positionY = trans.localPosition.y + GetContentPositionY();

            if (positionY < -GetViewportHeight() + GetContentTopPositionY())
            {
                return true;
            }
            return false;
        }


        private GameObject CreateItem(int index)
        {
            Transform itemTrans = null;
            if (itemPool.transform.childCount > 0)
            {
                itemTrans = itemPool.transform.GetChild(0);
            }
            else
            {
                GameObject item = GameObject.Instantiate(itemTemplate) as GameObject;
                item.SetActive(true);
                itemTrans = item.transform;
            }

            itemTrans.name = "item_" + index.ToString();
            itemTrans.SetParent(ItemParentTrans);
            itemTrans.localPosition = Vector3.zero;
            itemTrans.localRotation = Quaternion.Euler(Vector3.zero);
            itemTrans.localScale = Vector3.one;
            FillItem(itemTrans.gameObject, index);

            //绑定对象的onDrag事件
            UIDragScrollView[] dragComponents = itemTrans.GetComponentsInChildren<UIDragScrollView>(true);
            for (int i = 0, iMax = dragComponents.Length; i < iMax; i++)
            {
                UIEventListener.Get(dragComponents[i].gameObject).onDrag = OnScrollValueChanged;
            }

            return itemTrans.gameObject;
        }

        private void FillItem(GameObject item, int index)
        {
            if (_itemFillDelegate == null)
            {
                Debug.LogError("itemFillDelegate 未设置");
                return;
            }
            _itemFillDelegate(item, index);
        }
        private Transform ItemParentTrans
        {
            get
            {
                if (_itemParentTrans == null)
                {
                    _itemParentTrans = transform.Find("Viewport/Content");
                }
                if (_itemParentTrans == null)
                {
                    Debug.LogError("item的父对象Content不存在");
                    return null;
                }
                return _itemParentTrans;
            }
        }

        // 要处理多重drag的情况
        private void OnScrollValueChanged(GameObject go, Vector2 delta)
        {
            if (Mathf.Abs(delta.x) > 50 || Mathf.Abs(delta.y) > 50)
            {
                return;
            }
            float contentPostionY = GetContentPositionY();
            float distance = contentPostionY - _lastContentPositionY;
            if (distance > MOVE_DELTA)
            {
                Forward();
                _lastContentPositionY = contentPostionY;

            }
            else if (distance < -MOVE_DELTA)
            {
                Backward();
                _lastContentPositionY = contentPostionY;
            }
        }

        private float GetContentHeight()
        {
            _scrollView.InvalidateBounds();
            return _scrollView.bounds.size.y;
        }
        private void SetItemPosition(GameObject item, float x, float y)
        {
            Transform trans = item.transform;
            trans.localPosition = new Vector3(x, y, 0f);
        }

        //用于处理需要展开和收起情况，重排列位置
        public void RefreshLayout(int index)
        {
            //index的范围要校验下
            if (index < _startIndex || index > _endIndex)
            {
                return;
            }
            for (int i = index; i <= _endIndex; i++)
            {
                GameObject item = _onAirItemDict[i];
                Transform trans = item.transform;
                if (i > index)
                {
                    SetItemPosition(item, 0, _bottomBoundary);
                }
                float itemHeight = GetItemHeight(item);
                _bottomBoundary = trans.localPosition.y - itemHeight;
            }
            _scrollView.InvalidateBounds();
        }

        public void Clear()
        {
            for (int i = _startIndex; i <= _endIndex; i++)
            {
                RecycleItem(_onAirItemDict[i]);
            }
            _onAirItemDict.Clear();
            _scrollView.InvalidateBounds();
            _scrollView.ResetPosition();
            _lastContentPositionY = GetContentPositionY();

        }

        #region
        private Bounds CalculateRelativeWidgetBounds(Transform content, bool considerInactive = false)
        {
            if (content == null)
            {
                return new Bounds(Vector3.zero, Vector3.zero);
            }
            bool isSet = false;
            Matrix4x4 toLocal = content.worldToLocalMatrix;
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            CalculateRelativeWidgetBounds(content, considerInactive, true, ref toLocal, ref min, ref max, ref isSet, true);

            if (isSet == false)
            {
                return new Bounds(Vector3.zero, Vector3.zero);
            }

            Bounds bound = new Bounds(min, Vector3.zero);
            bound.Encapsulate(max);
            return bound;
        }

        [System.Diagnostics.DebuggerHidden]
        [System.Diagnostics.DebuggerStepThrough]
        private void CalculateRelativeWidgetBounds(Transform content, bool considerInactive, bool isRoot, ref Matrix4x4 toLocal, ref Vector3 vMin, ref Vector3 vMax, ref bool isSet, bool considerChildren = true)
        {
            if (content == null)
            {
                return;
            }
            if (considerInactive == false && IsGameObjectActive(content.gameObject) == false)
            {
                return;
            }
            //非根节点，判断是否有uipanel组件
            UIPanel panel = isRoot ? null : content.GetComponent<UIPanel>();
            if (panel != null && panel.enabled == false)
            {
                return;
            }

            // 裁剪panel，把裁剪区域包含进来
            if (panel != null && panel.clipping != UIDrawCall.Clipping.None)
            {
                Vector3[] corners = panel.worldCorners;
                SetExtremeValue(corners, ref toLocal, ref vMax, ref vMin);
                isSet = true;
                return;
            }
            //没有panel的情况
            UIWidget widget = content.GetComponent<UIWidget>();
            if (widget != null && widget.enabled == true)
            {
                Vector3[] corners = widget.worldCorners;
                SetExtremeValue(corners, ref toLocal, ref vMax, ref vMin);
                isSet = true;
                if (considerChildren == false)
                {
                    return;
                }
            }

            for (int i = 0, iMax = content.childCount; i < iMax; i++)
            {
                CalculateRelativeWidgetBounds(content.GetChild(i), considerInactive, false, ref toLocal, ref vMin, ref vMax, ref isSet, true);
            }
        }

        private bool IsGameObjectActive(GameObject go)
        {
            return go && go.activeInHierarchy;
        }

        private void SetExtremeValue(Vector3[] corners, ref Matrix4x4 toLocal, ref Vector3 vMax, ref Vector3 vMin)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector3 v = toLocal.MultiplyPoint3x4(corners[i]);
                if (v.x > vMax.x)
                {
                    vMax.x = v.x;
                }
                if (v.y > vMax.y)
                {
                    vMax.y = v.y;
                }
                if (v.z > vMax.z)
                {
                    vMax.z = v.z;
                }

                if (v.x < vMin.x)
                {
                    vMin.x = v.x;
                }
                if (v.y < vMin.y)
                {
                    vMin.y = v.y;
                }
                if (v.z < vMin.z)
                {
                    vMin.z = v.z;
                }
            }
        }
        #endregion

    }

}