using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SMBTiming { OnEnter, OnExit, OnUpdate, OnEnd }

public class SMBEvent : StateMachineBehaviour
{
    [System.Serializable]
    public class Events
    {
        public bool fired;
        public string eventName;
        public SMBTiming timing;
        public float onUpdateFrame = 1;
    }

    [SerializeField]
    private int totalFrames;
    [SerializeField]
    private int currentFrame;
    [SerializeField]
    private float normalizedTime;
    [SerializeField]
    private float normalizedTimeUncapped;
    [SerializeField]
    private string motionTime = string.Empty;

    public List<Events> eventsList = new List<Events>();

    private bool hasParam;
    private SMBEventCurrator eventCurrator;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        hasParam = HasParameter(animator, motionTime);
        eventCurrator = animator.GetComponent<SMBEventCurrator>();
        totalFrames = GetTotalFrames(animator, layerIndex);

        normalizedTimeUncapped = stateInfo.normalizedTime;
        normalizedTime = hasParam ? animator.GetFloat(motionTime) : GetNormalizedTime(stateInfo);
        currentFrame = GetCurrentFrame(totalFrames, normalizedTime);

        if(eventCurrator != null)
        {
            foreach (Events e in eventsList)
            {
                e.fired = false;
                if (e.timing == SMBTiming.OnEnter)
                {
                    e.fired = true;
                    eventCurrator.EventString.Invoke(e.eventName);
                }
            }
        }        
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        normalizedTimeUncapped = stateInfo.normalizedTime;
        normalizedTime = hasParam ? animator.GetFloat(motionTime) : GetNormalizedTime(stateInfo);
        currentFrame = GetCurrentFrame(totalFrames, normalizedTime);

        if (eventCurrator != null)
        {
            foreach (Events e in eventsList)
            {
                if(!e.fired)
                {
                    if(e.timing == SMBTiming.OnUpdate)
                    {
                        if(currentFrame >= e.onUpdateFrame)
                        {
                            e.fired = true;
                            eventCurrator.EventString.Invoke(e.eventName);
                        }
                    }
                    else if (e.timing == SMBTiming.OnEnd)
                    {
                        if (currentFrame >= totalFrames)
                        {
                            e.fired = true;
                            eventCurrator.EventString.Invoke(e.eventName);
                        }
                    }
                }
            }
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (eventCurrator != null)
        {
            foreach (Events e in eventsList)
            {
                if (e.timing == SMBTiming.OnExit)
                {
                    e.fired = true;
                    eventCurrator.EventString.Invoke(e.eventName);
                }
            }
        }
    }

    private bool HasParameter(Animator animator, string parameterName)
    {
        if(string.IsNullOrEmpty(parameterName) || string.IsNullOrWhiteSpace(parameterName))
        {
            return false;
        }
        foreach(var parameter in animator.parameters)
        {
            if(parameter.name == parameterName)
            {
                return true;
            }
        }
        return false;
    }

    private int GetTotalFrames(Animator animator, int layerIndex)
    {
        AnimatorClipInfo[] clipInfos = animator.GetNextAnimatorClipInfo(layerIndex);
        if(clipInfos.Length == 0)
        {
            clipInfos = animator.GetCurrentAnimatorClipInfo(layerIndex);
        }

        AnimationClip clip = clipInfos[0].clip;
        return Mathf.RoundToInt(clip.length / clip.frameRate);
    }

    private float GetNormalizedTime(AnimatorStateInfo stateInfo)
    {
        return stateInfo.normalizedTime > 1 ? 1 : stateInfo.normalizedTime;
    }

    private int GetCurrentFrame(int totalFrames, float normalizedTime)
    {
        return Mathf.RoundToInt(totalFrames * normalizedTime);
    }
}
