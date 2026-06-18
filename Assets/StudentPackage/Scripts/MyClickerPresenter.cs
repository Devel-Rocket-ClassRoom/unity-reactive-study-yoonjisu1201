using System;
using System.Collections.Generic;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MyClickerPresenter : MonoBehaviour
{
    [Header("입력")]
    [SerializeField]
    private Button m_ClickButton;

    [SerializeField]
    private Button m_FeverButton;

    [SerializeField]
    private Button m_ClickUpgradeButton;

    [SerializeField]
    private Button m_AutoUpgradeButton;

    [Header("표시")]
    [SerializeField]
    private TextMeshProUGUI m_GoldText;

    [SerializeField]
    private TextMeshProUGUI m_StatsText;

    [SerializeField]
    private TextMeshProUGUI m_ComboText;

    [SerializeField]
    private TextMeshProUGUI m_FeverStateText;

    [SerializeField]
    private TextMeshProUGUI m_ClickUpgradeLabel;

    [SerializeField]
    private TextMeshProUGUI m_AutoUpgradeLabel;

    [SerializeField]
    private TextMeshProUGUI m_EventLogText;

    [SerializeField]
    private Image m_ClickButtonImage;

    private static readonly TimeSpan ComboResetDelay = TimeSpan.FromMilliseconds(800);
    private static readonly TimeSpan FeverCooldown = TimeSpan.FromSeconds(15);
    private const int ComboBonusUnit = 10;

    private readonly Queue<string> m_LogLines = new();
    private MyClickerModel m_Model;

    private void Start()
    {
        m_Model = new MyClickerModel();

        BindInput();
        BindView();
    }

    private void BindInput() 
    {
        var clicks = m_ClickButton.OnClickAsObservable().Share();

        clicks.Subscribe(_ => m_Model.Click()).AddTo(this);

        var combo = clicks
            .Select(_ => 1)
            .Merge(clicks.Debounce(ComboResetDelay).Select(_ => 0))
            .Scan(0, (acc, x) => x == 0 ? 0 : acc + 1)
            .Share();

        combo.Subscribe(c => m_ComboText.text = c >= 2 ? $"{c} 콤보!" : string.Empty).AddTo(this);
        combo.Where(c => c > 0 && c % ComboBonusUnit == 0)
            .Subscribe(c =>
            {
                long bonus = c * m_Model.GoldPerClick.Value;
                m_Model.AddBonus(bonus);
                Log($"{c} 콤보 달성! 보너스 + {bonus:N0}");
            })
            .AddTo(this);

        m_FeverButton.OnClickAsObservable()
            .ThrottleFirst(FeverCooldown)
            .Subscribe(_ =>
            {
                if (m_Model.TryStartFever())
                    Log($"피버 시작! : {m_Model.FeverDuration.TotalSeconds:0} 초간 클릭 2배");
            })
            .AddTo(this);

        m_ClickUpgradeButton
            .OnClickAsObservable()
            .Subscribe(_ =>
            {
                if (m_Model.TryBuyClickUpgrade())
                {
                    Log($"클릭 강화! 클릭당 + {m_Model.GoldPerClick.Value:N0}");
                }
            })
            .AddTo(this);

        m_AutoUpgradeButton
            .OnClickAsObservable()
            .Subscribe(_ =>
            {
                if (m_Model.TryBuyAutoUpgrade())
                {
                    Log($"클릭 강화! 클릭당 + {m_Model.GoldPerSecond.Value:N0}");
                }
            })
            .AddTo(this);
    }

    private void BindView() 
    {
        m_Model.Gold.Subscribe(
            gold => m_GoldText.text = $"골드 {gold:N0}").AddTo(this);

        m_Model.CanBuyClickUpgrade.Subscribe(can => m_ClickUpgradeButton.interactable = can).AddTo(this);
        m_Model.CanBuyAutoUpgrade.Subscribe(can => m_AutoUpgradeButton.interactable = can).AddTo(this);
    
        m_Model.ClickUpgradeCost
            .Subscribe(cost => m_ClickUpgradeLabel.text = $"클릭 강화\n비용 {cost:N0}")
            .AddTo(this);

        m_Model.AutoUpgradeCost
            .Subscribe(cost => m_AutoUpgradeLabel.text = $"도우미 고용\n비용 {cost:N0}")
            .AddTo(this);

        m_Model.GoldPerClick
            .CombineLatest(m_Model.GoldPerSecond,
            (perClick, perSecond) => $"클릭당 +{perClick:N0} /  초당 +{perSecond:N0}")
            .DistinctUntilChanged()
            .Subscribe(text => m_StatsText.text = text)
            .AddTo(this);

        m_Model.IsFever.Subscribe(isFever =>
        {
            m_FeverStateText.text = isFever ? "피버 중! 클릭 2배" : "피버 (15초 쿨다운)";
            m_ClickButtonImage.color = isFever ? Color.red : Color.white;
        }).AddTo(this);
    }

    private void Log(string message)
    {
        m_LogLines.Enqueue(message);
        while (m_LogLines.Count > 5)
            m_LogLines.Dequeue();
        if (m_EventLogText != null)
            m_EventLogText.text = string.Join("\n", m_LogLines);
    }

    private void OnDestroy()
    {
        m_Model?.Dispose();
    }
}
