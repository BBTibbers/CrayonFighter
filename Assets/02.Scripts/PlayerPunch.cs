using UnityEngine;
public enum PunchType
{
    Left,
    Right
}

public class PlayerPunch : MonoBehaviour
{

    public PunchType PunchType;

    public void OnTriggerStay2D(Collider2D collision)
    {
        Player.Instance.SetGaurd(true);

    }
    public void OnTriggerExit2D(Collider2D collision)
    {
        Player.Instance.SetGaurd(false);

    }
}
