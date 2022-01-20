﻿using Leap;
using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.XRTools.Utils;

public class SafeGuard : MonoBehaviour
{

	Projector projector;

	public ExpanDialSticks pins;

	private const float bodyOutlineWidth = 0.4f/100f;
	private const float bodySecondOutlineWidth = 0.8f / 100f;
	private const float pinOutlineWidth = 0.04f / 100f;
	private const float pinSecondOutlineWidth = 0.08f / 100f;


	public Mesh _pinMesh;
	public Material _pinMat;
	private Matrix4x4[] _pinMatrices;
	private Vector4[] _pinColors, _pinOutlineColors, _pinSecondOutlineColors;
	private float[] _pinOutlineWidths, _pinSecondOutlineWidths;
	private int _pinIndex = 0;

	public Mesh _handMesh;
	public Material _handMat;
	private Matrix4x4[] _handMatrices;
	private Vector4[] _handColors, _handOutlineColors, _handSecondOutlineColors;
	private float[] _handOutlineWidths, _handSecondOutlineWidths;
	private int _handIndex = 0;

	public Mesh _armMesh;
	public Material _armMat;
	private Matrix4x4[] _armMatrices;
	private Vector4[] _armColors, _armOutlineColors, _armSecondOutlineColors;
	private float[] _armOutlineWidths, _armSecondOutlineWidths;
	private int _armIndex = 0;


	private const int textureSize = 512;

	private Texture2D[] _shapeTextures;
	private Texture2D _emptyTexture;
	private Texture2D[] _noTextures;
	private Texture2DArray _noTextureArray;
	private float[] _noTextureIndexes;
	private Texture2D[] _iconTextures;
	private Texture2DArray _iconTextureArray;
	private float[] _iconTextureIndexes;


	public Mesh _planeMesh;
	public Material _planeMat;
	private Matrix4x4[] _planeMatrices;
	private Vector4[] _planeColors, _planeOutlineColors, _planeSecondOutlineColors;
	private float[] _planeOutlineWidths, _planeSecondOutlineWidths;
	private Vector4[] _planeLeftHandCenters, _planeRightHandCenters;
	private float[] _planeLeftHandRadius, _planeRightHandRadius;
	private Vector4[] _planeLeftBackArmCenters, _planeRightBackArmCenters;
	private Vector4[] _planeLeftFrontArmCenters, _planeRightFrontArmCenters;
	private float[] _planeLeftArmRadius, _planeRightArmRadius;
	private int _planeIndex = 0;


	public Mesh _dotMesh;
	public Material _dotMat;
	private Matrix4x4[] _dotMatrices;
	private Vector4[] _dotColors, _dotOutlineColors, _dotSecondOutlineColors, _dotThirdOutlineColors, _dotFourthOutlineColors, _dotFifthOutlineColors;
	private float[] _dotOutlineWidths, _dotSecondOutlineWidths, _dotThirdOutlineWidths, _dotFourthOutlineWidths, _dotFifthOutlineWidths;
	private Vector4[] _dotLeftHandCenters, _dotRightHandCenters;
	private float[] _dotLeftHandRadius, _dotRightHandRadius;
	private Vector4[] _dotLeftBackArmCenters, _dotRightBackArmCenters;
	private Vector4[] _dotLeftFrontArmCenters, _dotRightFrontArmCenters;
	private float[] _dotLeftArmRadius, _dotRightArmRadius;
	private int _dotIndex = 0;



	private float minOrthographicSize = 0f; // -1.5f / 2f;
	private float maxOrthographicSize = 0f; // 3.3f / 3.3f;
	private float minOutlineWidth = 0f;
	private float maxOutlineWidth = 0f;
	private float minSecondOutlineWidth = 0f;
	private float maxSecondOutlineWidth = 0f;
	private float minThirdOutlineWidth = 0f; 
	private float maxThirdOutlineWidth = 0f;
	private float minFourthOutlineWidth = 0f;
	private float maxFourthOutlineWidth = 0f;
	private float minFifthOutlineWidth = 0f;
	private float maxFifthOutlineWidth = 0f;


	public Mesh _lineMesh;
	public Material _lineMat;
	private Matrix4x4[] _lineMatrices;
	private Vector4[] _lineColors, _lineOutlineColors, _lineSecondOutlineColors;
	private float[] _lineOutlineWidths, _lineSecondOutlineWidths;
	private Vector4[] _lineLeftHandCenters, _lineRightHandCenters;
	private float[] _lineLeftHandRadius, _lineRightHandRadius;
	private Vector4[] _lineLeftBackArmCenters, _lineRightBackArmCenters;
	private Vector4[] _lineLeftFrontArmCenters, _lineRightFrontArmCenters;
	private float[] _lineLeftArmRadius, _lineRightArmRadius;
	private int _lineIndex = 0;

	public enum SafetyOverlayMode {User, System};
	private SafetyOverlayMode overlayMode = SafetyOverlayMode.User;
	public enum SemioticMode { None, Index, Symbol, Icon};
	private SemioticMode semioticMode = SemioticMode.Icon;
	public enum FeedbackMode { None, State, Intent};
	private FeedbackMode feedbackMode = FeedbackMode.State;


	private const int SEPARATION_LAYER = 10; // Safety Level 0

	private Color _leftDivergingColor = Color.white;
	private Color _middleDivergingColor = Color.white;
	private Color _rightDivergingColor = Color.white;
	public static bool freeze = false;
	private bool frozen = false;
	private bool toDraw = false;
	private GameObject currLeftHandCollider = null;
	private GameObject currLeftArmCollider = null;
	private GameObject prevLeftHandCollider = null;
	private GameObject prevLeftArmCollider = null;
	private GameObject currRightHandCollider = null;
	private GameObject currRightArmCollider = null;
	private GameObject prevRightHandCollider = null;
	private GameObject prevRightArmCollider = null;
	private List<Vector3> currUnsafeTransitions = null;
	private List<Vector3> nextUnsafeTransitions = null;
	private float bodyGamma = 0f;

	// Get current Unsafes Pins

	public const float feedbackInDuration = 0.250f;
	public const float feedbackOutDuration = 0.250f;
	public const float feedbackMinGamma = 0f;
	public const float feedbackMaxGamma = 1f;

	public const float recoveryRateIn = (feedbackMaxGamma - feedbackMinGamma) / feedbackInDuration;
	public const float recoveryRateOut = (feedbackMaxGamma - feedbackMinGamma) / feedbackOutDuration;

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

