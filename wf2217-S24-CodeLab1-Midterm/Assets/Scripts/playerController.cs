using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour
{
    [Header("Camera")] 
    [SerializeField] private Camera _camera;
    [SerializeField] private float camSpeed;
    private float xRotation;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CameraControl();
    }

    void CameraControl()
    {
        //get mouse with input system
        float mouseX = Input.GetAxis("Mouse X") * camSpeed * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * camSpeed * Time.deltaTime;
        
        //set x ration according to mouse Y
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
        //rotate player according to mouse x asis
        transform.Rotate(Vector3.up * mouseX);
        //rotate yaw of camera
        _camera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
    }
}
