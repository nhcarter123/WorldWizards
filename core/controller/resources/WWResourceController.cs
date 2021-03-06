using System.Collections.Generic;
using UnityEngine;
using WorldWizards.core.entity.common;
using WorldWizards.core.entity.gameObject.resource;

namespace WorldWizards.core.controller.resources
{
    public static class WWResourceController
    {
        public static Dictionary<string, WWResource> bundles = new Dictionary<string, WWResource>();

        /// <summary>
        ///     Gets the resource keys by asset bundle tag.
        /// </summary>
        /// <returns>The resource keys by asset bundle.</returns>
        /// <param name="assetBundleTag">Asset bundle tag.</param>
        public static List<string> GetResourceKeysByAssetBundle(string assetBundleTag, string characterBundleTag)
        {
            var filteredKeys = new List<string>();
            foreach (KeyValuePair<string, WWResource> kvp in bundles)
                if (kvp.Value.assetBundleTag.Equals(assetBundleTag) || kvp.Value.assetBundleTag.Equals(characterBundleTag))
                {
                    filteredKeys.Add(kvp.Key);
                }
            return filteredKeys;
        }

        /// <summary>
        ///     Gets the resource keys by asset bundle tag and filter by WWType
        /// </summary>
        /// <param name="assetBundleTag"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<string> GetResourceKeysByAssetBundleFiltered(string assetBundleTag, string characterBundleTag, WWType type)
        {
            var filteredKeys = new List<string>();
            foreach (KeyValuePair<string, WWResource> kvp in bundles)
                if (kvp.Value.assetBundleTag.Equals(assetBundleTag) || kvp.Value.assetBundleTag.Equals(characterBundleTag))
                {
                    if (kvp.Value.GetMetaData().wwObjectMetadata.type.Equals(type))
                    {
                        filteredKeys.Add(kvp.Key);
                    }
                }
            return filteredKeys;
        }

        public static void LoadResource(string tag, string assetBundleTag, string path)
        {
            if (bundles.ContainsKey(tag))
            {
                Debug.Log("resourceTag: " + tag + " has already been used.");
            }
            else
            {
                var resource = new WWResource(assetBundleTag, path);
                bundles.Add(tag, resource);
            }
        }

        public static WWResource GetResource(string tag)
        {
            WWResource resource;
            if (bundles.TryGetValue(tag, out resource))
            {
                return resource;
            }
            Debug.Log("A resource with the tag: " + tag + " has not been loaded.");
            return new WWResource(null, null);
        }
    }
}