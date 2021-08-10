using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.CrosshairMissionBehavior;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Abilities
{
    public class AbilityManagerMissionLogic : MissionLogic
    {
        //private bool isChangingModeState = false;
        private EquipmentIndex mainHand;
        private EquipmentIndex offHand;
        private Ability currentAbility;
        private AbilityComponent abilityComponent;
        private GameKeyContext keyContext = HotKeyManager.GetCategory("CombatHotKeyCategory");
        private CustomCrosshairMissionBehavior crosshairMissionBehavior = Mission.Current.GetMissionBehaviour<CustomCrosshairMissionBehavior>();
        System.Timers.Timer sheathTimer = new System.Timers.Timer(1000);

        public AbilityManagerMissionLogic() { }

        public override void OnBehaviourInitialize()
        {
            base.OnBehaviourInitialize();
            sheathTimer.AutoReset = false;
            sheathTimer.Elapsed += (s, e) => Agent.Main.TryToSheathWeaponInHand(Agent.HandIndex.OffHand, Agent.WeaponWieldActionType.WithAnimationUninterruptible);
        }
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (abilityComponent != null)
            {
                if (abilityComponent.IsAbilityModeOn)
                {
                    if (Input.IsKeyPressed(InputKey.Q))
                    {
                        DisableSpellMode();
                    }
                    else if (Input.IsKeyPressed(InputKey.LeftMouseButton))
                    {
                        Agent.Main.CastCurrentAbility();
                    }
                    else if (Input.IsKeyPressed(InputKey.MouseScrollUp))
                    {
                        currentAbility.Crosshair.IsVisible = false;
                        Agent.Main.SelectNextAbility();
                        currentAbility = Agent.Main.GetCurrentAbility();
                        crosshairMissionBehavior.SetCrosshair(currentAbility.Crosshair);
                    }
                    else if (Input.IsKeyPressed(InputKey.MouseScrollDown))
                    {
                        currentAbility.Crosshair.IsVisible = false;
                        Agent.Main.SelectPreviousAbility();
                        currentAbility = Agent.Main.GetCurrentAbility();
                        crosshairMissionBehavior.SetCrosshair(currentAbility.Crosshair);
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
            else if (Agent.Main != null)
            {
                abilityComponent = Agent.Main.GetComponent<AbilityComponent>();
                currentAbility = abilityComponent.CurrentAbility;
                crosshairMissionBehavior.SetAbilityComponent(abilityComponent);
            }
        }

        private void EnableSpellMode()
        {
            if (crosshairMissionBehavior != null)
                crosshairMissionBehavior.SetWeaponCrosshairVisibility(false);
            mainHand = Agent.Main.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            offHand = Agent.Main.GetWieldedItemIndex(Agent.HandIndex.OffHand);
            Agent.Main.TryToSheathWeaponInHand(Agent.HandIndex.MainHand, Agent.WeaponWieldActionType.WithAnimationUninterruptible);
            sheathTimer.Start();
            abilityComponent.EnableAbilityMode();
            ChangeKeyBindings();
            TOWCommon.Say("Ability mode is enabled");
        }
        private void DisableSpellMode()
        {
            Agent.Main.TryToWieldWeaponInSlot(mainHand, Agent.WeaponWieldActionType.WithAnimationUninterruptible, false);
            Agent.Main.TryToWieldWeaponInSlot(offHand, Agent.WeaponWieldActionType.WithAnimationUninterruptible, false);
            abilityComponent.DisableAbilityMode();
            abilityComponent.CurrentAbility.Crosshair.IsVisible = false;
            ChangeKeyBindings();
            TOWCommon.Say("Ability mode is disabled");
        }
        private void ChangeKeyBindings()
        {
            if (abilityComponent.IsAbilityModeOn)
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
