using Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MilitaryGame.UI.HealthBar
{
    public class HealthBar : BaseMonoBehaviour
    {
        [SerializeField] private Image _fillImage;
        [SerializeField] private TMP_Text _healtValueText;
        
        [Header("VALUES")]
        [SerializeField] private float _fillDuration;

        private float _maxValue;
    
        public override void Initialize(params object[] list)
        {
            base.Initialize(list);

            _maxValue = (float) list[0];
            SetHealthBar(_maxValue);
        }

        /// <summary>
        /// Updates the health bar with the specified current value.
        /// </summary>
        /// <param name="currentValue">The current value of the health bar.</param>
        public void SetHealthBar(float currentValue)
        {
            _fillImage.DOFillAmount(currentValue / _maxValue, _fillDuration);
            _healtValueText.text = ((int)currentValue).ToString();
        }
    }
}