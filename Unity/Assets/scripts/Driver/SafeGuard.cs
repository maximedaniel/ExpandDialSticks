using Leap;
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

	private const float bodyOutlineWidth = 0.4f;
	private const float bodySecondOutlineWidth = 0.8f;
	private Color bodyOutlineColor = new Color(0f, 0f, 0f, 1f);

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

	private Mesh _hullMesh;
	private Matrix4x4[] _hullMatrices;
	private Vector4[] _hullColors, _hullOutlineColors, _hullSecondOutlineColors;
	private float[] _hullOutlineWidths, _hullSecondOutlineWidths;
	private Vector4[] _hullLeftHandCenters, _hullRightHandCenters;
	private float[] _hullLeftHandRadius, _hullRightHandRadius;
	private Vector4[] _hullLeftBackArmCenters, _hullRightBackArmCenters;
	private Vector4[] _hullLeftFrontArmCenters, _hullRightFrontArmCenters;
	private float[] _hullLeftArmRadius, _hullRightArmRadius;
	private int _hullIndex = 0;

	public enum SafetyOverlayMode {None, Dot, Surface, Hull, Zone};
	private SafetyOverlayMode overlayMode = SafetyOverlayMode.Dot;
	public enum SemioticMode { None, Index, Symbol, Icon};
	private SemioticMode semioticMode = SemioticMode.None;
	public enum FeedbackMode { None, State, Intent};
	private FeedbackMode feedbackMode = FeedbackMode.State;

	private const int SEPARATION_LAYER = 10; // Safety Level 0

	private Color _leftDivergingColor = Color.white;
	private Color _middleDivergingColor = Color.white;
	private Color _rightDivergingColor = Color.white;
	public static bool freeze = false;
	private bool frozen = false;

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
							_shapeTextures[i] = Resources.Load<Texture2D>("pause");
						}
						_shapeTextures[15] = Resources.Load<Texture2D>("white");
						break;
					case SemioticMode.Symbol:
						for (int i = 0; i < _shapeTextures.Length; i++)
						{
							_shapeTextures[i] = Resources.Load<Texture2D>("spause");
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
		this.transform.position = new Vector3(((pins.NbRows - 1) * (pins.diameter + pins.offset)) / 2f + (pins.diameter - pins.BorderOffset) / 2f, pins.CameraDistanceFromPins, ((pins.NbColumns - 1) * (pins.diameter + pins.offset)) / 2f);
		Vector3 safeCameraLookAtPosition = this.transform.position - new Vector3(0f, pins.CameraDistanceFromPins, 0f);
		this.transform.LookAt(safeCameraLookAtPosition);
		this.transform.eulerAngles += new Vector3(0f, 90f, 0f);

		// Configure camera
		Camera cam = this.GetComponent<Camera>();
		cam.orthographicSize = (pins.NbRows * (pins.diameter + pins.offset) + 2 * pins.offset + 2 * pins.BorderOffset + (pins.diameter - pins.BorderOffset)) / 2f;
		
		// Configure projector
		projector = this.GetComponent<Projector>();
		projector.orthographicSize = cam.orthographicSize;
		//projector.material.renderQueue = 1000;

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

		_hullMatrices = new Matrix4x4[32];
		_hullColors = new Vector4[32];
		_hullOutlineColors = new Vector4[32];
		_hullOutlineWidths = new float[32];
		_hullSecondOutlineColors = new Vector4[32];
		_hullSecondOutlineWidths = new float[32];
		_hullLeftHandCenters = new Vector4[32];
		_hullRightHandCenters = new Vector4[32];
		_hullLeftHandRadius = new float[32];
		_hullRightHandRadius = new float[32];
		_hullLeftBackArmCenters = new Vector4[32];
		_hullRightBackArmCenters = new Vector4[32];
		_hullLeftFrontArmCenters = new Vector4[32];
		_hullRightFrontArmCenters = new Vector4[32];
		_hullLeftArmRadius = new float[32];
		_hullRightArmRadius = new float[32];
		/*
		// Generate color mapping
		Gradient gradient = new Gradient();

		// Populate the color keys at the relative time 0 and 1 (0 and 100%)
		GradientColorKey[] colorKey = new GradientColorKey[11];

		ColorUtility.TryParseHtmlString("#364B9A", out colorKey[0].color);
		colorKey[0].time = 0 / 10f;
		ColorUtility.TryParseHtmlString("#4A7BB7", out colorKey[1].color);
		colorKey[1].time = 1 / 10f;
		ColorUtility.TryParseHtmlString("#6EA6CD", out colorKey[2].color);
		colorKey[2].time = 2 / 10f;
		ColorUtility.TryParseHtmlString("#98CAE1", out colorKey[3].color);
		colorKey[3].time = 3 / 10f;
		ColorUtility.TryParseHtmlString("#C2E4EF", out colorKey[4].color);
		colorKey[4].time = 4 / 10f;
		ColorUtility.TryParseHtmlString("#EAECCC", out colorKey[5].color);
		colorKey[5].time = 5 / 10f;
		ColorUtility.TryParseHtmlString("#FEDA8B", out colorKey[6].color);
		colorKey[6].time = 6 / 10f;
		ColorUtility.TryParseHtmlString("#FDB366", out colorKey[7].color);
		colorKey[7].time = 7 / 10f;
		ColorUtility.TryParseHtmlString("#F67E4B", out colorKey[8].color);
		colorKey[8].time = 8 / 10f;
		ColorUtility.TryParseHtmlString("#DD3D2D", out colorKey[9].color);
		colorKey[9].time = 9 / 10f;
		ColorUtility.TryParseHtmlString("#A50026", out colorKey[10].color);
		colorKey[10].time = 10 / 10f;



		// Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
		GradientAlphaKey [] alphaKey = new GradientAlphaKey[11];
		for (int i = 0; i < 11; i++)
		{
			alphaKey[i].alpha = 1f;
			alphaKey[i].time = i / 10f;
		}

		gradient.SetKeys(colorKey, alphaKey);*/

		// What's the color at the relative time 0.25 (25 %) ?
		//Debug.Log(gradient.Evaluate(0.25f));

		ColorUtility.TryParseHtmlString("#384bc1", out _leftDivergingColor);
		ColorUtility.TryParseHtmlString("#ffffff", out _middleDivergingColor);
		ColorUtility.TryParseHtmlString("#b50021", out _rightDivergingColor);

		freeze = frozen = false;

		pins.OnConnected += HandleConnected;
	}
	private void Render()
	{
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

		// Draw Hull
		MaterialPropertyBlock hullBlock = new MaterialPropertyBlock();
		hullBlock.SetFloatArray("_TextureIndex", _noTextureIndexes);
		hullBlock.SetVectorArray("_Color", _hullColors);
		hullBlock.SetVectorArray("_OutlineColor", _hullOutlineColors);
		hullBlock.SetFloatArray("_Outline", _hullOutlineWidths);
		hullBlock.SetVectorArray("_SecondOutlineColor", _hullSecondOutlineColors);
		hullBlock.SetFloatArray("_SecondOutline", _hullSecondOutlineWidths);
		hullBlock.SetVectorArray("_LeftHandCenter", _hullLeftHandCenters);
		hullBlock.SetFloatArray("_LeftHandRadius", _hullLeftHandRadius);
		hullBlock.SetVectorArray("_LeftBackArmCenter", _hullLeftBackArmCenters);
		hullBlock.SetVectorArray("_LeftFrontArmCenter", _hullLeftFrontArmCenters);
		hullBlock.SetFloatArray("_LeftArmRadius", _hullLeftArmRadius);
		hullBlock.SetVectorArray("_RightHandCenter", _hullRightHandCenters);
		hullBlock.SetFloatArray("_RightHandRadius", _hullRightHandRadius);
		hullBlock.SetVectorArray("_RightBackArmCenter", _hullRightBackArmCenters);
		hullBlock.SetVectorArray("_RightFrontArmCenter", _hullRightFrontArmCenters);
		hullBlock.SetFloatArray("_RightArmRadius", _hullRightArmRadius);
		Graphics.DrawMeshInstanced(_hullMesh, 0, _lineMat, _hullMatrices, _hullIndex, hullBlock, UnityEngine.Rendering.ShadowCastingMode.Off, false, SEPARATION_LAYER);

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

	void ProjectorEaseIn()
	{
		// Projector Transition
		float feedbackInDuration = 1f;
		float feedbackMinGamma = 0f;
		float feedbackMaxGamma = 1f;

		float recoveryRate = (feedbackMaxGamma - feedbackMinGamma) / feedbackInDuration;
		float projectorAlpha = Mathf.MoveTowards(projector.material.color.a, feedbackMaxGamma, recoveryRate * Time.deltaTime);
		projector.material.color = new Color(1f, 1f, 1f, projectorAlpha);
	}
	void ProjectorEaseOut()
	{
		float feedbackOutDuration = 0.250f;
		float feedbackMinGamma = 0f;
		float feedbackMaxGamma = 1f;
		float recoveryRate = (feedbackMaxGamma - feedbackMinGamma) / feedbackOutDuration;
		float projectorAlpha = Mathf.MoveTowards(projector.material.color.a, feedbackMinGamma, recoveryRate * Time.deltaTime);
		projector.material.color = new Color(1f, 1f, 1f, projectorAlpha);
	}

	void DrawUnsafeShape()
	{

	}

	// Update is called once per frame
	void Update()
    {
		if (!freeze && frozen) // Unfreeze guardian
		{
			frozen = false;
			projector.material.color = new Color(1f, 1f, 1f, 0f);

		}
		else if (freeze && frozen) // Freeze guardian
		{
			ProjectorEaseIn();
			Render();
			return;
		}
		_handIndex = _armIndex = _planeIndex = _dotIndex = _lineIndex = _hullIndex = 0;

		bool toDraw = false;

		float backgroundDistance = 0f;


		// Handle Overlay Modes à priori
		switch (overlayMode)
		{
			case SafetyOverlayMode.Dot:
				// set outlines width
				minOrthographicSize = pins.diameter - 3f; // -1.5f / 2f;
				maxOrthographicSize = minOrthographicSize * 3.3f; // 3.3f / 3.3f;
				minOutlineWidth = 1f;
				maxOutlineWidth = minOutlineWidth * 3.3f;
				minSecondOutlineWidth = 2f;
				maxSecondOutlineWidth = minSecondOutlineWidth * 3.3f;
				minThirdOutlineWidth = 4f;
				maxThirdOutlineWidth = minThirdOutlineWidth * 3.3f;
				minFourthOutlineWidth = 5f;
				maxFourthOutlineWidth = minFourthOutlineWidth * 3.3f;
				minFifthOutlineWidth = 6f;
				maxFifthOutlineWidth = minFifthOutlineWidth * 3.3f;
				break;
			case SafetyOverlayMode.Surface:
				// set outlines width
				minOrthographicSize = pins.diameter - 2f; // -1.5f / 2f;
				maxOrthographicSize = minOrthographicSize * 3.3f; // 3.3f / 3.3f;
				minOutlineWidth = 0f;
				maxOutlineWidth = minOutlineWidth * 3.3f;
				minSecondOutlineWidth = 0f;
				maxSecondOutlineWidth = minSecondOutlineWidth * 3.3f;
				minThirdOutlineWidth = 0f;
				maxThirdOutlineWidth = minThirdOutlineWidth * 3.3f;
				minFourthOutlineWidth = 1.5f;
				maxFourthOutlineWidth = minFourthOutlineWidth * 3.3f;
				minFifthOutlineWidth = 2.9f;
				maxFifthOutlineWidth = minFifthOutlineWidth * 3.3f;
				break;
			case SafetyOverlayMode.Hull:
				// set outlines width
				minOrthographicSize = pins.diameter - 2f; // -1.5f / 2f;
				maxOrthographicSize = minOrthographicSize * 3.3f; // 3.3f / 3.3f;
				minOutlineWidth = 0f;
				maxOutlineWidth = minOutlineWidth * 3.3f;
				minSecondOutlineWidth = 0f;
				maxSecondOutlineWidth = minSecondOutlineWidth * 3.3f;
				minThirdOutlineWidth = 0f;
				maxThirdOutlineWidth = minThirdOutlineWidth * 3.3f;
				minFourthOutlineWidth = 1.5f;
				maxFourthOutlineWidth = minFourthOutlineWidth * 3.3f;
				minFifthOutlineWidth = 2.9f;
				maxFifthOutlineWidth = minFifthOutlineWidth * 3.3f;
				break;
			case SafetyOverlayMode.Zone:
				// set outlines width
				minOrthographicSize = pins.diameter - 2f; // -1.5f / 2f;
				maxOrthographicSize = minOrthographicSize * 3.3f; // 3.3f / 3.3f;
				minOutlineWidth = 0f;
				maxOutlineWidth = minOutlineWidth * 3.3f;
				minSecondOutlineWidth = 0f;
				maxSecondOutlineWidth = minSecondOutlineWidth * 3.3f;
				minThirdOutlineWidth = 0f;
				maxThirdOutlineWidth = minThirdOutlineWidth * 3.3f;
				minFourthOutlineWidth = 1.5f;
				maxFourthOutlineWidth = minFourthOutlineWidth * 3.3f;
				minFifthOutlineWidth = 2.9f;
				maxFifthOutlineWidth = minFifthOutlineWidth * 3.3f;
				break;
			case SafetyOverlayMode.None:
				return;
		}

		//float minScaleDistance = 0f;
		//float maxScaleDistance = minOrthographicSize;


		Vector3 leftHandPos, leftBackArmPos, leftFrontArmPos;
		float leftHandRadius, leftArmRadius;
		leftHandPos = leftBackArmPos = leftFrontArmPos = Vector3.zero;
		leftHandRadius = leftArmRadius = 0f;
		// Generate Left Forearm Zone
		if (pins.leftHand != null && pins.leftHand.IsActive())
		{
			// Left Hand Zone
			GameObject handCollider = pins.leftHand.GetHandCollider();
			SphereCollider sc = handCollider.GetComponent<SphereCollider>();
			Vector3 handColliderPosition = handCollider.transform.position;
			backgroundDistance = - (sc.radius * 2.0f + pins.height);
			handColliderPosition.y = backgroundDistance;
			Vector3 handColliderScale = new Vector3(sc.radius * 2.0f, sc.radius * 2.0f, sc.radius * 2.0f);

			// Save for shader
			leftHandPos = handColliderPosition;
			leftHandRadius = sc.radius;

			_handMatrices[_handIndex] = Matrix4x4.TRS(handColliderPosition, Quaternion.identity, handColliderScale);
			_handColors[_handIndex] = new Vector4(1f, 1f, 1f, 0f);
			_handOutlineColors[_handIndex] = new Vector4(0f, 0f, 0f, 1f);
			_handOutlineWidths[_handIndex] = bodyOutlineWidth;
			_handSecondOutlineColors[_handIndex] = new Vector4(1f, 1f, 1f, 1f);
			_handSecondOutlineWidths[_handIndex] = bodySecondOutlineWidth; 
			_handIndex++;

			// Left Arm Zone
			GameObject armCollider = pins.leftHand.GetArmCollider();
			CapsuleCollider capsuleCollider1 = armCollider.GetComponent<CapsuleCollider>();
			Vector3 forwardArmColliderPosition = armCollider.transform.position + armCollider.transform.forward * (capsuleCollider1.height / 2.0f);
			Vector3 backwardArmColliderPosition = armCollider.transform.position - armCollider.transform.forward * (capsuleCollider1.height / 2.0f);
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
			_armColors[_armIndex] = new Vector4(1f, 1f, 1f, 1f);
			_armOutlineColors[_armIndex] = new Vector4(0f, 0f, 0f, 1f);
			_armOutlineWidths[_armIndex] = bodyOutlineWidth;
			_armSecondOutlineColors[_armIndex] = new Vector4(1f, 1f, 1f, 1f);
			_armSecondOutlineWidths[_armIndex] = bodySecondOutlineWidth;
			_armIndex++;
		}
		Vector3 rightHandPos, rightBackArmPos, rightFrontArmPos;
		float rightHandRadius, rightArmRadius;
		rightHandPos = rightBackArmPos = rightFrontArmPos = Vector3.zero;
		rightHandRadius = rightArmRadius = 0f;
		// Generate Right Hand Zone
		if (pins.rightHand != null && pins.rightHand.IsActive())
		{
			// Right Hand Zone
			GameObject handCollider = pins.rightHand.GetHandCollider();
			SphereCollider sc = handCollider.GetComponent<SphereCollider>();
			Vector3 handColliderPosition = handCollider.transform.position;
			backgroundDistance = - (sc.radius * 2.0f + pins.height);
			handColliderPosition.y = backgroundDistance;
			Vector3 handColliderScale = new Vector3(sc.radius * 2.0f, sc.radius * 2.0f, sc.radius * 2.0f);

			rightHandPos = handColliderPosition;
			rightHandRadius = sc.radius;

			_handMatrices[_handIndex] = Matrix4x4.TRS(handColliderPosition, Quaternion.identity, handColliderScale);
			_handColors[_handIndex] = new Vector4(1f, 1f, 1f, 1f);
			_handOutlineColors[_handIndex] = new Vector4(0f, 0f, 0f, 1f);
			_handOutlineWidths[_handIndex] = bodyOutlineWidth;
			_handSecondOutlineColors[_handIndex] = new Vector4(1f, 1f, 1f, 1f);
			_handSecondOutlineWidths[_handIndex] = 2f;
			_handIndex++;

			// Right Arm Zone
			GameObject armCollider = pins.rightHand.GetArmCollider();
			CapsuleCollider capsuleCollider1 = armCollider.GetComponent<CapsuleCollider>();
			Vector3 forwardArmColliderPosition = armCollider.transform.position + armCollider.transform.forward * (capsuleCollider1.height / 2.0f);
			Vector3 backwardArmColliderPosition = armCollider.transform.position - armCollider.transform.forward * (capsuleCollider1.height / 2.0f);
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
			_armColors[_armIndex] = new Vector4(1f, 1f, 1f, 1f);
			_armOutlineColors[_armIndex] = new Vector4(0f, 0f, 0f, 1f);
			_armOutlineWidths[_armIndex] = bodyOutlineWidth;
			_armSecondOutlineColors[_armIndex] = new Vector4(1f, 1f, 1f, 1f);
			_armSecondOutlineWidths[_armIndex] = bodySecondOutlineWidth;
			_armIndex++;
		}


		// list of points to find convex hull
		List<Vector3> points = new List<Vector3>();

		for (int row = 0; row < pins.NbRows; row++)
		{
			for (int column = 0; column < pins.NbColumns; column++)
			{
				int paused = pins.viewMatrix[row, column].CurrentPaused;
				int feedforwarded = pins.viewMatrix[row, column].CurrentFeedForwarded;
				int displacement = (paused != 0) ? paused : feedforwarded;
				//Debug.Log("displacement : " + displacement);
				Transform pin = pins.viewMatrix[row, column].transform;
				if (displacement != 0)
				{
					toDraw = true;
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

					Color dotColor = (displacement > 0) ? Color.Lerp(_middleDivergingColor, _rightDivergingColor, displacement/40f) : Color.Lerp(_middleDivergingColor, _leftDivergingColor, -displacement/40f);
					_dotColors[_dotIndex] = (feedbackMode != FeedbackMode.State) ? dotColor : new Color(1f, 1f, 1f, 0f);//Color.white;
					_dotOutlineColors[_dotIndex] = new Vector4(1f, 1f, 1f, 1f);
					_dotOutlineWidths[_dotIndex] = Mathf.Lerp(minOutlineWidth, maxOutlineWidth, scaleDistanceCoeff);
					_dotSecondOutlineColors[_dotIndex] = new Vector4(0f, 0f, 0f, 1f);
					_dotSecondOutlineWidths[_dotIndex] = Mathf.Lerp(minSecondOutlineWidth, maxSecondOutlineWidth, scaleDistanceCoeff);
					_dotThirdOutlineColors[_dotIndex] = _dotColors[_dotIndex];
					_dotThirdOutlineWidths[_dotIndex] = Mathf.Lerp(minThirdOutlineWidth, maxThirdOutlineWidth, scaleDistanceCoeff);
					_dotFourthOutlineColors[_dotIndex] = new Vector4(0f, 0f, 0f, 1f);
					_dotFourthOutlineWidths[_dotIndex] = Mathf.Lerp(minFourthOutlineWidth, maxFourthOutlineWidth, scaleDistanceCoeff);
					_dotFifthOutlineColors[_dotIndex] = new Vector4(1f, 1f, 1f, 1f);
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

					// add pin to points for convex hull
					points.Add(new Vector3(dotPos.x, 0f, dotPos.z));
					// Generate Plane
					Vector3 planePos = pin.position + pin.up * ((dotDiameter - minOrthographicSize + 0.1f) + pins.height / 2.0f);
					Quaternion planeRot = pin.rotation * Quaternion.AngleAxis(90, pin.up);
					Vector3 planeScale = new Vector3(minOrthographicSize + 0.1f, 0.1f, minOrthographicSize + 0.1f);
					_planeMatrices[_planeIndex] = Matrix4x4.TRS(
						 planePos,
						 planeRot,
						 planeScale
					);
					float displacementPercent = Mathf.InverseLerp(-40f, 40f, displacement);
					int directionAmount = Mathf.RoundToInt(Mathf.Lerp(0f, 30f, displacementPercent));
					Graphics.CopyTexture(_shapeTextures[directionAmount], 0, 0, _iconTextureArray, _planeIndex, 0); // i is the index of the texture
					_iconTextureIndexes[_planeIndex] = _planeIndex;
					_planeColors[_planeIndex] = dotColor;
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


					// Find Nearest Point
					Vector3 targetPos = Vector3.zero;
					float minDistance = float.PositiveInfinity;
					// Distance to left Hand
					Vector3 projectLeftHandPos = new Vector3(leftHandPos.x, dotPos.y, leftHandPos.z);
					float distanceToLeftHand = (projectLeftHandPos - dotPos).magnitude - dotScale.x / 2.0f;
					if (distanceToLeftHand < leftHandRadius && distanceToLeftHand < minDistance)
					{
						targetPos = projectLeftHandPos;
						minDistance = distanceToLeftHand;

					}
					// Distance to right Hand
					Vector3 projectRightHandPos = new Vector3(rightHandPos.x, dotPos.y, rightHandPos.z);
					float distanceToRightHand = (projectRightHandPos - dotPos).magnitude - dotScale.x / 2.0f;
					if (distanceToRightHand < rightHandRadius && distanceToRightHand < minDistance)
					{
						targetPos = projectRightHandPos;
						minDistance = distanceToRightHand;
					}
					if (minDistance == float.PositiveInfinity)
					{
						// Distance to left arm
						Vector3 projectLeftBackArmPos = new Vector3(leftBackArmPos.x, dotPos.y, leftBackArmPos.z);
						Vector3 projectLeftFrontArmPos = new Vector3(leftFrontArmPos.x, dotPos.y, leftFrontArmPos.z);
						Vector3 projectLeftArmPos = ProjectPointLine(dotPos, projectLeftBackArmPos, projectLeftFrontArmPos);
						float distanceToLeftArm = (projectLeftArmPos - dotPos).magnitude - dotScale.x / 2.0f;
						if (distanceToLeftArm < leftArmRadius && distanceToLeftArm < minDistance)
						{
							targetPos = projectLeftArmPos;
							minDistance = distanceToLeftArm;

						}
						// Distance to right Hand
						Vector3 projectRightBackArmPos = new Vector3(rightBackArmPos.x, dotPos.y, rightBackArmPos.z);
						Vector3 projectRightFrontArmPos = new Vector3(rightFrontArmPos.x, dotPos.y, rightFrontArmPos.z);
						Vector3 projectRightArmPos = ProjectPointLine(dotPos, projectRightBackArmPos, projectRightFrontArmPos);
						float distanceToRightArm = (projectRightArmPos - dotPos).magnitude - dotScale.x / 2.0f;
						if (distanceToRightArm < rightArmRadius && distanceToRightArm < minDistance)
						{
							targetPos = projectRightArmPos;
							minDistance = distanceToRightArm;
						}
					}
				}
				

			}
		}
		// Generate Convex Hull
		List<Vector3> hull = new List<Vector3>();
		_hullMesh = new Mesh();
		bool hasAtLeastThreeEntries = GeometryUtils.ConvexHull2D(points, hull);
		if (hasAtLeastThreeEntries)
		{
			// Draw Lines
			for (int i = 0; i < hull.Count; i++)
			{
				Vector3 dotPos = hull[i];
				Vector3 targetPos = hull[(i + 1) % hull.Count];

				// Generate Lines
				Vector3 dotToTarget = targetPos - dotPos;
				float length = dotToTarget.magnitude / 2.0f;
				Vector3 linePos = dotPos + dotToTarget / 2.0f;
				Quaternion lineRot = (dotToTarget == Vector3.zero) ? Quaternion.identity : Quaternion.LookRotation(targetPos - dotPos) * Quaternion.AngleAxis(90, Vector3.right);
				Vector3 lineScale = new Vector3(pins.diameter - 2f, length, pins.diameter - 2f);
				_lineMatrices[_lineIndex] = Matrix4x4.TRS(
					linePos,
					lineRot,
					lineScale
				);
				_lineColors[_lineIndex] = new Vector4(1f, 1f, 1f, 1f);
				_lineOutlineColors[_lineIndex] = new Vector4(0f, 0f, 0f, 1f);
				_lineOutlineWidths[_lineIndex] = 1.2f;
				_lineSecondOutlineColors[_lineIndex] = new Vector4(1f, 1f, 1f, 1f);
				_lineSecondOutlineWidths[_lineIndex] = 2.6f;
				// left hand mask
				_lineLeftHandCenters[_lineIndex] = leftHandPos;
				_lineLeftHandRadius[_lineIndex] = leftHandRadius;
				// left arm mask
				_lineLeftBackArmCenters[_lineIndex] = leftBackArmPos;
				_lineLeftFrontArmCenters[_lineIndex] = leftFrontArmPos;
				_lineLeftArmRadius[_lineIndex] = leftArmRadius;
				// right hand mask
				_lineRightHandCenters[_lineIndex] = rightHandPos;
				_lineRightHandRadius[_lineIndex] = rightHandRadius;
				// right arm mask
				_lineRightBackArmCenters[_lineIndex] = rightBackArmPos;
				_lineRightFrontArmCenters[_lineIndex] = rightFrontArmPos;
				_lineRightArmRadius[_lineIndex] = rightArmRadius;
				_lineIndex++;
			}

			// Fill Polygon
			_hullMesh.hideFlags = HideFlags.DontSave;
			Vector3 centroid = GeometryUtils.PolygonCentroid2D(hull);
			// Vertices
			Vector3[] vertices = new Vector3[hull.Count + 1];
			vertices[0] = centroid;
			for (int i = 0; i < hull.Count; i++) vertices[i + 1] = hull[i];
			_hullMesh.vertices = vertices;
			// Triangles
			int[] tris = new int[hull.Count * 3];
			for(int i = 0; i < hull.Count; i++)
			{
				int currPointIndex = i;
				int nextPointIndex = (i + 1) % hull.Count;
				tris[i * 3] = currPointIndex + 1;
				tris[i * 3 + 1] = nextPointIndex + 1;
				tris[i * 3 + 2] = 0;

			}
			_hullMesh.triangles = tris;
			_hullMesh.Optimize();
			_hullMesh.RecalculateNormals();

			_hullMatrices[_hullIndex] = Matrix4x4.TRS(
					Vector3.zero,
					Quaternion.identity,
					Vector3.one
				);
			_hullColors[_hullIndex] = new Vector4(1f, 1f, 1f, 1f);
			_hullOutlineColors[_hullIndex] = new Vector4(0f, 0f, 0f, 1f);
			_hullOutlineWidths[_hullIndex] = 1.2f;
			_hullSecondOutlineColors[_hullIndex] = new Vector4(1f, 1f, 1f, 1f);
			_hullSecondOutlineWidths[_hullIndex] = 2.6f;
			// left hand mask
			_hullLeftHandCenters[_hullIndex] = leftHandPos;
			_hullLeftHandRadius[_hullIndex] = leftHandRadius;
			// left arm mask
			_hullLeftBackArmCenters[_hullIndex] = leftBackArmPos;
			_hullLeftFrontArmCenters[_hullIndex] = leftFrontArmPos;
			_hullLeftArmRadius[_hullIndex] = leftArmRadius;
			// right hand mask
			_hullRightHandCenters[_hullIndex] = rightHandPos;
			_hullRightHandRadius[_hullIndex] = rightHandRadius;
			// right arm mask
			_hullRightBackArmCenters[_hullIndex] = rightBackArmPos;
			_hullRightFrontArmCenters[_hullIndex] = rightFrontArmPos;
			_hullRightArmRadius[_hullIndex] = rightArmRadius;
			_hullIndex++;
		}

		// Handle Overlay Modes à posteriori
		switch (overlayMode)
		{
			case SafetyOverlayMode.Dot:
				// hide lines and hull
				_lineIndex =_hullIndex = 0;
				// hide zones
				for (int i = 0; i < _handIndex; i++)
				{
					_handColors[i] = new Vector4(1f, 1f, 1f, 0f);
				}
				for (int i = 0; i < _armIndex; i++)
				{
					_armColors[i] = new Vector4(1f, 1f, 1f, 0f);
				}
			break;
			case SafetyOverlayMode.Surface:
				// hide lines and hull
				_lineIndex = _hullIndex = 0;
				// hide zones
				for (int i = 0; i < _handIndex; i++)
				{
					_handColors[i] = new Vector4(1f, 1f, 1f, 0f);
				}
				for (int i = 0; i < _armIndex; i++)
				{
					_armColors[i] = new Vector4(1f, 1f, 1f, 0f);
				}
				break;
			case SafetyOverlayMode.Hull:
				// hide zones
				for (int i = 0; i < _handIndex; i++)
				{
					_handColors[i] = new Vector4(1f, 1f, 1f, 0f);
				}
				for (int i = 0; i < _armIndex; i++)
				{
					_armColors[i] = new Vector4(1f, 1f, 1f, 0f);
				}
				break;
			case SafetyOverlayMode.Zone:
				// hide lines and hull
				_lineIndex = _hullIndex = 0;
				// hide dots outlines
				for (int i = 0; i < _dotIndex; i++)
				{
					_dotOutlineColors[i] = new Vector4(1f, 1f, 1f, 0f);
					_dotSecondOutlineColors[i] = new Vector4(1f, 1f, 1f, 0f);
					_dotThirdOutlineColors[i] = new Vector4(1f, 1f, 1f, 0f);
				}
				break;
			case SafetyOverlayMode.None:
				return;
		}
		// Render graphics
		Render();
		if (freeze && toDraw && !frozen) frozen = true;
		// Combine and Draw hand & arm zones
		if (toDraw) ProjectorEaseIn();
		else ProjectorEaseOut();
	}
}
