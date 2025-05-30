using UnityEngine;

public class MissionObject_CarDeliveryZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Car_Controller>() != null)
        {
            Car_Controller car = other.GetComponent<Car_Controller>();

            // When the car enters the delivery zone, invoke the car delivery method
            if (car != null)
                car.GetComponent<MissionObject_Car>().InvokeCarDelivery();
        }
    }
}
