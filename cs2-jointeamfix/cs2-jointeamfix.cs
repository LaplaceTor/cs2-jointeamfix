using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace cs2_jointeamfix;

public class JoinTeamFix : BasePlugin
{
    public override string ModuleName => "cs2-jointeamfix";
    public override string ModuleAuthor => "Lapl";
    public override string ModuleVersion => "4.0";
    private string[] TeamValue = new string[3];
    private List<string[]> TeamHistory = new();
    private List<CBaseEntity> spawnentityct = new();
    private List<CBaseEntity> spawnentityt = new();
    private List<Vector> spawnoriginct = new();
    private List<Vector> spawnorigint = new();

    public override void Load(bool hotLoad)
    {
        RegisterListener<Listeners.OnMapStart>((onmapstart) =>
        {
            spawnentityct.Clear();
            spawnentityt.Clear();
            spawnoriginct.Clear();
            spawnorigint.Clear();
            Server.NextFrame(()=>
            {
                spawnentityct = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("info_player_counterterrorist").ToList();
                if(spawnentityct.Count != 0)
                {
                    foreach(var entity in spawnentityct)
                    {
                        entity.AbsOrigin!.Z += 16f;
                        spawnoriginct.Add(entity.AbsOrigin!);
                    }
                }
                spawnentityt = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("info_player_terrorist").ToList();
                if(spawnentityt.Count != 0)
                {
                    foreach(var entity in spawnentityt)
                    {
                        entity.AbsOrigin!.Z += 16f;
                        spawnorigint.Add(entity.AbsOrigin!);
                    }
                }
            });
        });

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
                TeamValue = new string[] { @event.Userid!.SteamID.ToString(), @event.Team.ToString(), @event.Oldteam.ToString() };
                TeamHistory.Add(TeamValue);
            }
            else
                TeamHistory = TeamHistory.Where(x => !x.Contains(@event.Userid!.SteamID.ToString())).ToList();
            return HookResult.Continue;
        });
        RegisterEventHandler<EventJointeamFailed>((@event, info) =>
        {
            TryJoinTeam(@event.Userid!);
            TeamHistory = TeamHistory.Where(x => !x.Contains(@event.Userid!.SteamID.ToString())).ToList();
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
        Vector? spawnorigin;
        List<Vector> spawnorigingroup = new();
        bool searchany = false;
        Random random = new();

        while (!foundTeamJoin)
        {
            if (player.TeamNum == 2)
            {
                spawnorigingroup = spawnorigint;
                if(searchany == true)
                    spawnorigingroup = spawnoriginct;
            }
            else if (player.TeamNum == 3)
            {
                spawnorigingroup = spawnoriginct;
                if(searchany == true)
                    spawnorigingroup = spawnorigint;
            }
            if (spawnorigingroup.Count() != 0)
            {
                spawnorigin = spawnorigingroup[random.Next(0,spawnorigingroup.Count-1)];
                player.PlayerPawn.Value!.Teleport(spawnorigin!, new QAngle(0, 0, 0), new Vector(0, 0, 0));
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