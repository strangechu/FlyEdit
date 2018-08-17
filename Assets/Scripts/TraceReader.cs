using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class TraceReader : MonoBehaviour
{
    public GameObject bird;
    private List<float[]> tracePosition = new List<float[]>();
    private List<float[]> tracePosition2 = new List<float[]>();
    private List<GameObject> birds = new List<GameObject>();
    private int FRAME_MAX = 67;

    // Use this for initialization
    void Start()
    {
        loadTraceFromCSV("trace1", ref tracePosition);
        if (tracePosition != null)
        {
            //SpawnBirds(tracePosition);
            SpawnBird(tracePosition);
        }

        loadTraceFromCSV("trace2", ref tracePosition2);
        if (tracePosition2 != null)
        {
            //SpawnBirds(tracePosition);
            SpawnBird(tracePosition2);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            //SpawnBirdOnMouse();
        }
        UpdateBird(0, tracePosition);
        UpdateBird(1, tracePosition2);
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

    public void SpawnBird(List<float[]> tracePositionList)
    {
        float[] data = tracePositionList[0];
        float posX = (float)(1280 * data[1]);
        float posY = (float)(458 * (1.0 - data[2]));
        Vector3 pos = new Vector3(posX, posY, 10.0f);
        Vector3 v3 = Camera.main.ScreenToWorldPoint(pos);
        GameObject boid = Instantiate(bird);
        boid.name = "Bird" + birds.Count;
        boid.transform.position = v3;
        birds.Add(boid);
        Debug.Log("Boid " + birds.Count + " spawned X=" + posX + " Y=" + posY);
    }

    public void UpdateBird(int no, List<float[]> tracePositionList)
    {
        GameObject boid = birds[no];
        GameObject slider_object = GameObject.Find("Slider");
        Slider slider = slider_object.GetComponent<Slider>();
        int frame = (int)(slider.value * FRAME_MAX);

        float[] data = tracePositionList[frame];
        float posX = (float)(1280 * data[1]);
        float posY = (float)(458 * (1.0 - data[2]));
        Vector3 pos = new Vector3(posX, posY, 10.0f);
        Vector3 v3 = Camera.main.ScreenToWorldPoint(pos);
        boid.transform.position = v3;
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

    public bool loadTraceFromCSV(string fileName, ref List<float[]> loadedTracePosition)
    {
        Debug.Log("Start loading trace data " + fileName);

        // load csv file as TextAsset
        var csvFile = Resources.Load(fileName) as TextAsset;
        if (csvFile == null)
        {
            Debug.Log("track file" + fileName + " not found!");
            return false;
        }

        // tranform csv file into StringReader
        var reader = new StringReader(csvFile.text);

        List<string[]> traceData = new List<string[]>();

        while (reader.Peek() > -1)
        {
            var lineData = reader.ReadLine();
            string[] data = lineData.Split(',');
            traceData.Add(data);
            Debug.Log(data[0]);
        }

        reader.Close();
        reader = null;

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
        return true;
    }
}
