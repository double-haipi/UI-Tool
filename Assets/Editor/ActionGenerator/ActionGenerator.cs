//#define USING_UGUI
#define USING_NGUI

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace com.tencent.pandora.tools
{
    public class ActionGenerator
    {
        #region 参数
        private Dictionary<string, string> _config;
        private string _actionFolderPath = string.Empty;
        private string _luaFolderPath = string.Empty;
        private string _templatesFolderPath = string.Empty;

        private string _actionFolderPathPrefix = "Actions/Resources";
        private string _generatorPathPrefix = "Pandora/Editor/ActionGenerator";
        private string[] _luaTemplatesNames = new string[]{
                                                            "Action.lua.bytes",
                                                            "ActionController.lua.bytes",
                                                            "ActionMessager.lua.bytes",
                                                            "ActionPanel.lua.bytes",
                                                            "ActionRequestAssembler.lua.bytes",
                                                            "ActionResponseParser.lua.bytes",
                                                            "ActionSettings.lua.bytes",
                                                          };
        private string _deletePatternUGUI = @"(--UGUIBegin[\s\S]*?--UGUIEnd)";
        private string _deletePatternNGUI = @"(--NGUIBegin[\s\S]*?--NGUIEnd)";


        #endregion
        public void Generate(Dictionary<string, string> config)
        {
            _config = config;
            if (CheckConfig() == false)
            {
                DisplayWarningDialog("配置不完整，请检查。");
                return;
            }
            InitPaths();
            CreateDirectories();
            GenerateLua();
            GeneratePrefab();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        private bool CheckConfig()
        {
            foreach (var item in _config)
            {
                if (item.Value.Equals(string.Empty))
                {
                    return false;
                }
            }
            return true;
        }

        private void InitPaths()
        {
            string actionName = _config["__ACTION_NAME__"];
            _actionFolderPath = Path.Combine(Application.dataPath, Path.Combine(_actionFolderPathPrefix, CapitalizeInitials(actionName)));
            _luaFolderPath = Path.Combine(_actionFolderPath, "Lua");
            _templatesFolderPath = Path.Combine(Application.dataPath, Path.Combine(_generatorPathPrefix, "LuaTemplates"));
        }

        private void CreateDirectories()
        {
            if (Directory.Exists(_actionFolderPath) == true)
            {
                return;
            }

            Directory.CreateDirectory(_actionFolderPath);
            Directory.CreateDirectory(_luaFolderPath);
            Directory.CreateDirectory(Path.Combine(_actionFolderPath, "Atlas"));
            Directory.CreateDirectory(Path.Combine(_actionFolderPath, "Prefabs"));
            Directory.CreateDirectory(Path.Combine(_actionFolderPath, "Textures"));
        }

        //首字母大写
        private string CapitalizeInitials(string source)
        {
            string head = source.Substring(0, 1);
            string left = source.Substring(1);
            return head.ToUpper() + left;
        }

        private void GenerateLua()
        {
            string content = string.Empty;
            string sourcePath = string.Empty;
            string targetPath = string.Empty;

            for (int i = 0, length = _luaTemplatesNames.Length; i < length; i++)
            {
                sourcePath = Path.Combine(_templatesFolderPath, _luaTemplatesNames[i]);
                if (File.Exists(sourcePath) == true)
                {
                    content = File.ReadAllText(sourcePath);
                    content = FillConfig(content);
                    targetPath = GetLuaPath(_luaTemplatesNames[i]);
                    File.WriteAllText(targetPath, content);
                }
            }
        }

        private string FillConfig(string source)
        {
            string target = source;
            foreach (var item in _config)
            {
                target = Regex.Replace(target, item.Key, item.Value);
            }
#if USING_UGUI
            target = Regex.Replace(target, _deletePatternNGUI, "");
            target = target.Replace("--UGUIBegin", "");
            target = target.Replace("--UGUIEnd", "");
#endif

#if USING_NGUI
            target = Regex.Replace(target, _deletePatternUGUI, "");
            target = target.Replace("--NGUIBegin", "");
            target = target.Replace("--NGUIEnd", "");
#endif
            return target;
        }

        private string GetLuaPath(string luaTemplateName)
        {
            string name = Path.GetFileName(luaTemplateName);
            name = Regex.Replace(name, "Action", CapitalizeInitials(_config["__ACTION_NAME__"]));
            return Path.Combine(_luaFolderPath, name);
        }

        private void GeneratePrefab()
        {
            string panelName = CapitalizeInitials(_config["__ACTION_NAME__"]);
            string prefabPath = string.Format("Assets/{0}/{1}/Prefabs/{2}.prefab", _actionFolderPathPrefix, panelName, panelName);
            GameObject instance;
#if USING_UGUI
            instance = new GameObject(panelName, typeof(RectTransform));
            GameObject parent = GameObject.Find("Canvas");
            if (parent == null)
            {
                DisplayWarningDialog("当前场景中没有Canvas，无法挂载生成的活动面板。");
                return;
            }
#endif

#if USING_NGUI
            instance = new GameObject(panelName, typeof(UIPanel));
            GameObject parent = GameObject.Find("UI Root");
            if (parent == null)
            {
                DisplayWarningDialog("当前场景中没有UI Root，无法挂载生成的活动面板。");
                return;
            }
#endif
            Transform instanceTransform = instance.transform;
            instanceTransform.SetParent(parent.transform);
            instanceTransform.localPosition = Vector3.zero;
            instanceTransform.localRotation = Quaternion.identity;
            instanceTransform.localScale = Vector3.one;
            Object prefab = PrefabUtility.CreatePrefab(prefabPath, instance);
            PrefabUtility.ReplacePrefab(instance, prefab, ReplacePrefabOptions.ConnectToPrefab);
        }

        private void DisplayWarningDialog(string message, string title = "")
        {
            EditorUtility.DisplayDialog(title, message, "我知道了");
        }
    }
}