using UnityEngine;

public class Player3Controller : PlayerBase
{
    public override int PlayerIndex => 3;

    protected override Vector2 ReadInput()
    {
        float h = 0f, v = 0f;
        if (Input.GetKey(KeyCode.J)) h = -1f;
        if (Input.GetKey(KeyCode.L)) h =  1f;
        if (Input.GetKey(KeyCode.K)) v = -1f;
        if (Input.GetKey(KeyCode.I)) v =  1f;
        return new Vector2(h, v);
    }
}
