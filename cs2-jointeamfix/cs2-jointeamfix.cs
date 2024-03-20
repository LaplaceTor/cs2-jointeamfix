using System.Security.Cryptography.X509Certificates;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace cs2_jointeamfix;

public class JoinTeamFix : BasePlugin
{
    public override string ModuleName => "cs2-jointeamfix";
    public override string ModuleAuthor => "Lapl";
    public override string ModuleVersion => "3.0.1";

    private string[] TeamValue = new string[3];

    private List<string[]> TeamHistory = new();

    private Random random = new();

    public override void Load(bool hotLoad)
    {
        RegisterEventHandler<EventRoundStart>((@event, info) =>
        {
            TeamHistory.Clear();
            var playerlist = Utilities.GetPlayers().Where((x) => x.TeamNum > 1).ToList();
            TeamBalanceCheck(playerlist);
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
            if (TeamHistory.Any(x => x.Contains(@event.Userid.SteamID.ToString())) && @event.Userid.TeamNum < 2)
            {
                var playerhistory = TeamHistory.Where(x => x.Contains(@event.Userid.SteamID.ToString())).FirstOrDefault();
                if (@event.Userid.TeamNum == int.Parse(playerhistory![2]))
                    switch (int.Parse(playerhistory[1]))
                    {
                        case 1: @event.Userid.ChangeTeam(CsTeam.Spectator); break;
                        case 2: @event.Userid.ChangeTeam(CsTeam.Terrorist); break;
                        case 3: @event.Userid.ChangeTeam(CsTeam.CounterTerrorist); break;
                    }
            }
            TeamHistory = TeamHistory.Where(x => !x.Contains(@event.Userid.SteamID.ToString())).ToList();
            return HookResult.Continue;
        });
    }

    private void TeamBalanceCheck(List<CCSPlayerController> playerlist)
    {
        var tlist = playerlist.Where(x => x.TeamNum == 2).ToList();
        var ctlist = playerlist.Where(x => x.TeamNum == 3).ToList();
        if (tlist.Count() - ctlist.Count() > 1)
        {
            var gap = Math.Floor((tlist.Count() - ctlist.Count()) * 0.5f);
            for (var x = 0; x < gap; x++)
            {
                var randomplayer = tlist[random.Next(0, tlist.Count() - 1)];
                if (randomplayer.PawnIsAlive)
                    randomplayer.SwitchTeam(CsTeam.CounterTerrorist);
                else
                    randomplayer.ChangeTeam(CsTeam.CounterTerrorist);
            }
        }
        else if (ctlist.Count() - tlist.Count() > 1)
        {
            var gap = Math.Floor((ctlist.Count() - tlist.Count()) * 0.5f);
            for (var x = 0; x < gap; x++)
            {
                var randomplayer = ctlist[random.Next(0, ctlist.Count() - 1)];
                if (randomplayer.PawnIsAlive)
                    randomplayer.SwitchTeam(CsTeam.Terrorist);
                else
                    randomplayer.ChangeTeam(CsTeam.Terrorist);
            }
        }
        else
            return;
    }

    public void TryJoinTeam(CCSPlayerController player)
    {
        bool FoundSpawnplace = false;
        QAngle? spawnangle;
        Vector? spawnorigin;
        List<CBaseEntity> spawnentitygroup = new();
        CBaseEntity? spawnentity = null;
        bool searchany = false;

        while (!FoundSpawnplace)
        {
            if (player.TeamNum == 2 || searchany == true)
                spawnentitygroup = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("info_player_counterterrorist").ToList();
            if (player.TeamNum == 3 || searchany == true)
                spawnentitygroup = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("info_player_counterterrorist").ToList();
            if (spawnentity != null)
            {
                spawnentity = spawnentitygroup[random.Next(0, spawnentitygroup.Count - 1)];
                spawnangle = spawnentity.AbsRotation;
                spawnorigin = spawnentity.AbsOrigin;
                player.PlayerPawn.Value!.Teleport(spawnorigin!, spawnangle!, new Vector(0, 0, 0));
                player.Respawn();
                FoundSpawnplace = true;
            }
            else
            {
                searchany = true;
            }
        }
    }
}

