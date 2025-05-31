using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;
using System;
using System.Windows.Forms;


namespace PeekingMod
{
    public class Main : Script
    {
        private int PeekingPosition;
        private bool enable;
        private Camera cam;
        private Camera SniperCam;
        private int stance = 0;
        private int lastTickTime;
        private bool CamDidHit;
        private bool GameplayCameraDidHit;
        private int didhit;
        private bool playerAlpha;
        private int opacityTick;
        private bool alpha;
        private bool backalpha;
        private int opacity;
        private float startLerpPos;
        private float startLerpRot;
        private float reversLerpPos;
        private float reversLerpRot;
        private float sLerpPos;
        private float sLerpRot;
        private float rLerpPos;
        private float rLerpRot;
        private bool useScope;
        private bool endLerpRot;

        public Main()
        {
            this.Tick += new EventHandler(this.onTick);
            this.KeyDown += new KeyEventHandler(this.onKeyDown);
            this.KeyUp += new KeyEventHandler(this.onKeyUp);
            this.cam = (Camera)null;
            this.SniperCam = (Camera)null;
            this.didhit = 0;
            this.opacity = (int)byte.MaxValue;
        }

        private float fCameraPositionX()
        {
            float num = 0.0f;
            switch (Function.Call<int>(Hash.GET_FOLLOW_PED_CAM_VIEW_MODE))
            {
                case 0:
                    return this.PeekingPosition == 1 ? 0.31f : -0.31f;
                case 1:
                    return this.PeekingPosition == 1 ? 0.45f : -0.45f;
                case 2:
                    return this.PeekingPosition == 1 ? 0.55f : -0.55f;
                case 4:
                    return this.PeekingPosition == 1 ? 0.19f : -0.19f;
                default:
                    return num;
            }
        }

        private float fCameraRotationY()
        {
            float num = 0.0f;
            switch (Function.Call<int>(Hash.GET_FOLLOW_PED_CAM_VIEW_MODE))
            {
                case 0:
                    return this.PeekingPosition == 1 ? 10f : -10f;
                case 1:
                    return this.PeekingPosition == 1 ? 10f : -10f;
                case 2:
                    return this.PeekingPosition == 1 ? 10f : -10f;
                case 4:
                    return this.PeekingPosition == 1 ? 10f : -10f;
                default:
                    return num;
            }
        }

