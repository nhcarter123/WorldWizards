﻿using UnityEngine;
using WorldWizards.core.controller.level;
using WorldWizards.core.controller.level.utils;
using WorldWizards.core.entity.coordinate;
using WorldWizards.core.entity.gameObject;
using WorldWizards.core.manager;

namespace WorldWizards.core.experimental
{
    internal class BuildFromTestBundle : MonoBehaviour
    {
        private void Start()
        {
            ResourceLoader.LoadResources();

            for (var i = 0; i < 5; i++)
            {
                WWObjectData objData = WWObjectFactory.CreateNew(new Coordinate(i, i, i), "ww_basic_assets_Tile_Grass");
                WWObject go = WWObjectFactory.Instantiate(objData);
                ManagerRegistry.Instance.sceneGraphManager.Add(go);
            }

            for (var i = 0; i < 5; i++)
            {
                WWObjectData objData =
                    WWObjectFactory.CreateNew(new Coordinate(i, i + 1, i), "ww_basic_assets_Tile_Arch");
                WWObject go = WWObjectFactory.Instantiate(objData);
                ManagerRegistry.Instance.sceneGraphManager.Add(go);
            }

            for (var i = 0; i < 5; i++)
            {
                WWObjectData objData =
                    WWObjectFactory.CreateNew(new Coordinate(i, i + 2, i), "ww_basic_assets_Tile_FloorBrick");
                WWObject go = WWObjectFactory.Instantiate(objData);
                ManagerRegistry.Instance.sceneGraphManager.Add(go);
            }

            for (var i = 0; i < 5; i++)
            {
                WWObjectData objData =
                    WWObjectFactory.CreateNew(new Coordinate(i, i + 2, i), "ww_basic_assets_blueCube");
                WWObject go = WWObjectFactory.Instantiate(objData);
                ManagerRegistry.Instance.sceneGraphManager.Add(go);
            }
        }
    }
}