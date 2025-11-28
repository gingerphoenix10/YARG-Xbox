using UnityEngine;

namespace YARG
{
    public class CheckPlatform_NotStandalone : MonoBehaviour
    {
        private void Awake()
        {
#if UNITY_STANDALONE
            Destroy(gameObject);
#endif
        }
    }
}
