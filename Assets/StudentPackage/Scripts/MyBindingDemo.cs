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

    private void Start() { }

    private void OnDestroy()
    {
        m_Hp.Dispose();
        m_Mp.Dispose();
    }
}
