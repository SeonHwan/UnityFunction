using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// 현재 메모리 사용량을 체크하여 허용 범위를 벗어났는지 유무를 알수 있게 해주는 함수
/// </summary>
public class MemoryTracker : MonoSingleton<MemoryTracker>
{
    [NonSerialized]
    public bool IsLimitedOverflow = false;

    //전체 메모리 중 허용가능한 메모리 사이즈 (0 ~ 1 사잇값의 퍼센트값 e.g) 0.5 = 50%까지만 허용)
    public float LimitPercent { set; private get; }

    private void Awake()
    {
        LimitPercent = 0.0f;
    }

    IEnumerator Start()
    {
        while(true)
        {
            if(StaticFunction.ConvertByte2MB(Profiler.usedHeapSizeLong) + StaticFunction.ConvertByte2MB(Profiler.GetMonoUsedSizeLong()) > SystemInfo.systemMemorySize * LimitPercent)
            {
                if (IsLimitedOverflow == false)
                    IsLimitedOverflow = true;

                Debug.LogWarning($"HeapSize : {StaticFunction.ConvertByte2MBStr(Profiler.usedHeapSizeLong)}, LimitMemory : {SystemInfo.systemMemorySize * LimitPercent}MB");
            }
            else
            {
                if(IsLimitedOverflow == true)
                {
                    IsLimitedOverflow = false;
                }
            }

            yield return new WaitForSeconds(3f);
        }
    }
}
