using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool debugMode;
    
    public GameObject spaceTunnel;
    public GameObject CockpitBody;
    
    [Header("Game Status & Engine Start")] 
    [SerializeField] private GameObject playerHolder;
    [SerializeField] private GameObject hatchHolder;
    [SerializeField] private GameObject hologram;
    private float hatchRotateSpeed = 180f;
    [SerializeField] Vector3 hatchRotateDirectionClose = new Vector3();
    [SerializeField] private GameObject monitorL;
    [SerializeField] private GameObject monitorR;
    private float monitorLrotationSpeedClose = 30f;
    private float monitorRrotationSpeedClose = 50f;
    private float monitorLrotationSpeedOpen = 60f;
    private float monitorRrotationSpeedOpen = 20f;
    [SerializeField] private Material _cockpitMaterial;
    private float _materialAlpha = 1;
    [SerializeField] private Material _hologramMaterial;
    private Vector2 holoValue;
    private bool hasLoadedLevel;
    private bool hasResetPositionLevel1;
    public bool engineStart;
    public bool controlReady;
    float countDownTimer = 15f;

    [Header("Player Engine Start Control")]
    public string engineStartCode = "555";
    private string inputCode = "";
    private bool gameStarted;
    private bool audEngineHasPlayed;
    private float inputCoolDown;    //avoid spamming the numbers
    private float inputCoolDownMax;

    [Header("Audio")] 
    private AudioSource _audioSource;
    [SerializeField] AudioClip[] _clips;
    private bool audSlideHasPlayed;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        gameStarted = false;
        hasLoadedLevel = false;
        hasResetPositionLevel1 = false;
        hologram.SetActive(false);
        _audioSource = GetComponent<AudioSource>();
        audSlideHasPlayed = false;
        audEngineHasPlayed = false;
        inputCoolDownMax = .2f;



        if (debugMode)
        {
            
            controlReady = true;
            SceneManager.LoadScene(2);
            
            //effect
            hologram.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (debugMode)
        {
            
            //cockpit transparent control;
            _cockpitMaterial.color = new Color(0, 0, 0, _materialAlpha);
            //hologram shader control;
            _hologramMaterial.SetVector("_Hologram_Tiling", holoValue);
            if (hatchHolder.transform.localRotation.x < 0)
            {
                hatchHolder.transform.Rotate(hatchRotateSpeed *20f* hatchRotateDirectionClose * Time.deltaTime);
                
            }
            if (hatchHolder.transform.localRotation.x >= 0)
            {

                hatchHolder.transform.localRotation = Quaternion.Euler(0,0,0);
            }
            
            //monitors movement
            if (monitorR.transform.localRotation.y > 0)
            {
                monitorR.transform.Rotate(monitorRrotationSpeedClose *20f * -Vector3.forward * Time.deltaTime);
            }
            if (monitorL.transform.localRotation.y < 0)
            {
                monitorL.transform.Rotate(monitorLrotationSpeedClose *20f * Vector3.forward * Time.deltaTime);
            }
            
            hologram.SetActive(true);
                    
            if (!hasResetPositionLevel1)    //load level 1 once
            {
                playerHolder.transform.position = new Vector3(-44.6f, 500, 48.3f);
                playerHolder.transform.rotation = Quaternion.Euler(0, 148, 0);
                hasResetPositionLevel1 = true;
            }
            
            
            if (_materialAlpha > 0)
            {
                _materialAlpha -= Time.deltaTime;
            }

            if (_materialAlpha <= 0)
            {
                _materialAlpha = 0;
            }
            
            //hologram start
            if (_hologramMaterial.GetVector("_Hologram_Tiling").y <= 64)
            {
                holoValue.y += 100 * Time.deltaTime;
            }

            if (holoValue.y >= 64)
            {
                holoValue.y = 64;
            }
            
            
            return;
        }
        
        //engine start setting
        if (inputCoolDown > 0)
        {
            inputCoolDown -= Time.deltaTime;
        }

        if (inputCoolDown <= 0)
        {
            inputCoolDown = 0;
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad0) ||
            Input.GetKeyDown(KeyCode.Keypad1) ||
            Input.GetKeyDown(KeyCode.Keypad2) ||
            Input.GetKeyDown(KeyCode.Keypad3) || 
            Input.GetKeyDown(KeyCode.Keypad4) || 
            Input.GetKeyDown(KeyCode.Keypad5) || 
            Input.GetKeyDown(KeyCode.Keypad6) || 
            Input.GetKeyDown(KeyCode.Keypad7) || 
            Input.GetKeyDown(KeyCode.Keypad8) ||
            Input.GetKeyDown(KeyCode.Keypad9) ||
            Input.GetKeyDown(KeyCode.Alpha1) ||
            Input.GetKeyDown(KeyCode.Alpha2) ||
            Input.GetKeyDown(KeyCode.Alpha3) ||
            Input.GetKeyDown(KeyCode.Alpha4) ||
            Input.GetKeyDown(KeyCode.Alpha5) ||
            Input.GetKeyDown(KeyCode.Alpha6) ||
            Input.GetKeyDown(KeyCode.Alpha7) ||
            Input.GetKeyDown(KeyCode.Alpha8) ||
            Input.GetKeyDown(KeyCode.Alpha9)
            )
        {
            if (inputCoolDown == 0)
            {
                //audio
                _audioSource.PlayOneShot(_clips[1]);
            
                //get pressed keys
                string pressedKey = Input.inputString;
                //add onto the code
                inputCode += pressedKey;
                
                //reset input cool down
                inputCoolDown = inputCoolDownMax;
            }
            
            //Check if the input code matches the access code
            if (inputCode == engineStartCode && !engineStart)
            {
                inputCode = "";
                engineStart = true;
                _audioSource.PlayOneShot(_clips[2]);
            }
            else if (inputCode.Length >= engineStartCode.Length)
            {
                //reset the input code
                inputCode = "";
                _audioSource.PlayOneShot(_clips[3]);

            }
        }
        
        HatchAndMonitorControl();     //control hatch according to engine status
    }
    
    void HatchAndMonitorControl()
    {
        //cockpit transparent control;
        _cockpitMaterial.color = new Color(0, 0, 0, _materialAlpha);
        //hologram shader control;
        _hologramMaterial.SetVector("_Hologram_Tiling", holoValue);
        
        //game ready setting
        if (engineStart && holoValue.y == 64)
        {
            controlReady = true;
        }
        else
        {
            controlReady = false;
        }
        
        //animation
        if (engineStart)
        {
            
            
            
            //hatch holder rotate
            if (hatchHolder.transform.localRotation.x < 0)
            {
                hatchHolder.transform.Rotate(hatchRotateSpeed * hatchRotateDirectionClose * Time.deltaTime);

                if (!audSlideHasPlayed)
                {
                    _audioSource.PlayOneShot(_clips[0]);
                    audSlideHasPlayed = true;
                }
            }

            if (hatchHolder.transform.localRotation.x >= 0)
            {

                hatchHolder.transform.localRotation = Quaternion.Euler(0,0,0);
            }
            
            //monitors movement
            if (monitorR.transform.localRotation.y > 0)
            {
                monitorR.transform.Rotate(monitorRrotationSpeedClose * -Vector3.forward * Time.deltaTime);
            }
            if (monitorL.transform.localRotation.y < 0)
            {
                monitorL.transform.Rotate(monitorLrotationSpeedClose * Vector3.forward * Time.deltaTime);
            }
            
            //load new level
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            if (!hasLoadedLevel && hatchHolder.transform.localRotation.x == 0)
            {
                //load to level 1 if not debug mode
             
                SceneManager.LoadScene(currentSceneIndex + 1);

                hasLoadedLevel = true;
                
                
                // vfx
                spaceTunnel.SetActive(true);
                CockpitBody.SetActive(false);
                
                

            }
            
            //alpha becomes zero && holo start
            if (currentSceneIndex == 1)
            {
                if (!hasResetPositionLevel1)    //load level 1 once
                {
                    playerHolder.transform.position = new Vector3(-44.6f, 500, 48.3f);
                    playerHolder.transform.rotation = Quaternion.Euler(0, 148, 0);
                    hasResetPositionLevel1 = true;
                }
                
                //count down for 5 seconds
                //play count down audio
                if (!audEngineHasPlayed)
                {
                    _audioSource.PlayOneShot(_clips[4]);
                    audEngineHasPlayed=true;
                }
                countDownTimer -= Time.deltaTime;

                if (countDownTimer <= 0)
                {
                    countDownTimer = 0;
                }

                if (countDownTimer == 0)
                {
                    hologram.SetActive(true);
                    
                    //vfx
                    spaceTunnel.SetActive(false);
                    
                    if (_materialAlpha > 0)
                    {
                        _materialAlpha -= Time.deltaTime;
                    }

                    if (_materialAlpha <= 0)
                    {
                        _materialAlpha = 0;
                    }
            
                    //hologram start
                    if (_hologramMaterial.GetVector("_Hologram_Tiling").y <= 64)
                    {
                        holoValue.y += 100 * Time.deltaTime;
                    }

                    if (holoValue.y >= 64)
                    {
                        holoValue.y = 64;
                    }
                }
                
            }
        }

        /*if (!engineStart)
        {
            //alpha becomes zero
            if (_materialAlpha < 1)
            {
                _materialAlpha += Time.deltaTime;
            }
            if (_materialAlpha >= 1)
            {
                _materialAlpha = 1;
            }
            //hologram ends
            if (_hologramMaterial.GetVector("_Hologram_Tiling").y >= 0)
            {
                holoValue.y -= 100 * Time.deltaTime;
            }

            if (holoValue.y <= 0)
            {
                holoValue.y = 0;
            }

            if (_materialAlpha == 1)
            {
                //hatch Movement
                if (hatch.transform.localPosition.z < 0.1f)
                {
                    hatch.transform.localPosition += new Vector3(0,0,hatchMoveSpeed * Time.deltaTime);
                }

                if (hatch.transform.localPosition.z >= .1f)
                {
                    hatch.transform.localPosition = new Vector3(0, 0, 0.1f);
                }
            
                //hatch holder rotate
                if (hatchHolder.transform.localRotation.eulerAngles.x == 0 || hatchHolder.transform.localRotation.eulerAngles.x > 180 && hatch.transform.localPosition.z >= .1f)
                {
                    hatchHolder.transform.Rotate(hatchRotateSpeed * -hatchRotateDirectionClose * Time.deltaTime);
                }

                if (hatchHolder.transform.localRotation.x >= -180f)
                {
                    if (hatchHolder.transform.localPosition.x != 0)
                    {
                        hatchHolder.transform.localRotation = Quaternion.Euler(-180f,0,0);
                    }
                }
            
                //monitors movement
                if (monitorR.transform.rotation.eulerAngles.y < 90 || monitorR.transform.rotation.eulerAngles.y > 270)
                {
                    monitorR.transform.Rotate(monitorRrotationSpeedOpen * Vector3.forward * Time.deltaTime);
                }
                if (monitorL.transform.rotation.eulerAngles.y > 270 || (monitorL.transform.rotation.eulerAngles.y > 0 && monitorL.transform.rotation.eulerAngles.y < 10))
                {
                    monitorL.transform.Rotate(monitorLrotationSpeedOpen * -Vector3.forward * Time.deltaTime);
                }
            }
        }*/
    }
}