	public void setOverlayMode(SafetyOverlayMode overlayMode, SemioticMode semioticMode, FeedbackMode feedbackMode)
	{
		this.overlayMode = overlayMode;
		this.semioticMode = semioticMode;
		this.feedbackMode = feedbackMode;
		switch (this.feedbackMode)
		{
			case FeedbackMode.None:
				break;
			case FeedbackMode.State:
				switch (this.semioticMode)
				{
					case SemioticMode.Icon:
						for (int i = 0; i < _shapeTextures.Length; i++)
						{
							_shapeTextures[i] = Resources.Load<Texture2D>("ipause-white");
						}
						_shapeTextures[15] = Resources.Load<Texture2D>("ipause-white"); // put "white" for neutral between -15 and 15
						break;
					case SemioticMode.Symbol:
						for (int i = 0; i < _shapeTextures.Length; i++)
						{
							_shapeTextures[i] = Resources.Load<Texture2D>("spause");
						}
						_shapeTextures[15] = Resources.Load<Texture2D>("spause");  // put "white" for neutral between -15 and 15
						break;
					case SemioticMode.Index:
						for (int i = 0; i < _shapeTextures.Length; i++)
						{
							_shapeTextures[i] = Resources.Load<Texture2D>("white");
						}
						break;
					case SemioticMode.None:
						for (int i = 0; i < _shapeTextures.Length; i++)
						{
							_shapeTextures[i] = Resources.Load<Texture2D>("default");
						}
						break;
					default:
						break;
				}
				break;

			case FeedbackMode.Intent:
				switch (this.semioticMode)
				{
					case SemioticMode.Icon:
						for (int i = 0; i < 15; i++)
						{
							_shapeTextures[i] = Resources.Load<Texture2D>("down" + (15 - i));
							_shapeTextures[i + 16] = Resources.Load<Texture2D>("up" + (i + 1));
						}
						_shapeTextures[15] = Resources.Load<Texture2D>("white");
						break;
					case SemioticMode.Symbol:
						for (int i = 0; i < 15; i++)
						{
							_shapeTextures[i] = Resources.Load<Texture2D>("sdown" + (15 - i));
							_shapeTextures[i + 16] = Resources.Load<Texture2D>("sup" + (i + 1));
						}
						_shapeTextures[15] = Resources.Load<Texture2D>("white");
						break;
					case SemioticMode.Index:
						for (int i = 0; i < _shapeTextures.Length; i++)
						{
							_shapeTextures[i] = Resources.Load<Texture2D>("white");
						}
						break;
					case SemioticMode.None:
						for (int i = 0; i < _shapeTextures.Length; i++)
						{
							_shapeTextures[i] = Resources.Load<Texture2D>("default");
						}
						break;
					default:
						break;
				}
				break;
		}

	}
	IEnumerator ResetProjectorSafeGuard()
	{
		this.projector.enabled = false;
		yield return 0;
		this.projector.enabled = true;
	}

	private void HandleConnected(object sender, MqttConnectionEventArgs e)
	{
		StartCoroutine(ResetProjectorSafeGuard());

	}

