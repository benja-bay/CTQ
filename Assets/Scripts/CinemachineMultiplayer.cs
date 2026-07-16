using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;

[RequireComponent(typeof(CinemachineTargetGroup))]
public class CinemachineMultiplayer : MonoBehaviour
{
    private CinemachineTargetGroup targetGroup;

    void Start()
    {
        targetGroup = GetComponent<CinemachineTargetGroup>();
        
        GameObject p1 = GameObject.Find("Player1");
        GameObject p2 = GameObject.Find("Player2");
        
        var newTargets = new List<CinemachineTargetGroup.Target>();
        
        if (p1 != null) 
        {
            newTargets.Add(new CinemachineTargetGroup.Target { Object = p1.transform, Weight = 1f, Radius = 4f });
        }
        
        if (p2 != null) 
        {
            newTargets.Add(new CinemachineTargetGroup.Target { Object = p2.transform, Weight = 1f, Radius = 4f });
        }
        
        targetGroup.Targets = newTargets;
    }
}