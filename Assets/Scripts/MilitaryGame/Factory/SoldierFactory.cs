using System.Collections.Generic;
using Data.MilitaryGame;
using UnityEngine;
using SoldierType = Data.MilitaryGame.SoldierData.SoldierType;

namespace MilitaryGame.Factory
{
    public class SoldierFactory : Singleton<SoldierFactory>
    {
        [Header("SOLDIER COUNT")]
        [SerializeField] private int _soldierCount;

        private List<SoldierData> _soldierDataList = new();
        private Dictionary<SoldierType, ObjectPool<Soldier.Soldier>> _soldierPoolList = new();

        public void Initialize(List<SoldierData> soldierDataList)
        {
            _soldierDataList = soldierDataList;
            
            // Initializes the soldier pool for each soldier data in the list.
            foreach (SoldierData soldierData in _soldierDataList)
            {
                _soldierPoolList.Add(soldierData.Type, new ObjectPool<Soldier.Soldier>(transform, soldierData.Prefab, _soldierCount));
            }
        }
    
        // Creates a soldier of the specified type at the given position and rotation using the soldier pool.
        public Soldier.Soldier CreateSoldier(SoldierType soldierType, Vector3 position, Quaternion rotation)
        {
            return _soldierPoolList[soldierType].GetObject(position, rotation);
        }

        // Destroys the specified soldier by returning it to the soldier pool.
        public void DestroySoldier(Soldier.Soldier soldier)
        {
            _soldierPoolList[soldier.SoldierData.Type].ReturnObject(soldier);
        }

        public void End()
        {
            foreach (var pool in _soldierPoolList.Values)
            {
                pool.ClearPool();
            }
        }
    }
}
