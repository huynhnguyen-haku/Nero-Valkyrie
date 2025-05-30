using UnityEngine;

public class SetupLayer : MonoBehaviour
{
    [ContextMenu("Set static objects to environment layer")]
    private void AdjustLayerForStaticObjects()
    {
        foreach (Transform child in transform.GetComponentInChildren<Transform>(true))
        {
            if (child.gameObject.isStatic)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Environment");
            }
        }
    }
}
