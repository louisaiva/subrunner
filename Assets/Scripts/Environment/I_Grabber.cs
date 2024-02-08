using UnityEngine;

public interface I_Grabber
{

    // fonctions
    bool canGrab(); // est-ce qu'on peut grab des choses ?
    void grab(GameObject target); // on grab un objet

}