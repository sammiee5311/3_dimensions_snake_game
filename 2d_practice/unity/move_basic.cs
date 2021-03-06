﻿using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;

public class move_basic : MonoBehaviour

{
    Thread m_thread;
    IPAddress localAdd;
    TcpListener listener;
    TcpClient client;

    public string IP = "";
    public int PORT = 12345;

    private bool is_moving;
    private Vector3 cur_pos, target_pos;
    private float time_to_move = 0.2f;

    bool running;

    Vector3 received_pos = Vector3.zero;

    void Update()
    {
        if (!is_moving)
        {
            StartCoroutine(move_player(received_pos));
        }
    }

    public void Start()
    {
        ThreadStart ts = new ThreadStart(get_info);
        m_thread = new Thread(ts);
        m_thread.Start();
    }

    private IEnumerator move_player(Vector3 direction)
    {
        is_moving = true;

        float elapsed_time = 0;

        cur_pos = transform.position;
        target_pos = cur_pos + direction;

        while (elapsed_time < time_to_move)
        {
            transform.position = Vector3.Lerp(cur_pos, target_pos, (elapsed_time / time_to_move));
            elapsed_time += Time.deltaTime;
            yield return null;
        }

        transform.position = target_pos;

        is_moving = false;
    }

    void get_info()
    {
        localAdd = IPAddress.Parse(IP);
        listener = new TcpListener(IPAddress.Any, PORT);
        listener.Start();

        client = listener.AcceptTcpClient();

        running = true;
        while (running)
        {
            send_receive_data();
        }
        listener.Stop();
    }

    void send_receive_data()
    {
        NetworkStream nw_stream = client.GetStream();
        byte[] buffer = new byte[client.ReceiveBufferSize];

        int bytes_read = nw_stream.Read(buffer, 0, client.ReceiveBufferSize);
        string data_received = Encoding.UTF8.GetString(buffer, 0, bytes_read);

        if (data_received != null)
        {
            received_pos = string_to_vector(data_received);
            print("received pos data");

            byte[] my_write_buffer = Encoding.ASCII.GetBytes("Connected successfully");
            nw_stream.Write(my_write_buffer, 0, my_write_buffer.Length);
        }
    }

    public static Vector3 string_to_vector(string s_vector)
    {
        if (s_vector.StartsWith("(") && s_vector.EndsWith(")"))
        {
            s_vector = s_vector.Substring(1, s_vector.Length - 2);
        }

        string[] s_array = s_vector.Split(',');

        Vector3 result = new Vector3(
            float.Parse(s_array[0]),
            float.Parse(s_array[1]),
            float.Parse(s_array[2]));

        return result;
    }
}
