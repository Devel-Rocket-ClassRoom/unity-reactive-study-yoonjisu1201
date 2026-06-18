using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MyAsyncDemo : MonoBehaviour
{
    [SerializeField]
    private Button m_SequentialButton;

    [SerializeField]
    private Button m_DropButton;

    [SerializeField]
    private TextMeshProUGUI m_LogText;

    private readonly Queue<string> m_LogLines = new();
    private int m_SequentialCount;
    private int m_DropCount;

    private void Start() 
    {
        m_SequentialButton
            .OnClickAsObservable()
            .SubscribeAwait(async (_, ct) =>
            {
                int id = ++m_SequentialCount;
                Log($"[순차] 로드 #{id} 시작...");
                await FakeLoadAsync(ct);
                Log($"[순차] 로드 #{id} 완료...");
            },AwaitOperation.Sequential)
            .AddTo(this);

        m_DropButton
            .OnClickAsObservable()
            .SubscribeAwait(async (_, ct) =>
            {
                int id = ++m_DropCount;
                Log($"[드롭] 로드 #{id} 시작...");
                await FakeLoadAsync(ct);
                Log($"[드롭] 로드 #{id} 완료...");
            }, AwaitOperation.Drop)
            .AddTo(this);
    }

    private static async UniTask FakeLoadAsync(CancellationToken ct)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1.5), cancellationToken: ct);
    }

    private void Log(string message)
    {
        m_LogLines.Enqueue(message);
        while (m_LogLines.Count > 6)
            m_LogLines.Dequeue();
        if (m_LogText != null)
            m_LogText.text = string.Join("\n", m_LogLines);
    }
}
