using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(SlicerPlane))]
public class SlicerPlaneEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SlicerPlane cp = this.target as SlicerPlane;
        base.OnInspectorGUI();
        if (Application.isPlaying && GUILayout.Button("切割"))
        {
            GameObject go = GameObject.Find("Sliceable");
            if (null != go)
            {
                ISliceable sliceable = go.GetComponent<ISliceable>();
                if (null != sliceable)
                {
                    cp.Slice(sliceable);
                }
            }
        }
    }
}
