using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(HackCapacity))]
public class HackrayMaterialVariation : MonoBehaviour
{
    // this class is used to handle the material variation of hackrays
    // -> changes the Intensity property of the material based on fully random for now

    [Header("Hackray Parameters")]
    private float hackray_base_intensity = 0f;
    // [SerializeField] private GameObject hackray_prefab;
    [HideInInspector] public Material hackray_material;

    [Header("Hackray Variation Settings")]
    [SerializeField] private float intensity_variation = 0f;
    [SerializeField] private float max_intensity_variation = 50f;
    [SerializeField] private float max_delay = 0.2f;

    // START
    void Start()
    {
        // on duplique le material des hackrays
        GameObject hk = Instantiate(GetComponent<HackCapacity>().hackray_prefab, transform);
        hackray_material = new Material(hk.transform.Find("sr").GetComponent<SpriteRenderer>().sharedMaterial);
        hackray_base_intensity = hackray_material.GetFloat("_Intensity");
        Destroy(hk);

        // on lance la variation
        applyVariation();
    }

    // functions
    private void applyVariation()
    {
        // on récupère un random
        intensity_variation = Random.Range(0f, max_intensity_variation);
        float delay = Random.Range(0f, max_delay);

        // on met à jour le material des hackrays
        hackray_material.SetFloat("_Intensity", hackray_base_intensity + intensity_variation);

        // on appelle la fonction dans next_variation_time secondes
        Invoke("applyVariation", delay);
    }

}