using System;
using System.Collections.Generic;
using UnityEngine;

public class AbilityKingCyclesOfMastery : AbilityBase
{
    [Header("Cycle List")]
    [SerializeField] private List<AbilityBase> _cycle = new List<AbilityBase>();

    [Header("Cycling Rules")]
    [SerializeField] private bool _cycleOnlyOnSuccess = true;

    [SerializeField] private bool _skipIfNotReady = true;

    [SerializeField] private int _maxScanPerPress = 8;

    [SerializeField] private bool _wrap = true;

    int _index;

    protected override void OnStart()
    {
        base.OnStart();
        TryUseCurrentAndCycle();
    }

    private void TryUseCurrentAndCycle()
    {
        if (_cycle == null || _cycle.Count == 0) return;

        int scans = 0;
        int startIndex = _index;

        while (scans < Mathf.Max(1, _maxScanPerPress))
        {
            AbilityBase ability = GetCurrentAbility();

            if (ability == null || ability == this)
            {
                AdvanceIndex();
                scans++;
                if (_index == startIndex) return;
                continue;
            }

            if (_skipIfNotReady && !ability.IsReady)
            {
                AdvanceIndex();
                scans++;
                if (_index == startIndex) return;
                continue;
            }

            bool fired = ability.TryUse();

            if (fired)
            {
                if (_cycleOnlyOnSuccess)
                {
                    AdvanceIndex();
                }
                return;
            }

            if (!_cycleOnlyOnSuccess)
            {
                AdvanceIndex();
            }
            return;
        }
    }

    private AbilityBase GetCurrentAbility()
    {
        if (_cycle == null || _cycle.Count == 0) return null;

        if (_index < 0)
        {
            _index = 0;
        }
        if (_index >= _cycle.Count)
        {
            _index = _wrap ? 0 : _cycle.Count - 1;
        }

        return _cycle[_index];
    }

    private void AdvanceIndex()
    {
        if (_cycle == null || _cycle.Count == 0) return;

        _index++;

        if (_index >= _cycle.Count)
        {
            _index = _wrap ? 0 : _cycle.Count - 1;
        }
    }
}
