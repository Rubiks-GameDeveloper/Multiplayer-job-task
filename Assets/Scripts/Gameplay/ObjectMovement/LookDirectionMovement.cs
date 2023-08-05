using Unity.Netcode;
using UnityEngine;

namespace Gameplay.ObjectMovement
{
    public class LookDirectionMovement : NetworkBehaviour
    {
        public void MoveAndRotateInDirection(Vector2 direction, float moveSpeed)
        {
            if (direction != Vector2.zero) transform.rotation = Quaternion.Euler(0, 0, GetAngleFromDirection(direction));
            transform.localPosition += (Vector3)direction.normalized * (moveSpeed * Time.fixedDeltaTime);
        }
        public void MoveInDirection(Vector2 direction, float moveSpeed)
        {
            transform.localPosition += (Vector3)direction.normalized * (moveSpeed * Time.fixedDeltaTime);
        }
        private float GetAngleFromDirection(Vector2 dir)
        {
            return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }
    }
}
