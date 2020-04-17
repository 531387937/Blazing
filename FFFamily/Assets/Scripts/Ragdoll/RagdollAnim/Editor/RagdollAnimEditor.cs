using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditorInternal;
[CustomEditor(typeof(RagdollAnimCreator))]
[CanEditMultipleObjects]
public class RagdollAnimEditor : Editor
{
    private RagdollAnimCreator creator;
    private string animName;
    bool[] showAnim;
    GameObject debugObj = null;

    int curAnim;
    int curBone;
    private void OnEnable()
    {
        creator = (RagdollAnimCreator)target;
        showAnim = new bool[15];
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //Undo.RegisterFullObjectHierarchyUndo(target, "RagdollAnimCreator");
        if(creator.ragdollAnim==null)
        {
            if (GUILayout.Button("创建一个新动画文件"))
            {
                creator.CreateNewAnim();
            }
        }
        else
        {
            Undo.RecordObject(creator.ragdollAnim, "Change Anim");
            GUILayout.Space(30);
            Undo.RecordObject(this, "change");
            for (int i = 0;i<creator.ragdollAnim.animation.Count;i++)
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
                            
                            if(GUILayout.Button("校准布娃娃目前旋转角度"))
                            {
                                GameObject obj = creator.ragdoll.transform.FindC(tempBone.name);
                                tempBone.targetRotation = obj.transform.localRotation.eulerAngles;
                            }
                            if(GUILayout.Button("开启/关闭辅助调整"))
                            {
                                GameObject obj = creator.ragdoll.transform.FindC(tempBone.name);
                                if (debugObj!=obj)
                                {
                                    debugObj = obj;
                                    curAnim = i;
                                    curBone = j;
                                }
                                else
                                {
                                    debugObj = null;
                                }
                            }
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
            if (!File.Exists("Assets/Resources/RagdollAnims/" + creator.ragdollAnim.name + ".asset"))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("给动画起个名");
                animName = GUILayout.TextField(animName);
                GUILayout.EndHorizontal();
                GUILayout.Space(10);

                if (GUILayout.Button("这里进行保存"))
                {
                    AssetDatabase.CreateAsset(creator.ragdollAnim, "Assets/Resources/RagdollAnims/" + animName + ".asset");
                }
            }
            
        }
    }
    private void OnSceneGUI()
    {
            if(debugObj!=null)
            {
                Handles.color = Color.red;
                Handles.CubeHandleCap(1, debugObj.transform.position, Quaternion.Euler(debugObj.transform.parent.InverseTransformVector(creator.ragdollAnim.animation[curAnim].bones[curBone].targetRotation)), 0.5f, EventType.Repaint);
            Handles.PositionHandle(debugObj.transform.position, Quaternion.Euler(debugObj.transform.parent.InverseTransformVector(creator.ragdollAnim.animation[curAnim].bones[curBone].targetRotation)));
                EditorGUI.BeginChangeCheck();
                Quaternion q = Handles.RotationHandle(Quaternion.Euler(debugObj.transform.parent.InverseTransformVector(creator.ragdollAnim.animation[curAnim].bones[curBone].targetRotation)), debugObj.transform.position);
                if (EditorGUI.EndChangeCheck())
                {
                Undo.RecordObject(creator.ragdollAnim, "Free Rotate");
                creator.ragdollAnim.animation[curAnim].bones[curBone].targetRotation =q.eulerAngles;
                }
            }
        
    }
}
