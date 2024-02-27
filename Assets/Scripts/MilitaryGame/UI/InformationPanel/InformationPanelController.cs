using System.Collections;
using System.Collections.Generic;
using System.Text;
using Core;
using Data.MilitaryGame;
using MilitaryGame.Building;
using MilitaryGame.UI.InformationPanel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BuildingType = Data.MilitaryGame.BuildingData.BuildingType;

public class InformationPanelController : BaseMonoBehaviour
{
    #region Variable Fields
    
    [Header("PANELS")]
    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private GameObject _productionPanel;

    [Header("INFO COMPONENTS")]
    [SerializeField] private Image _buildingIconImage;
    [SerializeField] private TMP_Text _buildingNameText;
    [SerializeField] private TMP_Text _buildingInfoText;

    [Header("SOLDIER SLOT")]
    [SerializeField] private SoldierSlot _soldierSlotPrefab;
    [SerializeField] private Transform _productionContentParent;
   
    private List<SoldierData> _soldierDataList = new List<SoldierData>();
    private List<SoldierSlot> _soldierSlotList = new List<SoldierSlot>();
    private BaseBuilding _tempBaseBuilding;

    #endregion // Variable Fields

    public override void Initialize(params object[] list)
    {
        base.Initialize(list);
        
        _soldierDataList = (List<SoldierData>) list[0];
        Close();
    }

    private void Open()
    {
        if(!_mainPanel.activeSelf)
            _mainPanel.SetActive(true);
    }
    
    public void Close()
    {
        if(_mainPanel.activeSelf)
            _mainPanel.SetActive(false);
    }

    #region SHOWING INFO METHODS

    /// <summary>
    /// Displays information about the selected building in the UI.
    /// </summary>
    /// <param name="baseBuilding">The selected building to show information about.</param>
    public void ShowBuildingInfo(BaseBuilding baseBuilding)
    {
        if(_tempBaseBuilding == baseBuilding && _mainPanel.activeSelf)
            return;
        
        Open();

        _tempBaseBuilding = baseBuilding;

        _buildingNameText.text = _tempBaseBuilding.BuildingData.Name;
        _buildingIconImage.sprite = _tempBaseBuilding.BuildingData.Icon;

        // Build information string
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(_tempBaseBuilding.BuildingData.InfoString);
        stringBuilder.Append($"\n\nDEFENCE: {_tempBaseBuilding.BuildingData.HealthPoint} Health Points");
        _buildingInfoText.text = stringBuilder.ToString();
        
        // Check if the building is productive and update the UI accordingly
        CheckProductive(_tempBaseBuilding.BuildingData.IsProductive);
    }

    /// <summary>
    /// Checks if the building is productive and updates the production panel accordingly.
    /// </summary>
    /// <param name="isProductive">A flag indicating whether the building is productive.</param>
    private void CheckProductive(bool isProductive)
    {
        _productionPanel.SetActive(isProductive);

        if (isProductive && _tempBaseBuilding.BuildingData.Type == BuildingType.Barracks)
        {
            CreateSoldierSlots();
        }
    }
    
    #endregion

    #region SOLDIER SLOT TRANSACTIONS
    
    private void CreateSoldierSlots()
    {
        ClearAllSoldierSlot();
        
        Barracks barracks = _tempBaseBuilding as Barracks;
        
        for (int i = 0; i < _soldierDataList.Count; i++)
        {
            SoldierSlot soldierSlot = Instantiate(_soldierSlotPrefab, _productionContentParent);
        
            if (barracks is not null) 
                soldierSlot.Initialize(_soldierDataList[i], barracks);
        
            _soldierSlotList.Add(soldierSlot);
        }
    }
    
    private void ClearAllSoldierSlot()
    {
        foreach (SoldierSlot soldierSlot in _soldierSlotList)
        {
            if(soldierSlot == null)
                continue;
                
            soldierSlot.End();
            Destroy(soldierSlot.gameObject);
        }

        _soldierSlotList.Clear();
    }

    #endregion
}
