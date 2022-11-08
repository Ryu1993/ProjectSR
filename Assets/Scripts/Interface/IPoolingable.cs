using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoolingable
{
    public ObjectPool home { get; set; }
}
