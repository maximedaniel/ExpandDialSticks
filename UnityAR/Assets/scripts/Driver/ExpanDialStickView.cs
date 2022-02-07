using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using TMPro;
using System;
using UnityEngine.Rendering;

public class ExpanDialStickView : MonoBehaviour
{
	private float diameter = 4.0f;
	private float height = 10.0f;
	private const float maxHeight = 0.10f;
	private float offset = 0.5f;
	private int nbSeparationLevels = 2;


	private int i = 0;
	private int j = 0;

	private float xAxisCurrent = 0f;
	private float xAxisDiff = 0f;
	private float xAxisTarget = 0f;

	private float yAxisCurrent = 0f;
	private float yAxisDiff = 0f;
	private float yAxisTarget = 0f;

	private float selectCountCurrent = 0f;	
	private float selectCountDiff = 0f;
	private float selectCountTarget = 0f;

	private float rotationCurrent = 0f;
	private float rotationDiff = 0f;
	private float rotationTarget = 0f;

	private float positionCurrent = 0f;
	private float positionDiff = 0f;
	private float positionTarget = 0f;

	private float reachingCurrent = 0f;
	private float reachingDiff = 0f;
	private float reachingTarget = 0f;

	private float holdingCurrent = 0f;
	private float holdingDiff = 0f;
	private float holdingTarget = 0f;

	private float separationLevelTarget = 0f;
	private float separationLevelCurrent = 0f;
	private float separationLevelDiff = 0f;

	private float proximityCurrent = 0f;
	private float proximityDiff = 0f;
	private float proximityTarget = 0f;

	private float distanceCurrent = 0f;
	private float distanceDiff = 0f;
	private float distanceTarget = 0f;


	private float pauseCurrent = 0f;
	private float pauseDiff = 0f;
	private float pauseTarget = 0f;

	private float feedforwardCurrent = 0f;
	private float feedforwardDiff = 0f;
	private float feedforwardTarget = 0f;


	private float shapeChangeDurationCurrent = 0f;
	private float shapeChangeDurationDiff = 0f;
	private float shapeChangeDurationTarget = 0f;


	private Color colorCurrent = Color.white;
	private Color colorDiff = Color.black;
	private Color colorTarget = Color.white;

	private Color reverseColorCurrent = Color.black;

	private TextAlignmentOptions textAlignmentCurrent = TextAlignmentOptions.Center;
	private TextAlignmentOptions textAlignmentTarget = TextAlignmentOptions.Center;

	private float textSizeCurrent = 1f;
	private float textSizeDiff = 0f;
	private float textSizeTarget = 1f;

	private float textRotationCurrent = 0f;
	private float textRotationDiff = 0f;
	private float textRotationTarget = 0f;

	private Color textColorCurrent = Color.black;
	private Color textColorDiff = Color.white;
	private Color textColorTarget = Color.black;
	
	private string textCurrent = "";
	private string textTarget = "";

	private float textureChangeDurationCurrent = 0f;
	private float textureChangeDurationDiff = 0f;
	private float textureChangeDurationTarget = 0f;

	private Color projectorColorCurrent = Color.black;
	private Color projectorColorTarget = Color.black;
	private string projectorTextureCurrent = "";
	private string projectorTextureTarget = "";
	private Vector2 projectorOffsetCurrent = Vector2.zero;
	private Vector2 projectorOffsetTarget = Vector2.zero;
	private float projectorRotationCurrent = 0f;
	private float projectorRotationTarget = 0f;
	private float projectorSizeCurrent = 0f;
	private float projectorSizeTarget = 0f;
	private float projectorChangeDurationCurrent = 0f;
	private float projectorChangeDurationTarget = 0f;

	public Material transparentMaterial;
	
	private GameObject textGameObject;
	private TextMeshPro textMesh;
	private RectTransform textRectTransform;

	
	private GameObject planeGameObject;
	public Material planeMaterial;
	private MeshRenderer planeMeshRenderer;
	private string planeTextureCurrent = "";
	private string planeTextureTarget = "";
	private Color planeColorCurrent = Color.black;
	private Color planeColorTarget = Color.black;
	private Vector2 planeOffsetCurrent = Vector2.zero;
	private Vector2 planeOffsetTarget = Vector2.zero;
	private float planeRotationTarget= 1f;
	private float planeRotationCurrent= 1f;
	private float planeSizeCurrent = 0f;
	private float planeSizeTarget = 0f;
	private bool paused = false;

	private MeshRenderer meshRenderer;
	private GameObject projectorGameObject;
	private Projector projector;
	public Material projectorMaterial;



	/*private string projectorTexture = "projector";
	private float feedbackInDuration = 1f;
	private float feedbackOutDuration = 0.250f;
	private float feedbackMinGamma = 0f;
	private float feedbackMaxGamma = 1f;
	private float feedbackMinOrthographicSize =  3.2f; 
	private float feedbackMaxOrthographicSize = 9.6f;
	private float delayPerRow = 0f;*/
	public int TargetFeedForwarded
	{
		get => (int)Mathf.Round(this.feedforwardTarget);
		set => this.feedforwardTarget = value;
	}

	public int CurrentFeedForwarded
	{
		get => (int)Mathf.Round(this.feedforwardCurrent);
		set
		{
			this.feedforwardDiff += value - this.feedforwardCurrent;
			this.feedforwardCurrent = value;
		}
	}

	public bool Paused
	{
		get => this.paused;
		set => this.paused = value;
	}
	// Getters and Setters
	public int Row{
		get => this.i;
		set => this.i = value;
	}

	public int Column{
		get => this.j;
		set => this.j = value;
	}

	public float Diameter{
		get => this.diameter;
		set => this.diameter = value;
	}

	public float Height{
		get => this.height;
		set => this.height = value;
	}