        private void onTick(object sender, EventArgs e)
        {
            if (Configuration.ControllerEnable)
            {
                if (Game.IsControlPressed(Configuration.Controller_PeekingRight) && !Game.Player.Character.IsAnimPlay("weapons@misc@digi_scanner", "walk_additive_right"))
                {
                    Game.Player.Character.Task.PlayAnimation("weapons@misc@digi_scanner", "walk_additive_right", 8f, -8f, -1, AnimationFlags.StayInEndFrame | AnimationFlags.Secondary | AnimationFlags.Additive, 0.0f);
                    this.PeekingPosition = 1;
                    this.stance = 1;
                }
                if (Game.IsControlPressed(Configuration.Controller_PeekingLeft) && !Game.Player.Character.IsAnimPlay("weapons@misc@digi_scanner", "walk_additive_left"))
                {
                    Game.Player.Character.Task.PlayAnimation("weapons@misc@digi_scanner", "walk_additive_left", 8f, -8f, -1, AnimationFlags.StayInEndFrame | AnimationFlags.Secondary | AnimationFlags.Additive, 0.0f);
                    this.PeekingPosition = 2;
                    this.stance = 1;
                }
                if (Game.IsControlJustReleased(Configuration.Controller_PeekingRight) && Game.Player.Character.IsAnimPlay("weapons@misc@digi_scanner", "walk_additive_right") && this.PeekingPosition == 1)
                {
                    Game.Player.Character.Task.ClearSecondary();
                    this.stance = 4;
                }
                if (Game.IsControlJustReleased(Configuration.Controller_PeekingLeft) && Game.Player.Character.IsAnimPlay("weapons@misc@digi_scanner", "walk_additive_left") && this.PeekingPosition == 2)
                {
                    Game.Player.Character.Task.ClearSecondary();
                    this.stance = 4;
                }
            }
            if (this.useScope && !Game.Player.Character.IsAiming)
            {
                Function.Call(Hash.CLEAR_TIMECYCLE_MODIFIER);
                Game.Player.Character.IsVisible = true;
                this.useScope = false;
            }
            if (this.stance != 0 && this.cam != (Camera)null)
            {
                if (this.PeekingPosition == 2)
                {
                    if ((this.stance == 2 || this.stance == 3) && Game.Player.Character.IsAiming && Function.Call<int>(Hash.GET_FOLLOW_PED_CAM_VIEW_MODE) == 1 && !this.playerAlpha)
                    {
                        this.opacity = Game.Player.Character.Opacity;
                        this.opacityTick = Environment.TickCount;
                        this.alpha = true;
                        this.playerAlpha = true;
                        this.backalpha = false;
                    }
                    if (this.alpha)
                    {
                        Game.Player.Character.Opacity = (int)Utilits.LerpTime((float)this.opacity, 125f, this.opacityTick, 33f);
                        if ((int)Utilits.LerpTime((float)this.opacity, 125f, this.opacityTick, 33f) == 125)
                        {
                            this.opacityTick = Environment.TickCount;
                            this.alpha = false;
                        }
                    }
                }
                if (((this.stance == 5 && Function.Call<int>(Hash.GET_FOLLOW_PED_CAM_VIEW_MODE) == 1 || this.stance == 3 && Function.Call<int>(Hash.GET_FOLLOW_PED_CAM_VIEW_MODE) != 1) && this.PeekingPosition == 2 || this.PeekingPosition == 1 || (double)this.startLerpPos == 0.0 || !Game.Player.Character.IsAiming) && this.playerAlpha && !this.backalpha)
                {
                    this.opacity = Game.Player.Character.Opacity;
                    this.opacityTick = Environment.TickCount;
                    this.alpha = false;
                    this.backalpha = true;
                }
                if (!this.alpha && this.playerAlpha && this.backalpha)
                {
                    Game.Player.Character.Opacity = (int)Utilits.LerpTime((float)this.opacity, (float)byte.MaxValue, this.opacityTick, 33f);
                    if ((int)Utilits.LerpTime((float)this.opacity, (float)byte.MaxValue, this.opacityTick, 33f) == (int)byte.MaxValue)
                    {
                        this.opacityTick = Environment.TickCount;
                        this.alpha = false;
                        this.backalpha = false;
                        this.playerAlpha = false;
                    }
                }
                if (this.stance >= 1 && this.stance < 6 && this.PeekingPosition != 0)
                {
                    ShapeTestHandle shapeTestHandle = ShapeTest.StartTestCapsule(Game.Player.Character.Position, this.cam.GetOffsetPosition(this.stance == 2 ? new Vector3(Utilits.LerpTime(0.0f, this.fCameraPositionX(), this.lastTickTime, 33f), 0.0f, 0.0f) : (this.stance == 3 || this.stance == 5 ? new Vector3(this.fCameraPositionX(), 0.0f, 0.0f) : Vector3.Zero)), 0.2f, IntersectFlags.Everything, (Entity)Game.Player.Character);
                    this.CamDidHit = shapeTestHandle.GetResult().result.DidHit;
                    shapeTestHandle = ShapeTest.StartTestCapsule(Game.Player.Character.Position, GameplayCamera.GetOffsetPosition(new Vector3(this.fCameraPositionX(), 0.0f, 0.0f)), 0.2f, IntersectFlags.Everything, (Entity)Game.Player.Character);
                    this.GameplayCameraDidHit = shapeTestHandle.GetResult().result.DidHit;
                    if (Game.Player.Character.IsAiming && Game.Player.Character.Weapons.Current.Group != WeaponGroup.Unarmed && Game.Player.Character.Weapons.Current.Group != WeaponGroup.Melee && Game.Player.Character.Weapons.Current.Group != WeaponGroup.FireExtinguisher && Game.Player.Character.Weapons.Current.Group != WeaponGroup.PetrolCan && Game.Player.Character.Weapons.Current.Group != WeaponGroup.DigiScanner)
                        Hud.ShowComponentThisFrame(HudComponent.Reticle);
                    if (Game.Player.Character.IsAiming && Game.Player.Character.Weapons.Current.Group == WeaponGroup.Sniper && !this.useScope)
                    {
                        Function.Call(Hash.SET_TIMECYCLE_MODIFIER, (InputArgument)"Sniper");
                        this.cam.FarDepthOfField = Function.Call<float>(Hash.GET_FINAL_RENDERED_CAM_FAR_DOF);
                        this.cam.NearDepthOfField = Function.Call<float>(Hash.GET_FINAL_RENDERED_CAM_NEAR_DOF);
                        this.cam.FarClip = Function.Call<float>(Hash.GET_FINAL_RENDERED_CAM_FAR_CLIP);
                        this.cam.NearClip = Function.Call<float>(Hash.GET_FINAL_RENDERED_CAM_NEAR_CLIP);
                        this.cam.MotionBlurStrength = Function.Call<float>(Hash.GET_FINAL_RENDERED_CAM_MOTION_BLUR_STRENGTH);
                        Game.Player.Character.IsVisible = false;
                        this.useScope = true;
                    }
                }
            }
            switch (this.stance)
            {
                case 1:
                    if (this.cam == (Camera)null && (this.PeekingPosition == 1 || this.PeekingPosition == 2))
                    {
                        this.rLerpPos = (double)this.reversLerpPos == 0.0 ? 0.0f : this.reversLerpPos;
                        this.rLerpRot = (double)this.reversLerpRot == 0.0 ? 0.0f : this.reversLerpRot;
                        GameplayCamera.GetOffsetPosition(new Vector3(0.0f, 0.0f, 0.0f));
                        Vector3 vector3 = GameplayCamera.Rotation + new Vector3(0.0f, 30f, 0.0f);
                        this.cam = World.CreateCamera(GameplayCamera.GetOffsetPosition(new Vector3(0.0f, 0.0f, 0.0f)), GameplayCamera.Rotation, GameplayCamera.FieldOfView);
                        World.RenderingCamera = this.cam;
                        this.lastTickTime = Environment.TickCount;
                        if (this.didhit != 0)
                            this.didhit = 0;
                        this.stance = 2;
                        break;
                    }
                    if (!(this.cam != (Camera)null) || this.PeekingPosition != 1 && this.PeekingPosition != 2)
                        break;
                    this.rLerpPos = (double)this.reversLerpPos == 0.0 ? 0.0f : this.reversLerpPos;
                    this.rLerpRot = (double)this.reversLerpRot == 0.0 ? 0.0f : this.reversLerpRot;
                    this.didhit = !this.CamDidHit && !this.GameplayCameraDidHit || this.CamDidHit && !this.GameplayCameraDidHit || !this.CamDidHit && this.GameplayCameraDidHit ? 1 : 2;
                    this.cam.Position = GameplayCamera.GetOffsetPosition(new Vector3((double)this.reversLerpPos != 0.0 || this.didhit == 1 ? this.reversLerpPos : 0.0f, 0.0f, 0.0f));
                    this.cam.Rotation = GameplayCamera.Rotation + new Vector3(0.0f, (double)this.reversLerpRot == 0.0 ? 0.0f : this.reversLerpRot, 0.0f);
                    this.lastTickTime = Environment.TickCount;
                    if (this.didhit != 0)
                        this.didhit = 0;
                    this.stance = 2;
                    break;
                case 2:
                    this.didhit = !this.CamDidHit && !this.GameplayCameraDidHit || this.CamDidHit && !this.GameplayCameraDidHit || !this.CamDidHit && this.GameplayCameraDidHit ? 1 : 2;
                    this.startLerpPos = Utilits.LerpTime(this.rLerpPos, this.fCameraPositionX(), this.lastTickTime, 33f);
                    this.startLerpRot = Utilits.LerpTime(this.rLerpRot, this.fCameraRotationY(), this.lastTickTime, 33f);
                    bool flag1 = (double)this.startLerpPos == (double)this.fCameraPositionX();
                    bool flag2 = (double)this.startLerpRot == (double)this.fCameraRotationY();
                    Vector3 offset1 = new Vector3(this.didhit == 1 ? this.startLerpPos : 0.0f, 0.0f, 0.0f);
                    Vector3 vector3_1 = new Vector3(0.0f, this.startLerpRot, 0.0f);
                    this.cam.Position = GameplayCamera.GetOffsetPosition(offset1);
                    this.cam.Rotation = GameplayCamera.Rotation + vector3_1;
                    if (flag1 && this.didhit == 1)
                    {
                        this.lastTickTime = Environment.TickCount;
                        this.didhit = 0;
                        this.endLerpRot = false;
                        this.stance = 3;
                        break;
                    }
                    if (flag1 || this.didhit != 2)
                        break;
                    this.startLerpPos = 0.0f;
                    this.endLerpRot = true;
                    this.didhit = 0;
                    this.stance = 3;
                    break;
                case 3:
                    if (this.PeekingPosition != 0)
                    {
                        if (this.endLerpRot)
                        {
                            float y = Utilits.LerpTime(this.startLerpRot, this.fCameraRotationY(), this.lastTickTime, 33f);
                            this.cam.Rotation = GameplayCamera.Rotation + new Vector3(0.0f, y, 0.0f);
                            if ((double)y == (double)this.fCameraRotationY())
                            {
                                this.lastTickTime = Environment.TickCount;
                                this.endLerpRot = false;
                            }
                        }
                        if (!this.endLerpRot)
                        {
                            this.startLerpPos = !this.CamDidHit || !this.GameplayCameraDidHit ? this.fCameraPositionX() : 0.0f;
                            this.startLerpRot = this.fCameraRotationY();
                            this.cam.Rotation = GameplayCamera.Rotation + this.CameraRotationTranslation();
                        }
                        Vector3 offset2 = !this.CamDidHit || !this.GameplayCameraDidHit ? new Vector3(this.fCameraPositionX(), 0.0f, 0.0f) : Vector3.Zero;
                        Function.Call(Hash.FORCE_ALL_HEADING_VALUES_TO_ALIGN, (InputArgument)(Entity)Game.Player.Character);
                        Function.Call(Hash.SET_CAM_AFFECTS_AIMING, (InputArgument)this.cam, (InputArgument)true);
                        Function.Call(Hash.SET_CAM_CONTROLS_MINI_MAP_HEADING, (InputArgument)this.cam, (InputArgument)true);
                        this.cam.Position = GameplayCamera.GetOffsetPosition(offset2);
                        break;
                    }
                    if (this.PeekingPosition != 0)
                        break;
                    World.RenderingCamera = (Camera)null;
                    this.cam.Delete();
                    this.cam = (Camera)null;
                    this.stance = 0;
                    break;
                case 4:
                    this.lastTickTime = Environment.TickCount;
                    this.sLerpPos = (double)this.startLerpPos == (double)this.fCameraPositionX() ? this.fCameraPositionX() : this.startLerpPos;
                    this.sLerpRot = (double)this.startLerpRot == (double)this.fCameraRotationY() ? this.fCameraRotationY() : this.startLerpRot;
                    this.stance = 5;
                    break;
                case 5:
                    if (this.didhit == 0)
                        this.didhit = !this.CamDidHit && !this.GameplayCameraDidHit || this.CamDidHit && !this.GameplayCameraDidHit || !this.CamDidHit && this.GameplayCameraDidHit ? 1 : 2;
                    this.reversLerpPos = Utilits.LerpTime(this.sLerpPos, 0.0f, this.lastTickTime, 33f);
                    this.reversLerpRot = Utilits.LerpTime(this.sLerpRot, 0.0f, this.lastTickTime, 33f);
                    bool flag3 = (double)this.reversLerpPos == 0.0;
                    bool flag4 = (double)this.reversLerpRot == 0.0;
                    Vector3 offset3 = this.didhit == 1 ? new Vector3(this.reversLerpPos, 0.0f, 0.0f) : Vector3.Zero;
                    Vector3 vector3_2 = new Vector3(0.0f, this.reversLerpRot, 0.0f);
                    this.cam.Position = GameplayCamera.GetOffsetPosition(offset3);
                    this.cam.Rotation = GameplayCamera.Rotation + vector3_2;
                    if ((!flag3 || this.didhit != 1) && (!flag4 || this.didhit != 2))
                        break;
                    this.didhit = 0;
                    this.lastTickTime = Environment.TickCount;
                    this.stance = 6;
                    break;
                case 6:
                    World.RenderingCamera = (Camera)null;
                    this.cam.Delete();
                    this.cam = (Camera)null;
                    this.stance = 0;
                    this.startLerpPos = 0.0f;
                    this.startLerpRot = 0.0f;
                    this.PeekingPosition = 0;
                    break;
            }
        }