	// Start is called before the first frame update
	void Start()
	{
		// Configure transform
		this.transform.position = new Vector3(((pins.NbRows - 1) * (pins.diameter + pins.offset)) / 2f + (pins.diameter - pins.BorderOffset) / 2f, pins.DistanceFromSARCamera, ((pins.NbColumns - 1) * (pins.diameter + pins.offset)) / 2f);
		Vector3 safeCameraLookAtPosition = this.transform.position - new Vector3(0f, pins.DistanceFromSARCamera, 0f);
		this.transform.LookAt(safeCameraLookAtPosition);
		this.transform.eulerAngles += new Vector3(0f, 90f, 0f);

		// Configure camera
		Camera cam = this.GetComponent<Camera>();
		cam.orthographicSize = (pins.NbRows * (pins.diameter + pins.offset) + 2 * pins.offset + 2 * pins.BorderOffset + (pins.diameter - pins.BorderOffset)) / 2f;
		
		// Configure projector
		projector = this.GetComponent<Projector>();
		projector.orthographicSize = cam.orthographicSize;
		projector.material.color = new Color(1f, 1f, 1f, 1f);

		currUnsafeTransitions = new List<Vector3>();
		nextUnsafeTransitions = new List<Vector3>();
		//projector.material.renderQueue = 1000;
		_pinMatrices = new Matrix4x4[32];
		_pinColors = new Vector4[32];
		_pinOutlineColors = new Vector4[32];
		_pinOutlineWidths = new float[32];
		_pinSecondOutlineColors = new Vector4[32];
		_pinSecondOutlineWidths = new float[32];

		_handMatrices = new Matrix4x4[32];
		_handColors = new Vector4[32];
		_handOutlineColors = new Vector4[32];
		_handOutlineWidths = new float[32];
		_handSecondOutlineColors = new Vector4[32];
		_handSecondOutlineWidths = new float[32];

		_armMatrices = new Matrix4x4[32];
		_armColors = new Vector4[32];
		_armOutlineColors = new Vector4[32];
		_armOutlineWidths = new float[32];
		_armSecondOutlineColors = new Vector4[32];
		_armSecondOutlineWidths = new float[32];

		// load used textures
		_emptyTexture = Resources.Load<Texture2D>("white");
		_noTextures = new Texture2D[32];
		_noTextureIndexes = new float[32];
		_noTextureArray = new Texture2DArray(textureSize, textureSize, _noTextures.Length, TextureFormat.DXT5Crunched, false);


		for (int i = 0; i < _noTextures.Length; i++)
		{
			_noTextures[i] = _emptyTexture;
			_noTextureIndexes[i] = i;
			Graphics.CopyTexture(_noTextures[i], 0, 0, _noTextureArray, i, 0); // i is the index of the texture
		}
		_dotMat.SetTexture("_Textures", _noTextureArray);
		_lineMat.SetTexture("_Textures", _noTextureArray);

		_iconTextures = new Texture2D[32];
		_iconTextureIndexes = new float[32];
		_iconTextureArray = new Texture2DArray(textureSize, textureSize, _iconTextures.Length, TextureFormat.DXT5Crunched, false);

		_shapeTextures = new Texture2D[31];

		setOverlayMode(overlayMode, semioticMode, feedbackMode);
		/* 
		Resources.Load<Texture2D>(projectorTexture);

			Texture2DArray textureArray = new Texture2DArray(textureWidth, textureHeight, textures.Length, TextureFormat.RGBA32, false);

			for (int i = 0; i < textures.Length; i++)
			{
				Graphics.CopyTexture(textures[i], 0, 0, textureArray, i, 0); // i is the index of the texture
			}
		*/
		_planeMatrices = new Matrix4x4[32];
		_planeColors = new Vector4[32];
		_planeOutlineColors = new Vector4[32];
		_planeOutlineWidths = new float[32];
		_planeSecondOutlineColors = new Vector4[32];
		_planeSecondOutlineWidths = new float[32];
		_planeLeftHandCenters = new Vector4[32];
		_planeRightHandCenters = new Vector4[32];
		_planeLeftHandRadius = new float[32];
		_planeRightHandRadius = new float[32];
		_planeLeftBackArmCenters = new Vector4[32];
		_planeRightBackArmCenters = new Vector4[32];
		_planeLeftFrontArmCenters = new Vector4[32];
		_planeRightFrontArmCenters = new Vector4[32];
		_planeLeftArmRadius = new float[32];
		_planeRightArmRadius = new float[32];

		_dotMatrices = new Matrix4x4[32];
		_dotColors = new Vector4[32];
		_dotOutlineColors = new Vector4[32];
		_dotOutlineWidths = new float[32];
		_dotSecondOutlineColors = new Vector4[32];
		_dotSecondOutlineWidths = new float[32];
		_dotThirdOutlineColors = new Vector4[32];
		_dotThirdOutlineWidths = new float[32];
		_dotFourthOutlineColors = new Vector4[32];
		_dotFourthOutlineWidths = new float[32];
		_dotFifthOutlineColors = new Vector4[32];
		_dotFifthOutlineWidths = new float[32];
		_dotLeftHandCenters = new Vector4[32];
		_dotRightHandCenters = new Vector4[32];
		_dotLeftHandRadius = new float[32];
		_dotRightHandRadius = new float[32];
		_dotLeftBackArmCenters = new Vector4[32]; 
		_dotRightBackArmCenters = new Vector4[32];
		_dotLeftFrontArmCenters = new Vector4[32];
		_dotRightFrontArmCenters = new Vector4[32];
		_dotLeftArmRadius = new float[32];
		_dotRightArmRadius = new float[32];


		_lineMatrices = new Matrix4x4[32];
		_lineColors = new Vector4[32];
		_lineOutlineColors = new Vector4[32];
		_lineOutlineWidths = new float[32];
		_lineSecondOutlineColors = new Vector4[32];
		_lineSecondOutlineWidths = new float[32];
		_lineLeftHandCenters = new Vector4[32];
		_lineRightHandCenters = new Vector4[32];
		_lineLeftHandRadius = new float[32];
		_lineRightHandRadius = new float[32];
		_lineLeftBackArmCenters = new Vector4[32];
		_lineRightBackArmCenters = new Vector4[32];
		_lineLeftFrontArmCenters = new Vector4[32];
		_lineRightFrontArmCenters = new Vector4[32];
		_lineLeftArmRadius = new float[32];
		_lineRightArmRadius = new float[32];

		ColorUtility.TryParseHtmlString("#384bc1", out _leftDivergingColor);
		ColorUtility.TryParseHtmlString("#ffffff", out _middleDivergingColor);
		ColorUtility.TryParseHtmlString("#b50021", out _rightDivergingColor);

		freeze = frozen = toDraw = false;


		pins.OnConnected += HandleConnected;
	}
	private void Render()
	{
		// draw pins
		MaterialPropertyBlock pinBlock = new MaterialPropertyBlock();
		pinBlock.SetVectorArray("_Color", _pinColors);
		pinBlock.SetVectorArray("_OutlineColor", _pinOutlineColors);
		pinBlock.SetFloatArray("_Outline", _pinOutlineWidths);
		pinBlock.SetVectorArray("_SecondOutlineColor", _pinSecondOutlineColors);
		pinBlock.SetFloatArray("_SecondOutline", _pinSecondOutlineWidths);

		Graphics.DrawMeshInstanced(_pinMesh, 0, _armMat, _pinMatrices, _pinIndex, pinBlock, UnityEngine.Rendering.ShadowCastingMode.Off, false, SEPARATION_LAYER);


		CombineInstance[] combine = new CombineInstance[_handIndex + _armIndex];
		for (int i = 0; i < _handIndex; i++)
		{
			combine[i].mesh = _handMesh;
			combine[i].transform = _handMatrices[i];
		}
		for (int i = 0; i < _armIndex; i++)
		{
			combine[_handIndex + i].mesh = _armMesh;
			combine[_handIndex + i].transform = _armMatrices[i];
		}
		Mesh bodyMesh = new Mesh();
		bodyMesh.CombineMeshes(combine);

		MaterialPropertyBlock bodyBlock = new MaterialPropertyBlock();
		Matrix4x4[] _bodyMatrices = new Matrix4x4[] { Matrix4x4.identity };
		bodyBlock.SetVectorArray("_Color", _armColors);
		bodyBlock.SetVectorArray("_OutlineColor", _armOutlineColors);
		bodyBlock.SetFloatArray("_Outline", _armOutlineWidths);
		bodyBlock.SetVectorArray("_SecondOutlineColor", _armSecondOutlineColors);
		bodyBlock.SetFloatArray("_SecondOutline", _armSecondOutlineWidths);

		Graphics.DrawMeshInstanced(bodyMesh, 0, _armMat, _bodyMatrices, _bodyMatrices.Length, bodyBlock, UnityEngine.Rendering.ShadowCastingMode.Off, false, SEPARATION_LAYER);


		// Draw dots
		MaterialPropertyBlock dotBlock = new MaterialPropertyBlock();
		dotBlock.SetFloatArray("_TextureIndex", _noTextureIndexes);
		dotBlock.SetVectorArray("_Color", _dotColors);
		dotBlock.SetVectorArray("_OutlineColor", _dotOutlineColors);
		dotBlock.SetFloatArray("_Outline", _dotOutlineWidths);
		dotBlock.SetVectorArray("_SecondOutlineColor", _dotSecondOutlineColors);
		dotBlock.SetFloatArray("_SecondOutline", _dotSecondOutlineWidths);
		dotBlock.SetVectorArray("_ThirdOutlineColor", _dotThirdOutlineColors);
		dotBlock.SetFloatArray("_ThirdOutline", _dotThirdOutlineWidths);
		dotBlock.SetVectorArray("_FourthOutlineColor", _dotFourthOutlineColors);
		dotBlock.SetFloatArray("_FourthOutline", _dotFourthOutlineWidths);
		dotBlock.SetVectorArray("_FifthOutlineColor", _dotFifthOutlineColors);
		dotBlock.SetFloatArray("_FifthOutline", _dotFifthOutlineWidths);

		dotBlock.SetVectorArray("_LeftHandCenter", _dotLeftHandCenters);
		dotBlock.SetFloatArray("_LeftHandRadius", _dotLeftHandRadius);
		dotBlock.SetVectorArray("_LeftBackArmCenter", _dotLeftBackArmCenters);
		dotBlock.SetVectorArray("_LeftFrontArmCenter", _dotLeftFrontArmCenters);
		dotBlock.SetFloatArray("_LeftArmRadius", _dotLeftArmRadius);
		dotBlock.SetVectorArray("_RightHandCenter", _dotRightHandCenters);
		dotBlock.SetFloatArray("_RightHandRadius", _dotRightHandRadius);
		dotBlock.SetVectorArray("_RightBackArmCenter", _dotRightBackArmCenters);
		dotBlock.SetVectorArray("_RightFrontArmCenter", _dotRightFrontArmCenters);
		dotBlock.SetFloatArray("_RightArmRadius", _dotRightArmRadius);
		Graphics.DrawMeshInstanced(_dotMesh, 0, _dotMat, _dotMatrices, _dotIndex, dotBlock, UnityEngine.Rendering.ShadowCastingMode.Off, false, SEPARATION_LAYER);

		// Draw lines
		MaterialPropertyBlock lineBlock = new MaterialPropertyBlock();
		lineBlock.SetFloatArray("_TextureIndex", _noTextureIndexes);
		lineBlock.SetVectorArray("_Color", _lineColors);
		lineBlock.SetVectorArray("_OutlineColor", _lineOutlineColors);
		lineBlock.SetFloatArray("_Outline", _lineOutlineWidths);
		lineBlock.SetVectorArray("_SecondOutlineColor", _lineSecondOutlineColors);
		lineBlock.SetFloatArray("_SecondOutline", _lineSecondOutlineWidths);
		lineBlock.SetVectorArray("_LeftHandCenter", _lineLeftHandCenters);
		lineBlock.SetFloatArray("_LeftHandRadius", _lineLeftHandRadius);
		lineBlock.SetVectorArray("_LeftBackArmCenter", _lineLeftBackArmCenters);
		lineBlock.SetVectorArray("_LeftFrontArmCenter", _lineLeftFrontArmCenters);
		lineBlock.SetFloatArray("_LeftArmRadius", _lineLeftArmRadius);
		lineBlock.SetVectorArray("_RightHandCenter", _lineRightHandCenters);
		lineBlock.SetFloatArray("_RightHandRadius", _lineRightHandRadius);
		lineBlock.SetVectorArray("_RightBackArmCenter", _lineRightBackArmCenters);
		lineBlock.SetVectorArray("_RightFrontArmCenter", _lineRightFrontArmCenters);
		lineBlock.SetFloatArray("_RightArmRadius", _lineRightArmRadius);
		Graphics.DrawMeshInstanced(_lineMesh, 0, _lineMat, _lineMatrices, _lineIndex, lineBlock, UnityEngine.Rendering.ShadowCastingMode.Off, false, SEPARATION_LAYER);

		// Draw planes
		_planeMat.SetTexture("_Textures", _iconTextureArray);
		MaterialPropertyBlock planeBlock = new MaterialPropertyBlock();
		planeBlock.SetFloatArray("_TextureIndex", _iconTextureIndexes);
		planeBlock.SetVectorArray("_Color", _planeColors);
		planeBlock.SetVectorArray("_OutlineColor", _planeOutlineColors);
		planeBlock.SetFloatArray("_Outline", _planeOutlineWidths);
		planeBlock.SetVectorArray("_SecondOutlineColor", _planeSecondOutlineColors);
		planeBlock.SetFloatArray("_SecondOutline", _planeSecondOutlineWidths);
		planeBlock.SetVectorArray("_LeftHandCenter", _planeLeftHandCenters);
		planeBlock.SetFloatArray("_LeftHandRadius", _planeLeftHandRadius);
		planeBlock.SetVectorArray("_LeftBackArmCenter", _planeLeftBackArmCenters);
		planeBlock.SetVectorArray("_LeftFrontArmCenter", _planeLeftFrontArmCenters);
		planeBlock.SetFloatArray("_LeftArmRadius", _planeLeftArmRadius);
		planeBlock.SetVectorArray("_RightHandCenter", _planeRightHandCenters);
		planeBlock.SetFloatArray("_RightHandRadius", _planeRightHandRadius);
		planeBlock.SetVectorArray("_RightBackArmCenter", _planeRightBackArmCenters);
		planeBlock.SetVectorArray("_RightFrontArmCenter", _planeRightFrontArmCenters);
		planeBlock.SetFloatArray("_RightArmRadius", _planeRightArmRadius);
		Graphics.DrawMeshInstanced(_planeMesh, 0, _planeMat, _planeMatrices, _planeIndex, planeBlock, UnityEngine.Rendering.ShadowCastingMode.Off, false, SEPARATION_LAYER);

	}

