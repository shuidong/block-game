using UnityEngine;
using System.Collections;

public class ChunkIntro : MonoBehaviour
{
    Vector3 targetPos;
    bool done;

    void OnEnable()
    {
        targetPos = transform.position;
        transform.position += Vector3.down * 100;
        done = false;
    }

    void FixedUpdate()
    {
        if (!done)
        {
            Vector3 pos = transform.position;
            if ((targetPos - pos).magnitude <= .1f)
            {
                transform.position = targetPos;
                done = true;
            }
            else
            {
                transform.position = Vector3.Lerp(pos, targetPos, .1f);
            }
        }
    }
}
