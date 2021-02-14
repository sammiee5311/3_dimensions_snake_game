using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace SA
{
    public class basic : MonoBehaviour
    {
        public int max_height = 15, max_width = 17;

        public Color color1, color2;
        public Color fruit = Color.red;
        public Color player = Color.white;

        public Transform camera_holder;

        GameObject map_object, player_obj, fruit_object, tail_parent;
        Node player_node, fruit_node, prev_player_node;
        Sprite player_sprite;

        SpriteRenderer map_renderer, player_renderer, fruit_renderer;

        Node[,] grid;

        List<Node> available_nodes = new List<Node>();
        List<Node_tail> tail = new List<Node_tail>();

        bool up, down, left, right=true;

        int current_score, highest_score;

        bool move_player, is_score;

        public bool is_gameover;

        Direction cur_direction = Direction.RIGHT, target_direction;

        public Text current_score_text, highest_score_text;

        #region SOCKET
        Thread m_thread;
        IPAddress localAdd;
        TcpListener listener;
        TcpClient client;

        public string IP = "";
        public int PORT = 12345, episodes = 0;

        private Vector3 cur_pos, target_pos;

        bool running = true;

        Vector3 received_direction = Vector3.zero;
        Vector3 v1 = new Vector3(1, 0, 0);
        Vector3 v2 = new Vector3(0, 1, 0);
        #endregion

        public enum Direction
        {
            RIGHT = 0, 
            DOWN = 1, 
            LEFT = 2, 
            UP = 3
        }

        public UnityEvent on_start;
        public UnityEvent first_input;
        public UnityEvent on_score;

        #region Init
        private void Start()
        {
            get_info();
            on_start.Invoke();
        }

        public void restart_game()
        {
            clear();
            create_map();
            place_player();
            place_camera();
            create_apple();
            cur_direction = Direction.RIGHT;
            is_gameover = false;
            current_score = 0;
            update_score();
        }

        public void clear()
        {
            if(map_object != null)
                Destroy(map_object);
            if (player_obj != null)
                Destroy(player_obj);
            if (fruit_object != null)
                Destroy(fruit_object); 
            foreach (var t in tail)
            {
                if(t.obj != null)
                    Destroy(t.obj);
            }
            tail.Clear();
            available_nodes.Clear();
            grid = null;
        }
         
        void create_map()
        {
            map_object = new GameObject("Map");
            map_renderer = map_object.AddComponent<SpriteRenderer>();

            grid = new Node[max_width, max_height];

            Texture2D txt = new Texture2D(max_width, max_height);

            for (int x = 0; x < max_width; x++)
            {
                for (int y = 0; y < max_height; y++)
                {
                    Vector3 position = Vector3.zero;
                    position.x = x;
                    position.y = y;

                    Node n = new Node()
                    {
                        x = x,
                        y = y,
                        world_position = position
                    };

                    grid[x, y] = n;

                    available_nodes.Add(n);

                    #region Visual
                    if (x % 2 != 0)
                    {
                        if (y % 2 != 0) txt.SetPixel(x, y, color1);
                        else txt.SetPixel(x, y, color2);
                    }
                    else
                    {
                        if (y % 2 != 0) txt.SetPixel(x, y, color2);
                        else txt.SetPixel(x, y, color1);
                    }
                    #endregion
                }
            }
            txt.filterMode = FilterMode.Point;
            txt.Apply();
            Rect rect = new Rect(0, 0, max_width, max_height);
            Sprite sprite = Sprite.Create(txt, rect, Vector2.zero, 1, 0, SpriteMeshType.FullRect);
            map_renderer.sprite = sprite;
        }

        void place_player()
        {
            player_obj = new GameObject("Player");
            SpriteRenderer player_renderer = player_obj.AddComponent<SpriteRenderer>();
            player_sprite = create_sprite(player);
            player_renderer.sprite = player_sprite;
            player_renderer.sortingOrder = 1;
            player_node = get_node(3, 3);

            place_player_object(player_obj, player_node.world_position);
            player_obj.transform.localScale = Vector3.one * 1.2f;

            tail_parent = new GameObject("tail_parent");
        }

        void place_camera()
        {
            Node n = get_node(max_width / 2, max_height / 2);
            Vector3 p = n.world_position;
            p += Vector3.one * .5f;

            camera_holder.position = p;
        }

        void create_apple()
        {
            fruit_object = new GameObject("Apple");
            SpriteRenderer fruit_renderer = fruit_object.AddComponent<SpriteRenderer>();
            fruit_renderer.sprite = create_sprite(fruit);
            fruit_renderer.sortingOrder = 1;
            random_place_apple();
        }


        #endregion

        #region Update

        private void Update()
        {
            if (is_gameover || episodes > 4000)
            {
                episodes = 0;
                restart_game();
            }

            move_player_node();
            episodes++;
        }

        public void get_direction()
        {
            up = false;
            down = false;
            right = false;
            left = false;

            switch (cur_direction)
            {
                case Direction.RIGHT:
                    right = true;
                    break;
                case Direction.DOWN:
                    down = true;
                    break;
                case Direction.LEFT:
                    left = true;
                    break;
                case Direction.UP:
                    up = true;
                    break;
            }
        }

        void move_player_node()
        {
            send_data();
            receive_data();

            int x = 0, y = 0;

            int idx = (int)cur_direction;

            if (received_direction.Equals(v1))
                cur_direction = (Direction)idx;

            else if (received_direction.Equals(v2))
            {
                int next_idx = (idx + 1) % 4;
                if (next_idx == 4) next_idx = 0;
                cur_direction = (Direction)next_idx;
            }
            else
            {
                int next_idx = (idx - 1) % 4;
                if (next_idx == -1) next_idx = 3;
                cur_direction = (Direction)next_idx;
            }

            switch (cur_direction)
            {
                case Direction.UP:
                    y = 1;
                    break;
                case Direction.DOWN:
                    y = -1;
                    break;
                case Direction.LEFT:
                    x = -1;
                    break;
                case Direction.RIGHT:
                    x = 1;
                    break;
            }

            if (is_collision(player_node.x + x, player_node.y + y)) is_gameover = true;
                
            else
            {
                Node target_node = get_node(player_node.x + x, player_node.y + y);

                is_score = false;

                if (target_node == fruit_node) is_score = true;

                Node prev_node = player_node;
                available_nodes.Add(prev_node);

                if (is_score)
                {
                    tail.Add(create_tail_node(prev_node.x, prev_node.y));
                    available_nodes.Remove(prev_node);
                }

                move_tail();

                place_player_object(player_obj, target_node.world_position);
                player_node = target_node;
                available_nodes.Remove(player_node);

                if (is_score)
                {
                    current_score++;

                    if (current_score > highest_score) highest_score = current_score;

                    on_score.Invoke();

                    if (available_nodes.Count > 0) random_place_apple();
                }
            }
            send_data(); // train with send_data(), test without send_data() 
        }

        void move_tail()
        {
            Node prev_node = null;

            for(int i=0; i<tail.Count; i++)
            {
                Node_tail p = tail[i];
                available_nodes.Add(p.node);

                if(i == 0)
                {
                    prev_node = p.node;
                    p.node = player_node;
                }
                else
                {
                    Node prev = p.node;
                    p.node = prev_node;
                    prev_node = prev;
                }

                available_nodes.Remove(p.node);
                place_player_object(p.obj, p.node.world_position);
            }
        }

        #endregion

        #region Utilities

        void receive_data()
        {
            NetworkStream nw_stream = client.GetStream();
            byte[] buffer = new byte[client.ReceiveBufferSize];

            int bytes_read = nw_stream.Read(buffer, 0, client.ReceiveBufferSize);
            string data_received = Encoding.UTF8.GetString(buffer, 0, bytes_read);

            if (data_received != null) received_direction = string_to_vector(data_received);
        }
        
        void send_data()
        {
            NetworkStream nw_stream = client.GetStream();
            string state = get_state();
            byte[] my_write_buffer = Encoding.ASCII.GetBytes(state);
            nw_stream.Write(my_write_buffer, 0, my_write_buffer.Length);
        }

        public static Vector3 string_to_vector(string s_vector)
        {
            if (s_vector.StartsWith("(") && s_vector.EndsWith(")"))
                s_vector = s_vector.Substring(1, s_vector.Length - 2);

            string[] s_array = s_vector.Split(',');

            Vector3 result = new Vector3(
                int.Parse(s_array[0]),
                int.Parse(s_array[1]),
                int.Parse(s_array[2]));
            return result;
        }

        void get_info()
        {
            localAdd = IPAddress.Parse(IP);
            listener = new TcpListener(IPAddress.Any, PORT);
            listener.Start();

            client = listener.AcceptTcpClient();

            if (running == false) listener.Stop();
        }

        string get_state()
        {
            get_direction();

            string a = ((right && is_collision(player_node.x + 1, player_node.y)) || 
                       (left && is_collision(player_node.x - 1, player_node.y)) ||
                       (up && is_collision(player_node.x, player_node.y + 1)) ||
                       (down && is_collision(player_node.x, player_node.y - 1))) ? "1" : "0";
            string b = ((up && is_collision(player_node.x + 1, player_node.y)) ||
                      (down && is_collision(player_node.x - 1, player_node.y)) ||
                      (left && is_collision(player_node.x, player_node.y + 1)) ||
                      (right && is_collision(player_node.x, player_node.y - 1))) ? "1" : "0";
            string c = ((down && is_collision(player_node.x + 1, player_node.y)) ||
                      (up && is_collision(player_node.x - 1, player_node.y)) ||
                      (right && is_collision(player_node.x, player_node.y + 1)) ||
                      (left && is_collision(player_node.x, player_node.y - 1))) ? "1" : "0";
            string d = (left) ? "1" : "0";
            string e = (right) ? "1" : "0";
            string f = (up) ? "1" : "0";
            string g = (down) ? "1" : "0";
            string h = (fruit_node.x < player_node.x) ? "1" : "0";
            string i = (fruit_node.x > player_node.x) ? "1" : "0";
            string j = (fruit_node.y < player_node.y) ? "1" : "0";
            string k = (fruit_node.y > player_node.y) ? "1" : "0";
            string l = (is_gameover) ? "1" : "0";
            string m = (is_score) ? "1" : "0";

            return a + "," + b + "," + c + "," + d + "," + e + "," + f + "," + g + "," + h + "," + i + "," + j + "," + k + "," + l + "," + m;
        }

        public void gameover()
        {
            is_gameover = true;
        }

        public void update_score()
        {
            current_score_text.text = current_score.ToString();
            highest_score_text.text = highest_score.ToString();
        }

        bool is_collision(int x, int y)
        {
            if (x < 0 || x > max_width - 1 || y < 0 || y > max_height - 1) return true;

            for (int i=0; i<tail.Count; i++)
                if((tail[i].node.x, tail[i].node.y) == (x,y)) return true;

            return false;
        }

        void place_player_object(GameObject obj, Vector3 pos)
        {
            pos += Vector3.one * .5f;
            obj.transform.position = pos;
        }

        void random_place_apple()
        {
            int ran = UnityEngine.Random.Range(0, available_nodes.Count);
            Node n = available_nodes[ran];
            place_player_object(fruit_object, n.world_position);
            fruit_node = n;
        }

        Node get_node(int x, int y)
        {
            return grid[x, y];
        }

        Node_tail create_tail_node(int x, int y)
        {
            Node_tail s = new Node_tail();
            s.node = get_node(x, y);
            s.obj = new GameObject();
            s.obj.transform.parent = tail_parent.transform;
            s.obj.transform.position = s.node.world_position;
            s.obj.transform.localScale = Vector3.one * .95f;
            SpriteRenderer r = s.obj.AddComponent<SpriteRenderer>();
            r.sprite = player_sprite;
            r.sortingOrder = 1;

            return s;
        }

        Sprite create_sprite(Color target_color)
        {
            Texture2D txt = new Texture2D(1, 1);
            txt.SetPixel(0, 0, target_color);
            txt.Apply();
            txt.filterMode = FilterMode.Point;
            Rect rect = new Rect(0, 0, 1, 1);
            return Sprite.Create(txt, rect, Vector2.one * .5f, 1, 0, SpriteMeshType.FullRect);
        }
        #endregion
    }
}