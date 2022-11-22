using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

public class PlayerMotionManager : Singleton<PlayerMotionManager>
{
    public Dictionary<AttackInfo,MotionPlayer> attackMotions = new Dictionary<AttackInfo,MotionPlayer>();
    public readonly int parameterAnimation = Animator.StringToHash("animation");
    public AnimatorOverrideController overrideController;
    public Ease ease;

    private void Awake()
    {
        SetAttackMotion();
    }

    public void SetAttackMotion()
    {
        MotionSetByCharacter(GameManager.Instance.party, () => print("로딩"));
        MotionSetByCharacter(GameManager.Instance.enemys, () => print("로딩"));
    }
    private void MotionSetByCharacter(List<Character> characters,Action callback)
    {
        foreach (Character character in characters)
            foreach (var info in character.attackList)
                if (!attackMotions.ContainsKey(info))
                {
                    MotionPlayer player = Activator.CreateInstance(Type.GetType(info.name)) as MotionPlayer;
                    if (player == null) continue;
                    info.attackMotion.LoadAssetAsync<AnimationClip>().Completed += 
                        (handle) => { player.motionClip = handle.Result; 
                                      callback.Invoke(); };
                    info.attackEffect.InstantiateAsync(Vector3.zero, Quaternion.identity).Completed += 
                        (handle) => { player.effect = handle.Result;
                                      callback.Invoke(); };
                    attackMotions.Add(info, player);
                }
    }

 


}
