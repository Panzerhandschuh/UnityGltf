using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UnityGltf
{
	public class AnimationLoader
	{
		private GltfLoaderData data;

		private AccessorLoader accessorLoader;

		private GameObject root;

		public AnimationLoader(GltfLoaderData data)
		{
			this.data = data;

			accessorLoader = new AccessorLoader(data);
		}

		public void LoadAnimations(GameObject root)
		{
			this.root = root;

			var unityAnimation = root.AddComponent<Animation>();
			for (int i = 0; i < data.gltf.Animations.Length; i++)
			{
				var clip = LoadClip(i);

				unityAnimation.AddClip(clip, clip.name);
				if (i == 0)
					unityAnimation.clip = clip;
			}
		}

		private AnimationClip LoadClip(int animationIndex)
		{
			var clip = data.cache.animations[animationIndex];
			if (clip == null)
			{
				var animation = data.gltf.Animations[animationIndex];

				clip = new AnimationClip();
				clip.name = string.IsNullOrEmpty(animation.Name) ? $"Animation{animationIndex}" : animation.Name;
				clip.legacy = true;
				clip.wrapMode = WrapMode.Loop;
				foreach (var channel in animation.Channels)
					LoadChannel(channel, animation.Samplers, clip);

				data.cache.animations[animationIndex] = clip;
			}

			return clip;
		}

		private void LoadChannel(glTFLoader.Schema.AnimationChannel channel, glTFLoader.Schema.AnimationSampler[] samplers, AnimationClip clip)
		{
			var target = channel.Target;
			var node = data.cache.nodes[target.Node.Value];
			var relativePath = GetNodeRelativePath(node.transform, root.transform);

			var sampler = samplers[channel.Sampler];
			var input = accessorLoader.LoadAccessor(sampler.Input);
			var output = accessorLoader.LoadAccessor(sampler.Output);

			switch (target.Path)
			{
				case glTFLoader.Schema.AnimationChannelTarget.PathEnum.translation:
					LoadTranslation(input, output, clip, relativePath);
					break;
				case glTFLoader.Schema.AnimationChannelTarget.PathEnum.rotation:
					LoadRotation(input, output, clip, relativePath);
					break;
				case glTFLoader.Schema.AnimationChannelTarget.PathEnum.scale:
					LoadScale(input, output, clip, relativePath);
					break;
				case glTFLoader.Schema.AnimationChannelTarget.PathEnum.weights:
					LoadWeights(input, output, clip, relativePath);
					break;
				default:
					break;
			}
		}

		private string GetNodeRelativePath(Transform node, Transform root)
		{
			var nodeNames = new List<string>();
			var currentNode = node;
			while (currentNode != root && currentNode != null)
			{
				nodeNames.Insert(0, currentNode.name);

				currentNode = currentNode.parent;
			}

			return string.Join("/", nodeNames);
		}

		private void LoadTranslation(Array input, Array output, AnimationClip clip, string relativePath)
		{
			var curveX = new AnimationCurve();
			var curveY = new AnimationCurve();
			var curveZ = new AnimationCurve();

			var times = (float[])input;
			var positions = (Vector3[])output;
			CoordinateSystemConverter.Convert(positions);

			for (int i = 0; i < times.Length; i++)
			{
				var time = times[i];
				var position = positions[i];

				curveX.AddKey(time, position.x);
				curveY.AddKey(time, position.y);
				curveZ.AddKey(time, position.z);
			}

			clip.SetCurve(relativePath, typeof(Transform), "localPosition.x", curveX);
			clip.SetCurve(relativePath, typeof(Transform), "localPosition.y", curveY);
			clip.SetCurve(relativePath, typeof(Transform), "localPosition.z", curveZ);
		}

		private void LoadRotation(Array input, Array output, AnimationClip clip, string relativePath)
		{
			var curveX = new AnimationCurve();
			var curveY = new AnimationCurve();
			var curveZ = new AnimationCurve();
			var curveW = new AnimationCurve();

			var times = (float[])input;
			var rotations = (Vector4[])output;
			CoordinateSystemConverter.Convert(rotations);

			for (int i = 0; i < times.Length; i++)
			{
				var time = times[i];
				var rotation = rotations[i];

				curveX.AddKey(time, rotation.x);
				curveY.AddKey(time, rotation.y);
				curveZ.AddKey(time, rotation.z);
				curveW.AddKey(time, rotation.w);
			}

			clip.SetCurve(relativePath, typeof(Transform), "localRotation.x", curveX);
			clip.SetCurve(relativePath, typeof(Transform), "localRotation.y", curveY);
			clip.SetCurve(relativePath, typeof(Transform), "localRotation.z", curveZ);
			clip.SetCurve(relativePath, typeof(Transform), "localRotation.w", curveW);
		}

		private void LoadScale(Array input, Array output, AnimationClip clip, string relativePath)
		{
			var curveX = new AnimationCurve();
			var curveY = new AnimationCurve();
			var curveZ = new AnimationCurve();

			var times = (float[])input;
			var scales = (Vector3[])output;

			for (int i = 0; i < times.Length; i++)
			{
				var time = times[i];
				var scale = scales[i];

				curveX.AddKey(time, scale.x);
				curveY.AddKey(time, scale.y);
				curveZ.AddKey(time, scale.z);
			}

			clip.SetCurve(relativePath, typeof(Transform), "localScale.x", curveX);
			clip.SetCurve(relativePath, typeof(Transform), "localScale.y", curveY);
			clip.SetCurve(relativePath, typeof(Transform), "localScale.z", curveZ);
		}

		private void LoadWeights(Array input, Array output, AnimationClip clip, string path)
		{
			// TODO: Load weight animations
		}
	}
}