#define DEBUG

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using TMPro;
using System;
using System.Globalization;
using System.Threading;

public class SafetyBenchApp : MonoBehaviour
{

	public GameObject expanDialSticksPrefab;
	private ExpanDialSticks expanDialSticks;
	private bool connected = false;
	private Vector4 targetPinMotion;
	private int pin_index;
	private float stopDelay = 1.5f;
	private float shapeChangeDuration = 2f;
	private float shapeChangeWaitFor = 3f;
	private int _pinRow = 0;
	private int _pinCol = 0;
	private sbyte _stopPos = 0;
	private sbyte _finalPos = 0;
	private sbyte _targetPos = 40;
	private float _startTime = 0;
	private float _triggerTime = 0;
	private float _endTime = 0;
	private bool _pinIsMoving = false;


	void Start()
	{
		expanDialSticks = expanDialSticksPrefab.GetComponent<ExpanDialSticks>();
		// Listen to events
		expanDialSticks.OnConnecting += HandleConnecting;
		expanDialSticks.OnConnected += HandleConnected;
		expanDialSticks.OnDisconnected += HandleDisconnected;
		expanDialSticks.OnXAxisChanged += HandleXAxisChanged;
		expanDialSticks.OnYAxisChanged += HandleYAxisChanged;
		expanDialSticks.OnClickChanged += HandleClickChanged;
		expanDialSticks.OnRotationChanged += HandleRotationChanged;
		expanDialSticks.OnActuationChanged += HandleActuationChanged;
		expanDialSticks.onHoldingChanged += HandleHoldingChanged;
		expanDialSticks.onReachingChanged += HandleReachingChanged;

		pin_index = 0;
		_pinRow = 0;
		_pinCol = 0;
		_stopPos = 0;
		_finalPos = 0;
		_targetPos = 40;
		_startTime = 0;
		_triggerTime = 0;
		_endTime = 0;
		_pinIsMoving = false;
		connected = false;
		targetPinMotion = new Vector4(0.0f, 0.0f, 40.0f, 2.0f);
		// Connection to MQTT Broker
		expanDialSticks.client_MqttConnect();
	}

	private void HandleConnecting(object sender, MqttConnectionEventArgs e)
	{
		Debug.Log("ExpanDialSticks connecting to MQTT Broker @" + e.address + ":" + e.port);
		connected = false;
	}

	private void HandleConnected(object sender, MqttConnectionEventArgs e)
	{
		Debug.Log("ExpanDialSticks connected.");
		connected = true;

	}

	private void HandleDisconnected(object sender, MqttConnectionEventArgs e)
	{
		Debug.Log("ExpanDialSticks disconnected.");
		connected = false;
	}

	private void HandleXAxisChanged(object sender, ExpanDialStickEventArgs e)
	{
		//Debug.Log("HandleXAxisChanged -> (" + e.i + '|' + e.j + '|' + e.diff + ") = " + e.next);
		
	}

	private void HandleYAxisChanged(object sender, ExpanDialStickEventArgs e)
	{
		//Debug.Log("HandleYAxisChanged -> (" + e.i + '|' + e.j + '|' + e.diff + ") = " + e.next);
	}

	private void HandleClickChanged(object sender, ExpanDialStickEventArgs e)
	{
		//Debug.Log("HandleClickChanged -> (" + e.i + '|' + e.j + '|' + e.diff + ")");
		/*users.Enqueue(e.i + '|' + e.j + '|' + CLICK);*/
	}

	private void HandleRotationChanged(object sender, ExpanDialStickEventArgs e)
	{
		//Debug.Log("HandleRotationChanged -> (" + e.i + '|' + e.j + '|' + e.diff + ")");
	}

	private void HandleActuationChanged(object sender, ExpanDialStickEventArgs e)
	{
		Debug.Log("HandleActuationChanged -> (" + e.i + '|' + e.j + '|' + e.next + ")");
		if (_pinIsMoving && e.i == _pinRow && e.j == _pinCol && e.next == _targetPos)
		{

		}
	}

	private void HandleReachingChanged(object sender, ExpanDialStickEventArgs e)
	{
		//Debug.Log("HandleReachingChanged -> (" + e.i + '|' + e.j + '|' + e.next + ")");
		if (e.i == _pinRow && e.j == _pinCol && e.prev == 1 && e.next == 0)
		{
			_endTime = Time.time;
			_finalPos = expanDialSticks.modelMatrix[_pinRow, _pinCol].CurrentPosition;
			//Debug.Log("_endTime:" + _endTime);
			float durationTime = _endTime - _startTime;
			Debug.Log("pin (" + _pinRow + ", " + _pinCol + ") move from " + _stopPos + " to " + _finalPos + " in " + durationTime + "s");
			_pinRow = _pinRow + (_pinCol + 1) / expanDialSticks.NbColumns;
			_pinCol = (_pinCol + 1) % expanDialSticks.NbColumns;
		}
	}

	private void HandleHoldingChanged(object sender, ExpanDialStickEventArgs e)
	{			

	}

