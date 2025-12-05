using UnityEngine;

namespace YARG
{
    public class Drumstick : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            VirtualDrums.Instance.Hit(other);
        }
    }
}
