using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Test0719 : MonoBehaviour, IPointerDownHandler
{
    float lastTime = 0;
    public void OnPointerDown(PointerEventData eventData)
    {
        if (Time.realtimeSinceStartup - lastTime < 0.2f)
        {
            print("sj");
            lastTime = 0;
            return;
        }
        lastTime = Time.realtimeSinceStartup;
    }
}
