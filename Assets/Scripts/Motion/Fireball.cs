using System.Collections;
using UnityEngine;
using DG.Tweening;
using System;


public class Fireball : CurveMotionPlayer,IPlayerMotionable
{
    public AnimationClip motionClip { get ; set; }
    private ParticleSystem ballEffect;
    private ParticleSystem explosionEffect;
    private TweenCallback complete;

    public override void Play(Character order, Vector3 target)
    {
        base.Play(order, target);
        WayPointSet(Vector3Int.RoundToInt(order.transform.position), Vector3Int.RoundToInt(target));
        explosionEffect.transform.position = target;
        ballEffect.transform.position = order.transform.position + Vector3.up * 1.5f;
        MotionManager.Instance.MotionChange(order.Animator, Motion.Attack);
        effect.gameObject.SetActive(true);
        ballEffect.gameObject.SetActive(true);
        ballEffect.Play();
        ballEffect.transform.DOPath(ways, 1, PathType.CubicBezier).SetDelay(attackDelay).onComplete+=complete;       
    }

    public override void Set()
    {
        effect.transform.GetChild(0).TryGetComponent(out ballEffect);
        effect.transform.GetChild(1).TryGetComponent(out explosionEffect);
        complete = Attack;
    }

    protected override void Attack()
    {
        ballEffect.Stop();
        ballEffect.gameObject.SetActive(false);
        explosionEffect.gameObject.SetActive(true);
        explosionEffect.Play();
        RangeAttack();
        base.Attack();
    }
}
