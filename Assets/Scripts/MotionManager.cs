using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WaitMotion : CustomYieldInstruction
{
    public override bool keepWaiting
    {
        get
        {
            return !MotionManager.Instance.isMotionCompleted;
        }
    }
}

public enum Motion { Idle,Move,Jump,Attack}

public class MotionManager : Singleton<MotionManager>
{
    [HideInInspector] public Dictionary<AttackInfo, MotionPlayer> playerAttackMotions = new Dictionary<AttackInfo, MotionPlayer>();
    [HideInInspector] public Dictionary<AttackInfo, MotionPlayer> monsterAttackMotions = new Dictionary<AttackInfo, MotionPlayer>();
    private List<KeyValuePair<AnimationClip, AnimationClip>> playerControllerClips = new List<KeyValuePair<AnimationClip, AnimationClip>>();
    public AnimatorOverrideController playerOverrideController;
    public bool isMotionCompleted;
    private WaitMotion waitMotion = new WaitMotion();
    private readonly int animationHash = Animator.StringToHash("animation");

    public CustomYieldInstruction PlayerAttackSet(Character character,Vector3 target)
    {
        playerControllerClips.Clear();
        RuntimeAnimatorController playerOriginController = character.Animator.runtimeAnimatorController;        
        AnimationClip playerOriginalAttackClip = character.originalAttackClip;
        KeyValuePair<AnimationClip, AnimationClip> changePair;
        if (playerAttackMotions.TryGetValue(character.CurAttackInfo, out MotionPlayer motion))
        {
            IPlayerMotionable playerMotion = motion as IPlayerMotionable;
            changePair = new KeyValuePair<AnimationClip, AnimationClip>(playerOriginalAttackClip, playerMotion.motionClip);
        }
        else
            return null;
        for (int i = 0; i < playerOriginController.animationClips.Length; i++)
        {
            AnimationClip clip = playerOriginController.animationClips[i];
            playerControllerClips.Add(new KeyValuePair<AnimationClip, AnimationClip>(clip, clip));
            if (clip == playerOriginalAttackClip)
                playerControllerClips[i] = changePair;
        }
        playerOverrideController.ApplyOverrides(playerControllerClips);
        character.Animator.runtimeAnimatorController = playerOverrideController;
        motion.Play(character,target);
        return waitMotion;
    }

    public CustomYieldInstruction Attack(Character character,Vector3 target)
    {
        if (character is Monster)
        {
            if (monsterAttackMotions.TryGetValue(character.CurAttackInfo, out MotionPlayer motion))
                motion.Play(character, target);
            return waitMotion;
        }          
        else
            return PlayerAttackSet(character, target);
    }

    public void MotionChange(Animator playAnimator,Motion changeMotion)
    {
        playAnimator.SetInteger(animationHash, (int)changeMotion);
        playAnimator.Update(0);
        playAnimator.SetInteger(animationHash, 0);
    }


    //private void Awake()
    //{
    //    MotionSetByCharacter(GameManager.Instance.party, playerAttackMotions);
    //    MotionSetByCharacter(GameManager.Instance.enemy, monsterAttackMotions);
    //}




    private void MotionSetByCharacter(List<Character> characters, Dictionary<AttackInfo, MotionPlayer> dictionary, Action callback = null)
    {
        foreach (Character character in characters)
            foreach (var info in character.attackList)
                if (!dictionary.ContainsKey(info))
                {
                    MotionPlayer player = Activator.CreateInstance(Type.GetType(info.name)) as MotionPlayer;
                    if (player == null) continue;
                    IPlayerMotionable playerMotion = player as IPlayerMotionable;
                    if (playerMotion != null)
                    {
                        info.attackMotion.LoadAssetAsync<AnimationClip>().Completed +=
                            (handle) => {
                                playerMotion.motionClip = handle.Result;
                                callback?.Invoke();
                            };
                    }
                    info.attackEffect.InstantiateAsync(Vector3.down, Quaternion.identity).Completed +=
                        (handle) => {
                            player.effect = handle.Result;
                            player.effect.SetActive(false);
                            player.Set();
                            callback?.Invoke();
                        };
                    dictionary.Add(info, player);
                }
    }





}
