using UnityEngine;

[System.Serializable]
public class SpecialAbility_HomingAttack : SpecialAbility
{
    public AudioClip sound;
    public float homingSpeed;
    public float jumpDashSpeed;
    public bool requireTargetOnScreen;
    public float detectionRadius;
    public bool hasJumpDash;
    public bool requireFacing;
    public bool requireDirInput;

    float amountMoved = 0;
    float distanceAway = 0;
    float minDistance = 0.1f; //how close the player has to be to cancel the homing attack
    int iterarionsSinceStart = 0;
    int maxIterations = 100;

    public GameObject reticlePrefab;
    GameObject reticleGO;

    Transform lastAttackedTarget;
    Transform homingTarget;
    Vector2 homingDirection;

    bool alreadyUsed = false;

    protected override bool SecondaryActivationCriteria()
    {
        return (homingTarget != null || hasJumpDash) && (!requireDirInput || InputManager.Instance.moveInputDir.x != 0f) && !alreadyUsed;
    }

    public override void UpdateInactive()
    {
        base.UpdateInactive();
        
        if(ActivationCriteriaMet())
        {
            bool newTarget;
            Transform prevTarget = homingTarget;
            homingTarget = null;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(playerMovement.Position, detectionRadius);

            if(InputManager.Instance.moveInputDir.x == 0f && requireDirInput) return;

            float facingSign = playerMovement.facingRight ? 1f : -1f;

            float lowestDistance = detectionRadius;
            float greatestDistance = 0f;

            //Find the closest homing target

            for(int i = 0; i < colliders.Length; i++)
            {
                if(colliders[i].CompareTag("Homing Target"))
                {
                    Vector2 dir = (Vector2)colliders[i].transform.position - playerMovement.Position;
                    dir.Normalize();

                    if(requireFacing && (Mathf.Sign(dir.x) != facingSign || playerMovement.Position.x == colliders[i].transform.position.x))
                    {
                        continue;
                    }


                    float candidateDistance = Vector2.Distance(colliders[i].transform.position, playerMovement.Position);
                    
                    if(candidateDistance > greatestDistance)
                    {
                        greatestDistance = candidateDistance;
                    }

                    //Check for obstacles in the way
                    RaycastHit2D hit = Physics2D.Raycast(playerMovement.Position, dir, candidateDistance);

                    bool onScreen = !requireTargetOnScreen || colliders[i].GetComponent<Renderer>().isVisible;
                    bool sameAsLastTarget = colliders[i].transform == lastAttackedTarget;

                    //if this candidate is the same as the last target, we change it's distance to be the greatest distance so it is the lowest priority, but still targetable if there are no other suitable candidates.
                    if(sameAsLastTarget)
                    {
                        candidateDistance = greatestDistance;
                    }

                    if(i == 0 || candidateDistance < lowestDistance && hit.collider == colliders[i] && onScreen)
                    {
                        homingTarget = colliders[i].transform;
                        lowestDistance = candidateDistance;
                        homingDirection = dir;            
                    }
                }
            }

            newTarget = prevTarget != homingTarget;

            if(homingTarget == null)
            {
                homingDirection = Vector2.right * facingSign;
            }
            UpdateReticle(newTarget);
        }
        else
        {
            homingTarget = null;

            if(playerMovement.isGrounded())
            {
                lastAttackedTarget = null;
                alreadyUsed = false;          
            }
            reticleGO.SetActive(false);
        }


        CustomDrawGizmos();
    }