	public float Offset{
		get => this.offset;
		set => this.offset = value;
	}
	public int NbSeparationLevels
	{
		get => this.nbSeparationLevels;
		set => this.nbSeparationLevels = value;
	}

	public sbyte TargetAxisX{
		get => (sbyte)Mathf.Round(this.xAxisTarget);
		set => this.xAxisTarget = value;
	}

	public sbyte CurrentAxisX{
		get => (sbyte)Mathf.Round(this.xAxisCurrent);
		set {
			this.xAxisDiff += value - this.xAxisCurrent;
			this.xAxisCurrent = value;
		}
	}

	public sbyte TargetAxisY{
		get => (sbyte)Mathf.Round(this.yAxisTarget);
		set => this.yAxisTarget = value;
	}

	public sbyte CurrentAxisY{
		get => (sbyte)Mathf.Round(this.yAxisCurrent);
		set {
			this.yAxisDiff += value - this.yAxisCurrent;
			this.yAxisCurrent = value;
		}
	}

	public byte TargetSelectCount{
		get => (byte)Mathf.Round(this.selectCountTarget);
		set => this.selectCountTarget = value;
	}

	public byte CurrentSelectCount{
		get => (byte)Mathf.Round(this.selectCountCurrent);
		set {
			this.selectCountDiff += this.selectCountCurrent <= value ? value - this.selectCountCurrent : 255 + value - this.selectCountCurrent;
			this.selectCountCurrent = value;
		}
	}
	
	public sbyte TargetRotation{
		get => (sbyte)Mathf.Round(this.rotationTarget);
		set => this.rotationTarget = value;
	}
	
	public sbyte CurrentRotation{
		get => (sbyte)Mathf.Round(this.rotationCurrent);
		set {
			this.rotationDiff += Mathf.Abs(value - this.rotationCurrent) <= 127 ? value - this.rotationCurrent : 255 + value - this.rotationCurrent;
			this.rotationCurrent = value;
		}
	}
	
	public sbyte TargetPosition{
		get => (sbyte)Mathf.Round(this.positionTarget);
		set => this.positionTarget = value;
	}
	
	public sbyte CurrentPosition{
		get => (sbyte)Mathf.Round(this.positionCurrent);
		set {
			this.positionDiff += (CurrentReaching) ? 0 :  value - this.positionCurrent;
			this.positionCurrent = value;
		}
	}

	public bool TargetReaching{
		get => this.reachingTarget > 0f ? true:false;
		set => this.reachingTarget = value ? 1f : 0f;
	}

	public bool CurrentReaching{
		get => this.reachingCurrent > 0f ? true:false;
		set {
			this.reachingDiff += (value ? 1f : 0f) - this.reachingCurrent;
			this.reachingCurrent = value ? 1f : 0f;
		}
	}

	public bool TargetHolding{
		get => this.holdingTarget > 0f ? true:false;
		set => this.holdingTarget = value ? 1f : 0f;
	}

	public bool CurrentHolding{
		get => this.holdingCurrent > 0f ? true:false;
		set {
			this.holdingDiff += (value ? 1f : 0f) - this.holdingCurrent;
			this.holdingCurrent = value ? 1f : 0f;
		}
	}
	public int TargetSeparationLevel
	{
		get => (int)this.separationLevelTarget;
		set => this.separationLevelTarget = value;
	}
	public int CurrentSeparationLevel
	{
		get => (int)this.separationLevelCurrent;
		set
		{
			this.separationLevelDiff += value - this.separationLevelCurrent;
			this.separationLevelCurrent = value;
		}
	}


	public float TargetProximity
	{
		get => this.proximityTarget;
		set => this.proximityTarget = value;
	}

	public float CurrentProximity
	{
		get => this.proximityCurrent;
		set
		{
			this.proximityDiff += value - this.proximityCurrent;
			this.proximityCurrent = value;
		}
	}
	public float TargetDistance
	{
		get => this.distanceTarget;
		set => this.distanceTarget = value;
	}

	public float CurrentDistance
	{
		get => this.distanceCurrent;
		set
		{
			this.distanceDiff += value - this.distanceCurrent;
			this.distanceCurrent = value;
		}
	}
	public int TargetPaused
	{
		get => (int)Mathf.Round(this.pauseTarget);
		set => this.pauseTarget = value;
	}

	public int CurrentPaused
	{
		get => (int)Mathf.Round(this.pauseCurrent);
		set
		{
			this.pauseDiff += value - this.pauseCurrent;
			this.pauseCurrent = value;
		}
	}

	public float TargetShapeChangeDuration{
		get => this.shapeChangeDurationTarget;
		set => this.shapeChangeDurationTarget = value;
	}

	public float CurrentShapeChangeDuration{
		get => this.shapeChangeDurationCurrent;
		set {
			this.shapeChangeDurationDiff += value - this.shapeChangeDurationCurrent;
			this.shapeChangeDurationCurrent = value;
		}
	}
	// Texture Change

	public string TargetPlaneTexture{
		get => this.planeTextureTarget;
		set => this.planeTextureTarget = value;
	}
	
	public string CurrentPlaneTexture{
		get => this.planeTextureCurrent;
		set => this.planeTextureCurrent = value;
	}
	public Color CurrentPlaneColor
	{
		get => this.planeColorCurrent;
		set => this.planeColorCurrent = value;
	}
	public Color TargetPlaneColor
	{
		get => this.planeColorTarget;
		set => this.planeColorTarget = value;
	}
	public Vector2 CurrentPlaneOffset
	{
		get => this.planeOffsetCurrent;
		set => this.planeOffsetCurrent = value;
	}

