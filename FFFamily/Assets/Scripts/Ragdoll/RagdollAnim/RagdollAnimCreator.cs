using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class RagdollAnimCreator : MonoBehaviour
{
    public RagdollAnim ragdollAnim;
    public GameObject ragdoll;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CreateNewAnim()
    {
        ragdollAnim = new RagdollAnim(ragdoll);
        ragdollAnim.CreateNewAnim();
    }

    public void SaveAnim()
    {

    }
}
