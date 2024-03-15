// CONTRIB: made by malediktus, not checked
using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.Dungeon.D13LapisManalis.D131Albion
{
    public enum OID : uint
    {
        Boss = 0x3CFE, //R=4.6
        WildBeasts = 0x3D03, //R=0.5
        Helper = 0x233C,
        WildBeasts1 = 0x3CFF, // R1,320
        WildBeasts2 = 0x3D00, // R1,700
        WildBeasts3 = 0x3D02, // R4,000
        WildBeasts4 = 0x3D01, // R2,850
        IcyCrystal = 0x3D04, // R2,000
    }

    public enum AID : uint
    {
        AutoAttack = 872, // Boss->player, no cast, single-target
        Teleport = 32812, // Boss->location, no cast, single-target, boss teleports mid
        CallOfTheMountain = 31356, // Boss->self, 3,0s cast, single-target, boss calls wild beasts
        WildlifeCrossing = 31357, // WildBeasts->self, no cast, range 7 width 10 rect
        AlbionsEmbrace = 31365, // Boss->player, 5,0s cast, single-target
        RightSlam = 32813, // Boss->self, 5,0s cast, range 80 width 20 rect
        LeftSlam = 32814, // Boss->self, 5,0s cast, range 80 width 20 rect
        KnockOnIce = 31358, // Boss->self, 4,0s cast, single-target
        KnockOnIce2 = 31359, // Helper->self, 6,0s cast, range 5 circle
        Icebreaker = 31361, // Boss->3D04, 5,0s cast, range 17 circle
        IcyThroes = 31362, // Boss->self, no cast, single-target
        IcyThroes2 = 32783, // Helper->self, 5,0s cast, range 6 circle
        IcyThroes3 = 31363, // Helper->player, 5,0s cast, range 6 circle
        IcyThroes4 = 32697, // Helper->self, 5,0s cast, range 6 circle
        RoarOfAlbion = 31364, // Boss->self, 7,0s cast, range 60 circle
    };

    public enum IconID : uint
    {
        Tankbuster = 218, // player
        Target = 210, // IceCrystal
        Spreadmarker = 139, // player
    };

    class WildlifeCrossing : Components.GenericAOEs
    {
        private static readonly AOEShapeRect rect = new(20, 5, 20);
        private WPos stampede1 = default;
        private WPos stampede2 = default;
        private int stampede1counter;
        private int stampede2counter;
        private bool active1;
        private bool active2;
        private DateTime _reset1;
        private DateTime _reset2;           

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (active1)
                    yield return new(rect, stampede1, 90.Degrees());
            if (active2)
                    yield return new(rect, stampede2, 90.Degrees());                  
        }

        public override void OnEventEnvControl(BossModule module, byte index, uint state)
        {
            if (state == 0x00020001)
            {
                if (index is 0x21 or 0x25)
                    if (stampede1 == default)
                    {
                        stampede1 = new(24, -759);
                        active1 = true;
                    }
                    else
                    {
                        stampede2 = new(24, -759);
                        active2 = true;
                    }                        
                if (index is 0x22 or 0x26)
                    if (stampede1 == default)
                    {
                        stampede1 = new(24, -749);
                        active1 = true;
                    }
                    else
                    {
                        stampede2 = new(24, -749);
                        active2 = true;
                    }    
                if (index is 0x23 or 0x27)
                    if (stampede1 == default)
                    {
                        stampede1 = new(24, -739);
                        active1 = true;
                    }
                    else
                    {
                        stampede2 = new(24, -739);
                        active2 = true;
                    }    
                if (index is 0x24 or 0x28)
                    if (stampede1 == default)
                    {
                        stampede1 = new(24, -729);
                        active1 = true;
                    }
                    else
                    {
                        stampede2 = new(24, -729);
                        active2 = true;
                    }   
            }
        }

        public override void Update(BossModule module)
        {
            if (_reset1 != default && module.WorldState.CurrentTime > _reset1)
            {
                active1 = false;
                stampede1counter = 0;
                stampede1 = default;
                _reset1 = default;
            }
            if (_reset2 != default && module.WorldState.CurrentTime > _reset2)
            {
                active2 = false;
                stampede2counter = 0;
                stampede2 = default;
                _reset2 = default;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.WildlifeCrossing)
            {
                if (MathF.Abs(caster.Position.Z - stampede1.Z) < 1)
                    ++stampede1counter;
                if (MathF.Abs(caster.Position.Z - stampede2.Z) < 1)
                    ++stampede2counter;
                if (stampede1counter == 30) //sometimes stampedes only have 30 instead of 31 hits for some reason, so i take the lower value and add a 0,5s reset timer via update
                    _reset1 = module.WorldState.CurrentTime.AddSeconds(0.5f);
                if (stampede2counter == 30)
                    _reset2 = module.WorldState.CurrentTime.AddSeconds(0.5f);
            }
        }
    }

    class IcyThroes : Components.GenericBaitAway
    {
        private readonly List<Actor> _targets = new();

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.Spreadmarker)
            {
                CurrentBaits.Add(new(module.PrimaryActor, actor, new AOEShapeCircle(6)));
                _targets.Add(actor);
                CenterAtTarget = true;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.IcyThroes3)
            {
                CurrentBaits.Clear();
                _targets.Clear();
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_targets.Contains(actor))
                hints.Add("Bait away!");
        }
    }

    class Icebreaker : Components.GenericAOEs
    {
        private List<Actor> _casters = new();
        private static readonly AOEShapeCircle circle = new(17);
        private DateTime _activation;

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_casters.Count > 0)
                foreach (var c in _casters)
                    yield return new(circle, c.Position, activation: _activation);
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.Target)
            {
                _casters.Add(actor);
                _activation = module.WorldState.CurrentTime.AddSeconds(6);
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.Icebreaker)
                _activation = spell.NPCFinishAt;
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.Icebreaker)
                _casters.Clear();
        }
    }

    class IcyThroes2 : Components.SelfTargetedAOEs
    {
        public IcyThroes2() : base(ActionID.MakeSpell(AID.IcyThroes4), new AOEShapeCircle(6)) { }
    }


    class KnockOnIce : Components.SelfTargetedAOEs
    {
        public KnockOnIce() : base(ActionID.MakeSpell(AID.KnockOnIce2), new AOEShapeCircle(5)) { }
    }

    class RightSlam : Components.SelfTargetedAOEs
    {
        public RightSlam() : base(ActionID.MakeSpell(AID.RightSlam), new AOEShapeRect(20, 80, directionOffset: -90.Degrees())) { } //full width = half width in this case + angle is detected incorrectly, length and width are also switched
    }

    class LeftSlam : Components.SelfTargetedAOEs
    {
        public LeftSlam() : base(ActionID.MakeSpell(AID.LeftSlam), new AOEShapeRect(20, 80, directionOffset: 90.Degrees())) { } //full width = half width in this case + angle is detected incorrectly, length and width are also switched
    }

    class AlbionsEmbrace : Components.SingleTargetCast
    {
        public AlbionsEmbrace() : base(ActionID.MakeSpell(AID.AlbionsEmbrace)) { }
    }

    class RoarOfAlbion : Components.CastLineOfSightAOE
    {
        public RoarOfAlbion() : base(ActionID.MakeSpell(AID.RoarOfAlbion), 60, false) { }
        public override IEnumerable<Actor> BlockerActors(BossModule module) => module.Enemies(OID.IcyCrystal);
    }

    class D130AlbusGriffinStates : StateMachineBuilder
    {
        public D130AlbusGriffinStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<WildlifeCrossing>()
                .ActivateOnEnter<LeftSlam>()
                .ActivateOnEnter<RightSlam>()
                .ActivateOnEnter<AlbionsEmbrace>()
                .ActivateOnEnter<Icebreaker>()
                .ActivateOnEnter<KnockOnIce>()
                .ActivateOnEnter<IcyThroes>()
                .ActivateOnEnter<IcyThroes2>()
                .ActivateOnEnter<RoarOfAlbion>();
        }
    }

    [ModuleInfo(CFCID = 896, NameID = 12245)]
    public class D130AlbusGriffin : BossModule
    {
        public D130AlbusGriffin(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(24, -744), 19.5f)) { }
    }
}
