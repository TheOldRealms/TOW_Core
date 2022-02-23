using System;
using System.Collections.Generic;
using SandBox;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.Screens;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Screen;
using TaleWorlds.TwoDimension;
using TOW_Core.Battle.Damage;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.Sound
{
    public class AgentSoundComponent: AgentComponent
    {
        public AgentSoundComponent(Agent agent) : base(agent) {}
        private string _fireHitSoundId0 = "fireImpact1";
        private string _fireHitSoundId1 = "fireImpact2";
        private string _fireHitSoundId2 = "fireImpact3";
        private string _fireHitSoundId3 = "fireImpact4";
        private string _lightningHitSoundId0 = "lightningImpact1";
        private string _lightningHitSoundId1 = "lightningImpact2";
        private string _lightningHitSoundId2 = "lightningImpact3";
        private string _lightningHitSoundId3 = "lightningImpact4";
        private string _magicHitSoundId0 = "magicImpact1";
        private string _magicHitSoundId1 = "magicImpact2";
        private string _magicHitSoundId2 = "magicImpact3";
        private string _magicHitSoundId3 = "magicImpact4";
        private string _holyHitSoundId0 = "holyImpact1";
        private string _holyHitSoundId1 = "holyImpact2";
        private string _holyHitSoundId2 = "holyImpact3";
        private string _holyHitSoundId3 = "holyImpact4";

        private int _fireSoundIndex0;
        private int _fireSoundIndex1;
        private int _fireSoundIndex2;
        private int _fireSoundIndex3;
        private int _lightningSoundIndex0;
        private int _lightningSoundIndex1;
        private int _lightningSoundIndex2;
        private int _lightningSoundIndex3;
        private int _magicSoundIndex0;
        private int _magicSoundIndex1;
        private int _magicSoundIndex2;
        private int _magicSoundIndex3;
        private int _holySoundIndex0;
        private int _holySoundIndex1;
        private int _holySoundIndex2;
        private int _holySoundIndex3;

        private List<int> _fireImpactSounds = new List<int>();
        private List<int> _lightningImpactSounds = new List<int>();
        private List<int> _magicImpactSounds = new List<int>();
        private List<int> _holyImpactSounds = new List<int>();
        
        private string _magicalHitSoundIndex;
        private string _holyHitSoundIndex;
        private string _lightingSoundIndex;


        private Agent _playerAgent;
        

        private SoundEvent _fireHitSound;
        private SoundEvent _lightningHitSound;
        private Scene Scene;
            
        public override void Initialize()
        {
            base.Initialize();

            _fireSoundIndex0 = SoundEvent.GetEventIdFromString(_fireHitSoundId0);
            _fireSoundIndex1 = SoundEvent.GetEventIdFromString(_fireHitSoundId1);
            _fireSoundIndex2 = SoundEvent.GetEventIdFromString(_fireHitSoundId2);
            _fireSoundIndex3 = SoundEvent.GetEventIdFromString(_fireHitSoundId3);
            
            _fireImpactSounds.Add(_fireSoundIndex0);
            _fireImpactSounds.Add(_fireSoundIndex1);
            _fireImpactSounds.Add(_fireSoundIndex2);
            _fireImpactSounds.Add(_fireSoundIndex3);

            _lightningSoundIndex0 = SoundEvent.GetEventIdFromString(_lightningHitSoundId0);
            _lightningSoundIndex1 = SoundEvent.GetEventIdFromString(_lightningHitSoundId1);
            _lightningSoundIndex2 = SoundEvent.GetEventIdFromString(_lightningHitSoundId2);
            _lightningSoundIndex3 = SoundEvent.GetEventIdFromString(_lightningHitSoundId3);
            
            _lightningImpactSounds.Add(_lightningSoundIndex0);
            _lightningImpactSounds.Add(_lightningSoundIndex1);
            _lightningImpactSounds.Add(_lightningSoundIndex2);
            _lightningImpactSounds.Add(_lightningSoundIndex3);

            _holySoundIndex0 = SoundEvent.GetEventIdFromString(_holyHitSoundId0);
            _holySoundIndex1 = SoundEvent.GetEventIdFromString(_holyHitSoundId1);
            _holySoundIndex2 = SoundEvent.GetEventIdFromString(_holyHitSoundId2);
            _holySoundIndex3 = SoundEvent.GetEventIdFromString(_holyHitSoundId3);
            
            _holyImpactSounds.Add(_holySoundIndex0);
            _holyImpactSounds.Add(_holySoundIndex1);
            _holyImpactSounds.Add(_holySoundIndex2);
            _holyImpactSounds.Add(_holySoundIndex3);

            Scene = Mission.Current.Scene;
            
            

            
        }

        public void PlayHitSound(DamageType damageType)
        {
            var pos = this.Agent.Position;

            if (!(this.Agent.IsPlayerControlled || pos.Distance(Mission.Current.GetCameraFrame().origin) < 30))
                return;


            Mission.Current.MakeSound(
                MBRandom.RandomFloatRanged(0, 1) < 0.5
                    ? _holyImpactSounds.GetRandomElement()
                    : _fireImpactSounds.GetRandomElement(), pos, false, true, -1, -1);


            /*
            switch (damageType)
            {
                case DamageType.Fire:
                    Mission.Current.MakeSound(_fireImpactSounds.GetRandomElement(), pos, false, true, -1, -1);
                    break;
                case DamageType.Lightning: 
                    Mission.Current.MakeSound(_lightningImpactSounds.GetRandomElement(),pos,false,true,-1,-1);
                    break;
                case DamageType.Holy:
                    Mission.Current.MakeSound(_holyImpactSounds.GetRandomElement(),pos,false,true,-1,-1);
                    break;
            }*/
            
            
        }

        
        
    }
}