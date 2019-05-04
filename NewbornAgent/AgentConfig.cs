﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "Agent config", menuName = "agentConfig")]
public class AgentConfig : ScriptableObject {
    public float bounciness;
    public float yLimit;
    public float highLimit;
    public float lowLimit;
    public float zLimit;
    public float threshold;
    public int layerNumber;
}