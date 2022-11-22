using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public abstract class CurveMotionPlayer : MotionPlayer
{
    protected Vector3[] ways = new Vector3[3];
    protected List<Vector3Int> wayObstacle;
    protected ParticleSystem _diffusionParitcle;
    protected ParticleSystem _trailParticle;
    protected ParticleSystem diffusionParitcle
    {
        get
        {
            if (_diffusionParitcle == null)
            {
                effect.transform.GetChild(0).TryGetComponent(out _diffusionParitcle);
                _diffusionParitcle.transform.SetParent(null, true);
            }

            return _diffusionParitcle;
        }
    }
    protected ParticleSystem trailParticle
    {
        get
        {
            if (_trailParticle == null)
            {
                effect.transform.GetChild(0).TryGetComponent(out _trailParticle);
                _trailParticle.transform.SetParent(null, true);
            }
            return _trailParticle;
        }
    }




    public override void Play(Animator order, Vector3 target, Action attackAction)
    {
        overrideController.animationClips[1] = motionClip;
        order.runtimeAnimatorController = overrideController;
        order.SetInteger(animation, 2);
        order.Update(0);
        order.SetInteger(animation, 0);
        WayPointSet(order, target);
        PlayAlongPath(attackAction);
    }

    protected void WayPointSet(Animator order,Vector3 target)
    {

        Vector3 orderPos = order.transform.position;
        Vector3Int targetInt = target.ToInt();
        Vector3Int maxHeight = Vector3Int.zero;
        trailParticle.transform.position = orderPos;
        diffusionParitcle.transform.position = target;
        wayObstacle = CubeCheck.CubeRaySurface(orderPos, target);
        foreach (var obstacle in wayObstacle)
        {
            if (obstacle == targetInt) continue;
            if (maxHeight.y <= obstacle.y)
                maxHeight = obstacle;
        }
        if (maxHeight != Vector3Int.zero)
        {
            ways[0] = target;
            ways[1] = order.transform.position;
            ways[1].y += maxHeight.y;
            ways[2] = target;
            ways[2].y += maxHeight.y;

        }
        else
        {
            ways[0] = target;
            ways[1] = order.transform.position + Vector3.up * 0.5f;
            ways[2] = target + Vector3.up * 0.5f;
        }          
    }

    protected abstract void PlayAlongPath(Action action);



}
