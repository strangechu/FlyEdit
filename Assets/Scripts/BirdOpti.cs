using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class BirdOpti : MonoBehaviour {

    [DllImport("BirdOpti")]
    public static extern int Add(int a, int b);
}
