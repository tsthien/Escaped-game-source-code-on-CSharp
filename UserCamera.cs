﻿using UnityEngine;
using System.Collections;
using System.CodeDom;
using Photon.Pun;
using Photon.Pun.UtilityScripts;

public class UserCamera : MonoBehaviour {
	public float _deltaDistance;
	public InstantiateRotate InsRotate;
	public Transform target; 							// Target to follow
	public float targetHeight = 3f; 						// Vertical offset adjustment
	public float distance = 12f;							// Default Distance
	public float offsetFromWall = 2f;						// Bring camera away from any colliding objects
	public float maxDistance = 20; 						// Maximum zoom Distance
	public float minDistance = 4f; 						// Minimum zoom Distance
	public float xSpeed = 160.0f; 							// Orbit speed (Left/Right)
	public float ySpeed = 160.0f; 							// Orbit speed (Up/Down)
	public float yMinLimit = -85; 							// Looking up limit
	public float yMaxLimit = 85; 							// Looking down limit
	public float zoomRate = 20; 							// Zoom Speed
	public float rotationDampening = 3.0f; 				// Auto Rotation speed (higher = faster)
	public float zoomDampening = 3.0f; 					// Auto Zoom speed (Higher = faster)
	LayerMask collisionLayers = -1;		// What the camera will collide with

	public bool lockToRearOfTarget;				
	public bool allowMouseInputX = true;				
	public bool allowMouseInputY = true;				
	
	private float xDeg = 0.0f; 
	private float yDeg = 0.0f; 
	private float currentDistance; 
	public float desiredDistance; 
	private float correctedDistance; 
	//private bool rotateBehind;
	
	public GameObject userModel;
	public bool inFirstPerson;
		
	void Start () { 

		Vector3 angles = transform.eulerAngles; 
		xDeg = angles.x; 
		yDeg = angles.y; 
		currentDistance = distance; 
		desiredDistance = distance; 
		correctedDistance = distance; 
		
		// Make the rigid body not change rotation 
		if (GetComponent<Rigidbody>()) 
			GetComponent<Rigidbody>().freezeRotation = true;
		
	} 
	
	void Update () {
		
		if (_deltaDistance != 0) {
			
			if(inFirstPerson == true) {
				
				minDistance = 2; //10
				//desiredDistance = 15;
				//userModel.SetActive(true);
				//inFirstPerson = false;
			}
		}
		
		if(desiredDistance == 10) {
			
			minDistance = 2; //0
			desiredDistance = 5; //0
			//userModel.SetActive(false);
			inFirstPerson = true;
		}	
	}
	
	//Only Move camera after everything else has been updated
	void LateUpdate () 
	{ 

		// Don't do anything if target is not defined 
		if (!target) 
			return;
		
		//Vector3 vTargetOffset3;

		// If either mouse buttons are down, let the mouse govern camera position 
		if (GUIUtility.hotControl == 0)
		{
			if(Input.GetKey(KeyCode.LeftControl)) {
				
			}
			else {
				if (InsRotate.TouchDist.x != 0 || InsRotate.TouchDist.y != 0)
				{ 
					//Check to see if mouse input is allowed on the axis
					if (allowMouseInputX)
						xDeg += InsRotate.TouchDist.x * xSpeed * 0.02f; 
					if (allowMouseInputY)
						yDeg -= InsRotate.TouchDist.y * ySpeed * 0.02f; 

				} 
			}
		}
		ClampAngle (yDeg); 
		
		// Set camera rotation 
		Quaternion rotation = Quaternion.Euler (yDeg, xDeg, 0);

		// Calculate the desired distance 
		//desiredDistance -= _deltaDistance * Time.deltaTime * zoomRate * Mathf.Abs (desiredDistance);
		desiredDistance = _deltaDistance;
		desiredDistance = Mathf.Clamp (desiredDistance, minDistance, maxDistance);
		correctedDistance = desiredDistance; 
		
		// Calculate desired camera position
		Vector3 vTargetOffset = new Vector3 (0, -targetHeight, 0);
		Vector3 position = target.position - (rotation * Vector3.forward * desiredDistance + vTargetOffset); 
		
		// Check for collision using the true target's desired registration point as set by user using height 
		RaycastHit collisionHit; 
		Vector3 trueTargetPosition = new Vector3 (target.position.x, target.position.y + targetHeight, target.position.z); 
		
		// If there was a collision, correct the camera position and calculate the corrected distance 

		bool isCorrected = false; 
		if (Physics.Linecast (trueTargetPosition, position,out collisionHit, collisionLayers) && ((int)PhotonNetwork.LocalPlayer.CustomProperties["code"] != 2 || collisionHit.transform.tag == "ground")) 
		{ 
			correctedDistance = Vector3.Distance (trueTargetPosition, collisionHit.point) - offsetFromWall; 
			isCorrected = true;
		}

		
		// For smoothing, lerp distance only if either distance wasn't corrected, or correctedDistance is more than currentDistance 
		currentDistance = !isCorrected || correctedDistance > currentDistance ? Mathf.Lerp (currentDistance, correctedDistance, Time.deltaTime * zoomDampening) : correctedDistance;

		// Keep within limits
		currentDistance = Mathf.Clamp (currentDistance, minDistance, maxDistance); 
		
		// Recalculate position based on the new currentDistance 
		position = target.position - (rotation * Vector3.forward * currentDistance + vTargetOffset); 
		
		//Finally Set rotation and position of camera
		transform.rotation = rotation; 
		transform.position = position; 
	}
	void ClampAngle (float angle)
	{
		if (angle < -360)
			angle += 360;
		if (angle > 360)
			angle -= 360;

		yDeg = Mathf.Clamp(angle, -60, 80);
	}

}