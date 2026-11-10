using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingBreak : MonoBehaviour
{
    private GameObject parentBuilding;
    private BuildingHealth _parentScript;
    private Rigidbody _rb;
    
    // Start is called before the first frame update
    void Start()
    {
        parentBuilding = transform.parent.gameObject;
        _parentScript = parentBuilding.GetComponent<BuildingHealth>();
        _rb = gameObject.GetComponent<Rigidbody>();
        _rb.isKinematic = true;

        GetComponent<MeshRenderer>().material = _parentScript.buildingMat[0];
    }

    // Update is called once per frame
    void Update()
    {
        if (_parentScript.blocksBroken)
        {
            //fall
            _rb.isKinematic = false;
            //change material
            GetComponent<MeshRenderer>().material = _parentScript.buildingMat[1];
        }
    }
}
