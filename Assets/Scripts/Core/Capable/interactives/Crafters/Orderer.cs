using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orderer : Crafter
{

    ///
    //
    /// CRAFTER
    //
    ///
    public override string EmptyInventoryDesc => "ingredients will be shown here";
    public override string ItemRule => "food,ingredient";
    public override string UI_PoolName => "orderer";


}