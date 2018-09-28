﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class TraceReader : MonoBehaviour
{
    struct BirdInfo
    {
        public List<Vector3> positions;
        public List<Vector3> directions;

        public BirdInfo(List<Vector3> pos, List<Vector3> dir) { positions = pos; directions = dir; }
    };

    public GameObject bird;
    public GameObject sphere;
    private GameObject center_object;
    private List<List<float[]>> tracePositions = new List<List<float[]>>();
    private List<BirdInfo> birdInfos = new List<BirdInfo>();
    private List<GameObject> birds = new List<GameObject>();
    private List<Vector3> centerPositions = new List<Vector3>();
    private int FRAME_MAX = 70;
    private int TRACE_MAX = 5;

    private int frame = 0;

    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < TRACE_MAX; i++)
        {
            tracePositions.Insert(i, new List<float[]>());
            List<float[]> position = tracePositions[i];
            if (loadTraceFromCSV("trace0" + (i+1).ToString(), ref position))
            {
                SpawnBird(i);
            }
        }
        InitBirdInfos();
        ProcessBirdInfos();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            //SpawnBirdOnMouse();
        }

        GameObject slider_object = GameObject.Find("Slider");
        Slider slider = slider_object.GetComponent<Slider>();
        frame = (int)(slider.value * FRAME_MAX);
        if (frame >= FRAME_MAX - 1) frame = FRAME_MAX - 1;

        for (int i = 0; i < TRACE_MAX; i++)
        {
            UpdateBird(i, tracePositions[i]);
        }
        center_object.transform.position = centerPositions[frame];
    }

    public void InitBirdInfos()
    {
        // project screen to world
        for (int i = 0; i < TRACE_MAX; i++)
        {
            List<Vector3> positions = new List<Vector3>();
            List<Vector3> directions = new List<Vector3>();
            for (int j = 0; j < FRAME_MAX; j++)
            {
                //Debug.Log("Init" + i + " , " + j);
                float[] data = tracePositions[i][j];
                float d = 10.0f;
                if (i == 0)
                    d -= 2.0f * j / FRAME_MAX;
                else if (i == 1)
                    d -= 1.0f * j / FRAME_MAX;
                else if (i == 4)
                    d += 1.5f * j / FRAME_MAX;
                Vector3 pos = ScreenToWorld(data[1], data[2], d);
                positions.Insert(j, pos);
                directions.Insert(j, Vector3.zero);
            }
            BirdInfo info = new BirdInfo(positions, directions);
            birdInfos.Insert(i, info);
        }

        for (int i = 0; i < TRACE_MAX; i++)
        {
            for (int j = 0; j < FRAME_MAX - 1; j++)
            {
                birdInfos[i].directions[j] = birdInfos[i].positions[j + 1] - birdInfos[i].positions[j];
            }
        }

        for (int i = 0; i < TRACE_MAX; i++)
        {
            for (int j = 0; j < FRAME_MAX - 1; j++)
            {
                if (birdInfos[i].directions[j] == Vector3.zero)
                {
                    for (int k = j + 1; k < FRAME_MAX - 1; k++)
                    {
                        if (birdInfos[i].directions[k].magnitude < Mathf.Epsilon)
                        {
                            birdInfos[i].directions[j] = birdInfos[i].directions[k];
                            break;
                        }
                    }
                }
            }
        }
    }

    public void ProcessBirdInfos()
    {
        // Find center
        for (int i = 0; i < FRAME_MAX - 1; i++)
        {
            Vector3 center = Vector3.zero;
            for (int j = 0; j < TRACE_MAX; j++)
            {
                center += birdInfos[j].positions[i];
            }
            center /= TRACE_MAX;
            centerPositions.Add(center);
        }
        center_object = Instantiate(sphere);
        center_object.transform.position = centerPositions[0];
    }

        public Vector3 ScreenToWorld (float x, float y, float d)
    {
        float posX = (float)(1280 * x);
        float posY = (float)(458 * (1.0 - y));
        Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(posX, posY, d));
        return pos;
    }

    public void SpawnBirds(List<float[]> tracePositionList)
    {
        int count = 0;
        foreach (float[] data in tracePositionList)
        {
            GameObject boid = Instantiate(bird);
            float d = 10.0f;
            boid.transform.position = ScreenToWorld(data[1], data[2], d);
            boid.name = count.ToString();
            count++;
        }
    }

    public void SpawnBird(int no)
    {
        float[] data = tracePositions[no][0];
        GameObject boid = Instantiate(bird);
        boid.name = "Bird" + birds.Count;
        float d = 10.0f;
        boid.transform.position = ScreenToWorld(data[1], data[2], d);
        birds.Add(boid);
        Debug.Log("Boid " + birds.Count + " spawned X=" + boid.transform.position.x + " Y=" + boid.transform.position.y);
    }

    public void UpdateBird(int no, List<float[]> tracePositionList)
    {
        GameObject boid = birds[no];
        GameObject text_object = GameObject.Find("FrameText");
        Text text = text_object.GetComponent<Text>();

        boid.transform.position = birdInfos[no].positions[frame];

        Vector3 dir = birdInfos[no].directions[frame];
        if (dir != Vector3.zero)
        {
            boid.transform.rotation = Quaternion.LookRotation(dir);
        }
        else
        {
            boid.transform.rotation = Quaternion.identity;
        }

        text.text = "Frame: " + frame;
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
            //Debug.Log(data[0]);
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
