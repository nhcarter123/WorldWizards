using UnityEngine;
using Pathfinding;

using WorldWizards.core.manager;
using System.Collections.Generic;

namespace WorldWizards.core.entity.gameObject
{
    public class WWSeeker : Seeker
    {
        int count = 0;

        //states
        bool idle = true;
        bool attacking = false;
        bool alive = true;
        bool fade = false;

        //movement stats
        int turnSpeed = 30;
        float maxWalkSpeed = 1.8f;
        float walkSpeed = 0;
        float acceleration = 0.08f;
        int runSpeed = 4;

        float attackDistance = 4.5f;
        float aggroDistance = 14;
        float deAggraDistance = 20;

        float health = 100;

        Vector3 targetLocation = new Vector3(0,0,0);
        Animator anim;
        List<Vector3> pathPoints;
        Seeker seeker;
        SkinnedMeshRenderer[] rend;

        Material[] transparentMaterials;

        private void Start()
        {
            //GameObject seeker_object = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //SphereCollider sc = gameObject.AddComponent<SphereCollider>() as SphereCollider;
            seeker = gameObject.GetComponent<Seeker>();
            anim = gameObject.GetComponent<Animator>();
            rend = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            for (var i = 0; i < rend.Length; i++)
            {
                transparentMaterials[i] = rend[i].materials[0];
                rend[i].material = rend[i].materials[1];
            }
            // Start a new path request from the current position to a position 10 units forward.
            // When the path has been calculated, it will be returned to the function OnPathComplete unless it was canceled by another path request
            //seeker.StartPath(transform.position, transform.position + transform.forward * 20, OnPathComplete);
            // Note that the path is NOT calculated at this stage
            // It has just been queued for calculation
        }

        private void StartPath()
        {
            seeker.StartPath(transform.position, targetLocation, OnPathComplete);
        }

        private void Update()
        {
            if (health > 0)
            {
                //get targt location
                targetLocation = Camera.main.transform.position;

                //get distance to target
                var dist = (targetLocation - transform.position).magnitude;
                Debug.Log(dist);

                //aggro or deaggro based on distance
                if (dist < aggroDistance)
                {
                    idle = false;
                    if (dist < attackDistance)
                    {
                        attacking = true;
                        anim.SetBool("Attacking", attacking);

                    }
                }
                else if (dist > deAggraDistance)
                {
                    idle = true;
                }

                //set animation
                anim.SetFloat("Forward", (walkSpeed + 1f)/2f);

                //if state is idle
                if (idle)
                {
                    Deccelerate();
                }
                else
                {
                    //rotate towards the target in one dimention
                    Vector3 targetLocationVector = targetLocation;
                    Vector3 positionVector = transform.position;
                    targetLocationVector.y = 0;
                    positionVector.y = 0;
                    var targetDir = Quaternion.LookRotation(targetLocationVector - positionVector);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetDir, Time.deltaTime * turnSpeed);

                    if (attacking)
                    {
                        Deccelerate();
                    }
                    else
                    {

                        //update path every three seconds
                        count++;
                        if (count > 180)
                        {
                            count = 0;


                            StartPath();
                        }

                        //change walkspeed based on direction
                        if (walkSpeed < maxWalkSpeed)
                        {
                            var angleDiff = Quaternion.LookRotation(targetLocation - transform.position);
                            Debug.Log(angleDiff);
                            walkSpeed += acceleration * 1;
                        }

                        //move along the path
                        transform.position = Vector3.MoveTowards(transform.position, pathPoints[pathPoints.Count - 1], walkSpeed * Time.deltaTime);
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
                        color.a -= 0.003f;
                        rend[i].material.SetColor("_Color", color);
                        if (color.a <= 0)
                        {
                            Destroy(gameObject);
                        }
                    }
                }
            }
        }

        private void OnPathComplete(Path p)
        {
            // We got our path back
            if (p.error)
            {
                // Nooo, a valid path couldn't be found
            }
            else
            {
                pathPoints = p.vectorPath;
                //transform.position = p.vectorPath.FindLast;
                Debug.Log(p.vectorPath);
                // Yay, now we can get a Vector3 representation of the path
                // from p.vectorPath
            }
        }

        private void Deccelerate()
        {
            if (walkSpeed > 0)
            {
                walkSpeed -= acceleration;
                if (walkSpeed < 0)
                {
                    walkSpeed = 0;
                }
            }
        }

        private void Die()
        {
            alive = false;
            anim.SetBool("Dead", true);
            anim.Play("Death");
            for (var i = 0; i < rend.Length; i++)
            {
                rend[i].material = transparentMaterials[i];
            }
        }

        public void Death()
        {
            fade = true;
        }

        public void AttackEnd()
        {
            attacking = false;
            anim.SetBool("Attacking", attacking);
        }
    }


}