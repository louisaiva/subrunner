using UnityEngine;

public class XPCollector : MonoBehaviour
{
    
    // GAMEOBJECTS
    public GameObject perso;
    public GameObject xp_provider;

    // detection d'xp
    LayerMask particles_layers;

    // xp
    int xp = 0;


    // start
    void Start()
    {
        // on récupère le perso
        perso = GameObject.Find("/perso");

        // on récupère le xp_provider
        xp_provider = GameObject.Find("/world/xp_provider");

        // on récupère les layers des particules
        particles_layers = LayerMask.GetMask("Particles");
    }


    void Update()
    {

        // on regarde si une particule d'XP est à proximité
        /* 
        Collider2D[] particles = Physics2D.OverlapCircleAll(transform.position, 0.5f, particles_layers);

        // on récupère l'xp de chaque particule
        foreach (Collider2D particle in particles)
        {
            // on récupère l'xp de la particule
            // int xp_particle = particle.GetComponent<Particle>().xp;

            // on ajoute l'xp
            catch_xp(xp_particle);

            // on détruit la particule
            // Destroy(particle.gameObject);
        } */

    }

    public void catch_xp(int count){

        // on ajoute l'xp
        xp += count;
        
        print("we just catched XP ! " + xp);

    }


}