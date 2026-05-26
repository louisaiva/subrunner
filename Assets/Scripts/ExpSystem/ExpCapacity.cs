using UnityEngine;

public class ExpCapacity : Capacity
{
    
}

[Serializable] public class CapacityUpgrade<T, U> where T : Capacity where U : struct
{
    public T capacity_type; // todo : très peu serializable ça mdr
    public string variable_name;
    public List<U> levels; // todo ça aussi
    public int current_level = 0; 
}