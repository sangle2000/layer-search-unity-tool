using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MyTools;

public class EditorMenus : MonoBehaviour
{
    [MenuItem("Tools/Layer Founding")]
    public static void initProjectSetupTool()
    {
        LayerFoundingTool.InitWindow();
    }
}
