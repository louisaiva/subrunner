using UnityEngine;

/// <summary>
/// a ConnectionTree is associated to a HackCapacity
/// and handles every connection the hacker currently has
/// under a tree form in order to fix some bugs
/// when a connection is lost in the middle but we can still
/// hack things from far away
/// </summary>
public class ConnectionTree : MonoBehaviour
{

}

class DeviceNode
{
    
}









/* here's a tree structure example from stack overflow
(uses generic naming which we don't want for now i guess ?)

class TreeNode<T>
{
    List<TreeNode<T>> Children = new List<TreeNode<T>>();

    T Item {get;set;}

    public TreeNode (T item)
    {
        Item = item;
    }

    public TreeNode<T> AddChild(T item)
    {
        TreeNode<T> nodeItem = new TreeNode<T>(item);
        Children.Add(nodeItem);
        return nodeItem;
    }
}
*/