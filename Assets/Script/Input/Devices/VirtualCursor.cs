using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using YARG.Core.Logging;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEngine.InputSystem.UI.VirtualMouseInput;

namespace YARG
{
    public class VirtualCursor : VirtualMouseInput
    {
        public InputActionProperty Recenter;
        private float hideAfterHold;
        private bool isHolding;
        private float holdTime;
        private void LateUpdate()
        {
            Vector2 virtualMousePosition = virtualMouse.position.ReadValue();
            RectTransform graphicTransform = (RectTransform)cursorGraphic.transform;
            virtualMousePosition.x = Mathf.Clamp(virtualMousePosition.x, -graphicTransform.rect.width, Screen.width+graphicTransform.rect.width);
            virtualMousePosition.y = Mathf.Clamp(virtualMousePosition.y, -graphicTransform.rect.height, Screen.height+graphicTransform.rect.height);
            InputState.Change(virtualMouse.position, virtualMousePosition);

            graphicTransform.localRotation *= Quaternion.Euler(0f, 0f, -Time.deltaTime*60f);

            /*bool isPressed = Recenter.reference.();
            YargLogger.LogInfo(isPressed.ToString());
            if (isPressed)
            {
                if (!isHolding)
                {
                    isHolding = true;
                    holdTime = 0f;
                }

                // Count how long it's held
                holdTime += Time.deltaTime;

                if (holdTime >= hideAfterHold)
                {
                    // Held for 3 seconds
                    Debug.Log("Right stick clicked and held for 3 seconds!");
                    isHolding = false; // reset if you only want it to trigger once
                }
            }
            else
            {
                // Reset if button released
                isHolding = false;
                holdTime = 0f;
            }*/
        }
    }
}