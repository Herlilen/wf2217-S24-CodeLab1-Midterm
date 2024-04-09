using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HugController : MonoBehaviour
{
    public GameObject person01;
    public GameObject person02;
    private Animator person01Anim;
    private Animator person02Anim;
    void Start()
    {
        person01Anim = person01.GetComponent<Animator>();
        person02Anim = person02.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (person01.activeSelf && person02.activeSelf)
        {
            person01Anim.enabled = true;
            person02Anim.enabled = true;
        }
        else
        {
            person01Anim.enabled = false;
            person02Anim.enabled = false;
        }

    }
}
