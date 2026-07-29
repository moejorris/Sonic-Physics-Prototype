using UnityEngine;

[CreateAssetMenu(fileName = "Player Character", menuName = "Player Characters/New Character")]
public class PlayerCharacter : ScriptableObject
{
    public string char_name = "Default Character";
    public string char_name_hud = "CHAR";
    public MovementStats_Character playerStats;
    public bool CanGoSuper => superFormStats != null;
    public MovementStats_Character superFormStats;

    #if UNITY_EDITOR
    //Ensures that player character doesn't have empty strings for names
    public void CreateFallBackReferences()
    {
        playerStats ??= CreateInstance<MovementStats_Character>();
        superFormStats ??= CreateInstance<MovementStats_Character>();
    }

    void AssignDefaultName(ref string name, string defaultName)
    {
        if(name.Length > 0 || name.Trim() == "")
        {
            name = defaultName;
        }
    }
    
    void OnValidate() //Force specific rules for name formatting in the inspector
    {
        AssignDefaultName(ref char_name, "Default Character");
        AssignDefaultName(ref char_name_hud, "CHAR");

        //Forces length limits and uppercase for HUD name
        char_name_hud = char_name_hud.ToUpper();
        if(char_name_hud.Length > 5)
        {
            char_name_hud = char_name_hud.Substring(0, 5);
        }
    }
    #endif
}
