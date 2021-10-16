using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Screen;

namespace TOW_Core.Battle.Artillery
{
    public class ArtilleryView : UsableMissionObjectComponent
	{
		public Artillery Artillery { get; private set; }

		public MissionScreen MissionScreen { get; private set; }

		public Camera Camera { get; private set; }

		public GameEntity CameraHolder
		{
			get
			{
				return this.Artillery.CameraHolder;
			}
		}

		public Agent PilotAgent
		{
			get
			{
				return this.Artillery.PilotAgent;
			}
		}

		internal void Initialize(Artillery artillery, MissionScreen missionScreen)
		{
			this.Artillery = artillery;
			this.MissionScreen = missionScreen;
			this.UsesMouseForAiming = true;
		}

		protected override void OnAdded(Scene scene)
		{
			base.OnAdded(scene);
			if (this.CameraHolder != null)
			{
				this.CreateCamera();
			}
		}

		protected override void OnMissionReset()
		{
			base.OnMissionReset();
			if (this.CameraHolder != null)
			{
				this._cameraYaw = this._cameraInitialYaw;
				this._cameraPitch = this._cameraInitialPitch;
				this.ApplyCameraRotation();
				this._isInWeaponCameraMode = false;
				this.ResetCamera();
			}
		}

		public override bool IsOnTickRequired()
		{
			return true;
		}

		protected override void OnTick(float dt)
		{
			base.OnTick(dt);
			if (!GameNetwork.IsReplay)
			{
				this.HandleUserInput(dt);
			}
		}

		protected virtual void HandleUserInput(float dt)
		{
			if (this.PilotAgent != null && this.PilotAgent.IsMainAgent && this.CameraHolder != null)
			{
				if (!this._isInWeaponCameraMode)
				{
					this._isInWeaponCameraMode = true;
					this.StartUsingWeaponCamera();
				}
				this.HandleUserCameraRotation(dt);
			}
			if (this._isInWeaponCameraMode && (this.PilotAgent == null || !this.PilotAgent.IsMainAgent))
			{
				this._isInWeaponCameraMode = false;
				this.ResetCamera();
			}
			this.HandleUserAiming(dt);
		}

        private void CreateCamera()
		{
			this.Camera = Camera.CreateCamera();
			float aspectRatio = Screen.AspectRatio;
			this.Camera.SetFovVertical(1.0471976f, aspectRatio, 0.1f, 1000f);
			this.Camera.Entity = this.CameraHolder;
			this._cameraInitialPitch = 1.5f;
			this._cameraInitialYaw = 0;
			this._cameraPitch = _cameraInitialPitch;
			this._cameraYaw = _cameraInitialYaw;
		}

		private void StartUsingWeaponCamera()
		{
			if (this.CameraHolder != null && this.Camera.Entity != null)
			{
				this.MissionScreen.CustomCamera = this.Camera;
				Agent.Main.IsLookDirectionLocked = true;
			}
		}

		private void ResetCamera()
		{
			if (this.MissionScreen.CustomCamera == this.Camera)
			{
				this.MissionScreen.CustomCamera = null;
				if (Agent.Main != null)
				{
					Agent.Main.IsLookDirectionLocked = false;
				}
			}
		}

		protected virtual void HandleUserCameraRotation(float dt)
		{
			float cameraYaw = this._cameraYaw;
			float cameraPitch = this._cameraPitch;
			if (this.MissionScreen.SceneLayer.Input.IsGameKeyDown(10))
			{
				this._cameraYaw = this._cameraInitialYaw;
				this._cameraPitch = this._cameraInitialPitch;
			}
			this._cameraYaw -= this.MissionScreen.SceneLayer.Input.GetMouseMoveX() * dt * 0.2f;
			this._cameraPitch -= this.MissionScreen.SceneLayer.Input.GetMouseMoveY() * dt * 0.2f;
			this._cameraYaw = MBMath.ClampFloat(this._cameraYaw, 0, 0);
			this._cameraPitch = MBMath.ClampFloat(this._cameraPitch, 1.3f, 2);
			if (cameraPitch != this._cameraPitch || cameraYaw != this._cameraYaw)
			{
				this.ApplyCameraRotation();
			}
		}

		private void ApplyCameraRotation()
		{
			MatrixFrame frame = CameraHolder.GetFrame();
			frame.rotation = Mat3.Identity;
			frame.rotation.RotateAboutUp(this._cameraYaw);
			frame.rotation.RotateAboutSide(this._cameraPitch);
			this.CameraHolder.SetFrame(ref frame);
			//TOW_Core.Utilities.TOWCommon.Say("Input recieved. Yaw: " + _cameraYaw.ToString() + ", Pitch: " + _cameraPitch.ToString());
		}

		private void HandleUserAiming(float dt)
		{
			bool hasinput = false;
			float inputX = 0f;
			float inputY = 0f;
			if (this.PilotAgent != null && this.PilotAgent.IsMainAgent)
			{
				if (this.MissionScreen.SceneLayer.Input.IsGameKeyDown(2))
				{
					inputX = 1f;
				}
				else if (this.MissionScreen.SceneLayer.Input.IsGameKeyDown(3))
				{
					inputX = -1f;
				}
				if (this.MissionScreen.SceneLayer.Input.IsGameKeyDown(0))
				{
					inputY = 1f;
				}
				else if (this.MissionScreen.SceneLayer.Input.IsGameKeyDown(1))
				{
					inputY = -1f;
				}
				if (inputX != 0f)
				{
					hasinput = true;
				}
				if (inputY != 0f)
				{
					hasinput = true;
				}
			}
			this.Artillery.GiveInput(inputX, inputY, dt, hasinput);
		}

		private float _cameraYaw = 0;
		private float _cameraPitch = 0;
		private bool _isInWeaponCameraMode;

		protected bool UsesMouseForAiming;
        private float _cameraInitialYaw;
        private float _cameraInitialPitch;
    }
}