	public Vector2 TargetPlaneOffset
	{
		get => this.planeOffsetTarget;
		set => this.planeOffsetTarget = value;
	}
	public float CurrentPlaneSize
	{
		get => this.planeSizeCurrent;
		set => this.planeSizeCurrent = value;
	}
	public float TargetPlaneSize
	{
		get => this.planeSizeTarget;
		set => this.planeSizeTarget = value;
	}
	public float TargetPlaneRotation{
		get => this.planeRotationTarget;
		set => this.planeRotationTarget = value;
	}
	
	public float CurrentPlaneRotation{
		get => this.planeRotationCurrent;
		set => this.planeRotationCurrent = value;
	}

	public TextAlignmentOptions TargetTextAlignment{
		get => this.textAlignmentTarget;
		set => this.textAlignmentTarget = value;
	}

	public TextAlignmentOptions CurrentTextAlignment{
		get => this.textAlignmentCurrent;
		set => this.textAlignmentCurrent = value;
	}
	
	public float TargetTextSize{
		get => this.textSizeTarget;
		set => this.textSizeTarget = value;
	}
	
	public float CurrentTextSize{
		get => this.textSizeCurrent;
		set {
			this.textSizeDiff += value - this.textSizeCurrent;
			this.textSizeCurrent = value;
		}
	}

	public float TargetTextRotation
	{
		get => this.textRotationTarget;
		set => this.textRotationTarget = value;
	}

	public float CurrentTextRotation
	{
		get => this.textRotationCurrent;
		set
		{
			this.textRotationDiff += value - this.textRotationCurrent;
			this.textRotationCurrent = value;
		}
	}

	public Color TargetTextColor{
		get => this.textColorTarget;
		set => this.textColorTarget = value;
	}
	
	public Color CurrentTextColor{
		get => this.textColorCurrent;
		set {
			this.textColorDiff += value - this.textColorCurrent;
			this.textColorCurrent = value;
		}
	}
	

	public string TargetText{
		get => this.textTarget;
		set => this.textTarget = value;
	}
	
	public string CurrentText{
		get => this.textCurrent;
		set => this.textCurrent = value;
	}

	public Color TargetColor{
		get => this.colorTarget;
		set => this.colorTarget = value;
	}

	public Color CurrentColor{
		get => this.colorCurrent;
		set {
			this.colorDiff += value - this.colorCurrent;
			this.colorCurrent = value;
		}
	}
	
	public float TargetTextureChangeDuration{
		get => this.textureChangeDurationTarget;
		set => this.textureChangeDurationTarget = value;
	}

	public float CurrentTextureChangeDuration{
		get => this.textureChangeDurationCurrent;
		set {
			this.textureChangeDurationDiff += value - this.textureChangeDurationCurrent;
			this.textureChangeDurationCurrent = value;
		}
	}
	public Color CurrentProjectorColor{
		get => this.projectorColorCurrent;
		set => this.projectorColorCurrent = value;
	}
	public Color TargetProjectorColor{
		get => this.projectorColorTarget;
		set => this.projectorColorTarget = value;
	}
	public string CurrentProjectorTexture{
		get => this.projectorTextureCurrent;
		set => this.projectorTextureCurrent = value;
	}
	public string TargetProjectorTexture{
		get => this.projectorTextureTarget;
		set => this.projectorTextureTarget = value;
	}
	public Vector2 CurrentProjectorOffset
	{
		get => this.projectorOffsetCurrent;
		set => this.projectorOffsetCurrent = value;
	}
	public Vector2 TargetProjectorOffset
	{
		get => this.projectorOffsetTarget;
		set => this.projectorOffsetTarget = value;
	}
	public float CurrentProjectorRotation{
		get => this.projectorRotationCurrent;
		set => this.projectorRotationCurrent = value;
	}
	public float TargetProjectorRotation{
		get => this.projectorRotationTarget;
		set => this.projectorRotationTarget = value;
	}
	public float CurrentProjectorSize{
		get => this.projectorSizeCurrent;
		set => this.projectorSizeCurrent = value;
	}
	public float TargetProjectorSize{
		get => this.projectorSizeTarget;
		set => this.projectorSizeTarget = value;
	}
	public float CurrentProjectorChangeDuration{
		get => this.projectorChangeDurationCurrent;
		set => this.projectorChangeDurationCurrent = value;
	}
	public float TargetProjectorChangeDuration{
		get => this.projectorChangeDurationTarget;
		set => this.projectorChangeDurationTarget = value;
	}

	public void setShapeChangeTarget(sbyte xAxis, sbyte yAxis, byte selectCount, sbyte rotation, sbyte position, bool reaching, bool holding, int separationLevel, float proximity, float distance, int paused, float shapeChangeDuration)
	{
		TargetAxisX = xAxis;
		TargetAxisY = yAxis;
		TargetSelectCount = selectCount;
		TargetRotation = rotation;
		TargetReaching = reaching;
		TargetHolding = holding;
		TargetSeparationLevel = separationLevel;
		TargetProximity = proximity;
		TargetDistance = distance;
		TargetPaused = paused;
		TargetPosition = position;
		TargetShapeChangeDuration = shapeChangeDuration;
	}
	
