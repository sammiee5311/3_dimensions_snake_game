using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeMove : MonoBehaviour
{
    [HideInInspector]
    public PlayerDirection direction;

    [HideInInspector]
    public float step_length = 2;

    [HideInInspector]
    public int movement_frequency = 1;

    private float counter;
    private bool move, gameover;

    public enum PlayerDirection
    {
        LEFT = 0,
        UP = 1,
        RIGHT = 2,
        DOWN = 3,
        POS_DIM = 4,
        NEG_DIM = 5,
        COUNT = 6
    }

    [SerializeField]
    private GameObject tailPrefab;

    public List<Vector3> delta_position;
    public List<Rigidbody> nodes;
    private Transform tr;
    public static Rigidbody main_body, head_body;
    public static int max_width = 10, max_height = 8, max_depth = 15, min_depth = 5;

    private bool create_node_at_tail;

    void Awake()
    {
        tr = transform;
        main_body = GetComponent<Rigidbody>();

        InitSnakeNodes();
        InitPlayer();

        delta_position = new List<Vector3>()
        {
            new Vector3(-2, 0, 0),  // LEFT
            new Vector3(0, 2, 0),  // UP
            new Vector3(2, 0, 0),  // RIGHT
            new Vector3(0, -2, 0), // DOWN
            new Vector3(0, 0, 2),  // POS DIMENSION
            new Vector3(0, 0, -2)  // NEG DIMENSION
        };
    }

    void Update()
    {
        CheckMovementFrequency();
    }

    void FixedUpdate()
    {
        if (move)
        {
            move = false;
            Move();
        }
    }

    void InitSnakeNodes()
    {
        nodes = new List<Rigidbody>();
        nodes.Add(main_body);
        head_body = nodes[0];
    }

    void SetDirectionRandom()
    {
        int dirRandom = Random.Range(0, (int)PlayerDirection.COUNT);
        direction = (PlayerDirection)dirRandom;
    }

    void InitPlayer()
    {
        SetDirectionRandom();
    }

    void Move()
    {
        Vector3 dPosition = delta_position[(int)direction];

        Vector3 parentPos = head_body.position;
        Vector3 prevPosition;

        head_body.position = head_body.position + dPosition;

        gameover = IsCollision();

        if (gameover) print("game over");

        if (Fruit.fruit_pos == head_body.position)
        {
            while (Fruit.fruit_pos == head_body.position)
            {
                Fruit.fruit_pos = new Vector3(
                    Random.Range(-max_width, max_width - 2)*2,
                    Random.Range(-max_height, max_height - 2)*2,
                    Random.Range(min_depth, max_depth - 2)*2);
            }

            GameObject newNode = Instantiate(tailPrefab, nodes[nodes.Count - 1].position, Quaternion.identity);

            newNode.transform.SetParent(transform, true);
            nodes.Add(newNode.GetComponent<Rigidbody>());
        }

        for (int i = 1; i < nodes.Count; i++)
        {
            prevPosition = nodes[i].position;

            nodes[i].position = parentPos;
            parentPos = prevPosition;
        }
    }

    bool IsCollision()
    {
        int x = (int)head_body.position.x, y = (int)head_body.position.y, z = (int)head_body.position.z;

        if (x < -max_width*2 || x > max_width*2 || y < -max_height*2 || y > max_height*2 || z < min_depth*2 || z > max_depth*2) return true;

        for (int i = 1; i < nodes.Count; i++)
        {
            if ((nodes[i].position.x, nodes[i].position.y, nodes[i].position.z) == (x, y, z)) return true;
        }

        return false;
    }

    void CheckMovementFrequency()
    {
        counter += Time.deltaTime;
        if (counter >= movement_frequency*0.7f)
        {
            counter = 0f;
            move = true;
        }
    }

    public void SetInputDirection(PlayerDirection dir)
    {
        if (dir == PlayerDirection.UP && direction == PlayerDirection.DOWN ||
           dir == PlayerDirection.DOWN && direction == PlayerDirection.UP ||
           dir == PlayerDirection.RIGHT && direction == PlayerDirection.LEFT ||
           dir == PlayerDirection.LEFT && direction == PlayerDirection.RIGHT ||
           dir == PlayerDirection.POS_DIM && direction == PlayerDirection.NEG_DIM ||
           dir == PlayerDirection.NEG_DIM && direction == PlayerDirection.POS_DIM)  return;

        direction = dir;

        ForceMove();
    }

    void ForceMove()
    {
        counter = 0;
        move = false;
        Move();
    }
}
