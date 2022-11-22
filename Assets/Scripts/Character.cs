using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    private Animator _animator;
    public List<AttackInfo> attackList;
    public int moveablePoint;
    public int jumpableHeight;











    public Animator animator { get { if (_animator == null) transform.TryGetComponent(out _animator); return _animator; } }
}
