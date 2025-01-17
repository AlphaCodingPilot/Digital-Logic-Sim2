﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chip : MonoBehaviour {

	public string chipName = "Untitled";
	public Pin[] inputPins;
	public Pin[] outputPins;
	int[] debugInput;

	// Number of input signals received (on current simulation step)
	int numInputSignalsReceived;
	int lastSimulatedFrame;
	int lastSimulationInitFrame;

	// Cached components
	[HideInInspector]
	public BoxCollider2D bounds;

	protected virtual void Awake () {
		bounds = GetComponent<BoxCollider2D> ();
		Simulation.onStoreInputDebug += StoreDebugInput;
		Simulation.onDebugStep += DebugStep;
	}

	protected virtual void Start () {
		SetPinIndices ();
	}

	void StoreDebugInput () {
		debugInput = StateOfInputPins ();
	}

	void DebugStep() {
		ProcessOutput (debugInput);
	}

	public void InitSimulationFrame () {
		if (lastSimulationInitFrame != Simulation.simulationFrame) {
			lastSimulationInitFrame = Simulation.simulationFrame;
			ProcessCycleAndUnconnectedInputs ();
		}
	}

	// Receive input signal from pin: either pin has power, or pin does not have power.
	// Once signals from all input pins have been received, calls the ProcessOutput() function, unless debug mode is enabled.
	public virtual void ReceiveInputSignal (Pin pin) {

		// Reset if on new step of simulation
		if (lastSimulatedFrame != Simulation.simulationFrame) {
			lastSimulatedFrame = Simulation.simulationFrame;
			numInputSignalsReceived = 0;
			InitSimulationFrame ();
		}

		if (Simulation.debugMode) {
			return;
		}

		numInputSignalsReceived++;

		if (numInputSignalsReceived == inputPins.Length) {
			ProcessOutput (StateOfInputPins ());
		}
	}

	int[] StateOfInputPins () {
		List<int> stateOfPins = new List<int> ();
		foreach (Pin pin in inputPins)
		{
			stateOfPins.Add(pin.State);
		}
		return stateOfPins.ToArray();
	}

	void ProcessCycleAndUnconnectedInputs () {
		for (int i = 0; i < inputPins.Length; i++) {
			if (inputPins[i].cyclic) {
				ReceiveInputSignal (inputPins[i]);
			} else if (!inputPins[i].parentPin) {
				inputPins[i].ReceiveSignal (0);
				//ReceiveInputSignal (inputPins[i]);
			}
		}
	}

	// Called once all inputs to the component are known.
	// Sends appropriate output signals to output pins
	protected virtual void ProcessOutput (int[] input) {

	}

	void SetPinIndices () {
		for (int i = 0; i < inputPins.Length; i++) {
			inputPins[i].index = i;
		}
		for (int i = 0; i < outputPins.Length; i++) {
			outputPins[i].index = i;
		}
	}

	public Vector2 BoundsSize {
		get {
			return bounds.size;
		}
	}
}