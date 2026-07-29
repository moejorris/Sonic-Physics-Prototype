using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "SpecialAbility_SpinDash", menuName = "Scriptable Objects/SpinDash")]
public class Spindash_Data : ScriptableObject, IAbility
{
    public InputActionReference button;
    public CharacterSprite[] animationFrames;
    public AudioClip chargeSound;
    public AudioClip releaseSound;
    public GameObject dustParticlePrefab;
    [SerializeField] bool requireCrouch = true;

    public SpecialAbility GetAbility(PlayerMovement playerMovement)
    {
        SpecialAbility_Spindash ability = new SpecialAbility_Spindash(playerMovement);

        ability.animationFrames = animationFrames;
        ability.button = button;
        ability.criteria = requireCrouch ? SpecialAbility.Ability_ActivationCriteria.Crouch : SpecialAbility.Ability_ActivationCriteria.Grounded;
        ability.buttonPressType = SpecialAbility.Ability_ButtonPressType.Press;
        ability.inputAxesLockType = SpecialAbility.Ability_InputAxesLockType.BothAxes;
        ability.inputButtonLockType = SpecialAbility.Ability_InputButtonLockType.Jump;
        ability.animationMode = animationFrames != null && animationFrames.Length > 0 && animationFrames[0].sprites.Length > 0 ? SpecialAbility.Ability_AnimationOverride.Custom : SpecialAbility.Ability_AnimationOverride.Ball;

        ability.ApplySoundEffects(chargeSound, releaseSound);
        ability.AssignDustParticle(dustParticlePrefab);

        return ability;
    }
}
