using System;
using System.Collections.Generic;
using System.Reflection;

using DuckGame;
using XNA = Microsoft.Xna.Framework;
using static BetterCombat.BetterCombat;

namespace BetterCombat
{
    public class ModUpdate : XNA.IUpdateable
    {
        public bool Enabled { get { return true; } }
        public int UpdateOrder { get { return 1; } }

        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;


        private Duck _player = null;
        private bool _isGamepad = false;
        private bool _prevGamepadInput = false;
        private MethodInfo _methodTryGrab = null;

        private Vec2 _prevMousePosition = Mouse.position;
        private sbyte _offDir = 1;
        private float _tan = 0f;

        void XNA.IUpdateable.Update(XNA.GameTime gameTime)
        {
            Program.main.IsMouseVisible = Network.isActive;
            if (Network.isActive && Steam.user != null)
            {
                if (_player == null
                    || _player.level != Level.current
                    || _player.profile?.steamID != Steam.user.id)
                {
                    foreach (var thing in Level.current.things[typeof(Duck)])
                    {
                        if (thing is Duck duck && duck?.profile?.steamID == Steam.user.id)
                        {
                            _player = duck;
                            break;
                        }
                    }
                }

                if (_player != null
                    && _player.level == Level.current
                    && _player.profile != null
                    && _player.inputProfile != null)
                {
                    if (Mouse.position != _prevMousePosition)
                    {
                        _isGamepad = false;
                    }
                    else if (_player.inputProfile.rightStick != Vec2.Zero)
                    {
                        _isGamepad = true;
                    }

                    if (_player.gun != null)
                    {
                        var input = Vec2.Zero;
                        var isDirty = false;
                        if (_isGamepad)
                        {
                            if (_player.inputProfile.rightStick.lengthSq != 0)
                            {
                                isDirty = true;
                                _prevGamepadInput = true;
                                var stickInput = _player.inputProfile.rightStick.normalized;
                                input = new Vec2(stickInput.x, -stickInput.y);

                            }
                            else if (_prevGamepadInput)
                            {
                                _prevGamepadInput = false;
                                _player.gun.handAngle = _player.holdAngle == _player.gun.handAngle ? 0f : (_player.gun.handAngle - _player.holdAngle) * _player.offDir;
                            }
                        }
                        else
                        {
                            isDirty = true;
                            input = Mouse.positionScreen - _player.gun.barrelPosition;
                        }
                        if (isDirty)
                        {
                            var tan = (float)Math.Atan(input.y / input.x);
                            if (!float.IsNaN(tan))
                            {
                                _player.strafing = true;
                                _tan = tan;
                                _offDir = _player.gun.offDir = _player.offDir = (sbyte)(input.x < 0 ? -1 : 1);
                                _player.gun.handAngle = _player.holdAngle == _player.gun.handAngle ? tan : tan + (_player.gun.handAngle - _player.holdAngle) * _player.offDir;
                            }
                        }

                        _prevMousePosition = Mouse.position;

                        if ((!SwapMouseButtons && Mouse.left == InputState.Pressed)
                            || (SwapMouseButtons && Mouse.right == InputState.Pressed)
                            || _player.inputProfile.Pressed(Triggers.RightTrigger))
                        {
                            _player.gun.OnPressAction();
                        }

                        if ((!SwapMouseButtons && Mouse.left == InputState.Down)
                            || (SwapMouseButtons && Mouse.right == InputState.Down)
                            || _player.inputProfile.Down(Triggers.RightTrigger))
                        {
                            _player.gun.OnHoldAction();
                        }

                        if ((!SwapMouseButtons && Mouse.left == InputState.Pressed)
                            || (SwapMouseButtons && Mouse.right == InputState.Pressed)
                            || _player.inputProfile.Released(Triggers.RightTrigger))
                        {
                            _player.gun.OnReleaseAction();
                        }

                    }
                    if ((!SwapMouseButtons && Mouse.right == InputState.Pressed)
                            || (SwapMouseButtons && Mouse.left == InputState.Pressed))
                    {
                        if (_player.holdObject != null)
                        {
                            _player.doThrow = true;
                        }
                        else
                        {
                            if (_methodTryGrab == null)
                            {
                                var reflectionFlags = BindingFlags.NonPublic | BindingFlags.Instance;
                                _methodTryGrab = typeof(Duck).GetMethod("TryGrab", reflectionFlags);
                            }
                            _methodTryGrab.Invoke(_player, null);
                        }

                    }
                }
            }
        }
    }
}
