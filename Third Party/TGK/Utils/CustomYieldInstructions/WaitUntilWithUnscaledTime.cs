using System;
using UnityEngine;

namespace TaigaGames.Utils
{
    /// <summary>
    /// Custom yield instruction that waits until a condition is met or a unscaled time has passed.
    /// </summary>
    public sealed class WaitUntilWithUnscaledTime : CustomYieldInstruction
    {
        private readonly Func<bool> _func;
        private readonly float _time;
        private readonly float _startTime;

        public override bool keepWaiting => !_func() && Time.unscaledTime - _startTime < _time;

        public WaitUntilWithUnscaledTime(Func<bool> func, float time)
        {
            _func = func;
            _time = time;
            _startTime = Time.unscaledTime;
        }
    }
}