using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TraceReader : MonoBehaviour
{
    public GameObject bird;
    private List<string[]> traceData = new List<string[]>();
    private List<float[]> tracePosition = new List<float[]>();

    // Use this for initialization
    void Start()
    {
        tracePosition = loadTraceFromCSV("track");
        if (tracePosition != null)
        {
            SpawnBirds(tracePosition);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            SpawnBirdOnMouse();
        }
    }

    public void SpawnBirds(List<float[]> tracePositionList)
    {
        foreach (float[] data in tracePositionList)
        {
            float posX = (float)(1280 * data[1]);
            float posY = (float)(458 * (1.0 - data[2]));
            Vector3 pos = new Vector3(posX, posY, 10.0f);
            Vector3 v3 = Camera.main.ScreenToWorldPoint(pos);
            GameObject boid = Instantiate(bird);
            boid.transform.position = v3;
        }
    }

    public void SpawnBirdOnMouse()
    {
        Vector3 pos = (Input.mousePosition);
        pos.z = 10.0f;
        Ray ray = Camera.main.ScreenPointToRay(pos);
        Vector3 v3 = Camera.main.ScreenToWorldPoint(pos);
        GameObject boid = Instantiate(bird);
        boid.transform.position = v3;
        Debug.Log("SpawnBirdOnMouse : X=" + pos.x + " Y=" + pos.y);
    }

    public List<float[]> loadTraceFromCSV(string fileName)
    {
        List<float[]> loadedTracePosition = new List<float[]>();

        // load csv file as TextAsset
        var csvFile = Resources.Load(fileName) as TextAsset;
        if (csvFile == null)
        {
            Debug.Log("track.csv not found!");
            return null;
        }

        // tranform csv file into StringReader
        var reader = new StringReader(csvFile.text);

        while (reader.Peek() > -1)
        {
            var lineData = reader.ReadLine();
            string[] data = lineData.Split(',');
            traceData.Add(data);
        }

        int phase = 0;
        float count = 0;
        foreach (string[] data in traceData)
        {
            if (string.Compare(data[0], "x") == 0)
            {
                phase = 1;
                continue;
            }
            else if (string.Compare(data[0], "ID") == 0)
            {
                phase = 2;
                continue;
            }
            else if (string.Compare(data[0], "Variable") == 0)
            {
                phase = 3;
                continue;
            }
            else if (string.Compare(data[0], "") == 0)
            {
                continue;
            }
            if (phase == 1)
            {
                float[] f = new float[4];
                f[0] = count;
                f[1] = float.Parse(data[0])/1280;
                f[2] = float.Parse(data[1])/719;
                f[3] = float.Parse(data[2]);
                //Debug.Log("Frame " + f[0] + " : X=" + f[1] + " Y=" + f[2]);
                loadedTracePosition.Add(f);
                count++;
            }
        }
        return loadedTracePosition;
    }
}
