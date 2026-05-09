using UnityEngine;

public class Player2Controller : PlayerBase
{
    protected override Vector2 ReadInput()
    {
        float h = 0f, v = 0f;
        if (Input.GetKey(KeyCode.LeftArrow))  h = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) h =  1f;
        if (Input.GetKey(KeyCode.DownArrow))  v = -1f;
        if (Input.GetKey(KeyCode.UpArrow))    v =  1f;
        return new Vector2(h, v);
    }
}
