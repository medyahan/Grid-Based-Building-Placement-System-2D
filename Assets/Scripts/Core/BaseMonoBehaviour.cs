using UnityEngine;

namespace Core
{
    public class BaseMonoBehaviour : MonoBehaviour
    {
        private bool _isInitialize;

        public virtual void Initialize(params object[] list)
        {
            if (!_isInitialize)
            {
                RegisterEvents();
                _isInitialize = true;
            }
        }

        public virtual void RegisterEvents()
        {
        }

        public virtual void UnregisterEvents()
        {
        }

        public virtual void End()
        {
            _isInitialize = false;
            UnregisterEvents();
        }
    }
}