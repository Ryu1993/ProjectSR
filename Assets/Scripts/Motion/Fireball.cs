using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using Unity.VisualScripting.FullSerializer;
using UnityEngine.Video;

public class Fireball : MotionPlayer
{
    Vector3[] path = new Vector3[6];
    List<Vector3Int> pathCheck;
    public override void Play(Animator order, Vector3 target,Action attackAction)
    {
        overrideController.animationClips[1] = motionClip;
        order.runtimeAnimatorController = overrideController;
        order.SetInteger("animation", 2);

        Vector3 orderPos = order.transform.position;
        pathCheck = CubeCheck.CubeRayCastAll(order.transform.position.ToInt(), target.ToInt());
        float maxHeight = 0;
        foreach (Vector3Int pathPos in pathCheck)
        {
            Debug.Log(pathPos);
            if (pathPos.y >= maxHeight)
                maxHeight = pathPos.y;
        }
        if (pathCheck.Count == 0)
        {
            maxHeight = 1;
        }          
        path[2] = (orderPos + target) / 2 + Vector3.up * (maxHeight+1);
        path[1] = (orderPos + path[2]) / 2;
        path[0] = (orderPos + path[1]) / 2;
        path[3] = (target + path[2]) / 2;
        path[4] = (target + path[3]) / 2;
        path[5] = target;
        foreach (var temp in path)
            Debug.Log(temp);

        effectParticle.transform.position = order.transform.position;
        effectParticle.gameObject.SetActive(true);
        effectParticle.transform.DOPath(path, 2,PathType.Linear).OnComplete(() => { attackAction.Invoke(); order.SetInteger("animation", 1);effectParticle.gameObject.SetActive(false); }) ;


    }
}
