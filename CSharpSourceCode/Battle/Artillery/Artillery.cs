using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.Artillery
{


    public class Artillery : UsableMachine
    {
        public GameEntity CameraHolder { get; private set; }
        public GameEntity Barrel { get; private set; }

        private MatrixFrame _cameraHolderInitialFrame;

        public string CameraHolderTag { get; private set; } = "CameraHolder";
        public string BarrelTag { get; private set; } = "Barrel";

        public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
        {
            return new TextObject("Use (F)");
        }

        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return "Artillery Battery";
        }

        protected override void OnInit()
        {
            base.OnInit();
            List<GameEntity> list = base.GameEntity.CollectChildrenEntitiesWithTag(this.CameraHolderTag);
            if (list.Count > 0)
            {
                CameraHolder = list[0];
                _cameraHolderInitialFrame = CameraHolder.GetFrame();
            }
            List<GameEntity> list2 = base.GameEntity.CollectChildrenEntitiesWithTag(this.BarrelTag);
            if (list2.Count > 0)
            {
                Barrel = list2[0];
            }
        }

        internal void GiveInput(float deltaYaw, float deltaPitch, float deltaTime)
        {
            var frame = base.GameEntity.GetFrame();
            frame.rotation.RotateAboutUp(deltaYaw * deltaTime * 0.2f);
            base.GameEntity.SetFrame(ref frame);
            if(Barrel != null)
            {
                var frame2 = Barrel.GetFrame();
                frame2.rotation.RotateAboutSide(deltaPitch * deltaTime * 0.2f);
                Barrel.SetFrame(ref frame2);
            }
        }
    }
}
