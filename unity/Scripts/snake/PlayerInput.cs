using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private SnakeMove SnakeMove;

    private int horizontal = 0, vertical = 0, dimension = 0;

    public enum Axis
    {
        Horizontal,
        Vertical,
        Dimension
    }

    void Awake()
    {
        SnakeMove = GetComponent<SnakeMove>();
    }

    void Update()
    {
        horizontal = 0;
        vertical = 0;
        dimension = 0;

        GetKeyboardInput();

        SetMovement();
    }

    void GetKeyboardInput()
    {
        horizontal = GetAxisRaw(Axis.Horizontal);
        vertical = GetAxisRaw(Axis.Vertical);
        dimension = GetAxisRaw(Axis.Dimension);

        if (horizontal != 0)
        {
            vertical = 0;
            dimension = 0;
        }
        else if(vertical != 0)
        {
            horizontal = 0;
            dimension = 0;
        }
        else if(dimension != 0)
        {
            vertical = 0;
            horizontal = 0;
        }

    }

    void SetMovement()
    {
        if (vertical != 0)
        {
            SnakeMove.SetInputDirection((vertical == 1) ?
                                                    SnakeMove.PlayerDirection.UP : SnakeMove.PlayerDirection.DOWN);
        }
        else if (horizontal != 0)
        {
            SnakeMove.SetInputDirection((horizontal == 1) ?
                                                    SnakeMove.PlayerDirection.RIGHT : SnakeMove.PlayerDirection.LEFT);
        }
        else if(dimension != 0)
        {
            SnakeMove.SetInputDirection((dimension == 1) ?
                                                    SnakeMove.PlayerDirection.POS_DIM : SnakeMove.PlayerDirection.NEG_DIM);
        }
    }

    int GetAxisRaw(Axis axis)
    {
        bool left = false, right = false, up = false, down = false, pos_dim = false, neg_dim = false;

        if (axis == Axis.Horizontal)
        {
            left = Input.GetKeyDown(KeyCode.LeftArrow);
            right = Input.GetKeyDown(KeyCode.RightArrow);
        }

        else if (axis == Axis.Vertical)
        {
            up = Input.GetKeyDown(KeyCode.UpArrow);
            down = Input.GetKeyDown(KeyCode.DownArrow);
        }

        else if(axis == Axis.Dimension)
        {
            pos_dim = Input.GetKeyDown(KeyCode.W);
            neg_dim = Input.GetKeyDown(KeyCode.S);
        }

        if (left || down || neg_dim) return -1;
        if (right || up || pos_dim) return 1;

        return 0;
    }
}
