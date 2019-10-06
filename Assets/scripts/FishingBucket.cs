﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FishingBucket : MonoBehaviour
{
    public int requiredFish = 5;
    public UnityEvent onComplete;

    private int score = 0;
    private Dictionary<Fish, bool> doneFish = new Dictionary<Fish, bool>();

    private void OnTriggerEnter(Collider other)
    {
        var fish = other.gameObject.GetComponent<Fish>();

        if (fish)
        {
            if (doneFish.ContainsKey(fish))
            {
                return;
            }

            doneFish.Add(fish, true);

            score++;

            if(score >= requiredFish)
            {
                onComplete?.Invoke();
            }
        }
    }
}