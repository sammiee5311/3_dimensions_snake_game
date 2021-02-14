# 2 dimension

This is practice for communicating between python and unity.

## Image

![](../images/test.gif)


## How to use

+ #### line 68 in train.py [code](https://github.com/sammiee5311/3_dimension_snake_game/blob/2cebb3ea3ac2c5c5846d7d0a9948dc83a6c20c9e/2d_practice/python/train.py#L68)

``` python

IP, PORT = '', 12345  # type IP address and any port you want

```

+ #### line 54,55 in basic.cs [code](https://github.com/sammiee5311/3_dimension_snake_game/blob/2cebb3ea3ac2c5c5846d7d0a9948dc83a6c20c9e/2d_practice/unity/Scripts/basic.cs#L54)

``` c#

public string IP = "";  // type your IP address
public int PORT = 12345, episodes = 0;  // type any port you want

```

+ #### line 316 in basic.cs [code](https://github.com/sammiee5311/3_dimension_snake_game/blob/2cebb3ea3ac2c5c5846d7d0a9948dc83a6c20c9e/2d_practice/unity/Scripts/basic.cs#L316)

    + If you want to try to train with other hyperparameters, leave the 'send_data()' code and change learning_rate, epsilon etc. <br>
    + Otherwise, please delete the line.

    ``` c#

    send_data(); // train with send_data(), test without send_data()  

    ```