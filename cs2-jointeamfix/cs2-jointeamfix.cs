using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace cs2_jointeamfix;

public class JoinTeamFix : BasePlugin
{
    public override string ModuleName => "cs2-jointeamfix";
    public override string ModuleVersion => "0.1.0";

    public override void Load(bool hotLoad)
    {
        RegisterEventHandler<EventJointeamFailed>((@event, info) =>
        {
            bool foundTeamJoin = false;
            QAngle? spawnangle;
            Vector? spawnorigin;
            CBaseEntity? spawnentity = null;
            bool searchCT = false;
            while (!foundTeamJoin)
            {
                if (!searchCT)
                {
                    spawnentity = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("info_player_terrorist").ElementAt(0);
                    @event.Userid.ChangeTeam(CsTeam.Terrorist);
                }
                else
                {
                    spawnentity = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("info_player_counterterrorist").ElementAt(0);
                    @event.Userid.ChangeTeam(CsTeam.CounterTerrorist);
                }
                if (spawnentity != null)
                {
                    spawnangle = spawnentity.AbsRotation;
                    spawnorigin = spawnentity.AbsOrigin;
                    @event.Userid.PlayerPawn.Value!.Teleport(spawnorigin!, spawnangle!, new Vector(0, 0, 0));
                    foundTeamJoin = true;
                }
                else if (!searchCT)
                {
                    searchCT = true;
                }
                else
                {
                    break;
                }
            }
            return HookResult.Continue;
        });
    }
}