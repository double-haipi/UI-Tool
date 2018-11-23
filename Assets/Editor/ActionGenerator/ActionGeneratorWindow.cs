using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace com.tencent.pandora.tools
{
    public class ActionGeneratorWindow : EditorWindow
    {
        #region 参数

        private Dictionary<string, string> _actionConfig = new Dictionary<string, string>()
    {
        {"__ACTION_NAME__",""},
        {"__SWITCH_NAME__",""},
        {"__ACT_STYLE__",""},
        {"__CHANNEL_ID__",""},
        {"__GAME_COMMAND_CONTENT__",""},
    };

        private Dictionary<string, string> _actionConfigDescription = new Dictionary<string, string>()
    {
        {"__ACTION_NAME__","活动名称(例:Pop)"},
        {"__SWITCH_NAME__","活动开关(例:pop)"},
        {"__ACT_STYLE__","活动类型(例:10166)"},
        {"__CHANNEL_ID__","活动频道(例:10130)"},
        {"__GAME_COMMAND_CONTENT__","活动打开命令(例:pop)"},
    };

        private Vector2 _actionConfigScrollPositon = Vector2.zero;
        #endregion

        #region GUI
        [MenuItem("Assets/GenerateAction", priority = 0)]
        private static void Init()
        {
            EditorWindow.GetWindow<ActionGeneratorWindow>(false, "GenerateAction", true).Show();
        }

        [MenuItem("Assets/GenerateAction", true)]
        private static bool MenuValidation()
        {
            string activeObjectPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (activeObjectPath.Equals("Assets/Actions/Resources"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            DrawConfigList("ActionConfig", ref _actionConfigScrollPositon, ref _actionConfig, ref _actionConfigDescription);
            DrawGenerateButton();
            EditorGUILayout.EndVertical();
        }

        private void DrawConfigList(string title, ref Vector2 scrollPosition, ref Dictionary<string, string> config, ref Dictionary<string, string> configDescription)
        {
            //DrawTitle(title);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(180));
            List<string> keys = new List<string>(config.Keys);
            string key = string.Empty;
            for (int i = 0, count = keys.Count; i < count; i++)
            {
                key = keys[i];
                config[key] = EditorGUILayout.TextField(configDescription[key] + ":", config[key]).Trim();
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawTitle(string title)
        {
            GUIStyle titleStyle = new GUIStyle();
            titleStyle.fontSize = 15;
            titleStyle.normal.textColor = Color.green;
            EditorGUILayout.LabelField(title, titleStyle);
            DrawSpace(2);
        }

        private void DrawSpace(int spaceNumber)
        {
            for (int i = 0; i < spaceNumber; i++)
            {
                EditorGUILayout.Space();
            }
        }

        private void DrawGenerateButton()
        {
            if (GUILayout.Button("生成", GUILayout.Height(50)))
            {
                new ActionGenerator().Generate(_actionConfig);
            }
        }
        #endregion
    }
}