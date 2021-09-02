﻿using Leap;
using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeGuard : MonoBehaviour
{
	Projector projector;


	public ExpanDialSticks pins;

	private const float bodyOutlineWidth = 0.4f;
	private const float bodySecondOutlineWidth = 0.8f;

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
	private Vector4[] _dotColors, _dotOutlineColors, _dotSecondOutlineColors;
	private float[] _dotOutlineWidths, _dotSecondOutlineWidths;
	private Vector4[] _dotLeftHandCenters, _dotRightHandCenters;
	private float[] _dotLeftHandRadius, _dotRightHandRadius;
	private Vector4[] _dotLeftBackArmCenters, _dotRightBackArmCenters;
	private Vector4[] _dotLeftFrontArmCenters, _dotRightFrontArmCenters;
	private float[] _dotLeftArmRadius, _dotRightArmRadius;
	private int _dotIndex = 0;

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

	public enum SafetyOverlayMode {Dot, Line, Zone};
	public SafetyOverlayMode overlayMode = SafetyOverlayMode.Line;

	private const int SEPARATION_LAYER = 10; // Safety Level 0


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


	public SafeGuard(ExpanDialSticks expanDialSticks)
	{
		this.pins = expanDialSticks;
	}


	// Start is called before the first frame update
	void Start()
	{
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

		_noTextures = new Texture2D[32];
		_noTextureIndexes = new float[32];
		for (int i = 0; i < _noTextures.Length; i++)
		{
			_noTextures[i] = Resources.Load<Texture2D>("white");
			_noTextureIndexes[i] = i;
		}
		int textureWidth = 512;
		int textureHeight = 512;
		_noTextureArray = new Texture2DArray(textureWidth, textureHeight, _noTextures.Length, TextureFormat.DXT1Crunched, false);
		for (int i = 0; i < _noTextures.Length; i++)
		{
			Graphics.CopyTexture(_noTextures[i], 0, 0, _noTextureArray, i, 0); // i is the index of the texture
		}
		_dotMat.SetTexture("_Textures", _noTextureArray);
		_lineMat.SetTexture("_Textures", _noTextureArray);
		Texture2D[] _iconTextures = new Texture2D[32];
		_iconTextureIndexes = new float[32];
		for (int i = 0; i < _iconTextures.Length; i++)
		{
			_iconTextures[i] = Resources.Load<Texture2D>("moon");
			_iconTextureIndexes[i] = i;
		}
		_iconTextureArray = new Texture2DArray(textureWidth, textureHeight, _iconTextures.Length, TextureFormat.DXT5Crunched, false);
		for (int i = 0; i < _iconTextures.Length; i++)
		{
			Graphics.CopyTexture(_iconTextures[i], 0, 0, _iconTextureArray, i, 0); // i is the index of the texture
		}
		_planeMat.SetTexture("_Textures", _iconTextureArray);

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

		projector = this.GetComponent<Projector>();
	}

    // Update is called once per frame
    void Update()
    {
		_handIndex = _armIndex = _planeIndex = _dotIndex = _lineIndex = 0;

		bool toDraw = false;

		float backgroundDistance = 0f;


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


		for (int row = 0; row < pins.NbRows; row++)
		{
			for (int column = 0; column < pins.NbColumns; column++)
			{
				int displacement = pins.viewMatrix[row, column].CurrentPaused;
				Transform pin = pins.viewMatrix[row, column].transform;
				if (displacement != 0)
				{
					toDraw = true;
					// Generate dots adjust dot diameter under body

					Vector3 dotPos = pin.position;
					Quaternion dotRot = pin.rotation;
					float distance = pins.viewMatrix[row, column].CurrentDistance;
					
					float minOrthographicSize = pins.diameter - 1f; 
					float maxOrthographicSize = minOrthographicSize * 3f;
					float minOutlineWidth = 1.2f;
					float maxOutlineWidth = minOutlineWidth * 3f;
					float minSecondOutlineWidth = 2.6f;
					float maxSecondOutlineWidth = minSecondOutlineWidth * 3f;

					float minScaleDistance = 0f;
					float maxScaleDistance = minOrthographicSize;
					float scaleDistanceCoeff = 1f - (Mathf.Clamp(distance, minScaleDistance, maxScaleDistance) - minScaleDistance) / (maxScaleDistance - minScaleDistance);
					float dotDiameter = Mathf.Lerp(minOrthographicSize, maxOrthographicSize, scaleDistanceCoeff);
					Vector3 dotScale = new Vector3(dotDiameter, dotDiameter, dotDiameter);
					_dotMatrices[_dotIndex] = Matrix4x4.TRS(
						dotPos,
						dotRot,
						dotScale
					);
					Color dotColor = (displacement > 0f) ? Color.Lerp(Color.white, Color.red, displacement/40f) : Color.Lerp(Color.blue, Color.white, (40f - displacement) / 40f);
					_dotColors[_dotIndex] = dotColor;
					_dotOutlineColors[_dotIndex] = new Vector4(0f, 0f, 0f, 1f);
					_dotOutlineWidths[_dotIndex] = Mathf.Lerp(minOutlineWidth, maxOutlineWidth, scaleDistanceCoeff);
					_dotSecondOutlineColors[_dotIndex] = new Vector4(1f, 1f, 1f, 1f);
					_dotSecondOutlineWidths[_dotIndex] = Mathf.Lerp(minSecondOutlineWidth, maxSecondOutlineWidth, scaleDistanceCoeff);
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


					Vector3 planePos = pin.position + pin.up * ((dotDiameter - minOrthographicSize + 0.1f) + pins.height / 2.0f);
					Quaternion planeRot = pin.rotation; // * Quaternion.AngleAxis(90, pin.right);
					Vector3 planeScale = new Vector3(dotDiameter, 0.1f, dotDiameter);
					_planeMatrices[_planeIndex] = Matrix4x4.TRS(
						 planePos,
						 planeRot,
						 planeScale
					);
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


					// Draw if nearest point found
					if (targetPos != Vector3.zero)
					{
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
				}
				

			}
		}
		// Handle Overlay Modes
		switch (overlayMode)
		{
			case SafetyOverlayMode.Dot:
				// hide lines
				_lineIndex = 0;
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
			case SafetyOverlayMode.Line:
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
				// hide lines
				_lineIndex = 0;
				// hide dots outlines
				for (int i = 0; i < _dotIndex; i++)
				{
					_dotOutlineColors[i] = new Vector4(1f, 1f, 1f, 0f);
				}
				for (int i = 0; i < _dotIndex; i++)
				{
					_dotSecondOutlineColors[i] = new Vector4(1f, 1f, 1f, 0f);
				}
				break;
		}

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
		// Draw planes
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


		// Combine and Draw hand & arm zones
		if (toDraw)
		{
			// Projector Transition
			float feedbackInDuration = 1f;
			float feedbackMinGamma = 0f;
			float feedbackMaxGamma = 1f;

			float recoveryRate = (feedbackMaxGamma - feedbackMinGamma) / feedbackInDuration;
			float projectorAlpha = Mathf.MoveTowards(projector.material.color.a, feedbackMaxGamma, recoveryRate * Time.deltaTime);
			projector.material.color = new Color(1f, 1f, 1f, projectorAlpha);
		} else
		{
			float feedbackOutDuration = 0.250f;
			float feedbackMinGamma = 0f;
			float feedbackMaxGamma = 1f;
			float recoveryRate = (feedbackMaxGamma - feedbackMinGamma) / feedbackOutDuration;
			float projectorAlpha = Mathf.MoveTowards(projector.material.color.a, feedbackMinGamma, recoveryRate * Time.deltaTime);
			projector.material.color = new Color(1f, 1f, 1f, projectorAlpha);
		}

	}
}
