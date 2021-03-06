using System.Collections.Generic;
using UnityEngine;
using WorldWizards.core.controller.resources;
using WorldWizards.core.entity.common;

namespace WorldWizards.core.manager
{
    /// <summary>
    ///     Controls what is in the object gun when the player is placing new objects in the scene
    ///     Allows for filters to be added based on WWType and/or asset bundle
    ///     AssetBundleMenu sets the values
    ///     Default: All objects in "ww_basic_assets"
    /// </summary>
    
    public class WWObjectGunManager : Manager
    {
        private bool doFilter;
        private WWType filterType = WWType.None;
        private static string characterBundleTag = "characters";
        private static string assetBundleTag = "medeivalvillage";
        
        // The list of objects for the object gun
        // Default is all the objects in ww_basic_assets
        private List<string> possibleObjects = WWResourceController.GetResourceKeysByAssetBundle(assetBundleTag, characterBundleTag);
        /// <summary>
        ///     Gets whether the object gun should be filtered
        /// </summary>
        /// <returns>True if filtering, false if not filtering</returns>
        public bool GetDoFilter()
        {
            return doFilter;
        }

        /// <summary>
        ///     Get the type of object we are filtering for
        /// </summary>
        /// <returns>The type of object we are filtering for</returns>
        public WWType GetFilterType()
        {
            return filterType;
        }
        
        /// <summary>
        ///     Returns the list of possible objects for object gun
        /// </summary>
        /// <returns>List of all objects that should be in object gun</returns>
        public List<string> GetPossibleObjectKeys()
        {
            return possibleObjects;
        }

        /// <summary>
        ///     Gets the list of possible objects for the object gun based on 
        /// </summary>
        /// <param name="doFilter"></param>
        /// <param name="filterType"></param>
        public void SetPossibleObjectKeys(bool doFilter, WWType filterType)
        {
            if (doFilter)
            {
                possibleObjects = WWResourceController.GetResourceKeysByAssetBundleFiltered(assetBundleTag, characterBundleTag, filterType);
            }
            else
            {
                possibleObjects = WWResourceController.GetResourceKeysByAssetBundle(assetBundleTag, characterBundleTag);
            }

            this.doFilter = doFilter;
            this.filterType = filterType;
            
            Debug.Log("doFilter: " + doFilter + ", filterType: " + filterType + ", currentAssetBundle: " + assetBundleTag);
        }

        public void SetPossibleObjectKeys(string assetBundleTag)
        {
            if (doFilter)
            {
                possibleObjects = WWResourceController.GetResourceKeysByAssetBundleFiltered(assetBundleTag, characterBundleTag, filterType);
            }
            else
            {
                possibleObjects = WWResourceController.GetResourceKeysByAssetBundle(assetBundleTag, characterBundleTag);
            }

            WWObjectGunManager.assetBundleTag = assetBundleTag;
            
            Debug.Log("doFilter: " + doFilter + ", filterType: " + filterType + ", currentAssetBundle: " + assetBundleTag);
        }
    }
}