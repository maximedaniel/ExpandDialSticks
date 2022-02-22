using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyFakeCapsuleHand : MonoBehaviour, IArmController
{
    // Constants
    private const int SEPARATION_LEVEL = 3; 
    private const int SEPARATION_LAYER = 10; // Safety Level 0
    private const float HAND_RADIUS = 0.10f;
    private const float FOREARM_RADIUS = 0.05f;
    private const float STOP_RADIUS = 0.03f;
    private const float FOREARM_HEIGHT = 0.40f;

    // Attributs
    public Chirality handedness;
    private GameObject _anchorObject;
    private GameObject[] _handColliders;
    private GameObject[] _forearmColliders;

    // Getters and Setter
    public Chirality Handedness
    {
        get
        {
            return handedness;
        }
        set { }
    }


    // Start is called before the first frame update
    void Start()
    {
        _anchorObject = this.gameObject;    

        // Create Hand Collider
        _handColliders = new GameObject[SEPARATION_LEVEL];
        for (var i = 0; i < SEPARATION_LEVEL; i++)
        {
            var handColliderName = "Simulate" + Handedness + "HandCollider" + i;
            _handColliders[i] = GameObject.Find(handColliderName);
            if (_handColliders[i] == null)
            {
                _handColliders[i] = new GameObject(handColliderName);
                _handColliders[i].transform.parent = _anchorObject.transform;
                _handColliders[i].transform.position = _anchorObject.transform.position;
                _handColliders[i].transform.localScale = _anchorObject.transform.localScale;
                _handColliders[i].gameObject.tag = "Player";
                _handColliders[i].gameObject.layer = SEPARATION_LAYER + i + 1;
                SphereCollider sc = _handColliders[i].AddComponent<SphereCollider>();
                sc.radius = HAND_RADIUS;
                sc.radius += i * STOP_RADIUS;
                sc.enabled = true;
            }
        }
        // Create Arm Collider
        _forearmColliders = new GameObject[SEPARATION_LEVEL];
        for (var i = 0; i < SEPARATION_LEVEL; i++)
        {
            var forearmColliderName = "Simulate" +  Handedness + "ForearmCollider" + i;
            _forearmColliders[i] = GameObject.Find(forearmColliderName);
            if (_forearmColliders[i] == null)
            {
                _forearmColliders[i] = new GameObject(forearmColliderName);
                Vector3 forearmPosition = _anchorObject.transform.position + _anchorObject.transform.right * (HAND_RADIUS + FOREARM_HEIGHT/2f);
                _forearmColliders[i].transform.parent = _anchorObject.transform;
                _forearmColliders[i].transform.position = forearmPosition;
                _forearmColliders[i].transform.rotation = Quaternion.LookRotation(-_anchorObject.transform.right, _anchorObject.transform.forward);

                _forearmColliders[i].transform.localScale = _anchorObject.transform.localScale;
                _forearmColliders[i].gameObject.tag = "Player";
                _forearmColliders[i].gameObject.layer = SEPARATION_LAYER + i + 1;
                CapsuleCollider cc = _forearmColliders[i].AddComponent<CapsuleCollider>();
                cc.radius = FOREARM_RADIUS;
                cc.radius += i * STOP_RADIUS;
                cc.height = FOREARM_HEIGHT + cc.radius * 2f;
                cc.direction = 2;
                cc.enabled = true;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
       /* for (var i = 0; i < SEPARATION_LEVEL; i++)
        {
            SphereCollider sc = _handColliders[i].AddComponent<SphereCollider>();
            sc.radius = INIT_RADIUS;
            sc.radius += i * STOP_RADIUS;
        }*/
    }

    private void InstantiateGameObjects()
    {

        // Init FOREARM COLLIDERS
        /*_forearmColliders = new GameObject[SEPARATION_LEVEL];
        for (var i = 0; i < SEPARATION_LEVEL; i++)
        {
            var forearmColliderName = Handedness + "ForearmCollider" + i;
            _forearmColliders[i] = GameObject.Find(forearmColliderName);
            if (_forearmColliders[i] == null)
            {
                _forearmColliders[i] = new GameObject(forearmColliderName);
                _forearmColliders[i].AddComponent<CapsuleCollider>();
                _forearmColliders[i].transform.parent = _handObject.transform;
                _forearmColliders[i].gameObject.tag = "Player";
                _forearmColliders[i].gameObject.layer = SEPARATION_LAYER + i + 1;
                _forearmColliders[i].GetComponent<CapsuleCollider>().enabled = true;
            }
        }*/
    }

	public GameObject GetHandColliderAt(int index)
	{
        return _handColliders[index];
	}

	public GameObject GetArmColliderAt(int index)
    {
        return _forearmColliders[index];
    }

	public bool IsActive()
	{
        return true;
	}
}
