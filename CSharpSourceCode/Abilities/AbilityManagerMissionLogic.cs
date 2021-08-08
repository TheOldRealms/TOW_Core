using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Abilities
{
    public class AbilityManagerMissionLogic : MissionLogic
    {
        private EquipmentIndex mainHand;
        private EquipmentIndex offHand;
        private bool isSpellModeOn = false;
        private GameKeyContext keyContext;
        System.Timers.Timer sheathTimer = new System.Timers.Timer(1000);

        public AbilityManagerMissionLogic()
        {
            foreach (var cat in HotKeyManager.GetAllCategories())
            {
                if (cat.GameKeyCategoryId == "CombatHotKeyCategory")
                {
                    keyContext = cat;
                }
            }
            sheathTimer.AutoReset = false;
            sheathTimer.Elapsed += (s, e) => Agent.Main.TryToSheathWeaponInHand(Agent.HandIndex.OffHand, Agent.WeaponWieldActionType.WithAnimationUninterruptible);
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (Mission.CurrentState == Mission.State.Continuing && Agent.Main != null && Agent.Main.IsAbilityUser())
            {
                if (isSpellModeOn)
                {
                    if (Input.IsKeyPressed(InputKey.Q))
                    {
                        DisableSpellMode();
                    }
                    if (Input.IsKeyPressed(InputKey.LeftMouseButton))
                    {
                        Agent.Main.CastCurrentAbility();
                    }
                    if (Input.IsKeyPressed(InputKey.MouseScrollUp))
                    {
                        Agent.Main.SelectNextAbility();
                    }
                    else if (Input.IsKeyPressed(InputKey.MouseScrollDown))
                    {
                        Agent.Main.SelectPreviousAbility();
                    }
                }
                else
                {
                    if (Input.IsKeyPressed(InputKey.Q))
                    {
                        EnableSpellMode();
                    }
                }
            }
        }

        private void EnableSpellMode()
        {
            isSpellModeOn = true;
            ChangeKeyBindings();
            mainHand = Agent.Main.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            offHand = Agent.Main.GetWieldedItemIndex(Agent.HandIndex.OffHand);
            Agent.Main.TryToSheathWeaponInHand(Agent.HandIndex.MainHand, Agent.WeaponWieldActionType.WithAnimationUninterruptible);
            sheathTimer.Start();
        }
        private void DisableSpellMode()
        {
            isSpellModeOn = false;
            ChangeKeyBindings();
            Agent.Main.TryToWieldWeaponInSlot(mainHand, Agent.WeaponWieldActionType.WithAnimationUninterruptible, false);
            Agent.Main.TryToWieldWeaponInSlot(offHand, Agent.WeaponWieldActionType.WithAnimationUninterruptible, false);
        }
        private void ChangeKeyBindings()
        {
            if (isSpellModeOn)
            {
                keyContext.GetGameKey(11).KeyboardKey.ChangeKey(InputKey.Invalid);
                keyContext.GetGameKey(12).KeyboardKey.ChangeKey(InputKey.Invalid);
                keyContext.GetGameKey(18).KeyboardKey.ChangeKey(InputKey.Invalid);
                keyContext.GetGameKey(19).KeyboardKey.ChangeKey(InputKey.Invalid);
                keyContext.GetGameKey(20).KeyboardKey.ChangeKey(InputKey.Invalid);
                keyContext.GetGameKey(21).KeyboardKey.ChangeKey(InputKey.Invalid);
            }
            else
            {
                keyContext.GetGameKey(11).KeyboardKey.ChangeKey(InputKey.MouseScrollUp);
                keyContext.GetGameKey(12).KeyboardKey.ChangeKey(InputKey.MouseScrollDown);
                keyContext.GetGameKey(18).KeyboardKey.ChangeKey(InputKey.Numpad1);
                keyContext.GetGameKey(19).KeyboardKey.ChangeKey(InputKey.Numpad2);
                keyContext.GetGameKey(20).KeyboardKey.ChangeKey(InputKey.Numpad3);
                keyContext.GetGameKey(21).KeyboardKey.ChangeKey(InputKey.Numpad4);
            }
        }
    }
}
