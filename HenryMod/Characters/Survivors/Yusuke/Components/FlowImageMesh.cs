using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using YusukeMod.Survivors.Yusuke;

namespace YusukeMod.Characters.Survivors.Yusuke.Components
{
    public class FlowImageMesh : MonoBehaviour
    {
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;

        public Vector3 direction;
        public float speed = 1f;
        public float lifetime = 0.4f;
        //public AnimationCurve fadeCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        public AnimationCurve speedCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
        private float stopwach;
        private EffectManagerHelper effectManagerHelper;

        void Awake()
        {
            effectManagerHelper = gameObject.GetComponent<EffectManagerHelper>();
        }


        public void Start()
        {
            if (!effectManagerHelper) effectManagerHelper = gameObject.GetComponent<EffectManagerHelper>();
            if (!meshFilter.mesh)
            {
                //meshFilter.mesh = new Mesh();
            }

        }
        public void Update()
        {
            transform.position += direction * speed * speedCurve.Evaluate(stopwach / lifetime) * Time.deltaTime;
            stopwach += Time.deltaTime;
            if (stopwach >= lifetime)
            {
                stopwach = 0f;
                //effectManagerHelper = GetComponent<FlowImageMeshTrail>().GetEMHObject();
                if (effectManagerHelper && effectManagerHelper.OwningPool != null)
                {
                    Log.Info("Returning pool to the manager: " + effectManagerHelper.gameObject.name);
                    effectManagerHelper.OwningPool.ReturnObject(effectManagerHelper);
                    return;
                }
                else
                {
                    Log.Info("Not pooled, destroying: "+gameObject.name);
                    Destroy(gameObject);
                }
            }

        }

        public void OnDestroy()
        {
            if (meshFilter && meshFilter.mesh) Destroy(meshFilter.mesh);
        }

        public void SetEMH(EffectManagerHelper emh)
        {
            effectManagerHelper = emh;
        }


    }
}