	public void setShapeChangeCurrent(sbyte xAxis, sbyte yAxis, byte selectCount, sbyte rotation, sbyte position, bool reaching, bool holding, int separationLevel, float proximity, float distance, int paused, float shapeChangeDuration)
	{
		CurrentAxisX = xAxis;
		CurrentAxisY = yAxis;
		CurrentSelectCount = selectCount;
		CurrentRotation = rotation;
		CurrentHolding = holding;
		CurrentSeparationLevel = separationLevel;
		CurrentProximity = proximity;
		CurrentDistance = distance;
		CurrentPaused = paused;

		if (this.reachingCurrent > 0f  && reaching == false){
			CurrentPosition = position;
			CurrentReaching = reaching;
		} else {
			CurrentReaching = reaching;
			CurrentPosition = position;
		}
		
		CurrentShapeChangeDuration = shapeChangeDuration;
	}
	public void setProjectorChangeTarget(Color color, string textureName, Vector2 textureOffset, float textureRotation, float textureSize, float projectorChangeDuration)
	{
		TargetProjectorColor = color;
		TargetProjectorTexture = textureName;
		TargetProjectorOffset = textureOffset;
		TargetProjectorRotation = textureRotation;
		TargetProjectorSize = textureSize;
		TargetProjectorChangeDuration = projectorChangeDuration;
	}
	public void setProjectorChangeCurrent(Color color, string textureName, Vector2 textureOffset, float textureRotation, float textureSize, float projectorChangeDuration)
	{
		CurrentProjectorColor = color;
		CurrentProjectorTexture = textureName;
		CurrentProjectorOffset = textureOffset;
		CurrentProjectorRotation = textureRotation;
		CurrentProjectorSize = textureSize;
		CurrentProjectorChangeDuration = projectorChangeDuration;
	}
	public void setTextureChangeTarget(Color color, string textureName, Color textureColor, float textureSize, Vector2 textureOffset, float textureRotation, float textureChangeDuration)
	{
		TargetColor = color;
		TargetPlaneTexture = textureName;
		TargetPlaneColor = textureColor;
		TargetPlaneOffset = textureOffset;
		TargetPlaneSize = textureSize;
		TargetPlaneRotation = textureRotation;
		TargetTextureChangeDuration = textureChangeDuration;
	}

	public void setTextureChangeCurrent(Color color, string textureName, Color textureColor, float textureSize, Vector2 textureOffset, float textureRotation, float textureChangeDuration)
	{
		CurrentColor = color;
		CurrentPlaneTexture = textureName;
		CurrentPlaneColor = textureColor;
		CurrentPlaneOffset = textureOffset;
		CurrentPlaneSize = textureSize;
		CurrentPlaneRotation = textureRotation;
		CurrentTextureChangeDuration = textureChangeDuration;
	}

	public void setTextChangeTarget(Color color, TextAlignmentOptions textAlignment, float textSize, float textRotation, Color textColor, string text, float textureChangeDuration )
	{
		TargetColor = color;
		TargetTextAlignment = textAlignment;
		TargetTextSize = textSize;
		TargetTextRotation = textRotation;
		TargetTextColor = textColor;
		TargetText = text;
		TargetTextureChangeDuration = textureChangeDuration;
	}

	public void setTextChangeCurrent(Color color, TextAlignmentOptions textAlignment, float textSize, float textRotation, Color textColor, string text, float textureChangeDuration )
	{
		CurrentColor = color;
		CurrentTextAlignment = textAlignment;
		CurrentTextSize = textSize;
		CurrentTextRotation = textRotation;
		CurrentTextColor = textColor;
		CurrentText = text;
		CurrentTextureChangeDuration = textureChangeDuration;
	}

	public void setSafetyChange(int feedForwarded)
	{
		CurrentFeedForwarded = feedForwarded;
	}


	public float[] readShapeDiffs()
	{
		return new float[]{
			this.xAxisDiff,
			this.yAxisDiff,
			this.selectCountDiff,
			this.rotationDiff,
			this.positionDiff,
			this.reachingDiff,
			this.holdingDiff,
			this.separationLevelDiff,
			this.proximityDiff,
			this.distanceDiff,
			this.shapeChangeDurationDiff
		};
	}

	public void eraseShapeDiffs()
	{
		this.xAxisDiff
			= this.yAxisDiff
			= this.selectCountDiff
			= this.rotationDiff
			= this.positionDiff
			= this.reachingDiff
			= this.holdingDiff
			= this.separationLevelDiff
			= this.proximityDiff
			= this.distanceDiff
			= this.shapeChangeDurationDiff
			= 0f;
	}
	
	public float [] readAndEraseShapeDiffs()
	{
		float[] ans = readShapeDiffs();
		eraseShapeDiffs();
		return ans;
	}
	
	// Start is called before the first frame update
	void Start()
    {
		meshRenderer = this.transform.GetComponent<MeshRenderer>();
		meshRenderer.material = transparentMaterial;
		this.name = "ExpanDialStick (" + i + ", " + j + ")";

		projectorGameObject = this.transform.GetChild(0).gameObject;
		projector = projectorGameObject.GetComponent<Projector>();
		projector.material  = new Material(projectorMaterial);

		textGameObject = new GameObject( "Text (" + i + ", " + j + ")");
		textGameObject.transform.parent = this.transform;

		textMesh = textGameObject.AddComponent<TextMeshPro>();
		textMesh.alignment = TextAlignmentOptions.Center;
		textMesh.fontSize = 1;
		textMesh.color = Color.black;
		textMesh.text = "(" + i + ", " + j + ")";
		textRectTransform = textGameObject.GetComponent<RectTransform>();
		textRectTransform.position += new Vector3(0f, 1.01f, 0f);
		textRectTransform.Rotate(90f, 0f, 0f, Space.Self);

		// Image Plane
		planeGameObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		Destroy(planeGameObject.GetComponent<CapsuleCollider>());
		//Destroy(planeGameObject.GetComponent<MeshCollider>());
		planeGameObject.transform.parent = this.transform;
		planeGameObject.transform.position += new Vector3(0f, 1.01f, 0f);
		planeGameObject.transform.localScale  = new Vector3(this.planeSizeCurrent, 0.01f, this.planeSizeCurrent);
		planeMeshRenderer = planeGameObject.GetComponent<MeshRenderer> ();
		planeMeshRenderer.material = planeMaterial;
		planeMeshRenderer.material.mainTexture = Resources.Load<Texture2D>("default");

	}

