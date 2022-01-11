/******************************************************************************
 * Copyright (C) Ultraleap, Inc. 2011-2020.                                   *
 *                                                                            *
 * Use subject to the terms of the Apache License 2.0 available at            *
 * http://www.apache.org/licenses/LICENSE-2.0, or another agreement           *
 * between Ultraleap and you, your company or other organization.             *
 ******************************************************************************/

using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using Leap.Unity.Attributes;
using System;
using g3;

namespace Leap.Unity {
  /** A basic Leap hand model constructed dynamically vs. using pre-existing geometry*/
  public class MyCapsuleHand : HandModelBase {
    /// <summary>
    /// Extract translation from transform matrix.
    /// </summary>
    /// <param name="matrix">Transform matrix. This parameter is passed by reference
    /// to improve performance; no changes will be made to it.</param>
    /// <returns>
    /// Translation offset.
    /// </returns>
    public static Vector3 ExtractTranslationFromMatrix(ref Matrix4x4 matrix)
    {
        Vector3 translate;
        translate.x = matrix.m03;
        translate.y = matrix.m13;
        translate.z = matrix.m23;
        return translate;
    }

    /// <summary>
    /// Extract rotation quaternion from transform matrix.
    /// </summary>
    /// <param name="matrix">Transform matrix. This parameter is passed by reference
    /// to improve performance; no changes will be made to it.</param>
    /// <returns>
    /// Quaternion representation of rotation transform.
    /// </returns>
    public static Quaternion ExtractRotationFromMatrix(ref Matrix4x4 matrix)
    {
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;

        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;

        return Quaternion.LookRotation(forward, upwards);
    }

    /// <summary>
    /// Extract scale from transform matrix.
    /// </summary>
    /// <param name="matrix">Transform matrix. This parameter is passed by reference
    /// to improve performance; no changes will be made to it.</param>
    /// <returns>
    /// Scale vector.
    /// </returns>
    public static Vector3 ExtractScaleFromMatrix(ref Matrix4x4 matrix)
    {
        Vector3 scale;
        scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
        scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
        scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
        return scale;
    }

    /// <summary>
    /// Extract position, rotation and scale from TRS matrix.
    /// </summary>
    /// <param name="matrix">Transform matrix. This parameter is passed by reference
    /// to improve performance; no changes will be made to it.</param>
    /// <param name="localPosition">Output position.</param>
    /// <param name="localRotation">Output rotation.</param>
    /// <param name="localScale">Output scale.</param>
    public static void DecomposeMatrix(ref Matrix4x4 matrix, out Vector3 localPosition, out Quaternion localRotation, out Vector3 localScale)
    {
        localPosition = ExtractTranslationFromMatrix(ref matrix);
        localRotation = ExtractRotationFromMatrix(ref matrix);
        localScale = ExtractScaleFromMatrix(ref matrix);
    }

    /// <summary>
    /// Set transform component from TRS matrix.
    /// </summary>
    /// <param name="transform">Transform component.</param>
    /// <param name="matrix">Transform matrix. This parameter is passed by reference
    /// to improve performance; no changes will be made to it.</param>
    public static void SetTransformFromMatrix(Transform transform, ref Matrix4x4 matrix)
    {
        transform.localPosition = ExtractTranslationFromMatrix(ref matrix);
        transform.localRotation = ExtractRotationFromMatrix(ref matrix);
        transform.localScale = ExtractScaleFromMatrix(ref matrix);
    }


    // EXTRAS!

    /// <summary>
    /// Identity quaternion.
    /// </summary>
    /// <remarks>
    /// <para>It is faster to access this variation than <c>Quaternion.identity</c>.</para>
    /// </remarks>
    public static readonly Quaternion IdentityQuaternion = Quaternion.identity;
    /// <summary>
    /// Identity matrix.
    /// </summary>
    /// <remarks>
    /// <para>It is faster to access this variation than <c>Matrix4x4.identity</c>.</para>
    /// </remarks>
    public static readonly Matrix4x4 IdentityMatrix = Matrix4x4.identity;

    /// <summary>
    /// Get translation matrix.
    /// </summary>
    /// <param name="offset">Translation offset.</param>
    /// <returns>
    /// The translation transform matrix.
    /// </returns>
    public static Matrix4x4 TranslationMatrix(Vector3 offset)
    {
        Matrix4x4 matrix = IdentityMatrix;
        matrix.m03 = offset.x;
        matrix.m13 = offset.y;
        matrix.m23 = offset.z;
        return matrix;
    }

    public MyCapsuleHand otherHand;

