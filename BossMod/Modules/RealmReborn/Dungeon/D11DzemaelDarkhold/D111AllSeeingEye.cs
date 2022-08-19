﻿namespace BossMod.RealmReborn.Dungeon.D11DzemaelDarkhold.D111AllSeeingEye
{
    public enum OID : uint
    {
        Boss = 0x605, // x1
        MoucheVolante = 0x606, // spawn during fight
        Amanuensis = 0x607, // spawn during fight
        Crystal = 0x1E8594, // x6, EventObj type
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss/Amanuensis->player, no cast, single-target
        CursedGaze = 512, // Boss->self, 2.5s cast, range 6+2.7 90-degree cone
        DreadGaze = 513, // Boss->self, 3.0s cast, range 6+2.7 90-degree cone
        EyesOnMe = 951, // Boss->self, no cast, raidwide

        AutoAttackAdd = 878, // MoucheVolante->player, no cast, single-target
        Thunderstrike = 1097, // MoucheVolante->self, 2.0s cast, range 10+1.2 width 3 rect
        Condemnation = 1100, // Amanuensis->self, 2.5s cast, range 6+1.3 90-degree cone
    };

    public enum SID : uint
    {
        Invincibility = 325, // none->Boss, extra=0x0
    };

    class CursedGaze : Components.SelfTargetedAOEs
    {
        public CursedGaze() : base(ActionID.MakeSpell(AID.CursedGaze), new AOEShapeCone(8.7f, 45.Degrees()), false) { }
    }

    class DreadGaze : Components.SelfTargetedAOEs
    {
        public DreadGaze() : base(ActionID.MakeSpell(AID.DreadGaze), new AOEShapeCone(8.7f, 45.Degrees()), false) { }
    }

    class Thunderstrike : Components.SelfTargetedAOEs
    {
        public Thunderstrike() : base(ActionID.MakeSpell(AID.Thunderstrike), new AOEShapeRect(11.2f, 1.5f), false) { }
    }

    class Condemnation : Components.SelfTargetedAOEs
    {
        public Condemnation() : base(ActionID.MakeSpell(AID.Condemnation), new AOEShapeCone(7.3f, 45.Degrees()), false) { }
    }

    // try to always stay in active crystal closest to boss
    class Positioning : BossComponent
    {
        private static AOEShapeCircle _shape = new(8); // TODO: verify range

        public override void UpdateSafeZone(BossModule module, int slot, Actor actor, SafeZone zone)
        {
            if (module.PrimaryActor.CastInfo == null) // do not restrict zone while boss is casting, to allow avoiding aoe, even if it means temporarily leaving crystal veil
            {
                var closestCrystal = module.Enemies(OID.Crystal).Closest(module.PrimaryActor.Position);
                if (closestCrystal != null)
                    zone.RestrictToZone(_shape, closestCrystal.Position, new(), module.WorldState.CurrentTime, 10000);
            }
        }
    }

    class D111AllSeeingEyeStates : StateMachineBuilder
    {
        public D111AllSeeingEyeStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<CursedGaze>()
                .ActivateOnEnter<DreadGaze>()
                .ActivateOnEnter<Thunderstrike>()
                .ActivateOnEnter<Condemnation>()
                .ActivateOnEnter<Positioning>();
        }
    }

    public class D111AllSeeingEye : BossModule
    {
        public D111AllSeeingEye(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(40, 70), 30)) { }

        public override bool FillTargets(BossTargets targets, int pcSlot)
        {
            targets.Autofill(WorldState);
            if (PrimaryActor.FindStatus(SID.Invincibility) != null)
                targets.Valid.Remove(PrimaryActor);
            return true;
        }
    }
}
