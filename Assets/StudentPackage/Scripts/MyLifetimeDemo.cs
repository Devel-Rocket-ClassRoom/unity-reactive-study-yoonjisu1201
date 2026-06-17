using System;
using System.Text;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MyLifetimeDemo : MonoBehaviour
{
    [SerializeField]
    private Button m_AddSubscriptionButton;

    [SerializeField]
    private Button m_DisposeAllButton;

    [SerializeField]
    private Button m_LeakButton;

    [SerializeField]
    private Button m_TrackerButton;

    [SerializeField]
    private Button m_DestroyProbeButton;

    [SerializeField]
    private GameObject m_ProbeObject;

    [SerializeField]
    private TextMeshProUGUI m_StatusText;

    [SerializeField]
    private TextMeshProUGUI m_TrackerText;

    private readonly CompositeDisposable m_Disposables = new();
    private int m_AddedCount;
    private int m_LeakedCount;

    private void Awake()
    {
        ObservableTracker.EnableTracking = true;
        ObservableTracker.EnableStackTrace = true;
    }

    private void Start() 
    {
        m_AddSubscriptionButton
            .OnClickAsObservable()
            .Subscribe(_ =>
            {
                int id = ++m_AddedCount;
                Observable
                .Interval(TimeSpan.FromSeconds(5))
                .Subscribe(_ => Debug.Log($"MyLifeTimeDemo #{id} tick"))
                .AddTo(m_Disposables);
                UpdateStatus($"구독 #{id} 추가");
            })
            .AddTo(this);

        m_DisposeAllButton
            .OnClickAsObservable()
            .Subscribe(_ =>
            {
                m_Disposables.Clear();
                UpdateStatus($"전부 해제");
            })
            .AddTo(this);

        m_LeakButton
            .OnClickAsObservable()
            .Subscribe(_ =>
            {
                int id = ++m_AddedCount;
                Observable
                .Interval(TimeSpan.FromSeconds(5))
                .Subscribe(_ => Debug.Log($"MyLifeTimeDemo 누수 #{id} tick"));
                UpdateStatus($"구독 #{id} 추가");
            })
            .AddTo(this);

        m_DestroyProbeButton
            .OnClickAsObservable()
            .Subscribe(x =>
            {
                Destroy(m_ProbeObject);
                UpdateStatus("프로브 파괴");
            })
            .AddTo (this);

        m_TrackerButton
            .OnClickAsObservable()
            .Subscribe(_ => DumpTracker())
            .AddTo(this);
    }

    private void DumpTracker()
    {
        var sb = new StringBuilder();
        int count = 0;
        ObservableTracker.ForEachActiveTask(state =>
        {
            count++;
            if (count <= 8)
                sb.AppendLine($"#{state.TrackingId} {state.FormattedType}");
        });
        if (count > 8)
            sb.AppendLine($"... 외 {count - 8}개");
        if (m_TrackerText != null)
            m_TrackerText.text = $"활성 구독 {count}개\n{sb}";
    }

    private void UpdateStatus(string lastAction)
    {
        if (m_StatusText != null)
            m_StatusText.text = $"관리 중 {m_Disposables.Count}개 · 누수 {m_LeakedCount}개 | {lastAction}";
    }

    private void OnDestroy()
    {
        m_Disposables.Dispose();
    }
}
