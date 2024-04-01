using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Cinemachine;

public class WarpSpeedScript : MonoBehaviour
{
    [Space]
    [Header("WARP EFFECT")]
    [Space]
    public VisualEffect warpSpeedVFX;
    public MeshRenderer cylinder;
    public float rate = 0.025f;
    public float cylinderDelay = 2;
    [Space]
    [Header("POST-PROCESSING")]
    [Space]
    public Volume volume;
    [Space]
    [Header("CAMERA")]
    [Space]
    public CinemachineVirtualCamera cvCam;
    public CinemachineImpulseSource impulseSource;
    public float shakeAmplitude=5;
    public float shakeFrequency=2.5f;
    public float originalFoV = 60;
    public float additionalFoV = 20;

    private bool warpActive;
    private bool warpOpen;
    private ChromaticAberration chromatic;

    void Start()
    {
        warpSpeedVFX.Stop();
        warpSpeedVFX.SetFloat("WarpAmount", 0);
        cylinder.material.SetFloat ("Active_", 0);
        //volume.profile.TryGet<ChromaticAberration>(out chromatic);
        cvCam.m_Lens.FieldOfView = originalFoV;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            warpActive = true;
            StartCoroutine(AnimateParticles());
            StartCoroutine(AnimateShader());
        }

        if(Input.GetKey(KeyCode.Space) && warpOpen)
        {            
            ShakeCameraWithImpulse(1.1f);
        }

        if(Input.GetKeyUp(KeyCode.Space))
        {
            warpActive = false;
            StartCoroutine(AnimateParticles());
            StartCoroutine(AnimateShader());
        }
    }

    IEnumerator AnimateParticles()
    {   
        if(warpActive)
        {   
            warpSpeedVFX.Play();

            float amount = warpSpeedVFX.GetFloat("WarpAmount");
            while (amount < 1 && warpActive)
            {
                amount += rate;
                warpSpeedVFX.SetFloat("WarpAmount", amount);
                chromatic.intensity.value = amount;
                ShakeCameraWithImpulse(amount);
                cvCam.m_Lens.FieldOfView = originalFoV + (additionalFoV * amount);
                yield return new WaitForSeconds (0.1f);
            }
            
            if(amount >= 1)
                warpOpen = true;
        }
        else
        {            
            warpOpen = false;
            float amount = warpSpeedVFX.GetFloat("WarpAmount");
            var fovAmount = cvCam.m_Lens.FieldOfView - originalFoV;
            var times = amount/rate;
            while (amount > 0 && !warpActive)
            {
                amount -= rate;
                warpSpeedVFX.SetFloat("WarpAmount", amount);
                chromatic.intensity.value = amount;
                ShakeCameraWithImpulse(amount);
                cvCam.m_Lens.FieldOfView -= fovAmount/times;
                yield return new WaitForSeconds (0.1f);

                if(amount <= 0+rate)
                {
                    amount = 0;
                    warpSpeedVFX.SetFloat("WarpAmount", amount);
                    chromatic.intensity.value = amount;
                    warpSpeedVFX.Stop();
                }  
            }                     
        }
    }

    IEnumerator AnimateShader()
    {
        if(warpActive)
        {   
            yield return new WaitForSeconds (cylinderDelay);
            float amount = cylinder.material.GetFloat ("Active_");
            while (amount < 1 && warpActive)
            {
                amount += rate;
                cylinder.material.SetFloat("Active_", amount);
                yield return new WaitForSeconds (0.1f);
            }
        }
        else
        {
            float amount = cylinder.material.GetFloat ("Active_");
            while (amount > 0 && !warpActive)
            {
                amount -= rate;
                cylinder.material.SetFloat("Active_", amount);
                yield return new WaitForSeconds (0.1f);

                if(amount <= 0+rate)
                {
                    amount = 0;
                    cylinder.material.SetFloat("Active_", amount);
                }
            }                        
        }
    }

    void ShakeCameraWithImpulse(float rate)
    {
        impulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_SustainTime = 0.1f;
        impulseSource.m_ImpulseDefinition.m_AmplitudeGain = shakeAmplitude * rate;
        impulseSource.m_ImpulseDefinition.m_FrequencyGain = shakeFrequency;
        impulseSource.GenerateImpulse();
    }
}
