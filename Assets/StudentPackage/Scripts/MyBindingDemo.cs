using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MyBindingDemo : MonoBehaviour
{
    [SerializeField]
    private Button m_DamageButton;

    [SerializeField]
    private Button m_HealButton;

    [SerializeField]
    private Button m_UseMpButton;

    [SerializeField]
    private Button m_RestoreMpButton;

    [SerializeField]
    private Button m_SkillButton;

    [SerializeField]
    private TextMeshProUGUI m_HpText;

    [SerializeField]
    private TextMeshProUGUI m_MpText;

    [SerializeField]
    private TextMeshProUGUI m_HpStateText;

    [SerializeField]
    private TextMeshProUGUI m_SkillLogText;

    [SerializeField]
    private SerializableReactiveProperty<int> m_Hp = new(100);

    [SerializeField]
    private SerializableReactiveProperty<int> m_Mp = new(50);

    private const int SkillMpCost = 20;

    private void Start() 
    {
        //share 는 팩토리에서 주의 
        m_Hp.Subscribe(hp => m_HpText.text = $"HP: {hp}").AddTo(this);
        m_Mp.Subscribe(mp => m_MpText.text = $"MP: {mp}").AddTo(this);

        m_DamageButton
            .OnClickAsObservable()
            .Subscribe(_ => m_Hp.Value = Mathf.Max(0, m_Hp.Value - 10))
            .AddTo(this);

        m_HealButton
            .OnClickAsObservable()
            .Subscribe(_ => m_Hp.Value = Mathf.Min(100, m_Hp.Value + 10))
            .AddTo(this);

        m_UseMpButton
            .OnClickAsObservable()
            .Subscribe(_ => m_Mp.Value = Mathf.Max(0, m_Mp.Value - 5))
            .AddTo(this);

        m_RestoreMpButton
            .OnClickAsObservable()
            .Subscribe(_ => m_Mp.Value = Mathf.Min(50, m_Mp.Value + 5))
            .AddTo(this);

        m_Hp.Select(hp => hp <= 0 ? "쓰러짐" : hp < 30 ? "위험" : hp < 70 ? "주의" : "안전")
            .DistinctUntilChanged()
            .Subscribe(state =>
            {
                m_HpStateText.text = $"상태: {state}";
                m_HpStateText.color = state switch
                {
                    "안전" => Color.green,
                    "주의" => Color.yellow,
                    _ => Color.red,
                };
            })
            .AddTo(this);

        m_Hp
            .CombineLatest(m_Mp, (hp, mp) => hp > 0 && mp >= SkillMpCost) //bool형
            .DistinctUntilChanged()
            .Subscribe(canUse =>
            {
                m_SkillButton.interactable = canUse;
            })
            .AddTo(this);

        m_SkillButton
            .OnClickAsObservable()
            .Subscribe(_ =>
            {
                m_Mp.Value -= SkillMpCost;
                m_SkillLogText.text = $"스킬 사용! (MP - {SkillMpCost}, 시간: {Time.time})";
            })
            .AddTo(this);


    }

    private void OnDestroy()
    {
        m_Hp.Dispose();
        m_Mp.Dispose();
    }
}
