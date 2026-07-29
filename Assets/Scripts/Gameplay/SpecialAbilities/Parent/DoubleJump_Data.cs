using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "SpecialAbility_DoubleJump", menuName = "Scriptable Objects/DoubleJump")]
public class DoubleJump_Data : ScriptableObject, IAbility
{
    public InputActionReference button;
    public AudioClip jumpSound;

    public SpecialAbility GetAbility(PlayerMovement playerMovement)
    {
        SpecialAbility_DoubleJump ability = new SpecialAbility_DoubleJump(playerMovement);
        // ability.button = button;
        ability.button = button;
        ability.animationMode = SpecialAbility.Ability_AnimationOverride.Ball;
        ability.criteria = SpecialAbility.Ability_ActivationCriteria.Jumping;
        ability.buttonPressType = SpecialAbility.Ability_ButtonPressType.Press;
        ability.sound = jumpSound;
        return ability;
    }
}
