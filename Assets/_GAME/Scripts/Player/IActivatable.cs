using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActivatable
{
    float cooldown { get; set;}
    
    void Activate();

    void Deactivate();

}
