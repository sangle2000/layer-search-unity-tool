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
using UnityEditor.Experimental.GraphView;
using log4net.Util;
using Codice.Client.BaseCommands.BranchExplorer;

namespace MyTools
{
    public class LayerFoundingTool : EditorWindow
    {
        static LayerFoundingTool win;
        string showAttrib = "";
        string nameAttribObj = "";
        string nameLayer = "";
        string nameAttrib = "";
        string checkObject = "";
        string[] text = null;
        string[] options = { "In Scene", "Prefab" };

        int selected = 0;
        int selected_option = 0;
        int lenOfPrefab = 0;
        int count = 0;
        int index = 0;
        int lenOfList = 0;

        bool showAttribObj = true;
        bool showLayer = true;
        bool GOSelection = true;
        bool beginCount = false;

        Vector2 paletteScrollPos = new Vector2(0, 0);

        JObject data = new JObject();
        List<string> layerList = new List<string>();

        public static void InitWindow()
        {
            win = EditorWindow.GetWindow<LayerFoundingTool>("Layer Founding");
            win.minSize = new Vector2(450, 500);
            win.maxSize = new Vector2(450, 500);
            Selection.activeGameObject = null;
            win.Show();
        }

        private void OnGUI()
        {

            SaveDataIntoJsonFile();

            //GetLayerList();
            if (Selection.activeGameObject == null)
            {
                Selection.activeGameObject = GetGameObjectByIndex(index);

                SaveDataIntoJsonFile();

                text = layerList.ToArray();

                lenOfList = text.Length;

                checkObject = text[selected];

                count = 0;

                selected_option = 0;

                beginCount = true;
            }

            text = layerList.ToArray();

            lenOfList = text.Length;

            GUILayout.BeginArea(new Rect(0, 20, 500, showLayer ? ((lenOfList / 2) * 75) : 100));
            nameLayer = "Layer List";
            showLayer = EditorGUILayout.Foldout(showLayer, nameLayer);
            if (showLayer)
            {
                if (Selection.activeTransform)
                {
                    EditorGUI.BeginChangeCheck();
                    GUILayout.BeginArea(new Rect(40, 30, 500, 100));
                    selected = GUILayout.SelectionGrid(selected, text, Math.Min(lenOfList, 3), EditorStyles.radioButton, GUILayout.Width(500));
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
            selected_option = GUILayout.SelectionGrid(selected_option, options, 2, EditorStyles.radioButton, GUILayout.Width(400));
            if (EditorGUI.EndChangeCheck())
            {
                count = 0;
                beginCount = true;
                if(selected_option == 0)
                {
                    GOSelection = true;
                }
                else
                {
                    GOSelection = false;
                }
            }

            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(0, showLayer ? ((lenOfList / 2) * 50) + 30 : 80, 500, showAttribObj ? (GOSelection ? data.Count : lenOfPrefab) * 100 : 100));
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
                                nameAttrib = property.Name.Split("_")[1];

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
                    }
                    else if (!Selection.activeTransform)
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

            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            //GameObject[] allObjects = GetAllGameObjects();

            foreach (GameObject go in allObjects)
            {
                realList.Add(go);
            }

            for (int i = 0; i < realList.Count; i++)
            {
                if (LayerMask.LayerToName(realList[i].layer) != "Default")
                {
                    string name = i + "_" + realList[i].name;
                    data.Add(name, LayerMask.LayerToName(realList[i].layer));
                    if (!layerList.Contains(LayerMask.LayerToName(realList[i].layer)))
                    {
                        layerList.Add(LayerMask.LayerToName(realList[i].layer));
                    }
                }
            }
        }

        private GameObject GetGameObjectByIndex(int index)
        {
            List<GameObject> allObjects = new List<GameObject>();
            GetAllGameObjectsInScene(allObjects);

            if (index >= 0 && index < allObjects.Count)
            {
                return allObjects[index];
            }

            return null;
        }

        private void GetAllGameObjectsInScene(List<GameObject> allObjects)
        {
            GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

            foreach (GameObject rootObject in rootObjects)
            {
                allObjects.Add(rootObject);
                AddChildrenRecursive(rootObject.transform, allObjects);
            }
        }

        private void AddChildrenRecursive(UnityEngine.Transform parent, List<GameObject> allObjects)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                UnityEngine.Transform child = parent.GetChild(i);
                allObjects.Add(child.gameObject);
                AddChildrenRecursive(child, allObjects);
            }
        }
    }
}

