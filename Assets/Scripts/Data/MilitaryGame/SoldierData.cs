using MilitaryGame.Soldier;
using UnityEngine;

namespace Data.MilitaryGame
{
    [CreateAssetMenu(fileName = "SoldierData", menuName = "Data/SoldierData")]
    public class SoldierData : ScriptableObject
    {
        public string Name;
        public SoldierType Type;
        public Sprite Icon;
        public int HealthPoint;
        public int DamagePoint;
        public Soldier Prefab;
    
        public enum SoldierType
        {
            Ordinary,
            General,
            Helmet
        }
    }
}
