using R3;
using System;
using UnityEngine;

public class LifeTimeProbe : MonoBehaviour
{
    void Start()
    {
        Observable
            .Interval(TimeSpan.FromSeconds(5))
            .Subscribe(_ => Debug.Log("MyLifeTimeProbe Interval"))
            .AddTo(this);
    }
}
