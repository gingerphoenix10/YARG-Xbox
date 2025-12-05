using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XInput;

namespace YARG
{
    [InputControlLayout(displayName = "Virtual Drums")]
    public class VirtualDrumsDevice : Gamepad
    {
    }

    public class VirtualDrums : MonoSingleton<VirtualDrums>
    {
        Gamepad virtualDrum;
        private void OnEnable()
        {
            InputSystem.RemoveLayout("DPad");
            InputSystem.RegisterLayout<UnityEngine.InputSystem.Controls.DpadControl>("DPad");

            virtualDrum = (Gamepad)InputSystem.AddDevice<VirtualDrumsDevice>();
        }

        private void OnDisable()
        {
            if (virtualDrum != null && virtualDrum.added)
                InputSystem.RemoveDevice(virtualDrum);
        }

        private void Update()
        {
            //Hit(null);
        }

        public void Hit(Collider drum)
        {
            virtualDrum.CopyState<GamepadState>(out var drumState);
            drumState.WithButton(GamepadButton.South, true);
            InputState.Change(virtualDrum, drumState);
            drumState.WithButton(GamepadButton.South, false);
            InputState.Change(virtualDrum, drumState);
        }
    }
}
