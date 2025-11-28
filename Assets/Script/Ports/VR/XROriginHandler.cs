using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace YARG
{
    public class CrossPlatformXROrigin : XROrigin
    {
        void Update()
        {
            if (!XRSettings.enabled)
            {
                Destroy(gameObject);
                return;
            }

            foreach (Canvas canv in GameObject.FindObjectsByType<Canvas>(FindObjectsSortMode.InstanceID))
            {
                if (!canv.GetComponent<TrackedDeviceGraphicRaycaster>())
                    canv.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>().blockingMask = 0;

                if (canv.transform.parent != null && canv.transform.parent.GetComponentInParent<Canvas>())
                    continue;

                canv.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                canv.transform.localPosition = new Vector3(0,0,4.9f);
                canv.renderMode = RenderMode.WorldSpace;
                canv.worldCamera = Camera;
            }

            InputSystemUIInputModule flatModule = EventSystem.current.GetComponent<InputSystemUIInputModule>();
            XRUIInputModule xrModule = EventSystem.current.GetComponent<XRUIInputModule>();

            if (flatModule != null)
                flatModule.enabled = false;
            if (xrModule != null)
                xrModule.enabled = true;
        }
    }
}
