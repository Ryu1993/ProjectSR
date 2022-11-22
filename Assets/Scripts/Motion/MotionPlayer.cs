using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MotionPlayer
{
    public AnimationClip motionClip;
    public GameObject effect;
    public AnimatorOverrideController overrideController { get { return PlayerMotionManager.Instance.overrideController; } }
    public int animation { get { return PlayerMotionManager.Instance.parameterAnimation; } }
    public abstract void Play(Animator order,Vector3 target,Action attackAction);
}
