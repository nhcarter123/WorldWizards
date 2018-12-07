using UnityEngine;
using Pathfinding;
using Pathfinding.RVO;

namespace WorldWizards.core.manager
{
    /// <summary>
    ///     WWMenuManager holds references to all of the menus used in World Wizards.
    ///     Allows menus to be activated/deactivated from anywhere.
    ///     Allows you to get references to specific menus from anywhere.
    /// </summary>

    public class WWAIManager : Manager
    {

        LayerGridGraph LGG;

        public WWAIManager()
        {
            GameObject obj = new GameObject("AIScript");
            obj.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
            obj.AddComponent<AstarPath>();
            
            //RVOSimulator rsim = obj.AddComponent<RVOSimulator>();

            ProceduralGridMover gridMover = obj.AddComponent<ProceduralGridMover>();
            gridMover.target = Camera.main.transform;
            gridMover.updateDistance = 5;
            gridMover.floodFill = true;

            // This holds all graph data
            AstarData data = AstarPath.active.data;

            // This creates a Grid Graph
            LGG = data.AddGraph(typeof(LayerGridGraph)) as LayerGridGraph;

            // Setup a grid graph with some values
            int width = 80;
            int depth = 80;
            float nodeSize = 0.5f;
            LGG.characterHeight = 2;
            LGG.maxClimb = 1.5f;

            LGG.collision.collisionOffset = 1;
            LGG.collision.diameter = 4;
            LGG.collision.type = 0;

            LGG.center = new Vector3(0,0,0);

            // Updates internal size from the above values
            LGG.SetDimensions(width, depth, nodeSize);

            LGG.collision.heightMask = LayerMask.GetMask("Terrain");
            LGG.collision.mask = LayerMask.GetMask("Obstacles");

            //set erode iterations to givebetter edges for obstacles
            LGG.erodeIterations = 0;

            // Scans graph
            LGG.Scan();
        }

        public void RefreshGrid ()
        {
            LGG.Scan();
        }
    }
}
