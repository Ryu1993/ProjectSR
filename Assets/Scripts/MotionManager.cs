using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionManager : Singleton<MotionManager>
{
    public List<KeyValuePair<AnimationClip, AnimationClip>> playerControllerClips = new List<KeyValuePair<AnimationClip, AnimationClip>>();
    public Dictionary<AttackInfo, MotionPlayer> playerAttackMotions = new Dictionary<AttackInfo, MotionPlayer>();
    public Dictionary<AttackInfo, MotionPlayer> monsterAttackMotions = new Dictionary<AttackInfo, MotionPlayer>();
    public AnimatorOverrideController playerOverrideController;
    public RuntimeAnimatorController playerOriginController;
    public AnimationClip playerOriginalAttackClip;
    public int playerAttackClipIndex;

    public void PlayerAnimationChangeSet(Character character)
    {
        playerOriginController = character.Animator.runtimeAnimatorController;
        playerControllerClips.Clear();
        for(int i = 0; i<playerOriginController.animationClips.Length; i++)
        {
            AnimationClip clip = playerOriginController.animationClips[i];
            playerControllerClips.Add(new KeyValuePair<AnimationClip, AnimationClip>(clip, clip));
            if (clip == playerOriginalAttackClip)
                playerAttackClipIndex = i;
        }
        playerOverrideController.ApplyOverrides(playerControllerClips);
        
    }

    private void Awake()
    {
        MotionSetByCharacter(GameManager.Instance.party, playerAttackMotions);
        MotionSetByCharacter(GameManager.Instance.enemy, monsterAttackMotions);
        
    }




    private void MotionSetByCharacter(List<Character> characters, Dictionary<AttackInfo, MotionPlayer> dictionary, Action callback = null)
    {
        foreach (Character character in characters)
            foreach (var info in character.attackList)
                if (!dictionary.ContainsKey(info))
                {
                    MotionPlayer player = Activator.CreateInstance(Type.GetType(info.name)) as MotionPlayer;
                    PlayerMotion playerMotion = player as PlayerMotion;
                    player.overrideController = playerOverrideController;
                    if (player == null) continue;                    
                    if(playerMotion != null)
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
                            callback?.Invoke();
                        };
                    dictionary.Add(info, player);
                }
    }





}
