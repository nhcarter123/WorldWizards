using UnityEngine;
using Pathfinding;

using WorldWizards.core.manager;
using System.Collections.Generic;
using System.Linq;

namespace WorldWizards.core.entity.gameObject
{
    public enum contexts_enum
    {
        None, Health, Allies, Enemies
    }

    enum actions_enum
    {
        None, Attack, Flee, Regroup
    }

    public class WWSeeker : Seeker
    {
        int count = 0;

        //States
        bool attacking = false;
        bool alive = true;
        bool fade = false;

        //Public Stats
        public int team;

        //Turn Rate
        public float turnSpeed = 5f;

        //Walk Speed
        public float maxWalkSpeed = 4f;

        //Attack Distance
        public float attackDistance = 1.5f;

        //Aggro Distance
        public float aggroDistance = 18;

        //Attack De-Aggro Distance
        public float deAggroDistance = 30;

        //health
        public int max_health = 100;
        public int health = 100;

        //damage
        public int damage = 5;

        float vision = 100;
        float walkSpeed = 0;
        float acceleration = 0.03f;
        float dist = 0;

        // Delay before character fades out
        int fadeDelay = 0;

        //State of the character
        string state = "idle";

        WWSeeker closest_enemy;
        int waiting = 0;
        Vector3 targetLocation;
        Vector3 positionPrevious;
        Ray sight;
        bool placed = false;
        IEnumerable<WWSeeker> scripts;

        Animator anim;
        Seeker seeker;
        WWSeeker target = null;
        SkinnedMeshRenderer[] rend;
        Rigidbody controller;
        //RVOController controller;
        FunnelModifier modifier;
        AIPath movementController;

        //public string[] actions = { "None", "Attack", "Flee", "Regroup" };
        //public string[] contexts = { "None", "Health", "Allies", "Enemies" };

        public string[] actions = System.Enum.GetNames(typeof(actions_enum));
        public string[] contexts = System.Enum.GetNames(typeof(contexts_enum));

        public List<int> active_actions = new List<int>();
        public List<int> active_contexts = new List<int>();

        List<float> action_ratings = new List<float>();
        List<float> context_ratings = new List<float>();

        public List<int> selectionsA = new List<int>() { 0 };
        public List<int> selectionsB = new List<int>();
        public List<int[]> pairs = new List<int[]>();

        public List<AnimationCurve> curves = new List<AnimationCurve>();

        List<Material> transparentMaterials = new List<Material>();

        private void Start()
        {

            for (var i = 0; i < System.Enum.GetNames(typeof(actions_enum)).Length; i++)
            {
                action_ratings.Add(0);
            }

            for (var i = 0; i < System.Enum.GetNames(typeof(contexts_enum)).Length; i++)
            {
                context_ratings.Add(0);
            }

            UpdateActiveActionsContexts();

            //get character components
            seeker = gameObject.GetComponent<Seeker>();
            controller = gameObject.AddComponent<Rigidbody>();
            movementController = gameObject.AddComponent<AIPath>();
            anim = gameObject.GetComponent<Animator>();
            rend = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();

            movementController.slowdownDistance = 1;
            movementController.pickNextWaypointDist = 1f;
            movementController.endReachedDistance = 1f;
            movementController.slowWhenNotFacingTarget = true;
            movementController.repathRate = 1;
            //movementController.gravity = new Vector3(0,0,0);

            controller.mass = 5;
            controller.freezeRotation = true;

            positionPrevious = transform.position;

            //add a smoothing modifier
            modifier = gameObject.AddComponent<FunnelModifier>();

            targetLocation = transform.position;

            //find all mesh renderers
            for (var i = 0; i < rend.Length; i++)
            {
                Material[] mats = rend[i].materials;
                transparentMaterials.Add(mats[0]);
                mats[0] = rend[i].materials[1];
                mats[1] = rend[i].materials[1];
                rend[i].materials = mats;
            }
        }

