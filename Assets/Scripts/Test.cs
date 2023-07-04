using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{

    public Slider meanSlider;
    public Slider maxSlider;
    public Slider minSlider;

    private AgentManager aManager;

    // Start is called before the first frame update
    void Start()
    {
        aManager = FindObjectOfType<AgentManager>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        LogClipFrame frame = aManager.getFrame();

        List<Tuple<LogAgentData,LogAgentData>> links = FrameTools.GetLinksList(frame);

        float fov = frame.GetParameters().GetFieldOfViewSize();

        float min = float.MaxValue;
        float max = float.MinValue;
        float mean = 0.0f;

        foreach(Tuple<LogAgentData, LogAgentData> t in links)
        {
            float dist = Vector3.Distance(t.Item1.getPosition(), t.Item2.getPosition());
            float ratio = dist / fov;

            mean += ratio;

            if (ratio > max) max = ratio;
            if (ratio < min) min = ratio;
        }

        mean /= links.Count;

        meanSlider.value = mean;
        maxSlider.value = max;
        minSlider.value = min;
    }
}
