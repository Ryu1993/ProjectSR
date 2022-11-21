using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public List<AttackInfo> attackList;
    private Animator _animator;
    public Animator animator { get { if (_animator == null) transform.TryGetComponent(out _animator); return _animator;  } }
}
