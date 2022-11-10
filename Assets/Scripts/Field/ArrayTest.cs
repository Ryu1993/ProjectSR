using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrayTest : MonoBehaviour
{


    public struct TestArray
    {
        public int num;
    }


    private void Start()
    {
        TestArray[] tests = new TestArray[10];
        TestArray[] tests2 = tests;
        tests[0].num = 2;

        print(tests2[0].num);





    }



}