	public void Freeze()
	{
		prevLeftHandCollider = (currLeftHandCollider!=null)?Instantiate(currLeftHandCollider):null;
		prevLeftArmCollider = (currLeftArmCollider != null)?Instantiate(currLeftArmCollider) : null;
		prevRightHandCollider = (currRightHandCollider != null) ? Instantiate(currRightHandCollider) : null;
		prevRightArmCollider = (currRightArmCollider != null) ? Instantiate(currRightArmCollider) : null;
		frozen = true;
	}

	public void UnFreeze()
	{
		Destroy(prevLeftHandCollider);
		Destroy(prevLeftArmCollider);
		Destroy(prevRightHandCollider);
		Destroy(prevRightArmCollider);
		frozen = false;
	}

	private void GenerateSystemOverlay()
	{
		float backgroundDistance = 0f;

		Vector3 leftHandPos, leftBackArmPos, leftFrontArmPos;
		float leftHandRadius, leftArmRadius;
		leftHandPos = leftBackArmPos = leftFrontArmPos = Vector3.zero;
		leftHandRadius = leftArmRadius = 0f;


		// Generate Left Forearm Zone
		if (currLeftHandCollider != null && currLeftArmCollider != null)
		{
			// Left Hand Zone
			SphereCollider sc = currLeftHandCollider.GetComponent<SphereCollider>();
			Vector3 handColliderPosition = currLeftHandCollider.transform.position;
			backgroundDistance = -(sc.radius * 2.0f + pins.height);
			handColliderPosition.y = backgroundDistance;
			Vector3 handColliderScale = new Vector3(sc.radius * 2.0f, sc.radius * 2.0f, sc.radius * 2.0f);

			// Save for shader
			leftHandPos = handColliderPosition;
			leftHandRadius = sc.radius;

			// Left Arm Zone
			CapsuleCollider capsuleCollider1 = currLeftArmCollider.GetComponent<CapsuleCollider>();
			Vector3 forwardArmColliderPosition = currLeftArmCollider.transform.position + currLeftArmCollider.transform.forward * (capsuleCollider1.height / 2.0f);
			Vector3 backwardArmColliderPosition = currLeftArmCollider.transform.position - currLeftArmCollider.transform.forward * (capsuleCollider1.height / 2.0f);
			forwardArmColliderPosition.y = backgroundDistance;
			backwardArmColliderPosition.y = backgroundDistance;
			// Save for shader
			Vector3 frontToBackArm = Vector3.Normalize(backwardArmColliderPosition - forwardArmColliderPosition);
			float distFrontArmOutOfHand = sc.radius - Vector3.Magnitude(handColliderPosition - forwardArmColliderPosition);
			leftFrontArmPos = forwardArmColliderPosition + frontToBackArm * distFrontArmOutOfHand;
			leftBackArmPos = backwardArmColliderPosition;
			leftArmRadius = capsuleCollider1.radius;
		}

		Vector3 rightHandPos, rightBackArmPos, rightFrontArmPos;
		float rightHandRadius, rightArmRadius;
		rightHandPos = rightBackArmPos = rightFrontArmPos = Vector3.zero;
		rightHandRadius = rightArmRadius = 0f;

		// Generate Right Hand Zone
		if (currRightHandCollider != null && currRightArmCollider != null)
		{
			// Right Hand Zone
			// prevRightHandCollider = pins.rightHand.GetHandCollider();
			SphereCollider sc = currRightHandCollider.GetComponent<SphereCollider>();
			Vector3 handColliderPosition = currRightHandCollider.transform.position;
			backgroundDistance = -(sc.radius * 2.0f + pins.height);
			handColliderPosition.y = backgroundDistance;
			Vector3 handColliderScale = new Vector3(sc.radius * 2.0f, sc.radius * 2.0f, sc.radius * 2.0f);

			rightHandPos = handColliderPosition;
			rightHandRadius = sc.radius;

			// Right Arm Zone
			// prevRightArmCollider = pins.rightHand.GetArmCollider();
			CapsuleCollider capsuleCollider1 = currRightArmCollider.GetComponent<CapsuleCollider>();
			Vector3 forwardArmColliderPosition = currRightArmCollider.transform.position + currRightArmCollider.transform.forward * (capsuleCollider1.height / 2.0f);
			Vector3 backwardArmColliderPosition = currRightArmCollider.transform.position - currRightArmCollider.transform.forward * (capsuleCollider1.height / 2.0f);
			forwardArmColliderPosition.y = backgroundDistance;
			backwardArmColliderPosition.y = backgroundDistance;
			// Save for shader
			Vector3 frontToBackArm = Vector3.Normalize(backwardArmColliderPosition - forwardArmColliderPosition);
			float distFrontArmOutOfHand = sc.radius - Vector3.Magnitude(handColliderPosition - forwardArmColliderPosition);
			rightFrontArmPos = forwardArmColliderPosition + frontToBackArm * distFrontArmOutOfHand;
			rightBackArmPos = backwardArmColliderPosition;
			rightArmRadius = capsuleCollider1.radius;
		}

		for(int i = 0; i < currUnsafeTransitions.Count; i++)
		{
			Vector3 unsafeTransition = currUnsafeTransitions[i];
			int row = (int)unsafeTransition.x;
			int column = (int)unsafeTransition.y;
			float gamma = unsafeTransition.z;
			int paused = pins.viewMatrix[row, column].CurrentPaused;
			int feedforwarded = pins.viewMatrix[row, column].CurrentFeedForwarded;
			int displacement = (paused != 0) ? paused : feedforwarded;
			Transform pin = pins.viewMatrix[row, column].transform;

			// Generate dots adjust dot diameter under body
			Vector3 dotPos = pin.position;
			Quaternion dotRot = pin.rotation;
			float distance = pins.viewMatrix[row, column].CurrentDistance;
			float scaleDistanceCoeff = 0f; // 1f - (Mathf.Clamp(distance, minScaleDistance, maxScaleDistance) - minScaleDistance) / (maxScaleDistance - minScaleDistance);
			float dotDiameter = Mathf.Lerp(minOrthographicSize, maxOrthographicSize, scaleDistanceCoeff);


			// Generate Pin in background
			float safetyRadius = (pins.diameter + MyCapsuleHand.SAFETY_RADIUS * 2f) / 2f;
			Vector3 pinPos = new Vector3(dotPos.x, backgroundDistance, dotPos.z);
			Vector3 pinScale = new Vector3(safetyRadius * 2f, safetyRadius * 2f, safetyRadius * 2f);
			_pinMatrices[_pinIndex] = Matrix4x4.TRS(pinPos, Quaternion.identity, pinScale);
			_pinColors[_pinIndex] = new Vector4(1f, 1f, 1f, gamma);
			_pinOutlineColors[_pinIndex] = new Vector4(0f, 0f, 0f, gamma);
			_pinOutlineWidths[_pinIndex] = pinOutlineWidth;
			_pinSecondOutlineColors[_pinIndex] = new Vector4(1f, 1f, 1f, gamma);
			_pinSecondOutlineWidths[_pinIndex] = pinSecondOutlineWidth;
			_pinIndex++;


			Vector3 dotScale = new Vector3(dotDiameter, dotDiameter, dotDiameter);
			_dotMatrices[_dotIndex] = Matrix4x4.TRS(
				dotPos,
				dotRot,
				dotScale
			);

			//Color dotColor = (displacement > 0) ? Color.Lerp(_middleDivergingColor, _rightDivergingColor, displacement / 40f) : Color.Lerp(_middleDivergingColor, _leftDivergingColor, -displacement / 40f);
			Color dotColor = new Color(1f, 0f, 0f, gamma);
			_dotColors[_dotIndex] = dotColor; //(feedbackMode != FeedbackMode.State) ? dotColor : new Color(1f, 1f, 1f, 0f);//Color.white;
			_dotOutlineColors[_dotIndex] = new Vector4(1f, 1f, 1f, gamma);
			_dotOutlineWidths[_dotIndex] = Mathf.Lerp(minOutlineWidth, maxOutlineWidth, scaleDistanceCoeff);
			_dotSecondOutlineColors[_dotIndex] = new Vector4(0f, 0f, 0f, gamma);
			_dotSecondOutlineWidths[_dotIndex] = Mathf.Lerp(minSecondOutlineWidth, maxSecondOutlineWidth, scaleDistanceCoeff);
			_dotThirdOutlineColors[_dotIndex] = dotColor;
			_dotThirdOutlineWidths[_dotIndex] = Mathf.Lerp(minThirdOutlineWidth, maxThirdOutlineWidth, scaleDistanceCoeff);
			_dotFourthOutlineColors[_dotIndex] = new Vector4(0f, 0f, 0f, gamma);
			_dotFourthOutlineWidths[_dotIndex] = Mathf.Lerp(minFourthOutlineWidth, maxFourthOutlineWidth, scaleDistanceCoeff);
			_dotFifthOutlineColors[_dotIndex] = new Vector4(1f, 1f, 1f, gamma);
			_dotFifthOutlineWidths[_dotIndex] = Mathf.Lerp(minFifthOutlineWidth, maxFifthOutlineWidth, scaleDistanceCoeff);

			// left hand mask
			_dotLeftHandCenters[_dotIndex] = dotPos;
			_dotLeftHandRadius[_dotIndex] = safetyRadius;
			// left arm mask
			_dotLeftBackArmCenters[_dotIndex] = dotPos;
			_dotLeftFrontArmCenters[_dotIndex] = dotPos;
			_dotLeftArmRadius[_dotIndex] = safetyRadius;
			// right hand mask
			_dotRightHandCenters[_dotIndex] = dotPos;
			_dotRightHandRadius[_dotIndex] = safetyRadius;
			// right arm mask
			_dotRightBackArmCenters[_dotIndex] = dotPos;
			_dotRightFrontArmCenters[_dotIndex] = dotPos;
			_dotRightArmRadius[_dotIndex] = safetyRadius;
			_dotIndex++;

			// Generate Plane
			Vector3 planePos = pin.position + pin.up * ((dotDiameter - minOrthographicSize + 0.01f) + pins.height / 2.0f);
			Quaternion planeRot = pin.rotation * Quaternion.AngleAxis(90, pin.up);
			Vector3 planeScale = new Vector3(minOrthographicSize + 0.01f, 0.01f, minOrthographicSize + 0.01f);
			_planeMatrices[_planeIndex] = Matrix4x4.TRS(
					planePos,
					planeRot,
					planeScale
			);
			float displacementPercent = Mathf.InverseLerp(-40f, 40f, displacement);
			int directionAmount = Mathf.RoundToInt(Mathf.Lerp(0f, 30f, displacementPercent));
			Graphics.CopyTexture(_shapeTextures[directionAmount], 0, 0, _iconTextureArray, _planeIndex, 0); // i is the index of the texture
			_iconTextureIndexes[_planeIndex] = _planeIndex;
			_planeColors[_planeIndex] = new Color(1f, 1f, 1f, gamma);//dotColor;
			_planeOutlineColors[_planeIndex] = Vector4.zero;
			_planeOutlineWidths[_planeIndex] = 0;
			_planeSecondOutlineColors[_planeIndex] = Vector4.zero;
			_planeSecondOutlineWidths[_planeIndex] = 0;
			// left hand mask
			_planeLeftHandCenters[_planeIndex] = dotPos;
			_planeLeftHandRadius[_planeIndex] = safetyRadius;
			// left arm mask
			_planeLeftBackArmCenters[_planeIndex] = dotPos;
			_planeLeftFrontArmCenters[_planeIndex] = dotPos;
			_planeLeftArmRadius[_planeIndex] = safetyRadius;
			// right hand mask
			_planeRightHandCenters[_planeIndex] = dotPos;
			_planeRightHandRadius[_planeIndex] = safetyRadius;
			// right arm mask
			_planeRightBackArmCenters[_planeIndex] = dotPos;
			_planeRightFrontArmCenters[_planeIndex] = dotPos;
			_planeRightArmRadius[_planeIndex] = safetyRadius;
			_planeIndex++;
		}
	}

