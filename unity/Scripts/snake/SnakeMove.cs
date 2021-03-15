using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeMove : MonoBehaviour
{
    [HideInInspector]
    public PlayerDirection direction;
    
    public StartMenu StartMenu;

    public static int SIZE = 2;

    public double movement_frequency = 0.8;

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
    public static int max_width = 10, max_height = 8, max_depth = 5, min_depth = -5;

    private bool create_node_at_tail;
    public int score = 0;

    void Awake()
    {
        tr = transform;
        main_body = GetComponent<Rigidbody>();

        InitSnakeNodes();
        InitPlayer();

        delta_position = new List<Vector3>()
        {
            new Vector3(-SIZE, 0, 0),  // LEFT
            new Vector3(0, SIZE, 0),  // UP
            new Vector3(SIZE, 0, 0),  // RIGHT
            new Vector3(0, -SIZE, 0), // DOWN
            new Vector3(0, 0, SIZE),  // POS DIMENSION
            new Vector3(0, 0, -SIZE)  // NEG DIMENSION
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

        if (gameover) StartMenu.Setup(score);

        if (Fruit.fruit_pos == head_body.position)
        {
            while (Fruit.fruit_pos == head_body.position)
            {
                score++;
                Fruit.fruit_pos = new Vector3(
                    Random.Range(-max_width+1, max_width - 1)* SIZE,
                    Random.Range(-max_height+1, max_height - 1)* SIZE,
                    Random.Range(min_depth+1, max_depth - 1)* SIZE);
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

        if (x < -max_width* SIZE || x > max_width* SIZE || y < -max_height* SIZE || y > max_height* SIZE || z < min_depth* SIZE || z > max_depth* SIZE) return true;

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
