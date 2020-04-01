using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditorInternal;
[CustomEditor(typeof(RagdollAnimCreator))]
public class RagdollAnimEditor : Editor
{
    private RagdollAnimCreator creator;
    private string animName;
    bool[] showAnim;
    private void OnEnable()
    {
        creator = (RagdollAnimCreator)target;
        showAnim = new bool[15];
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
            GUILayout.Space(30);

            for(int i = 0;i<creator.ragdollAnim.animation.Count;i++)
            {
                RagdollClip tempAnim = creator.ragdollAnim.animation[i];
                showAnim[i] = EditorGUILayout.Foldout(showAnim[i],"AnimClip"+(i+1));
                if (showAnim[i])
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("几秒后播放下个动画");
                    tempAnim.nextAnim = EditorGUILayout.FloatField(tempAnim.nextAnim);
                    EditorGUILayout.EndHorizontal();
                    for (int j = 0; j < tempAnim.bones.Length; j++)
                    {
                        RagdollBones tempBone = tempAnim.bones[j];
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(tempBone.name);
                        tempBone.rotaThis = EditorGUILayout.Toggle(tempBone.rotaThis);
                        EditorGUILayout.EndHorizontal();
                        if (tempBone.rotaThis)
                        {
                            tempBone.targetRotation = EditorGUILayout.Vector3Field("骨骼的旋转角度", tempBone.targetRotation);
                            tempBone.force = EditorGUILayout.FloatField("对该骨骼施加的力", tempBone.force);
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space();
                    }
                }
                EditorGUILayout.Space();
            }
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("欢乐喜加一"))
            {
                creator.AddAnim();
            }
            if(GUILayout.Button("删了最后一个"))
            {
                creator.ragdollAnim.DelletAnimClip();
            }
            GUILayout.EndHorizontal();
            //如果动画未被保存，则显示
            if (!File.Exists("Assets/Arts/" + creator.ragdollAnim.name + ".asset"))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("给动画起个名");
                animName = GUILayout.TextField(animName);
                GUILayout.EndHorizontal();
                GUILayout.Space(10);

                if (GUILayout.Button("这里进行保存"))
                {
                    AssetDatabase.CreateAsset(creator.ragdollAnim, "Assets/Arts/" + animName + ".asset");
                }
            }
            
        }
    }
}