	void render()
	{
		float positionCurrentInverseLerp = Mathf.InverseLerp(0f, 40f, this.positionCurrent);
		float positionCurrentLerp = Mathf.Lerp(0f, maxHeight, positionCurrentInverseLerp);
		this.transform.position = new Vector3(i * (diameter + offset), positionCurrentLerp, j * (diameter + offset));

		this.transform.localScale = new Vector3(diameter, height / 2, diameter);
		
		this.transform.rotation = Quaternion.identity;
		//float rotationCurrentInverseLerp = Mathf.InverseLerp(-127f, 127f, this.rotationCurrent);
		//float rotationCurrentLerp = Mathf.Lerp(-360f * 8, 360f * 8, rotationCurrentInverseLerp) % 360f;
		//this.transform.RotateAround(this.transform.position - new Vector3(0f, height / 2, 0f), Vector3.up, rotationCurrentLerp);
		
		float yAxisCurrentInverseLerp = Mathf.InverseLerp(-127f, 127f, this.yAxisCurrent);
		float yAxisCurrentLerp = Mathf.Lerp(-30f, 30f, yAxisCurrentInverseLerp);
		this.transform.RotateAround(this.transform.position - new Vector3(0f, height / 2, 0f), Vector3.left, yAxisCurrentLerp);
		
		float xAxisCurrentInverseLerp = Mathf.InverseLerp(-127f, 127f, this.xAxisCurrent);
		float xAxisCurrentLerp = Mathf.Lerp(-30f, 30f, xAxisCurrentInverseLerp);
		this.transform.RotateAround(this.transform.position - new Vector3(0f, height / 2, 0f), Vector3.back, xAxisCurrentLerp);



		// SAFETY CUE
		/*if (this.pauseCurrent != 0f || this.safetyFeedForwardEnabled) // || this.CurrentProximity >= 1f // this.positionCurrent > 0f)//  || (this.colorCurrent != Color.white))
		{
			if (this.projectorTextureCurrent != projectorTexture){
				projector.material.mainTexture = Resources.Load<Texture2D>(projectorTexture);
			}

			float maxScaleDistance = feedbackMaxOrthographicSize;
			float minScaleDistance = 0f;
			float scaleDistanceCoeff = 1f - (Mathf.Clamp(this.distanceCurrent, minScaleDistance, maxScaleDistance) - minScaleDistance) / (maxScaleDistance - minScaleDistance);

			float maxAlphaDistance = 2f * feedbackMaxOrthographicSize;
			float minAlphaDistance = 0f;
			float alphaDistanceCoeff = 1f - (Mathf.Clamp(this.distanceCurrent, minAlphaDistance, maxAlphaDistance) - minAlphaDistance) / (maxAlphaDistance - minAlphaDistance);

			projector.material.renderQueue = 3000 + (int)((1f - scaleDistanceCoeff) * 1000);

			switch (feedbackMode)
			{
				case FeedbackMode.Flash:
					meshRenderer.material.color = this.colorCurrent;
					float maxGammaDistance4 = feedbackMinGamma + (feedbackMaxGamma - feedbackMinGamma) * alphaDistanceCoeff;
					float recoveryRate4 = (feedbackMaxGamma - feedbackMinGamma) / feedbackInDuration;
					float alpha4 = Mathf.MoveTowards(projector.material.color.a, maxGammaDistance4, recoveryRate4 * Time.deltaTime);
					float H4, S4, V4;
					Color.RGBToHSV(this.colorCurrent, out H4, out S4, out V4);
					projector.material.color = new Color(1f, 1f, 1f, alpha4); // (V4 > 0.5f) ? new Color(0f, 0f, 0f, alpha4) : new Color(1f, 1f, 1f, alpha4);
					projector.material.SetColor("_OutlineColor", (V4 <= 0.5f) ? new Color(0f, 0f, 0f, alpha4) : new Color(1f, 1f, 1f, alpha4));
					projector.orthographicSize = Mathf.Lerp(feedbackMinOrthographicSize, feedbackMaxOrthographicSize, scaleDistanceCoeff);
					projector.material.SetFloat("_OrthographicSize", projector.orthographicSize);
				break;
				case FeedbackMode.Blink:
					meshRenderer.material.color = this.colorCurrent;
					float delayDuration0 = this.Row * delayPerRow;
					float currentDuration0 = Mathf.PingPong(Time.time, feedbackInDuration + delayDuration0);
					currentDuration0 = Mathf.Max (0f, currentDuration0 - delayDuration0);
					float blinkCoeff0 = currentDuration0 / feedbackInDuration;
					float maxGammaDistance0 = feedbackMinGamma + (feedbackMaxGamma - feedbackMinGamma) * alphaDistanceCoeff;
					float alpha0 =  feedbackMinGamma + maxGammaDistance0  * blinkCoeff0;
					float H0, S0, V0;
					Color.RGBToHSV(this.colorCurrent, out H0, out S0, out V0);
					projector.material.color = new Color(1f, 1f, 1f, alpha0); //projector.material.color = (V0 > 0.5f) ? new Color(0f, 0f, 0f, alpha0) : new Color(1f, 1f, 1f, alpha0);
					projector.material.SetColor("_OutlineColor", (V0 <= 0.5f) ? new Color(0f, 0f, 0f, alpha0) : new Color(1f, 1f, 1f, alpha0));
					projector.orthographicSize = Mathf.Lerp(feedbackMinOrthographicSize, feedbackMaxOrthographicSize, scaleDistanceCoeff);
					projector.material.SetFloat("_OrthographicSize", projector.orthographicSize);
				break;
				case FeedbackMode.Pulse:
					meshRenderer.material.color = this.colorCurrent;
					float delayDuration1 = this.Row * delayPerRow;
					float currentDuration1 = Mathf.Repeat(Time.time, feedbackInDuration + feedbackInDuration + delayDuration1);
					currentDuration1 = Mathf.Max(0f, currentDuration1 - delayDuration1);
					float pulseCoeff1 = (this.pauseCurrent >= 0f) ? Mathf.Min(currentDuration1 / feedbackInDuration, 1f) : 1f - Mathf.Max(0f, (currentDuration1 - feedbackInDuration) / feedbackInDuration);
					float blinkCoeff1 = (this.pauseCurrent >= 0f) ? 1f - Mathf.Max(0f, (currentDuration1 - feedbackInDuration) / feedbackInDuration) : Mathf.Min(currentDuration1 / feedbackInDuration, 1f);
					float alpha1 = feedbackMinGamma + (feedbackMaxGamma - feedbackMinGamma) * blinkCoeff1; // from black to white
					float H1, S1, V1;
					Color.RGBToHSV(this.colorCurrent, out H1, out S1, out V1);
					projector.material.color = (V1 > 0.5f) ? new Color(0f, 0f, 0f, alpha1) : new Color(1f, 1f, 1f, alpha1);
					projector.material.SetColor("_OutlineColor", (V1 <= 0.5f) ? new Color(0f, 0f, 0f, alpha1) : new Color(1f, 1f, 1f, alpha1));
					projector.orthographicSize =  Mathf.Lerp(0f, Mathf.Lerp(feedbackMinOrthographicSize, feedbackMaxOrthographicSize, scaleDistanceCoeff), pulseCoeff1);
					projector.material.SetFloat("_OrthographicSize", projector.orthographicSize);
				break;

				case FeedbackMode.PulseOut:
					meshRenderer.material.color = this.colorCurrent;
					float delayDuration2 = this.Row * delayPerRow;
					float currentDuration2 = Mathf.Repeat(Time.time, feedbackInDuration + feedbackInDuration + delayDuration2);
					currentDuration2 = Mathf.Max(0f, currentDuration2 - delayDuration2);
					float pulseCoeff2 =  Mathf.Min(currentDuration2 / feedbackInDuration, 1f);
					float blinkCoeff2 = 1f - Mathf.Max(0f, (currentDuration2 - feedbackInDuration) / feedbackInDuration);
					float alpha2 = feedbackMinGamma + (feedbackMaxGamma - feedbackMinGamma) * blinkCoeff2; // from black to white
					float H2, S2, V2;
					Color.RGBToHSV(this.colorCurrent, out H2, out S2, out V2);
					projector.material.color = (V2 > 0.5f) ? new Color(0f, 0f, 0f, alpha2) : new Color(1f, 1f, 1f, alpha2);
					projector.material.SetColor("_OutlineColor", (V2 <= 0.5f) ? new Color(0f, 0f, 0f, alpha2) : new Color(1f, 1f, 1f, alpha2));
					projector.orthographicSize = Mathf.Lerp(0f, Mathf.Lerp(feedbackMinOrthographicSize, feedbackMaxOrthographicSize, scaleDistanceCoeff), pulseCoeff2);
					projector.material.SetFloat("_OrthographicSize", projector.orthographicSize);
				break;
				case FeedbackMode.PulseIn:
					meshRenderer.material.color = this.colorCurrent;
					float delayDuration3 = this.Row * delayPerRow;
					float currentDuration3 = Mathf.Repeat(Time.time, feedbackInDuration + feedbackInDuration + delayDuration3);
					currentDuration3 = Mathf.Max(0f, currentDuration3 - delayDuration3);
					float pulseCoeff3 =  1f - Mathf.Max(0f, (currentDuration3 - feedbackInDuration) / feedbackInDuration);
					float blinkCoeff3 = Mathf.Min(currentDuration3 / feedbackInDuration, 1f);
					float alpha3 = feedbackMinGamma + (feedbackMaxGamma - feedbackMinGamma) * blinkCoeff3; // from black to white
					float H3, S3, V3;
					Color.RGBToHSV(this.colorCurrent, out H3, out S3, out V3);
					projector.material.color = (V3 > 0.5f) ? new Color(0f, 0f, 0f, alpha3) : new Color(1f, 1f, 1f, alpha3);
					projector.material.SetColor("_OutlineColor", (V3 <= 0.5f) ? new Color(0f, 0f, 0f, alpha3) : new Color(1f, 1f, 1f, alpha3));
					projector.orthographicSize = Mathf.Lerp(0f, Mathf.Lerp(feedbackMinOrthographicSize, feedbackMaxOrthographicSize, scaleDistanceCoeff), pulseCoeff3);
					projector.material.SetFloat("_OrthographicSize", projector.orthographicSize);
				break;

				default:
					meshRenderer.material.color = this.colorCurrent;
					projector.material.color = this.projectorColorCurrent;
					projector.material.SetColor("_OutlineColor", projector.material.color);
					projector.orthographicSize = this.projectorSizeCurrent;
					projector.material.SetFloat("_OrthographicSize", projector.orthographicSize);
				break;

			}
		}
		else
		{

			switch (feedbackMode)
			{
				case FeedbackMode.Flash:
					meshRenderer.material.color = this.colorCurrent;
					float recoveryRate4 = (feedbackMaxGamma - feedbackMinGamma) / feedbackOutDuration;
					float alpha4 = Mathf.MoveTowards(projector.material.color.a, feedbackMinGamma, recoveryRate4 * Time.deltaTime);
					float H4, S4, V4;
					Color.RGBToHSV(this.colorCurrent, out H4, out S4, out V4);
					projector.material.color = (V4 > 0.5f) ? new Color(0f, 0f, 0f, alpha4) : new Color(1f, 1f, 1f, alpha4);
					projector.material.SetColor("_OutlineColor", (V4 <= 0.5f) ? new Color(0f, 0f, 0f, alpha4) : new Color(1f, 1f, 1f, alpha4)); 
					if (Mathf.Approximately(alpha4, feedbackMinGamma)) projector.orthographicSize = this.projectorSizeCurrent;
					projector.material.SetFloat("_OrthographicSize", projector.orthographicSize); 
					break;
				case FeedbackMode.Blink:
					meshRenderer.material.color = this.colorCurrent;
					float recoveryRate0 = (feedbackMaxGamma - feedbackMinGamma) / feedbackOutDuration;
					float alpha0 = Mathf.MoveTowards(projector.material.color.a, feedbackMinGamma, recoveryRate0 * Time.deltaTime);
					float H0, S0, V0;
					Color.RGBToHSV(this.colorCurrent, out H0, out S0, out V0);
					projector.material.color = (V0 > 0.5f) ? new Color(0f, 0f, 0f, alpha0) : new Color(1f, 1f, 1f, alpha0);
					projector.material.SetColor("_OutlineColor", (V0 <= 0.5f) ? new Color(0f, 0f, 0f, alpha0) : new Color(1f, 1f, 1f, alpha0));
					if (Mathf.Approximately(alpha0, feedbackMinGamma)) projector.orthographicSize = this.projectorSizeCurrent;
					projector.material.SetFloat("_OrthographicSize", projector.orthographicSize); 
				break;
				case FeedbackMode.Pulse:
					meshRenderer.material.color = this.colorCurrent;
					float recoveryRate1 = (feedbackMaxGamma - feedbackMinGamma) / feedbackOutDuration;
					float alpha1 = Mathf.MoveTowards(projector.material.color.a, feedbackMinGamma, recoveryRate1 * Time.deltaTime);
					float H1, S1, V1;
					Color.RGBToHSV(this.colorCurrent, out H1, out S1, out V1);
					projector.material.color = (V1 > 0.5f) ? new Color(0f, 0f, 0f, alpha1) : new Color(1f, 1f, 1f, alpha1);
					projector.material.SetColor("_OutlineColor", (V1 <= 0.5f) ? new Color(0f, 0f, 0f, alpha1) : new Color(1f, 1f, 1f, alpha1)); 
					if (Mathf.Approximately(alpha1, feedbackMinGamma)) projector.orthographicSize = this.projectorSizeCurrent;
					projector.material.SetFloat("_OrthographicSize", projector.orthographicSize); 
				break;
				case FeedbackMode.PulseIn:
					meshRenderer.material.color = this.colorCurrent;
					float recoveryRate2 = (feedbackMaxGamma - feedbackMinGamma) / feedbackOutDuration;
					float alpha2 = Mathf.MoveTowards(projector.material.color.a, feedbackMinGamma, recoveryRate2 * Time.deltaTime);
					float H2, S2, V2;
					Color.RGBToHSV(this.colorCurrent, out H2, out S2, out V2);
					projector.material.color = (V2 > 0.5f) ? new Color(0f, 0f, 0f, alpha2) : new Color(1f, 1f, 1f, alpha2);
					projector.material.SetColor("_OutlineColor", (V2 <= 0.5f) ? new Color(0f, 0f, 0f, alpha2) : new Color(1f, 1f, 1f, alpha2));
					if (Mathf.Approximately(alpha2, feedbackMinGamma)) projector.orthographicSize = this.projectorSizeCurrent;
					projector.material.SetFloat("_OrthographicSize", projector.orthographicSize); 
				break;
				case FeedbackMode.PulseOut:
					meshRenderer.material.color = this.colorCurrent;
					float recoveryRate3 = (feedbackMaxGamma - feedbackMinGamma) / feedbackOutDuration;
					float alpha3 = Mathf.MoveTowards(projector.material.color.a, feedbackMinGamma, recoveryRate3 * Time.deltaTime);
					float H3, S3, V3;
					Color.RGBToHSV(this.colorCurrent, out H3, out S3, out V3);
					projector.material.color = (V3 > 0.5f) ? new Color(0f, 0f, 0f, alpha3) : new Color(1f, 1f, 1f, alpha3);
					projector.material.SetColor("_OutlineColor", (V3 <= 0.5f) ? new Color(0f, 0f, 0f, alpha3) : new Color(1f, 1f, 1f, alpha3));
					if (Mathf.Approximately(alpha3, feedbackMinGamma)) projector.orthographicSize = this.projectorSizeCurrent;
					projector.material.SetFloat("_OrthographicSize", projector.orthographicSize); 
				break;
				default:
					meshRenderer.material.color = this.colorCurrent;
					projector.material.color = this.projectorColorCurrent;
					projector.material.SetColor("_OutlineColor", projector.material.color);
					projector.orthographicSize = this.projectorSizeCurrent;
					projector.material.SetFloat("_OrthographicSize", projector.orthographicSize); 
				break;
			}
		}*/

		meshRenderer.material.color = this.colorCurrent;

		projector.material.color = this.projectorColorCurrent;
		projector.material.SetColor("_OutlineColor", projector.material.color);
		projector.orthographicSize = this.projectorSizeCurrent;
		projector.material.SetFloat("_OrthographicSize", projector.orthographicSize);

		if (this.projectorTextureCurrent != this.projectorTextureTarget)
		{
			projector.material.mainTexture = Resources.Load<Texture2D>(this.projectorTextureTarget);
			this.projectorTextureCurrent = this.projectorTextureTarget;
		}
		projector.transform.localRotation = Quaternion.Euler(90f, this.projectorRotationCurrent, 180f);

		//Quaternion.LookRotation(-transform.up, transform.forward);
		//projector.transform.Rotate(Vector3.forward, this.projectorRotationCurrent);

		//if (feedbackMode == FeedbackMode.Debug)
		//	meshRenderer.material.color = Color.Lerp(Color.white, Color.red, 1f - this.separationLevelCurrent/(float)this.nbSeparationLevels);

		this.textMesh.alignment = this.textAlignmentTarget;
		this.textMesh.fontSize =  this.textSizeCurrent;
		this.textMesh.color = this.textColorCurrent;
		this.textMesh.text = this.textTarget;

		//Vector3 targetPosition = this.transform.position - new Vector3(1f, 0f, 0f);
		textRectTransform.localEulerAngles = new Vector3(90f, 0f, 90f); // + this.textRotationCurrent


		// plane
		if (this.planeTextureCurrent != this.planeTextureTarget){
			planeMeshRenderer.material.mainTexture = Resources.Load<Texture2D>(this.planeTextureTarget);
			this.planeTextureCurrent = this.planeTextureTarget;
		}
		planeMeshRenderer.material.color = this.planeColorCurrent;
		planeMeshRenderer.material.SetTextureOffset("_MainTex", this.planeOffsetCurrent);

		planeGameObject.transform.localScale = new Vector3(this.planeSizeCurrent, 0.01f, this.planeSizeCurrent);
		planeGameObject.transform.rotation = Quaternion.LookRotation(transform.forward, transform.up);
		planeGameObject.transform.Rotate(Vector3.up, this.planeRotationCurrent);

	}

