using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionTest : MonoBehaviour
{
    public List<Character> testcharacters;
    public Transform target;
    Character testC;
    bool isPlaying;

    private void Start()
    {
        MotionManager.Instance.TestSet(testcharacters);
        testC = testcharacters[0];
        testC.CurAttackInfo = testC.attackList[0];
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.X)&!isPlaying)
        {
            StartCoroutine(Attack());
        }
    }

    private IEnumerator Attack()
    {
        isPlaying = true;
        yield return MotionManager.Instance.Attack(testC, target.transform.position);
        isPlaying = false;
    }


}