    private bool frozen = false;
    private const int TOTAL_JOINT_COUNT = 4 * 5;
    private const float CYLINDER_MESH_RESOLUTION = 0.1f; //in centimeters, meshes within this resolution will be re-used
    private const int THUMB_BASE_INDEX = (int)Finger.FingerType.TYPE_THUMB * 4;
    private const int PINKY_BASE_INDEX = (int)Finger.FingerType.TYPE_PINKY * 4;
    private const int INDEX_BASE_INDEX = (int)Finger.FingerType.TYPE_INDEX * 4;

    private static int _leftColorIndex = 0;
    private static int _rightColorIndex = 0;
    private static Color[] _leftColorList  = { new Color(0.0f, 0.0f, 1.0f), new Color(0.2f, 0.0f, 0.4f), new Color(0.0f, 0.2f, 0.2f) };
    private static Color[] _rightColorList = { new Color(1.0f, 0.0f, 0.0f), new Color(1.0f, 1.0f, 0.0f), new Color(1.0f, 0.5f, 0.0f) };

    #pragma warning disable 0649
    [SerializeField]
    private Chirality handedness;

    [SerializeField]
    private bool _showArm = true;

    [SerializeField]
    private bool _castShadows = true;

    [SerializeField]
    private Material _material;
    private Material _backing_material;

    [SerializeField]
    public Mesh _sphereMesh;
    public Mesh _cylinderMesh;

   /* [MinValue(3)]
    [SerializeField]
    private int _cylinderResolution = 12;*/

    [MinValue(0)]
    [SerializeField]
    private float _jointRadius = 0.006f;

    [MinValue(0)]
    [SerializeField]
    private float _cylinderRadius = 0.006f;
    public float _cylinderColliderRadius = 0.012f;

   /* [MinValue(0)]
    [SerializeField]
    private float _palmRadius = 0.015f;*/
#pragma warning restore 0649

    private Color _bodyColor = Color.gray; //Color.black
    public Material _quadMaterial;
    public Material _sphereMaterial;
    public Material _colliderMaterial;

    private Mesh _quadMesh;
    //private Material _sphereMaterial;
    private Hand _hand;
    private Vector3[] _spherePositions;
    private Matrix4x4[] _sphereMatrices   = new Matrix4x4[32], 
                        _cylinderMatrices = new Matrix4x4[32],
                        _colliderMatrices = new Matrix4x4[32];
    private Mesh[] _colliderMeshes = new Mesh[32];
    private const int SEPARATION_LEVEL = 3;
    private const int SEPARATION_LAYER = 10; // Safety Level 0
    private GameObject _handObject;
    private GameObject[] _fillColliders;
    private const int _fillCollidersSize = 3;
    private GameObject[] _fingerColliders;
    private const int _fingerCollidersSize = 27;
    private GameObject[] _handColliders;
    private GameObject [] _forearmColliders;
   // private float handColliderOffset = 2.5f; //6f
    private float pinHalfWidth = 0.03f; //6f
    private float forearmColliderOffset = 0.24f;
    private CombineInstance[] combine;
    private int _curSphereIndex, _curCylinderIndex;

    public override ModelType HandModelType {
      get {
        return ModelType.Graphics;
      }
    }

    public override Chirality Handedness {
      get {
        return handedness;
      }
      set { }
    }

    public override bool SupportsEditorPersistence() {
      return false;
    }

    public override Hand GetLeapHand() {
      return _hand;
    }
    
    public  bool IsActive() {
        return _handObject != null && _handObject.activeSelf;
        //return (_handColliders != null && _handColliders[0].GetComponent<SphereCollider>().enabled);
    }

    public  GameObject GetHandCollider() {
      return _handColliders[0];
    }

    public  GameObject GetArmCollider() {
      return _forearmColliders[0];
    }

    public override void SetLeapHand(Hand hand) {
      _hand = hand;
    }

    public void Unfreeze()
	{
        frozen = false;
	}
    public void Freeze()
	{
        frozen = true;
	}

