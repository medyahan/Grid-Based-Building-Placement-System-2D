using Core;
using Data.MilitaryGame;
using MilitaryGame.Building;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MilitaryGame.UI.InformationPanel
{
    public class SoldierSlot : BaseMonoBehaviour
    {
        [SerializeField] private Button _produceButton;
        [SerializeField] private Image _soldierIconImage;
        
        [Header("TEXTS")]
        [SerializeField] private TMP_Text _soldierNameText;
        [SerializeField] private TMP_Text _soldierInfoText;

        private SoldierData _soldierData;
        private Barracks _barracks;

        public override void Initialize(params object[] list)
        {
            base.Initialize(list);

            _soldierData = (SoldierData) list[0];
            _barracks = (Barracks) list[1];
        
            _produceButton.onClick.RemoveAllListeners();
            _produceButton.onClick.AddListener(OnClickProduceButton);
        
            SetData(_soldierData);
        }

        // Sets the UI data for displaying information about a soldier.
        private void SetData(SoldierData soldierData)
        {
            _soldierIconImage.sprite = soldierData.Icon;
            _soldierNameText.text = soldierData.Name;
            _soldierInfoText.text = $"Att: {soldierData.DamagePoint} / Hp: {soldierData.HealthPoint}" ;
        }
    
        private void OnClickProduceButton()
        {
            _barracks.ProduceSoldier(_soldierData.Type);
        }
    }
}
    