        private void Update()
        {
            if (health > 0)
            {

                StepUp();

                ///////////////////////////////////////////////////// BRAIN ///////////////////////////////////////////////////////////////
                //update mind on delay
                if (count > 30)
                {
                    count = 0;

                    //iterate through curves
                    //for (var i = 0; i < active_actions.Count; i++)
                    //{
                    //    action_ratings[active_actions[i]] = 0;
                    //}
                    for (var i = 0; i < curves.Count; i++)
                    {
                        action_ratings[selectionsA[i]] = 0;
                    }

                    for (var i = 0; i < active_contexts.Count; i++)
                    {
                        context_ratings[active_contexts[i]] = CalculateRating(active_contexts[i]);
                    }

                    for (var i = 0; i < curves.Count; i++)
                    {
                        action_ratings[selectionsA[i]] += curves[i].Evaluate(context_ratings[selectionsB[i]]) + 1000;
                    }

                    //average ratings with additive 0.1 * n
                    for (var i = 0; i < action_ratings.Count; i++)
                    {
                        float rating = action_ratings[i];
                        float n = Mathf.Round(rating / 1000);
                        if (n > 0)
                        {
                            rating = (rating - (1000 * n)) / n;
                            action_ratings[i] = rating + (rating * 0.1f * (n - 1));
                        }
                        //Debug.Log(action_ratings[i]);
                    }

                    //find highest rating
                    float highest = 0;
                    int chosen_action = -1;
                    for (var i = 0; i < action_ratings.Count; i++)
                    {
                        if (action_ratings[i] > highest)
                        {
                            highest = action_ratings[i];
                            chosen_action = i;
                        }
                    }
                    if (highest < 0.2)
                    {
                        chosen_action = 0;
                    }

                    ExecuteAction(chosen_action);
                }
                count++;
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                //set animation
                
                var xyVel = Vector3.Distance(transform.position, positionPrevious) * 30f;
                anim.SetFloat("Forward", xyVel);
                movementController.maxSpeed = walkSpeed;
                attacking = false;
                anim.SetBool("Attacking", false);
                //transform.position += new Vector3(0, -0.1f, 0);

                positionPrevious = transform.position;

                if (state == "attack")
                {

                    //If we have a target check if they are in range
                    if (target != null)
                    {
                        if (dist < aggroDistance)
                        {
                            if (dist < attackDistance)
                            {
                                if (target.health > 0 && CanSeeTarget())
                                {
                                    attacking = true;
                                    anim.SetBool("Attacking", true);
                                    movementController.canSearch = false;
                                }
                            }
                        }
                        else if (dist > deAggroDistance)
                        {
                            state = "walk";
                            movementController.canSearch = false;
                        }
                    } else
                    {
                        state = "walk";
                    }
                }

                if (state == "walk")
                {
                    if ((transform.position - movementController.destination).magnitude < 1)
                    {
                        state = "idle";
                    }
                    waiting++;
                    if (waiting > 240)
                    {
                        waiting = 0;
                        movementController.destination = transform.position + new Vector3(Random.Range(-4, 4), 0, Random.Range(-4, 4));
                        movementController.canSearch = true;
                    }
                }

                //if state is idle
                if (state == "idle")
                {
                    movementController.canSearch = false;
                    Deccelerate(2);
                    waiting++;
                    if (waiting > 240)
                    {
                        state = "walk";
                    }
                }
                else
                {
                    if (attacking)
                    {
                        //stop and turn towards target
                        var direction = target.transform.position - transform.position;
                        direction.y = 0;
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), turnSpeed * Time.deltaTime);
                        Deccelerate(4);
                    } else
                    {
                        Accelerate(2);
                    }
                }
            }
            else if (alive)
            {
                Die();
            }
            else
            {
                if (fade)
                {
                    if (fadeDelay > 120)
                    {
                        for (var i = 0; i < rend.Length; i++)
                        {
                            Color color = rend[i].material.GetColor("_Color");
                            color.a -= 0.005f;
                            rend[i].material.SetColor("_Color", color);
                            if (color.a <= -0.5)
                            {
                                Destroy(gameObject);
                            }
                        }
                    }
                    fadeDelay++;
                }
            }
        }

        private void Deccelerate(float multiplier)
        {
            if (walkSpeed > 0)
            {
                walkSpeed -= acceleration * multiplier;
                if (walkSpeed < 0)
                {
                    walkSpeed = 0;
                }
            }
        }

        private void Accelerate(float multiplier)
        {
            if (walkSpeed < maxWalkSpeed)
            {
                walkSpeed += acceleration * multiplier;
                if (walkSpeed > maxWalkSpeed)
                {
                    walkSpeed = maxWalkSpeed;
                }
            }
        }

        private void Die()
        {
            alive = false;
            anim.SetBool("Dead", true);
            movementController.canMove = false;
        }

        public void Death()
        {
            fade = true;
            for (var i = 0; i < rend.Length; i++)
            {
                Material[] mats = rend[i].materials;
                mats[0] = transparentMaterials[i];
                mats[1] = transparentMaterials[i];
                rend[i].materials = mats;
            }
        }

        public void AttackEnd()
        {
            if (target != null)
            {
                target.health -= damage;
                attacking = false;
                anim.SetBool("Attacking", attacking);
            }
        }

        private float CalculateRating(int rating_type)
        {
            float score = 0;
            switch (rating_type)
            {
                //Calculate health score
                case 1:
                    score = health / max_health;
                    break;
                //Nearby Allies
                case 2:
                    score = CheckAllies();
                    break;
                //Nearby Enemies
                case 3:
                    score = CheckEnemies();
                    break;
            }
            return score;
        }

        private void ExecuteAction (int action) {
            int total = 0;
            Vector3 pos = new Vector3(0, 0, 0);
            target = null;
            switch (action)
            {
                case 0:
                    //idle
                    state = "idle";
                    break;
                case 1:
                    //attack
                    Debug.Log("Attack");
                    target = closest_enemy;
                    state = "attack";
                    if (closest_enemy != null)
                    {
                        movementController.destination = target.transform.position;
                        movementController.canSearch = true;
                    }
                    break;
                case 2:
                    // flee
                    Debug.Log("Flee");
                    state = "flee";
                    scripts = FindObjectsOfType<MonoBehaviour>().OfType<WWSeeker>();
                    
                    foreach (WWSeeker s in scripts)
                    {  
                        if (s.team != team && s.alive)
                        {
                            total++;
                            pos += s.transform.position;
                        }
                    }
                    movementController.destination = - pos / total;
                    movementController.canSearch = true;

                    break;
                case 3:
                    // regroup
                    // move to the closest team member
                    Debug.Log("Regroup");
                    state = "regroup";
                    scripts = FindObjectsOfType<MonoBehaviour>().OfType<WWSeeker>();

                    var dist = Mathf.Infinity;
                    var closest_ally = this;

                    foreach (WWSeeker s in scripts)
                    {
                        if (s.team == team && s.alive)
                        {
                            var dist1 = (transform.position - s.transform.position).magnitude;

                            //get closest enemy
                            if (dist1 < dist)
                            {
                                dist = dist1;
                                closest_ally = s;
                            }
                        }
                    }

                    if (closest_ally != this)
                    {
                        movementController.destination = closest_ally.transform.position;
                        movementController.canSearch = true;
                    }
                    break;
            }
        }

        public void UpdateActiveActionsContexts()
        {
            active_actions.Clear();
            active_contexts.Clear();
            for (var i = 0; i < selectionsB.Count; i++)
            {
                int selection = selectionsA[i];
                if (!active_actions.Contains(selection) && selection != 0)
                {
                    active_actions.Add(selection);
                }
                selection = selectionsB[i];
                if (!active_contexts.Contains(selection) && selection != 0)
                {
                    active_contexts.Add(selection);
                }
            }
            Debug.Log(active_actions.Count);
        }

        public bool CanSeeTarget()
        {
            sight.origin = new Vector3(transform.position.x,transform.position.y+1,transform.position.z);
            Vector3 enemy_body = new Vector3(target.transform.position.x, target.transform.position.y + 1, target.transform.position.z);
            sight.direction = enemy_body - sight.origin;
            RaycastHit rayHit;
            var distance_to_enemy = (enemy_body - sight.origin).magnitude;
            int layer1 = 8;
            int layer2 = 9;
            int layermask1 = 1 << layer1;
            int layermask2 = 1 << layer2;
            int finalmask  = layermask1 | layermask2;

            if (distance_to_enemy < vision)
            {
                Debug.DrawLine(sight.origin, enemy_body, Color.white);
                if (Physics.Raycast(sight, out rayHit, distance_to_enemy, finalmask))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            } else
            {
                return false;
            }
        }
        
        public float CheckEnemies()
        {
            dist = Mathf.Infinity;
            target = null;
            float rating = 0;
            var scripts = FindObjectsOfType<MonoBehaviour>().OfType<WWSeeker>();
            foreach (WWSeeker s in scripts)
            {
                if (s.team != team && s.alive)
                {

                    var dist1 = (transform.position - s.transform.position).magnitude;

                    //formula 0.1/x+0.1 (0-1)
                    rating += Mathf.Clamp(0.1f/dist1+0.1f,0,1);

                    //get closest enemy
                    if (dist1 < dist)
                    {
                        dist = dist1;
                        closest_enemy = s;
                    }
                }
            }
            return Mathf.Clamp(rating, 0, 1);
        }

        public float CheckAllies()
        {
            target = null;
            float rating = 0;
            var scripts = FindObjectsOfType<MonoBehaviour>().OfType<WWSeeker>();
            foreach (WWSeeker s in scripts)
            {
                if (s.team == team && s.alive)
                {

                    var dist1 = (transform.position - s.transform.position).magnitude;

                    //formula 0.1/x+0.1 (0-1)
                    rating += Mathf.Clamp(0.1f / dist1 + 0.1f, 0, 1);
                }
            }
            return Mathf.Clamp(rating, 0, 1);
        }

        public void StepUp()
        {
            // local coordinate rotation around the Y axis to the given angle
            // Quaternion rotation = Quaternion.AngleAxis(-20, Vector3.up);
            // add the desired distance to the direction
            //Vector3 addDistanceToDirection = rotation * transform.forward * 1;
            var isStep = false;

            var layer = 9;
            var layermask = 1 << layer;

            RaycastHit rayHit;

            sight.origin = new Vector3(transform.position.x, transform.position.y + 0.15f, transform.position.z);
            sight.direction = transform.forward;
            
            Debug.DrawLine(sight.origin, sight.origin + sight.direction * 1f, Color.white);
            if (Physics.Raycast(sight, out rayHit, 1.6f, layermask))
            {
                isStep = true;
            }

            sight.origin = new Vector3(transform.position.x, transform.position.y + 0.3f, transform.position.z);
            sight.direction = transform.forward;

            Debug.DrawLine(sight.origin, sight.origin + sight.direction * 1.6f, Color.white);
            if (Physics.Raycast(sight, out rayHit, 1.6f, layermask))
            {
                isStep = true;
            }

            if (isStep)
            {
                controller.velocity = new Vector3(transform.forward.x * 2f, 8f, transform.forward.z * 2f);
            }

        }

        /*public void OnPathComplete(Path p)
        {
            Debug.Log("A path was calculated. Did it fail with an error? " + p.error);
            if (!p.error)
            {
                path = p;
                // Reset the waypoint counter so that we start to move towards the first point in the path
                waypoint = 0;
            }
        }*/

        public void Place ()
        {
            placed = true;
        }
    }
}

/*if (placed)
{
    var layerMask = 1 << 9;

    var pos = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);

    RaycastHit hit;
    if (Physics.Raycast( pos, -transform.up, out hit, 1, layerMask))
    {
        isGrounded = true;
        velY = 0;
        transform.position = hit.point;
    }
    else
    {
        isGrounded = false;
        velY -= 0.005f;
        if (velY > 1)
        {
            Die();
        }
        transform.position = transform.position + new Vector3(0, velY, 0);
    }
}*/

/*if (placed)
{
    movementController.gravity = new Vector3(0, -9.8f, 0);
    movementController.constrainInsideGraph = true;
} else
{
    movementController.gravity = new Vector3(0, 0, 0);
    movementController.constrainInsideGraph = false;
}*/
