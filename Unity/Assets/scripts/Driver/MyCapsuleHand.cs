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

    private const int TOTAL_JOINT_COUNT = 4 * 5;
    private const float CYLINDER_MESH_RESOLUTION = 0.1f; //in centimeters, meshes within this resolution will be re-used
    private const int THUMB_BASE_INDEX = (int)Finger.FingerType.TYPE_THUMB * 4;
    private const int PINKY_BASE_INDEX = (int)Finger.FingerType.TYPE_PINKY * 4;

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
    private Mesh _sphereMesh;

    private Mesh _cylinderMesh;

    [MinValue(3)]
    [SerializeField]
    private int _cylinderResolution = 12;

    [MinValue(0)]
    [SerializeField]
    private float _jointRadius = 0.008f;

    [MinValue(0)]
    [SerializeField]
    private float _cylinderRadius = 0.006f;

    [MinValue(0)]
    [SerializeField]
    private float _palmRadius = 0.015f;
    #pragma warning restore 0649

    private Material _sphereMat;
    private Hand _hand;
    private Vector3[] _spherePositions;
    private Matrix4x4[] _sphereMatrices   = new Matrix4x4[32], 
                        _cylinderMatrices = new Matrix4x4[32];
    private const int SEPARATION_LEVEL = 3;
    private const int SEPARATION_LAYER = 10; // Safety Level 0
    private GameObject _handObject;
    private GameObject[] _handColliders;
    private GameObject [] _forearmColliders;

    private int _curSphereIndex = 0, _curCylinderIndex = 0;

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

    public override void SetLeapHand(Hand hand) {
      _hand = hand;
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
                    _handColliders[i].gameObject.layer = SEPARATION_LAYER + i;
                    _handColliders[i].GetComponent<SphereCollider>().enabled = true;
                }
            }

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
                _forearmColliders[i].gameObject.layer = SEPARATION_LAYER + i;
                _forearmColliders[i].GetComponent<CapsuleCollider>().enabled = true;
             }
        }
    }
    private void setCollisionMode(bool enabled)
	{
        if (_handColliders != null && _forearmColliders != null)
        {
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
      if (_handColliders == null || _forearmColliders == null) InstantiateGameObjects();

      if (_material != null && (_backing_material == null || !_backing_material.enableInstancing)) {
        _backing_material = new Material(_material);
        _backing_material.hideFlags = HideFlags.DontSaveInEditor;
        if(!Application.isEditor && !_backing_material.enableInstancing) {
          Debug.LogError("Capsule Hand Material needs Instancing Enabled to render in builds!", this);
        }
        _backing_material.enableInstancing = true;
        _sphereMat = new Material(_backing_material);
        _sphereMat.hideFlags = HideFlags.DontSaveInEditor;
      }
    }

    #if UNITY_EDITOR
    private void OnValidate() {
      _meshMap.Clear();
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
      //Debug.Log(handedness + "BeginHand()");
      //ShowGameObjects();
      base.BeginHand();
      if (_hand.IsLeft) {
        _sphereMat.color = _leftColorList[_leftColorIndex];
        _leftColorIndex = (_leftColorIndex + 1) % _leftColorList.Length;
      } else {
        _sphereMat.color = _rightColorList[_rightColorIndex];
        _rightColorIndex = (_rightColorIndex + 1) % _rightColorList.Length;
      }
      setCollisionMode(true);
    }
    public override void FinishHand()
    {
      //Debug.Log(handedness + "FinishHand()");
      //HideGameObjects();
      setCollisionMode(false);
      base.FinishHand();

    }
  public override string ToString(){

    if(_hand == null || (_hand.IsLeft && handedness != Chirality.Left) || (!_hand.IsLeft && handedness != Chirality.Right)) return "NULL";
    string s = "";
    //Get finger position
    for(int f = 0; f < _hand.Fingers.Count; f++) {
      var finger = _hand.Fingers[f];
      s += "F" + f + " ";
      for (int j = 0; j < 4; j++) {
        int key = getFingerJointIndex((int)finger.Type, j);
        Vector3 position = finger.Bone((Bone.BoneType)j).NextJoint.ToVector3();
        s += position.ToString("F3") + " ";
      }
    }
    // Get palm et thumb position
    Vector3 palmPosition = _hand.PalmPosition.ToVector3();
    Vector3 thumbBaseToPalm = _spherePositions[THUMB_BASE_INDEX] - _hand.PalmPosition.ToVector3();
    Vector3 mockThumbJointPos = _hand.PalmPosition.ToVector3() + Vector3.Reflect(thumbBaseToPalm, _hand.Basis.xBasis.ToVector3());
    s += "T " + mockThumbJointPos.ToString("F3") + " ";
    s += "P " + palmPosition.ToString("F3") + " ";
    // Get arm position
    if (_showArm){
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

      s += "AFR " + armFrontRight.ToString("F3") + " ";
      s += "AFL " + armFrontLeft.ToString("F3") + " ";
      s += "ABR " + armBackRight.ToString("F3") + " ";
      s += "ABL " + armBackLeft.ToString("F3") + " ";
    }
    return s;
  }

  public override void UpdateHand() {
      //Debug.Log(handedness + "UpdateHand()");

      if (_handColliders == null || _forearmColliders == null) return; //InstantiateGameObjects();

      _curSphereIndex = 0;
      _curCylinderIndex = 0;

      if (_spherePositions == null || _spherePositions.Length != TOTAL_JOINT_COUNT) {
        _spherePositions = new Vector3[TOTAL_JOINT_COUNT];
      }

      if (_material != null && (_backing_material == null || !_backing_material.enableInstancing)) {
        _backing_material = new Material(_material);
        _backing_material.hideFlags = HideFlags.DontSaveInEditor;
        _backing_material.enableInstancing = true;
        _sphereMat = new Material(_backing_material);
        _sphereMat.hideFlags = HideFlags.DontSaveInEditor;
      }

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
      drawSphere(palmPosition, _palmRadius);

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

        for(var i = 0; i < SEPARATION_LEVEL; i++)
		{
            _handColliders[i].transform.position = centerPoint;
            SphereCollider sc = _handColliders[i].GetComponent<SphereCollider>();
            sc.radius = maxDistPoint;
            sc.radius += sc.radius * (i * 0.5f);
            }
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
            drawCylinder(armBackLeft, armBackRight);
            drawCylinder(armFrontLeft, armBackLeft);
            drawCylinder(armFrontRight, armBackRight);

            Vector3 centered = (armFrontLeft + armFrontRight + armBackLeft + armBackRight) / 4f;
            Vector3 dirForward = Vector3.Normalize(armFrontLeft - armBackLeft);
            Vector3 dirUpward = Vector3.Normalize(armFrontRight - armFrontLeft);
            Quaternion rotationForearm = Quaternion.LookRotation(dirForward, dirUpward);
            var offsetExtends = ExtractScaleFromMatrix(ref _sphereMatrices[0]);
            Vector3 extended = new Vector3(offsetExtends.x * 2.0f, Vector3.Distance(armBackLeft, armBackRight) +
                offsetExtends.x * 2.0f, Vector3.Distance(armBackLeft, armFrontLeft) +
                offsetExtends.x * 2.0f);
            Bounds bounds = new Bounds(new Vector3(0, 0, 0), extended);
            float offset = 12.0f;

            for (var i = 0; i < SEPARATION_LEVEL; i++)
            {
                _forearmColliders[i].transform.position = centered;
                _forearmColliders[i].transform.rotation = rotationForearm;
                CapsuleCollider capsuleCollider = _forearmColliders[i].GetComponent<CapsuleCollider>();
                capsuleCollider.radius = (Vector3.Distance(armFrontRight, armFrontLeft) + offsetExtends.x) / 2.0f + offset / 2.0f;
                capsuleCollider.radius += capsuleCollider.radius * (i * 0.5f);
                capsuleCollider.height = Vector3.Distance(armBackLeft, armFrontLeft) + offsetExtends.x + offset;
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
        }
      }

      //Draw cylinders between finger knuckles
      for (int i = 0; i < 4; i++) {
        int keyA = getFingerJointIndex(i, 0);
        int keyB = getFingerJointIndex(i + 1, 0);

        Vector3 posA = _spherePositions[keyA];
        Vector3 posB = _spherePositions[keyB];

        drawCylinder(posA, posB);
      }

      //Draw the rest of the hand
      drawCylinder(mockThumbJointPos, THUMB_BASE_INDEX);
      drawCylinder(mockThumbJointPos, PINKY_BASE_INDEX);

       MaterialPropertyBlock mpb = new MaterialPropertyBlock();
       Vector4[] colors = new Vector4[_curSphereIndex];
       Vector4[] firstOutlineColors = new Vector4[_curSphereIndex];
       float[] fistOutlineWidths = new float [_curSphereIndex];
       Vector4[] secondOutlineColors = new Vector4[_curSphereIndex];
       float[] secondOutlineWidths = new float[_curSphereIndex];
       float[] angles = new float[_curSphereIndex];
            for (int i = 0; i < _curSphereIndex; i++) {
                colors[i] = Color.black;
                firstOutlineColors[i] = Color.red;
                fistOutlineWidths[i] = 0.5f;
                secondOutlineColors[i] = Color.red;
                secondOutlineWidths[i] = 0.5f;
                angles[i] = 89.0f;
            }

       /*mpb.SetVectorArray("_Color", colors);
       mpb.SetVectorArray("_FirstOutlineColor", firstOutlineColors);
       mpb.SetFloatArray("_FirstOutlineWidth", fistOutlineWidths);
       mpb.SetVectorArray("_FirstOutlineColor", secondOutlineColors);
       mpb.SetFloatArray("_FirstOutlineWidth", secondOutlineWidths);
       mpb.SetFloatArray("_Angle", angles);*/
      /* mpb.SetVector("_Color", Color.black);
       mpb.SetVector("_FirstOutlineColor", Color.red);
       mpb.SetFloat("_FirstOutlineWidth", 0.5f);*/

       //Debug.Log("_sphereMat.shader.name: " + _sphereMat.shader.name);
       //if (mpb.isEmpty) Debug.Log("MPB IS EMPTY!");

       // Draw Spheres
       Graphics.DrawMeshInstanced(_sphereMesh, 0, _sphereMat, _sphereMatrices, _curSphereIndex, null, 
       _castShadows?UnityEngine.Rendering.ShadowCastingMode.On: UnityEngine.Rendering.ShadowCastingMode.Off, true, gameObject.layer);

      // Draw Cylinders
      if(_cylinderMesh == null) { _cylinderMesh = getCylinderMesh(1f); }
      Graphics.DrawMeshInstanced(_cylinderMesh, 0, _backing_material, _cylinderMatrices, _curCylinderIndex, null,
        _castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off, true, gameObject.layer);
       // Draw Quads
      // Mesh quadMesh = getQuadMesh();
      //Graphics.DrawMeshInstanced(_cylinderMesh, 0, _backing_material, _cylinderMatrices, _curCylinderIndex, null,
      //  _castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off, true, gameObject.layer);

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

      float length = (a - b).magnitude;

      if ((a - b).magnitude > 0.001f) {
        _cylinderMatrices[_curCylinderIndex++] = Matrix4x4.TRS(a,
          Quaternion.LookRotation(b - a), new Vector3(transform.lossyScale.x, transform.lossyScale.x, length));
      }
    }

    private Mesh getQuadMesh(Vector3 a, Vector3 b, Vector3 c, Vector3 d) {
        Mesh mesh = new Mesh();
        mesh.name = "GeneratedQuad";
        mesh.hideFlags = HideFlags.DontSave;
        // Points
        Vector3[] verts = new Vector3[4]{a,b,c,d};
        // Triangles
        int[] tris = new int[6]
        {
        // lower left triangle
        0, 2, 1,
        // upper right triangle
        2, 3, 1
        };
       mesh.SetVertices(verts);
       mesh.SetIndices(tris, MeshTopology.Triangles, 0);
       mesh.RecalculateBounds();
       mesh.RecalculateNormals();
       mesh.UploadMeshData(true);
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

    private Dictionary<int, Mesh> _meshMap = new Dictionary<int, Mesh>();
    private Mesh getCylinderMesh(float length) {
      int lengthKey = Mathf.RoundToInt(length * 100 / CYLINDER_MESH_RESOLUTION);

      Mesh mesh;
      if (_meshMap.TryGetValue(lengthKey, out mesh)) {
        return mesh;
      }

      mesh = new Mesh();
      mesh.name = "GeneratedCylinder";
      mesh.hideFlags = HideFlags.DontSave;

      List<Vector3> verts = new List<Vector3>();
      List<Color> colors = new List<Color>();
      List<int> tris = new List<int>();

      Vector3 p0 = Vector3.zero;
      Vector3 p1 = Vector3.forward * length;
      for (int i = 0; i < _cylinderResolution; i++) {
        float angle = (Mathf.PI * 2.0f * i) / _cylinderResolution;
        float dx = _cylinderRadius * Mathf.Cos(angle);
        float dy = _cylinderRadius * Mathf.Sin(angle);

        Vector3 spoke = new Vector3(dx, dy, 0);

        verts.Add(p0 + spoke);
        verts.Add(p1 + spoke);

        colors.Add(Color.white);
        colors.Add(Color.white);

        int triStart = verts.Count;
        int triCap = _cylinderResolution * 2;

        tris.Add((triStart + 0) % triCap);
        tris.Add((triStart + 2) % triCap);
        tris.Add((triStart + 1) % triCap);

        tris.Add((triStart + 2) % triCap);
        tris.Add((triStart + 3) % triCap);
        tris.Add((triStart + 1) % triCap);
      }

      mesh.SetVertices(verts);
      mesh.SetIndices(tris.ToArray(), MeshTopology.Triangles, 0);
      mesh.RecalculateBounds();
      mesh.RecalculateNormals();
      mesh.UploadMeshData(true);

      _meshMap[lengthKey] = mesh;

      return mesh;
    }
  }
}
