using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;

public class SpikeMotion : MotionPlayer
{

    protected ParticleSystem spikeEffect;
    protected float effectDuration;
    private TweenCallback complete;
    float progress;

    public override void Play(Character order, Vector3 target)
    {
        base.Play(order,target);
        spikeEffect.transform.transform.position = order.transform.position + order.transform.forward;
        var effectMain = spikeEffect.main;
        effectMain.startSpeed = Vector3.Distance(order.transform.position, target) * 5f;
        progress = 0;
        MotionManager.Instance.MotionChange(order.Animator, Motion.Attack);
        DOTween.To(() => progress, (x) => progress = x, effectDuration, effectDuration).
            SetDelay(attackDelay).
            OnStart(() => spikeEffect.gameObject.SetActive(true)).
            OnComplete(complete);
    }

    public override void Set()
    {
        effect.transform.TryGetComponent(out spikeEffect);
        effectDuration = spikeEffect.main.duration;
        complete = Attack;
    }

    protected override void Attack()
    {
        PenetrateAttack();
        RangeAttack();
        base.Attack();
    }


}
