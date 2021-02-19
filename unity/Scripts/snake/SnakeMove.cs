using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeMove : MonoBehaviour
{
    [HideInInspector]
    public PlayerDirection direction;

    [HideInInspector]
    public float step_length = 2;

    public int movement_frequency = 50;

    private float counter;
    private bool move;

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

    private List<Vector3> delta_position;
    private List<Rigidbody> nodes;
    private Rigidbody main_body, head_body;
    private Transform tr;

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
        head_body = main_body;
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

        /*main_body.position = main_body.position + dPosition;*/
        head_body.position = head_body.position + dPosition;

        for (int i = 1; i < nodes.Count; i++)
        {
            prevPosition = nodes[i].position;

            nodes[i].position = parentPos;
            parentPos = prevPosition;
        }

/*        if (create_node_at_tail)
        {

        }*/
    }

    void CheckMovementFrequency()
    {
        counter += Time.deltaTime;
        print(movement_frequency);
        if (counter >= movement_frequency)
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
