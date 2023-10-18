using System;
using UnityEngine;

namespace Moball {
    public class LaunchDomeModel : MonoBehaviour
    {
        private Animator _animator;
        private static readonly int LaunchChargeAmount = Animator.StringToHash("LaunchChargeAmount");
        private static readonly int Launch = Animator.StringToHash("Launch");
        private static readonly int ChargeStart = Animator.StringToHash("ChargeStart");

        void Awake()
        {
            TryGetComponent(out _animator);
        }
        
        public void TriggerLaunch()
        {
            _animator.SetTrigger(Launch);
        }
        
        public void TriggerChargeStart()
        {
            _animator.SetTrigger(ChargeStart);
        }

        public void UpdateDome(float chargeAmount)
        {
            _animator.SetFloat(LaunchChargeAmount, chargeAmount);
        }
    } 
}
