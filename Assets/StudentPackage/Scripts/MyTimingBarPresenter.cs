using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using Unity.Properties;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MyTimingBarPresenter : MonoBehaviour
{
    [Header("트랙 / 마커")]
    [SerializeField]
    private Image m_TrackImage;

    [SerializeField]
    private RectTransform m_Marker;

    [Header("입력")]
    [SerializeField]
    private Button m_HitButton;

    [SerializeField]
    private Button m_StartButton;

    [Header("표시")]
    [SerializeField]
    private TextMeshProUGUI m_ScoreText;

    [SerializeField]
    private TextMeshProUGUI m_ComboText;

    [SerializeField]
    private TextMeshProUGUI m_AttemptText;

    [SerializeField]
    private TextMeshProUGUI m_JudgementText;

    [SerializeField]
    private TextMeshProUGUI m_StateText;

    [Header("설정")]
    [SerializeField]
    private float m_MarkerSpeed = 0.6f;

    [SerializeField]
    private int m_AttemptsPerRound = 12;

    //Presenter에서 관리할 상태들
    private readonly ReactiveProperty<float> m_Phase = new(0.5f);
    private readonly ReactiveProperty<bool> m_IsPlaying = new(false);
    private readonly ReactiveProperty<int> m_Count = new(0);

    private int m_Direction = 1; //1이면 오른쪽, -1이면 왼쪽


    private MyTimingBarModel m_Model;

    private void Start()
    {
        m_Model = new MyTimingBarModel();

        BindInput();
        BindMoveLoop();
        BindView();
    }

    private void BindView()
    {
        m_IsPlaying
            .Subscribe(isPlaying =>
        {
            m_StartButton.interactable = !isPlaying;
            m_HitButton.interactable = isPlaying;
        }).AddTo(this);

        m_Phase.Subscribe(phase =>
        {
            RectTransform trackRect = m_TrackImage.rectTransform;

            float width = trackRect.rect.width;
            float x = Mathf.Lerp(-width * 0.5f, width * 0.5f, phase);

            Vector2 pos = m_Marker.anchoredPosition;
            pos.x = x;
            m_Marker.anchoredPosition = pos;
        }).AddTo(this);

        m_Phase
            .Select(phase => MyTimingBarModel.ZoneOf(phase))
            .Where(_ => m_IsPlaying.Value)
            .DistinctUntilChanged()
            .Subscribe(zone =>
            {
                m_TrackImage.color = GetColor(zone);
            })
            .AddTo(this);

        m_Model.Score
            .Subscribe(score =>
            {
                m_ScoreText.text = $"점수 {score}";
            })
            .AddTo(this);

        m_Count
            .Subscribe(count =>
            {
                m_AttemptText.text = $"{count} / {m_AttemptsPerRound}";
            })
            .AddTo(this);

        m_Model.Combo
            .Subscribe(combo =>
            {
                m_ComboText.text = $"{combo} 콤보!";
                m_ComboText.color = Color.softYellow;
            })
            .AddTo(this);
    }

    private Color GetColor(Zone zone)
    {
        switch (zone)
        {
            case Zone.Perfect:
                return Color.yellow;
            case Zone.Good:
                return Color.blue;
            default:
                return Color.gray;
        }
    }

    private void BindInput()
    {
        m_StartButton
            .OnClickAsObservable()
            .SubscribeAwait(async (_, ct) =>
            {
                await startRoundAsync(ct);
            }, AwaitOperation.Drop)
            .AddTo(this);

        var hitButtonStream = m_HitButton.OnClickAsObservable();

        var spaceKeyStream = Observable.EveryUpdate()
            .Where(_ => Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            .Select(_ => Unit.Default);

        hitButtonStream
            .Merge(spaceKeyStream)
            .Where(_ => m_IsPlaying.Value)
            .Subscribe(_ =>
            {
                Hit();
            })
            .AddTo(this);
    }
    private async UniTask startRoundAsync(CancellationToken ct)
    {
        m_Model.ResetForNewGame();

        m_TrackImage.color = Color.gray;
        m_Phase.Value = 0.5f;
        m_Count.Value = 0;
        m_Direction = 1;
        m_JudgementText.text = "";

        m_IsPlaying.Value = false;

        m_StartButton.interactable = false;
        m_HitButton.interactable = false;

        m_StateText.text = "3";
        await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: ct);

        m_StateText.text = "2";
        await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: ct);

        m_StateText.text = "1";
        await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: ct);

        StartGameAfterCountdown();
    }

    private void StartGameAfterCountdown()
    {
        m_StateText.text = "존에 올 때 Space 또는 HIT!";

        m_IsPlaying.Value = true;


    }

    private void Hit()
    {
        Judgement judgement = m_Model.ApplyHit(m_Phase.Value);

        m_Count.Value++;

        switch (judgement)
        {
            case Judgement.Perfect:
                m_JudgementText.text = "PERFECT!";
                m_JudgementText.color = Color.softYellow;
                break;
            case Judgement.Good:
                m_JudgementText.text = "GREAT!";
                m_JudgementText.color = Color.blue;
                break;
            case Judgement.Miss:
                m_JudgementText.text = "MISS!";
                m_JudgementText.color = Color.softRed;
                break;
        }

        if (m_Count.Value >= m_AttemptsPerRound) EndGame();
    }

    private void EndGame()
    {
        m_IsPlaying.Value = false;
        m_StateText.text = $"결과 - 점수 {m_Model.Score} . 최고 콤보 {m_Model.MaxCombo}";
    }

    private void BindMoveLoop()
    {
        Observable.EveryUpdate()
            .Where(_ => m_IsPlaying.Value)
            .Subscribe(_ =>
            {
                MovePhase();
            })
            .AddTo(this);
    }

    private void MovePhase()
    {
        float nextPhase = m_Phase.Value + Time.deltaTime * m_MarkerSpeed * m_Direction;

        if (nextPhase >= 1f)
        {
            nextPhase = 1f;
            m_Direction = -1;
        }
        else if (nextPhase <= 0f)
        {
            nextPhase = 0f;
            m_Direction = 1;
        }

        m_Phase.Value = nextPhase;
    }
    private void OnDestroy()
    {
        m_Model?.Dispose();
        m_Phase.Dispose();
        m_IsPlaying.Dispose();
        m_Count.Dispose();
    }

}
