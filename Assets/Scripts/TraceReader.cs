using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;

public class TraceReader : MonoBehaviour
{
    struct BirdInfo
    {
        public List<Vector3> positions;
        public List<Vector3> directions;
        public List<float> distances;
        public List<Vector3> rays;

        public BirdInfo(List<Vector3> pos, List<Vector3> dir, List<float> dis, List<Vector3> ray) { positions = pos; directions = dir; distances = dis; rays = ray; }
    };

    public GameObject bird;
    public GameObject sphere;
    private GameObject center_object;
    private List<List<float[]>> tracePositions = new List<List<float[]>>();
    private List<BirdInfo> birdInfos = new List<BirdInfo>();
    private List<GameObject> birds = new List<GameObject>();
    private List<Vector3> centerPositions = new List<Vector3>();
    public int FRAME_MAX = /*70*/290;
    public int TRACE_MAX = 5;
    public Camera main_camera;


    public int SEPERATION_DIST = 50;
    public float SEPERATION_WEIGHT = 0.5f;

    private int frame = -1;

    public static TraceReader instance = null;

    Vector3 GetBirdPosition(int no, int frame)
    {
        return birdInfos[no].positions[frame];
    }

    void SetBirdPosition(int no, int frame, Vector3 pos)
    {
        birdInfos[no].positions[frame] = pos;
    }

    Vector3 GetBirdDirection(int no, int frame)
    {
        return birdInfos[no].directions[frame];
    }

    float GetBirdDistance(int no, int frame)
    {
        return birdInfos[no].distances[frame];
    }

    Vector3 GetBirdRay(int no, int frame)
    {
        return birdInfos[no].rays[frame];
    }

    // Use this for initialization
    void Start()
    {
        if (instance == null)
            instance = this;

        // load trace data
        for (int i = 0; i < TRACE_MAX; i++)
        {
            tracePositions.Insert(i, new List<float[]>());
            List<float[]> position = tracePositions[i];
            if (loadTraceFromCSV("sim_10_white_5566_" + (i + 1).ToString(), ref position))
            {
                /////////
                // custom trajactory
                //UnityEngine.Random.InitState(5566);
                //float offset_x = UnityEngine.Random.Range(-0.2f, 0.2f);
                //float offset_y = UnityEngine.Random.Range(0.0f, 0.4f);

                //position.Clear();
                //for (int j = 0; j < 360; j++)
                //{
                //    float[] f = new float[4];
                //    float count = 0;
                //    float cos = Mathf.Cos(j * 2 * Mathf.Deg2Rad);
                //    float sin = Mathf.Sin(j * 2 * Mathf.Deg2Rad);
                //    f[0] = count;
                //    f[1] = cos * 0.5f * 0.5f + 0.5f + offset_x;
                //    f[2] = sin * 0.5f * 0.25f + 0.5f + offset_y;
                //    f[3] = 0;
                //    //Debug.Log("Frame " + f[0] + " : X=" + f[1] + " Y=" + f[2]);
                //    position.Add(f);
                //    count++;
                //}
                //FRAME_MAX = 360;
                ////////
                SpawnBird(i);
            }
            else
            {
                break;
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

        // debug draw
        for (int i = 0; i < TRACE_MAX; i++)
        {
            Vector3 seperation = CalcSeparation(i, frame);
            Debug.DrawRay(GetBirdPosition(i, frame), seperation, Color.red);
            Debug.DrawRay(GetBirdPosition(i, frame), GetBirdDirection(i, frame).normalized, Color.green);
            Debug.DrawRay(main_camera.transform.position, GetBirdRay(i, frame), Color.yellow);
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
            List<Vector3> rays = new List<Vector3>();
            float[] data = tracePositions[i][0];
            float d = UnityEngine.Random.Range(8.0f, 12.0f); // default distance
            Vector3 pos = ScreenToWorld(data[1], data[2], 20.0f);
            Vector3 ray = ScreenToRay(data[1], data[2]);
            positions.Insert(0, pos);
            directions.Insert(0, Vector3.zero);
            distances.Insert(0, d);
            rays.Insert(0, ray);
            BirdInfo info = new BirdInfo(positions, directions, distances, rays);
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
                Vector3 ray = ScreenToRay(x, y);
                Vector3 seperation = CalcSeparation(i, j - 1);
                Vector3 pos = ScreenToWorld(x, y, pre_d);
                Vector3 camera_pos = main_camera.transform.position;
                Vector3 v = (pos - camera_pos).normalized;
                Vector3 projected_v = Vector3.Project(seperation, v);
                pos += projected_v;
                Vector3 heading = pos - camera_pos;
                float d = Vector3.Dot(heading, main_camera.transform.forward);
                birdInfos[i].positions.Insert(j, pos);
                birdInfos[i].directions.Insert(j, Vector3.zero);
                birdInfos[i].distances.Insert(j, d);
                birdInfos[i].rays.Insert(j, ray);
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
                Vector3 v = pos - pre_pos;
                Vector3 direction = Vector3.Slerp(pre_dir, v, 0.1f);
                birdInfos[i].directions[j] = direction;
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

    public Vector3 ScreenToWorld(float x, float y, float d)
    {
        //float posX = (float)(1280 * x);
        //float posY = (float)(458 * (1.0 - y));
        Vector3 pos = main_camera.ScreenToWorldPoint(new Vector3(main_camera.pixelWidth * x, main_camera.pixelHeight * y, d));
        return pos;
    }
    public Vector3 ScreenToRay(float x, float y)
    {
        Ray ray = main_camera.ScreenPointToRay(new Vector3(main_camera.pixelWidth * x, main_camera.pixelHeight * y, 0.0f));
        return ray.direction;
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
            Quaternion q_rot = Quaternion.LookRotation(dir);
            boid.transform.rotation = q_rot;

            // angle of elevation cap
            var rot = boid.transform.rotation.eulerAngles;
            if ((rot.x > 30f && rot.x < 90f) || (rot.x > 90f && rot.x < 150f))
                rot.x = 30f;
            else if ((rot.x > 210.0f && rot.x < 270f) || (rot.x > 270f && rot.x < 330f))
                rot.x = 330f;
            boid.transform.rotation = Quaternion.Euler(rot);
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
        Vector3 v3 = main_camera.ScreenToWorldPoint(pos);
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
                f[1] = float.Parse(data[0]) / 1280;
                f[2] = float.Parse(data[1]) / 720;
                f[3] = float.Parse(data[2]);
                //Debug.Log("Frame " + f[0] + " : X=" + f[1] + " Y=" + f[2]);
                loadedTracePosition.Add(f);
                count++;
            }
        }
        //FRAME_MAX = (int)count;

        // reverse try
        //List<float[]> reversed = new List<float[]>();
        //reversed.AddRange(loadedTracePosition);
        //reversed.Reverse();
        //FRAME_MAX = (int)count;
        //loadedTracePosition.AddRange(reversed);
        //FRAME_MAX = (int)count * 2;
        return true;
    }

    Vector3 CalcSeparation(int no, int frame)
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
            force = (force / count);
            force *= SEPERATION_WEIGHT;
            return force;
        }
        return Vector3.zero;
    }

