using System.Collections.Generic;
using Data.MilitaryGame;
using MilitaryGame.Building;
using UnityEngine;
using BuildingType = Data.MilitaryGame.BuildingData.BuildingType;

namespace MilitaryGame.Factory
{
    public class BuildingFactory : Singleton<BuildingFactory>
    {
        [Header("BUILDING COUNT")]
        [SerializeField] private int _buildingCount;

        private List<BuildingData> _buildingDataList = new();
        private Dictionary<BuildingType, ObjectPool<BaseBuilding>> _buildingPoolList = new();

        public void Initialize(List<BuildingData> buildingDataList)
        {
            _buildingDataList = buildingDataList;
            
            // Initializes the building pool for each building data in the list.
            foreach (BuildingData buildingData in _buildingDataList)
            {
                _buildingPoolList.Add(buildingData.Type, new ObjectPool<BaseBuilding>(transform, buildingData.Prefab, _buildingCount));
            }
        }

        // Creates a building of the specified type at the given position and rotation using the building pool.
        public BaseBuilding CreateBuilding(BuildingType buildingType, Vector3 position, Quaternion rotation)
        {
            return _buildingPoolList[buildingType].GetObject(position, rotation);
        }

        // Destroys the specified building by returning it to the building pool.
        public void DestroyBuilding(BaseBuilding building)
        {
            _buildingPoolList[building.BuildingData.Type].ReturnObject(building);
        }
    
        public void End()
        {
            foreach (var pool in _buildingPoolList.Values)
            {
                pool.ClearPool();
            }
        }
    }
}
