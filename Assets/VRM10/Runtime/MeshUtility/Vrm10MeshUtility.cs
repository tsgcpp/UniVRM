using System;
using System.Collections.Generic;
using System.Linq;
using UniHumanoid;
using UnityEngine;


namespace UniVRM10
{
    public class Vrm10MeshUtility : UniGLTF.MeshUtility.GltfMeshUtility
    {
        bool _generateFirstPerson = false;
        protected override IEnumerable<UniGLTF.MeshUtility.MeshIntegrationGroup> CopyInstantiate(GameObject go, GameObject instance)
        {
            var copy = new List<UniGLTF.MeshUtility.MeshIntegrationGroup>();
            _generateFirstPerson = false;
            if (GenerateMeshForFirstPersonAuto)
            {
                foreach (var g in MeshIntegrationGroups)
                {
                    if (g.Name == "auto")
                    {
                        _generateFirstPerson = true;
                        // 元のメッシュを三人称に変更
                        copy.Add(new UniGLTF.MeshUtility.MeshIntegrationGroup
                        {
                            Name = UniGLTF.Extensions.VRMC_vrm.FirstPersonType.thirdPersonOnly.ToString(),
                            Renderers = g.Renderers.ToList(),
                        });
                    }
                    copy.Add(g);
                }
            }
            else
            {
                copy.AddRange(MeshIntegrationGroups);
            }
            return copy;
        }

        protected override
         bool TryIntegrate(
            GameObject empty,
            UniGLTF.MeshUtility.MeshIntegrationGroup group,
            out (UniGLTF.MeshUtility.MeshIntegrationResult, GameObject[]) resultAndAdded)
        {
            if (!base.TryIntegrate(empty, group, out resultAndAdded))
            {
                return false;
            }
            var (result, newList) = resultAndAdded;

            if (_generateFirstPerson && group.Name == nameof(UniGLTF.Extensions.VRMC_vrm.FirstPersonType.auto))
            {
                // Mesh 統合の後処理
                // FirstPerson == "auto" の場合に                
                // 頭部の無いモデルを追加で作成する
                Debug.Log("generateFirstPerson");
                if (result.Integrated.Mesh != null)
                {
                    // BlendShape 有り
                    _ProcessFirstPerson(_vrmInstance.Humanoid.Head, result.Integrated.IntegratedRenderer);
                }
                if (result.IntegratedNoBlendShape.Mesh != null)
                {
                    // BlendShape 無しの方
                    _ProcessFirstPerson(_vrmInstance.Humanoid.Head, result.IntegratedNoBlendShape.IntegratedRenderer);
                }
            }
            return true;
        }

        private void _ProcessFirstPerson(Transform firstPersonBone, SkinnedMeshRenderer smr)
        {
            var task = VRM10ObjectFirstPerson.CreateErasedMeshAsync(
                smr,
                firstPersonBone,
                new VRMShaders.ImmediateCaller());
            task.Wait();
            var mesh = task.Result;
            if (mesh != null)
            {
                smr.sharedMesh = mesh;
                smr.name = "auto.headless";
            }
            else
            {
                Debug.LogWarning("no result");
            }
        }

        Vrm10Instance _vrmInstance = null;
        /// <summary>
        /// glTF に比べて Humanoid や FirstPerson の処理が追加される
        /// </summary>
        public override (List<UniGLTF.MeshUtility.MeshIntegrationResult>, List<GameObject>) Process(GameObject go, GameObject instance)
        {
            _vrmInstance = go.GetComponent<Vrm10Instance>();
            if (_vrmInstance == null)
            {
                throw new ArgumentException();
            }

            if (ForceUniqueName)
            {
                throw new NotImplementedException();

                // 必用？
                var animator = go.GetComponent<Animator>();
                var newAvatar = AvatarDescription.RecreateAvatar(animator);
                animator.avatar = newAvatar;
            }

            // TODO: update: spring
            // TODO: update: constraint
            // TODO: update: firstPerson offset
            var (list, newList) = base.Process(go, instance);

            if (FreezeBlendShape || FreezeRotation || FreezeScaling)
            {
                var animator = go.GetComponent<Animator>();
                var newAvatar = AvatarDescription.RecreateAvatar(animator);
                animator.avatar = newAvatar;
            }

            return (list, newList);
        }

        public override void UpdateMeshIntegrationGroups(GameObject root)
        {
            if (root == null)
            {
                return;
            }
            var vrm1 = root.GetComponent<Vrm10Instance>();
            if (vrm1 == null)
            {
                return;
            }
            var vrmObject = vrm1.Vrm;
            if (vrmObject == null)
            {
                return;
            }
            var fp = vrmObject.FirstPerson;
            if (fp == null)
            {
                return;
            }
            foreach (var a in fp.Renderers)
            {
                var g = _GetOrCreateGroup(a.FirstPersonFlag.ToString());
                g.Renderers.Add(a.GetRenderer(root.transform));
            }
        }
    }
}