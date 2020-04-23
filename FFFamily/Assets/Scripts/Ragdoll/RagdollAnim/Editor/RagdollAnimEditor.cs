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
    ConfigurableJoint joint = null;

    RagdollBones curBone;
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
            EditorUtility.SetDirty(creator.ragdollAnim);
            Undo.RecordObject(creator.ragdollAnim, "Change Anim");
            GUILayout.Space(30);
            Undo.RecordObject(this, "change");
            GUILayout.Label("开启辅助调整后(0,0,0)状态下,x为绕蓝色轴旋转,y为绕绿色轴旋转,z为绕红色轴旋转,数字不宜太大");
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
                            if(GUILayout.Button("开启/关闭辅助调整")&&Application.isPlaying)
                            {
                                GameObject obj = creator.ragdoll.transform.FindC(tempBone.name);
                                if (debugObj!=obj)
                                {
                                    debugObj = obj;
                                    joint = obj.GetComponent<ConfigurableJoint>();
                                    curBone = tempBone;
                                    creator.ragdoll.GetComponent<APRController>().enabled = false;
                                }
                                else
                                {
                                    debugObj = null;
                                    joint = null;
                                    creator.ragdoll.GetComponent<APRController>().enabled = true;
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
            if(GUILayout.Button("新增动画片段"))
            {
                creator.AddAnim();
            }
            if(GUILayout.Button("删除动画片段"))
            {
                creator.ragdollAnim.DelletAnimClip();
            }
            GUILayout.EndHorizontal();
            //如果动画未被保存，则显示
            if (!File.Exists("Assets/Resources/RagdollAnims/" + creator.ragdollAnim.name + ".asset"))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("动画名");
                animName = GUILayout.TextField(animName);
                GUILayout.EndHorizontal();
                GUILayout.Space(10);

                if (GUILayout.Button("保存"))
                {
                    AssetDatabase.CreateAsset(creator.ragdollAnim, "Assets/Resources/RagdollAnims/" + animName + ".asset");
                }
            }
            else if (File.Exists("Assets/Resources/RagdollAnims/" + creator.ragdollAnim.name + ".asset")&&Application.isPlaying)
            {
                if (GUILayout.Button("保存四元数数据"))
                {
                    SaveAnim();
                }
            }
        }
    }
    private void OnSceneGUI()
    {
            if(debugObj!=null)
            {
            ConfigurableJoint j = debugObj.GetComponent<ConfigurableJoint>();
            Quaternion q1 = Quaternion.LookRotation(joint.axis, joint.secondaryAxis);
            q1 = debugObj.transform.rotation*q1;
            Handles.PositionHandle(debugObj.transform.position,q1);
            joint.targetRotation = new Quaternion(curBone.targetRotation.x, curBone.targetRotation.y, curBone.targetRotation.z, 1);
            }
        
    }

    private void SaveAnim()
    {
        Undo.RecordObject(creator.ragdollAnim, "Save Quaternion");
        for (int i = 0; i < creator.ragdollAnim.animation.Count; i++)
        {
            RagdollClip tempAnim = creator.ragdollAnim.animation[i];

                for (int j = 0; j < tempAnim.bones.Length; j++)
                {
                
                    RagdollBones tempBone = tempAnim.bones[j];
                if (tempBone.rotaThis)
                {
                    GameObject obj = creator.ragdoll.transform.FindC(tempBone.name);
                    tempBone.jointTarget = new Quaternion(tempBone.targetRotation.x, tempBone.targetRotation.y, tempBone.targetRotation.z, 1);
                }
                }
            
        }
        Debug.Log("保存成功");
    }
}
