using UnityEngine;
using UnityEngine.EventSystems;

public class CameraMoveArrow : MonoBehaviour
{
    public GameObject mainCamera;
    public Collider2D polygonCollider;

    void Update()
    {
        Vector2 worldPos = Vector2.one * 300;
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) {
            worldPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
        }
#endif
        if (worldPos.x < 200)
        {
            if (gameObject.GetComponent<PolygonCollider2D>().OverlapPoint(worldPos))
            {
                moveCamera();
            }
        }
    }

    void moveCamera()
    {
        float way = transform.rotation.eulerAngles.z / 90;
        Debug.Log($"Move {way} {transform.rotation.eulerAngles}");

        mainCamera.GetComponent<MatchHandler>().moveOffset((int)way);
    }

}