using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class RagdollAnimCreator : MonoBehaviour
{
    public RagdollAnim ragdollAnim;
    public GameObject ragdoll;
    private Vector3 o;
    // Start is called before the first frame update
    void Start()
    {
        o = ragdoll.transform.GetChild(0).transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            ragdoll.GetComponent<APRController>().PlayAnim(ragdollAnim);
        }
    }
    private void OnGUI()
    {
        if (GUILayout.Button("播放动画"))
        {
            ragdoll.GetComponent<APRController>().PlayAnim(ragdollAnim);
        }
        if (GUILayout.Button("复位"))
        {
            ragdoll.transform.GetChild(0).transform.position = o;
        }
    }
    public void CreateNewAnim()
    {
        ragdollAnim = new RagdollAnim(ragdoll);
        ragdollAnim.CreateNewAnim();
    }
    public void AddAnim()
    {
        ragdollAnim.AddAnimClip(ragdoll);
    }
}