    public override void ActivateAbility()
    {
        base.ActivateAbility();
        float speed;

        if(homingTarget != null) //performing homing attack
        {
            homingDirection = ((Vector2) homingTarget.transform.position - playerMovement.Position).normalized;
            distanceAway = Vector2.Distance(playerMovement.Position, homingTarget.position);
            amountMoved = 0;
            iterarionsSinceStart = 0;
            maxIterations = Mathf.CeilToInt(distanceAway / homingSpeed) + 2; //+2 is to allow for wiggle room
            speed = homingSpeed;
            lastAttackedTarget = homingTarget;
        }
        else
        {            
            //Additive jump dash
            // speed = Mathf.Abs(playerMovement.Velocity.x) + jumpDashSpeed;

            //Fixed jump dash
            speed = Mathf.Max(jumpDashSpeed, Mathf.Abs(playerMovement.Velocity.x));
            alreadyUsed = true;
            DeactivateAbility();
        }

        playerMovement.SetVelocity(homingDirection * speed);
        playerMovement.PlaySoundEffect(sound);

        playerMovement.EnterBallState();

        playerMovement.trail?.ActivateTrail();
    }

    public override void UpdateActive()
    {
        base.UpdateActive();

        if(homingTarget == null || iterarionsSinceStart >= maxIterations || Vector2.Distance(playerMovement.Position, homingTarget.position) <= minDistance || amountMoved >= distanceAway)
        {
            if(homingTarget != null)
            {
                playerMovement.SetVelocity(Vector2.up * playerMovement.movementStats.jump_force);                            
                
                //Below needed for variable rebound height (when HA not initiated from jumping), needs some thought...
                // playerMovement.isJumping = true;
            }
            DeactivateAbility();
            return;
        }

        playerMovement.ForceStateChange(PlayerMovement.PlayerState.Airborne);
        playerMovement.EnterBallState();

        float curDistance = Vector2.Distance(homingTarget.position, playerMovement.Position);

        float moveAmount = Mathf.Min(homingSpeed, curDistance);
        amountMoved += moveAmount;

        homingDirection = ((Vector2) homingTarget.transform.position - playerMovement.Position).normalized;
        playerMovement.SetVelocity(homingDirection * moveAmount);

        iterarionsSinceStart++;

        playerMovement.EnterBallState();
        playerMovement.trail?.ActivateTrail();
    }

    public override void DeactivateAbility()
    {
        base.DeactivateAbility();
        distanceAway = 0;
        maxIterations = 0;
        amountMoved = 0;
        iterarionsSinceStart = 0;
        homingTarget = null;
    }

    void UpdateReticle(bool newTarget)
    {
        if(reticleGO)
        {
            if(homingTarget == null || !SecondaryActivationCriteria() || !ActivationCriteriaMet())
            {
                reticleGO.SetActive(false);
            }
            else if(newTarget)
            {
                reticleGO.SetActive(false);
                reticleGO.transform.position = homingTarget.transform.position;
                reticleGO.SetActive(true);      
            }
        }
    }

    public SpecialAbility_HomingAttack(PlayerMovement playerMovement)
    {
        this.playerMovement = playerMovement;

    }

    public void AssignReticle(GameObject prefab)
    {   
        reticlePrefab = prefab;

        if(reticlePrefab)
        {
            reticleGO = UnityEngine.Object.Instantiate(reticlePrefab);
            reticleGO.SetActive(false);
        }
    }

    void CustomDrawGizmos()
    {
        if(homingTarget != null)
        {
            Vector2 pos = homingTarget.position;

            Debug.DrawLine(playerMovement.Position, pos, Color.red);
        }
        else
        {
            Debug.DrawRay(playerMovement.Position, homingDirection, Color.red * 0.5f);
        }

        //Draw Detection Radius
        Vector2 prevCirclePos = Vector2.zero;
        for(int i = 0; i < 360; i += 10)
        {
            Vector2 newPos = new Vector2(Mathf.Sin(Mathf.Deg2Rad * i), Mathf.Cos(Mathf.Deg2Rad * i)).normalized * detectionRadius;
            newPos += playerMovement.Position;

            if(i > 0)
            {
                Debug.DrawLine(prevCirclePos, newPos, Color.magenta);
            }
            prevCirclePos = newPos;
        }

    }
}
