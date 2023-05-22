using UnityEngine;

public class EnemyVisionController : MonoBehaviour
{
    const float GizmoViewLineLenght = 20f;

    [Range(0, 180)]
    public float FieldOfView = 40f;

    public GameObject Player { get; private set; }

    private void OnDrawGizmos()
    {
        var rightEdgeViewDirection = Quaternion.Euler(0f, FieldOfView, 0f) * transform.forward;
        var leftEdgeViewDirection = Quaternion.Euler(0f, -FieldOfView, 0f) * transform.forward;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * GizmoViewLineLenght);
        Gizmos.DrawLine(transform.position, transform.position + rightEdgeViewDirection * GizmoViewLineLenght);
        Gizmos.DrawLine(transform.position, transform.position + leftEdgeViewDirection * GizmoViewLineLenght);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(PlayerController.Tag))
            Player = other.gameObject;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(PlayerController.Tag))  
            Player = null;
    }

    public bool CanSeePlayer()
    {
        if (Player == null)
            return false;

        var sameHeightPlayerPosition = Player.transform.position;
        sameHeightPlayerPosition.y = 0;
        var sameHeightEnemyPosition = transform.position;
        sameHeightEnemyPosition.y = 0;

        var fromEnemyToPlayerDirection = (sameHeightPlayerPosition - sameHeightEnemyPosition).normalized;
        var playerDot = Vector3.Dot(transform.forward, fromEnemyToPlayerDirection);
        var fieldOfViewDot = AngleToDotValue(FieldOfView);
        if (fieldOfViewDot > playerDot)
            return false;

        var directionToPlayer = Player.transform.position - transform.position;
        if (Physics.Raycast(transform.position, directionToPlayer, out var hitInfo))
            return hitInfo.collider.CompareTag(PlayerController.Tag);

        return false;
    }

    private float AngleToDotValue(float angle)
    {
        const float minAngle = 0f;
        const float maxAngle = 180f;
        const float minDot = 1f;
        const float maxDot = -1f;
        var result = minDot + (angle - minAngle) * (maxDot - minDot) / (maxAngle - minAngle);
        return result;
    }
}
