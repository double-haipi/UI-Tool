using UnityEngine;
using System.Collections;
using UnityEditor;

namespace com.tencent.pandora.tools
{
    public class WaterfallScrollViewCreator_NGUI
    {
        [MenuItem("PandoraTools/组件生成器/瀑布流_NGUI")]
        public static void Create()
        {
            GameObject waterfallScrollView = CreateGameObject("WaterfallScrollView", null);
            GameObject pool = CreateGameObject("Container_pool", waterfallScrollView);
            GameObject template = CreateGameObject("itemTemplate", waterfallScrollView, true);
            GameObject viewport = CreateGameObject("Viewport", waterfallScrollView);
            GameObject content = CreateGameObject("Content", viewport, true);

            UIScrollView scrollView = waterfallScrollView.AddComponent<UIScrollView>();
            scrollView.movement = UIScrollView.Movement.Vertical;
            scrollView.contentPivot = UIWidget.Pivot.TopLeft;

            UIPanel clipPanel = waterfallScrollView.GetComponent<UIPanel>();
            clipPanel.clipping = UIDrawCall.Clipping.SoftClip;

            WaterfallScrollViewHelper_NGUI helper = waterfallScrollView.AddComponent<WaterfallScrollViewHelper_NGUI>();
            helper.itemTemplate = template;
            helper.itemPool = pool;
        }

        private static GameObject CreateGameObject(string name, GameObject parent, bool hasWidget = false)
        {
            GameObject go = new GameObject(name);
            Transform trans = go.transform;
            if (parent != null)
            {
                trans.SetParent(parent.transform);
            }
            if (hasWidget == true)
            {
                UIWidget widget = go.AddComponent<UIWidget>();
                widget.pivot = UIWidget.Pivot.TopLeft;
            }
            trans.localPosition = Vector3.zero;
            trans.localRotation = Quaternion.Euler(Vector3.zero);
            trans.localScale = Vector3.one;

            return go;
        }
    }
}