using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterMotionPlayer : MotionPlayer
{
    protected WaitUntil _animationSetWaiting;

    protected virtual IEnumerator AnimationWaitSet(Action attackAction)
    {
        yield return animationSetWaiting;
        AnimatorSet(targetAniamtor);
        attackAction?.Invoke();
    }

    public override void AnimatorSet(Animator order)
    {     
        targetAniamtor.SetInteger(AnimationHash.animation, 2);
        targetAniamtor.Update(0);
        targetAniamtor.SetInteger(AnimationHash.animation, 0);
    }

    public override YieldInstruction Play(Animator order, Vector3 target, Action attackAction)
    {
        targetAniamtor = order;
        return PlayerMotionManager.Instance.StartCoroutine(AnimationWaitSet(attackAction));
    }



    protected WaitUntil animationSetWaiting
    {
        get
        {
            if (_animationSetWaiting == null)
                _animationSetWaiting = new WaitUntil(() => targetAniamtor.CurState(AnimationHash.idle));
            return _animationSetWaiting;
        }
    }


}
