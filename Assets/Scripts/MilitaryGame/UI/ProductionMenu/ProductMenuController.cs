using System.Collections.Generic;
using Core;
using Data.MilitaryGame;
using MilitaryGame.UI.ProductionMenu.InfiniteScroll;
using UnityEngine;

namespace MilitaryGame.UI.ProductionMenu
{
    public class ProductMenuController : BaseMonoBehaviour
    {
        #region Variable Fields

        [SerializeField] private InfiniteScrollView _infiniteScrollView;

        [Header("BUILDING SLOT")] [SerializeField]
        private BuildingSlotButton _buildingSlotButtonPref;

        [SerializeField] private Transform _contentParent;

        private List<BuildingSlotButton> _buildingSlotButtonList = new();
        private bool _isMenuOpen;

        private List<BuildingData> _buildingDataList = new();

        [SerializeField] private int _itemCount;

        #endregion // Variable Fields

        public override void Initialize(params object[] list)
        {
            base.Initialize(list);

            _buildingDataList = (List<BuildingData>) list[0];

            List<BuildingData> contentDataList = new List<BuildingData>();

            for (int i = 0; i < _itemCount; i++)
            {
                foreach (var buildingData in _buildingDataList)
                {
                    contentDataList.Add(buildingData);
                }
            }

            _infiniteScrollView.Initialize(contentDataList);
        }
    }
}