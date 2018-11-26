using System.Collections;
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
        public List<float> distances;

        public BirdInfo(List<Vector3> pos, List<Vector3> dir, List<float> dis) { positions = pos; directions = dir; distances = dis; }
    };

    public GameObject bird;
    public GameObject sphere;
    private GameObject center_object;
    private List<List<float[]>> tracePositions = new List<List<float[]>>();
    private List<BirdInfo> birdInfos = new List<BirdInfo>();
    private List<GameObject> birds = new List<GameObject>();
    private List<Vector3> centerPositions = new List<Vector3>();
    private int FRAME_MAX = /*70*/194;
    private int TRACE_MAX = 5;
    public int SEPERATION_DIST = 2;
    public float SEPERATION_WEIGHT = 0.1f;

    private int frame = -1;

    Vector3 GetBirdPosition (int no, int frame)
    {
        return birdInfos[no].positions[frame];
    }

    Vector3 GetBirdDirection(int no, int frame)
    {
        return birdInfos[no].directions[frame];
    }

    float GetBirdDistance(int no, int frame)
    {
        return birdInfos[no].distances[frame];
    }
    // Use this for initialization
    void Start()
    {
        // load trace data
        for (int i = 0; i < TRACE_MAX; i++)
        {
            tracePositions.Insert(i, new List<float[]>());
            List<float[]> position = tracePositions[i];
            if (loadTraceFromCSV("trace0" + (i+1).ToString() + "_turn", ref position))
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
        int current_frame = (int)(slider.value * FRAME_MAX);
        if (current_frame != frame)
        {
            frame = (int)(slider.value * FRAME_MAX);
            if (frame >= FRAME_MAX - 1) frame = FRAME_MAX - 1;

            for (int i = 0; i < TRACE_MAX; i++)
            {
                UpdateBird(i, tracePositions[i]);
            }
            //center_object.transform.position = centerPositions[frame];
        }
        for (int i = 0; i < TRACE_MAX; i++)
        {
            Vector3 seperation = CalcSeperation(i, frame);
            Debug.DrawRay(GetBirdPosition(i, frame), seperation, Color.red);
            Debug.DrawRay(GetBirdPosition(i, frame), GetBirdDirection(i, frame).normalized, Color.green);
        }
    }

    public void InitBirdInfos()
    {
        // init first frame
        for (int i = 0; i < TRACE_MAX; i++)
        {
            List<Vector3> positions = new List<Vector3>();
            List<Vector3> directions = new List<Vector3>();
            List<float> distances = new List<float>();
            float[] data = tracePositions[i][0];
            float d = Random.Range(8.0f, 12.0f); // default distance
            Vector3 pos = ScreenToWorld(data[1], data[2], d);
            positions.Insert(0, pos);
            directions.Insert(0, Vector3.zero);
            distances.Insert(0, d);
            BirdInfo info = new BirdInfo(positions, directions, distances);
            birdInfos.Insert(i, info);
        }

        // iterate all frames
        for (int j = 1; j < FRAME_MAX; j++)
        {
            for (int i = 0; i < TRACE_MAX; i++)
            {
                //Debug.Log("iterating i = " + i + " j = " + j);
                float[] data = tracePositions[i][j];
                float x = data[1];
                float y = data[2];
                float pre_d = GetBirdDistance(i, j - 1);
                Vector3 seperation = CalcSeperation(i, j - 1) * SEPERATION_WEIGHT;
                Vector3 pos = ScreenToWorld(x, y, pre_d);
                Vector3 camera_pos = Camera.main.transform.position;
                Vector3 v = (pos - camera_pos).normalized;
                Vector3 projected_v = Vector3.Project(seperation, v);
                pos += projected_v;
                Vector3 heading = pos - camera_pos;
                float d = Vector3.Dot(heading, Camera.main.transform.forward);
                birdInfos[i].positions.Insert(j, pos);
                birdInfos[i].directions.Insert(j, Vector3.zero);
                birdInfos[i].distances.Insert(j, d);
            }
        }

        // set directions
        for (int i = 0; i < TRACE_MAX; i++)
        {
            for (int j = 1; j < FRAME_MAX - 1; j++)
            {
                Vector3 pre_pos = GetBirdPosition(i, j - 1);
                Vector3 pos = GetBirdPosition(i, j);
                Vector3 pre_dir = GetBirdDirection(i, j - 1);
                birdInfos[i].directions[j] = pre_dir * 0.1f + (pos - pre_pos);
            }
        }

        // fill empty directions
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
        //center_object = Instantiate(sphere);
        //center_object.transform.position = centerPositions[0];
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

        boid.transform.position = GetBirdPosition(no, frame);

        Vector3 dir = GetBirdDirection(no, frame);
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

    Vector3 CalcSeperation (int no, int frame)
    {
        Vector3 force = Vector3.zero;
        Vector3 my_pos = GetBirdPosition(no, frame);
        int count = 0;
        for (int i = 0; i < TRACE_MAX; i++)
        {
            Vector3 i_pos = GetBirdPosition(i, frame);
            if (i == no) continue;
            float distance = Vector3.Distance(my_pos, i_pos);
            if (distance > 0 && distance < SEPERATION_DIST)
            {
                force += (my_pos - i_pos).normalized / distance;
                count++;
            }
        }

        if (count > 0)
        {
            force = (force / count).normalized;
            return force;
        }
        return Vector3.zero;
    }
}