    private void InstantiateGameObjects()
	{
        // ME
        // First Create Hand
        var handObjectName = Handedness + "Hand";
        GameObject handObjectFound = GameObject.Find(handObjectName);
		if (handObjectFound == null)
		{
            handObjectFound = new GameObject(handObjectName);
            handObjectFound.gameObject.tag = "Player";

        }
        _handObject = handObjectFound;

        // Init hand projector
       /* var handProjectorName = Handedness + "HandProjector";
        handProjectorObject = GameObject.Find(handProjectorName);
        if(handProjectorObject == null)
		{
                handProjectorObject = Instantiate(handProjectorPrefab);
        }
        handProjector = handProjectorObject.GetComponent<Projector>();
            
        // Init arm projector
        var armProjectorName = Handedness + "ArmProjector";
        armProjectorObject = GameObject.Find(armProjectorName);
        if(armProjectorObject == null)
		{
                armProjectorObject = Instantiate(armProjectorPrefab);
        }
        armProjector = armProjectorObject.GetComponent<Projector>();*/

        // Init FILL COLLIDERS
        _fillColliders = new GameObject[_fillCollidersSize];
        for (var i = 0; i < _fillCollidersSize; i++)
        {
            var fillColliderName = Handedness + "FillCollider" + i;
            _fillColliders[i] = GameObject.Find(fillColliderName);
            if (_fillColliders[i] == null)
            {
                _fillColliders[i] = new GameObject(fillColliderName);
                MeshCollider meshCollider = _fillColliders[i].AddComponent<MeshCollider>();
                _fillColliders[i].transform.parent = _handObject.transform;
                _fillColliders[i].gameObject.tag = "Player";
                _fillColliders[i].gameObject.layer = SEPARATION_LAYER;
                _fillColliders[i].GetComponent<MeshCollider>().enabled = true;
            }
        }
        
        // Init FINGER COLLIDERS
        _fingerColliders = new GameObject[_fingerCollidersSize];
        for (var i = 0; i < _fingerCollidersSize; i++)
        {
            var fingerColliderName = Handedness + "FingerCollider" + i;
            _fingerColliders[i] = GameObject.Find(fingerColliderName);
            if (_fingerColliders[i] == null)
            {
                _fingerColliders[i] = new GameObject(fingerColliderName);
                _fingerColliders[i].AddComponent<CapsuleCollider>();
                _fingerColliders[i].transform.parent = _handObject.transform;
                _fingerColliders[i].gameObject.tag = "Player";
                _fingerColliders[i].gameObject.layer = SEPARATION_LAYER+1;
                _fingerColliders[i].GetComponent<CapsuleCollider>().enabled = true;
            }
        }

        // Init HAND COLLIDERS
        _handColliders = new GameObject[SEPARATION_LEVEL];
        for(var i = 0; i < SEPARATION_LEVEL; i++)
            {
                var handColliderName = Handedness + "HandCollider" + i;
                _handColliders[i] = GameObject.Find(handColliderName);
                if (_handColliders[i] == null)
                {
                    _handColliders[i] = new GameObject(handColliderName);
                    _handColliders[i].AddComponent<SphereCollider>();
                    _handColliders[i].transform.parent = _handObject.transform;
                    _handColliders[i].gameObject.tag = "Player";
                    _handColliders[i].gameObject.layer = SEPARATION_LAYER + i + 1;
                    _handColliders[i].GetComponent<SphereCollider>().enabled = true;
                }
            }
        
        // Init FOREARM COLLIDERS
        _forearmColliders = new GameObject[SEPARATION_LEVEL];
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
        }
    }
    private void setCollisionMode(bool enabled)
	{
        if (_fillColliders!= null && _fingerColliders!= null && _handColliders != null && _forearmColliders != null)
        {
            for (var i = 0; i < _fillCollidersSize; i++)
            {
                if (_fillColliders[i] != null)
                {
                    _fillColliders[i].GetComponent<MeshCollider>().enabled = enabled;
                }
            }
            for (var i = 0; i < _fingerCollidersSize; i++)
            {
                if (_fingerColliders[i] != null)
                {
                    _fingerColliders[i].GetComponent<CapsuleCollider>().enabled = enabled;
                }
            }
            for (var i = 0; i < SEPARATION_LEVEL; i++)
            {
                if (_handColliders[i] != null)
                {
                    _handColliders[i].GetComponent<SphereCollider>().enabled = enabled;
                }
                if (_forearmColliders[i] != null)
                {
                    _forearmColliders[i].GetComponent<CapsuleCollider>().enabled = enabled;
                }
            }
        }
    }
    private void HideGameObjects()
	{
            if (_handObject != null)
                _handObject.SetActive(false);
           
            setCollisionMode(false);
    }
    private void ShowGameObjects()
    {
        if (_handObject != null)
            _handObject.SetActive(true);

        setCollisionMode(true);
    }

     public override void InitHand() {
      //Debug.Log(handedness + "InitHand()");
      if (_fillColliders== null || _fingerColliders == null ||  _handColliders == null || _forearmColliders == null) InstantiateGameObjects();

            /*if (_material != null && (_backing_material == null || !_backing_material.enableInstancing)) {
              _backing_material = new Material(_material);
              _backing_material.hideFlags = HideFlags.DontSaveInEditor;
              if(!Application.isEditor && !_backing_material.enableInstancing) {
                Debug.LogError("Capsule Hand Material needs Instancing Enabled to render in builds!", this);
              }
              _backing_material.enableInstancing = true;
              _sphereMaterial = new Material(_backing_material);
              _sphereMaterial.hideFlags = HideFlags.DontSaveInEditor;
            }*/
            if (_sphereMaterial != null)
            {
                _sphereMaterial.enableInstancing = true;
                _sphereMaterial.hideFlags = HideFlags.DontSaveInEditor;
            }
            if (_quadMaterial != null)
            {
                _quadMaterial.enableInstancing = true;
                _quadMaterial.hideFlags = HideFlags.DontSaveInEditor;
            }
        }

