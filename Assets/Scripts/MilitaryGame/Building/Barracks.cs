using System.Collections.Generic;
using MilitaryGame.Factory;
using MilitaryGame.GridBuilding;
using UnityEngine;
using UnityEngine.Tilemaps;
using SoldierType = Data.MilitaryGame.SoldierData.SoldierType;

namespace MilitaryGame.Building
{
    public class Barracks : BaseBuilding
    {
        [Header("SOLDIER SPAWN")] 
        [SerializeField] private List<Transform> _soldierSpawnPointList = new List<Transform>();

        /// <summary>
        /// Produces a soldier of the specified type and places it on an available spawn point.
        /// </summary>
        /// <param name="soldierType">Type of soldier to produce.</param>
        public void ProduceSoldier(SoldierType soldierType)
        {
            Soldier.Soldier soldier = SoldierFactory.Instance.CreateSoldier(soldierType, Vector3.zero, Quaternion.identity);

            bool isSoldierPlaced = false;
            // Iterate through each soldier spawn point to find an available one.
            foreach (var soldierSpawnPoint in _soldierSpawnPointList)
            {
                // Check if the soldier can be placed at the current spawn point.
                if (CanSoldierBePlaced(soldierSpawnPoint.position, soldier.Area))
                {
                    Vector3Int cellPos = GridBuildingSystem.Instance.GridLayout.LocalToCell(soldierSpawnPoint.position);
                    soldier.transform.position = GridBuildingSystem.Instance.MainTilemap.GetCellCenterWorld(cellPos);
                    isSoldierPlaced = true;
                    
                    soldier.Initialize();
                    break;
                }
            }
            
            // If the soldier couldn't be placed, destroy it.
            if (!isSoldierPlaced)
                SoldierFactory.Instance.DestroySoldier(soldier);
        }

        /// <summary>
        /// Checks if a soldier can be placed at the specified world position within the given area on the grid.
        /// </summary>
        /// <param name="pos">World position to check for soldier placement.</param>
        /// <param name="soldierArea">Bounds of the area the soldier will occupy on the grid.</param>
        private bool CanSoldierBePlaced(Vector3 pos, BoundsInt soldierArea)
        {
            // Convert the world position to grid cell coordinates.
            Vector3Int positionInt = GridBuildingSystem.Instance.GridLayout.LocalToCell(pos);

            BoundsInt areaTemp = soldierArea;
            areaTemp.position = positionInt;

            // Check if the area is available on the grid.
            return GridBuildingSystem.Instance.CanTakeArea(areaTemp);
        }
    }
}