using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform player;
    private float posY;
    private float posZ;
    private float currX;
    private float maxLeft = -8.181f;
    private float maxRight = 7.85f;
    void Start()
    {
        GameObject getPlayer = GameObject.FindGameObjectWithTag("Player");
        player = getPlayer.transform;
        posY = transform.position.y;
        posZ = transform.position.z;
    }
    void Update()
    {
        if (!player) return;
        currX = player.position.x;
        if (currX > maxLeft && currX < maxRight)
        {
            transform.position = new Vector3(
                player.position.x,
                posY,
                posZ
            );
        }
        else if (currX <= maxLeft)
        {
            transform.position = new Vector3(
                maxLeft,
                posY,
                posZ
            );
        }
        else
        {
            transform.position = new Vector3(
                maxRight,
                posY,
                posZ
            );
        }
    }
}