    #if UNITY_EDITOR
    private void OnValidate() {
      if (_material == null || !_material.enableInstancing) {
        Debug.LogWarning("CapsuleHand's Material must have " +
          "instancing enabled in order to work in builds! Replacing " +
          "Material with a Default Material now...", this);
        _material = (Material)UnityEditor.AssetDatabase.LoadAssetAtPath(
          System.IO.Path.Combine("Assets", "Plugins", "LeapMotion",
          "Core", "Materials", "InstancedCapsuleHand.mat"), typeof(Material));
      }
    }
    #endif

    public override void BeginHand() {
      if (frozen) return;  // do nothing if frozen

      Debug.Log(handedness + "BeginHand()");
      base.BeginHand();
      if (_hand.IsLeft) {
        _sphereMaterial.color = _bodyColor;// _leftColorList[_leftColorIndex];
        _leftColorIndex = (_leftColorIndex + 1) % _leftColorList.Length;
      } else {
        _sphereMaterial.color = _bodyColor;// _rightColorList[_rightColorIndex];
        _rightColorIndex = (_rightColorIndex + 1) % _rightColorList.Length;
      }
      ShowGameObjects();

        // If the other hand is still tracked
        if (!otherHand.IsTracked)
        {
            // then hide this one
            otherHand.HideGameObjects();
        }
        
    }
    public override void FinishHand()
    {
           
         if (frozen) return;  // do nothing if frozen
         Debug.Log(handedness + "FinishHand()");
         base.FinishHand();
        // If the other hand is still tracked
        if (otherHand.IsTracked)
        {
            // then hide this one
            this.HideGameObjects();
        }

    }
        // Project /point/ onto a line.
        public static Vector3 ProjectPointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 relativePoint = point - lineStart;
        Vector3 lineDirection = lineEnd - lineStart;
        float length = lineDirection.magnitude;
        Vector3 normalizedLineDirection = lineDirection;
        if (length > .000001f)
            normalizedLineDirection /= length;

        float dot = Vector3.Dot(normalizedLineDirection, relativePoint);
        dot = Mathf.Clamp(dot, 0.0F, length);

