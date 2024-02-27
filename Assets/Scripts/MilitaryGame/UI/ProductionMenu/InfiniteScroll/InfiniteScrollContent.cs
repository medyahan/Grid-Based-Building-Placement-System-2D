using System.Collections.Generic;
using Core;
using Data.MilitaryGame;
using UnityEngine;
using UnityEngine.UI;

namespace MilitaryGame.UI.ProductionMenu.InfiniteScroll
{
    public class InfiniteScrollContent : BaseMonoBehaviour
    {
        [SerializeField] private ScrollRect _scrollRect;
        
        [Header("CONTENT ITEM")]
        [SerializeField] private ScrollItem _referenceItem;
        
        [Header("GRID VALUES")]
        [SerializeField] private Vector2 _spacingSize = Vector2.zero;
        [Min(1)] [SerializeField] private int _fixedColumnCount = 1;
        
        public Vector2 SpacingSize => _spacingSize;
        public float ItemWidth => _referenceItem.RectTransform.rect.width;
        public float ItemHeight =>  _referenceItem.RectTransform.rect.height;
        
        private List<ScrollItem> _activatedItems = new List<ScrollItem>();
        private List<ScrollItem> _deactivatedItems = new List<ScrollItem>();
        private List<BuildingData> _contentDataList = new List<BuildingData>();
        
        public override void Initialize(params object[] list)
        {
            base.Initialize(list);

            _contentDataList = (List<BuildingData>) list[0];
            
            CreateContentGrid(_contentDataList);
        }
        
        private void CreateContentGrid(List<BuildingData> contentDataList)
        {
            int itemCount = 0;
            Vector2Int initialGridSize = CalculateInitialGridSize();

            for (int col = 0; col < initialGridSize.y; col++)
            {
                for (int row = 0; row < initialGridSize.x; row++)
                {
                    if (itemCount == contentDataList.Count)
                    {
                        return;
                    }
                    ActivateItem(itemCount);
                    
                    itemCount++;
                }
            }
        }
        
        private Vector2Int CalculateInitialGridSize()
        {
            Vector2 contentSize = _scrollRect.content.rect.size;
            int verticalItemCount = 8 + (int) (contentSize.y / (ItemHeight + _spacingSize.y));

            return new Vector2Int(_fixedColumnCount, verticalItemCount);
        }
        
        public void ClearContent()
        {
            List<ScrollItem> activatedItems = new List<ScrollItem>(_activatedItems);

            foreach (ScrollItem item in activatedItems)
            {
                DeactivateItem(item);
            }
        }

        #region ADD METHODS
        
        public void AddIntoHead()
        {
            for (int i = 0; i < _fixedColumnCount; i++)
            {
                AddItemToHead();
            }
        }

        public void AddIntoTail()
        {
            for (int i = 0; i < _fixedColumnCount; i++)
            {
                AddItemToTail();
            }
        }
        
        private void AddItemToTail()
        {
            if (!CanAddNewItemIntoTail())
                return;

            int itemIndex = _activatedItems[_activatedItems.Count - 1].Index + 1;

            if (itemIndex == _contentDataList.Count)
                return;
            
            ActivateItem(itemIndex);
        }

        private void AddItemToHead()
        {
            if (!CanAddNewItemIntoHead())
                return;

            int itemIndex = _activatedItems[0].Index - 1;

            if (itemIndex < 0)
                return;
            
            ActivateItem(itemIndex);
        }
        
        public bool CanAddNewItemIntoTail()
        {
            if (_activatedItems == null || _activatedItems.Count == 0)
                return false;

            return _activatedItems[_activatedItems.Count - 1].Index < _contentDataList.Count - 1;
        }

        public bool CanAddNewItemIntoHead()
        {
            if (_activatedItems == null || _activatedItems.Count == 0)
                return false;
            
            return _activatedItems[0].Index - 1 >= 0;
        }

        #endregion

        #region DELETE METHODS

        public void DeleteFromHead()
        {
            int firstRowIndex = (int) _activatedItems[0].GridIndex.y;

            DeleteRow(firstRowIndex);
        }
        