	private void GenerateUserOverlay()
	{
		Vector3 leftHandPos, leftBackArmPos, leftFrontArmPos;
		float leftHandRadius, leftArmRadius;
		leftHandPos = leftBackArmPos = leftFrontArmPos = Vector3.zero;
		leftHandRadius = leftArmRadius = 0f;
		float backgroundDistance = 0f;


		// Generate Left Forearm Zone
		if (currLeftHandCollider != null && currLeftArmCollider != null)
		{
			// Left Hand Zone
			SphereCollider sc = currLeftHandCollider.GetComponent<SphereCollider>();
			Vector3 handColliderPosition = currLeftHandCollider.transform.position;
			backgroundDistance = -(sc.radius * 2.0f + pins.height);
			handColliderPosition.y = backgroundDistance;
			Vector3 handColliderScale = new Vector3(sc.radius * 2.0f, sc.radius * 2.0f, sc.radius * 2.0f);

			// Save for shader
			leftHandPos = handColliderPosition;
			leftHandRadius = sc.radius;

			_handMatrices[_handIndex] = Matrix4x4.TRS(handColliderPosition, Quaternion.identity, handColliderScale);
			_handColors[_handIndex] = new Vector4(1f, 1f, 1f, bodyGamma);
			_handOutlineColors[_handIndex] = new Vector4(0f, 0f, 0f, bodyGamma);
			_handOutlineWidths[_handIndex] = bodyOutlineWidth;
			_handSecondOutlineColors[_handIndex] = new Vector4(1f, 1f, 1f, bodyGamma);
			_handSecondOutlineWidths[_handIndex] = bodySecondOutlineWidth;
			_handIndex++;

			// Left Arm Zone
			CapsuleCollider capsuleCollider1 = currLeftArmCollider.GetComponent<CapsuleCollider>();
			Vector3 forwardArmColliderPosition = currLeftArmCollider.transform.position + currLeftArmCollider.transform.forward * (capsuleCollider1.height / 2.0f);
			Vector3 backwardArmColliderPosition = currLeftArmCollider.transform.position - currLeftArmCollider.transform.forward * (capsuleCollider1.height / 2.0f);
			forwardArmColliderPosition.y = backgroundDistance;
			backwardArmColliderPosition.y = backgroundDistance;
			Vector3 armColliderPosition = backwardArmColliderPosition + (forwardArmColliderPosition - backwardArmColliderPosition) / 2.0f;
			Quaternion armColliderRotation = Quaternion.LookRotation(forwardArmColliderPosition - backwardArmColliderPosition) * Quaternion.AngleAxis(90, Vector3.right); ; // Quaternion.Euler(_forearmColliders[0].transform.rotation.eulerAngles.x, _forearmColliders[0].transform.rotation.eulerAngles.y, _forearmColliders[0].transform.rotation.eulerAngles.z);
			Vector3 colliderScale = new Vector3(capsuleCollider1.radius * 2f, capsuleCollider1.height / 2.0f, capsuleCollider1.radius * 2f);

			// Save for shader
			Vector3 frontToBackArm = Vector3.Normalize(backwardArmColliderPosition - forwardArmColliderPosition);
			float distFrontArmOutOfHand = sc.radius - Vector3.Magnitude(handColliderPosition - forwardArmColliderPosition);
			leftFrontArmPos = forwardArmColliderPosition + frontToBackArm * distFrontArmOutOfHand;
			leftBackArmPos = backwardArmColliderPosition;
			leftArmRadius = capsuleCollider1.radius;


			_armMatrices[_armIndex] = Matrix4x4.TRS(
				armColliderPosition,
				armColliderRotation,
				colliderScale
				);
			_armColors[_armIndex] = new Vector4(1f, 1f, 1f, bodyGamma);
			_armOutlineColors[_armIndex] = new Vector4(0f, 0f, 0f, bodyGamma);
			_armOutlineWidths[_armIndex] = bodyOutlineWidth;
			_armSecondOutlineColors[_armIndex] = new Vector4(1f, 1f, 1f, bodyGamma);
			_armSecondOutlineWidths[_armIndex] = bodySecondOutlineWidth;
			_armIndex++;
		}
		Vector3 rightHandPos, rightBackArmPos, rightFrontArmPos;
		float rightHandRadius, rightArmRadius;
		rightHandPos = rightBackArmPos = rightFrontArmPos = Vector3.zero;
		rightHandRadius = rightArmRadius = 0f;
		// Generate Right Hand Zone
		if (currRightHandCollider != null && currRightArmCollider != null)
		{
			// Right Hand Zone
			// prevRightHandCollider = pins.rightHand.GetHandCollider();
			SphereCollider sc = currRightHandCollider.GetComponent<SphereCollider>();
			Vector3 handColliderPosition = currRightHandCollider.transform.position;
			backgroundDistance = -(sc.radius * 2.0f + pins.height);
			handColliderPosition.y = backgroundDistance;
			Vector3 handColliderScale = new Vector3(sc.radius * 2.0f, sc.radius * 2.0f, sc.radius * 2.0f);

			rightHandPos = handColliderPosition;
			rightHandRadius = sc.radius;

			_handMatrices[_handIndex] = Matrix4x4.TRS(handColliderPosition, Quaternion.identity, handColliderScale);
			_handColors[_handIndex] = new Vector4(1f, 1f, 1f, bodyGamma);
			_handOutlineColors[_handIndex] = new Vector4(0f, 0f, 0f, bodyGamma);
			_handOutlineWidths[_handIndex] = bodyOutlineWidth;
			_handSecondOutlineColors[_handIndex] = new Vector4(1f, 1f, 1f, bodyGamma);
			_handSecondOutlineWidths[_handIndex] = 2f / 100f;
			_handIndex++;

			// Right Arm Zone
			// prevRightArmCollider = pins.rightHand.GetArmCollider();
			CapsuleCollider capsuleCollider1 = currRightArmCollider.GetComponent<CapsuleCollider>();
			Vector3 forwardArmColliderPosition = currRightArmCollider.transform.position + currRightArmCollider.transform.forward * (capsuleCollider1.height / 2.0f);
			Vector3 backwardArmColliderPosition = currRightArmCollider.transform.position - currRightArmCollider.transform.forward * (capsuleCollider1.height / 2.0f);
			forwardArmColliderPosition.y = backgroundDistance;
			backwardArmColliderPosition.y = backgroundDistance;
			Vector3 armColliderPosition = backwardArmColliderPosition + (forwardArmColliderPosition - backwardArmColliderPosition) / 2.0f;
			Quaternion armColliderRotation = Quaternion.LookRotation(forwardArmColliderPosition - backwardArmColliderPosition) * Quaternion.AngleAxis(90, Vector3.right); ; // Quaternion.Euler(_forearmColliders[0].transform.rotation.eulerAngles.x, _forearmColliders[0].transform.rotation.eulerAngles.y, _forearmColliders[0].transform.rotation.eulerAngles.z);
			Vector3 colliderScale = new Vector3(capsuleCollider1.radius * 2f, capsuleCollider1.height / 2.0f, capsuleCollider1.radius * 2f);

			// Save for shader
			Vector3 frontToBackArm = Vector3.Normalize(backwardArmColliderPosition - forwardArmColliderPosition);
			float distFrontArmOutOfHand = sc.radius - Vector3.Magnitude(handColliderPosition - forwardArmColliderPosition);
			rightFrontArmPos = forwardArmColliderPosition + frontToBackArm * distFrontArmOutOfHand;
			rightBackArmPos = backwardArmColliderPosition;
			rightArmRadius = capsuleCollider1.radius;

			_armMatrices[_armIndex] = Matrix4x4.TRS(
				armColliderPosition,
				armColliderRotation,
				colliderScale
				);
			_armColors[_armIndex] = new Vector4(1f, 1f, 1f, bodyGamma);
			_armOutlineColors[_armIndex] = new Vector4(0f, 0f, 0f, bodyGamma);
			_armOutlineWidths[_armIndex] = bodyOutlineWidth;
			_armSecondOutlineColors[_armIndex] = new Vector4(1f, 1f, 1f, bodyGamma);
			_armSecondOutlineWidths[_armIndex] = bodySecondOutlineWidth;
			_armIndex++;
		}


		for (int i = 0; i < currUnsafeTransitions.Count; i++)
		{
			Vector3 unsafeTransition = currUnsafeTransitions[i];
			int row = (int)unsafeTransition.x;
			int column = (int)unsafeTransition.y;
			float gamma = unsafeTransition.z;
			int paused = pins.viewMatrix[row, column].CurrentPaused;
			int feedforwarded = pins.viewMatrix[row, column].CurrentFeedForwarded;
			int displacement = (paused != 0) ? paused : feedforwarded;
			Transform pin = pins.viewMatrix[row, column].transform;
			// Generate dots adjust dot diameter under body
			Vector3 dotPos = pin.position;
			Quaternion dotRot = pin.rotation;
			float distance = pins.viewMatrix[row, column].CurrentDistance;
			float scaleDistanceCoeff = 0f; // 1f - (Mathf.Clamp(distance, minScaleDistance, maxScaleDistance) - minScaleDistance) / (maxScaleDistance - minScaleDistance);
			float dotDiameter = Mathf.Lerp(minOrthographicSize, maxOrthographicSize, scaleDistanceCoeff);
			Vector3 dotScale = new Vector3(dotDiameter, dotDiameter, dotDiameter);
			_dotMatrices[_dotIndex] = Matrix4x4.TRS(
				dotPos,
				dotRot,
				dotScale
			);

			//Color dotColor = (displacement > 0) ? Color.Lerp(_middleDivergingColor, _rightDivergingColor, displacement / 40f) : Color.Lerp(_middleDivergingColor, _leftDivergingColor, -displacement / 40f);
			Color dotColor = new Color(1f, 0f, 0f, bodyGamma);
			_dotColors[_dotIndex] = dotColor; //(feedbackMode != FeedbackMode.State) ? dotColor : new Color(1f, 1f, 1f, 0f);//Color.white;
			_dotOutlineColors[_dotIndex] = new Vector4(1f, 1f, 1f, bodyGamma);
			_dotOutlineWidths[_dotIndex] = Mathf.Lerp(minOutlineWidth, maxOutlineWidth, scaleDistanceCoeff);
			_dotSecondOutlineColors[_dotIndex] = new Vector4(0f, 0f, 0f, bodyGamma);
			_dotSecondOutlineWidths[_dotIndex] = Mathf.Lerp(minSecondOutlineWidth, maxSecondOutlineWidth, scaleDistanceCoeff);
			_dotThirdOutlineColors[_dotIndex] = dotColor;
			_dotThirdOutlineWidths[_dotIndex] = Mathf.Lerp(minThirdOutlineWidth, maxThirdOutlineWidth, scaleDistanceCoeff);
			_dotFourthOutlineColors[_dotIndex] = new Vector4(0f, 0f, 0f, bodyGamma);
			_dotFourthOutlineWidths[_dotIndex] = Mathf.Lerp(minFourthOutlineWidth, maxFourthOutlineWidth, scaleDistanceCoeff);
			_dotFifthOutlineColors[_dotIndex] = new Vector4(1f, 1f, 1f, bodyGamma);
			_dotFifthOutlineWidths[_dotIndex] = Mathf.Lerp(minFifthOutlineWidth, maxFifthOutlineWidth, scaleDistanceCoeff);

			// left hand mask
			_dotLeftHandCenters[_dotIndex] = leftHandPos;
			_dotLeftHandRadius[_dotIndex] = leftHandRadius;
			// left arm mask
			_dotLeftBackArmCenters[_dotIndex] = leftBackArmPos;
			_dotLeftFrontArmCenters[_dotIndex] = leftFrontArmPos;
			_dotLeftArmRadius[_dotIndex] = leftArmRadius;
			// right hand mask
			_dotRightHandCenters[_dotIndex] = rightHandPos;
			_dotRightHandRadius[_dotIndex] = rightHandRadius;
			// right arm mask
			_dotRightBackArmCenters[_dotIndex] = rightBackArmPos;
			_dotRightFrontArmCenters[_dotIndex] = rightFrontArmPos;
			_dotRightArmRadius[_dotIndex] = rightArmRadius;
			_dotIndex++;

			// Generate Plane
			Vector3 planePos = pin.position + pin.up * ((dotDiameter - minOrthographicSize + 0.01f) + pins.height / 2.0f);
			Quaternion planeRot = pin.rotation * Quaternion.AngleAxis(90, pin.up);
			Vector3 planeScale = new Vector3(minOrthographicSize + 0.01f, 0.01f, minOrthographicSize + 0.01f);
			_planeMatrices[_planeIndex] = Matrix4x4.TRS(
				 planePos,
				 planeRot,
				 planeScale
			);
			float displacementPercent = Mathf.InverseLerp(-40f, 40f, displacement);
			int directionAmount = Mathf.RoundToInt(Mathf.Lerp(0f, 30f, displacementPercent));
			Graphics.CopyTexture(_shapeTextures[directionAmount], 0, 0, _iconTextureArray, _planeIndex, 0); // i is the index of the texture
			_iconTextureIndexes[_planeIndex] = _planeIndex;
			_planeColors[_planeIndex] = new Color(1f, 1f, 1f, bodyGamma);//dotColor;
			_planeOutlineColors[_planeIndex] = Vector4.zero;
			_planeOutlineWidths[_planeIndex] = 0;
			_planeSecondOutlineColors[_planeIndex] = Vector4.zero;
			_planeSecondOutlineWidths[_planeIndex] = 0;
			// left hand mask
			_planeLeftHandCenters[_planeIndex] = leftHandPos;
			_planeLeftHandRadius[_planeIndex] = leftHandRadius;
			// left arm mask
			_planeLeftBackArmCenters[_planeIndex] = leftBackArmPos;
			_planeLeftFrontArmCenters[_planeIndex] = leftFrontArmPos;
			_planeLeftArmRadius[_planeIndex] = leftArmRadius;
			// right hand mask
			_planeRightHandCenters[_planeIndex] = rightHandPos;
			_planeRightHandRadius[_planeIndex] = rightHandRadius;
			// right arm mask
			_planeRightBackArmCenters[_planeIndex] = rightBackArmPos;
			_planeRightFrontArmCenters[_planeIndex] = rightFrontArmPos;
			_planeRightArmRadius[_planeIndex] = rightArmRadius;
			_planeIndex++;
		}
	}
	// Update is called once per frame
	public void Update()
    {
		_pinIndex = _handIndex = _armIndex = _planeIndex = _dotIndex = _lineIndex = 0;

		// Handle Overlay Modes à priori
		switch (overlayMode)
		{
			case SafetyOverlayMode.User:
				// set outlines width
				minOrthographicSize = pins.diameter - (1f / 100f); // -1.5f / 2f;
				maxOrthographicSize = minOrthographicSize * (3.3f / 100f); // 3.3f / 3.3f;
				minOutlineWidth = 0f;
				maxOutlineWidth = minOutlineWidth * (3.3f / 100f);
				minSecondOutlineWidth = 0f;
				maxSecondOutlineWidth = minSecondOutlineWidth * (3.3f / 100f);
				minThirdOutlineWidth = 0f;
				maxThirdOutlineWidth = minThirdOutlineWidth * (3.3f / 100f);
				minFourthOutlineWidth = 0f;
				maxFourthOutlineWidth = minFourthOutlineWidth * (3.3f / 100f);
				minFifthOutlineWidth = 0f;
				maxFifthOutlineWidth = minFifthOutlineWidth * (3.3f / 100f);
				break;
			case SafetyOverlayMode.System:
				// set outlines width
				minOrthographicSize = pins.diameter - (1f / 100f); // -1.5f / 2f;
				maxOrthographicSize = minOrthographicSize * (3.3f / 100f); // 3.3f / 3.3f;
				minOutlineWidth = 0f;
				maxOutlineWidth = minOutlineWidth * (3.3f / 100f);
				minSecondOutlineWidth = 0f;
				maxSecondOutlineWidth = minSecondOutlineWidth * (3.3f / 100f);
				minThirdOutlineWidth = 0f;
				maxThirdOutlineWidth = minThirdOutlineWidth * (3.3f / 100f);
				minFourthOutlineWidth = 0f;
				maxFourthOutlineWidth = minFourthOutlineWidth * (3.3f / 100f);
				minFifthOutlineWidth = 0f;
				maxFifthOutlineWidth = minFifthOutlineWidth * (3.3f / 100f);
				break;
		}

		//float minScaleDistance = 0f;
		//float maxScaleDistance = minOrthographicSize;



		if (frozen)
		{

			currLeftHandCollider = prevLeftHandCollider;
			currLeftArmCollider = prevLeftArmCollider;
			currRightHandCollider = prevRightHandCollider;
			currRightArmCollider = prevRightArmCollider;
		}
		else
		{
			if (pins.leftHand != null && pins.leftHand.IsActive())
			{

				currLeftHandCollider = pins.leftHand.GetHandCollider();
				currLeftArmCollider =  pins.leftHand.GetArmCollider();
			}
			if (pins.rightHand != null && pins.rightHand.IsActive())
			{

				currRightHandCollider = pins.rightHand.GetHandCollider();
				currRightArmCollider = pins.rightHand.GetArmCollider();
			}
		}
		nextUnsafeTransitions.Clear();
		for (int row = 0; row < pins.NbRows; row++)
		{
			for (int column = 0; column < pins.NbColumns; column++)
			{
				int paused = pins.viewMatrix[row, column].CurrentPaused;
				int feedforwarded = pins.viewMatrix[row, column].CurrentFeedForwarded;
				float proximity = pins.viewMatrix[row, column].CurrentProximity;
				bool reaching = pins.viewMatrix[row, column].CurrentReaching;

				int displacement = (paused != 0) ? paused : feedforwarded;
				if (proximity > 0f && (reaching || displacement != 0))
				{
					Vector3 nextUnsafeTransition = new Vector3(row, column, proximity);
					//Debug.Log("nextUnsafeTransition => " + nextUnsafeTransition);
					nextUnsafeTransitions.Add(nextUnsafeTransition);
					int matchIndex = currUnsafeTransitions.FindIndex(currUnsafeTransition => (currUnsafeTransition.x == nextUnsafeTransition.x && currUnsafeTransition.y == nextUnsafeTransition.y));


					if (matchIndex != -1) // next unsafe already exist then ease-in
					{
						Vector3 currUnsafeTransition = currUnsafeTransitions[matchIndex];
						currUnsafeTransition.z = Mathf.MoveTowards(currUnsafeTransition.z, nextUnsafeTransition.z, recoveryRateIn * Time.deltaTime);
						currUnsafeTransitions[matchIndex] = currUnsafeTransition;
					}
					else // next unsafe does not already exist then ease-in
					{
						Vector3 currUnsafeTransition = new Vector3(row, column, 0f);
						currUnsafeTransition.z = Mathf.MoveTowards(currUnsafeTransition.z, nextUnsafeTransition.z, recoveryRateIn * Time.deltaTime);
						currUnsafeTransitions.Add(currUnsafeTransition);

					}
				}
			}
		}
		List<int> toRemoveIndexes = new List<int>();
		for(int i = 0; i < currUnsafeTransitions.Count; i++)
		{
			Vector3 currUnsafeTransition = currUnsafeTransitions[i];

			int matchIndex = nextUnsafeTransitions.FindIndex(nextUnsafeTransition => (nextUnsafeTransition.x == currUnsafeTransition.x && nextUnsafeTransition.y == currUnsafeTransition.y));


			if (matchIndex == -1) // curr unsafe does not exist anymore then ease-in
			{
				if(currUnsafeTransition.z > 0f)
				{
					currUnsafeTransition.z = Mathf.MoveTowards(currUnsafeTransition.z, feedbackMinGamma, recoveryRateOut * Time.deltaTime);
					currUnsafeTransitions[i] = currUnsafeTransition;
					//Debug.Log(currUnsafeTransitions[i]);
				} else
				{
					toRemoveIndexes.Add(i);
				}
			}
		}
		for (int i = 0; i < toRemoveIndexes.Count; i++)
		{
			currUnsafeTransitions.RemoveAt(toRemoveIndexes[i]);
		}
		if(nextUnsafeTransitions.Count == 0) bodyGamma = Mathf.MoveTowards(bodyGamma, feedbackMinGamma, recoveryRateOut * Time.deltaTime);
		else bodyGamma = Mathf.MoveTowards(bodyGamma, feedbackMaxGamma, recoveryRateIn * Time.deltaTime);

		if (currUnsafeTransitions.Count > 0)
		{
			switch (overlayMode)
			{
				case SafetyOverlayMode.User:
					GenerateUserOverlay();
					break;
				case SafetyOverlayMode.System:
					GenerateSystemOverlay();
					break;
			}


			// Handle Overlay Modes à posteriori
			switch (overlayMode)
			{
				case SafetyOverlayMode.User:
					// hide lines and hull
					_lineIndex = 0;
					// hide dots outlines
					for (int i = 0; i < _dotIndex; i++)
					{
						_dotOutlineColors[i] = new Vector4(1f, 1f, 1f, 0f);
						_dotSecondOutlineColors[i] = new Vector4(1f, 1f, 1f, 0f);
						_dotThirdOutlineColors[i] = new Vector4(1f, 1f, 1f, 0f);
					}
					break;
				case SafetyOverlayMode.System:
					// hide lines and hull
					_lineIndex = 0;
					// hide dots outlines
					for (int i = 0; i < _dotIndex; i++)
					{
						_dotOutlineColors[i] = new Vector4(1f, 1f, 1f, 0f);
						_dotSecondOutlineColors[i] = new Vector4(1f, 1f, 1f, 0f);
						_dotThirdOutlineColors[i] = new Vector4(1f, 1f, 1f, 0f);
					}
					break;
			}
			// Render graphics
			Render();
		}

	}
}
