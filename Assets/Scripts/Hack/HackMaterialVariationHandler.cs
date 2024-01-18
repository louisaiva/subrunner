using UnityEngine;
using System.Collections.Generic;

public class HackMaterialVariationHandler : MonoBehaviour
{
    // this class is used to handle the material variation of hackrays
    // -> changes the Intensity property of the material based on fully random for now
    // -> also changes the perso Intensity property accordingly
    // -> when perso has no bits, changes the perso Intensity property to -1

    // perso
    [SerializeField] private GameObject perso;
    private float perso_base_intensity = 0f;
    [SerializeField] private Material perso_material;
    [SerializeField] private Material perso_base_material;
    
    // hackrays
    [SerializeField] private List<Hackray> hackrays = new List<Hackray>();
    private float hackray_base_intensity = 0f;
    [SerializeField] private Material hackray_material;

    // random
    [SerializeField] private float intensity_variation = 0f;
    [SerializeField] private float max_intensity_variation = 50f;
    [SerializeField] private float max_delay = 0.2f;

    // unity functions
    void Start()
    {
        // on récupère le perso
        perso = GameObject.Find("/perso");

        // on duplique le material du perso
        perso_base_material = new Material(perso.GetComponent<SpriteRenderer>().sharedMaterial);
        perso_material = new Material(perso_base_material);
        perso_base_intensity = perso_material.GetFloat("_Intensity");

        // on applique le material au perso
        perso.GetComponent<SpriteRenderer>().sharedMaterial = perso_base_material;

        // on duplique le material des hackrays
        GameObject hk = Instantiate(Resources.Load("prefabs/hacks/hackray") as GameObject, transform.parent);
        hackray_material = new Material(hk.transform.Find("sr").GetComponent<SpriteRenderer>().sharedMaterial);
        hackray_base_intensity = hackray_material.GetFloat("_Intensity");
        Destroy(hk);

        // on lance la variation
        applyVariation();
    }

    void Update()
    {
        // on met à jour les hackrays
        updateHackrays();

        // on met à jour le perso
        if (hackrays.Count > 0 && perso.GetComponent<SpriteRenderer>().sharedMaterial == perso_base_material)
        {
            // on applique le material au perso
            perso.GetComponent<SpriteRenderer>().material = perso_material;
        }
        else if (hackrays.Count == 0 && perso.GetComponent<SpriteRenderer>().sharedMaterial != perso_base_material)
        {
            // on applique le material au perso
            perso.GetComponent<SpriteRenderer>().material = perso_base_material;
        }

        // on vérifie le nombre de bits du perso
        if (perso.GetComponent<Perso>().bits < 1f)
        {
            // on met à jour le material du perso
            perso_material.SetFloat("_Intensity", -1f);
            perso_base_material.SetFloat("_Intensity", -1f);
        }
        else
        {
            // on met à jour le material du perso
            perso_material.SetFloat("_Intensity", perso_base_intensity + intensity_variation);
            perso_base_material.SetFloat("_Intensity", perso_base_intensity);
        }
    }

    void updateHackrays()
    {
        // on parcourt les enfants du transform
        for (int i=0; i<transform.childCount; i++)
        {
            // on récupère l'enfant
            Transform child = transform.GetChild(i);
            
            // on vérifie que c'est un Hackray et non un HackrayHoover
            if (child.GetComponent<Hackray>() != null && child.GetComponent<HackrayHoover>() == null)
            {
                // on récupère le hackray
                Hackray hackray = child.GetComponent<Hackray>();

                // on regarde s'il est dans la liste
                if (!hackrays.Contains(hackray))
                {
                    // on l'ajoute
                    hackrays.Add(hackray);
                    hackray.transform.Find("sr").GetComponent<SpriteRenderer>().material = hackray_material;
                }
            }
        }

        // on parcourt les hackrays
        List<Hackray> hackrays_to_remove = new List<Hackray>();
        foreach (Hackray hackray in hackrays)
        {
            // on vérifie si le hackray est toujours valide
            if (hackray == null)
            {
                // on enlève le hackray
                hackrays_to_remove.Add(hackray);
            }
        }
        foreach (Hackray hackray in hackrays_to_remove)
        {
            hackrays.Remove(hackray);
        }
    }


    // functions
    private void applyVariation()
    {
        // on récupère un random
        intensity_variation = Random.Range(0f, max_intensity_variation);
        float delay = Random.Range(0f, max_delay);

        // on met à jour le material du perso
        perso_material.SetFloat("_Intensity", perso_base_intensity + intensity_variation);

        // on met à jour le material des hackrays
        hackray_material.SetFloat("_Intensity", hackray_base_intensity + intensity_variation);

        // on appelle la fonction dans next_variation_time secondes
        Invoke("applyVariation", delay);
    }

}