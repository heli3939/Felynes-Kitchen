using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform player;
    private float posY;
    private float posZ;
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

        transform.position = new Vector3(
            player.position.x,
            posY,
            posZ
        );
    }
}
