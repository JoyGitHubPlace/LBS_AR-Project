using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Global
{

    public static float tilesizeRank = 0.1f;
    public static string POIPicUrl = "http://120.27.131.52:8000";
    public static float Lat = 31.2410499173f;//31.24031     
    public static float lon = 121.4961463038f;//121.4962

    public static bool IsDebug = true;
    public static string BattleIp = "";
    public static uint BattlePort = 0;
    public static bool IsCollisionEnter = false;
    public static bool IsCollisionStay = false;
    public static int MAX_CIRCLE_NUM = 6;
    public static int MAX_ROOM_PLAYER_NUM = 100;
    public static float TileSize = 300;
    public static float stringS = 0;
    public static bool SafeIsland = false;
    public static float player_y_up = 100f;
    public static float player_y_down = 0.5f;
    public static float item_y_up = 100f;
    public static float item_y_down = 0.2f;
    public static float wall_down = -10f;
    public static float Map_X1 = 0;
    public static float Map_X2 = 0;
    public static float Map_Y1 = 0;
    public static float Map_Y2 = 0;
    //path
    public static string SKILL_JSON_PATH = "Config/Skill";
    public static string EQUIP_JSON_PATH = "Config/Equip";
    public static string BOARDROUND_JSON_PATH = "Config/BoardRound";
    public static string ITEM_JSON_PATH = "Config/Item";
    public static string ATTRIBUTETABLE_JSON_PATH = "Config/AttributeTable";
    public static string BUFF_JSON_PATH = "Config/Buff";

    public static string PLAYERGEADE_CONFIG_PATH = "Config/PlayerGradeConfig";
    public static string PLAYER_EQUITMENT_PATH = "Config/EquitmentConfig";
    public static string ITEM_PATH = "Config/ItemConfig";
    public static string SKILLCONFIG_PATH = "Config/SkillConfig";

    public static string PREFAB_PATH = "Prefabs/";
    public static string PREFAB_EQUIPMENT_PATH = "Prefabs/Equip/";
    public static string PREFAB_COMMON_PATH = "Prefabs/Common/";
    public static string PREFAB_ITEMS_PATH = "Prefabs/Items/";
    public static string PREFAB_UI_PATH = "Prefabs/UI/";
    public static string PREFAB_WEAPON = PREFAB_PATH + "Weapon/";
    //effect
    public static string PREFAB_Effects = PREFAB_PATH + "Effects/";
    //Texture
    public static string Texture_UI_PATH = "Textures/UI/";
    public static string Texture_UICOMMON_PATH = Texture_UI_PATH + "common/";
    //Icon 
    public static string ICON_SKILL_PATH = "Icon/SkillUI/";
    public static string ICON_ITEM_PATH = "Icon/Item/";
    public static int EQUIPMENT_UPNUM = 20;
	public static int EQUIPMENT_DOWNNUM = 21;
	public static int EQUIPMENT_LEFTNUM = 22;
	public static int EQUIPMENT_RIGHT = 23;

    public static float BuildGround_H1 = -0.05f;
    public static float BuildGround_H2 = -0.1f;
    public static float Water_H = 0.2f;
    public static float Road_H = 0.3f;
    public static float GreenGround_H = 0.25f;

}
