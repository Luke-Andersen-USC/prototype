using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    enum PlayerState
    {
        Idle,
        Walking,
        Attacking,
        Jumping,
        Falling
    }
    
    // References

    // Movement
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float rotationSpeed = 90f;
    public float gravity = -9.81f;
    
    private bool _isGrounded = false;
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

    private void Start()
    {
        //throw new NotImplementedException();
    }

    void Update()
    {
        ProcessInput();

        Transform shipDeckTransform = PlayerManager.Instance.ShipDeck.transform;
        Vector3 move = Vector3.Normalize(shipDeckTransform.forward * _moveInput.y + shipDeckTransform.transform.right * _moveInput.x);
        if (move.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move, shipDeckTransform.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            
            _characterController.Move(Time.deltaTime * moveSpeed * move);
        }

        if (_isGrounded)
        {
            _verticalVelocity = 0.5f;
        }
        else
        {
            _verticalVelocity += gravity * Time.deltaTime;
        }
        
        /*
        LayerMask shipMask = LayerMask.GetMask("Ship");
        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, -transform.up);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 20.0f, shipMask))
        {
            //transform.position = hitInfo.point;
        }
        */
        
        //characterController.Move(_verticalVelocity * Time.deltaTime * shipDeckTransform.up);
        _characterController.Move(_verticalVelocity * Time.deltaTime * Vector3.up);

        //gameObject.transform.localRotation = Quaternion.identity;

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
        if (_isGrounded)
        {
            SwitchState(PlayerState.Idle);
        }
    }

    private void UpdateAttacking()
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
            _isGrounded = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Ship"))
        {
            _isGrounded = false;
        }
    }

    private void SwitchState(PlayerState state)
    {
        Debug.Log("Switched state to: " + state);
        
        _currentState = state;
        _animator.SetInteger("PlayerState", (int)_currentState);

        if (state == PlayerState.Jumping)
        {
            _isGrounded = false;
            _verticalVelocity = 10.0f;
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

    public PlayerInput PlayerInput;
    public InputDevice InputDevice;

    [Header("Input Variables")]
    public InputActionAsset m_inputAsset;
    public InputActionMap m_player;
    
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
