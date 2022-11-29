using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MotionPlayer
{
    public GameObject effect;
    public AnimationClip motionClip;
    public AnimatorOverrideController overrideController;
    public KeyValuePair<AnimationClip, AnimationClip> motionClipPair;
    protected WaitWhile _effectDelay;
    protected WaitUntil _animationDelay;
    protected ParticleSystem _particleEffect;
    protected Animator targetAniamtor;

    public abstract YieldInstruction Play(Animator order,Vector3 target,Action attackAction);

    public virtual void AnimatorSet(Animator order)
    {
        PlayerMotionManager manager = PlayerMotionManager.Instance;
        targetAniamtor = order;
        manager.controllerClips[manager.clipsIndex] = motionClipPair;
        overrideController.ApplyOverrides(PlayerMotionManager.Instance.controllerClips);
        order.runtimeAnimatorController = overrideController;
        order.SetInteger(AnimationHash.animation, 2);
        order.Update(0);
        order.SetInteger(AnimationHash.animation, 0);
    }

    #region Property  
    protected virtual WaitUntil animationDelay
    {
        get
        {
            if (_animationDelay == null)
                _animationDelay = new WaitUntil(()=>targetAniamtor.CurStateProgress(AnimationHash.attack)>0.6f);
            return _animationDelay;
        }
    }
    protected virtual WaitWhile effectDelay
    {
        get
        {
            if (_effectDelay == null)
                _effectDelay = new WaitWhile(() => particleEffect.isPlaying);
            return _effectDelay;
        }
    }
    protected virtual ParticleSystem particleEffect
    {
        get
        {
            if (_particleEffect == null)
                effect.TryGetComponent(out _particleEffect);
            return _particleEffect;
        }
    }
    #endregion
}
