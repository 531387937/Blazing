using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class ChoosePlayers : PlayableAsset
{
    // Factory method that generates a playable based on this asset
    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        ChoosePlayer choosePlayer = new ChoosePlayer();
        return ScriptPlayable<ChoosePlayer>.Create(graph,choosePlayer);
    }
}