	bool IsAllDown()
	{
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				if (expanDialSticks.viewMatrix[i, j].CurrentPosition > 0 || expanDialSticks.viewMatrix[i, j].CurrentReaching) return false;
			}
		}
		return true;
	}

	void AllDown(float duration)
	{
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				expanDialSticks.modelMatrix[i, j].TargetColor = Color.white;
				expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = duration;
				expanDialSticks.modelMatrix[i, j].TargetPosition = 0;
				expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = duration;

			}
		}
		expanDialSticks.triggerTextureChange();
		expanDialSticks.triggerShapeChange();
	}
	private void MovePinUp(int row, int col, float duration)
	{
		expanDialSticks.modelMatrix[row, col].TargetPosition = _targetPos;
		expanDialSticks.modelMatrix[row, col].TargetShapeChangeDuration = duration;
		expanDialSticks.triggerShapeChange();
	}
	private void PausePin(int row, int col)
	{
		expanDialSticks.modelMatrix[row, col].TargetPosition = ExpanDialSticks.STOP_POSITION;
		expanDialSticks.modelMatrix[row, col].TargetShapeChangeDuration = 0.1f;
		expanDialSticks.triggerShapeChange();
	}
	private bool IsPinUp(int row, int col)
	{
		return expanDialSticks.modelMatrix[row, col].CurrentPosition == _targetPos;
	}

	IEnumerator WaitUntilAllDown()
	{
		AllDown(shapeChangeDuration);
		Debug.Log("AllDown");
		yield return new WaitForSeconds(shapeChangeDuration);
		// wait until all pins are down
		float waitingSince = 0f;
		float waitingCount = 1;
		while (!IsAllDown())
		{
			if (waitingSince >= shapeChangeWaitFor)
			{
				if (--waitingCount <= 0) break;

				waitingSince = 0f;
				Debug.Log("AllDown again");
				AllDown(shapeChangeDuration);
			}
			else
			{
				waitingSince += 0.1f;
				yield return new WaitForSeconds(0.1f);
			}
		}
		Debug.Log("AreAllDown");
	}

	IEnumerator WaitUntilPinIsUp()
	{

		//Debug.Log("_startTime:" + _startTime);
		//MovePinUp(_pinRow, _pinCol, shapeChangeDuration);
		//_pinIsMoving = true;
		yield return new WaitForSeconds(1.5f);
		// pause pin
		/*int currPos = expanDialSticks.modelMatrix[_pinRow, _pinCol].CurrentPosition;
		_startTime = Time.time;
		Debug.Log("_startTime: " + _startTime);*/
		PausePin(_pinRow, _pinCol);

		/*while (_pinIsMoving)
		{
			yield return new WaitForSeconds(0.1f);
		}
		int finalPos = expanDialSticks.modelMatrix[_pinRow, _pinCol].CurrentPosition;
		float durationTime = _endTime - _startTime;
		_pinRow = _pinRow + (_pinCol + 1) / expanDialSticks.NbColumns;
		_pinCol = (_pinCol + 1) % expanDialSticks.NbColumns;*/

		/*_startTime = Time.time;
		MovePinUp(_pinRow, _pinCol, shapeChangeDuration);
		_pinIsMoving = true;
		//Debug.Log("MovePinUp");
		yield return new WaitForSeconds(shapeChangeDuration);
		while (_pinIsMoving)
		{
			_pinIsMoving = !IsPinUp(_pinRow, _pinCol);
			//yield return new WaitForSeconds(0.1f);
		}

		_endTime = Time.time;
		float durationTime = _endTime - _startTime;
		Debug.Log("pin (" + _pinRow + ", " + _pinCol + ") move to position " + _targetPos + " in " + durationTime + "s");
		_pinRow = _pinRow + (_pinCol + 1) / expanDialSticks.NbColumns;
		_pinCol = (_pinCol + 1) % expanDialSticks.NbColumns;*/

		// wait until all pins are down
		/*float waitingSince = 0f;
		float waitingCount = 1;
		while (!IsPinUp(_pinRow, _pinCol))
		{
			if (waitingSince >= shapeChangeWaitFor)
			{
				if (--waitingCount <= 0) break;

				waitingSince = 0f;
				//Debug.Log("MovePinUp again");
				MovePinUp(_pinRow, _pinCol, shapeChangeDuration);
			}
			else
			{
				waitingSince += 0.1f;
			}
		}
		//Debug.Log("PinIsUp");
		float endTime = Time.time;

		//Debug.Log("NextPin: " + _pinRow +", " + _pinCol);*/

	}
	void Update()
	{

		// check if ExpanDialSticks is connected
		if (connected)
		{
			float ct = Time.time;
			if (_triggerTime > 0 && ct - _triggerTime > stopDelay) // TREAT EVENTS	
			{
				_startTime = Time.time;
				_stopPos = expanDialSticks.modelMatrix[_pinRow, _pinCol].CurrentPosition;
				PausePin(_pinRow, _pinCol);
				_triggerTime = 0;
			}
			if (Input.GetKey("escape"))
			{
				Application.Quit();
			}

			if (Input.GetKeyDown(KeyCode.R))
			{
				StartCoroutine(WaitUntilAllDown());
			}
			if (Input.GetKeyDown(KeyCode.N))
			{

				MovePinUp(_pinRow, _pinCol, shapeChangeDuration);
				_triggerTime = Time.time;
			}

		}
	}
}
