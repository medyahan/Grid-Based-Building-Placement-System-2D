using System;
using System.Collections;
using System.Collections.Generic;
using Data.MilitaryGame;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MilitaryGame.UI.ProductionMenu.InfiniteScroll
{
    [Serializable]    
    public class ScrollRestrictionSettings
    {
        [SerializeField] private float _contentOverflowRange = 125f;
        public float ContentOverflowRange => _contentOverflowRange;

        [SerializeField] [Range(0, 1)] private float _contentDecelerationInOverflow = 0.5f;
        public float ContentDecelerationInOverflow => _contentDecelerationInOverflow;
    }
    
    public class InfiniteScrollView : ScrollRect
    {
        [SerializeField] private ScrollRestrictionSettings _restrictionSettings = null;
        private InfiniteScrollContent _content;
    
        private Vector2 _contentStartPos = Vector2.zero;
        private Vector2 _dragStartingPosition = Vector2.zero;
        private Vector2 _dragCurPosition = Vector2.zero;
        private Vector2 _lastDragDelta = Vector2.zero;
    
        private bool _isDragging = false;
        private bool _runningBack = false;
        private bool _needRunBack = false;
        private bool _isFocusActive = false;

        private IEnumerator _runBackRoutine;
        private IEnumerator _focusRoutine;
        
        public void Initialize(List<BuildingData> buildingDataList)
        {
            _content = GetComponentInChildren<InfiniteScrollContent>();
            _content.Initialize(buildingDataList);
        
            movementType = MovementType.Unrestricted;
            vertical = true;
            onValueChanged.AddListener(OnScrollRectValueChanged);
        }

        #region CALLBACK METHODS
    
        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);

            _isDragging = true;
            
            // Record the initial position of the content and the starting position of the drag.
            _contentStartPos = content.anchoredPosition;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                viewport,
                eventData.position,
                eventData.pressEventCamera,
                out _dragStartingPosition);

            _dragCurPosition = _dragStartingPosition;
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging || !IsActive() || eventData.button != PointerEventData.InputButton.Left)
                return;

            // Check if the drag event occurred inside the scroll view's RectTransform.
            bool isInsideRectTransform  = RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out Vector2 localCursor);
            
            if (!isInsideRectTransform)
                return;
            
            // Check if the vertical drag is valid; if not, restrict the content's position.
            if (!CheckDragValidVertical(localCursor - _dragCurPosition))
            {
                Vector2 restrictedPos = GetRestrictedContentPositionOnDrag(eventData);
                _needRunBack = true;

                SetContentAnchoredPosition(restrictedPos);
                return;
            }
            
            // Update the scroll view bounds and handle drag updates.
            UpdateBounds();
        
            _needRunBack = false;
            _lastDragDelta = localCursor - _dragCurPosition;
            _dragCurPosition = localCursor;
        
            // Set the content's anchored position and update the visible items.
            SetContentAnchoredPosition(CalculateContentPos(localCursor));
            UpdateItems(_lastDragDelta);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
        
            _isDragging = false;

            if (_needRunBack)
                StopMovement();
        }

        private void OnScrollRectValueChanged(Vector2 val)
        {
            if (_runningBack || _isDragging || _isFocusActive)
                return;
            
            Vector2 delta = velocity.normalized;
            
            if (!CheckDragValidVertical(delta))
            {
                // Get the restricted content position based on the scroll delta.
                Vector2 contentPos = GetRestrictedContentPositionOnScroll(delta);
                
                SetContentAnchoredPosition(contentPos);
            
                // If the magnitude of the product of velocity and deltaTime is less than 5, stop the movement.
                if ((velocity * Time.deltaTime).magnitude < 5)
                    StopMovement();
                
                return;
            }
            
            // Update items based on the scroll delta.
            UpdateItems(delta);
        }
        
        #endregion
        
        private void UpdateItems(Vector2 delta)
        {
            bool positiveDelta = delta.y > 0;
           
            // Check if scrolling down and need to add item into tail.
            if (positiveDelta && -_content.GetLastItemPos().y - content.anchoredPosition.y <= viewport.rect.height + _content.SpacingSize.y)
                _content.AddIntoTail();
            
            // Check if scrolling down and need to delete item from head.
            if (positiveDelta && content.anchoredPosition.y - -_content.GetFirstItemPos().y >= (2 * _content.ItemHeight) + _content.SpacingSize.y)
                _content.DeleteFromHead();
            
            // Check if scrolling up and need to add item into head.
            if (!positiveDelta && content.anchoredPosition.y + _content.GetFirstItemPos().y <= _content.ItemHeight + _content.SpacingSize.y)
                _content.AddIntoHead();
            
            // Check if scrolling up and need to delete item from tail.
            if (!positiveDelta && -_content.GetLastItemPos().y - content.anchoredPosition.y >= viewport.rect.height + _content.ItemHeight + _content.SpacingSize.y)
                _content.DeleteFromTail();
        }
    
        private bool CheckDragValidVertical(Vector2 delta)
        {
            bool positiveDelta = delta.y > 0;

            if (positiveDelta)
            {
                Vector2 lastItemPos = _content.GetLastItemPos();
            
                // Check if adding a new item into the tail is allowed, and if not, check if the content is still within bounds.
                if (!_content.CanAddNewItemIntoTail() && content.anchoredPosition.y + viewport.rect.height + lastItemPos.y - _content.ItemHeight > 0)
                    return false;
            }
            else
            {
                // Check if adding a new item into the head is allowed, and if not, check if the content is still within bounds.
                if (!_content.CanAddNewItemIntoHead() && content.anchoredPosition.y <= 0)
                    return false;
            }
            
            return true;
        }

        #region GET RESTRICTION METHODS
        
        private Vector2 GetRestrictedContentPositionOnDrag(PointerEventData eventData)
        {
            // Convert the screen point to local point in the scroll view's rect.
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                viewRect,
                eventData.position,
                eventData.pressEventCamera, out Vector2 localCursor);

            Vector2 delta = localCursor - _dragCurPosition;
            Vector2 position = CalculateContentPos(localCursor);
            float restriction = GetVerticalRestrictionWeight(delta);
            
            // Calculate the restricted position using the weighted previous and next positions.
            Vector2 result = CalculateRestrictedPosition(content.anchoredPosition, position, restriction);
            result.x = content.anchoredPosition.x;
            
            return result;
        }

        private Vector2 GetRestrictedContentPositionOnScroll(Vector2 delta)
        {
            float restriction = GetVerticalRestrictionWeight(delta);

            Vector2 deltaPos = velocity * Time.deltaTime;
            Vector2 res = Vector2.zero;
            deltaPos.x = 0;
            Vector2 curPos = content.anchoredPosition;
            Vector2 nextPos = curPos + deltaPos;
            
            // Calculate the restricted position using the weighted current and next positions.
            res = CalculateRestrictedPosition(curPos, nextPos, restriction);
            res.x = 0;
            
            // Apply content deceleration in overflow based on the restriction settings.
            velocity *= _restrictionSettings.ContentDecelerationInOverflow;

            return res;
        }

        private float GetVerticalRestrictionWeight(Vector2 delta)
        {
            bool positiveDelta = delta.y > 0;
            
            // Get the maximum limit for content overflow.
            float maxLimit = _restrictionSettings.ContentOverflowRange;

            if (positiveDelta)
            {
                Vector2 lastItemPos = _content.GetLastItemPos();

                // Check if the absolute value of lastItemPos.y is within the viewport height - item height.
                if (Mathf.Abs(lastItemPos.y) <= viewport.rect.height - _content.ItemHeight)
                {
                    float max = lastItemPos.y + maxLimit;
                    float cur = content.anchoredPosition.y + lastItemPos.y;
                    float diff = max - cur;

                    return 1f - Mathf.Clamp(diff / maxLimit, 0, 1);
                }
                else
                {
                    // Calculate the maximum, current, and difference values for the overflow case.
                    float max = -(viewport.rect.height - maxLimit - _content.ItemHeight);
                    float cur = content.anchoredPosition.y + lastItemPos.y;
                    float diff = max - cur;

                    return 1f - Mathf.Clamp(diff / maxLimit, 0, 1);
                }
            }

            // Calculate the vertical restriction value for negative delta.y.
            float restrictionVal = Mathf.Clamp(Mathf.Abs(content.anchoredPosition.y) / maxLimit, 0, 1);
            return restrictionVal;
        }
        
        #endregion

        #region CALCULATE METHODS
        
        /// <summary>
        /// Calculates the position of the content based on the local cursor position.
        /// </summary>
        /// <param name="localCursor">Local cursor position.</param>
        private Vector2 CalculateContentPos(Vector2 localCursor)
        {
            Vector2 dragDelta = localCursor - _dragStartingPosition;
            
            // Calculate the new position of the content.
            return _contentStartPos + dragDelta;
        }

        /// <summary>
        /// Calculates a position that combines the current and next positions with a specified restriction weight.
        /// </summary>
        /// <param name="curPos">Current position.</param>
        /// <param name="nextPos">Next position.</param>
        /// <param name="restrictionWeight">Weight of the restriction applied to the current position.</param>
        private Vector2 CalculateRestrictedPosition(Vector2 curPos, Vector2 nextPos, float restrictionWeight)
        {
            Vector2 weightedPrev = curPos * restrictionWeight;
            Vector2 weightedNext = nextPos * (1 - restrictionWeight);
            
            // Combine the weighted contributions to get the final result.
            return weightedPrev + weightedNext;
        }
        
        #endregion
        
        protected override void OnDestroy()
        {
            onValueChanged.RemoveListener(OnScrollRectValueChanged);
            base.OnDestroy();
        }
    }
    
}
