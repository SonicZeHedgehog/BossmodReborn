﻿namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4QueensGuard;

class WindsOfWeight(BossModule module) : Components.GenericAOEs(module)
{
    private List<Actor> _green = new();
    private List<Actor> _purple = new();
    private BitMask _invertedPlayers;

    private static readonly AOEShapeCircle _shape = new(20);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return (_invertedPlayers[slot] ? _purple : _green).Select(c => new AOEInstance(_shape, c.Position, c.CastInfo!.Rotation, c.CastInfo.NPCFinishAt));
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ReversalOfForces)
            _invertedPlayers.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        CasterList(spell)?.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        CasterList(spell)?.Remove(caster);
    }

    private List<Actor>? CasterList(ActorCastInfo spell) => (AID)spell.Action.ID switch
    {
        AID.WindsOfFate => _green,
        AID.WeightOfFortune => _purple,
        _ => null
    };
}
