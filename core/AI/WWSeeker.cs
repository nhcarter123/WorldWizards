using UnityEngine;
using Pathfinding;

using WorldWizards.core.manager;
using System.Collections.Generic;
using Pathfinding.RVO;

namespace WorldWizards.core.entity.gameObject
{

    [System.Serializable]
    public class Curves
    {
        public enum Action //actions
        {
            Attack,
            Flee,
            Regroup
        };
        public enum Context //context
        {
            Health,
            Allies,
            Enemies
        };
        public Action action;
        public Context context;
        public AnimationCurve curve;
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
        public bool enemy = true;

        //Turn Rate
        [Range(1.0f, 10.0f)]
        public float turnSpeed = 5f;

        //Walk Speed
        [Range(1.0f, 10.0f)]
        public float maxWalkSpeed = 4f;

        //Attack Distance
        [Range(1.0f, 10.0f)]
        public float attackDistance = 1.5f;

        //Aggro Distance
        [Range(1.0f, 100.0f)]
        public float aggroDistance = 18;

        //Attack De-Aggro Distance
        [Range(1.0f, 100.0f)]
        public float deAggraDistance = 30;

        float health = 100;

        /*//curves
        public List<AnimationCurve> curves = new List<AnimationCurve>() {
            new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)),
            new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)),
            new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1))
        };*/

        public Curves[] mylist = new Curves[2];


        float walkSpeed = 0;
        float acceleration = 0.08f;

        Animator anim;
        Seeker seeker;
        SkinnedMeshRenderer[] rend;
        RVOController controller;
        FunnelModifier modifier;
        AIPath movementController;

        List<Material> transparentMaterials = new List<Material>();

        private void Start()
        {

            //get character components
            seeker = gameObject.GetComponent<Seeker>();
            anim = gameObject.GetComponent<Animator>();
            rend = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();

            //add a local avoidance RVO controller
            controller = GetComponent<RVOController>();
            //controller.radius = 0.65f;

            //add a smoothing modifier
            modifier = gameObject.AddComponent<FunnelModifier>();

            //add a movement controller
            movementController = gameObject.AddComponent<AIPath>();
            movementController.repathRate = 0.5f;
            movementController.maxSpeed = maxWalkSpeed;
            movementController.rotationSpeed = turnSpeed * 25;
            movementController.slowWhenNotFacingTarget = true;

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
                health -= 0.3f;
                //get targt location
                movementController.destination = Camera.main.transform.position;
                /*Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit raycastHit;
                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out raycastHit, Mathf.Infinity))
                {
                    Vector3 position = raycastHit.point;
                    movementController.destination = position;
                }*/

                //update mind on delay
                if (count > 30)
                {
                    count = 0;
                    //get nearby enemies


                    //get nearby allies

                }
                count++;

                //get distance to target
                var dist = (movementController.destination - transform.position).magnitude;
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
                anim.SetFloat("Forward", (movementController.velocity.magnitude + 1f)/2f);

                //if state is idle
                if (idle)
                {
                    Deccelerate();
                }
                else
                {
                    if (attacking)
                    {
                        movementController.canMove = false;
                    }
                    else
                    {
                        movementController.canMove = true;
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
            //Disable movement controller
            movementController.canMove = false;
            movementController.canSearch = false;
        }

        public void AttackEnd()
        {
            attacking = false;
            anim.SetBool("Attacking", attacking);
        }
    }


}