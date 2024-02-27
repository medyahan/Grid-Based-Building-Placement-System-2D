using Core;
using Data.MilitaryGame;
using Interface;
using MilitaryGame.Factory;
using MilitaryGame.GridBuilding;
using MilitaryGame.UI.HealthBar;
using UnityEngine;

namespace MilitaryGame.Building
{
    public class BaseBuilding : BaseMonoBehaviour, ILeftClickable, IDamageable, IRightClickable
    {
        #region Variable Fields
        
        [SerializeField] private protected BuildingData _buildingData;
        [SerializeField] private BoundsInt _area;

        [Header("HEALTH BAR")] 
        [SerializeField] protected HealthBar _healthBar;

        private float _currentHealthPoint;
        
        public bool Placed { get; private set; }
        public BoundsInt Area => _area;
        public BuildingData BuildingData => _buildingData;
        
        #endregion // Variable Fields
        
        public override void Initialize(params object[] list)
        {
            base.Initialize(list);

            _currentHealthPoint = _buildingData.HealthPoint;
            ActivateHealthBar();
        }
        
        public override void End()
        {
            base.End();
            
            Placed = false;
            _healthBar.gameObject.SetActive(false);
        }

        /// <summary>
        /// Activates the health bar and initializes it with the current health point.
        /// </summary>
        private void ActivateHealthBar()
        {
            _healthBar.gameObject.SetActive(true);
            _healthBar.Initialize(_currentHealthPoint);
        }

        #region DAMAGE TRANSACTIONS

        public bool IsAlive()
        {
            return _currentHealthPoint > 0;
        }

        /// <summary>
        /// Applies damage to the building, updates health bar, and handles destruction if health reaches zero.
        /// </summary>
        /// <param name="damage">The amount of damage to apply.</param>
        public void TakeDamage(int damage)
        {
            _currentHealthPoint -= damage;
            _healthBar.SetHealthBar(_currentHealthPoint);
            
            if (!IsAlive())
            {
                MilitaryGameEventLib.Instance.CloseInformationPanel?.Invoke();
                GridBuildingSystem.Instance.ClearPlacedObject(transform.position, _area);
                End();
                BuildingFactory.Instance.DestroyBuilding(this);
            }
        }
        
        #endregion
        
        #region CLICK METHODS

        public void OnLeftClick()
        {
            if(!Placed) return;
            MilitaryGameEventLib.Instance.ShowBuildingInfo?.Invoke(this);
        }

        public void OnRightClick()
        {
            if(!Placed) return;
            MilitaryGameEventLib.Instance.SetDamageableObject?.Invoke(this);
        }

        #endregion

        #region BUILD PLACEMENT METHODS
        
        /// <summary>
        /// Checks if the object can be placed on the grid based on its current position.
        /// </summary>
        /// <returns>Returns true if the object can be placed, otherwise false.</returns>
        public bool CanBePlaced()
        {
            // Convert the world position to grid cell coordinates.
            Vector3Int positionInt = GridBuildingSystem.Instance.GridLayout.LocalToCell(transform.position);

            BoundsInt areaTemp = _area;
            areaTemp.position = positionInt;

            // Check if the area is available on the grid.
            return GridBuildingSystem.Instance.CanTakeArea(areaTemp);
        }

        /// <summary>
        /// Places the object on the grid, updates the occupied area, and initializes the placed object.
        /// </summary>
        public void Place()
        {
            // Convert the world position to grid cell coordinates.
            Vector3Int positionInt = GridBuildingSystem.Instance.GridLayout.LocalToCell(transform.position);

            BoundsInt areaTemp = _area;
            areaTemp.position = positionInt;

            // Mark the object as placed and update the occupied area on the grid.
            Placed = true;
            GridBuildingSystem.Instance.TakeArea(areaTemp);

            Initialize();
        }
        
        #endregion
    }
}
