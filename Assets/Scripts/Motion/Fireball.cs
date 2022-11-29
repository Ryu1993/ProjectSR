using System.Collections;
using UnityEngine;
using DG.Tweening;
using System;


public class Fireball : CurveMotionPlayer
{

    protected override IEnumerator PlayAlongPath(Action action)
    {
        //yield return animationDelay;
        //trailParticle.transform.position = trailParticle.transform.forward + Vector3.up;
        trailParticle.gameObject.SetActive(true);
        trailParticle.Play();
        yield return trailParticle.transform.DOPath(ways, 2f, PathType.CubicBezier).SetEase(Ease.InExpo).OnComplete(() =>
        {
            trailParticle.gameObject.SetActive(false);
            diffusionParitcle.Play();
            action.Invoke();
        }).WaitForCompletion();
    }

    protected override WaitUntil animationDelay
    {
        get
        {
            if (_animationDelay == null)
                _animationDelay = new WaitUntil(() => targetAniamtor.CurStateProgress(AnimationHash.attack)>0.5f);
            return _animationDelay;
        }
    }
}
