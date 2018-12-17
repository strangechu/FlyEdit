using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices; // for DllImport
using System; // for IntPtr

public class BirdOpti : MonoBehaviour {

    [DllImport("BirdOpti")]
    public static extern int Add(int a, int b);

    [DllImport("BirdOpti")]
    public static extern bool LoadData(int boid_num, int frame_num, IntPtr data);

    [DllImport("BirdOpti")]
    public static extern IntPtr OutputData(ref int size, ref IntPtr data);

    [DllImport("BirdOpti")]
    public static extern void ReleaseAll();
}
