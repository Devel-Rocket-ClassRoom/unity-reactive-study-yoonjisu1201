using System;
using System.Text;
using R3;
using UnityEngine;

//현재 마커가 어느 구간에 있는지 나타냄
public enum Zone
{
    None,
    Good,
    Perfect,
}

//HIT 했을 때 판정 결과
public enum Judgement
{
    Miss,
    Good,
    Perfect,
}

public sealed class MyTimingBarModel : IDisposable
{
    //phase 기준 0.5가 중앙,  0.5-0.04 ~ 0.5+0.04  범위가 퍼펙트
    public const float PerfectHalfWidth = 0.04f;

    //phase 기준 0.5가 중앙,  0.5-0.12 ~ 0.5+0.12  범위가 굿
    //퍼펙트를 먼저 검사
    public const float GoodHalfWidth = 0.12f;

    private const int PerfectScore = 3;
    private const int GoodScore = 1;

    //콤보 5 마다 점수 배율 증가
    private const int ComboPerMultiplier = 5;

    public ReactiveProperty<int> Score { get; } = new(0);
    public ReactiveProperty<int> Combo { get; } = new(0);
    public ReactiveProperty<int> MaxCombo { get; } = new(0);

    private readonly CompositeDisposable m_Disposables = new();

    public MyTimingBarModel()
    {
        Score.AddTo(m_Disposables);
        Combo.AddTo(m_Disposables);
        MaxCombo.AddTo(m_Disposables);
    }

    //phase 값으로 현재 구간을 판정. (0~1 사이의 값)
    public static Zone ZoneOf(float phase)
    {
        //중앙에서 얼마나 떨어져 있는지 계산
        float distanceFromCenter = Mathf.Abs(phase - 0.5f);

        if (distanceFromCenter <= PerfectHalfWidth)
        {
            return Zone.Perfect;
        }
        if (distanceFromCenter <= GoodHalfWidth)
        {
            return Zone.Good;
        }

        return Zone.None;
    }

    //현재 phase에서 HIT 했을 때 어떤 판정인지 반환
    public static Judgement Judge(float phase)
    {
        Zone zone = ZoneOf(phase);

        switch (zone)
        {
            case Zone.Perfect:
                return Judgement.Perfect;
            case Zone.Good:
                return Judgement.Good;
            default:
                return Judgement.Miss;
        }
    }

    //HIT 버튼 눌렀을 때 실행 함수
    public Judgement ApplyHit(float phase)
    {
        Judgement judgement = Judge(phase);

        switch (judgement)
        {
            case Judgement.Perfect:
                {
                    Combo.Value++;
                    int multiplier = 1 + Combo.Value / ComboPerMultiplier;
                    Score.Value += PerfectScore * multiplier;
                    break;
                }
            case Judgement.Good:
                {
                    Combo.Value++;
                    int multiplier = 1 + Combo.Value / ComboPerMultiplier;
                    Score.Value += GoodScore * multiplier;
                    break;
                }
            case Judgement.Miss:
                {
                    Combo.Value = 0;
                    break;
                }
        }
        if(Combo.Value > MaxCombo.Value) MaxCombo.Value = Combo.Value;

        return judgement;
    }

    public void ResetForNewGame() 
    {
        Score.Value = 0;
        Combo.Value = 0;
        MaxCombo.Value = 0;
    }

    public void Dispose() => m_Disposables.Dispose();
}
