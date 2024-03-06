using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace cs2_jointeamfix;

public class JoinTeamFix : BasePlugin
{
    public override string ModuleName => "cs2-jointeamfix";
    public override string ModuleVersion => "1.0";

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
                    spawnentity = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("info_player_terrorist").FirstOrDefault();
                    @event.Userid.ChangeTeam(CsTeam.Terrorist);
                }
                else
                {
                    spawnentity = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("info_player_counterterrorist").FirstOrDefault();
                    @event.Userid.ChangeTeam(CsTeam.CounterTerrorist);
                }
                if (spawnentity != null)
                {
                    spawnangle = spawnentity.AbsRotation;
                    spawnorigin = spawnentity.AbsOrigin;
                    @event.Userid.PlayerPawn.Value!.Teleport(spawnorigin!, spawnangle!, new Vector(0, 0, 0));
                    @event.Userid.Respawn();
                    foundTeamJoin = true;
                }
                else if (!searchCT)
                {
                    searchCT = true;
                }
                else
                {
                    @event.Userid.ChangeTeam(CsTeam.Spectator);
                    break;
                }
            }
            return HookResult.Continue;
        });
    }
}