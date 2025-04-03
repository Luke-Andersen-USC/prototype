using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityPhysics;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        Idle,
        Walking,
        Attacking,
        Jumping,
        Falling
    }
    
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpSpeed = 5f;
    public float rotationSpeed = 90f;
    public float gravity = -9.81f;
    
    [Header("Attacking")]
    public float attackDistance = 2f;
    public float attackRadius = 2f;
    
    public bool IsGrounded = false;
    private float _verticalVelocity = 0f;
    
    // Input
    private Vector2 _moveInput = Vector2.zero;
    private bool _isAttackPressed = false;
    private bool _isJumpPressed = false;
    
    // References
    private CharacterController _characterController;
    private Animator _animator;
    
    // Balloons
    private Tile _selectedTile = null;

    private PlayerState _currentState = PlayerState.Idle;
    void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _characterController.enableOverlapRecovery = true;
    }
    void Update()
    {
        if (_currentState == PlayerState.Falling)
        {
            UpdateFalling();
            return;
        }
        
        ProcessInput();

        Transform shipDeckTransform = PlayerManager.Instance.ShipDeck.transform;
        Vector3 move = Vector3.Normalize(shipDeckTransform.forward * _moveInput.y + shipDeckTransform.transform.right * _moveInput.x);
        if (move.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move, shipDeckTransform.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            
            _characterController.Move(Time.deltaTime * moveSpeed * move);
        }

        if (IsGrounded)
        {
            _verticalVelocity = 0.0f;

            transform.transform.localPosition = new Vector3(transform.transform.localPosition.x, 0.5f,
                transform.transform.localPosition.z);
        }
        else
        {
            _verticalVelocity += gravity * Time.deltaTime;
            _characterController.Move(_verticalVelocity * Time.deltaTime * Vector3.up);
        }

        if (i_interact.ReadValue<float>() > 0.1f)
        {
            SelectTileForBalloons();
        }
        else if (_selectedTile != null)
        {
            PlaceDownBalloon();
        }
        
        
        switch (_currentState)
        {
            case PlayerState.Idle:
                UpdateIdle();
                break;
            case PlayerState.Walking:
                UpdateWalking();
                break;
            case PlayerState.Jumping:
                UpdateJumping();
                break;
            case PlayerState.Attacking:
                UpdateAttacking();
                break;
        }
    }

    private void ProcessInput()
    {
        _moveInput = i_move.ReadValue<Vector2>();
        _isAttackPressed = i_attack.ReadValue<float>() > 0.1f;
        _isJumpPressed = i_jump.ReadValue<float>() > 0.1f;
    }

    private void UpdateIdle()
    {
        if (_moveInput.magnitude > 0.1f)
        {
            SwitchState(PlayerState.Walking);
        }
        else if (_isJumpPressed)
        {
            SwitchState(PlayerState.Jumping);
        }
        else if (_isAttackPressed)
        {
            SwitchState(PlayerState.Attacking);
        }
    }

    private void UpdateWalking()
    {
        if (_isJumpPressed)
        {
            SwitchState(PlayerState.Jumping);
        }
        else if (_isAttackPressed)
        {
            SwitchState(PlayerState.Attacking);
        }
        else if (_moveInput.magnitude < 0.1f)
        {
            SwitchState(PlayerState.Idle);
        }
    }

    private void UpdateJumping()
    {
        if (IsGrounded)
        {
            SwitchState(PlayerState.Idle);
        }
    }

    private void UpdateAttacking()
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, attackRadius, transform.forward, out hitInfo, attackDistance))
        {
            if (hitInfo.collider.CompareTag("Enemy"))
            {
                Skeleton sk = hitInfo.collider.gameObject.GetComponent<Skeleton>();
                if (sk)
                {
                    Debug.Log("Die should be called!");
                    sk.Die();
                } 
            }
        }
        
        if (stateInfo.normalizedTime >= 1.0f) 
        {
            SwitchState(PlayerState.Idle);
        }
    }
    
    private void UpdateFalling()
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.normalizedTime >= 1.0f) 
        {
            SwitchState(PlayerState.Idle);
        }
    }


    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.CompareTag("Ship") && _verticalVelocity < 0.1f)
        {
            IsGrounded = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Ship"))
        {
            IsGrounded = false;
        }
    }

    public void SwitchState(PlayerState state)
    {
        Debug.Log("Switched state to: " + state);
        
        _currentState = state;
        _animator.SetInteger("PlayerState", (int)_currentState);

        if (state == PlayerState.Jumping)
        {
            IsGrounded = false;
            _verticalVelocity = jumpSpeed;
        }
    }

    #region Balloons
    
    private void SelectTileForBalloons()
    {
        Tile closestTile = null;
        float closestAngle = float.MaxValue;
        
        Tile[,] tiles = Board.Instance.Tiles;
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                Tile tile = tiles[x, y];
                tile.Unhighlight();
                 
                if (tile.Balloon != null) continue;
                
                Vector3 toTile = tile.transform.position - transform.position;

                if (toTile.magnitude < 2.0f)
                {
                    toTile.Normalize();
                    
                    float angle = Vector3.Angle(transform.forward, toTile);

                    if (angle < closestAngle)
                    {
                        closestAngle = angle;
                        closestTile = tile;
                    }
                }
            }
        }

        _selectedTile = closestTile;
        if (_selectedTile != null) _selectedTile.Highlight();
        else
        {
            Debug.LogWarning("No valid tile was found for balloon placement!");
        }
    }

    
    private void PlaceDownBalloon()
    {
        if (_selectedTile != null)
        {
            ShipController.Instance.PlaceBalloon(_selectedTile);
            _selectedTile.Unhighlight();
        }
        _selectedTile = null;
    }
    
    #endregion

    [HideInInspector] public PlayerInput PlayerInput;
    [HideInInspector] public InputDevice InputDevice;
    [HideInInspector] public InputActionAsset m_inputAsset;
    [HideInInspector] public InputActionMap m_player;
    
    private InputAction i_move;
    private InputAction i_attack;
    private InputAction i_interact;
    private InputAction i_jump;

    public void SetupInput(InputDevice inputDevice)
    {
        InputDevice = inputDevice;
        PlayerInput = GetComponent<PlayerInput>();
        
        m_inputAsset = PlayerInput.actions;
        m_player = m_inputAsset.FindActionMap("Player");
        
        i_move = m_player.FindAction("Move");
        i_attack = m_player.FindAction("Attack");
        i_interact = m_player.FindAction("Interact");
        i_jump = m_player.FindAction("Jump");
    }
}
