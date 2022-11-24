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
    public override YieldInstruction Play(Animator order, Vector3 target, Action attackAction)
    {
        AnimatorSet(order);
        WayPointSet(order.transform.position, target);
        return PlayerMotionManager.Instance.StartCoroutine(PlayAlongPath(attackAction));
    }

    protected void WayPointSet(Vector3 orderPos,Vector3 target)
    {
        Vector3Int targetInt = target.ToInt();
        Vector3Int maxHeight = Vector3Int.zero;
        trailParticle.transform.position = orderPos;
        trailParticle.transform.LookAt(target);
        trailParticle.transform.position += trailParticle.transform.forward * 0.5f;
        diffusionParitcle.transform.position = target;
        ways[0] = target;
        wayObstacle = CubeCheck.CubeRaySurface(orderPos, target);
        foreach (var obstacle in wayObstacle)
        {
            if (obstacle == targetInt) continue;
            if (maxHeight.y <= obstacle.y)
                maxHeight = obstacle;
        }
        if (maxHeight != Vector3Int.zero)
        {
            ways[1] = orderPos + new Vector3(0, maxHeight.y+1, 0);
            ways[2] = target + new Vector3(0, maxHeight.y+1, 0);
        }
        else
        {
            ways[1] = orderPos + Vector3.up;
            ways[2] = target + Vector3.up;
        }          
    }

    protected abstract IEnumerator PlayAlongPath(Action action);


    #region Property
    protected ParticleSystem diffusionParitcle
    {
        get
        {
            if (_diffusionParitcle == null)
                effect.transform.GetChild(1).TryGetComponent(out _diffusionParitcle);
            return _diffusionParitcle;
        }
    }
    protected ParticleSystem trailParticle
    {
        get
        {
            if (_trailParticle == null)
                effect.transform.GetChild(0).TryGetComponent(out _trailParticle);
            return _trailParticle;
        }
    }
    #endregion
}
