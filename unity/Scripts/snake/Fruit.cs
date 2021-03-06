﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fruit : MonoBehaviour
{
    public static Vector3 fruit_pos;

    void Awake()
    {
        PlaceFruit();
    }

    void Update()
    {
        transform.position = fruit_pos;
    }

    public void PlaceFruit()
    {
        fruit_pos = new Vector3(SnakeMove.SIZE, SnakeMove.SIZE, 10);
        transform.position = fruit_pos;
    }
}