        public void DeleteFromTail()
        {
            int lastRowIndex = (int) _activatedItems[_activatedItems.Count - 1].GridIndex.y;

            DeleteRow(lastRowIndex);
        }

        private void DeleteRow(int rowIndex)
        {
            List<ScrollItem> items = _activatedItems.FindAll(i => (int) i.GridIndex.y == rowIndex);

            foreach (ScrollItem item in items)
            {
                DeactivateItem(item);
            }
        }
        
        #endregion

        #region ACTIVATE METHODS

        /// <summary>
        /// Activates a scroll item at the specified item index.
        /// </summary>
        /// <param name="itemIndex">Index of the item to activate.</param>
        private void ActivateItem(int itemIndex)
        {
            // Convert item index to grid position and anchored position.
            Vector2 gridPos = GetGridPosition(itemIndex);
            Vector2 anchoredPos = GetAnchoredPosition(gridPos);

            ScrollItem scrollItem;
        
            // Create a new scroll item or reuse a deactivated one.
            if (_deactivatedItems.Count == 0)
                scrollItem = CreateNewScrollItem();
            else
            {
                scrollItem = _deactivatedItems[0];
                _deactivatedItems.Remove(scrollItem);
            }
            
            scrollItem.gameObject.name = $"({gridPos.x}, {gridPos.y})";
            scrollItem.RectTransform.anchoredPosition = anchoredPos;
            
            scrollItem.Initialize(itemIndex, gridPos, _contentDataList[itemIndex]);

            // Determine whether to insert at the head or add to the end of the activated items list.
            bool insertHead = (_activatedItems.Count == 0 || (_activatedItems.Count > 0 && _activatedItems[0].Index > itemIndex));

            if (insertHead)
                _activatedItems.Insert(0, scrollItem);
            else
                _activatedItems.Add(scrollItem);

            scrollItem.Activate(true);
        }

        private void DeactivateItem(ScrollItem item)
        {
            _activatedItems.Remove(item);
            _deactivatedItems.Add(item);
        
            item.Activate(false);
        }

        #endregion
        
        private ScrollItem CreateNewScrollItem()
        {
            ScrollItem scrollItem = Instantiate(_referenceItem, _scrollRect.content);
            scrollItem.RectTransform.pivot = new Vector2(0, 1);

            return scrollItem;
        }

        /// <summary>
        /// Converts an item index into a grid position (column and row).
        /// </summary>
        /// <param name="itemIndex">Index of the item.</param>
        private Vector2 GetGridPosition(int itemIndex)
        {
            int col = itemIndex / _fixedColumnCount;
            int row = itemIndex - (col * _fixedColumnCount);
            
            return new Vector2(row, col);
        }
        
        /// <summary>
        /// Calculates the anchored position based on the grid position, item width, item height, and spacing sizes.
        /// </summary>
        /// <param name="gridPosition">Grid position (column and row).</param>
        private Vector2 GetAnchoredPosition(Vector2 gridPosition)
        {
            float xValue = (gridPosition.x * ItemWidth) + (gridPosition.x * _spacingSize.x);
            float yValue = (-gridPosition.y * ItemHeight) - (gridPosition.y * _spacingSize.y);
            
            return new Vector2(xValue, yValue);
        }
        
        /// <summary>
        /// Gets the anchored position of the first item in the content.
        /// </summary>
        /// <returns>The anchored position of the first item.</returns>
        public Vector2 GetFirstItemPos()
        {
            if (_activatedItems.Count == 0)
                return Vector2.zero;

            return _activatedItems[0].RectTransform.anchoredPosition;
        }

        /// <summary>
        /// Gets the anchored position of the last item in the content.
        /// </summary>
        /// <returns>The anchored position of the last item.</returns>
        public Vector2 GetLastItemPos()
        {
            if (_activatedItems.Count == 0)
                return Vector2.zero;
            
            return _activatedItems[_activatedItems.Count - 1].RectTransform.anchoredPosition;
        }
    }
}