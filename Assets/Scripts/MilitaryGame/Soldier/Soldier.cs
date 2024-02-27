using System.Collections;
using System.Collections.Generic;
using Core;
using Data.MilitaryGame;
using DG.Tweening;
using Interface;
using MilitaryGame.Factory;
using MilitaryGame.GridBuilding;
using MilitaryGame.UI.HealthBar;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MilitaryGame.Soldier
{
    public class Soldier : BaseMonoBehaviour, ILeftClickable, IAttackable, IDamageable, IRightClickable
    {
        #region Variable Fields
        [SerializeField] private BoundsInt _area;
        [SerializeField] private SoldierData _soldierData;
        [SerializeField] private HealthBar _healthBar;

        [Header("MOVEMENT")]
        [SerializeField] private float _moveDuration;
        
        [Header("INDICATOR")]
        [SerializeField] private GameObject _selectedIndicatorObj;

        public SoldierData SoldierData => _soldierData;
        
        private float _currentHealthPoint;
        public bool IsAttacking { get; set; }
        
        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                _selectedIndicatorObj.SetActive(value);
            }
        }
        public BoundsInt Area => _area;

        private IDamageable _currentDamageableForAttack;

        #endregion // Variable Fields

        public override void Initialize(params object[] list)
        {
            base.Initialize(list);
            
            Place();
            _currentHealthPoint = _soldierData.HealthPoint;
            _healthBar.Initialize(_currentHealthPoint);
        }

        private void Update()
        {
            if (!IsSelected) return;

            if (Input.GetMouseButtonDown(1))
            {
                if(IsAttacking)
                    StopAttack();
                
                IDamageable damageableObject = MilitaryGameEventLib.Instance.GetCurrentDamageableObject?.Invoke();
                
                if (damageableObject != null && _currentDamageableForAttack != damageableObject)
                {
                    _currentDamageableForAttack = damageableObject;
                    MilitaryGameEventLib.Instance.SetDamageableObject?.Invoke(null);
                    Attack(damageableObject);
                }
                
                Deselect();
                CheckMovement();
            }
        }
        
        #region CLICK METHODS
        
        public void OnLeftClick()
        {
            if (!IsSelected)
                Select();
            else
                Deselect();
        }
        
        public void OnRightClick()
        {
            if (!IsSelected)
                MilitaryGameEventLib.Instance.SetDamageableObject?.Invoke(this);
        }
        
        #endregion

        #region SELECT / DESELECT METHODS

        private void Select()
        {
            IsSelected = true;

            Soldier currentSelectedSoldier = MilitaryGameEventLib.Instance.GetSelectedSoldier?.Invoke();
            if (currentSelectedSoldier != null)
            {
                currentSelectedSoldier.Deselect();
            }
            
            MilitaryGameEventLib.Instance.SetSelectedSoldier?.Invoke(this);
        }

        private void Deselect()
        {
            IsSelected = false;
            MilitaryGameEventLib.Instance.SetSelectedSoldier?.Invoke(null);
        }

        #endregion

        #region MOVEMENT TRANSACTIONS
        
        /// <summary>
        /// Checks the movement by finding a path from the current position to the mouse cursor position.
        /// </summary> 
        private void CheckMovement()
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            // Convert the current position and target position to grid cell positions.
            Vector3Int startCellPos = GridBuildingSystem.Instance.MainTilemap.WorldToCell(transform.position);
            Vector3Int endCellPos = GridBuildingSystem.Instance.MainTilemap.WorldToCell(mouseWorldPos);

            // Find a path from the current position to the target position using the Pathfinder.
            List<Vector3Int> path = Pathfinder.Pathfinder.Instance.FindPath(startCellPos, endCellPos);
            
            if(path.Count != 0 && IsAttacking)
                StopAttack();
            
            StartCoroutine(MoveOnPathCoroutine(path));
        }
        
        /// <summary>
        /// Moves the object along the given path using the Unity coroutine system.
        /// </summary>
        /// <param name="path">The path to follow.</param>
        private IEnumerator MoveOnPathCoroutine(List<Vector3Int> path)
        {
            int index = 0;
            
            if(path.Count != 0)
                UnPlace();
            
            while (path.Count > 0 && index < path.Count && IsAlive())
            {
                Vector3 targetWorldPos = GridBuildingSystem.Instance.MainTilemap.GetCellCenterWorld(path[index]);
                
                transform.DOMove(targetWorldPos, _moveDuration);
                index++;

                yield return new WaitForSeconds(_moveDuration);
            }
            
            Place();
        }
        
        #endregion

        #region DAMAGE TRANSACTIONS
        
        public bool IsAlive()
        {
            return _currentHealthPoint > 0;
        }

        /// <summary>
        /// Inflicts damage to the soldier and updates the health bar. Destroys the soldier if health reaches zero.
        /// </summary>
        /// <param name="damage">Amount of damage to inflict.</param>
        public void TakeDamage(int damage)
        {
            _currentHealthPoint -= damage;
            _healthBar.SetHealthBar(_currentHealthPoint);
            
            if (!IsAlive())
            {
                UnPlace();
                End();
                SoldierFactory.Instance.DestroySoldier(this);
            }
        }
        
        #endregion

        #region ATTACK METHODS
        
        /// <summary>
        /// Initiates an attack on a damageable object.
        /// </summary>
        /// <param name="damageableObject">The damageable object to attack.</param>
        public void Attack(IDamageable damageableObject)
        {
            IsAttacking = true;
            StartCoroutine(AttackCoroutine(damageableObject));
        }

        /// <summary>
        /// Coroutine for handling the attack process over time.
        /// </summary>
        /// <param name="damageableObject">The damageable object to attack.</param>
        private IEnumerator AttackCoroutine(IDamageable damageableObject)
        {
            while (damageableObject.IsAlive() && IsAttacking)
            {
                damageableObject.TakeDamage(_soldierData.DamagePoint);
                yield return new WaitForSeconds(1f);
            }

            IsAttacking = false;
        }

        private void StopAttack()
        {
            IsAttacking = false;
            _currentDamageableForAttack = null;
        }

        #endregion

        #region PLACEMENT METHODS
        
        private void Place()
        {
            // Convert the world position to grid cell coordinates.
            Vector3Int positionInt = GridBuildingSystem.Instance.GridLayout.LocalToCell(transform.position);

            BoundsInt areaTemp = _area;
            areaTemp.position = positionInt;

            GridBuildingSystem.Instance.TakeArea(areaTemp);
        }

        private void UnPlace()
        {
            GridBuildingSystem.Instance.ClearPlacedObject(transform.position, _area);
        }

        #endregion
        
        public override void End()
        {
            base.End();
            
            _currentDamageableForAttack = null;
            Deselect();
            IsAttacking = false;
        }
    }
}