using UnityEngine;

public class PlayerLookBehaviour : MonoBehaviour
{
    public float detectDistance = 5f;

    private Outline currentTarget;

    void Update()
    {
        Ray ray = new Ray(
            Camera.main.transform.position,
            Camera.main.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, detectDistance))
        {
            Outline target =
                hit.collider.GetComponent<Outline>();

            if (target != null)
            {
                if (currentTarget != target)
                {
                    ClearCurrentTarget();

                    currentTarget = target;

                    currentTarget.EnableHighlight();
                }

                return;
            }
        }

        ClearCurrentTarget();
    }

    void ClearCurrentTarget()
    {
        if (currentTarget != null)
        {
            currentTarget.DisableHighlight();

            currentTarget = null;
        }
    }
}