        private void onKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Configuration.Key_PeekingRight && Game.Player.Character.IsAnimPlay("weapons@misc@digi_scanner", "walk_additive_right") && this.PeekingPosition == 1)
            {
                Game.Player.Character.Task.ClearSecondary();
                this.stance = 4;
            }
            if (e.KeyCode != Configuration.Key_PeekingLeft || !Game.Player.Character.IsAnimPlay("weapons@misc@digi_scanner", "walk_additive_left") || this.PeekingPosition != 2)
                return;
            Game.Player.Character.Task.ClearSecondary();
            this.stance = 4;
        }

        private void onKeyDown(object sender, KeyEventArgs e)
        {
            if (Configuration.ControllerEnable)
                return;
            if (e.KeyCode == Configuration.Key_PeekingRight && !Game.Player.Character.IsAnimPlay("weapons@misc@digi_scanner", "walk_additive_right"))
            {
                Game.Player.Character.Task.PlayAnimation("weapons@misc@digi_scanner", "walk_additive_right", 8f, -8f, -1, AnimationFlags.StayInEndFrame | AnimationFlags.Secondary | AnimationFlags.Additive, 0.0f);
                this.PeekingPosition = 1;
                this.stance = 1;
            }
            if (e.KeyCode == Configuration.Key_PeekingLeft && !Game.Player.Character.IsAnimPlay("weapons@misc@digi_scanner", "walk_additive_left"))
            {
                Game.Player.Character.Task.PlayAnimation("weapons@misc@digi_scanner", "walk_additive_left", 8f, -8f, -1, AnimationFlags.StayInEndFrame | AnimationFlags.Secondary | AnimationFlags.Additive, 0.0f);
                this.PeekingPosition = 2;
                this.stance = 1;
            }
        }

        private Vector3 CameraPositionTranslation()
        {
            Vector3 zero = Vector3.Zero;
            switch (Function.Call<int>(Hash.GET_FOLLOW_PED_CAM_VIEW_MODE))
            {
                case 0:
                    return this.PeekingPosition == 1 ? new Vector3(0.31f, -0.16f, 0.0f) : new Vector3(-0.31f, -0.16f, 0.0f);
                case 1:
                    return this.PeekingPosition == 1 ? new Vector3(0.45f, 0.0f, 0.0f) : new Vector3(-0.45f, 0.0f, 0.0f);
                case 2:
                    return this.PeekingPosition == 1 ? new Vector3(0.55f, 0.0f, 0.0f) : new Vector3(-0.55f, 0.0f, 0.0f);
                case 4:
                    return this.PeekingPosition == 1 ? new Vector3(0.19f, 0.0f, 0.0f) : new Vector3(-0.19f, 0.0f, 0.0f);
                default:
                    return zero;
            }
        }

        private Vector3 CameraRotationTranslation()
        {
            Vector3 zero = Vector3.Zero;
            switch (Function.Call<int>(Hash.GET_FOLLOW_PED_CAM_VIEW_MODE))
            {
                case 0:
                    return this.PeekingPosition == 1 ? new Vector3(0.0f, 10f, 0.0f) : new Vector3(0.0f, -10f, 0.0f);
                case 1:
                    return this.PeekingPosition == 1 ? new Vector3(0.0f, 10f, 0.0f) : new Vector3(0.0f, -10f, 0.0f);
                case 2:
                    return this.PeekingPosition == 1 ? new Vector3(0.0f, 10f, 0.0f) : new Vector3(0.0f, -10f, 0.0f);
                case 4:
                    return this.PeekingPosition == 1 ? new Vector3(0.0f, 10f, 0.0f) : new Vector3(0.0f, -10f, 0.0f);
                default:
                    return zero;
            }
        }
    }
    public static class Utilits
    {
        public static float LerpTime(float min, float max, int environmenttick, float smooth)
        {
            return Utilits.Lerp(min, max, (float)(Environment.TickCount - environmenttick) / 10000f * smooth);
        }

        public static float Lerp(float value1, float value2, float ammount)
        {
            return value1 + (value2 - value1) * Utilits.Clamp01(ammount);
        }

        public static float Clamp01(float value)
        {
            if ((double)value < 0.0)
                return 0.0f;
            return (double)value > 1.0 ? 1f : value;
        }

        public static float SmoothStep(float from, float to, float t)
        {
            t = Utilits.Clamp01(t);
            t = (float)(-2.0 * (double)t * (double)t * (double)t + 3.0 * (double)t * (double)t);
            return (float)((double)to * (double)t + (double)from * (1.0 - (double)t));
        }

        public static float GetAnimTime(this Ped ped, string animDict, string animName)
        {
            return Function.Call<float>(Hash.GET_ENTITY_ANIM_CURRENT_TIME, (InputArgument)(Entity)ped, (InputArgument)animDict, (InputArgument)animName);
        }

        public static bool IsAnimPlay(this Ped ped, string animDict, string animName)
        {
            return Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, (InputArgument)(Entity)ped, (InputArgument)animDict, (InputArgument)animName, (InputArgument)3);
        }

        public static bool IsTaskActive(this Ped ped, int taskIndex)
        {
            return Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, (InputArgument)(Entity)ped, (InputArgument)taskIndex);
        }
    }
    public class Configuration : Script
    {
        private ScriptSettings IniSettings;
        public static bool ControllerEnable;
        public static GTA.Control Controller_PeekingLeft;
        public static GTA.Control Controller_PeekingRight;
        public static Keys Key_PeekingLeft;
        public static Keys Key_PeekingRight;

        public Configuration()
        {
            this.IniSettings = ScriptSettings.Load("scripts\\PeekingMod.ini");
            Configuration.ControllerEnable = this.IniSettings.GetValue<bool>("Options", nameof(ControllerEnable), false);
            Configuration.Controller_PeekingLeft = this.IniSettings.GetValue<GTA.Control>("Controller_Options", "LeaningLeft", GTA.Control.VehicleHeadlight);
            Configuration.Controller_PeekingRight = this.IniSettings.GetValue<GTA.Control>("Controller_Options", "LeaningRight", GTA.Control.VehicleRadioWheel);
            Configuration.Key_PeekingLeft = this.IniSettings.GetValue<Keys>("Controls", "LeaningLeft", Keys.Q);
            Configuration.Key_PeekingRight = this.IniSettings.GetValue<Keys>("Controls", "LeaningRight", Keys.E);
        }
    }
}
