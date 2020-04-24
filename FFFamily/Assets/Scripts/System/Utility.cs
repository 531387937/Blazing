using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    private static Dictionary<string, RagdollAnim> anims = new Dictionary<string, RagdollAnim>();
    private static string animPath = "RagdollAnims/";
    public static RagdollAnim LoadAnim(string name)
    {
        RagdollAnim anim;
        if (!anims.TryGetValue(name, out anim))
        {
            anim = Resources.Load<RagdollAnim>(animPath + name);
            anims.Add(name, anim);
        }
        return anim;
    }
}
