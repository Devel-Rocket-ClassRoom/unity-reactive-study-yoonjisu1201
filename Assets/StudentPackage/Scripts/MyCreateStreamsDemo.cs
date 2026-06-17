using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MyCreateStreamsDemo : MonoBehaviour
{
    [SerializeField]
    private Button m_RangeButton;

    [SerializeField]
    private Button m_TimerButton;

    [SerializeField]
    private Button m_IntervalToggleButton;

    [SerializeField]
    private TextMeshProUGUI m_IntervalToggleLabel;

    [SerializeField]
    private Button m_SubjectButton;

    [SerializeField]
    private Button m_EventButton;

    [SerializeField]
    private TextMeshProUGUI m_LogText;

    private readonly Subject<string> m_Subject = new();

    private event Action<int> LegacyScoreEvent;

    private readonly Queue<string> m_LogLines = new();
    private IDisposable m_IntervalSubscription;
    private int m_SubjectCount;
    private int m_LegacyScore;

    //연습
    private void Start()
    {
        m_RangeButton
            .OnClickAsObservable()
            .Subscribe(_ =>
            {
                Observable
                .Range(1, 4)
                .Subscribe(
                    x => Log($"Range OnNext: {x}"),
                    result => Log($"Range OnCompleted: {result}"))
                .AddTo(this);
            }
            ).AddTo(this);

        m_TimerButton
            .OnClickAsObservable()
            .Subscribe(_ =>
            {
                Observable
                .Timer(TimeSpan.FromSeconds(2))
                .Subscribe(_ => Log("2초 경과!"))
                .AddTo(this);
            })
            .AddTo(this);

        m_IntervalToggleButton
            .OnClickAsObservable()
            .Subscribe(_ =>
            {
                if (m_IntervalSubscription == null)
                {
                    int tick = 0;
                    m_IntervalSubscription = 
                    Observable.Interval(TimeSpan.FromSeconds(1))
                    .Subscribe(_ => Log($"Interval Tick {tick++}"))
                    .AddTo (this);

                    Log("Interval 시작");
                    SetToggleLabel("Interval 중지");
                }
                else
                {
                    m_IntervalSubscription.Dispose ();
                    m_IntervalSubscription = null;

                    Log("Interval 중지");
                    SetToggleLabel("Interval 시작");
                }
            })
            .AddTo(this);

        m_Subject.Subscribe(msg => Log($"Subject 수신 {msg}")).AddTo(this);
        m_SubjectButton
            .OnClickAsObservable()
            .Subscribe(_ => m_Subject.OnNext($"메세지: {++m_SubjectCount}")) //OnNext 발행시키는거
            .AddTo(this);

        Observable.FromEvent<int>(
            h => LegacyScoreEvent += h,
            h => LegacyScoreEvent -= h
        ).Subscribe(score => Log($"FromEvent 수신: 점수 {score}"))
        .AddTo(this);

        m_EventButton
            .OnClickAsObservable()
            .Subscribe(_ => LegacyScoreEvent?.Invoke(m_LegacyScore += 10))
            .AddTo(this);
    }

    private void SetToggleLabel(string text)
    {
        if (m_IntervalToggleLabel != null)
            m_IntervalToggleLabel.text = text;
    }

    private void Log(string message)
    {
        m_LogLines.Enqueue(message);
        while (m_LogLines.Count > 7)
            m_LogLines.Dequeue();
        if (m_LogText != null)
            m_LogText.text = string.Join("\n", m_LogLines);
    }

    private void OnDestroy()
    {
        m_Subject.Dispose();
    }
}
