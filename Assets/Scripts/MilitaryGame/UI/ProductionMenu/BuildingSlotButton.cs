using Data.MilitaryGame;
using MilitaryGame.Building;
using MilitaryGame.GridBuilding;
using MilitaryGame.UI.ProductionMenu.InfiniteScroll;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MilitaryGame.UI.ProductionMenu
{
    public class BuildingSlotButton : ScrollItem
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _productIconImage;
        [SerializeField] private TMP_Text _productNameText;

        private BuildingData _buildingData;

        public override void Initialize(params object[] list)
        {
            base.Initialize(list);

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnClickProductButton);
        }

        // Sets the UI data for displaying information about a building.
        protected override void SetData(BuildingData buildingData)
        {
            _buildingData = buildingData;
            
            _productIconImage.sprite = buildingData.Icon;
            _productNameText.text = buildingData.Name;
        }
    
        // Clears any temporary building if present and initializes the grid system with the selected building type.
        private void OnClickProductButton()
        {
            BaseBuilding tempBaseBuilding = GridBuildingSystem.Instance.TempBaseBuilding;
            if (tempBaseBuilding != null && !tempBaseBuilding.Placed)
            {
                GridBuildingSystem.Instance.ClearTempBuilding();
            }
        
            GridBuildingSystem.Instance.InitializeWithBuilding(_buildingData.Type);
        }
    }
}
