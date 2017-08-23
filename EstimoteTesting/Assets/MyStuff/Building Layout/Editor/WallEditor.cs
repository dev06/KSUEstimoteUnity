using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Wall))]
public class WallEditor : Editor {
    //Get Wall using "target"
    public override void OnInspectorGUI()
    {
        //display what normally would show up (public variables, etc)
        base.OnInspectorGUI();
        Wall wall = (Wall)target;
        wall.UpdateScale();
        GUILayout.Space(50);
        GUILayout.Label("Move the wall in accordance to \"SEPARATOR\" region");
        if(GUILayout.Button("UPDATE WALL POSITION"))
        {
            wall.UpdateScale();
        }
    }
}
