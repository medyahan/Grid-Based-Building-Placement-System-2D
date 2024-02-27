using System.Collections.Generic;
using Interface;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MilitaryGame.Managers
{
    public class InputManager : Singleton<InputManager>
    {
        private Camera _camera;
        private RaycastHit2D _hit;

        protected override void Awake()
        {
            base.Awake();
            _camera = Camera.main;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _hit = Physics2D.Raycast(_camera.ScreenToWorldPoint(Input.mousePosition),
                    _camera.transform.position);
                
                if (_hit.collider is null)
                    return;

                if (_hit.transform.gameObject.TryGetComponent(out ILeftClickable clickable))
                {
                    clickable.OnLeftClick();
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                _hit = Physics2D.Raycast(_camera.ScreenToWorldPoint(Input.mousePosition),
                    _camera.transform.position);
                
                if (_hit.collider is null)
                    return;
                
                if (_hit.transform.gameObject.TryGetComponent(out IRightClickable clickable))
                    clickable.OnRightClick();
            }
        }

        private void RaycastUI()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            GraphicRaycaster[] raycasters = FindObjectsOfType<GraphicRaycaster>();
            foreach (GraphicRaycaster raycaster in raycasters)
            {
                List<RaycastResult> results = new List<RaycastResult>();
                raycaster.Raycast(eventData, results);

                if (results.Count == 0)
                {
                    MilitaryGameEventLib.Instance.CloseInformationPanel?.Invoke();
                }
            }
        }
    }
}
