using UnityEngine;

public class PlayerController : PlayerBase
{
    public override int PlayerIndex => 1;

    protected override Vector2 ReadInput()
    {
        return new Vector2(
            Input.GetAxisRaw("P1_Horizontal"),
            Input.GetAxisRaw("P1_Vertical")
        );
    }
}
