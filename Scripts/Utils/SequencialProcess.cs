using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public interface ISequenceData
{
    //Process 함수가 호출된 이후 해당값을 True 로 설정해주어야 다음 SequenceData를 작동시킨다
    bool IsComplete { get; set; }

    void Process();
    void ProcessAndCallback(Action callback);

    //처리중 게임 일시정지 같은 동작이 이루어질 경우 호출되는 함수
    //리소스 Release같은 동작이 주로 구현됨
    void Clean();
}

/// <summary>
/// 짧은 시간에 다량의 메모리를 소비할때 Memory Peak나 GC Peak 발생을 방지하기 위한 순차처리 클래스
/// </summary>
public class SequencialProcess : MonoBehaviour
{
    //메모리 사용이 일정량을 넘어섯을 경우처럼 Push를 대량으로 걸어놓고 처리를 중단해야 할 경우에 셋팅해주는 변수
    public bool ForceFlush { private get; set; }
    //최대 동시처리수. 프로젝트마다, 또한 처리해야 할 기능에 따라 조절
    private const int MAX_CONCURRENCY = 6;

    public int concurrency { get { return mConcurr; } set { mConcurr = Mathf.Clamp(value, 1, MAX_CONCURRENCY); } }
    private int mConcurr = 1;

    //순차처리할 Queue
    Queue<ISequenceData> queue = null;

    //현재 처리중인 Queue (데이터구조는 리스트) 
    List<ISequenceData> mProcessingSeqs = new List<ISequenceData>(MAX_CONCURRENCY);

    //mProcessingSeqs의 코루틴
    Coroutine mProcCoroutine = null;

    private void Awake()
    {
        queue = new Queue<ISequenceData>();
    }

    private void Start()
    {
        mProcCoroutine = StartCoroutine(OnTriggerProcess());
    }

    IEnumerator OnTriggerProcess()
    {
        while(true)
        {
            if (ForceFlush)
                queue.Clear();

            yield return Process();
            yield return null;
        }
    }

    public void Pause()
    {
        if(mProcCoroutine != null)
        {
            StopCoroutine(mProcCoroutine);
            mProcCoroutine = null;
        }

        foreach(ISequenceData seq in mProcessingSeqs)
        {
            if (seq.IsComplete)
                continue;

            seq.Clean();
        }

        mProcessingSeqs.RemoveAll(_ => _.IsComplete);
    }

    public void Resume()
    {
        if(mProcCoroutine != null)
        {
            Debug.LogError($"SequencialProcess의 resume은 pause를 통해 먼저 Coroutine이 중지된 상태여야 합니다. Resume을 연속으로 호출하지 마세요");
            return;
        }
        mProcCoroutine = StartCoroutine(OnTriggerProcess());
    }

    public void Push(ISequenceData data)
    {
        queue.Enqueue(data);
    }

    IEnumerator Process()
    {
        if (queue.Count <= 0)
            yield break;

        float debugTime = 0f;

        int popCnt = Mathf.Min(queue.Count, mConcurr);

        for(int i = 0; i < popCnt; ++i)
        {
            ISequenceData data = queue.Dequeue();
            data.IsComplete = false;

            mProcessingSeqs.Add(data);
        }

        int _completeMask = 0x00;
        int _isAllComplete = 0x00;

        for(int i = 0; i < mProcessingSeqs.Count; ++i)
        {
            _isAllComplete |= 1 << i;
            mProcessingSeqs[i].Process();
        }

        while(_completeMask != _isAllComplete)
        {
            debugTime += Time.deltaTime;

            if(debugTime >= 5)
            {
                Debug.Log($"Processing... Stack Trace : {StackTraceUtility.ExtractStackTrace()}");
                debugTime = 0f;
            }

            for(int i = 0; i < mProcessingSeqs.Count; ++i)
            {
                if (mProcessingSeqs[i].IsComplete)
                    _completeMask |= 1 << i;
            }

            yield return null;
        }

        mProcessingSeqs.Clear();
    }
}
