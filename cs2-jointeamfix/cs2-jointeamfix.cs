using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace cs2_jointeamfix;

public class JoinTeamFix : BasePlugin
{
    public override string ModuleName => "cs2-jointeamfix";
    public override string ModuleAuthor => "Lapl";
    public override string ModuleVersion => "final";
    private List<Vector> spawnoriginct = new();
    private List<Vector> spawnorigint = new();

    public override void Load(bool hotLoad)
    {
        RegisterListener<Listeners.OnMapStart>((onmapstart) =>
        {
            Server.ExecuteCommand("mp_spectators_max 64");
            spawnoriginct.Clear();
            spawnorigint.Clear();
            Server.NextFrame(()=>
            {
                var spawnentityct = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("info_player_counterterrorist").ToList();
                if(spawnentityct.Count != 0)
                {
                    foreach(var entity in spawnentityct)
                    {
                        entity.AbsOrigin!.Z += 16f;
                        spawnoriginct.Add(entity.AbsOrigin!);
                    }
                }
                var spawnentityt = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("info_player_terrorist").ToList();
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
            var playerlist = Utilities.GetPlayers().Where((x) => x.TeamNum > 1).ToList();
            foreach (var player in playerlist)
                if (!player.PawnIsAlive || player.InSwitchTeam)
                    player.Respawn();
            return HookResult.Continue;
        });
        
        RegisterEventHandler<EventJointeamFailed>((@event, info) =>
        {
            if(@event.Userid == null)
                return HookResult.Continue;
            TryJoinTeam(@event.Userid);
            return HookResult.Continue;
        });
    }

    public void TryJoinTeam(CCSPlayerController player)
    {
        bool foundTeamJoin = false;
        Vector? spawnorigin;
        List<Vector> spawnorigingroup = new();
        bool searchany = false;
        Random random = new();

        while (!foundTeamJoin)
        {
            spawnorigingroup = spawnorigint;
            if(searchany == true)
                spawnorigingroup = spawnoriginct;
            if (spawnorigingroup.Count() != 0)
            {
                spawnorigin = spawnorigingroup[random.Next(0,spawnorigingroup.Count-1)];
                player.PlayerPawn.Value!.Teleport(spawnorigin!, new QAngle(0, 0, 0), new Vector(0, 0, 0));
                player.Respawn();
                foundTeamJoin = true;
            }
            else
                searchany = true;
        }
    }
}