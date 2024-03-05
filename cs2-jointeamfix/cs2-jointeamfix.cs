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
        CCSPlayerController player;
        player.PlayerPawn.Value.Teleport(1,2,3);
        entity = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("info_player_terrorist").ElementAt(0);
        angle = entity.AbsRotation;
        position = entity.AbsOrigin;
    }
}