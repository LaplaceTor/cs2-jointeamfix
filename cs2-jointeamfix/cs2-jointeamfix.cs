using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace cs2_jointeamfix;

public class JoinTeamFix : BasePlugin
{
    public override string ModuleName => "cs2-jointeamfix";
    public override string ModuleAuthor => "Lapl";
    public override string ModuleVersion => "3.0";

    private string[] TeamValue = new string[3];

    private List<string[]> TeamHistory = new();

    public override void Load(bool hotLoad)
    {
        RegisterEventHandler<EventRoundStart>((@event, info) =>
        {
            TeamHistory.Clear();
            var playerlist = Utilities.GetPlayers().Where((x) => x.TeamNum > 1).ToList();
            foreach (var player in playerlist)
                if (!player.PawnIsAlive)
                    TryJoinTeam(player);
            return HookResult.Continue;
        });
        RegisterEventHandler<EventPlayerTeam>((@event, info) =>
        {
            if (!@event.Isbot && !@event.Disconnect)
            {
                TeamValue = new string[] { @event.Userid.SteamID.ToString(), @event.Team.ToString(), @event.Oldteam.ToString() };
                TeamHistory.Add(TeamValue);
            }
            else
                TeamHistory = TeamHistory.Where(x => !x.Contains(@event.Userid.SteamID.ToString())).ToList();
            return HookResult.Continue;
        });
        RegisterEventHandler<EventJointeamFailed>((@event, info) =>
        {
            TryJoinTeam(@event.Userid);
            TeamHistory = TeamHistory.Where(x => !x.Contains(@event.Userid.SteamID.ToString())).ToList();
            return HookResult.Continue;
        });
    }

    public void TryJoinTeam(CCSPlayerController player)
    {
        if (TeamHistory.Any(x => x.Contains(player.SteamID.ToString())))
        {
            var playerhistory = TeamHistory.Where(x => x.Contains(player.SteamID.ToString())).FirstOrDefault();
            if (player.TeamNum == int.Parse(playerhistory![2]))
                switch (int.Parse(playerhistory[1]))
                {
                    case 1: player.ChangeTeam(CsTeam.Spectator); break;
                    case 2: player.ChangeTeam(CsTeam.Terrorist); break;
                    case 3: player.ChangeTeam(CsTeam.CounterTerrorist); break;
                }
        }
        bool foundTeamJoin = false;
        QAngle? spawnangle;
        Vector? spawnorigin;
        List<CBaseEntity> spawnentitygroup = new();
        CBaseEntity? spawnentity = null;
        bool searchany = false;
        Random random = new();

        while (!foundTeamJoin)
        {
            if (player.TeamNum == 2 || searchany == true)
                spawnentitygroup = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("info_player_counterterrorist").ToList();
            if (player.TeamNum == 3 || searchany == true)
                spawnentitygroup = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("info_player_counterterrorist").ToList();
            if (spawnentity != null)
            {
                spawnentity = spawnentitygroup[random.Next(0,spawnentitygroup.Count-1)];
                spawnangle = spawnentity.AbsRotation;
                spawnorigin = spawnentity.AbsOrigin;
                player.PlayerPawn.Value!.Teleport(spawnorigin!, spawnangle!, new Vector(0, 0, 0));
                player.Respawn();
                foundTeamJoin = true;
            }
            else
            {
                searchany = true;
            }
        }
    }
}

