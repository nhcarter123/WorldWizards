using UnityEngine;
using Pathfinding;

using WorldWizards.core.manager;
using System.Collections.Generic;
using Pathfinding.RVO;
using System.Linq;

namespace WorldWizards.core.entity.gameObject
{
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
        public int health = 100;

        float walkSpeed = 0;
        float acceleration = 0.04f;
        float dist = 100000f;
        int waypoint = 0;
        Vector3 targetLocation;

        Animator anim;
        Seeker seeker;
        WWSeeker target = null;
        SkinnedMeshRenderer[] rend;
        CharacterController controller;
        //RVOController controller;
        FunnelModifier modifier;
        //AIPath movementController;

        List<Material> transparentMaterials = new List<Material>();

        private void Start()
        {

            //get character components
            seeker = gameObject.GetComponent<Seeker>();
            controller = gameObject.GetComponent<CharacterController>();
            anim = gameObject.GetComponent<Animator>();
            rend = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();

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
                            }
                        }
                    }
                    seeker.StartPath(transform.position, target.transform.position, OnPathComplete);
                    //get nearby allies

                }
                count++;
                Debug.Log(dist);

                if (dist < aggroDistance)
                {
                    idle = false;
                    if (dist < attackDistance)
                    {
                        attacking = true;
                        anim.SetBool("Attacking", attacking);
                    }
                }
                else if (dist > deAggroDistance)
                {
                    idle = true;
                }

                //set animation
                anim.SetFloat("Forward", controller.velocity.magnitude);

                //if state is idle
                if (idle)
                {
                    Deccelerate(1);
                    anim.SetFloat("Forward", 0);
                }
                else
                {
                    if (target != null)
                    {
                        Accelerate(1);

                        if (path != null)
                        {
                            targetLocation = path.vectorPath[waypoint];
                            if ((targetLocation - transform.position).magnitude < 2 && waypoint < path.vectorPath.Count)
                            {
                                waypoint++;
                            }
                        }
                    }

                    Vector3 direction = targetLocation - transform.position;
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), turnSpeed * Time.deltaTime);
                    transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

                    var dir = (targetLocation - transform.position).normalized;

                    if (!controller.isGrounded)
                    {
                        dir.y -= 9.8f;
                    }

                    controller.Move(dir * walkSpeed * Time.deltaTime);  


                    if (attacking)
                    {
                        Deccelerate(2);
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

        public void OnPathComplete(Path p)
        {
            Debug.Log("A path was calculated. Did it fail with an error? " + p.error);
            if (!p.error)
            {
                path = p;
                // Reset the waypoint counter so that we start to move towards the first point in the path
                waypoint = 0;
            }
        }
    }
}