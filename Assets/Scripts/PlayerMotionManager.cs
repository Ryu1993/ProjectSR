using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

public class PlayerMotionManager : Singleton<PlayerMotionManager>
{   
    public List<KeyValuePair<AnimationClip, AnimationClip>> controllerClips = new List<KeyValuePair<AnimationClip, AnimationClip>>();
    public Dictionary<AttackInfo, MotionPlayer> attackMotions = new Dictionary<AttackInfo, MotionPlayer>();
    public Dictionary<AttackInfo, MotionPlayer> monsterMotions = new Dictionary<AttackInfo, MotionPlayer>();
    public AnimatorOverrideController overrideController;
    public RuntimeAnimatorController originalController;
    public AnimationClip originalAttack;
    public int clipsIndex { get; private set; }


    public void SetAttackMotion()
    {
        MotionSetByCharacter(GameManager.Instance.party,attackMotions ,() => print("로딩"));
        MotionSetByCharacter(GameManager.Instance.enemy,monsterMotions, () => print("로딩"));
        foreach (var clip in originalController.animationClips)
            controllerClips.Add(new KeyValuePair<AnimationClip, AnimationClip>(clip, clip));
        overrideController.ApplyOverrides(controllerClips);
        for (int i = 0; i < controllerClips.Count; i++)
        {
            if (controllerClips[i].Key == originalAttack)
                clipsIndex = i;
        }
    }
    private void MotionSetByCharacter(List<Character> characters, Dictionary<AttackInfo, MotionPlayer> dictionary,Action callback = null)
    {
        foreach (Character character in characters)
            foreach (var info in character.attackList)
                if (!dictionary.ContainsKey(info))
                {
                    MotionPlayer player = Activator.CreateInstance(Type.GetType(info.name)) as MotionPlayer;
                    IPlayerMotionable playerMotionable = player as IPlayerMotionable;
                    if (player == null) continue;
                    info.attackMotion.LoadAssetAsync<AnimationClip>().Completed += 
                        (handle) => { playerMotionable.motionClip = handle.Result;
                                      callback?.Invoke(); };
                    info.attackEffect.InstantiateAsync(Vector3.down, Quaternion.identity).Completed += 
                        (handle) => { player.effect = handle.Result;
                                      player.effect.SetActive(false);
                                      callback?.Invoke(); };
                    dictionary.Add(info, player);
                }
    }

 


}
