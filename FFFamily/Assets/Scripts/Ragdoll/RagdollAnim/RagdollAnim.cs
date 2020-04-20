using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RagdollBones
{
    [HideInInspector]
    public string name;
    public bool rotaThis = false;

    [Header("该关节目标角度")]
    public Vector3 targetRotation;
    [Header("向该关节施加的力")]
    public float force;

    public Quaternion jointTarget;

}
[System.Serializable]
public class RagdollClip
{
    [HideInInspector]
    public string name;
    public float nextAnim;
    public RagdollClip(GameObject ragdoll)
    {
        Root = ragdoll.transform.FindC("APR_Root");
        Body = ragdoll.transform.FindC("APR_Body");
        Head = ragdoll.transform.FindC("APR_Head");
        UpperRightArm = ragdoll.transform.FindC("APR_UpperRightArm");
        LowerRightArm = ragdoll.transform.FindC("APR_LowerRightArm");
        RightHand = ragdoll.transform.FindC("APR_RightHand");
        UpperLeftArm = ragdoll.transform.FindC("APR_UpperLeftArm");
        LowerLeftArm = ragdoll.transform.FindC("APR_LowerLeftArm");
        LeftHand = ragdoll.transform.FindC("APR_LeftHand");
        UpperRightLeg = ragdoll.transform.FindC("APR_UpperRightLeg");
        LowerRightLeg = ragdoll.transform.FindC("APR_LowerRightLeg");
        RightFoot = ragdoll.transform.FindC("APR_RightFoot");
        UpperLeftLeg = ragdoll.transform.FindC("APR_UpperLeftLeg");
        LowerLeftLeg = ragdoll.transform.FindC("APR_LowerLeftLeg");
        LeftFoot = ragdoll.transform.FindC("APR_LeftFoot");
        GameObject[] obj = new GameObject[]
        {
			//array index numbers
			
			//0
			Root,
			//1
			Body,
			//2
			Head,
			//3
			UpperRightArm,
			//4
			LowerRightArm,
			//5
			UpperLeftArm,
			//6
			LowerLeftArm,
			//7
			UpperRightLeg,
			//8
			LowerRightLeg,
			//9
			UpperLeftLeg,
			//10
			LowerLeftLeg,
			//11
			RightFoot,
			//12
			LeftFoot,
            //13
            RightHand.gameObject,
            //14
            LeftHand.gameObject
        };
        for (int i = 0; i < obj.Length; i++)
        {
            bones[i] = new RagdollBones();
            bones[i].name = obj[i].name;
        }
    }


    //Root
    private GameObject Root;

    //Body
    private GameObject Body;

    //Head
    private GameObject Head;

    //UpperRightArm
    private GameObject UpperRightArm;

    //LowerRightArm
    private GameObject LowerRightArm;

    //RightHand
    private GameObject RightHand;

    //UpperLeftArm
    private GameObject UpperLeftArm;

    //LowerLeftArm
    private GameObject LowerLeftArm;

    //RightHand
    private GameObject LeftHand;

    //UpperRightLeg
    private GameObject UpperRightLeg;

    //LowerRightLeg
    private GameObject LowerRightLeg;

    //RightFoot
    private GameObject RightFoot;

    //UpperLeftLeg
    private GameObject UpperLeftLeg;

    //LowerLeftLeg
    private GameObject LowerLeftLeg;

    //LeftFoot
    private GameObject LeftFoot;
    public RagdollBones[] bones = new RagdollBones[15];

}
public class RagdollAnim : ScriptableObject
{
    private GameObject ragdoll;
    public RagdollAnim(GameObject rag)
    {
        ragdoll = rag;
    }
    public List<RagdollClip> animation;

    public void CreateNewAnim()
    {
        animation = new List<RagdollClip>();
        animation.Add(new RagdollClip(ragdoll));
        animation[animation.Count - 1].name = "AnimationClip" + animation.Count;
    }

    public void AddAnimClip(GameObject obj)
    {
        animation.Add(new RagdollClip(obj));
        animation[animation.Count - 1].name = "AnimationClip" + animation.Count;
    }

    public void DelletAnimClip()
    {
        animation.RemoveAt(animation.Count - 1);
        animation[animation.Count - 1].name = "AnimationClip" + animation.Count;
    }

}
