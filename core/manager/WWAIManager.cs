using Pathfinding;
using System.Collections.Generic;
using UnityEngine;
using WorldWizards.core.entity.gameObject;
using WorldWizards.core.controller.resources;

using WorldWizards.core.entity.coordinate;
using WorldWizards.core.entity.coordinate.utils;
using WorldWizards.core.entity.gameObject.utils;
using WorldWizards.core.entity.common;

namespace WorldWizards.core.manager
{
    /// <summary>
    ///     WWMenuManager holds references to all of the menus used in World Wizards.
    ///     Allows menus to be activated/deactivated from anywhere.
    ///     Allows you to get references to specific menus from anywhere.
    /// </summary>

    public class WWAIManager : Manager
    {
       
        public WWAIManager()
        {
            GameObject obj = new GameObject("AIScript");
            obj.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
            obj.AddComponent<AstarPath>();

            /*GridGraph gridGraph = AstarPath.active.data.gridGraph;
            PointGraph pointGraph = AstarPath.active.data.pointGraph;
            RecastGraph recastGraph = AstarPath.active.data.recastGraph;
            NavMeshGraph navmeshGraph = AstarPath.active.data.navmesh;
            LayerGridGraph layerGridGraph = AstarPath.active.data.layerGridGraph;
            NavGraph[] allGraphs = AstarPath.active.data.graphs;*/

            // This holds all graph data
            AstarData data = AstarPath.active.data;

            // This creates a Grid Graph
            LayerGridGraph gg = data.AddGraph(typeof(LayerGridGraph)) as LayerGridGraph;

            // Setup a grid graph with some values
            int width = 200;
            int depth = 200;
            float nodeSize = 0.5f;

            gg.center = new Vector3(0, 0, 0);

            // Updates internal size from the above values
            gg.SetDimensions(width, depth, nodeSize);
            //gg.collision.mask = LayerMask.GetMask("Water");

            // Scans all graphs
            gg.active.Scan();
        }
    }
}
