using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(UpdateMapData),true)]
public class UpdatableDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        UpdateMapData dataEditor = (UpdateMapData)target;

        if (GUILayout.Button("Update"))
        {
            dataEditor.NotifyOfUpdatedValues();
            EditorUtility.SetDirty(target);
        }
    }
}
