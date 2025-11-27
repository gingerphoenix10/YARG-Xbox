using UnityEngine;

namespace YARG
{
    public class CheckMarketPlatform : MonoBehaviour
    {
        private void Awake()
        {
#if UNITY_STANDALONE
            Destroy(gameObject);
#endif
        }
    }
}
