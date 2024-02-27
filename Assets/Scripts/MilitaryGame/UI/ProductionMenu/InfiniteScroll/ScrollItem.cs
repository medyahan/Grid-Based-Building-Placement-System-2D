using Core;
using Data.MilitaryGame;
using UnityEngine;

namespace MilitaryGame.UI.ProductionMenu.InfiniteScroll
{
    public class ScrollItem : BaseMonoBehaviour
    {
        public int Index { get; protected set; }
        public Vector2 GridIndex { get; protected set; }
        public RectTransform RectTransform => transform as RectTransform;

        public override void Initialize(params object[] list)
        {
            base.Initialize(list);
        
            Index = (int) list[0];
            GridIndex = (Vector2) list[1];
        
            SetData((BuildingData) list[2]);
        }

        protected virtual void SetData(BuildingData buildingData) { }

        public void Activate(bool status)
        {
            gameObject.SetActive(status);
        }
    }
}
