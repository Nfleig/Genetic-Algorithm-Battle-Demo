using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Public Properties
    public float MovementSpeed;
    public float ZoomSpeed;
    public float MinZoomDistance;
    public float MaxZoomDistance;
    public Vector2 BoundingBoxSize;

    // Private Properties
    private Camera camera;
    private bool _isClicking;
    private float rotX = 50;


    void Awake()
    {
        camera = Camera.main;
        rotX = camera.transform.eulerAngles.x;
    }

    /*
     * Main gameplay loop
     */
    void Update()
    {
        // Get keyboard inputs

        float xInput = Input.GetAxis("Horizontal");
        float zInput = Input.GetAxis("Vertical");
        
        // Get whether the player is right clicking

        if(Input.GetButtonDown("Fire2")){
            _isClicking = true;
        }
        if(Input.GetButtonUp("Fire2")){
            _isClicking = false;
        }
        
        // Get camera movement direction

        Vector3 dir = transform.forward * zInput + transform.right * xInput;
        
        // Apply camera movement

        transform.position += dir * MovementSpeed * Time.deltaTime;

        transform.position = new Vector3(Mathf.Clamp(transform.position.x, -BoundingBoxSize.x / 2, BoundingBoxSize.x / 2),
            transform.position.y,
            Mathf.Clamp(transform.position.z, -BoundingBoxSize.y / 2, BoundingBoxSize.y / 2));
        
        // If the player is clicking then rotate the camera with their mouse

        if (_isClicking)
        {

            // Get the horizontal mouse movement

            float rotY = Input.GetAxis("Mouse X") * 5;

            // Update the camera rotation with 

            rotX += Input.GetAxis("Mouse Y") * 2;

            // Clamp the camera's vertical rotation

            rotX = Mathf.Clamp(rotX, -60, -30);

            // Apply the camera's horizontal rotation to the parent object

            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + rotY, 0);

            // Apply the camera's vertical rotation to the camera itself

            camera.transform.eulerAngles = new Vector3(-rotX, transform.eulerAngles.y, 0);
        }

        // Get the scrollwheel input

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        
        // Calculate the distance that the camera is scrolled in

        float distance = Vector3.Distance(transform.position, camera.transform.position);
        
        // If the camera is not too zoomed in or out then zoom the camera

        if(!((distance < MinZoomDistance && scroll > 0f) || (distance > MaxZoomDistance && scroll < 0f))){
            camera.transform.position += camera.transform.forward * scroll * 5;
        }

    }
}
