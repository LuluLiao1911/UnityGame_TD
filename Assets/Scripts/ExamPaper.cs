using UnityEngine;

public class ExamPaper : MonoBehaviour
{
    public float speed = 1.0f;

    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);
    }
}
