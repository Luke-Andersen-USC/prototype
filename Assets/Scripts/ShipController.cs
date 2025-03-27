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
    
    [Header("Pitch Vars")]
    [SerializeField] float maxPitchAngle = 15.0f;
    [SerializeField] float pitchK = 0.25f;
    [SerializeField] float pitchD = 0.5f;
    [SerializeField] private float pitchDeltaDistance;
    [SerializeField, Range(0f, 1f)] private float rollMinDelta;

    //[SerializeField] private float _playerWeight = 5.0f;
    //[SerializeField] private float _balloonWeight = 5.0f;
    //[SerializeField] private float _objectWeight = 5.0f;
    
    [Header("Ship Objects")]
    [SerializeField] private GameObject shipDeck;
    [SerializeField] private GameObject shipCenter;

    [Header("Ship UI")]
    [SerializeField] private TextMeshProUGUI pitchDeltaGUI;
    [SerializeField] private TextMeshProUGUI rollDeltaGUI;
    [SerializeField] private Slider pitchDeltaSlider;
    [SerializeField] private Slider rollDeltaSlider;
    [SerializeField] private GameObject centerOfBalanceDebug;
    
    [HideInInspector] public float pitchDelta { get; private set; }
    [HideInInspector] public float rollDelta { get; private set; }
    
    [HideInInspector] public List<GameObject> Balloons = new List<GameObject>();
    [HideInInspector] public List<GameObject> WeightedObjects = new List<GameObject>();
    
    private Quaternion originalRotation;
    private float rollVelocity = 0f;
    private float rollAcceleration = 0f;
    private float pitchVelocity = 0f;
    private float pitchAcceleration = 0f;
    
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

        pitchDeltaSlider.minValue = -1.0f;
        pitchDeltaSlider.maxValue = 1.0f;

        rollDeltaSlider.minValue = -1.0f;
        rollDeltaSlider.maxValue = 1.0f;

        originalRotation = shipDeck.transform.rotation;

        Instance = this;
    }
    
    private void Update()
    {
        UpdateDeltas();
        UpdateTilt();
        UpdateUI();
    }

    private void UpdateDeltas()
    {
        Vector2 centerOfBalance = Vector2.zero;
        foreach (GameObject obj in WeightedObjects)
        {
            centerOfBalance += new Vector2(obj.transform.localPosition.x, obj.transform.localPosition.z);
        }

        foreach (GameObject balloon in Balloons)
        {
            centerOfBalance += new Vector2(-balloon.transform.localPosition.x, -balloon.transform.localPosition.z);
        }

        centerOfBalance /= (float)(Balloons.Count + WeightedObjects.Count);
        
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
    /*
    private void UpdateTiltOld()
    {
        if (Mathf.Abs(rollDelta) > rollMinDelta)
        {
            rollVelocity = Mathf.MoveTowards(rollVelocity, rollDelta * maxRollVelocity, rollAcceleration * Time.deltaTime);
        }
        else
        {
            float rollDifference = Mathf.DeltaAngle(shipDeck.transform.rotation.eulerAngles.z, originalRotation.eulerAngles.z);
                
            float diff = Mathf.Clamp(rollDifference / 15f, -1.0f, 1.0f);
            
            rollVelocity = Mathf.MoveTowards(rollVelocity, diff * maxRollVelocity, 
                rollAcceleration * Time.deltaTime);
           
        }
        Quaternion rollRotation = Quaternion.Euler(0f, 0f, rollVelocity * Time.deltaTime);

        if (Mathf.Abs(pitchDelta) > pitchMinDelta)
        {
            pitchVelocity = Mathf.MoveTowards(pitchVelocity, pitchDelta * maxPitchVelocity, pitchAcceleration * Time.deltaTime);
        }
        else
        {
            float pitchDifference = Mathf.DeltaAngle(shipDeck.transform.rotation.eulerAngles.x, originalRotation.eulerAngles.x);
                
            float diff = Mathf.Clamp(pitchDifference / 15f, -1.0f, 1.0f);
            
            pitchVelocity = Mathf.MoveTowards(pitchVelocity, diff * maxPitchVelocity, 
                pitchAcceleration * Time.deltaTime);
        }
        Quaternion pitchRotation = Quaternion.Euler(pitchVelocity * Time.deltaTime, 0f, 0f);

        shipDeck.transform.rotation *= rollRotation * pitchRotation;
        shipDeck.transform.rotation = Quaternion.Euler(shipDeck.transform.rotation.eulerAngles.x, 0f, shipDeck.transform.rotation.eulerAngles.z);
    }
    */

    private void UpdateUI()
    {
        pitchDeltaGUI.text = "PitchDelta: " + pitchDelta;
        rollDeltaGUI.text = "RollDelta " + rollDelta;

        pitchDeltaSlider.value = pitchDelta;
        rollDeltaSlider.value = rollDelta;
    }
}