	// Update is called once per frame
	void Update()
    {
		if (this.shapeChangeDurationTarget > 0f)
		{
			this.xAxisCurrent += (this.xAxisTarget - this.xAxisCurrent) / this.shapeChangeDurationTarget * Time.deltaTime;
			this.yAxisCurrent += (this.yAxisTarget - this.yAxisCurrent) / this.shapeChangeDurationTarget * Time.deltaTime;
			this.selectCountCurrent += (this.selectCountTarget - this.selectCountCurrent) / this.shapeChangeDurationTarget * Time.deltaTime;
			this.rotationCurrent += (this.rotationTarget - this.rotationCurrent) / this.shapeChangeDurationTarget * Time.deltaTime;
			this.positionCurrent += (this.positionTarget - this.positionCurrent) / this.shapeChangeDurationTarget * Time.deltaTime;
			//this.proximityCurrent += (this.proximityTarget - this.proximityCurrent) / this.shapeChangeDurationTarget * Time.deltaTime;
			this.reachingCurrent = this.reachingTarget;
			this.holdingCurrent = this.holdingTarget;
			this.separationLevelCurrent = this.separationLevelTarget;
			this.proximityCurrent = this.proximityTarget;
			this.distanceCurrent = this.distanceTarget;
			/*if(this.pauseTarget == 0.0f && this.pauseCurrent == 1.0f)
			{
				toUnpause = true;
			}*/
			this.pauseCurrent = this.pauseTarget;

			//this.reachingCurrent += (this.reachingTarget - this.reachingCurrent) / this.shapeChangeDurationTarget * Time.deltaTime;
			//this.holdingCurrent += (this.holdingTarget - this.holdingCurrent) / this.shapeChangeDurationTarget * Time.deltaTime;
			//this.pauseCurrent += (this.pauseTarget - this.pauseCurrent) / this.shapeChangeDurationTarget * Time.deltaTime;
			this.shapeChangeDurationTarget -= Time.deltaTime;
		}
		
		if (this.textureChangeDurationTarget > 0f)
		{
			this.planeOffsetCurrent += (this.planeOffsetTarget - this.planeOffsetCurrent) / this.textureChangeDurationTarget * Time.deltaTime;
			this.planeColorCurrent += (this.planeColorTarget - this.planeColorCurrent) / this.textureChangeDurationTarget * Time.deltaTime;
			this.planeSizeCurrent += (this.planeSizeTarget - this.planeSizeCurrent) / this.textureChangeDurationTarget * Time.deltaTime;
			this.planeRotationCurrent += (this.planeRotationTarget - this.planeRotationCurrent) / this.textureChangeDurationTarget * Time.deltaTime;
			this.colorCurrent += (this.colorTarget - this.colorCurrent) / this.textureChangeDurationTarget * Time.deltaTime;
			this.textColorCurrent += (this.textColorTarget - this.textColorCurrent) / this.textureChangeDurationTarget * Time.deltaTime;
			this.textSizeCurrent += (this.textSizeTarget - this.textSizeCurrent) / this.textureChangeDurationTarget * Time.deltaTime;

			/*float rawTextRotationDistance = this.textRotationTarget - this.textRotationCurrent;
			float textRotationDistance = (360f + rawTextRotationDistance) % 360f;
			if (i == 0 && j == 0) Debug.Log(rawTextRotationDistance + "=" + this.textRotationTarget + " + " + this.textRotationCurrent);
			if (i == 0 && j == 0) Debug.Log(rawTextRotationDistance  + "->" + textRotationDistance);
			this.textRotationCurrent += textRotationDistance / this.textureChangeDurationTarget * Time.deltaTime;
			this.textRotationCurrent %= 360f;*/
			//this.textRotationCurrent += (this.textRotationTarget - this.textRotationCurrent) / this.textureChangeDurationTarget * Time.deltaTime;
			this.textRotationCurrent = this.textRotationTarget;
			this.textureChangeDurationTarget -= Time.deltaTime;
		}
		if (this.projectorChangeDurationTarget > 0f)
		{
			this.projectorColorCurrent = this.projectorColorTarget;
			this.projectorSizeCurrent = this.projectorSizeTarget;
			this.projectorRotationCurrent = this.projectorRotationTarget;
			//Debug.Log("this.projectorSizeCurrent : " + this.projectorSizeCurrent);
			this.projectorChangeDurationTarget -= Time.deltaTime;
		}
		render();
	}
}