    public void Optimize()
    {
        Debug.Log("Optimize start.");
        // plugin passing test
        ////////
        //float[] data = { 1.1f, 2.2f, 3.3f };
        //IntPtr data_ptr = Marshal.AllocHGlobal(data.Length * sizeof(float));
        //Marshal.Copy(data, 0, data_ptr, data.Length);
        //BirdOpti.LoadData(data.Length, data_ptr);
        //int length = 0;
        //IntPtr data_out_ptr = IntPtr.Zero;
        //BirdOpti.OutputData(ref length, ref data_out_ptr);
        //Debug.Log(length);
        //float[] data_out = new float[length];
        //Marshal.Copy(data_out_ptr, data_out, 0, length);
        //Debug.Log(data_out[0] + " " + data_out[1] + " " + data_out [2]);
        //BirdOpti.ReleaseAll();
        //Marshal.FreeHGlobal(data_ptr);
        ////////

        int agent_num = TRACE_MAX;
        int start_frame = 0;
        int frame_num = FRAME_MAX;
        float[] data = new float[3 * agent_num * frame_num];
        int index = 0;
        for (int i = 0; i < agent_num; i++)
        {
            for (int j = start_frame; j < frame_num; j++)
            {
                Vector3 ray = GetBirdRay(i, j);
                data[index++] = ray.x;
                data[index++] = ray.y;
                data[index++] = ray.z;
            }
        }
        IntPtr data_ptr = Marshal.AllocHGlobal(data.Length * sizeof(float));
        Marshal.Copy(data, 0, data_ptr, data.Length);
        BirdOpti.LoadData(agent_num, frame_num - start_frame, data_ptr);

        float[] param_data = { 1.0f, 1.0f, 1.0f, 0.0f, 0.0f};
        param_data[0] = float.Parse(UIControl.instance.param0Input.text);
        param_data[1] = float.Parse(UIControl.instance.param1Input.text);
        param_data[2] = float.Parse(UIControl.instance.param2Input.text);
        param_data[3] = SEPERATION_DIST;
        param_data[4] = SEPERATION_WEIGHT;
        IntPtr param_data_ptr = Marshal.AllocHGlobal(param_data.Length * sizeof(float));
        Marshal.Copy(param_data, 0, param_data_ptr, param_data.Length);

        int length = 0;
        IntPtr data_out_ptr = IntPtr.Zero;
        //BirdOpti.GlobalOptimize(ref length, ref data_out_ptr);
        BirdOpti.StepOptimize(ref length, ref data_out_ptr, 5f, 30f, param_data_ptr);
        Debug.Log(length);
        float[] data_out = new float[length];
        Marshal.Copy(data_out_ptr, data_out, 0, length);
        index = 0;
        for (int i = 0; i < agent_num; i++)
        {
            for (int j = start_frame; j < frame_num; j++)
            {
                Vector3 pos = Vector3.zero;
                pos.x = data_out[index++];
                pos.y = data_out[index++];
                pos.z = data_out[index++];
                SetBirdPosition(i, j, main_camera.transform.position + pos);
            }
        }
        BirdOpti.ReleaseAll();
        Marshal.FreeHGlobal(data_ptr);
        Marshal.FreeHGlobal(param_data_ptr);

        Debug.Log("Optimize end. agent_num = " + agent_num + " , frame_num = " + frame_num);

        // set directions
        for (int i = 0; i < agent_num; i++)
        {
            for (int j = 1; j < frame_num - 1; j++)
            {
                Vector3 pre_pos = GetBirdPosition(i, j - 1);
                Vector3 pos = GetBirdPosition(i, j);
                Vector3 pre_dir = GetBirdDirection(i, j - 1);
                Vector3 v = pos - pre_pos;
                Vector3 direction = Vector3.Slerp(pre_dir, v, 0.1f);
                //Vector3 direction = v;
                birdInfos[i].directions[j] = direction;
            }
        }
    }
}