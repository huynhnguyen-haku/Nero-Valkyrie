using UnityEngine;

public class Enemy_DropController : MonoBehaviour
{
    [SerializeField] private GameObject missionObjectKey;

    // Assign a new key to drop
    public void GiveKey(GameObject newKey)
    {
        missionObjectKey = newKey;
    }

    // Drop items when enemy dies (in key finding mission)
    public void DropItems()
    {
        if (missionObjectKey != null)
            CreateItem(missionObjectKey);
    }

    // Instantiate the item at enemy's position
    private void CreateItem(GameObject itemPrefab)
    {
        GameObject item = Instantiate(itemPrefab, transform.position, Quaternion.identity);
    }
}
