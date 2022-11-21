using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MotionPlayer
{
    public AnimationClip motionClip;
    public ParticleSystem effectParticle;
    public AnimatorOverrideController overrideController { get { return PlayerMotionManager.Instance.overrideController; } }
    public abstract void Play(Animator order,Vector3 target,Action attackAction);
}
