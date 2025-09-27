using System.Collections.Generic;
using UnityEngine;

namespace FBX
{
    public enum FBXBlendShapePreset
    {
        Unknown,
        Neutral,
        Joy,
        Angry,
        Sad,
        Blink,
        Blink_L,
        Blink_R,
        A, E, I, O, U
    }

    [System.Serializable]
    public class BlendShapeBinding
    {
        public string name;
        [Range(0f, 1f)]
        public float multiplier = 1f;
    }

    [System.Serializable]
    public class FBXBlendShapeMapping
    {
        public FBXBlendShapePreset preset;
        public BlendShapeBinding[] blendshapes;
    }

    public class FBXBlendShapeProxy : MonoBehaviour
    {
        public SkinnedMeshRenderer skinnedMesh;
        public FBXBlendShapeMapping[] mappings;

        private Dictionary<FBXBlendShapePreset, BlendShapeBinding[]> presetMap = new();
        private Dictionary<string, int> blendShapeIndexMap = new();

        void Awake()
        {
            if (skinnedMesh == null)
                skinnedMesh = GetComponentInChildren<SkinnedMeshRenderer>();

            var mesh = skinnedMesh.sharedMesh;
            blendShapeIndexMap.Clear();

            for (int i = 0; i < mesh.blendShapeCount; i++)
            {
                string name = mesh.GetBlendShapeName(i);
                blendShapeIndexMap[name] = i;
            }

            presetMap.Clear();
            foreach (var mapping in mappings)
                presetMap[mapping.preset] = mapping.blendshapes;
        }

        public void ImmediatelySetValue(FBXBlendShapePreset preset, float weight)
        {
            if (!presetMap.ContainsKey(preset)) return;

            foreach (var binding in presetMap[preset])
            {
                if (blendShapeIndexMap.TryGetValue(binding.name, out int index))
                {
                    float finalWeight = Mathf.Clamp01(weight) * binding.multiplier * 100f;
                    skinnedMesh.SetBlendShapeWeight(index, finalWeight);
                }
            }
        }

        public void ImmediatelySetValue(string customName, float weight)
        {
            if (blendShapeIndexMap.TryGetValue(customName, out int index))
                skinnedMesh.SetBlendShapeWeight(index, Mathf.Clamp01(weight) * 100f);
        }

        public void Apply()
        {
            // VRM butuh Apply(), FBX tidak. 
            // Dibuat kosong untuk kompatibilitas.
        }
    }
}
