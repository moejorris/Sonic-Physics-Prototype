using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "HomingAttack_Data", menuName = "Scriptable Objects/HomingAttack_Data")]
public class HomingAttack_Data : ScriptableObject, IAbility
{
    [SerializeField] AudioClip sound;
    [SerializeField] InputActionReference button;
    [SerializeField] float homingSpeed = 1.5f;
    [SerializeField] float detectionRadius = 10f;
    [SerializeField] bool requireTargetOnScreen = true;
    [SerializeField] bool hasJumpDash = true;
    [SerializeField] float jumpDashSpeed = 0.5f;
    [SerializeField] bool requireFacing = true;
    [SerializeField] bool requireDirInput = false;
    [SerializeField] bool useReticle = true;
    [SerializeField] GameObject reticlePrefab;

    public SpecialAbility GetAbility(PlayerMovement playerMovement)
    {
        SpecialAbility_HomingAttack ability = new(playerMovement)
        {
            sound = this.sound,
            homingSpeed = this.homingSpeed == 0f ? playerMovement.movementStats.dash_speed : this.homingSpeed,
            jumpDashSpeed = this.jumpDashSpeed == 0f ? playerMovement.movementStats.top_speed : this.jumpDashSpeed,
            detectionRadius = this.detectionRadius,
            hasJumpDash = this.hasJumpDash,
            requireFacing = this.requireFacing,
            requireDirInput = this.requireDirInput,
            requireTargetOnScreen = this.requireTargetOnScreen,

            inputAxesLockType = SpecialAbility.Ability_InputAxesLockType.BothAxes,
            inputButtonLockType = SpecialAbility.Ability_InputButtonLockType.BothButtons,
            button = button,
            animationMode = SpecialAbility.Ability_AnimationOverride.Ball,
            criteria = SpecialAbility.Ability_ActivationCriteria.Airborne,
            buttonPressType = SpecialAbility.Ability_ButtonPressType.Press,            
        };

        if(useReticle && reticlePrefab)
        {
            ability.AssignReticle(reticlePrefab);
        }

        return ability;
    }
}
