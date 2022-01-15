using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.VFX;

public class LightManager : MonoBehaviour
{
    [SerializeField, Header("Managed Objects")] private Light DirectionalLight = null;
    [SerializeField] private LightPreset DayNightPreset, LampPreset;
    private List<Light> SpotLights = new List<Light>();

    public List<GameObject> Windows = new List<GameObject>();
    public List<GameObject> shuffledWindows = new List<GameObject>();
    public List<GameObject> windowsPub = new List<GameObject>();

    float time;
    float timeDelay;
    float exposure;

    public Material skyboxDay;
    public Material skyboxNight;

    //public ParticleSystem fireVFX;
    //public ParticleSystem estinguishVFX;
    //public Light fireLight;


    [SerializeField, Range(0, 1440), Header("Modifiers"), Tooltip("The game's current time of day")] private float TimeOfDay;
    [SerializeField, Tooltip("Angle to rotate the sun")] private float SunDirection = 170f;
    [SerializeField, Tooltip("How fast time will go")] private float TimeMultiplier = 1;
    [SerializeField] private bool ControlLights = true;

    private const float inverseDayLength = 1f / 1440f;


    /// <summary>
    /// On project start, if controlLights is true, collect all non-directional lights in the current scene and place in a list
    /// </summary>
    private void Start()
    {
        //fireLight.enabled = false;

        if (ControlLights)
        {
            Light[] lights = FindObjectsOfType<Light>();
            foreach (Light li in lights)
            {
                switch (li.type)
                {
                    case LightType.Disc:
                    case LightType.Point:
                    case LightType.Rectangle:
                    case LightType.Spot:
                        SpotLights.Add(li);
                        break;
                    case LightType.Directional:
                    default:
                        break;
                }
            }
        }

        // Add all game objects tagged window in a list and set light off
        foreach (GameObject window in GameObject.FindGameObjectsWithTag("window"))
        {
            Windows.Add(window);
            window.SetActive(false);
        }

        // Add all game objects of the pub in a different list and set light off
        foreach (GameObject window in GameObject.FindGameObjectsWithTag("Windows Pub"))
        {
            windowsPub.Add(window);
            window.SetActive(false);
        }

        time = 0f;
        timeDelay = 10f;


    }

    /// <summary>
    /// This method will not run if there is no preset set
    /// On each frame, this will calculate the current time of day factoring game time and the time multiplier (1440 is how many minutes exist in a day 24 x 60)
    /// Then send a time percentage to UpdateLighting, to evaluate according to the set preset, what that time of day should look like
    /// </summary>
    private void Update()
    {
        if (DayNightPreset == null)
            return;

        TimeOfDay = TimeOfDay + (Time.deltaTime * TimeMultiplier);
        TimeOfDay = TimeOfDay % 1440;
        UpdateLighting(TimeOfDay * inverseDayLength);

    }

    /// <summary>
    /// Based on the time percentage recieved, set the current scene's render settings and light coloring to the preset
    /// In addition, rotate the directional light (the sun) according to the current time
    /// </summary>
    /// <param name="timePercent"></param>
    private void UpdateLighting(float timePercent)
    {

        RenderSettings.ambientLight = DayNightPreset.AmbientColour.Evaluate(timePercent);
        RenderSettings.fogColor = DayNightPreset.FogColour.Evaluate(timePercent);

        //Set the directional light (the sun) according to the time percent
        if (DirectionalLight != null)
        {
            if (DirectionalLight.enabled == true)
            {
                DirectionalLight.color = DayNightPreset.DirectionalColour.Evaluate(timePercent);


                DirectionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, SunDirection, 0));

            }
        }

        //Go through each spot light, ensure it is active, and set it's color accordingly
        foreach (Light lamp in SpotLights)
        {
            if (lamp != null)
            {
                if (lamp.isActiveAndEnabled && lamp.shadows != LightShadows.None && LampPreset != null)
                {
                    lamp.color = LampPreset.DirectionalColour.Evaluate(timePercent);
                    
                }
            }

            //If it's night, go through each window and set it active after a delay

            shuffledWindows = Windows.OrderBy(x => Random.value).ToList();


            if (TimeOfDay > 1200 || TimeOfDay < 300)
            {
                //fireVFX.Play();
                //fireLight.enabled = true;

                //AudioSource fireCrackling = GameObject.FindGameObjectWithTag("Firepit").GetComponent<AudioSource>();
                //fireCrackling.mute = false;



                foreach (AudioSource source in GameObject.FindGameObjectWithTag("MainCamera").GetComponents<AudioSource>())
                {
                    if (source.clip.name.Equals("AmbienceForest"))
                        source.mute = true;


                    if (source.clip.name.Equals("crickets-at-night"))
                        source.mute = false;
                }

              

                if (RenderSettings.skybox = skyboxDay)
                RenderSettings.skybox = skyboxNight;

                if (exposure < 1)
                {
                    exposure += 0.001f;
                    RenderSettings.skybox.SetFloat("_Exposure", exposure);

                }

                foreach (GameObject window in shuffledWindows)
                {
                    time = time + 1f * Time.deltaTime;


                    if (time >= timeDelay)
                    {
                        window.SetActive(true);
                        time = 0f;
                       
                    }


                }

                foreach (GameObject window in windowsPub)
                {
                    window.SetActive(true);
                }

            }
            //If it's day time, go through each window and set it inactive after a delay 

            else
            {
                //if (fireLight.enabled == true)
                //{
                //    estinguishVFX.Play();
                //    fireVFX.Stop();
                //    fireLight.enabled = false;
                //}

                //AudioSource fireCrackling = GameObject.FindGameObjectWithTag("Firepit").GetComponent<AudioSource>();
                //fireCrackling.mute = true;

                foreach (AudioSource source in GameObject.FindGameObjectWithTag("MainCamera").GetComponents<AudioSource>())
                {
                    if (source.clip.name.Equals("crickets-at-night"))
                        source.mute = true;

                    if (source.clip.name.Equals("AmbienceForest"))
                        source.mute = false;

                }
             
                if (RenderSettings.skybox = skyboxNight)
                    RenderSettings.skybox = skyboxDay;
                exposure = 0f;


                foreach (GameObject window in shuffledWindows)
                {
                    time = time + 1f * Time.deltaTime;


                    if (time >= timeDelay)
                    {
                        window.SetActive(false);
                        time = 0f;
                    }

                }

                foreach (GameObject window in windowsPub)
                {
                    window.SetActive(false);
                }

            }

        }

    }

  
}
