using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;
using Unity.SharpZipLib.Utils;


public class UnzipAll : EditorWindow {
    [MenuItem("Window/UnzipAll")]
    public static void ShowWindow() {
        EditorWindow.GetWindow(typeof(UnzipAll));
    }
    void OnGUI() {
        if(GUILayout.Button("Unzip All Zip In This Project")) {
            UnzipAllinProject();
        }
        GUILayout.Space(10);
        EditorGUI.BeginChangeCheck();
        GUILayout.Label(GetAllZipProject());
        EditorGUI.EndChangeCheck();
    }


    string[] zipAssetPaths;
    private string GetAllZipProject() {
        zipAssetPaths = Directory.GetFiles(Application.dataPath,"*.zip",SearchOption.AllDirectories);
        string tempString = "";
        foreach(string str in zipAssetPaths) {
            var fileName = Path.GetFileName(str);
            tempString += (fileName + "\n");
        }
        return tempString;
    }


    private void UnzipAllinProject() {
        zipAssetPaths = Directory.GetFiles(Application.dataPath,"*.zip",SearchOption.AllDirectories);
        AssetDatabase.StartAssetEditing();
        foreach(string path in zipAssetPaths) {
            var fileName = Path.GetFileName(path);
            ZipUtility.UncompressFromZip(path,"",path.Replace(fileName,""));
        }
    }
}
