using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor; 
[ExecuteInEditMode]
public class AlembicPathFix: MonoBehaviour
{

#if UNITY_EDITOR
    private void Awake()
    {
      if(!Application.isPlaying)  ReImport(gameObject);
    }
    private void ReImport( GameObject go)
    {
            AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("fetus_alembic")[0]));           
    }

#endif
}
