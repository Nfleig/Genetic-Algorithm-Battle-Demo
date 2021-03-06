﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float MovementSpeed;
    public float ZoomSpeed;

    public float MinZoomDistance;
    public float MaxZoomDistance;

    private Camera camera;
    private bool isClicking;

    private float rotX = 50;
    // Start is called before the first frame update
    void Awake()
    {
        camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        float xInput = Input.GetAxis("Horizontal");
        float zInput = Input.GetAxis("Vertical");
        if(Input.GetButtonDown("Fire2")){
            isClicking = true;
        }
        if(Input.GetButtonUp("Fire2")){
            isClicking = false;
        }
        
        Vector3 dir = transform.forward * zInput + transform.right * xInput;
        transform.position += dir * MovementSpeed * Time.deltaTime;
        if(isClicking){
            float rotY = Input.GetAxis("Mouse X") * 5;
            rotX += Input.GetAxis("Mouse Y") * 5;
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + rotY, 0);
            //camera.transform.eulerAngles = new Vector3(-rotX, 0, 0);
        }
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float distance = Vector3.Distance(transform.position, camera.transform.position);
        if(!((distance < MinZoomDistance && scroll > 0f) || (distance > MaxZoomDistance && scroll < 0f))){
            camera.transform.position += camera.transform.forward * scroll * 5;
        }

    }
}
