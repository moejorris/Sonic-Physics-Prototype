using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    enum FollowType
    {
        Fixed,
        Lerp,
    };

    [SerializeField] FollowType followType;
    [SerializeField] Transform followTarget;
    [SerializeField] Vector3 offset;
    [SerializeField] float maxDist = 3f;
    [SerializeField] float lerpSpeed = 5f;
    [SerializeField] bool snapToPixel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        offset = transform.position - followTarget.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 targetPos = offset + followTarget.position;

        if(followType == FollowType.Fixed)
        {
            transform.position = targetPos;        
        }
        else if(followType == FollowType.Lerp)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, lerpSpeed * Time.deltaTime);

            if(Vector3.Distance(transform.position, targetPos) > maxDist)
            {
                Vector3 dir = transform.position - targetPos;
                transform.position = targetPos + dir.normalized * maxDist;
            }
        }

        if(snapToPixel)
        {
            transform.position = new Vector2(transform.position.x, transform.position.y).SnapToPixel();
            transform.position += Vector3.forward * targetPos.z;
        }

    }


}