        return lineStart + normalizedLineDirection * dot;
    }

        // Calculate distance between a point and a line.
    public static float DistancePointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        return Vector3.Magnitude(ProjectPointLine(point, lineStart, lineEnd) - point);
    }

    public float GetDistanceFromHand(Vector3 point)
    {
            Vector3 projectedPoint = new Vector3(point.x,0,point.z);
            float distance = Mathf.Infinity;
            if(_hand != null)
            {
                Vector3 palm = _hand.PalmPosition.ToVector3();
                Vector3 projectedPalm = new Vector3(palm.x, 0, palm.z);
                Vector3 wrist = _hand.Arm.WristPosition.ToVector3();
                Vector3 projectedWrist = new Vector3(wrist.x, 0, wrist.z);
                Vector3 elbow = _hand.Arm.ElbowPosition.ToVector3();
                Vector3 projectedElbow = new Vector3(elbow.x, 0, elbow.z);
                float distanceFromPalmWrist = DistancePointLine(projectedPoint, projectedPalm, projectedWrist);
                float distanceFromWristElbow = DistancePointLine(projectedPoint, projectedWrist, projectedElbow);
                distance = Mathf.Min(distanceFromPalmWrist, distanceFromWristElbow);
            }
            return distance;
    }

  public override string ToString(){
    if(_hand == null || (_hand.IsLeft && handedness != Chirality.Left) || (!_hand.IsLeft && handedness != Chirality.Right)) return null;
    string s = "";
    for (var i = 0; i < SEPARATION_LEVEL; i++)
    {
        s += "HAND_LEVEL " + i 
                    + " POSITION " + _handColliders[i].transform.position 
                    + " RADIUS " + _handColliders[i].GetComponent<SphereCollider>().radius 
                    + " ";

        CapsuleCollider capsuleCollider = _forearmColliders[i].GetComponent<CapsuleCollider>();
        s += "ARM_LEVEL " + i 
            + " POSITION " + _forearmColliders[i].transform.position
            + " ROTATION " + _forearmColliders[i].transform.rotation
            + " RADIUS " + capsuleCollider.radius
            + " HEIGHT " + capsuleCollider.height
            + " DIRECTION " + capsuleCollider.direction
            + " ";
    }
    return s;
  }

    private void configureFillColliderAt(int index, Mesh mesh)
	{
            
        MeshCollider meshCollider = _fillColliders[index].GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;    

        /*Vector3 fingerCenter = from + (to - from)/2f;
        Quaternion fingerRotation = Quaternion.LookRotation(to - from);
        _fingerColliders[index].transform.position = fingerCenter;
        _fingerColliders[index].transform.rotation = fingerRotation;
        CapsuleCollider capsuleCollider = _fingerColliders[index].GetComponent<CapsuleCollider>();
        capsuleCollider.radius = transform.lossyScale.x * _cylinderRadius;
        capsuleCollider.height = Vector3.Distance(from, to) + 2f * transform.lossyScale.x * _jointRadius;
        capsuleCollider.direction = 2;*/
	}
    private void configureFingerColliderAt(int index, Vector3 from, Vector3 to, int separationLevel)
	{
            
        Vector3 fingerCenter = from + (to - from)/2f;
        Quaternion fingerRotation = Quaternion.LookRotation(to - from);
        _fingerColliders[index].transform.position = fingerCenter;
        _fingerColliders[index].transform.rotation = fingerRotation;
        _fingerColliders[index].gameObject.layer = separationLevel;
        CapsuleCollider capsuleCollider = _fingerColliders[index].GetComponent<CapsuleCollider>();
        capsuleCollider.radius = transform.lossyScale.x * _cylinderColliderRadius;
        capsuleCollider.height = Vector3.Distance(from, to) + 2f * transform.lossyScale.x * _jointRadius;
        capsuleCollider.direction = 2;
	}

  public override void UpdateHand() {
      //Debug.Log(handedness + "UpdateHand()");
      if(frozen) return;
      if (_fillColliders == null || _fingerColliders == null || _handColliders == null || _forearmColliders == null) return; //InstantiateGameObjects();

      int _currFingerColliderIndex = 0;
      int _currFillColliderIndex = 0;
      _curSphereIndex = 0;
      _curCylinderIndex = 0;

      if (_spherePositions == null || _spherePositions.Length != TOTAL_JOINT_COUNT) {
        _spherePositions = new Vector3[TOTAL_JOINT_COUNT];
      }

      /*if (_material != null && (_backing_material == null || !_backing_material.enableInstancing)) {
        _backing_material = new Material(_material);
        _backing_material.hideFlags = HideFlags.DontSaveInEditor;
        _backing_material.enableInstancing = true;
        _sphereMaterial = new Material(_backing_material);
        _sphereMaterial.hideFlags = HideFlags.DontSaveInEditor;
      }*/

      //Update all joint spheres in the fingers
      foreach (var finger in _hand.Fingers) {
        for (int j = 0; j < 4; j++) {
          int key = getFingerJointIndex((int)finger.Type, j);
          Vector3 position = finger.Bone((Bone.BoneType)j).NextJoint.ToVector3();
          _spherePositions[key] = position;
          drawSphere(position);
        }
      }

      //Now we just have a few more spheres for the hands
      //PalmPos, WristPos, and mockThumbJointPos, which is derived and not taken from the frame obj

      Vector3 palmPosition = _hand.PalmPosition.ToVector3();
      /*drawSphere(palmPosition, _palmRadius);*/

      Vector3 thumbBaseToPalm = _spherePositions[THUMB_BASE_INDEX] - _hand.PalmPosition.ToVector3();
      Vector3 mockThumbJointPos = _hand.PalmPosition.ToVector3() + Vector3.Reflect(thumbBaseToPalm, _hand.Basis.xBasis.ToVector3());
      drawSphere(mockThumbJointPos);

        // Update Hand Collider
        /*for (int i = 0; i < _spherePositions.Length; i++)
        {
                _sphereColliders[i].transform.position = _spherePositions[i];
        }
        _sphereColliders[_spherePositions.Length].transform.position = palmPosition;
        _sphereColliders[_spherePositions.Length + 1].transform.position = mockThumbJointPos;*/


        Vector3 sumPoint = Vector3.zero;
        for (int i = 0; i < _spherePositions.Length; i++)
        {
                sumPoint +=  _spherePositions[i];
        }
        sumPoint += palmPosition;
        sumPoint += mockThumbJointPos;
        Vector3 centerPoint = sumPoint / (float)(_spherePositions.Length + 2);

        float maxDistPoint = 0f;
        for (int i = 0; i < _spherePositions.Length; i++)
        {
                maxDistPoint = Math.Max(maxDistPoint, Vector3.Distance(centerPoint, _spherePositions[i]));
        }
        maxDistPoint = Math.Max(maxDistPoint, Vector3.Distance(centerPoint, palmPosition));
        maxDistPoint = Math.Max(maxDistPoint, Vector3.Distance(centerPoint, mockThumbJointPos)); 

        // prepare colliderMatrix

        for(var i = 0; i < SEPARATION_LEVEL; i++)
		{
            _handColliders[i].transform.position = centerPoint;
            SphereCollider sc = _handColliders[i].GetComponent<SphereCollider>();
            sc.radius = maxDistPoint; // + pinHalfWidth;
            sc.radius += i * pinHalfWidth;// sc.radius * (i * 0.5f);
        }
            // drawing hand collider
            /*MaterialPropertyBlock block10 = new MaterialPropertyBlock();
            Matrix4x4[] mat10 = new Matrix4x4[] { Matrix4x4.identity };
            Vector4[] col10 = new Vector4[] { _bodyColor };
            block10.SetVectorArray("_Color", col10);
            Graphics.DrawMeshInstanced(_sphereMesh, 0, _sphereMaterial, _colliderMatrices, _curColliderIndex, block10,
                _castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off, true, gameObject.layer);
            */
            /* Bounds b = new Bounds(new Vector3(0, 0, 0), extends);
             BoxCollider bc = _handCollider.GetComponent<BoxCollider>();
             bc.size = b.size;*/



            //If we want to show the arm, do the calculations and display the meshes
            if (_showArm)
            {
                var arm = _hand.Arm;

                Vector3 right = arm.Basis.xBasis.ToVector3() * arm.Width * 0.7f * 0.5f;
                Vector3 wrist = arm.WristPosition.ToVector3();
                Vector3 elbow = arm.ElbowPosition.ToVector3();

                float armLength = Vector3.Distance(wrist, elbow);
                wrist -= arm.Direction.ToVector3() * armLength * 0.05f;

                Vector3 armFrontRight = wrist + right;
                Vector3 armFrontLeft = wrist - right;
                Vector3 armBackRight = elbow + right;
                Vector3 armBackLeft = elbow - right;

                drawSphere(armFrontRight);
                drawSphere(armFrontLeft);
                drawSphere(armBackLeft);
                drawSphere(armBackRight);

                drawCylinder(armFrontLeft, armFrontRight);
                configureFingerColliderAt(_currFingerColliderIndex++, armFrontLeft, armFrontRight, SEPARATION_LAYER);
                drawCylinder(armBackLeft, armBackRight);
                configureFingerColliderAt(_currFingerColliderIndex++, armBackLeft, armBackRight, SEPARATION_LAYER);
                drawCylinder(armFrontLeft, armBackLeft);
                configureFingerColliderAt(_currFingerColliderIndex++, armFrontLeft, armBackLeft, SEPARATION_LAYER);
                drawCylinder(armFrontRight, armBackRight);
                configureFingerColliderAt(_currFingerColliderIndex++, armFrontRight, armBackRight, SEPARATION_LAYER);

			    // Join arm to wirst (MD)
			    if (_hand.IsLeft)
			    {
                    drawCylinder(armFrontLeft, mockThumbJointPos);
                    configureFingerColliderAt(_currFingerColliderIndex++, armFrontLeft, mockThumbJointPos, SEPARATION_LAYER);
                    drawCylinder(armFrontRight, _spherePositions[THUMB_BASE_INDEX]);
                    configureFingerColliderAt(_currFingerColliderIndex++, armFrontRight, _spherePositions[THUMB_BASE_INDEX], SEPARATION_LAYER);
			    } else
			    {
                    drawCylinder(armFrontLeft, _spherePositions[THUMB_BASE_INDEX]);
                    configureFingerColliderAt(_currFingerColliderIndex++, armFrontLeft, _spherePositions[THUMB_BASE_INDEX], SEPARATION_LAYER);
                    drawCylinder(armFrontRight, mockThumbJointPos);
                    configureFingerColliderAt(_currFingerColliderIndex++, armFrontRight, mockThumbJointPos, SEPARATION_LAYER);
			    }

                // Fill Gap within arm (MD)
                _quadMesh = getQuadMesh(armBackLeft, armBackRight, armFrontRight, armFrontLeft); 
                configureFillColliderAt(_currFillColliderIndex++, _quadMesh);
                Matrix4x4[] mat1 = new Matrix4x4[] { Matrix4x4.identity };
                  Vector4[] col1 = new Vector4[] { _bodyColor };
                MaterialPropertyBlock block1 = new MaterialPropertyBlock();
                block1.SetVectorArray("_Color", col1);
                Graphics.DrawMeshInstanced(_quadMesh, 0, _quadMaterial, mat1, mat1.Length, block1,
                _castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off, true, gameObject.layer);

                // Fill Gap between hand and arm (MD)
                _quadMesh = (_hand.IsLeft) ? getQuadMesh(armFrontLeft, armFrontRight,  _spherePositions[THUMB_BASE_INDEX], mockThumbJointPos): getQuadMesh(armFrontLeft, armFrontRight, mockThumbJointPos,  _spherePositions[THUMB_BASE_INDEX]);
                configureFillColliderAt(_currFillColliderIndex++, _quadMesh); 
                Matrix4x4[] mat2 = new Matrix4x4[] { Matrix4x4.identity };
                Vector4[] col2 = new Vector4[] { _bodyColor };
                MaterialPropertyBlock block2 = new MaterialPropertyBlock();
                block2.SetVectorArray("_Color", col2);
                Graphics.DrawMeshInstanced(_quadMesh, 0, _quadMaterial, mat2, mat2.Length, block2,
                _castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off, true, gameObject.layer);


                Vector3 centered = (armFrontLeft + armFrontRight + armBackLeft + armBackRight) / 4f;
                Vector3 dirForward = Vector3.Normalize(armFrontLeft - armBackLeft);
                Vector3 dirUpward = Vector3.Normalize(armFrontRight - armFrontLeft);
                Quaternion rotationForearm = Quaternion.LookRotation(dirForward, dirUpward);
                var offsetExtends = ExtractScaleFromMatrix(ref _sphereMatrices[0]);
                Vector3 extended = new Vector3(offsetExtends.x * 2.0f, Vector3.Distance(armBackLeft, armBackRight) +
                    offsetExtends.x * 2.0f, Vector3.Distance(armBackLeft, armFrontLeft) +
                    offsetExtends.x * 2.0f);
                Bounds bounds = new Bounds(new Vector3(0, 0, 0), extended);

                for (var i = 0; i < SEPARATION_LEVEL; i++)
                {
                    _forearmColliders[i].transform.position = centered;
                    _forearmColliders[i].transform.rotation = rotationForearm;
                    CapsuleCollider capsuleCollider = _forearmColliders[i].GetComponent<CapsuleCollider>();
                    capsuleCollider.radius = (Vector3.Distance(armFrontRight, armFrontLeft) + offsetExtends.x) / 2.0f + forearmColliderOffset / 4.0f;
                    capsuleCollider.radius += capsuleCollider.radius * (i * 0.5f);
                    capsuleCollider.height = Vector3.Distance(armBackLeft, armFrontLeft) + offsetExtends.x + forearmColliderOffset;
                    capsuleCollider.direction = 2;
                }

            }


       //Draw cylinders between finger joints
       for (int i = 0; i < 5; i++) {
        for (int j = 0; j < 3; j++) {
            int keyA = getFingerJointIndex(i, j);
            int keyB = getFingerJointIndex(i, j + 1);

            Vector3 posA = _spherePositions[keyA];
            Vector3 posB = _spherePositions[keyB];

            drawCylinder(posA, posB);
            if(j < 2) configureFingerColliderAt(_currFingerColliderIndex++, posA, posB, SEPARATION_LAYER); // end joint of finger
            else configureFingerColliderAt(_currFingerColliderIndex++, posA, posB, SEPARATION_LAYER); // other joints
                    // Configure capsule collider
                    
        }
      }

      //Draw cylinders between finger knuckles
      for (int i = 0; i < 4; i++) {
        int keyA = getFingerJointIndex(i, 0);
        int keyB = getFingerJointIndex(i + 1, 0);

        Vector3 posA = _spherePositions[keyA];
        Vector3 posB = _spherePositions[keyB];

        drawCylinder(posA, posB);
        configureFingerColliderAt(_currFingerColliderIndex++, posA, posB, SEPARATION_LAYER);
      }

      //Draw the rest of the hand
      drawCylinder(mockThumbJointPos, THUMB_BASE_INDEX);
      configureFingerColliderAt(_currFingerColliderIndex++, mockThumbJointPos, _spherePositions[THUMB_BASE_INDEX], SEPARATION_LAYER);
      drawCylinder(mockThumbJointPos, PINKY_BASE_INDEX);
      configureFingerColliderAt(_currFingerColliderIndex++, mockThumbJointPos, _spherePositions[PINKY_BASE_INDEX], SEPARATION_LAYER);

      // Fill Arm with Quad (MD)
      _quadMesh = (_hand.IsLeft) ? getQuadMesh(mockThumbJointPos, _spherePositions[THUMB_BASE_INDEX], _spherePositions[INDEX_BASE_INDEX], _spherePositions[PINKY_BASE_INDEX]): getQuadMesh(_spherePositions[THUMB_BASE_INDEX], mockThumbJointPos, _spherePositions[PINKY_BASE_INDEX],  _spherePositions[INDEX_BASE_INDEX]);
      configureFillColliderAt(_currFillColliderIndex++, _quadMesh);

      MaterialPropertyBlock block = new MaterialPropertyBlock();
      Matrix4x4[] mat = new Matrix4x4[] { Matrix4x4.identity };
      Vector4[] col = new Vector4[] { _bodyColor };
      block.SetVectorArray("_Color", col);

      
      Graphics.DrawMeshInstanced(_quadMesh, 0, _quadMaterial, mat, mat.Length, block,
      _castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off, true, gameObject.layer);

      Graphics.DrawMeshInstanced(_sphereMesh, 0, _sphereMaterial, _sphereMatrices, _curSphereIndex, block, 
           _castShadows?UnityEngine.Rendering.ShadowCastingMode.On: UnityEngine.Rendering.ShadowCastingMode.Off, true, gameObject.layer);
      
      Graphics.DrawMeshInstanced(_cylinderMesh, 0, _sphereMaterial, _cylinderMatrices, _curCylinderIndex, block, 
           _castShadows?UnityEngine.Rendering.ShadowCastingMode.On: UnityEngine.Rendering.ShadowCastingMode.Off, true, gameObject.layer);
      
     }

    private void drawSphere(Vector3 position) {
      drawSphere(position, _jointRadius);
    }

    private void drawSphere(Vector3 position, float radius) {
      if (isNaN(position)) { return; }

      //multiply radius by 2 because the default unity sphere has a radius of 0.5 meters at scale 1.
      _sphereMatrices[_curSphereIndex++] = Matrix4x4.TRS(position, 
        Quaternion.identity, Vector3.one * radius * 2.0f * transform.lossyScale.x);
    }

    private void drawCylinder(Vector3 a, Vector3 b) {
      if (isNaN(a) || isNaN(b)) { return; }
      
      float length = (a - b).magnitude / 2f;
        
      Vector3 cylinderCenter = a + (b - a) / 2f;
      float cylinderWidth = _cylinderRadius * 2f * transform.lossyScale.x;
      Quaternion cylinderRotation = Quaternion.LookRotation(b - a);
      cylinderRotation *= Quaternion.AngleAxis(90, Vector3.right);

      if ((a - b).magnitude > 0.001f) {
        _cylinderMatrices[_curCylinderIndex++] = Matrix4x4.TRS(cylinderCenter,
          cylinderRotation, new Vector3(cylinderWidth, length, cylinderWidth));
      }
    }


    private Mesh getQuadMesh(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        Mesh mesh = new Mesh();
        mesh.name = "GeneratedQuad";
        mesh.hideFlags = HideFlags.DontSave;

        Vector3 normal = Vector3.Cross(b - a, c - a).normalized * _cylinderRadius * transform.lossyScale.x;
        Vector3[] vertices = new Vector3[8]
        {

                a-normal, //new Vector3(0, 0, 0),
				b-normal,//new Vector3(width, 0, 0),
				c-normal,//new Vector3(0, height, 0),
				d-normal,//new Vector3(width, height, 0)
				d+normal,//new Vector3(width, height, 0)
				c+normal,//new Vector3(0, height, 0),
				b+normal,//new Vector3(width, 0, 0),
                a+normal //new Vector3(0, 0, 0),
        };
        mesh.vertices = vertices;

        int[] tris = new int[36]
        {
            0, 2, 1, //face front
	        0, 3, 2,
            2, 3, 4, //face top
	        2, 4, 5,
            1, 2, 5, //face right
	        1, 5, 6,
            0, 7, 4, //face left
	        0, 4, 3,
            5, 4, 7, //face back
	        5, 7, 6,
            0, 6, 7, //face bottom
	        0, 1, 6
        };
        mesh.triangles = tris;
        Vector2[] uv = new Vector2[8]
        {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
        };
        mesh.uv = uv;

        mesh.Optimize();
        mesh.RecalculateNormals();
        return mesh;
    }

    private bool isNaN(Vector3 v) {
      return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z);
    }

    private void drawCylinder(int a, int b) {
      drawCylinder(_spherePositions[a], _spherePositions[b]);
    }

    private void drawCylinder(Vector3 a, int b) {
      drawCylinder(a, _spherePositions[b]);
    }

    private int getFingerJointIndex(int fingerIndex, int jointIndex) {
      return fingerIndex * 4 + jointIndex;
    }
  }
}
