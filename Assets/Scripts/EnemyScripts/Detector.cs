using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Detector : MonoBehaviour
{
    public abstract void Detect();
    public abstract bool HasDetected();
    public abstract Vector2 GetDetectedPosition();
}
