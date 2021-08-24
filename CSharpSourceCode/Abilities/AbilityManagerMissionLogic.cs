using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.AI.Components;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Abilities
{
    public class AbilityManagerMissionLogic : MissionLogic
    {
        private bool isMainAgentChecked;
        private EquipmentIndex mainHand;
        private EquipmentIndex offHand;
        private Ability currentAbility;
        private AbilityComponent abilityComponent;
        private GameKeyContext keyContext = HotKeyManager.GetCategory("CombatHotKeyCategory");
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
            if (Agent.Main != null && Agent.Main.State == AgentState.Active && abilityComponent != null)
            {
                if (abilityComponent.IsAbilityModeOn)
                {
                    if (Input.IsKeyPressed(InputKey.Q))
                    {
                        if (currentAbility.Crosshair != null)
                            currentAbility.Crosshair.Hide();
                        DisableSpellMode();
                    }
                    else if (Input.IsKeyPressed(InputKey.LeftMouseButton))
                    {
                        Agent.Main.CastCurrentAbility();
                    }
                    else if (Input.IsKeyPressed(InputKey.MouseScrollUp))
                    {
                        if (currentAbility.Crosshair != null)
                            currentAbility.Crosshair.Hide();
                        Agent.Main.SelectNextAbility();
                        currentAbility = Agent.Main.GetCurrentAbility();
                    }
                    else if (Input.IsKeyPressed(InputKey.MouseScrollDown))
                    {
                        if (currentAbility.Crosshair != null)
                            currentAbility.Crosshair.Hide();
                        Agent.Main.SelectPreviousAbility();
                        currentAbility = Agent.Main.GetCurrentAbility();
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
            else if (!isMainAgentChecked && Agent.Main != null)
            {
                isMainAgentChecked = true;
                abilityComponent = Agent.Main.GetComponent<AbilityComponent>();
                if (abilityComponent != null)
                {
                    currentAbility = abilityComponent.CurrentAbility;
                }
            }
        }

        public override void OnAgentCreated(Agent agent)
        {
            base.OnAgentCreated(agent);
            if (agent.IsAbilityUser())
            {
                agent.AddComponent(new AbilityComponent(agent));
                if (agent.IsAIControlled)
                {
                    agent.AddComponent(new WizardAIComponent(agent));
                }
            }
        }

        private void EnableSpellMode()
        {
            abilityComponent.EnableAbilityMode();
            ChangeKeyBindings();
            mainHand = Agent.Main.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            offHand = Agent.Main.GetWieldedItemIndex(Agent.HandIndex.OffHand);
            Agent.Main.TryToSheathWeaponInHand(Agent.HandIndex.MainHand, Agent.WeaponWieldActionType.WithAnimationUninterruptible);
            sheathTimer.Start();
        }

        private void DisableSpellMode()
        {
            abilityComponent.DisableAbilityMode();
            ChangeKeyBindings();
            Agent.Main.TryToWieldWeaponInSlot(mainHand, Agent.WeaponWieldActionType.WithAnimationUninterruptible, false);
            Agent.Main.TryToWieldWeaponInSlot(offHand, Agent.WeaponWieldActionType.WithAnimationUninterruptible, false);
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
