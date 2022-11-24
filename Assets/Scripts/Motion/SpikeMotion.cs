using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpikeMotion : MotionPlayer
{
   
    public override YieldInstruction Play(Animator order, Vector3 target, Action attackAction)
    {
        AnimatorSet(order);
        return PlayerMotionManager.Instance.StartCoroutine(EffectPlay(order.transform.position, target, attackAction));
    }
    protected virtual IEnumerator EffectPlay(Vector3 origin, Vector3 target,Action attackAction)
    {
        var main = particleEffect.main;
        main.startLifetime = Vector3.Distance(origin, target) * 0.025f;
        particleEffect.transform.position = origin;
        particleEffect.transform.LookAt(target);
        particleEffect.gameObject.SetActive(true);
        yield return animationDelay;
        particleEffect.Play();
        yield return effectDelay;
        attackAction?.Invoke();
    }

 



}
