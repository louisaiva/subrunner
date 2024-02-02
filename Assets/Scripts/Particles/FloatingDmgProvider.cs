using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatingDmgProvider : MonoBehaviour {
    
    // cette classe permet de générer des dégats flottants au dessus d'un objet
    // elle est utilisée par les beings pour afficher les dégats qu'ils reçoivent

    // PREFABS
    private GameObject floating_dmg_prefab;
    [SerializeField] private float dmg_size = 20f;

    // position aléatoire
    [SerializeField] private float random_position_range = 0.1f;
    [SerializeField] private float offset_y = 0.5f;

    // show damages once a while if there are too many
    private float show_dmg_delay = 0.1f;
    private float free_dmg_delay = 10f; // on supprime les dégats si on a pas fait de dégats depuis ce temps
    private Dictionary<GameObject,float> dmg_last_time_showed = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, float> dmg_compteur = new Dictionary<GameObject, float>();
    private List<GameObject> floating_dmg_gameObjects = new List<GameObject>();

    // UNITY FUNCTIONS
    void Start()
    {
        // on récupère le prefab
        floating_dmg_prefab = Resources.Load<GameObject>("prefabs/ui/floating_text");
    }

    void Update()
    {
        // on prepare la suppression des objets qui n'ont pas recu de dégats depuis un moment
        List<GameObject> obj_to_del = new List<GameObject>();

        // on parcourt les objets
        foreach (GameObject obj in floating_dmg_gameObjects)
        {
            // on regarde si on doit afficher les dégats
            if (Time.time - dmg_last_time_showed[obj] < show_dmg_delay) { continue; }

            // on vérifie si on doit supprimer l'objet
            if (dmg_compteur[obj] == 0f)
            {
                obj_to_del.Add(obj);
                continue;
            }

            // on vérfie si on fait au moins 1 dégat
            if (Mathf.Abs(dmg_compteur[obj]) < 1f)
            {
                // * il se peut que l'objet ait reçu des dégats inférieurs à 1
                // * mais pas depuis hyper longtemps -> on stocke le dégat pour "rien"
                // * donc on le supprime si on a pas fait de dégats depuis un moment
                if (dmg_last_time_showed[obj] != -1 && Time.time - dmg_last_time_showed[obj] > free_dmg_delay)
                {
                    obj_to_del.Add(obj);
                }

                continue;
            }

            // on affiche les dégats
            create_floating_dmg((int)dmg_compteur[obj], obj.transform.position);

            // on met à jour le compteur
            dmg_last_time_showed[obj] = Time.time;
            dmg_compteur[obj] = 0;
        }

        // on supprime les objets
        foreach (GameObject obj in obj_to_del)
        {
            dmg_compteur.Remove(obj);
            dmg_last_time_showed.Remove(obj);
            floating_dmg_gameObjects.Remove(obj);
        }
    }

    // MAIN FUNCTIONS

    public void AddFloatingDmg(GameObject obj, float dmg, Vector3 position)
    {
        if (floating_dmg_gameObjects.Contains(obj))
        {
            dmg_compteur[obj] += dmg;
        }
        else
        {
            dmg_last_time_showed[obj] = -1;
            dmg_compteur[obj] = dmg;
            floating_dmg_gameObjects.Add(obj);
        }
    }

    private void create_floating_dmg(int dmg, Vector3 position)
    {
        if (dmg == 0) { return; }

        // on met un peu d'aléatoire dans la position
        position.y += offset_y;
        position.x += Random.Range(-random_position_range, random_position_range);

        // on génère un floating dmg
        GameObject floating_dmg = Instantiate(floating_dmg_prefab, position, Quaternion.identity);

        // on ajuste la couleur en fonction des dégats
        string color = "yellow";
        if (dmg > 0)
        {
            color = "green";
        }
        else
        {
            color = "red";
        }

        floating_dmg.GetComponent<FloatingText>().init(dmg.ToString(), color, dmg_size);

        // on le met en enfant de l'objet
        floating_dmg.transform.SetParent(transform);


        // on applique le material
        // floating_dmg.GetComponent<FloatingText>().ajustMaterial();

        // on met à jour le texte
        // floating_dmg.GetComponent<TextMeshPro>().text = ;
    }

    // SECONDARY FUNCTIONS

    public void AddMissed(GameObject obj, Vector3 position)
    {
        // on ajoute un texte "missed"

        // on met un peu d'aléatoire dans la position
        position.y += offset_y;
        position.x += Random.Range(-random_position_range, random_position_range);

        // on génère un floating dmg
        GameObject floating_dmg = Instantiate(floating_dmg_prefab, position, Quaternion.identity);
        floating_dmg.GetComponent<FloatingText>().init("missed", "yellow",dmg_size);

        // on le met en enfant de l'objet
        floating_dmg.transform.SetParent(transform);

        // on ajuste la couleur en fonction des dégats
        /* floating_dmg.GetComponent<TextMeshPro>().color = Color.yellow;

        // on applique le material
        floating_dmg.GetComponent<FloatingText>().ajustMaterial();

        // on met à jour le texte
        floating_dmg.GetComponent<TextMeshPro>().text = "missed"; */
    }

}