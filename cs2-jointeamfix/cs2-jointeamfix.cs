using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace cs2_jointeamfix;

public class JoinTeamFix : BasePlugin
{
    public override string ModuleName => "cs2-jointeamfix";
    public override string ModuleAuthor => "Lapl";
    public override string ModuleVersion => "5.1";
    private string[] TeamValue = new string[3];
    private List<string[]> TeamHistory = new();
    private Random random = new();
    private List<CBaseEntity> spawnentityct = new();
    private List<CBaseEntity> spawnentityt = new();
    private List<Vector> spawnoriginct = new();
    private List<Vector> spawnorigint = new();
    private List<ulong> changeteamcalm = new();

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
            changeteamcalm.Clear();
            var playerlist = Utilities.GetPlayers().Where((x) => x.TeamNum > 1).ToList();
            TeamBalanceCheck(playerlist);
            foreach (var player in playerlist)
                if (!player.PawnIsAlive)
                    JoinTeam(player);
            return HookResult.Continue;
        });
        RegisterEventHandler<EventPlayerTeam>((@event, info) =>
        {
            if (!@event.Isbot)
            {
                if(!@event.Disconnect)
                {
                    TeamValue = new string[] { @event.Userid!.SteamID.ToString(), @event.Team.ToString(), @event.Oldteam.ToString() };
                    TeamHistory.Add(TeamValue);
                }
                else
                {
                    TeamHistory = TeamHistory.Where(x => !x.Contains(@event.Userid!.SteamID.ToString())).ToList();
                    changeteamcalm = changeteamcalm.Where(x => x != @event.Userid!.SteamID).ToList();
                }
            }
            return HookResult.Continue;
        });
        RegisterEventHandler<EventJointeamFailed>((@event, info) =>
        {
            if (TeamHistory.Any(x => x.Contains(@event.Userid!.SteamID.ToString())))
            {
                if(changeteamcalm.Contains(@event.Userid!.SteamID))
                {
                    @event.Userid.PrintToChat("Change team calm until the next round");
                    return HookResult.Continue;
                }
                var playerhistory = TeamHistory.Where(x => x.Contains(@event.Userid.SteamID.ToString())).First();
                if (@event.Userid.TeamNum == int.Parse(playerhistory![2]))
                    switch (int.Parse(playerhistory[1]))
                    {
                        case 1: @event.Userid.ChangeTeam(CsTeam.Spectator); break;
                        case 2: @event.Userid.ChangeTeam(CsTeam.Terrorist); break;
                        case 3: @event.Userid.ChangeTeam(CsTeam.CounterTerrorist); break;
                    }
                changeteamcalm.Add(@event.Userid!.SteamID);
            }
            TeamHistory = TeamHistory.Where(x => !x.Contains(@event.Userid!.SteamID.ToString())).ToList();
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

    public void JoinTeam(CCSPlayerController player)
    {
        bool FoundSpawnplace = false;
        Vector? spawnorigin;
        List<Vector> spawnorigingroup = new();
        bool searchany = false;
        Random random = new();

        while (!FoundSpawnplace)
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
                FoundSpawnplace = true;
            }
            else
            {
                searchany = true;
            }
        }
    }
}

