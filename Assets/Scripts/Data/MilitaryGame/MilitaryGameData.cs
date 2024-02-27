using System.Collections.Generic;
using MilitaryGame.Building;
using UnityEngine;

namespace Data.MilitaryGame
{
    [CreateAssetMenu(fileName = "MilitaryGameData", menuName = "Data/MilitaryGameData")]
    public class MilitaryGameData : ScriptableObject
    {
        public List<BuildingData> BuildingDataList;
        public List<SoldierData> SoldierDataList;
    }
}
