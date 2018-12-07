using UnityEngine;
using Pathfinding;

using WorldWizards.core.manager;
using System.Collections.Generic;
using Pathfinding.RVO;
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
        bool idle = true;
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

        float vision = 100;
        float walkSpeed = 0;
        float acceleration = 0.03f;
        float dist = 100000f;
        int waypoint = 0;
        int waiting = 0;
        Vector3 targetLocation;
        Ray sight;
        bool placed = false;

        Animator anim;
        Seeker seeker;
        WWSeeker target = null;
        SkinnedMeshRenderer[] rend;
        CharacterController controller;
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

        //environmental rating from 0.0-1.0
        float nearby_enemies_rating;
        float nearby_allies_rating;
        float health_rating;

        float attack_rating;
        float flee_rating;
        float regroup_rating;

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
            controller = gameObject.GetComponent<CharacterController>();
            movementController = gameObject.AddComponent<AIPath>();
            anim = gameObject.GetComponent<Animator>();
            rend = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();

            movementController.slowdownDistance = 1;
            movementController.pickNextWaypointDist = 1f;
            movementController.endReachedDistance = 1f;
            movementController.slowWhenNotFacingTarget = true;
            movementController.repathRate = 1;

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

                //update mind on delay
                if (count > 30)
                {
                    count = 0;

                    //get nearby enemies
                    dist = 100000f;
                    target = null;
                    var scripts = FindObjectsOfType<MonoBehaviour>().OfType<WWSeeker>();
                    foreach (WWSeeker s in scripts)
                    {
                        if (s.team != team && s.alive)
                        {
                            var dist1 = (transform.position - s.transform.position).magnitude;
                            if (dist1 < dist)
                            {
                                dist = dist1;
                                target = s;
                                movementController.destination = target.transform.position;
                            }
                        }
                    }
                    if (target == null)
                    {
                        waiting++;
                        if (waiting > 10)
                        {
                            waiting = 0;
                            movementController.destination = transform.position + new Vector3(Random.Range(-4,4),0,Random.Range(-4,4));
                            idle = false;
                        }
                    }
                    //get nearby allies

                    //iterate through curves

                    for (var i = 0; i < active_actions.Count; i++)
                    {
                        action_ratings[active_actions[i]] = 0;
                    }

                    for (var i = 0; i < active_contexts.Count; i++)
                    {
                        context_ratings[active_contexts[i]] = CalculateRating(active_contexts[i]);
                    }

                    for (var i = 0; i < curves.Count; i++)
                    {
                        action_ratings[selectionsA[i]] += curves[i].Evaluate(context_ratings[selectionsB[i]]) + 1001;
                    }

                    //average ratings with additive 0.1 * n
                    for (var i = 0; i < action_ratings.Count; i++)
                    {
                        float rating = action_ratings[i];
                        float n = Mathf.Round(rating / 1000);
                        if (n > 0)
                        {
                            rating = (rating - (1001 * n)) / n;
                            action_ratings[i] = rating + (rating * 0.1f * (n-1));
                        }
                        //Debug.Log(action_ratings[i]);
                    }             

                }
                count++;

                idle = false;
                movementController.canSearch = true;
                attacking = false;
                anim.SetBool("Attacking", attacking);

                if (target != null)
                {
                    if (dist < aggroDistance)
                    {
                        if (dist < attackDistance)
                        {
                            idle = true;
                            if (target != null && target.health > 0 && CanSeeTarget())
                            {
                                idle = false;
                                attacking = true;
                                anim.SetBool("Attacking", attacking);
                                movementController.canSearch = false;
                            }
                        }
                    }
                    else if (dist > deAggroDistance)
                    {
                        idle = true;
                        movementController.canSearch = false;
                    }
                }

                //set animation

                var xyVel = new Vector3(movementController.velocity.x, 0, controller.velocity.z);
                anim.SetFloat("Forward", xyVel.magnitude);
                movementController.maxSpeed = walkSpeed;

                //if state is idle
                if (idle)
                {
                    Deccelerate(2);
                }
                else
                {
                    if (target != null)
                    {
                        if (path != null)
                        {
                            if (waypoint < path.vectorPath.Count-1)
                            {
                                targetLocation = path.vectorPath[waypoint];
                                if ((targetLocation - transform.position).magnitude < 0.1f)
                                {
                                    waypoint++;
                                }
                                /*if ((transform.position - path.vectorPath[waypoint]).sqrMagnitude < 1 * 1)
                                {
                                    targetLocation = path.vectorPath[waypoint];
                                    waypoint++;
                                }*/
                            }
                        }
                    }

                    if (dist < 1)
                    {
                        Deccelerate(1);
                    } else
                    {
                        Accelerate(1);
                    }

                    //var dir = (targetLocation - transform.position).normalized;

                    /*if (!controller.isGrounded)
                    {
                        dir.y -= 9.8f;
                    }*/

                    //controller.Move(dir * walkSpeed * Time.deltaTime);
                    //controller.SetTarget(targetLocation, walkSpeed, maxWalkSpeed);
                    //var delta = controller.CalculateMovementDelta(transform.position, Time.deltaTime);

                    //transform.position = transform.position + delta;


                    if (attacking)
                    {
                        var direction = target.transform.position - transform.position;
                        direction.y = 0;
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), turnSpeed * Time.deltaTime);
                        Deccelerate(1);
                    }
                   
                    

                    //Deccelerate(2);
                    //}
                    //{
                    //   movementController.attacking = false;
                    //}
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
                    for (var i = 0; i < rend.Length; i++)
                    {
                        Color color = rend[i].material.GetColor("_Color");
                        color.a -= 0.005f;
                        rend[i].material.SetColor("_Color", color);
                        if (color.a <= 0)
                        {
                            Destroy(gameObject);
                        }
                    }
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
            target.health -= 20;
            attacking = false;
            anim.SetBool("Attacking", attacking);
        }

        private float CalculateRating (int rating_type)
        {
            float score = -1;
            switch (rating_type)
            {
                //Calculate health score
                case 1:
                    score = health / max_health;
                    break;
                //Nearby Allies
                case 2:
                    score = 0.5f;

                    break;
                //Nearby Enemies
                case 3:
                    score = 1;
                    break;
            }
            return score;
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
