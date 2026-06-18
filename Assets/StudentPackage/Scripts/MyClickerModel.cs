using System;
using R3;
using R3.Triggers;

public sealed class MyClickerModel : IDisposable
{
    private const double ClickUpgradeCostGrowth = 1.6;
    private const double AutoUpgradeCostGrowth = 1.8;
    private const int FeverMultiplier = 2;

    public ReactiveProperty<long> Gold { get; } = new(0);
    public ReactiveProperty<long> GoldPerClick { get; } = new(1);
    public ReactiveProperty<long> GoldPerSecond { get; } = new(0);
    public ReactiveProperty<long> ClickUpgradeCost { get; } = new(10);
    public ReactiveProperty<long> AutoUpgradeCost { get; } = new(25);
    public ReactiveProperty<bool> IsFever { get; } = new(false);

    public ReadOnlyReactiveProperty<bool> CanBuyClickUpgrade { get; }
    public ReadOnlyReactiveProperty<bool> CanBuyAutoUpgrade { get; }

    public TimeSpan FeverDuration { get; } = TimeSpan.FromSeconds(5);

    private readonly CompositeDisposable m_Disposables = new();

    public MyClickerModel()
    {
        CanBuyClickUpgrade = 
            Gold.CombineLatest(ClickUpgradeCost,
            (gold, cost) => gold >= cost)
            .ToReadOnlyReactiveProperty()
            .AddTo(m_Disposables);


        CanBuyAutoUpgrade =
            Gold.CombineLatest(AutoUpgradeCost,
            (gold, cost) => gold >= cost)
            .ToReadOnlyReactiveProperty()
            .AddTo(m_Disposables);

        Observable.Interval(TimeSpan.FromSeconds(1))
            .Where(m => GoldPerSecond.Value > 0)
            .Subscribe(_ => Gold.Value += GoldPerSecond.Value)
            .AddTo(m_Disposables);

        Gold.AddTo(m_Disposables);
        GoldPerClick.AddTo(m_Disposables);
        GoldPerSecond.AddTo(m_Disposables);
        ClickUpgradeCost.AddTo(m_Disposables);
        AutoUpgradeCost.AddTo(m_Disposables);
        IsFever.AddTo(m_Disposables);
    }

    public long Click()
    {
        long gain = GoldPerClick.Value * (IsFever.Value ? FeverMultiplier : 1);
        Gold.Value += gain;
        return gain;
    }

    public void AddBonus(long amount)
    {
        Gold.Value += amount;
    }

    public bool TryBuyClickUpgrade()
    {
        long cost = ClickUpgradeCost.Value;
        if (Gold.Value < cost)
            return false;
        Gold.Value -= cost;
        GoldPerClick.Value += 1;
        ClickUpgradeCost.Value = (long)Math.Ceiling(cost * ClickUpgradeCostGrowth);
        return true;
    }

    public bool TryBuyAutoUpgrade()
    {
        long cost = AutoUpgradeCost.Value;
        if (Gold.Value < cost)
            return false;
        Gold.Value -= cost;
        GoldPerSecond.Value += 1;
        AutoUpgradeCost.Value = (long)Math.Ceiling(cost * AutoUpgradeCostGrowth);
        return true;
    }

    public bool TryStartFever()
    {
        if (IsFever.Value) return false;

        IsFever.Value = true;
        Observable.Timer(FeverDuration).Subscribe(_ => IsFever.Value = false)
            .AddTo(m_Disposables);

        return true;
    }

    public void Dispose()
    {
        m_Disposables.Dispose();
    }
}
