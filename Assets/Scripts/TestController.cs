﻿using UnityEngine;

public class TestController : MonoBehaviour {
	public TankMotor motor;
	public TankData data;

	// Use this for initialization
	void Start () {
		motor = GetComponent<TankMotor>();
		data = GetComponent<TankData>();
	}
	
	// Update is called once per frame
	void Update () {
		motor.Move(data.moveSpeed);
		motor.Turn(data.turnSpeed);
	}
}
