using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeAgent : RoguelikeAgent
{
    protected override RoguelikeAgent SearchForTarget()
    {
        RoguelikeAgent t = GameObject.FindObjectOfType<KnightAgent>();
        if(t != null)
        {
            if((t.transform.position - this.transform.position).sqrMagnitude < searchRadius * searchRadius)
            {
                //near enough
                return t;
            }
            else return null; //too far
        }
        else return null; //none found
    }
}
