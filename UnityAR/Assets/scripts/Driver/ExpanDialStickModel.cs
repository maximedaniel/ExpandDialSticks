﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;



public class ExpanDialStickModel
{
	private const float AXIS_THRESHOLD = 10f;
	private float diameter = 4.0f;
	private float height = 10.0f;
	private float offset = 0.5f;

	private int i = 0;
	private int j = 0;

	private float xAxisCurrent = 0f;
	private float xAxisDiff = 0f;
	private float xAxisRiseDiff = 0f;
	private float xAxisFallDiff = 0f;
	private float xAxisTarget = 0f;

	private float yAxisCurrent = 0f;
	private float yAxisDiff = 0f;
	private float yAxisRiseDiff = 0f;
	private float yAxisFallDiff = 0f;
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

	private string planeTextureCurrent = "";
	private string planeTextureTarget = "";
	private Color planeColorCurrent = Color.black;
	private Color planeColorTarget = Color.black;
	private Vector2 planeOffsetCurrent = Vector2.zero;
	private Vector2 planeOffsetTarget = Vector2.zero;
	private float planeSizeCurrent = 1f;
	private float planeSizeTarget = 1f;
	private float planeRotationTarget= 0f;
	private float planeRotationCurrent= 0f;



	private Color projectorFrontColorCurrent = Color.black;
	private Color projectorFrontColorTarget = Color.black;
	private string projectorFrontTextureCurrent = "";
	private string projectorFrontTextureTarget = "";
	private Vector2 projectorFrontOffsetCurrent = Vector2.zero;
	private Vector2 projectorFrontOffsetTarget = Vector2.zero;
	private float projectorFrontRotationCurrent = 0f;
	private float projectorFrontRotationTarget = 0f;
	private float projectorFrontSizeCurrent = 0f;
	private float projectorFrontSizeTarget = 0f;
	private float projectorFrontChangeDurationCurrent = 0f;
	private float projectorFrontChangeDurationTarget = 0f;


	private Color projectorBackColorCurrent = Color.black;
	private Color projectorBackColorTarget = Color.black;
	private string projectorBackTextureCurrent = "";
	private string projectorBackTextureTarget = "";
	private Vector2 projectorBackOffsetCurrent = Vector2.zero;
	private Vector2 projectorBackOffsetTarget = Vector2.zero;
	private float projectorBackRotationCurrent = 0f;
	private float projectorBackRotationTarget = 0f;
	private float projectorBackSizeCurrent = 0f;
	private float projectorBackSizeTarget = 0f;
	private float projectorBackChangeDurationCurrent = 0f;
	private float projectorBackChangeDurationTarget = 0f;


	private float storedSpeed = 0f;


	private bool init = false;


	public Material transparentMaterial;

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
	public float StoredSpeed
	{
		get => this.storedSpeed;
		set => this.storedSpeed = value;
	}

