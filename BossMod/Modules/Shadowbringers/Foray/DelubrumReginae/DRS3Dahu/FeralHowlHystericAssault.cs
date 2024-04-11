﻿namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS3Dahu;

// these two abilities are very similar, only differ by activation delay and action id
class FeralHowlHystericAssault : Components.Knockback
{
    private AID _aidCast;
    private float _delay;
    private Source? _source;

    public FeralHowlHystericAssault(BossModule module, AID aidCast, AID aidAOE, float delay) : base(module, ActionID.MakeSpell(aidAOE), true)
    {
        _aidCast = aidCast;
        _delay = delay;
        StopAtWall = true; // TODO: not all the wall is safe...
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Utils.ZeroOrOne(_source);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == _aidCast)
            _source = new(caster.Position, 30, spell.NPCFinishAt.AddSeconds(_delay));
    }
}

class FeralHowl(BossModule module) : FeralHowlHystericAssault(module, AID.FeralHowl, AID.FeralHowlAOE, 2.1f) { }
class HystericAssault(BossModule module) : FeralHowlHystericAssault(module, AID.HystericAssault, AID.HystericAssaultAOE, 0.9f) { }
