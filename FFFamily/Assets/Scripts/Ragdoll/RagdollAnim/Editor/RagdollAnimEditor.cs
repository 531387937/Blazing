using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RagdollAnimCreator))]
public class RagdollAnimEditor : Editor
{
    private RagdollAnimCreator creator;
    private void OnEnable()
    {
        creator = (RagdollAnimCreator)target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(creator.ragdollAnim==null)
        {
            if (GUILayout.Button("创建一个新动画文件"))
            {
                creator.CreateNewAnim();
            }
        }
        else
        {
            if (GUILayout.Button("点击这里进行保存"))
            {
                AssetDatabase.CreateAsset(creator.ragdollAnim, "Assets/Arts/test.asset");
                //creator.CreateNewAnim();
            }
        }
    }
}
