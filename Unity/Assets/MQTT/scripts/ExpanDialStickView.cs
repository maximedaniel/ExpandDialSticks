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

	private float stateDurationCurrent = 0f;
	private float stateDurationDiff = 0f;
	private float stateDurationTarget = 0f;


	private float reachingCurrent = 0f;
	private float reachingDiff = 0f;
	private float reachingTarget = 0f;

	private float holdingCurrent = 0f;
	private float holdingDiff = 0f;
	private float holdingTarget = 0f;

	private Color colorCurrent = Color.white;
	private Color colorDiff = Color.black;
	private Color colorTarget = Color.white;

	private float colorDurationCurrent = 0f;
	private float colorDurationDiff = 0f;
	private float colorDurationTarget = 0f;

	private string text = "";

	public Material transparentMaterial;
	
	public TextMeshPro textMeshPro;


	// Start is called before the first frame update
	void Start()
    {
		this.transform.GetComponent<MeshRenderer>().material = transparentMaterial;
		this.name = "ExpanDialStick (" + i + ", " + j + ")";
		this.text = "(" + i + ", " + j + ")";
		
		GameObject textGameObject = new GameObject( "Text (" + i + ", " + j + ")");
		textGameObject.transform.parent = this.transform;

		TextMeshPro textMesh = textGameObject.AddComponent<TextMeshPro>();
		textMesh.alignment = TextAlignmentOptions.Center;
		textMesh.fontSize = 1;
		textMesh.color = Color.black;
		textMesh.text = this.text;
		RectTransform textRectTransform = textGameObject.GetComponent<RectTransform>();
		textRectTransform.position += new Vector3(0f, 1.01f, 0f);
		textRectTransform.Rotate(90f, 0f, 0f, Space.Self);
	}


	public void setIndexes(int i, int j)
	{
		this.i = i;
		this.j = j;
	}

	public void setConstants(float diameter, float height, float offset)
	{
		this.diameter = diameter;
		this.height = height;
		this.offset = offset;
	}

	public void setColorTarget(Color colorTarget, float colorDuration)
	{
		this.colorTarget = colorTarget;
		this.colorDiff = this.colorTarget - this.colorCurrent;

		this.colorDurationTarget = colorDuration;
		this.colorDurationDiff += this.colorDurationTarget - this.colorDurationCurrent;
	}

	public void setColorCurrent(Color colorCurrent)
	{
		this.colorTarget = colorTarget;
		this.colorDiff = this.colorTarget - this.colorCurrent;
		this.colorCurrent = this.colorTarget;

		this.colorDurationTarget = 0f;
		this.colorDurationDiff += this.colorDurationTarget - this.colorDurationCurrent;
		this.colorDurationCurrent = this.colorDurationTarget;
	}

	public void setTargetAxisX(sbyte xAxisTarget)
	{
		this.xAxisTarget = xAxisTarget;
		this.xAxisDiff += this.xAxisTarget - this.xAxisCurrent;
	}
	public sbyte getTargetAxisX()
	{
		return (sbyte)this.xAxisTarget;
	}

	public void setTargetAxisY(sbyte yAxisTarget)
	{
		this.yAxisTarget = yAxisTarget;
		this.yAxisDiff += this.yAxisTarget - this.yAxisCurrent;
	}
	public sbyte getTargetAxisY()
	{
		return (sbyte)this.yAxisTarget;
	}
	

	public void setTargetSelectCount(byte selectCountTarget)
	{
		this.selectCountTarget = selectCountTarget;
		this.selectCountDiff += this.selectCountCurrent < this.selectCountTarget ? this.selectCountTarget - this.selectCountCurrent : 255 + this.selectCountTarget - this.selectCountCurrent;
	}

	public byte getTargetSelectCount()
	{
		return (byte)this.selectCountTarget;
	}
	
	public void setTargetRotation(sbyte rotationTarget)
	{
		this.rotationTarget = rotationTarget;
		this.rotationDiff += Mathf.Abs(this.rotationTarget - this.rotationCurrent) < 127 ? this.rotationTarget - this.rotationCurrent : 255 + this.rotationTarget - this.rotationCurrent;
	}
	public sbyte getTargetRotation()
	{
		return (sbyte)this.rotationTarget;
	}

	public void setTargetPosition(sbyte positionTarget)
	{
		this.positionTarget = positionTarget;
		this.positionDiff += this.positionTarget - this.positionCurrent;
	}
	public sbyte getTargetPosition()
	{
		return (sbyte)this.positionTarget;
	}

	public void setTargetReaching(bool reachingTarget)
	{		
		this.reachingTarget = reachingTarget ? 1f : 0f;
		this.reachingDiff += this.reachingTarget - this.reachingCurrent;

	}
	public bool getTargetReaching()
	{
		return (this.reachingTarget > 0f)? true: false;
	}


	public void setTargetHolding(bool holdingTarget)
	{		
		this.holdingTarget = holdingTarget ? 1f : 0f;
		this.holdingDiff += this.holdingTarget - this.holdingCurrent;

	}
	public bool getTargetHolding()
	{
		return (this.holdingTarget > 0f)? true: false;
	}


	

	public void setTargetDuration( float stateDuration)
	{		
		this.stateDurationTarget = stateDurationTarget;
		this.stateDurationDiff += this.stateDurationTarget - this.stateDurationCurrent;
	}

	public float getTargetDuration()
	{
		return this.stateDurationTarget;
	}

	public void setStateTarget(sbyte xAxisTarget, sbyte yAxisTarget, byte selectCountTarget, sbyte rotationTarget, sbyte positionTarget, bool reachingTarget, bool holdingTarget, float stateDurationTarget)
	{
		this.xAxisTarget = xAxisTarget;
		this.xAxisDiff += this.xAxisTarget - this.xAxisCurrent;

		this.yAxisTarget = yAxisTarget;
		this.yAxisDiff += this.yAxisTarget - this.yAxisCurrent;

		this.selectCountTarget = selectCountTarget;
		this.selectCountDiff += this.selectCountCurrent < this.selectCountTarget ? this.selectCountTarget - this.selectCountCurrent : 255 + this.selectCountTarget - this.selectCountCurrent;

		this.rotationTarget = rotationTarget;
		this.rotationDiff += Mathf.Abs(this.rotationTarget - this.rotationCurrent) < 127 ? this.rotationTarget - this.rotationCurrent : 255 + this.rotationTarget - this.rotationCurrent;

		this.positionTarget = positionTarget;
		this.positionDiff += this.positionTarget - this.positionCurrent;

		this.reachingTarget = reachingTarget ? 1f : 0f;
		this.reachingDiff += this.reachingTarget - this.reachingCurrent;

		this.holdingTarget = holdingTarget ? 1f : 0f;
		this.holdingDiff += this.holdingTarget - this.holdingCurrent;

		this.stateDurationTarget = stateDurationTarget;
		this.stateDurationDiff += this.stateDurationTarget - this.stateDurationCurrent;
	}

	public void setStateCurrent(sbyte xAxisCurrent, sbyte yAxisCurrent, byte selectCountCurrent, sbyte rotationCurrent, sbyte positionCurrent, bool reachingCurrent, bool holdingCurrent)
	{
		this.xAxisTarget = xAxisCurrent;
		this.xAxisDiff += this.xAxisTarget - this.xAxisCurrent;
		this.xAxisCurrent = this.xAxisTarget;


		this.yAxisTarget = yAxisTarget;
		this.yAxisDiff += this.yAxisTarget - this.yAxisCurrent;
		this.yAxisCurrent = this.yAxisTarget;


		this.selectCountTarget = selectCountCurrent;
		this.selectCountDiff += this.selectCountCurrent < this.selectCountTarget ? this.selectCountTarget - this.selectCountCurrent : 255 + this.selectCountTarget - this.selectCountCurrent;
		this.selectCountCurrent = this.selectCountTarget;

		this.rotationTarget = rotationCurrent;
		this.rotationDiff += Mathf.Abs(this.rotationTarget - this.rotationCurrent) < 127 ? this.rotationTarget - this.rotationCurrent : 255 + this.rotationTarget - this.rotationCurrent;
		this.rotationCurrent = this.rotationTarget;


		this.positionTarget = positionCurrent;
		this.positionDiff += this.positionTarget - this.positionCurrent;
		this.positionCurrent = this.positionTarget;

		this.reachingCurrent =  this.reachingTarget = reachingCurrent ? 1f : 0f;


		this.reachingTarget = reachingCurrent ? 1f : 0f;
		this.reachingDiff += this.reachingTarget - this.reachingCurrent;
		this.reachingCurrent = this.reachingTarget;

		this.holdingTarget = holdingCurrent ? 1f : 0f;
		this.holdingDiff += this.holdingTarget - this.holdingCurrent;
		this.holdingCurrent = this.holdingTarget;

		this.stateDurationTarget = 0f;
		this.stateDurationDiff += this.stateDurationTarget - this.stateDurationCurrent;
		this.stateDurationCurrent = this.stateDurationTarget;
	}
	public float[] readStateDiffs()
	{
		return new float[]{
			this.xAxisDiff,
			this.yAxisDiff,
			this.selectCountDiff,
			this.rotationDiff,
			this.positionDiff,
			this.reachingDiff,
			this.holdingDiff,
			this.stateDurationDiff
		};
	}

	public void eraseStateDiffs()
	{
		this.xAxisDiff
			= this.yAxisDiff
			= this.selectCountDiff
			= this.rotationDiff
			= this.positionDiff
			= this.reachingDiff
			= this.holdingDiff
			= this.stateDurationDiff
			= 0f;
	}

	public float [] readAndEraseStateDiffs()
	{
		float[] ans = readStateDiffs();
		eraseStateDiffs();
		return ans;
	}

	void render()
	{
		float positionCurrentInverseLerp = Mathf.InverseLerp(0f, 40f, this.positionCurrent);
		float positionCurrentLerp = Mathf.Lerp(0f, 10f, positionCurrentInverseLerp);
		this.transform.position = new Vector3(i * (diameter + offset), positionCurrentLerp, j * (diameter + offset));

		this.transform.localScale = new Vector3(diameter, height / 2, diameter);
		this.transform.rotation = Quaternion.identity;

		float rotationCurrentInverseLerp = Mathf.InverseLerp(-128f, 127f, this.rotationCurrent);
		float rotationCurrentLerp = Mathf.Lerp(-360f * 8, 360f * 8, rotationCurrentInverseLerp) % 360f;
		this.transform.RotateAround(this.transform.position - new Vector3(0f, height / 2, 0f), Vector3.up, rotationCurrentLerp);
		
		float yAxisCurrentInverseLerp = Mathf.InverseLerp(-128f, 127f, this.yAxisCurrent);
		float yAxisCurrentLerp = Mathf.Lerp(-45f, 45f, yAxisCurrentInverseLerp);
		this.transform.RotateAround(this.transform.position - new Vector3(0f, height / 2, 0f), Vector3.left, yAxisCurrentLerp);
		
		float xAxisCurrentInverseLerp = Mathf.InverseLerp(-128f, 127f, this.xAxisCurrent);
		float xAxisCurrentLerp = Mathf.Lerp(-45f, 45f, xAxisCurrentInverseLerp);
		this.transform.RotateAround(this.transform.position - new Vector3(0f, height / 2, 0f), Vector3.back, xAxisCurrentLerp);
		
		this.transform.GetComponent<MeshRenderer>().material.color = this.colorCurrent;

	}

	// Update is called once per frame
	void Update()
    {

		if (this.stateDurationTarget > 0f)
		{
			this.xAxisCurrent += (this.xAxisTarget - this.xAxisCurrent) / this.stateDurationTarget * Time.deltaTime;
			this.yAxisCurrent += (this.yAxisTarget - this.yAxisCurrent) / this.stateDurationTarget * Time.deltaTime;
			this.selectCountCurrent += (this.selectCountTarget - this.selectCountCurrent) / this.stateDurationTarget * Time.deltaTime;
			this.rotationCurrent += (this.rotationTarget - this.rotationCurrent) / this.stateDurationTarget * Time.deltaTime;
			this.positionCurrent += (this.positionTarget - this.positionCurrent) / this.stateDurationTarget * Time.deltaTime;
			this.reachingCurrent += (this.reachingTarget - this.reachingCurrent) / this.stateDurationTarget * Time.deltaTime;
			this.holdingCurrent += (this.holdingTarget - this.holdingCurrent) / this.stateDurationTarget * Time.deltaTime;
			if(this.i == 4 && this.j == 5) Debug.Log("["+ this.i + "," + this.j + "] " + this.positionCurrent);

			this.stateDurationTarget -= Time.deltaTime;
		}
		
		if (this.colorDurationTarget > 0f)
		{

			this.colorCurrent += (this.colorTarget - this.colorCurrent) / this.colorDurationTarget * Time.deltaTime;
			this.colorDurationTarget -= Time.deltaTime;
		}

		render();

	}
}
