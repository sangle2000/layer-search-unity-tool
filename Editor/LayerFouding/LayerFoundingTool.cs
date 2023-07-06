using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Codice.Client.Common;
using Unity.Plastic.Newtonsoft.Json.Linq;
using System;
using System.Linq;
using static Codice.Client.BaseCommands.Import.Commit;
using static UnityEditor.Experimental.GraphView.GraphView;
using Unity.VisualScripting;
using UnityEditor.PackageManager.UI;

namespace MyTools
{
    public class LayerFoundingTool : EditorWindow
    {
        public enum OPTIONS { };
        static LayerFoundingTool win;
        string showAttrib = "";
        string nameAttribObj = "";
        string nameLayer = "";
        string nameAttrib = "";
        string checkObject = "";

        int selected = 0;
        int lenOfPrefab = 0;
        int count = 0;

        bool showAttribObj = true;
        bool showLayer = true;
        bool GOSelection = false;
        bool beginCount = false;

        Vector2 paletteScrollPos = new Vector2(0, 0);

        JObject data = new JObject();
        JObject prefabData = new JObject();
        List<string> layerList = new List<string>();
        ArrayList layerNames = new ArrayList();

        public static void InitWindow()
        {
            win = EditorWindow.GetWindow<LayerFoundingTool>("Layer Founding");
            win.minSize = new Vector2(450, 500);
            win.maxSize = new Vector2(450, 500);
            win.Show();
        }

        private void OnGUI()
        {

            SaveDataIntoJsonFile();

            GetLayerList();

            string[] text = layerList.ToArray();

            int lenOfList = text.Length;

            GUILayout.BeginArea(new Rect(0, 20, 500, showLayer ? ((lenOfList / 2) * 75) : 100));
            nameLayer = "Layer List";
            showLayer = EditorGUILayout.Foldout(showLayer, nameLayer);
            if (showLayer)
            {
                if (Selection.activeTransform)
                {
                    EditorGUI.BeginChangeCheck();
                    GUILayout.BeginArea(new Rect(40, 30, 500, 100));
                    selected = GUILayout.SelectionGrid(selected, text, Math.Min(lenOfList, 3), EditorStyles.radioButton, GUILayout.Width(400));
                    GUILayout.EndArea();

                    if (EditorGUI.EndChangeCheck())
                    {
                        count = 0;
                        checkObject = text[selected];
                        beginCount = true;
                    }
                }
                else if (!Selection.activeTransform)
                {
                    showLayer = false;
                }
            }
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(0, showLayer ? ((lenOfList / 2) * 50) : 50, 500, 20));

            EditorGUI.BeginChangeCheck();
            GOSelection = EditorGUILayout.Toggle("Game Object", GOSelection);
            if (EditorGUI.EndChangeCheck())
            {
                count = 0;
                beginCount = true;
            }

            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(0, showLayer ? ((lenOfList / 2) * 50) + 30 : 50, 500, showAttribObj ? (GOSelection ? data.Count : lenOfPrefab) * 100 : 100));
            paletteScrollPos = GUILayout.BeginScrollView(paletteScrollPos, GUILayout.ExpandWidth(true), GUILayout.Height(320));

            if (GOSelection)
            {
                nameAttribObj = "Game Object List " + $"({count})";
                showAttribObj = EditorGUILayout.Foldout(showAttribObj, nameAttribObj);

                if (showAttribObj)
                {
                    if (Selection.activeTransform)
                    {
                        foreach (JProperty property in data.Properties())
                        {
                            EditorGUI.indentLevel++;
                            if (property.Value.ToString() == text[selected])
                            {
                                if (beginCount)
                                {
                                    count++;
                                }
                                EditorGUILayout.BeginVertical("box");
                                showAttrib = property.Value.ToString();
                                nameAttrib = property.Name;

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(nameAttrib, GUILayout.Width(200));
                                EditorGUILayout.LabelField(showAttrib, GUILayout.Width(100));
                                if (GUILayout.Button("Find Object", GUILayout.Height(20), GUILayout.Width(100)))
                                {
                                    if (property.Value.ToString() == checkObject)
                                    {
                                        GameObject go = GameObject.Find(nameAttrib);
                                        EditorGUIUtility.PingObject(go);
                                    }
                                }
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.EndVertical();
                            }           
                            EditorGUI.indentLevel--;
                        }
                        beginCount = false;

                    }
                    else if (!Selection.activeTransform)
                    {
                        showAttribObj = false;
                    }
                }
            }
            else
            {
                nameAttribObj = "Prefab List " + $"({count})";
                showAttribObj = EditorGUILayout.Foldout(showAttribObj, nameAttribObj);

                if (showAttribObj)
                {
                    if (Selection.activeTransform)
                    {
                        string[] guids = AssetDatabase.FindAssets("t:Prefab");

                        lenOfPrefab = guids.Length;

                        foreach (var guid in guids)
                        {
                            var path = AssetDatabase.GUIDToAssetPath(guid);
                            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                            if (LayerMask.LayerToName(go.layer) == checkObject)
                            {
                                if (beginCount)
                                {
                                    count++;
                                }
                                EditorGUILayout.BeginVertical("box");
                                nameAttrib = go.name;
                                showAttrib = LayerMask.LayerToName(go.layer);

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(nameAttrib, GUILayout.Width(200));
                                EditorGUILayout.LabelField(showAttrib, GUILayout.Width(100));
                                if (GUILayout.Button("Find Prefab", GUILayout.Height(20), GUILayout.Width(100)))
                                {
                                    EditorGUIUtility.PingObject(go);
                                }
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.EndVertical();
                            }
                        }
                        beginCount = false;
                    } else if (!Selection.activeTransform)
                    {
                        showAttribObj = false;
                    }
                }            
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void SaveDataIntoJsonFile()
        {
            data = new();
            List<GameObject> realList = new List<GameObject>();

            GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            foreach (GameObject go in allObjects)
            {
                if (EditorUtility.IsPersistent(go.transform.root.gameObject))
                    continue;
                realList.Add(go);
            }

            for (int i = 0; i < realList.Count; i++)
            {
                string name = realList[i].name;
                data.Add(name, LayerMask.LayerToName(realList[i].layer));
                if (!layerList.Contains(LayerMask.LayerToName(realList[i].layer)))
                {
                    layerList.Add(LayerMask.LayerToName(realList[i].layer));
                }
            }
        }

        private void GetLayerList()
        {
            layerNames = new();
            for (int i = 0; i <= 31; i++) //user defined layers start with layer 8 and unity supports 31 layers
            {
                var layerN = LayerMask.LayerToName(i); //get the name of the layer
                if (layerN.Length > 0)
                {
                    layerNames.Add(layerN);
                }
            }
        }
    }
}

