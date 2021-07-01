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

public class ExpanDialStickView : MonoBehaviour
{
	private float diameter = 4.0f;
	private float height = 10.0f;
	private float offset = 0.5f;

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

	private float proximityCurrent = 0f;
	private float proximityDiff = 0f;
	private float proximityTarget = 0f;

	private float pauseCurrent = 0f;
	private float pauseDiff = 0f;
	private float pauseTarget = 0f;

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

	public Material transparentMaterial;
	
	private GameObject textGameObject;
	private TextMeshPro textMesh;
	private RectTransform textRectTransform;

	
	private GameObject planeGameObject;
	public Material planeMaterial;
	private MeshRenderer planeMeshRenderer;
	private string planeTextureCurrent = "";
	private string planeTextureTarget = "";
	private float planeRotationTarget= 0f;
	private float planeRotationCurrent= 0f;
	private bool paused = false;

	private MeshRenderer meshRenderer;
	private Projector projector;

	public const int BLINK_FEEDBACK = 0;
	public const int DOWN_FEEDBACK = 1;
	public const int UP_FEEDBACK = 2;

	private int feedbackMode = DOWN_FEEDBACK;

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

	public sbyte TargetAxisX{
		get => (sbyte)this.xAxisTarget;
		set => this.xAxisTarget = value;
	}

	public sbyte CurrentAxisX{
		get => (sbyte)this.xAxisCurrent;
		set {
			this.xAxisDiff += value - this.xAxisCurrent;
			this.xAxisCurrent = value;
		}
	}

	public sbyte TargetAxisY{
		get => (sbyte)this.yAxisTarget;
		set => this.yAxisTarget = value;
	}

	public sbyte CurrentAxisY{
		get => (sbyte)this.yAxisCurrent;
		set {
			this.yAxisDiff += value - this.yAxisCurrent;
			this.yAxisCurrent = value;
		}
	}

	public byte TargetSelectCount{
		get => (byte)this.selectCountTarget;
		set => this.selectCountTarget = value;
	}

	public byte CurrentSelectCount{
		get => (byte)this.selectCountCurrent;
		set {
			this.selectCountDiff += this.selectCountCurrent <= value ? value - this.selectCountCurrent : 255 + value - this.selectCountCurrent;
			this.selectCountCurrent = value;
		}
	}
	
	public sbyte TargetRotation{
		get => (sbyte)this.rotationTarget;
		set => this.rotationTarget = value;
	}
	
	public sbyte CurrentRotation{
		get => (sbyte)this.rotationCurrent;
		set {
			this.rotationDiff += Mathf.Abs(value - this.rotationCurrent) <= 127 ? value - this.rotationCurrent : 255 + value - this.rotationCurrent;
			this.rotationCurrent = value;
		}
	}
	
	public sbyte TargetPosition{
		get => (sbyte)this.positionTarget;
		set => this.positionTarget = value;
	}
	
