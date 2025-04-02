using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class ShipController : MonoBehaviour
{
    [HideInInspector] public static ShipController Instance;
    
    [Header("Roll Vars")]
    [SerializeField] float maxRollAngle = 45.0f;
    [SerializeField] float rollK = 1.0f;
    [SerializeField] float rollD = 1.0f;
    [SerializeField] private float rollDeltaDistance;
    [SerializeField, Range(0f, 1f)] private float pitchMinDelta;

    [Header("Legacy Roll Vars")] 
    [SerializeField] private float legacyMaxRollVelocity = 10.0f;
    [SerializeField] private float legacyRollAcceleration = 5.0f;
    
    [Header("Pitch Vars")]
    [SerializeField] float maxPitchAngle = 15.0f;
    [SerializeField] float pitchK = 0.25f;
    [SerializeField] float pitchD = 0.5f;
    [SerializeField] private float pitchDeltaDistance;
    [SerializeField, Range(0f, 1f)] private float rollMinDelta;
    [Header("Legacy Pitch Vars")] 
    [SerializeField] private float legacyMaxPitchVelocity = 4.0f;
    [SerializeField] private float legacyPitchAcceleration = 1.0f;

    [Header("Ship Tilt Mode")] 
    [SerializeField] private bool _useLegacy = false;

    [Header("Weights")]
    [SerializeField] private float _playerWeight = 5.0f;
    [SerializeField] private float _balloonWeight = 1.0f;
    [SerializeField] private float _objectWeight = 1.0f;
    
    [Header("Ship General")]
    [SerializeField] private GameObject shipDeck;
    [SerializeField] private GameObject shipCenter;
    [SerializeField] private List<Tile> startingBalloonTiles;

    [Header("Ship UI")]
    [SerializeField] private Canvas afloatUI;
    [SerializeField] private TextMeshProUGUI pitchDeltaGUI;
    [SerializeField] private TextMeshProUGUI rollDeltaGUI;
    [SerializeField] private Slider pitchDeltaSlider;
    [SerializeField] private Slider rollDeltaSlider;
    [SerializeField] private GameObject centerOfBalanceDebug;
    
    
    // NOT SERIALIZED
    [HideInInspector] public float pitchDelta { get; private set; }
    [HideInInspector] public float rollDelta { get; private set; }

    private bool _isAfloat = true;
    
    private Quaternion originalRotation;
    private float rollVelocity = 0f;
    private float rollAcceleration = 0f;
    private float pitchVelocity = 0f;
    private float pitchAcceleration = 0f;
    
    [HideInInspector] public List<GameObject> Players = new List<GameObject>();
    [HideInInspector] public List<GameObject> Balloons = new List<GameObject>();
    [HideInInspector] public List<GameObject> WeightedObjects = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameObject[] weighted = GameObject.FindGameObjectsWithTag("Weighted");
        WeightedObjects.Clear();

        foreach (GameObject obj in weighted)
        {
            WeightedObjects.Add(obj);
        }
        
        GameObject[] balloons = GameObject.FindGameObjectsWithTag("Balloon");
        Balloons.Clear();

        foreach (GameObject balloon in balloons)
        {
            Balloons.Add(balloon);
        }
        
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Players.Clear();

        foreach (GameObject player in players)
        {
            Players.Add(player);
        }

        originalRotation = shipDeck.transform.rotation;

        foreach (Tile tile in startingBalloonTiles)
        {
            PlaceBalloon(tile);
        }
    }

    private void Update()
    {
        if (_isAfloat)
        {
            UpdateDeltas();
            if (_useLegacy)
            {
                UpdateTiltLegacy();
            }
            else
            {
                UpdateTilt();
            }
            CheckAfloat();
            UpdateUI();
        }
    }

    private void UpdateDeltas()
    {
        Vector2 centerOfBalance = Vector2.zero;
        
        foreach (GameObject player in Players)
        {
            centerOfBalance += new Vector2(player.transform.localPosition.x, player.transform.localPosition.z) * _playerWeight;
        }

        foreach (GameObject balloon in Balloons)
        {
            centerOfBalance += new Vector2(-balloon.transform.localPosition.x, -balloon.transform.localPosition.z) * _balloonWeight;
        }
        
        
        foreach (GameObject obj in WeightedObjects)
        {
            centerOfBalance += new Vector2(obj.transform.localPosition.x, obj.transform.localPosition.z) * _objectWeight;
        }
        

        if (centerOfBalance.magnitude > 0.01f) centerOfBalance /= (Players.Count * _playerWeight + Balloons.Count * _balloonWeight + WeightedObjects.Count * _objectWeight);
        
        centerOfBalanceDebug.transform.position = shipCenter.transform.position + shipDeck.transform.forward * centerOfBalance.y +
                                                  shipDeck.transform.right * centerOfBalance.x + shipDeck.transform.up * 1.5f;

        rollDelta = Mathf.Clamp(-centerOfBalance.x / rollDeltaDistance, -1.0f, 1.0f);
        pitchDelta = Mathf.Clamp(centerOfBalance.y / pitchDeltaDistance, -1.0f, 1.0f);
    }
    
    private void UpdateTilt()
    {
        float targetRollAngle = (Mathf.Abs(rollDelta) > rollMinDelta) ? rollDelta * maxRollAngle : 0f;
        float targetPitchAngle = (Mathf.Abs(pitchDelta) > pitchMinDelta) ? pitchDelta * maxPitchAngle : 0f;;

        float deltaRoll = Mathf.DeltaAngle(targetRollAngle, shipDeck.transform.rotation.eulerAngles.z);
        float deltaPitch = Mathf.DeltaAngle(targetPitchAngle, shipDeck.transform.rotation.eulerAngles.x);


        rollAcceleration = -rollK * deltaRoll - rollD * rollVelocity;
        rollVelocity += rollAcceleration * Time.deltaTime;
        Quaternion rollRotation = Quaternion.Euler(0f, 0f, rollVelocity * Time.deltaTime);
        
        pitchAcceleration = -pitchK * deltaPitch - pitchD * pitchVelocity;
        pitchVelocity += pitchAcceleration * Time.deltaTime;
        Quaternion pitchRotation = Quaternion.Euler(pitchVelocity * Time.deltaTime, 0f, 0f);
            
        shipDeck.transform.rotation *= rollRotation * pitchRotation;
        shipDeck.transform.rotation = Quaternion.Euler(shipDeck.transform.rotation.eulerAngles.x, 0f, shipDeck.transform.rotation.eulerAngles.z);
    }

    // LEGACY - keeping around it case we want to revert
    private void UpdateTiltLegacy()
    {
        if (Mathf.Abs(rollDelta) > rollMinDelta)
        {
            rollVelocity = Mathf.MoveTowards(rollVelocity, rollDelta * legacyMaxRollVelocity, legacyRollAcceleration * Time.deltaTime);
        }
        else
        {
            float rollDifference = Mathf.DeltaAngle(shipDeck.transform.rotation.eulerAngles.z, originalRotation.eulerAngles.z);
                
            float diff = Mathf.Clamp(rollDifference / 15f, -1.0f, 1.0f);
            
            rollVelocity = Mathf.MoveTowards(rollVelocity, diff * legacyMaxRollVelocity, 
                legacyRollAcceleration * Time.deltaTime);
           
        }
        Quaternion rollRotation = Quaternion.Euler(0f, 0f, rollVelocity * Time.deltaTime);

        if (Mathf.Abs(pitchDelta) > pitchMinDelta)
        {
            pitchVelocity = Mathf.MoveTowards(pitchVelocity, pitchDelta * legacyMaxPitchVelocity, legacyPitchAcceleration * Time.deltaTime);
        }
        else
        {
            float pitchDifference = Mathf.DeltaAngle(shipDeck.transform.rotation.eulerAngles.x, originalRotation.eulerAngles.x);
                
            float diff = Mathf.Clamp(pitchDifference / 15f, -1.0f, 1.0f);
            
            pitchVelocity = Mathf.MoveTowards(pitchVelocity, diff * legacyMaxPitchVelocity, 
                legacyPitchAcceleration * Time.deltaTime);
        }
        Quaternion pitchRotation = Quaternion.Euler(pitchVelocity * Time.deltaTime, 0f, 0f);

        shipDeck.transform.rotation *= rollRotation * pitchRotation;
        shipDeck.transform.rotation = Quaternion.Euler(shipDeck.transform.rotation.eulerAngles.x, 0f, shipDeck.transform.rotation.eulerAngles.z);
    }

    private void CheckAfloat()
    {
        float rollDiff = Mathf.DeltaAngle(0f, shipDeck.transform.rotation.eulerAngles.z);
        //float pitchDiff = Mathf.DeltaAngle(0f, shipDeck.transform.rotation.eulerAngles.x);
        float offset = 10.0f;
        if (rollDiff >= maxRollAngle - offset)
        {
            Debug.Log("Upturning ship!");
            KillShip();
        }
    }
    public void PlaceBalloon(Tile tile)
    {
        GameObject balloon = Instantiate(PlayerManager.Instance.BalloonsPrefab, tile.transform.position, Quaternion.identity);
        balloon.transform.SetParent(shipDeck.transform);
        Balloons.Add(balloon);
        tile.Balloon = balloon;
    }
    
    public void PopBalloon(GameObject balloon)
    {
        Balloons.Remove(balloon);
        Destroy(balloon);
    }

    private void KillShip()
    {
        _isAfloat = false;
        while (Balloons.Count > 0)
        {
            PopBalloon(Balloons[0]);
        }
        shipDeck.GetComponent<Rigidbody>().useGravity = true;

        foreach (GameObject player in Players)
        {
            player.transform.SetParent(null);
            player.GetComponent<PlayerController>().SwitchState(PlayerController.PlayerState.Falling);
        }
        
        afloatUI.enabled = false;
    }
    
    

    private void UpdateUI()
    {
        pitchDeltaGUI.text = "PitchDelta: " + pitchDelta;
        rollDeltaGUI.text = "RollDelta " + rollDelta;

        pitchDeltaSlider.value = -pitchDelta;
        rollDeltaSlider.value = -rollDelta;
    }
}
