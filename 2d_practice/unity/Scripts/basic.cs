using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


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

        bool up, down, left, right;

        int current_score, highest_score;

        bool move_player;

        public bool is_gameover;
        public bool is_first_input;
        public float move_rate = 0.5f;
        float timer;

        Direction cur_direction, target_direction;

        public Text current_score_text, highest_score_text;


        public enum Direction
        {
            up,down,left,right
        }

        public UnityEvent on_start;
        public UnityEvent on_gameover;
        public UnityEvent first_input;
        public UnityEvent on_score;

        #region Init
        private void Start()
        {
            on_start.Invoke();
/*            create_map();
            place_player();
            place_camera();
            create_apple();
            target_direction = Direction.right;*/
        }

        public void restart_game()
        {
            clear();
            create_map();
            place_player();
            place_camera();
            create_apple();
            target_direction = Direction.right;
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
                        if (y % 2 != 0)
                        {
                            txt.SetPixel(x, y, color1);
                        }
                        else
                        {
                            txt.SetPixel(x, y, color2);
                        }

                    }
                    else
                    {
                        if (y % 2 != 0)
                        {
                            txt.SetPixel(x, y, color2);
                        }
                        else
                        {
                            txt.SetPixel(x, y, color1);
                        }
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
            if (is_gameover)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    on_start.Invoke();
                }
                return;
            }
                

            get_input();

            if (is_first_input)
            {
                set_player_direction();
                timer += Time.deltaTime;
                if (timer > move_rate)
                {
                    timer = 0;
                    cur_direction = target_direction;
                    move_player_node();
                }
            }
            else{
                if(up || down || left || right)
                {
                    is_first_input = true;
                    first_input.Invoke();
                }
            }
        }

        void get_input()
        {
            up = Input.GetButtonDown("Up");
            down = Input.GetButtonDown("Down");
            left = Input.GetButtonDown("Left");
            right = Input.GetButtonDown("Right");
        }

        void set_player_direction()
        {
            if (up)
            {
                set_direction(Direction.up);
            }
            else if (down)
            {
                set_direction(Direction.down);
            }
            else if (left)
            {
                set_direction(Direction.left);
            }
            else if (right)
            {
                set_direction(Direction.right);
            }
        }

        void set_direction(Direction d)
        {
            if (!is_opposite(d)) target_direction = d;
        }

        void move_player_node()
        {
            int x = 0, y = 0;
            switch (cur_direction)
            {
                case Direction.up:
                    y = 1;
                    break;
                case Direction.down:
                    y = -1;
                    break;
                case Direction.left:
                    x = -1;
                    break;
                case Direction.right:
                    x = 1;
                    break;
            }

            Node target_node = get_node(player_node.x + x, player_node.y + y);
            

            if(target_node == null)
            {
                on_gameover.Invoke();
            }
            else
            {
                if (is_tail_node(target_node)){
                    on_gameover.Invoke();
                }
                else
                {

                    bool is_score = false;

                    if (target_node == fruit_node)
                    {
                        is_score = true;
                    }

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
                        if (current_score > highest_score)
                            highest_score = current_score;

                        on_score.Invoke();

                        if (available_nodes.Count > 0)
                        {
                            random_place_apple();
                        }
                        else
                        {
                            // you won
                        }
                    }
                }
            }
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

        public void gameover()
        {
            is_gameover = true;
            is_first_input = false;
        }

        public void update_score()
        {
            current_score_text.text = current_score.ToString();
            highest_score_text.text = highest_score.ToString();
        }

        bool is_opposite(Direction d)
        {
            switch (d)
            {
                default:
                case Direction.up:
                    if (cur_direction == Direction.down)
                        return true;
                    return false;
                case Direction.down:
                    if (cur_direction == Direction.up)
                        return true;
                    return false;
                case Direction.left:
                    if (cur_direction == Direction.right)
                        return true;
                    return false;
                case Direction.right:
                    if (cur_direction == Direction.left)
                        return true;
                    return false;
            }
        }

        bool is_tail_node(Node n)
        {
            for(int i=0; i<tail.Count; i++)
            {
                if(tail[i].node == n)
                {
                    return true;
                }
            }

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
            if (x < 0 || x > max_width - 1 || y < 0 || y > max_height - 1) 
                return null;
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