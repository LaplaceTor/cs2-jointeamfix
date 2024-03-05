using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace cs2_jointeamfix;

public class JoinTeamFix : BasePlugin
{
    private CBaseEntity? entity;
    private QAngle? angle;
    private CounterStrikeSharp.API.Modules.Utils.Vector? position;

    public override string ModuleName => "cs2-jointeamfix";
    public override string ModuleVersion => "0.1.0";

    public override void Load(bool hotLoad)
    {
        RegisterEventHandler<EventJointeamFailed>((@event, info) =>
        {
            if (@event.Reason == 0)
                @event.Userid.ChangeTeam(CsTeam.Terrorist);
            entity = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("info_player_terrorist").ElementAt(0);
            if (entity != null)
            {
                angle = entity.AbsRotation;
                position = entity.AbsOrigin;
            }
            @event.Userid.PlayerPawn.Value!.Teleport(position, angle, new Vector(0, 0, 0));
            return HookResult.Continue;
        });
    }
}