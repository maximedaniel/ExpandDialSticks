using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpanDialStick : MonoBehaviour
{
	public float diameter = 4.0f;
	public float height = 10.0f;
	public float offset = 0.5f;

	public int i = 0;
	public int j = 0;

	private float xAxisCurrent = 0f;
	private float xAxisTarget = 0f;

	private float yAxisCurrent = 0f;
	private float yAxisTarget = 0f;

	private float selectCountCurrent = 0f;
	private float selectCountTarget = 0f;

	private float rotationCurrent = 0f;
	private float rotationTarget = 0f;

	private float positionCurrent = 0f;
	private float positionTarget = 0f;
	private float stateDuration = 1f;


	private float reachingCurrent = 0f;
	private float reachingTarget = 0f;

	private float holdingCurrent = 0f;
	private float holdingTarget = 0f;

	private Color colorCurrent = Color.white;
	private Color colorTarget = Color.white;
	private float colorDuration = 1f;



	public Material transparentMaterial;


	// Start is called before the first frame update
	void Start()
    {
		this.transform.GetComponent<MeshRenderer>().material = transparentMaterial;
		this.name = "ExpanDialStick (" + i + ", " + j + ")";
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
	public void setColorCurrent(Color colorCurrent)
	{
		this.colorCurrent = this.colorTarget = colorCurrent;
		this.colorDuration = 0f;
	}
	public void setColorTarget(Color colorTarget, float colorDuration)
	{
		this.colorTarget = colorTarget;
		this.colorDuration = colorDuration;
	}

	public void setStateCurrent(sbyte xAxisCurrent, sbyte yAxisCurrent, byte selectCountCurrent, sbyte rotationCurrent, sbyte positionCurrent, bool reachingCurrent, bool holdingCurrent)
	{
		float xAxisCurrentNormal = Mathf.InverseLerp(-128, 127, xAxisCurrent);
		this.xAxisCurrent = this.xAxisTarget = Mathf.Lerp(-45f, 45f, xAxisCurrentNormal);

		float yAxisCurrentNormal = Mathf.InverseLerp(-128, 127, yAxisCurrent);
		this.yAxisCurrent = this.yAxisTarget = Mathf.Lerp(-45f, 45f, yAxisCurrentNormal);

		this.selectCountCurrent = this.selectCountTarget = Mathf.InverseLerp(0, 255, selectCountCurrent);

		float rotationCurrentNormal = Mathf.InverseLerp(-128, 127, rotationCurrent);
		this.rotationCurrent = this.rotationTarget = Mathf.Lerp(-360f * 8, 360f * 8, rotationCurrentNormal) % 360f;

		float positionCurrentNormal = Mathf.InverseLerp(0, 40, positionCurrent);
		this.positionCurrent = this.positionTarget = Mathf.Lerp(0f, 10f, positionCurrentNormal);

		this.reachingCurrent =  this.reachingTarget = reachingCurrent ? 1f : 0f;

		this.holdingCurrent =  this.holdingTarget = holdingCurrent ? 1f : 0f;

		this.stateDuration = 0f;

	}


	public void setStateTarget(sbyte xAxisTarget, sbyte yAxisTarget, byte selectCountTarget, sbyte rotationTarget, sbyte positionTarget, bool reachingTarget, bool holdingTarget, float stateDuration)
	{
		float xAxisTargetNormal = Mathf.InverseLerp(-128, 127, xAxisTarget);
		this.xAxisTarget = Mathf.Lerp(-45f, 45f, xAxisTargetNormal);

		float yAxisTargetNormal = Mathf.InverseLerp(-128, 127, yAxisTarget);
		this.yAxisTarget = Mathf.Lerp(-45f, 45f, yAxisTargetNormal);

		this.selectCountTarget = Mathf.InverseLerp(0, 255, selectCountTarget);

		float rotationTargetNormal = Mathf.InverseLerp(-128, 127, rotationTarget);
		this.rotationTarget = Mathf.Lerp(-360f * 8, 360f * 8, rotationTargetNormal) % 360f;

		float positionTargetNormal = Mathf.InverseLerp(0, 40, positionTarget);
		this.positionTarget = Mathf.Lerp(0f, 10f, positionTargetNormal);

		this.reachingTarget = reachingTarget ? 1f : 0f;

		this.holdingTarget = holdingTarget ? 1f : 0f;

		this.stateDuration = stateDuration;
	}


	void render()
	{

		this.transform.position = new Vector3(i * (diameter + offset), this.positionCurrent, j * (diameter + offset));
		this.transform.localScale = new Vector3(diameter, height / 2, diameter);
		this.transform.rotation = Quaternion.identity;
		this.transform.RotateAround(this.transform.position - new Vector3(0f, height / 2, 0f), Vector3.up, (float)this.rotationCurrent);
		this.transform.RotateAround(this.transform.position - new Vector3(0f, height / 2, 0f), Vector3.left, (float)this.yAxisCurrent);
		this.transform.RotateAround(this.transform.position - new Vector3(0f, height / 2, 0f), Vector3.back, (float)this.xAxisCurrent);
		this.transform.GetComponent<MeshRenderer>().material.color = this.colorCurrent;

	}

	// Update is called once per frame
	void Update()
    {

		if (this.stateDuration > 0f)
		{
			this.xAxisCurrent += (this.xAxisTarget - this.xAxisCurrent) / this.stateDuration * Time.deltaTime;
			this.yAxisCurrent += (this.yAxisTarget - this.yAxisCurrent) / this.stateDuration * Time.deltaTime;
			this.selectCountCurrent += (this.selectCountTarget - this.selectCountCurrent) / this.stateDuration * Time.deltaTime;
			this.rotationCurrent += (this.rotationTarget - this.rotationCurrent) / this.stateDuration * Time.deltaTime;
			this.positionCurrent += (this.positionTarget - this.positionCurrent) / this.stateDuration * Time.deltaTime;
			this.reachingCurrent += (this.reachingTarget - this.reachingCurrent) / this.stateDuration * Time.deltaTime;
			this.holdingCurrent += (this.holdingTarget - this.holdingCurrent) / this.stateDuration * Time.deltaTime;

			this.stateDuration -= Time.deltaTime;
		}
		
		if (this.colorDuration > 0f)
		{

			this.colorCurrent += (this.colorTarget - this.colorCurrent) / this.colorDuration * Time.deltaTime;
			this.colorDuration -= Time.deltaTime;
		}

		render();

	}
}
