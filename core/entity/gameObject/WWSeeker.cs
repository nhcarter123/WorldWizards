using UnityEngine;
using Pathfinding;

using WorldWizards.core.manager;
using System.Collections.Generic;

namespace WorldWizards.core.entity.gameObject
{
    public class WWSeeker : Seeker
    {
        int count = 0;

        bool idle = true;
        bool attacking = false;

        int turnSpeed = 30;
        float maxWalkSpeed = 0.5f;
        float walkSpeed = 0;
        float acceleration = 0.025f;
        int runSpeed = 2;

        float attackDistance = 1;
        float aggroDistance = 8;
        float deAggraDistance = 16;

        Vector3 targetLocation = new Vector3(0,0,0);
        Animator anim;
        List<Vector3> pathPoints;

        public void Start()
        {
            //GameObject seeker_object = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //SphereCollider sc = gameObject.AddComponent<SphereCollider>() as SphereCollider;
            var seeker = gameObject.GetComponent<Seeker>();
            anim = gameObject.GetComponent<Animator>();
            // Start a new path request from the current position to a position 10 units forward.
            // When the path has been calculated, it will be returned to the function OnPathComplete unless it was canceled by another path request
            seeker.StartPath(transform.position, transform.position + transform.forward * 20, OnPathComplete);
            // Note that the path is NOT calculated at this stage
            // It has just been queued for calculation
        }

        public void StartPath()
        {
            var seeker = GetComponent<Seeker>();
            seeker.StartPath(transform.position, targetLocation, OnPathComplete);
        }

        public void Update()
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
                } else
                {
                    attacking = false;
                }
            } else if (dist > deAggraDistance)
            {
                idle = true;
            }

            //set animation
            anim.SetFloat("Forward", walkSpeed);

            //if state is idle
            if (idle)
            {
                Deccelerate();
            } else {
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

        public void OnPathComplete(Path p)
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

        public void Deccelerate()
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
    }


}