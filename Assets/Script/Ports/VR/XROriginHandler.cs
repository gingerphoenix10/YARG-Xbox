using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.UI;
using YARG.Core.Logging;

namespace YARG
{
    public class CrossPlatformXROrigin : XROrigin
    {
        public static bool VREnabled { get
            {
                return XRSettings.enabled || true;
            } }
        public List<Shader> replacementShaders;
        public float GlobalFadeDistance = 1;
        void Update()
        {
            if (!VREnabled)
            {
                Destroy(gameObject);
                return;
            }

            foreach (Canvas canv in FindObjectsByType<Canvas>(FindObjectsSortMode.InstanceID))
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

            foreach (Renderer renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.InstanceID))
            {
                foreach (Material mat in renderer.sharedMaterials)
                {
                    if (Shader.Find(mat.shader.name + "VR"))
                        mat.shader = Shader.Find(mat.shader.name + "VR");
                }
            }

            Shader.SetGlobalFloat("_Global_Fade_Distance", GlobalFadeDistance);

            InputSystemUIInputModule flatModule = EventSystem.current.GetComponent<InputSystemUIInputModule>();
            XRUIInputModule xrModule = EventSystem.current.GetComponent<XRUIInputModule>();

            if (flatModule != null)
                flatModule.enabled = false;
            if (xrModule != null)
                xrModule.enabled = true;
        }
    }
}
