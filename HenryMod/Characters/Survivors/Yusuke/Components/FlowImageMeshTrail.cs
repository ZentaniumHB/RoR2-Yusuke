using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using YusukeMod.Survivors.Yusuke;
using IL.RoR2.Orbs;

namespace YusukeMod.Characters.Survivors.Yusuke.Components
{
    public class FlowImageMeshTrail : MonoBehaviour
    {
        private GameObject flowImagePrefab;
        private GameObject hairObject; 

        private EffectManagerHelper _emh_flowImagePrefab;
        private EffectManagerHelper _emh_HairPrefab;
        private FlowImageMesh hairFlowMesh;

        private bool isPoolObject;
        private bool isHairObject;

        public SkinnedMeshRenderer[] skinnedMeshRenderers;
        public SkinnedMeshRenderer skinnedMeshRenderer;

        private ModelLocator modelLocator;
        private Transform hairRefTransform;

        public float meshRate = 0.02f;
        private float timer;

        public void Awake()
        {
            modelLocator = gameObject.GetComponent<ModelLocator>();
            var model = GetComponent<ModelLocator>().modelTransform;
            skinnedMeshRenderers = model.GetComponentsInChildren<SkinnedMeshRenderer>();
        }

        public void Update()
        {
            timer += Time.deltaTime;

            if (timer >= meshRate)
            {
                if (skinnedMeshRenderers.Length == 2) 
                {
                    SpawnTrail();
                }
                else
                {
                    Log.Warning("There is only (" + skinnedMeshRenderers.Length + ") found");
                }
                timer = 0f;

            }
        }

        public void SpawnTrail()
        {

            if (isPoolObject)
            {
                Log.Info("Pool exists");
                flowImagePrefab = _emh_flowImagePrefab.gameObject;
            }
            else
            {
                Log.Info("Pool does not exists");
            }

            if (hairObject)
            {
                hairObject = _emh_HairPrefab.gameObject;
            }

            // Matching and baking the skinnedmeshrenderer based on the animation its at (looks for the skinmesh and the face mesh).
            foreach (SkinnedMeshRenderer skinMesh in skinnedMeshRenderers)
            {
                string objectName = skinMesh.gameObject.name;

                FlowImageMesh flowImageMesh = flowImagePrefab.GetComponent<FlowImageMesh>();
                if (flowImageMesh)
                {
                    flowImageMesh.meshFilter = flowImagePrefab.GetComponent<MeshFilter>();

                    if (flowImageMesh.meshFilter)
                    {
                        Log.Info("Adding skin mesh to mesh filter");
                        skinMesh.BakeMesh(flowImageMesh.meshFilter.mesh, true);
                        flowImageMesh.transform.position = skinMesh.transform.position;
                        flowImageMesh.transform.rotation = skinMesh.transform.rotation;
                        flowImageMesh.transform.localScale = skinMesh.transform.lossyScale;
                    }
                    else
                    {
                        Log.Info("Mesh filter does not exist");
                    }

                    // Sets the effect manager after so it returns after a while
                    flowImageMesh.SetEMH(_emh_flowImagePrefab);



                }
                else
                {
                    Log.Info("mesh filter cannnot be found");
                }

                // If the object does not have the flowImageMesh, then add it. Needed for the effectmanager return.
                if (hairObject)
                {
                    hairObject.transform.position = hairRefTransform.position;
                    hairObject.transform.rotation = hairRefTransform.rotation;
                    hairObject.transform.localScale = hairRefTransform.lossyScale;

                }

                if (_emh_HairPrefab)
                {
                    _emh_HairPrefab.gameObject.transform.position = hairRefTransform.position;
                    _emh_HairPrefab.gameObject.transform.rotation = hairRefTransform.rotation;
                    _emh_HairPrefab.gameObject.transform.localScale = hairRefTransform.lossyScale;

                    hairFlowMesh.SetEMH(_emh_HairPrefab);
                }
            }



        }

        // grabbing the prefab from the location necessary
        public void SetFlowImagePrefab(GameObject prefab)
        {
            flowImagePrefab = prefab;
            isPoolObject = false;
        }

        public void SetFlowImageEMH(EffectManagerHelper EMH)
        {
            _emh_flowImagePrefab = EMH;
            isPoolObject = true;

        }

        // Will grab the prefab for the hair too, but also sets the mesh materials required
        public void SetHairImagePrefab(GameObject prefab, Transform hairTransform)
        {
            hairObject = prefab;
            hairObject.transform.SetParent(null);
            hairRefTransform = hairTransform;

            // changeing the material for the hair object
            MeshRenderer meshRenderer = hairObject.gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer) meshRenderer.material = YusukeAssets.flowMaterial;

            if(!hairObject.GetComponent<FlowImageMesh>()) hairObject.AddComponent<FlowImageMesh>();
            
            isHairObject = false;
        }

        public void SetHairImageEMH(EffectManagerHelper EMH, Transform hairTransform)
        {
            _emh_HairPrefab = EMH;
            _emh_HairPrefab.gameObject.transform.SetParent(null);
            hairRefTransform = hairTransform;

            // changeing the material for the hair object
            MeshRenderer meshRenderer = _emh_HairPrefab.gameObject.gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer) meshRenderer.material = YusukeAssets.flowMaterial;

            // If the object does not have the flowImageMesh, then add it. Needed for the effectmanager return.
            hairFlowMesh = _emh_HairPrefab.gameObject.GetComponent<FlowImageMesh>();
            if (!hairFlowMesh) hairFlowMesh = _emh_HairPrefab.gameObject.AddComponent<FlowImageMesh>();

            isHairObject = true;

        }

        public EffectManagerHelper GetEMHObject()
        {
            return _emh_flowImagePrefab;
        }
    }
}
