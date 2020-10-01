using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tobii.Gaming;

public class GazeOverlayBehavior : MonoBehaviour
{
    public Sprite[] m_overlaySprites;
    private Image m_img;
    private float m_maxGazeThreshold = 2F; //seconds
    private KeyValuePair<string,DateTime>[] m_gazeHits;
    private const int m_gazeHitsWindowLength = 100;
    private int m_nextInsertIndex;
    private int m_lastInsertIndex;
    
    void Start()
    {
        m_img = gameObject.GetComponent<Image>();
        m_gazeHits = new KeyValuePair<string, DateTime>[m_gazeHitsWindowLength];
        m_nextInsertIndex = 0;
        m_lastInsertIndex = 0;
    }
    
    void Update()
    {
        //update GazeOverlay sprite position
        Vector2 v = TobiiAPI.GetGazePoint().Screen;
        if (!(float.IsNaN(v.x) || float.IsNaN(v.y)))
        {
            //Move gaze overlay off screen, uncomment v and delete vector to see again
            m_img.rectTransform.position = v; //new Vector2(-1000F,-1000F) ;
        }

        //purge hits older than specified max expiration
        PurgeOldHits();

        //update GazeOverlay sprite image
        m_img.sprite = SetSprite(MaxFocusPercentage());
    }



    private Sprite SetSprite(float maxFocusPercentage)
    {
        if (maxFocusPercentage < 25F || float.IsNaN(maxFocusPercentage))
            return m_overlaySprites[0];
        else if (maxFocusPercentage < 50F)
            return m_overlaySprites[1];
        else if (maxFocusPercentage < 75F)
            return m_overlaySprites[2];
        else if (maxFocusPercentage < 100F)
            return m_overlaySprites[3];
        else if (maxFocusPercentage == 100F)
            return m_overlaySprites[4];
        else
            return m_overlaySprites[0];
    }

    public void RegisterGazeHit(string gazeTargetName)
    {
        //insert gaze target name into moving window
        m_gazeHits[m_nextInsertIndex] = new KeyValuePair<string, DateTime>(gazeTargetName, DateTime.Now);
        //update next and last insert indices
        m_lastInsertIndex = m_nextInsertIndex;
        m_nextInsertIndex++;
        if (m_nextInsertIndex == m_gazeHitsWindowLength)
            m_nextInsertIndex = 0;
    }

    private void PurgeOldHits()
    {
        for(int i = 0; i < m_gazeHitsWindowLength; i++)
        {
            if (DateTime.Now.Subtract(m_gazeHits[i].Value).TotalSeconds > m_maxGazeThreshold)
                m_gazeHits[i] = new KeyValuePair<string, DateTime>();
        }
    }

    private float MaxFocusPercentage()
    {
        string lastViewedTarget = m_gazeHits[m_lastInsertIndex].Key;

        if (lastViewedTarget == null)
            return 0;
        else
        {
            int numHitsOnTarget = 0;
            for (int i = 0; i < m_gazeHitsWindowLength; i++)
                if (m_gazeHits[i].Key == lastViewedTarget)
                    numHitsOnTarget++;

            return (numHitsOnTarget/(float)m_gazeHitsWindowLength) * 100F;
        }
    }
}