	public sbyte CurrentPosition{
		get => (sbyte)this.positionCurrent;
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
	public bool TargetPaused
	{
		get => this.pauseTarget > 0f ? true : false;
		set => this.pauseTarget = value ? 1f : 0f;
	}

	public bool CurrentPaused
	{
		get => this.pauseCurrent > 0f ? true : false;
		set
		{
			this.pauseDiff += (value ? 1f : 0f) - this.pauseCurrent;
			this.pauseCurrent = value ? 1f : 0f;
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

	public void setShapeChangeTarget(sbyte xAxis, sbyte yAxis, byte selectCount, sbyte rotation, sbyte position, bool reaching, bool holding, float proximity, bool paused, float shapeChangeDuration)
	{
		TargetAxisX = xAxis;
		TargetAxisY = yAxis;
		TargetSelectCount = selectCount;
		TargetRotation = rotation;
		TargetReaching = reaching;
		TargetHolding = holding;
		TargetProximity = proximity;
		TargetPaused = paused;
		TargetPosition = position;
		TargetShapeChangeDuration = shapeChangeDuration;
	}
	
	public void setShapeChangeCurrent(sbyte xAxis, sbyte yAxis, byte selectCount, sbyte rotation, sbyte position, bool reaching, bool holding, float proximity, bool paused, float shapeChangeDuration)
	{
		CurrentAxisX = xAxis;
		CurrentAxisY = yAxis;
		CurrentSelectCount = selectCount;
		CurrentRotation = rotation;
		CurrentHolding = holding;
		CurrentProximity = proximity;
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

	public void setTextureChangeTarget(Color color, string textureName, float textureRotation, float textureChangeDuration )
	{
		TargetColor = color;
		TargetPlaneTexture = textureName;
		TargetPlaneRotation = textureRotation;
		TargetTextureChangeDuration = textureChangeDuration;
	}

	public void setTextureChangeCurrent(Color color, string textureName, float textureRotation, float textureChangeDuration )
	{
		CurrentColor = color;
		CurrentPlaneTexture = textureName;
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
			this.proximityDiff,
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
			= this.proximityDiff
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

		projector = this.transform.GetChild(0).gameObject.GetComponent<Projector>();

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
		planeGameObject  = GameObject.CreatePrimitive(PrimitiveType.Plane);
		Destroy(planeGameObject.GetComponent<MeshCollider>());
		planeGameObject.transform.parent = this.transform;
		planeGameObject.transform.position += new Vector3(0f, 1.01f, 0f);
		planeGameObject.transform.localScale  = new Vector3(0.06f, 0.06f, 0.06f);
		planeMeshRenderer  = planeGameObject.GetComponent<MeshRenderer> ();
		planeMeshRenderer.material = planeMaterial;
		planeMeshRenderer.material.mainTexture = Resources.Load<Texture2D>("default");

	}

	void render()
	{
		float positionCurrentInverseLerp = Mathf.InverseLerp(0f, 40f, this.positionCurrent);
		float positionCurrentLerp = Mathf.Lerp(0f, 10f, positionCurrentInverseLerp);
		this.transform.position = new Vector3(i * (diameter + offset), positionCurrentLerp, j * (diameter + offset));

		this.transform.localScale = new Vector3(diameter, height / 2, diameter);
		
		this.transform.rotation = Quaternion.identity;
		float rotationCurrentInverseLerp = Mathf.InverseLerp(-127f, 127f, this.rotationCurrent);
		float rotationCurrentLerp = Mathf.Lerp(-360f * 8, 360f * 8, rotationCurrentInverseLerp) % 360f;
		this.transform.RotateAround(this.transform.position - new Vector3(0f, height / 2, 0f), Vector3.up, rotationCurrentLerp);
		
		float yAxisCurrentInverseLerp = Mathf.InverseLerp(-127f, 127f, this.yAxisCurrent);
		float yAxisCurrentLerp = Mathf.Lerp(-30f, 30f, yAxisCurrentInverseLerp);
		this.transform.RotateAround(this.transform.position - new Vector3(0f, height / 2, 0f), Vector3.left, yAxisCurrentLerp);
		
		float xAxisCurrentInverseLerp = Mathf.InverseLerp(-127f, 127f, this.xAxisCurrent);
		float xAxisCurrentLerp = Mathf.Lerp(-30f, 30f, xAxisCurrentInverseLerp);
		this.transform.RotateAround(this.transform.position - new Vector3(0f, height / 2, 0f), Vector3.back, xAxisCurrentLerp);

		// SAFETY CUE
		if(this.pauseCurrent > 0f)
		{
			switch (feedbackMode)
			{
				case BLINK_FEEDBACK:
					reverseColorCurrent = new Color(1.0f - this.colorCurrent.r, 1.0f - this.colorCurrent.g, 1.0f - this.colorCurrent.b);
					meshRenderer.material.color =  Color.Lerp(this.colorCurrent, reverseColorCurrent, Mathf.PingPong(Time.time, 1.999f));
					projector.orthographicSize = 0f;
					break;

				case UP_FEEDBACK:
					meshRenderer.material.color = this.colorCurrent;
					reverseColorCurrent = new Color(1.0f - this.colorCurrent.r, 1.0f - this.colorCurrent.g, 1.0f - this.colorCurrent.b);
					reverseColorCurrent.a = Mathf.Lerp(0f, 1f, Mathf.Repeat(Time.time, 1.999f));
					projector.material.color = reverseColorCurrent;
					projector.orthographicSize = Mathf.Lerp(8f, 0f, Mathf.Repeat(Time.time, 1.999f));
					break;
				case DOWN_FEEDBACK:
					meshRenderer.material.color = this.colorCurrent;
					reverseColorCurrent = new Color(1.0f - this.colorCurrent.r, 1.0f - this.colorCurrent.g, 1.0f - this.colorCurrent.b);
					reverseColorCurrent.a = Mathf.Lerp(1f, 0f, Mathf.Repeat(Time.time, 1.999f));
					projector.material.color = reverseColorCurrent;
					projector.orthographicSize = Mathf.Lerp(0f, 8f, Mathf.Repeat(Time.time, 1.999f));
					break;
				default:
					meshRenderer.material.color = this.colorCurrent;
					projector.orthographicSize = 0f;
					break;

			}
			//this.transform.localScale = Vector3.Lerp(new Vector3(diameter, height / 2, diameter), new Vector3(diameter * 1.1f, height / 2, diameter * 1.1f), Mathf.PingPong(Time.time, 1));
			//meshRenderer.material.SetColor("_FirstOutlineColor", meshRenderer.material.color);
			//meshRenderer.material.SetFloat("_FirstOutlineWidth", Mathf.Lerp(0f, 1f, Mathf.PingPong(Time.time, 1)));
		}
		else
		{
			meshRenderer.material.color = this.colorCurrent;
			projector.orthographicSize = 0f;
		}

		this.textMesh.alignment = this.textAlignmentTarget;
		this.textMesh.fontSize =  this.textSizeCurrent;
		this.textMesh.color = this.textColorCurrent;
		this.textMesh.text = this.textTarget;

		Vector3 targetPosition = this.transform.position - new Vector3(1f, 0f, 0f);
		Vector3 targetDirection = targetPosition - this.transform.position;
		//textRectTransform.rotation = Quaternion.LookRotation(targetDirection, Vector3.up);
		//targetPosition = this.transform.position - new Vector3(0f, 1f, 0f);
		//textRectTransform.LookAt(targetPosition, Vector3.left);
		//float textRotationEulerAngle = 360f + this.textRotationCurrent % 360f
		textRectTransform.localEulerAngles = new Vector3(90f, 0f, 90f); // + this.textRotationCurrent


		// plane
		if (this.planeTextureCurrent != this.planeTextureTarget){
			planeMeshRenderer.material.mainTexture = Resources.Load<Texture2D>(this.planeTextureTarget);
			this.planeTextureCurrent = this.planeTextureTarget;
		}

		planeGameObject.transform.eulerAngles = new Vector3(0f, this.planeRotationCurrent, 0f);
		
		//textRectTransform.LookAt(Vector3.zero, Vector3.up);

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
			this.proximityCurrent += (this.proximityTarget - this.proximityCurrent) / this.shapeChangeDurationTarget * Time.deltaTime;
			this.reachingCurrent = this.reachingTarget;
			this.holdingCurrent = this.holdingTarget;
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
			this.planeRotationCurrent = this.planeRotationTarget;
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
		render();
	}
}