	// Getters and Setters
	public bool Init{
		get => this.init;
		set => this.init = value;
	}

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
		get => (sbyte)Mathf.Round(this.xAxisTarget);
		set => this.xAxisTarget = value;
	}

	public sbyte CurrentAxisX{
		get => (sbyte)Mathf.Round(this.xAxisCurrent);
		set {
			this.xAxisDiff += value - this.xAxisCurrent;
			if (this.xAxisCurrent < AXIS_THRESHOLD && value > AXIS_THRESHOLD) this.xAxisRiseDiff++;
			if (this.xAxisCurrent > AXIS_THRESHOLD && value < AXIS_THRESHOLD) this.xAxisFallDiff++;
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
			if (this.yAxisCurrent < AXIS_THRESHOLD && value > AXIS_THRESHOLD) this.yAxisRiseDiff++;
			if (this.yAxisCurrent > AXIS_THRESHOLD && value < AXIS_THRESHOLD) this.yAxisFallDiff++;
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
			this.positionDiff += (CurrentReaching) ? 0 : value - this.positionCurrent;
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

	public Color TargetTextColor
	{
		get => this.textColorTarget;
		set => this.textColorTarget = value;
	}

	public Color CurrentTextColor
	{
		get => this.textColorCurrent;
		set
		{
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
	public Color CurrentProjectorFrontColor
	{
		get => this.projectorFrontColorCurrent;
		set => this.projectorFrontColorCurrent = value;
	}
	public Color TargetProjectorFrontColor
	{
		get => this.projectorFrontColorTarget;
		set => this.projectorFrontColorTarget = value;
	}
	public string CurrentProjectorFrontTexture
	{
		get => this.projectorFrontTextureCurrent;
		set => this.projectorFrontTextureCurrent = value;
	}
	public string TargetProjectorFrontTexture
	{
		get => this.projectorFrontTextureTarget;
		set => this.projectorFrontTextureTarget = value;
	}
	public Vector2 CurrentProjectorFrontOffset
	{
		get => this.projectorFrontOffsetCurrent;
		set => this.projectorFrontOffsetCurrent = value;
	}
	public Vector2 TargetProjectorFrontOffset
	{
		get => this.projectorFrontOffsetTarget;
		set => this.projectorFrontOffsetTarget = value;
	}
	public float CurrentProjectorFrontRotation
	{
		get => this.projectorFrontRotationCurrent;
		set => this.projectorFrontRotationCurrent = value;
	}
	public float TargetProjectorFrontRotation
	{
		get => this.projectorFrontRotationTarget;
		set => this.projectorFrontRotationTarget = value;
	}
	public float CurrentProjectorFrontSize
	{
		get => this.projectorFrontSizeCurrent;
		set => this.projectorFrontSizeCurrent = value;
	}
	public float TargetProjectorFrontSize
	{
		get => this.projectorFrontSizeTarget;
		set => this.projectorFrontSizeTarget = value;
	}
	public float CurrentProjectorFrontChangeDuration
	{
		get => this.projectorFrontChangeDurationCurrent;
		set => this.projectorFrontChangeDurationCurrent = value;
	}
	public float TargetProjectorFrontChangeDuration
	{
		get => this.projectorFrontChangeDurationTarget;
		set => this.projectorFrontChangeDurationTarget = value;
	}

	public Color CurrentProjectorBackColor
	{
		get => this.projectorBackColorCurrent;
		set => this.projectorBackColorCurrent = value;
	}
	public Color TargetProjectorBackColor
	{
		get => this.projectorBackColorTarget;
		set => this.projectorBackColorTarget = value;
	}
	public string CurrentProjectorBackTexture
	{
		get => this.projectorBackTextureCurrent;
		set => this.projectorBackTextureCurrent = value;
	}
	public string TargetProjectorBackTexture
	{
		get => this.projectorBackTextureTarget;
		set => this.projectorBackTextureTarget = value;
	}
	public Vector2 CurrentProjectorBackOffset
	{
		get => this.projectorBackOffsetCurrent;
		set => this.projectorBackOffsetCurrent = value;
	}
	public Vector2 TargetProjectorBackOffset
	{
		get => this.projectorBackOffsetTarget;
		set => this.projectorBackOffsetTarget = value;
	}
	public float CurrentProjectorBackRotation
	{
		get => this.projectorBackRotationCurrent;
		set => this.projectorBackRotationCurrent = value;
	}
	public float TargetProjectorBackRotation
	{
		get => this.projectorBackRotationTarget;
		set => this.projectorBackRotationTarget = value;
	}
	public float CurrentProjectorBackSize
	{
		get => this.projectorBackSizeCurrent;
		set => this.projectorBackSizeCurrent = value;
	}
	public float TargetProjectorBackSize
	{
		get => this.projectorBackSizeTarget;
		set => this.projectorBackSizeTarget = value;
	}
	public float CurrentProjectorBackChangeDuration
	{
		get => this.projectorBackChangeDurationCurrent;
		set => this.projectorBackChangeDurationCurrent = value;
	}
	public float TargetProjectorBackChangeDuration
	{
		get => this.projectorBackChangeDurationTarget;
		set => this.projectorBackChangeDurationTarget = value;
	}
	public void setShapeChangeTarget(sbyte xAxis, sbyte yAxis, byte selectCount, sbyte rotation, sbyte position, bool reaching, bool holding, int separationLevel, float proximity, float distance, int paused, float shapeChangeDuration)
	{
		TargetAxisX = xAxis;
		TargetAxisY = yAxis;
		TargetSelectCount = selectCount;
		TargetRotation = rotation;
		TargetReaching = reaching;
		TargetHolding = holding;
		TargetPosition = position;
		TargetSeparationLevel = separationLevel;
		TargetProximity = proximity;
		TargetDistance = distance;
		TargetPaused = paused;
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
		
		if(!this.init){
			eraseShapeDiffs();
			this.init = !this.init;
		}
	}

	public void setFrontProjectorChangeTarget(Color color, string textureName, Vector2 textureOffset, float textureRotation, float textureSize, float projectorChangeDuration)
	{
		TargetProjectorFrontColor = color;
		TargetProjectorFrontTexture = textureName;
		TargetProjectorFrontOffset = textureOffset;
		TargetProjectorFrontRotation = textureRotation;
		TargetProjectorFrontSize = textureSize;
		TargetProjectorFrontChangeDuration = projectorChangeDuration;
	}
	public void setFrontProjectorChangeCurrent(Color color, string textureName, Vector2 textureOffset, float textureRotation, float textureSize, float projectorChangeDuration)
	{
		CurrentProjectorFrontColor = color;
		CurrentProjectorFrontTexture = textureName;
		CurrentProjectorFrontOffset = textureOffset;
		CurrentProjectorFrontRotation = textureRotation;
		CurrentProjectorFrontSize = textureSize;
		CurrentProjectorFrontChangeDuration = projectorChangeDuration;
	}

	public void setBackProjectorChangeTarget(Color color, string textureName, Vector2 textureOffset, float textureRotation, float textureSize, float projectorChangeDuration)
	{
		TargetProjectorBackColor = color;
		TargetProjectorBackTexture = textureName;
		TargetProjectorBackOffset = textureOffset;
		TargetProjectorBackRotation = textureRotation;
		TargetProjectorBackSize = textureSize;
		TargetProjectorBackChangeDuration = projectorChangeDuration;
	}
	public void setBackProjectorChangeCurrent(Color color, string textureName, Vector2 textureOffset, float textureRotation, float textureSize, float projectorChangeDuration)
	{
		CurrentProjectorBackColor = color;
		CurrentProjectorBackTexture = textureName;
		CurrentProjectorBackOffset = textureOffset;
		CurrentProjectorBackRotation = textureRotation;
		CurrentProjectorBackSize = textureSize;
		CurrentProjectorBackChangeDuration = projectorChangeDuration;
	}
	public void setTextureChangeTarget(Color color, string textureName, Color textureColor, float textureSize, Vector2 textureOffset, float textureRotation, float textureChangeDuration )
	{
		TargetColor = color;
		TargetPlaneTexture = textureName;
		TargetPlaneColor = textureColor;
		TargetPlaneOffset = textureOffset;
		TargetPlaneSize = textureSize;
		TargetPlaneRotation = textureRotation;
		TargetTextureChangeDuration = textureChangeDuration;
	}

	public void setTextureChangeCurrent(Color color, string textureName, Color textureColor, float textureSize, Vector2 textureOffset, float textureRotation,  float textureChangeDuration )
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
		/*this.xAxisCurrent = this.xAxisTarget;
		this.yAxisCurrent = this.yAxisTarget;
		this.selectCountCurrent = this.selectCountTarget;
		this.rotationCurrent = this.rotationTarget;
		this.positionCurrent = this.positionTarget;
		this.reachingCurrent = this.reachingTarget;
		this.holdingCurrent = this.holdingTarget;
		this.shapeChangeDurationCurrent = this.shapeChangeDurationTarget = 0f;*/

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
}