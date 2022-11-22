using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using Unity.VisualScripting.FullSerializer;
using UnityEngine.Video;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;

public class Fireball : CurveMotionPlayer
{
    protected override void PlayAlongPath(Action action)
    {
        trailParticle.gameObject.SetActive(true);
        trailParticle.Play();
        trailParticle.transform.DOPath(ways, 2f, PathType.CubicBezier).SetEase(Ease.OutExpo).OnComplete(() =>
        {
            trailParticle.gameObject.SetActive(false);
            diffusionParitcle.Play();
            action.Invoke();
        });
    }
}
