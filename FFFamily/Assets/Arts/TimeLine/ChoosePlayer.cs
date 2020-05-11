using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

// A behaviour that is attached to a playable
public class ChoosePlayer : PlayableBehaviour
{
    public PlayableDirector timeline;
    private bool pause = false;
    // Called when the owning graph starts playing
    public override void OnPlayableCreate(Playable playable)
    {
        base.OnPlayableCreate(playable);
        timeline = playable.GetGraph().GetResolver() as PlayableDirector;
    }
    public override void OnGraphStart(Playable playable)
    {
        
    }
    // Called when the owning graph stops playing
    public override void OnGraphStop(Playable playable)
    {
        
    }

    // Called when the state of the playable is set to Play
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        
        //timeline.Pause();
    }

    // Called when the state of the playable is set to Paused
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (pause)
        {
            timeline.Pause();
            EventManager.Instance.AddListener("ChooseNext", GoToNext);
            EventManager.Instance.TriggerEvent("ChoosePlayer");
        }
    }

    // Called each frame while the state is set to Play
    public override void PrepareFrame(Playable playable, FrameData info)
    {
    }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!pause&&info.weight>0)
        {
            pause = true;
        }
    }
    private void GoToNext(params object[] arg)
    {
        if (pause)
        {
            timeline.Play();
            pause = false;
        }
    }
}
