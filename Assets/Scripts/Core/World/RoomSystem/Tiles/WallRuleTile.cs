using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class WallRuleTile : RuleTile
{

    public enum SiblingGroup
    {
        All,
        Wall,
        Ceiling,
        Carpet,
        Edge,
        Ground,
    }
    public SiblingGroup siblingGroup;

    public override bool RuleMatch(int neighbor, TileBase other)
    {
        if (other is RuleOverrideTile) { other = (other as RuleOverrideTile).m_InstanceTile; }

        // if not wall rule tile
        if (other is not WallRuleTile wrt)
        {
            if (neighbor == TilingRule.Neighbor.This) { return false; }
            if (neighbor == TilingRule.Neighbor.NotThis) { return true; }
            return base.RuleMatch(neighbor, other);
        }

        if (this.siblingGroup == SiblingGroup.All // if one of the 2 is All, they match
          || wrt.siblingGroup == SiblingGroup.All
          || this.siblingGroup == wrt.siblingGroup) // if the same sibling group, they match
        {
            if (neighbor == TilingRule.Neighbor.This) { return true; }
            if (neighbor == TilingRule.Neighbor.NotThis) { return false; }
        }
        
        return base.RuleMatch(neighbor, other);
    }
}