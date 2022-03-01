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
	private const float warningZoneRadius = 0.06f;

	private const float pinFirstOutlineWidth = 0.01f / 100f;
	private const float pinSecondOutlineWidth = pinFirstOutlineWidth + 0.04f / 100f;
	private const float pinThirdOutlineWidth = pinSecondOutlineWidth + 0.01f / 100f;

	private const float dotFirstOutlineWidth = 0.003f / 100f;
	private const float bodyFirstOutlineWidth = 0.1f/100f;
	private const float bodySecondOutlineWidth = bodyFirstOutlineWidth + 0.4f/100f;
	private const float bodyThirdOutlineWidth = bodySecondOutlineWidth + 0.1f/100f;

	private const float minStopDistance = 0.06f;
	private const float maxStopDistance = 0.09f;

	Gradient gradient;
	GradientColorKey[] colorKey;
	GradientAlphaKey[] alphaKey;


	public Mesh _handMesh;
	public Material _handMat;
	private Matrix4x4[] _foreHandMatrices, _backHandMatrices;
	private Vector4[] _foreHandColors,_foreHandFirstOutlineColors, _foreHandSecondOutlineColors, _foreHandThirdOutlineColors,
					 _backHandColors, _backHandFirstOutlineColors, _backHandSecondOutlineColors, _backHandThirdOutlineColors;
	private float[] _foreHandFirstOutlineWidths, _foreHandSecondOutlineWidths, _foreHandThirdOutlineWidths,
					_backHandFirstOutlineWidths, _backHandSecondOutlineWidths, _backHandThirdOutlineWidths;
	private int _foreHandIndex = 0;
	private int _backHandIndex = 0;

	public Mesh _armMesh;
	public Material _armMat;
	private Matrix4x4[] _foreArmMatrices, _backArmMatrices;
	private Vector4[] _foreArmColors, _foreArmFirstOutlineColors, _foreArmSecondOutlineColors, _foreArmThirdOutlineColors,
					  _backArmColors, _backArmFirstOutlineColors, _backArmSecondOutlineColors, _backArmThirdOutlineColors;
	private float[] _foreArmFirstOutlineWidths, _foreArmSecondOutlineWidths, _foreArmThirdOutlineWidths, 
					_backArmFirstOutlineWidths, _backArmSecondOutlineWidths, _backArmThirdOutlineWidths;
	private int _foreArmIndex = 0;
	private int _backArmIndex = 0;

	public Mesh _pinMesh;
	public Material _pinMat;
	private Matrix4x4[] _pinMatrices;
	private Vector4[] _pinColors, _pinFirstOutlineColors, _pinSecondOutlineColors, _pinThirdOutlineColors;
	private float[] _pinFirstOutlineWidths, _pinSecondOutlineWidths, _pinThirdOutlineWidths;
	private int _pinIndex = 0;

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
	private Vector4[] _dotColors, _dotFirstOutlineColors; // _dotSecondOutlineColors, _dotThirdOutlineColors, _dotFourthOutlineColors, _dotFifthOutlineColors;
	private float[] _dotFirstOutlineWidths; //, _dotSecondOutlineWidths, _dotThirdOutlineWidths, _dotFourthOutlineWidths, _dotFifthOutlineWidths;
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
	//private int _lineIndex = 0;

	public enum SafetyOverlayMode {User, System, Mixed};
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

	// LEFT HAND
	private Vector3 leftHandPos = Vector3.zero;
	private Vector3 leftHandScale = Vector3.zero;
	private float leftHandRadius = 0f;
	// RIGHT HAND
	private Vector3 rightHandPos = Vector3.zero;
	private Vector3 rightHandScale = Vector3.zero;
	private float rightHandRadius = 0f;
	// LEFT ARM
	private Vector3 leftArmPos = Vector3.zero;
	private Vector3 leftArmScale = Vector3.zero;
	private Quaternion leftArmRotation = Quaternion.identity;
	private Vector3 leftBackArmPos = Vector3.zero;
	private Vector3 leftFrontArmPos = Vector3.zero;
	private float leftArmRadius = 0f;
	// RIGHT ARM
	private Vector3 rightArmPos = Vector3.zero;
	private Vector3 rightArmScale = Vector3.zero;
	private Quaternion rightArmRotation = Quaternion.identity;
	private Vector3 rightBackArmPos = Vector3.zero;
	private Vector3 rightFrontArmPos = Vector3.zero;
	private float rightArmRadius = 0f;


	private const float backgroundDistance = -0.5f;
	private const float foregroundDistance = 0.5f;
	private float bodyGamma = 0f;

	// Get current Unsafes Pins

	public const float feedbackInDuration = 0.250f;
	public const float feedbackOutDuration = 0.250f;
	public const float feedbackMinGamma = 0f;
	public const float feedbackMaxGamma = 1f;

	public const float recoveryRateIn = (feedbackMaxGamma - feedbackMinGamma) / feedbackInDuration;
	public const float recoveryRateOut = (feedbackMaxGamma - feedbackMinGamma) / feedbackOutDuration;


	public float distanceFromSafetyCamera = 1f;
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
		this.transform.position = new Vector3(((pins.NbRows - 1) * (pins.diameter + pins.offset)) / 2f + (pins.diameter - pins.BorderOffset) / 2f, distanceFromSafetyCamera, ((pins.NbColumns - 1) * (pins.diameter + pins.offset)) / 2f);
		Vector3 safeCameraLookAtPosition = this.transform.position - new Vector3(0f, distanceFromSafetyCamera, 0f);
		this.transform.LookAt(safeCameraLookAtPosition);
		this.transform.eulerAngles += new Vector3(0f, 90f, 0f);

		// Configure camera
		Camera cam = this.GetComponent<Camera>();
		cam.orthographicSize = 0.30f; // (pins.NbRows * (pins.diameter + pins.offset) + 2 * pins.offset + 2 * pins.BorderOffset + (pins.diameter - pins.BorderOffset)) / 2f;
		
		// Configure projector
		projector = this.GetComponent<Projector>();
		projector.orthographicSize = cam.orthographicSize;
		projector.material.color = new Color(1f, 1f, 1f, 1f);

		currUnsafeTransitions = new List<Vector3>();
		nextUnsafeTransitions = new List<Vector3>();

		_foreHandMatrices =  new Matrix4x4[32];
		_backHandMatrices = new Matrix4x4[32];
		_foreHandColors = new Vector4[32];
		_backHandColors = new Vector4[32];

		_foreHandFirstOutlineColors = new Vector4[32];
		_backHandFirstOutlineColors = new Vector4[32];
		_foreHandFirstOutlineWidths = new float[32];
		_backHandFirstOutlineWidths = new float[32];

		_foreHandSecondOutlineColors = new Vector4[32]; 
		_backHandSecondOutlineColors = new Vector4[32];
		_foreHandSecondOutlineWidths = new float[32];
		_backHandSecondOutlineWidths = new float[32];

		_foreHandThirdOutlineColors = new Vector4[32];
		_backHandThirdOutlineColors = new Vector4[32];
		_foreHandThirdOutlineWidths = new float[32];
		_backHandThirdOutlineWidths = new float[32];

		_foreArmMatrices = new Matrix4x4[32];
		_backArmMatrices = new Matrix4x4[32];
		_foreArmColors = new Vector4[32];
		_backArmColors =  new Vector4[32];

		_foreArmFirstOutlineColors = new Vector4[32];
		_backArmFirstOutlineColors = new Vector4[32];
		_foreArmFirstOutlineWidths = new float[32];
		_backArmFirstOutlineWidths = new float[32];

		_foreArmSecondOutlineColors = new Vector4[32];
		_backArmSecondOutlineColors = new Vector4[32];
		_foreArmSecondOutlineWidths = new float[32];
		_backArmSecondOutlineWidths = new float[32];

		_foreArmThirdOutlineColors = new Vector4[32];
		_backArmThirdOutlineColors = new Vector4[32];
		_foreArmThirdOutlineWidths = new float[32];
		_backArmThirdOutlineWidths = new float[32];

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
		_pinMatrices = new Matrix4x4[32];
		_pinColors = new Vector4[32];
		_pinFirstOutlineColors = new Vector4[32];
		_pinFirstOutlineWidths = new float[32];
		_pinSecondOutlineColors = new Vector4[32];
		_pinSecondOutlineWidths = new float[32];
		_pinThirdOutlineColors = new Vector4[32];
		_pinThirdOutlineWidths = new float[32];

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
		_dotFirstOutlineColors = new Vector4[32];
		_dotFirstOutlineWidths = new float[32];
		/*_dotSecondOutlineColors = new Vector4[32];
		_dotSecondOutlineWidths = new float[32];
		_dotThirdOutlineColors = new Vector4[32];
		_dotThirdOutlineWidths = new float[32];
		_dotFourthOutlineColors = new Vector4[32];
		_dotFourthOutlineWidths = new float[32];
		_dotFifthOutlineColors = new Vector4[32];
		_dotFifthOutlineWidths = new float[32];*/
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


		gradient = new Gradient();

		// Populate the color keys at the relative time 0 and 1 (0 and 100%)
		colorKey = new GradientColorKey[3];
		colorKey[0].color = Color.green;
		colorKey[0].time = 0.0f;
		colorKey[1].color = Color.yellow;
		colorKey[1].time = 0.5f;
		colorKey[2].color = Color.red;
		colorKey[2].time = 1.0f;

		// Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
		alphaKey = new GradientAlphaKey[3];
		alphaKey[0].alpha = 1.0f;
		alphaKey[0].time = 0.0f;
		alphaKey[1].alpha = 1.0f;
		alphaKey[1].time = 0.5f;
		alphaKey[2].alpha = 1.0f;
		alphaKey[2].time = 1.0f;


		gradient.SetKeys(colorKey, alphaKey);


		freeze = frozen = false;


		pins.OnConnected += HandleConnected;
	}
	private void Render()
	{

		// Draw background body
		CombineInstance[] backCombine = new CombineInstance[_backHandIndex + _backArmIndex];
		for (int i = 0; i < _backHandIndex; i++)
		{
			backCombine[i].mesh = _handMesh;
			backCombine[i].transform = _backHandMatrices[i];
		}
		for (int i = 0; i < _backArmIndex; i++)
		{
			backCombine[_backHandIndex + i].mesh = _armMesh;
			backCombine[_backHandIndex + i].transform = _backArmMatrices[i];
		}
		Mesh backBodyMesh = new Mesh();
		backBodyMesh.CombineMeshes(backCombine);

		MaterialPropertyBlock backBodyBlock = new MaterialPropertyBlock();
		Matrix4x4[] _backBodyMatrices = new Matrix4x4[] { Matrix4x4.identity };
		backBodyBlock.SetVectorArray("_Color", _backArmColors);
		backBodyBlock.SetVectorArray("_FirstOutlineColor", _backArmFirstOutlineColors);
		backBodyBlock.SetFloatArray("_FirstOutlineWidth", _backArmFirstOutlineWidths);
		backBodyBlock.SetVectorArray("_SecondOutlineColor", _backArmSecondOutlineColors);
		backBodyBlock.SetFloatArray("_SecondOutlineWidth", _backArmSecondOutlineWidths);
		backBodyBlock.SetVectorArray("_ThirdOutlineColor", _backArmThirdOutlineColors);
		backBodyBlock.SetFloatArray("_ThirdOutlineWidth", _backArmThirdOutlineWidths);

		Graphics.DrawMeshInstanced(backBodyMesh, 0, _armMat, _backBodyMatrices, _backBodyMatrices.Length, backBodyBlock, UnityEngine.Rendering.ShadowCastingMode.Off, false, SEPARATION_LAYER);


		// Draw middle pin zone
		MaterialPropertyBlock pinBlock = new MaterialPropertyBlock();
		pinBlock.SetVectorArray("_Color", _pinColors);
		pinBlock.SetVectorArray("_FirstOutlineColor", _pinFirstOutlineColors);
		pinBlock.SetFloatArray("_FirstOutlineWidth", _pinFirstOutlineWidths);
		pinBlock.SetVectorArray("_SecondOutlineColor", _pinSecondOutlineColors);
		pinBlock.SetFloatArray("_SecondOutlineWidth", _pinSecondOutlineWidths);
		pinBlock.SetVectorArray("_ThirdOutlineColor", _pinThirdOutlineColors);
		pinBlock.SetFloatArray("_ThirdOutlineWidth", _pinThirdOutlineWidths);
		Graphics.DrawMeshInstanced(_pinMesh, 0, _pinMat, _pinMatrices, _pinIndex, pinBlock, UnityEngine.Rendering.ShadowCastingMode.Off, false, SEPARATION_LAYER);



		// Draw middle grounds
		MaterialPropertyBlock dotBlock = new MaterialPropertyBlock();
		dotBlock.SetFloatArray("_TextureIndex", _noTextureIndexes);
		dotBlock.SetVectorArray("_Color", _dotColors);
		dotBlock.SetVectorArray("_FirstOutlineColor", _dotFirstOutlineColors);
		dotBlock.SetFloatArray("_FirstOutlineWidth", _dotFirstOutlineWidths);
		/*dotBlock.SetVectorArray("_SecondOutlineColor", _dotSecondOutlineColors);
		dotBlock.SetFloatArray("_SecondOutline", _dotSecondOutlineWidths);
		dotBlock.SetVectorArray("_ThirdOutlineColor", _dotThirdOutlineColors);
		dotBlock.SetFloatArray("_ThirdOutline", _dotThirdOutlineWidths);
		dotBlock.SetVectorArray("_FourthOutlineColor", _dotFourthOutlineColors);
		dotBlock.SetFloatArray("_FourthOutline", _dotFourthOutlineWidths);
		dotBlock.SetVectorArray("_FifthOutlineColor", _dotFifthOutlineColors);
		dotBlock.SetFloatArray("_FifthOutline", _dotFifthOutlineWidths);*/

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
		/*MaterialPropertyBlock lineBlock = new MaterialPropertyBlock();
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
		*/
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
		
		// Draw foreground body
		CombineInstance[] foreCombine = new CombineInstance[_foreHandIndex + _foreArmIndex];
		for (int i = 0; i < _foreHandIndex; i++)
		{
			foreCombine[i].mesh = _handMesh;
			foreCombine[i].transform = _foreHandMatrices[i];
		}
		for (int i = 0; i < _foreArmIndex; i++)
		{
			foreCombine[_foreHandIndex + i].mesh = _armMesh;
			foreCombine[_foreHandIndex + i].transform = _foreArmMatrices[i];
		}
		Mesh foreBodyMesh = new Mesh();
		foreBodyMesh.CombineMeshes(foreCombine);

		MaterialPropertyBlock foreBodyBlock = new MaterialPropertyBlock();
		Matrix4x4[] _foreBodyMatrices = new Matrix4x4[] { Matrix4x4.identity };
		foreBodyBlock.SetVectorArray("_Color", _foreArmColors);
		foreBodyBlock.SetVectorArray("_FirstOutlineColor", _foreArmFirstOutlineColors);
		foreBodyBlock.SetFloatArray("_FirstOutlineWidth", _foreArmFirstOutlineWidths);
		foreBodyBlock.SetVectorArray("_SecondOutlineColor", _foreArmSecondOutlineColors);
		foreBodyBlock.SetFloatArray("_SecondOutlineWidth", _foreArmSecondOutlineWidths);
		foreBodyBlock.SetVectorArray("_ThirdOutlineColor", _foreArmThirdOutlineColors);
		foreBodyBlock.SetFloatArray("_ThirdOutlineWidth", _foreArmThirdOutlineWidths);
		Graphics.DrawMeshInstanced(foreBodyMesh, 0, _armMat, _foreBodyMatrices, _foreBodyMatrices.Length, foreBodyBlock, UnityEngine.Rendering.ShadowCastingMode.Off, false, SEPARATION_LAYER);

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
	private void FetchUserBody()
	{
		if (currLeftHandCollider != null && currLeftArmCollider != null)
		{
			// Left Hand Zone
			SphereCollider sc = currLeftHandCollider.GetComponent<SphereCollider>();
			Vector3 handColliderPosition = currLeftHandCollider.transform.position;
			//backgroundDistance = +(sc.radius * 2.0f + pins.height);
			//handColliderPosition.y = backgroundDistance;
			Vector3 handColliderScale = new Vector3((sc.radius + MyCapsuleHand.STOP_RADIUS) * 2.0f, (sc.radius + MyCapsuleHand.STOP_RADIUS) * 2.0f, (sc.radius + MyCapsuleHand.STOP_RADIUS) * 2.0f);

			// Save for shader
			leftHandPos = handColliderPosition;
			leftHandScale = handColliderScale;
			leftHandRadius = sc.radius;

			// Left Arm Zone
			CapsuleCollider capsuleCollider1 = currLeftArmCollider.GetComponent<CapsuleCollider>();
			Vector3 forwardArmColliderPosition = currLeftArmCollider.transform.position + currLeftArmCollider.transform.forward * (capsuleCollider1.height / 2.0f);
			Vector3 backwardArmColliderPosition = currLeftArmCollider.transform.position - currLeftArmCollider.transform.forward * (capsuleCollider1.height / 2.0f);
			forwardArmColliderPosition.y = 0f;
			backwardArmColliderPosition.y = 0f;
			Vector3 armColliderPosition = backwardArmColliderPosition + (forwardArmColliderPosition - backwardArmColliderPosition) / 2.0f;
			Quaternion armColliderRotation = Quaternion.LookRotation(forwardArmColliderPosition - backwardArmColliderPosition) * Quaternion.AngleAxis(90, Vector3.right); ; // Quaternion.Euler(_forearmColliders[0].transform.rotation.eulerAngles.x, _forearmColliders[0].transform.rotation.eulerAngles.y, _forearmColliders[0].transform.rotation.eulerAngles.z);
			Vector3 colliderScale = new Vector3((capsuleCollider1.radius + MyCapsuleHand.STOP_RADIUS) * 2f, capsuleCollider1.height / 2.0f, (capsuleCollider1.radius + MyCapsuleHand.STOP_RADIUS) * 2f);
			Vector3 frontToBackArm = Vector3.Normalize(backwardArmColliderPosition - forwardArmColliderPosition);
			float distFrontArmOutOfHand = sc.radius - Vector3.Magnitude(handColliderPosition - forwardArmColliderPosition);


			// Save for shader
			leftFrontArmPos = forwardArmColliderPosition + frontToBackArm * distFrontArmOutOfHand;
			leftBackArmPos = backwardArmColliderPosition;
			leftArmRadius = capsuleCollider1.radius;
			leftArmPos = armColliderPosition;
			leftArmRotation = armColliderRotation;
			leftArmScale = colliderScale;
		}

		// Generate Right Hand Zone
		if (currRightHandCollider != null && currRightArmCollider != null)
		{
			// Right Hand Zone
			// prevRightHandCollider = pins.rightHand.GetHandCollider();
			SphereCollider sc = currRightHandCollider.GetComponent<SphereCollider>();
			Vector3 handColliderPosition = currRightHandCollider.transform.position;
			//backgroundDistance = +(sc.radius * 2.0f + pins.height);
			//handColliderPosition.y = backgroundDistance;
			Vector3 handColliderScale = new Vector3((sc.radius + MyCapsuleHand.STOP_RADIUS) * 2.0f, (sc.radius + MyCapsuleHand.STOP_RADIUS) * 2.0f, (sc.radius + MyCapsuleHand.STOP_RADIUS) * 2.0f);

			rightHandPos = handColliderPosition;
			rightHandScale = handColliderScale;
			rightHandRadius = sc.radius;

			// Right Arm Zone
			// prevRightArmCollider = pins.rightHand.GetArmCollider();
			CapsuleCollider capsuleCollider1 = currRightArmCollider.GetComponent<CapsuleCollider>();
			Vector3 forwardArmColliderPosition = currRightArmCollider.transform.position + currRightArmCollider.transform.forward * (capsuleCollider1.height / 2.0f);
			Vector3 backwardArmColliderPosition = currRightArmCollider.transform.position - currRightArmCollider.transform.forward * (capsuleCollider1.height / 2.0f);
			forwardArmColliderPosition.y = 0f;
			backwardArmColliderPosition.y = 0f;

			Vector3 armColliderPosition = backwardArmColliderPosition + (forwardArmColliderPosition - backwardArmColliderPosition) / 2.0f;
			Quaternion armColliderRotation = Quaternion.LookRotation(forwardArmColliderPosition - backwardArmColliderPosition) * Quaternion.AngleAxis(90, Vector3.right); ; // Quaternion.Euler(_forearmColliders[0].transform.rotation.eulerAngles.x, _forearmColliders[0].transform.rotation.eulerAngles.y, _forearmColliders[0].transform.rotation.eulerAngles.z);
			Vector3 colliderScale = new Vector3((capsuleCollider1.radius + MyCapsuleHand.STOP_RADIUS) * 2f, capsuleCollider1.height / 2.0f, (capsuleCollider1.radius + MyCapsuleHand.STOP_RADIUS) * 2f);
			Vector3 frontToBackArm = Vector3.Normalize(backwardArmColliderPosition - forwardArmColliderPosition);
			float distFrontArmOutOfHand = sc.radius - Vector3.Magnitude(handColliderPosition - forwardArmColliderPosition);
			
			// Save for shader
			rightFrontArmPos = forwardArmColliderPosition + frontToBackArm * distFrontArmOutOfHand;
			rightBackArmPos = backwardArmColliderPosition;
			rightArmRadius = capsuleCollider1.radius;
			rightArmPos = armColliderPosition;
			rightArmRotation = armColliderRotation;
			rightArmScale = colliderScale;
		}

	}
	
	private void GenerateSystemOverlay()
	{

		for (int i = 0; i < currUnsafeTransitions.Count; i++)
		{
			Vector3 unsafeTransition = currUnsafeTransitions[i];
			int row = (int)unsafeTransition.x;
			int column = (int)unsafeTransition.y;
			float distanceGamma = unsafeTransition.z;
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


			dotPos = new Vector3(pin.position.x, 0f, pin.position.z);
			Vector3 dotScale = new Vector3(dotDiameter, 0.01f, dotDiameter);
			_dotMatrices[_dotIndex] = Matrix4x4.TRS(
				dotPos,
				dotRot,
				dotScale
			);

			//Color dotColor = (displacement > 0) ? Color.Lerp(_middleDivergingColor, _rightDivergingColor, displacement / 40f) : Color.Lerp(_middleDivergingColor, _leftDivergingColor, -displacement / 40f);
			Color dotColor = gradient.Evaluate(distanceGamma); // new Color(1f, 0f, 0f, distanceGamma);
			float safetyDiameter = pins.diameter + MyCapsuleHand.STOP_RADIUS * 2f;
			float safetyRadius = safetyDiameter / 2.0f;

			_dotColors[_dotIndex] = dotColor; //(feedbackMode != FeedbackMode.State) ? dotColor : new Color(1f, 1f, 1f, 0f);//Color.white;
			_dotFirstOutlineColors[_dotIndex] = Color.black;
			_dotFirstOutlineWidths[_dotIndex] = dotFirstOutlineWidth;
			/*_dotSecondOutlineColors[_dotIndex] = Color.black;
			_dotSecondOutlineWidths[_dotIndex] = 0.0f;
			_dotThirdOutlineColors[_dotIndex] = Color.black;
			_dotThirdOutlineWidths[_dotIndex] = 0.0f;
			_dotFourthOutlineColors[_dotIndex] = Color.black;
			_dotFourthOutlineWidths[_dotIndex] = 0.0f;
			_dotFifthOutlineColors[_dotIndex] = Color.black;
			_dotFifthOutlineWidths[_dotIndex] = 0.0f;*/

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

			// Generate safety zone
			float ZDepth = (row * pins.NbColumns + column) / 1000f;
			Vector3 pinPos = new Vector3(pin.position.x, -pins.height * 4f, pin.position.z);
			Vector3 pinScale = new Vector3(safetyDiameter, pin.localScale.y, safetyDiameter);

			_pinMatrices[_pinIndex] = Matrix4x4.TRS(pinPos, Quaternion.identity, pinScale);
			_pinColors[_pinIndex] = new Vector4(1f, 1f, 1f, 0f);//distanceGamma); //Color.white;
			_pinFirstOutlineColors[_pinIndex] = Color.black;
			_pinFirstOutlineWidths[_pinIndex] = pinFirstOutlineWidth;
			_pinSecondOutlineColors[_pinIndex] = dotColor;
			_pinSecondOutlineWidths[_pinIndex] = pinSecondOutlineWidth;
			_pinThirdOutlineColors[_pinIndex] = Color.black;
			_pinThirdOutlineWidths[_pinIndex] = pinThirdOutlineWidth;
			_pinIndex++;


			// Generate Plane

			Vector3 planePos = new Vector3(pin.position.x, 0.03f, pin.position.z);
			Quaternion planeRot = pin.rotation * Quaternion.AngleAxis(90, pin.up);
			Vector3 planeScale = new Vector3(minOrthographicSize, 0.01f, minOrthographicSize);
			_planeMatrices[_planeIndex] = Matrix4x4.TRS(
					planePos,
					planeRot,
					planeScale
			);
			float displacementPercent = Mathf.InverseLerp(-40f, 40f, displacement);
			int directionAmount = Mathf.RoundToInt(Mathf.Lerp(0f, 30f, displacementPercent));
			Graphics.CopyTexture(_shapeTextures[directionAmount], 0, 0, _iconTextureArray, _planeIndex, 0); // i is the index of the texture
			_iconTextureIndexes[_planeIndex] = _planeIndex;
			_planeColors[_planeIndex] = new Color(1f, 1f, 1f, (distanceGamma < 1f) ? 0f : 1f);//dotColor;
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

			/*
			_dotColors[_dotIndex] = dotColor; // new Color(1f, 0f, 0f, (distanceGamma <= 1f) ? 0f : 1f); //(feedbackMode != FeedbackMode.State) ? dotColor : new Color(1f, 1f, 1f, 0f);//Color.white;
			_dotFirstOutlineColors[_dotIndex] = Color.black;
			_dotFirstOutlineWidths[_dotIndex] = dotFirstOutlineWidth;

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

			Vector3 planePos = dotPos;
			planePos.y += dotDiameter + 0.01f;
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
			_planeColors[_planeIndex] = new Color(1f, 1f, 1f, (distanceGamma <= 1f)?0f:1f);//dotColor;
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
			_planeIndex++;*/
		}
	}
	private void GenerateUserOverlay()
	{
		Color bodyColor = gradient.Evaluate(bodyGamma); // new Color(1f, 0f, 0f, distanceGamma);
		float bodyAlpha = (bodyGamma > 0f) ? 1f : 0f; // new Color(1f, 0f, 0f, distanceGamma);
		bodyColor.a = bodyAlpha;

		// Generate Left Forearm Zone
		if (currLeftHandCollider != null && currLeftArmCollider != null)
		{
			// Left Hand Background
			Vector3 backLeftHandPos = new Vector3(leftHandPos.x, backgroundDistance, leftHandPos.z);
			_backHandMatrices[_backHandIndex] = Matrix4x4.TRS(backLeftHandPos, Quaternion.identity, leftHandScale);
			_backHandColors[_backHandIndex] = new Vector4(1f, 1f, 1f, 0f); //new Vector4(1f, 1f, 1f, bodyGamma);
			_backHandFirstOutlineColors[_backHandIndex] = new Vector4(0f, 0f, 0f, 0f);
			_backHandFirstOutlineWidths[_backHandIndex] = 0f;
			_backHandSecondOutlineColors[_backHandIndex] = new Vector4(0f, 0f, 0f, 0f);
			_backHandSecondOutlineWidths[_backHandIndex] = 0f;
			_backHandThirdOutlineColors[_backHandIndex] = new Vector4(0f, 0f, 0f, 0f);
			_backHandThirdOutlineWidths[_backHandIndex] = 0f;
			_backHandIndex++;

			// Left Hand Foreground
			Vector3 foreLeftHandPos = new Vector3(leftHandPos.x, foregroundDistance, leftHandPos.z);
			_foreHandMatrices[_foreHandIndex] = Matrix4x4.TRS(foreLeftHandPos, Quaternion.identity, leftHandScale);
			_foreHandColors[_foreHandIndex] = new Vector4(1f, 1f, 1f, 0f); //new Vector4(1f, 1f, 1f, bodyGamma);
			_foreHandFirstOutlineColors[_foreHandIndex] = Color.black;// new Vector4(1f, 0f, 0f, bodyGamma);
			_foreHandFirstOutlineWidths[_foreHandIndex] = bodyFirstOutlineWidth;
			_foreHandSecondOutlineColors[_foreHandIndex] = bodyColor;//new Vector4(1f, 0f, 0f, bodyGamma);
			_foreHandSecondOutlineWidths[_foreHandIndex] = bodySecondOutlineWidth;
			_foreHandThirdOutlineColors[_foreHandIndex] = Color.black;/// new Vector4(1f, 0f, 0f, bodyGamma);
			_foreHandThirdOutlineWidths[_foreHandIndex] = bodyThirdOutlineWidth;
			_foreHandIndex++;


			// Left Arm Background
			Vector3 backLeftArmPos = new Vector3(leftArmPos.x, backgroundDistance, leftArmPos.z);
			_backArmMatrices[_backArmIndex] = Matrix4x4.TRS(
				backLeftArmPos,
				leftArmRotation,
				leftArmScale
				);
			_backArmColors[_backArmIndex] = new Vector4(1f, 1f, 1f, 0f); //new Vector4(1f, 1f, 1f, bodyGamma);
			_backArmFirstOutlineColors[_backArmIndex] = new Vector4(0f, 0f, 0f, 0f);
			_backArmFirstOutlineWidths[_backArmIndex] = 0f;
			_backArmSecondOutlineColors[_backArmIndex] = new Vector4(0f, 0f, 0f, 0f);
			_backArmSecondOutlineWidths[_backArmIndex] = 0f;
			_backArmThirdOutlineColors[_backArmIndex] = new Vector4(0f, 0f, 0f, 0f);
			_backArmThirdOutlineWidths[_backArmIndex] = 0f;
			_backArmIndex++;

			// Left Arm Foreground
			Vector3 foreLeftArmPos = new Vector3(leftArmPos.x, foregroundDistance, leftArmPos.z);
			_foreArmMatrices[_foreArmIndex] = Matrix4x4.TRS(
				foreLeftArmPos,
				leftArmRotation,
				leftArmScale
				);
			_foreArmColors[_foreArmIndex] = new Vector4(1f, 1f, 1f, 0f); //new Vector4(1f, 1f, 1f, bodyGamma);
			_foreArmFirstOutlineColors[_foreArmIndex] = Color.black;// new Vector4(1f, 0f, 0f, bodyGamma);
			_foreArmFirstOutlineWidths[_foreArmIndex] = bodyFirstOutlineWidth;
			_foreArmSecondOutlineColors[_foreArmIndex] = bodyColor;// new Vector4(1f, 0f, 0f, bodyGamma);
			_foreArmSecondOutlineWidths[_foreArmIndex] = bodySecondOutlineWidth;
			_foreArmThirdOutlineColors[_foreArmIndex] = Color.black;// new Vector4(1f, 0f, 0f, bodyGamma);
			_foreArmThirdOutlineWidths[_foreArmIndex] = bodyThirdOutlineWidth;
			_foreArmIndex++;
		}
		// Generate Right Hand Zone
		if (currRightHandCollider != null && currRightArmCollider != null)
		{
			// Right Hand Background
			Vector3 backRightHandPos = new Vector3(rightHandPos.x, backgroundDistance, rightHandPos.z);
			_backHandMatrices[_backHandIndex] = Matrix4x4.TRS(backRightHandPos, Quaternion.identity, rightHandScale);
			_backHandColors[_backHandIndex] = new Vector4(1f, 1f, 1f, 0f); //new Vector4(1f, 1f, 1f, bodyGamma);
			_backHandFirstOutlineColors[_backHandIndex] = new Vector4(0f, 0f, 0f, 0f);
			_backHandFirstOutlineWidths[_backHandIndex] = 0f;
			_backHandSecondOutlineColors[_backHandIndex] = new Vector4(0f, 0f, 0f, 0f);
			_backHandSecondOutlineWidths[_backHandIndex] = 0f;
			_backHandThirdOutlineColors[_backHandIndex] = new Vector4(0f, 0f, 0f, 0f);
			_backHandThirdOutlineWidths[_backHandIndex] = 0f;
			_backHandIndex++;

			// Right Hand Foreground
			Vector3 foreRightHandPos = new Vector3(rightHandPos.x, foregroundDistance, rightHandPos.z);
			_foreHandMatrices[_foreHandIndex] = Matrix4x4.TRS(foreRightHandPos, Quaternion.identity, rightHandScale);
			_foreHandColors[_foreHandIndex] = new Vector4(1f, 1f, 1f, 0f); //new Vector4(1f, 1f, 1f, bodyGamma);
			_foreHandFirstOutlineColors[_foreHandIndex] = Color.black;// new Vector4(1f, 0f, 0f, bodyGamma);
			_foreHandFirstOutlineWidths[_foreHandIndex] = bodyFirstOutlineWidth;
			_foreHandSecondOutlineColors[_foreHandIndex] = bodyColor;// new Vector4(1f, 0f, 0f, bodyGamma);
			_foreHandSecondOutlineWidths[_foreHandIndex] = bodySecondOutlineWidth;
			_foreHandThirdOutlineColors[_foreHandIndex] = Color.black;// new Vector4(1f, 0f, 0f, bodyGamma);
			_foreHandThirdOutlineWidths[_foreHandIndex] = bodyThirdOutlineWidth;
			_foreHandIndex++;


			// Right Arm Background
			Vector3 backRightArmPos = new Vector3(rightArmPos.x, backgroundDistance, rightArmPos.z);
			_backArmMatrices[_backArmIndex] = Matrix4x4.TRS(
				backRightArmPos,
				rightArmRotation,
				rightArmScale
				);
			_backArmColors[_backArmIndex] = new Vector4(1f, 1f, 1f, 0f); //new Vector4(1f, 1f, 1f, bodyGamma);
			_backArmFirstOutlineColors[_backArmIndex] = new Vector4(0f, 0f, 0f, 0f);
			_backArmFirstOutlineWidths[_backArmIndex] = 0f;
			_backArmSecondOutlineColors[_backArmIndex] = new Vector4(0f, 0f, 0f, 0f);
			_backArmSecondOutlineWidths[_backArmIndex] = 0f;
			_backArmThirdOutlineColors[_backArmIndex] = new Vector4(0f, 0f, 0f, 0f);
			_backArmThirdOutlineWidths[_backArmIndex] = 0f;
			_backArmIndex++;

			// Right Arm Foreground
			Vector3 foreRightArmPos = new Vector3(rightArmPos.x, foregroundDistance, rightArmPos.z);
			_foreArmMatrices[_foreArmIndex] = Matrix4x4.TRS(
				foreRightArmPos,
				rightArmRotation,
				rightArmScale
				);
			_foreArmColors[_foreArmIndex] = new Vector4(1f, 1f, 1f, 0f); //new Vector4(1f, 1f, 1f, bodyGamma);
			_foreArmFirstOutlineColors[_foreArmIndex] = Color.black;// new Vector4(1f, 0f, 0f, bodyGamma);
			_foreArmFirstOutlineWidths[_foreArmIndex] = bodyFirstOutlineWidth;
			_foreArmSecondOutlineColors[_foreArmIndex] = bodyColor;// new Vector4(1f, 0f, 0f, bodyGamma);
			_foreArmSecondOutlineWidths[_foreArmIndex] = bodySecondOutlineWidth;
			_foreArmThirdOutlineColors[_foreArmIndex] = Color.black;// new Vector4(1f, 0f, 0f, bodyGamma);
			_foreArmThirdOutlineWidths[_foreArmIndex] = bodyThirdOutlineWidth;
			_foreArmIndex++;
		}


		for (int i = 0; i < currUnsafeTransitions.Count; i++)
		{

			Vector3 unsafeTransition = currUnsafeTransitions[i];
			int row = (int)unsafeTransition.x;
			int column = (int)unsafeTransition.y;
			float distanceGamma = unsafeTransition.z;
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


			dotPos = new Vector3(pin.position.x, 0f, pin.position.z);
			Vector3 dotScale = new Vector3(dotDiameter, 0.01f, dotDiameter);
			_dotMatrices[_dotIndex] = Matrix4x4.TRS(
				dotPos,
				dotRot,
				dotScale
			);

			//Color dotColor = (displacement > 0) ? Color.Lerp(_middleDivergingColor, _rightDivergingColor, displacement / 40f) : Color.Lerp(_middleDivergingColor, _leftDivergingColor, -displacement / 40f);
			Color dotColor = gradient.Evaluate(distanceGamma); // new Color(1f, 0f, 0f, distanceGamma);
			float safetyDiameter = pins.diameter + MyCapsuleHand.STOP_RADIUS * 2f;
			float safetyRadius = safetyDiameter / 2.0f;

			_dotColors[_dotIndex] = dotColor; //(feedbackMode != FeedbackMode.State) ? dotColor : new Color(1f, 1f, 1f, 0f);//Color.white;
			_dotFirstOutlineColors[_dotIndex] = Color.black;
			_dotFirstOutlineWidths[_dotIndex] = dotFirstOutlineWidth;
			/*_dotSecondOutlineColors[_dotIndex] = Color.black;
			_dotSecondOutlineWidths[_dotIndex] = 0.0f;
			_dotThirdOutlineColors[_dotIndex] = Color.black;
			_dotThirdOutlineWidths[_dotIndex] = 0.0f;
			_dotFourthOutlineColors[_dotIndex] = Color.black;
			_dotFourthOutlineWidths[_dotIndex] = 0.0f;
			_dotFifthOutlineColors[_dotIndex] = Color.black;
			_dotFifthOutlineWidths[_dotIndex] = 0.0f;*/

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

			Vector3 planePos = new Vector3(pin.position.x, 0.03f, pin.position.z);
			Quaternion planeRot = pin.rotation * Quaternion.AngleAxis(90, pin.up);
			Vector3 planeScale = new Vector3(minOrthographicSize, 0.01f, minOrthographicSize);
			_planeMatrices[_planeIndex] = Matrix4x4.TRS(
					planePos,
					planeRot,
					planeScale
			);
			float displacementPercent = Mathf.InverseLerp(-40f, 40f, displacement);
			int directionAmount = Mathf.RoundToInt(Mathf.Lerp(0f, 30f, displacementPercent));
			Graphics.CopyTexture(_shapeTextures[directionAmount], 0, 0, _iconTextureArray, _planeIndex, 0); // i is the index of the texture
			_iconTextureIndexes[_planeIndex] = _planeIndex;
			_planeColors[_planeIndex] = new Color(1f, 1f, 1f, (distanceGamma <1f)?0f:1f);//dotColor;
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
	// Update is called once per frame
	public void Update()
    {
		_backHandIndex = _foreHandIndex =  _backArmIndex = _foreArmIndex = _pinIndex = _planeIndex = _dotIndex = 0;

		// Handle Overlay Modes à priori
		switch (overlayMode)
		{
			case SafetyOverlayMode.User:
				// set outlines width
				minOrthographicSize = pins.diameter/2f; // -1.5f / 2f;
				maxOrthographicSize = minOrthographicSize * (3.3f / 100f); // 3.3f / 3.3f;
				minOutlineWidth = 0.02f;
				maxOutlineWidth = minOutlineWidth * (3.3f / 100f);
				minSecondOutlineWidth = 0.02f;
				maxSecondOutlineWidth = minSecondOutlineWidth * (3.3f / 100f);
				minThirdOutlineWidth = 0.02f;
				maxThirdOutlineWidth = minThirdOutlineWidth * (3.3f / 100f);
				minFourthOutlineWidth = 0.02f;
				maxFourthOutlineWidth = minFourthOutlineWidth * (3.3f / 100f);
				minFifthOutlineWidth = 0.02f;
				maxFifthOutlineWidth = minFifthOutlineWidth * (3.3f / 100f);
				break;
			case SafetyOverlayMode.System:
				// set outlines width
				minOrthographicSize = pins.diameter / 2f; // -1.5f / 2f;
				maxOrthographicSize = minOrthographicSize * (3.3f / 100f); // 3.3f / 3.3f;
				minOutlineWidth = 0.02f;
				maxOutlineWidth = minOutlineWidth * (3.3f / 100f);
				minSecondOutlineWidth = 0.02f;
				maxSecondOutlineWidth = minSecondOutlineWidth * (3.3f / 100f);
				minThirdOutlineWidth = 0.02f;
				maxThirdOutlineWidth = minThirdOutlineWidth * (3.3f / 100f);
				minFourthOutlineWidth = 0.02f;
				maxFourthOutlineWidth = minFourthOutlineWidth * (3.3f / 100f);
				minFifthOutlineWidth = 0.02f;
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
			if (pins.LeftArm != null && pins.LeftArm.IsActive())
			{

				currLeftHandCollider = pins.LeftArm.GetHandColliderAt(0);
				currLeftArmCollider =  pins.LeftArm.GetArmColliderAt(0);
			}
			if (pins.RightArm != null && pins.RightArm.IsActive())
			{

				currRightHandCollider = pins.RightArm.GetHandColliderAt(0);
				currRightArmCollider = pins.RightArm.GetArmColliderAt(0);
			}
		}
		FetchUserBody();
		nextUnsafeTransitions.Clear();

		float dynamicFeedbackMaxGamma = 0f;
		for (int row = 0; row < pins.NbRows; row++)
		{
			for (int column = 0; column < pins.NbColumns; column++)
			{
				int paused = pins.viewMatrix[row, column].CurrentPaused;
				int feedforwarded = pins.viewMatrix[row, column].CurrentFeedForwarded;
				Transform pin = pins.viewMatrix[row, column].transform;
				bool reaching = pins.viewMatrix[row, column].CurrentReaching;
				int displacement = (paused != 0) ? paused : feedforwarded;
				if (reaching || displacement != 0) // if moving pins or paused or feedforwarded
				{

					float distanceGamma = pins.collisionMatrix[row, column].Gamma();
					float distance = pins.collisionMatrix[row, column].Distance();

					if (distanceGamma > 0f)
					{
						float finalGamma = distanceGamma;// (distanceGamma < 1f) ? 0f: 1f; // (distanceGamma < 1f) ? 0.6f : distanceGamma; 
						dynamicFeedbackMaxGamma = Mathf.Max(finalGamma, dynamicFeedbackMaxGamma);
						//Debug.Log(row + " " + column + " finalGamma:" + finalGamma);
						Vector3 nextUnsafeTransition = new Vector3(row, column, finalGamma);
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
		}
		List<Vector3> transitionsToRemove = new List<Vector3>();
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
					transitionsToRemove.Add(currUnsafeTransition);
				}
			}
		}
		currUnsafeTransitions.RemoveAll(currUnsafeTransition => transitionsToRemove.Contains(currUnsafeTransition));

		if (nextUnsafeTransitions.Count == 0) bodyGamma = Mathf.MoveTowards(bodyGamma, feedbackMinGamma, recoveryRateOut * Time.deltaTime);
		else bodyGamma = Mathf.MoveTowards(bodyGamma, dynamicFeedbackMaxGamma, recoveryRateIn * Time.deltaTime);

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

			/*for (int i = 0; i < _dotIndex; i++)
			{
				_dotFirstOutlineColors[i] = new Vector4(1f, 1f, 1f, 0f);
				_dotSecondOutlineColors[i] = new Vector4(1f, 1f, 1f, 0f);
				_dotThirdOutlineColors[i] = new Vector4(1f, 1f, 1f, 0f);
			}*/
			Render();
		}

	}
}
