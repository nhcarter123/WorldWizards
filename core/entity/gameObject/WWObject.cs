﻿using System;
using System.Collections.Generic;
using UnityEngine;
using WorldWizards.core.controller.builder;
using WorldWizards.core.entity.common;
using WorldWizards.core.entity.coordinate;
using WorldWizards.core.entity.coordinate.utils;
using WorldWizards.core.entity.gameObject.resource.metaData;
using WorldWizards.core.entity.gameObject.utils;

namespace WorldWizards.core.entity.gameObject
{
    /// <summary>
    ///     The WorldWizardsObject is base class for all World Wizards objects.
    ///     WorldWizardsObject extends MonoBehavior so it has to be attached to a GameObject.
    /// </summary>
    public abstract class WWObject : MonoBehaviour
    {
        public WWResourceMetaData resourceMetaData { get; private set; }

        public WWObjectData objectData { get; private set; }

        public TileFader tileFader { get; private set; }

        public virtual void Init(Guid id, Coordinate coordinate,
            WWObjectData parent, List<WWObjectData> children, string resourceTag)
        {
            objectData = new WWObjectData(id, coordinate, parent, children, resourceTag);
        }

        public virtual void Init(WWObjectData objectData, WWResourceMetaData resourceMetaData)
        {
            this.objectData = objectData;
            this.resourceMetaData = resourceMetaData;
            tileFader = new TileFader(gameObject);
        }

        public Guid GetId()
        {
            return objectData.id;
        }

        public Coordinate GetCoordinate()
        {
            if (resourceMetaData.wwObjectMetaData.type == WWType.Tile)
            {
                // we only want the index without the offset for Tiles
                return new Coordinate(objectData.coordinate.Index, objectData.coordinate.Rotation);
            }
            return objectData.coordinate;
        }

        public WWWalls GetWallsWRotationApplied()
        {
            return WWWallsHelper.GetRotatedWWWalls(resourceMetaData, GetCoordinate().Rotation);
        }

        public WWObject GetOldestParent()
        {
            return null;
        }

        /// <summary>
        ///     Promote this World Wizard Object to not have a parent.
        /// </summary>
        public void Unparent()
        {
            objectData.Unparent();
        }


        public void Parent(WWObject parent)
        {
            objectData.Parent(parent.objectData);
        }


        public void RemoveChild(WWObject child)
        {
            var childrenToRemove = new List<WWObject>();
            childrenToRemove.Add(child);
            RemoveChildren(childrenToRemove);
        }

        public void RemoveChildren(List<WWObject> children)
        {
            foreach (WWObject child in children)
                if (objectData.children.Contains(child.objectData))
                {
                    objectData.RemoveChild(child.objectData);
                }
        }

        public void AddChildren(List<WWObject> children)
        {
            objectData.AddChildren(children);
            foreach (WWObject child in children) child.Parent(this);
        }

        public List<WWObject> GetChildren()
        {
            return null; // children;
        }

        public WWObjectData GetParent()
        {
            return objectData.parent; // parent;
        }

        public List<WWObjectData> GetAllDescendents()
        {
            return objectData.GetAllDescendents();
        }

        public void SetRotation(int yRotation)
        {
            transform.rotation = Quaternion.Euler(0, yRotation, 0);
            objectData.coordinate.Rotation = yRotation;
        }

        public virtual void SetPosition(Vector3 position, bool snapToGrid)
        {
            transform.position = position + GetPositionOffset();
            objectData.coordinate = CoordinateHelper.UnityCoordToWWCoord(position, objectData.coordinate.Rotation);
        }

        public void SetPosition(Coordinate coordinate)
        {
            SetPosition(CoordinateHelper.WWCoordToUnityCoord(coordinate), false);
            SetRotation(coordinate.Rotation);
        }

        protected abstract Vector3 GetPositionOffset();
    }
}