using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MotionPlayer
{
    public GameObject effect;
    public AnimationClip motionClip;
    public AnimatorOverrideController overrideController { get { return PlayerMotionManager.Instance.overrideController; } }
    public int animation { get { return PlayerMotionManager.Instance.parameterAnimation; } }
    public abstract void Play(Animator order,Vector3 target,Action attackAction);

    public void AnimatorSet(Animator order)
    {
        KeyValuePair<AnimationClip, AnimationClip> motionClipPair = new KeyValuePair<AnimationClip, AnimationClip>(PlayerMotionManager.Instance.originalAttack, motionClip);
        PlayerMotionManager.Instance.controllerClips[PlayerMotionManager.Instance.clipsIndex] = motionClipPair;
        overrideController.ApplyOverrides(PlayerMotionManager.Instance.controllerClips);


        order.runtimeAnimatorController = overrideController;
        order.SetInteger(animation, 2);
        order.Update(0);
        order.SetInteger(animation, 0);
        PlayerMotionManager.Instance.originalAttack = motionClip;
    }
}
