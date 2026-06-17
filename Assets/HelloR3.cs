using UnityEngine;
using R3;

public class HelloR3 : MonoBehaviour
{
    private void Start()
    {
        ////Range -> 얘가 발행
        ////Subscribe -> 얘가 구독
        //Observable.Range(1, 3).Subscribe(x => Debug.Log($"받음: {x}"));  (테스트)
    }
}
