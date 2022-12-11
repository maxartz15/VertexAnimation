using System.Collections.Generic;
using UnityEngine;

namespace TAO.VertexAnimation
{
	public static class AnimationBaker
	{
		[System.Serializable]
		public struct BakedData
		{
			public Mesh mesh;
			public List<Texture2D> positionMaps;
			public int maxFrames;
			public Vector3 minBounds;
			public Vector3 maxBounds;

			// Returns main position map.
			public Texture2D GetPositionMap
			{
				get
				{
					return positionMaps[0];
				}
			}
		}

		[System.Serializable]
		public struct AnimationInfo
		{
			public bool applyRootMotion;
			public int rawFrameHeight;
			public int frameHeight;
			public int frameSpacing;
			public int frames;
			public int maxFrames;
			public int textureWidth;
			public int textureHeight;
			public int fps;

			// Create animation info and calculate values.
			public AnimationInfo(Mesh mesh, bool applyRootMotion, int frames, int textureWidth, int fps)
			{
				this.applyRootMotion = applyRootMotion;
				this.frames = frames;
				this.textureWidth = textureWidth;
				this.fps = fps;

				rawFrameHeight = Mathf.CeilToInt((float)mesh.vertices.Length / this.textureWidth);
				frameHeight = Mathf.NextPowerOfTwo(rawFrameHeight);
				frameSpacing = (frameHeight - rawFrameHeight) + 1;

				textureHeight = Mathf.NextPowerOfTwo(frameHeight * this.frames);

				maxFrames = textureHeight / frameHeight;
			}
		}

		[System.Serializable]
		public struct BakedAnimation
		{
			public Texture2D positionMap;
			public Vector3 minBounds;
			public Vector3 maxBounds;
		}

		public static BakedData Bake(this GameObject model, AnimationClip[] animationClips, bool applyRootMotion, int fps, int textureWidth)
		{
			BakedData bakedData = new BakedData()
			{
				mesh = null,
				positionMaps = new List<Texture2D>(),
				minBounds = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue),
				maxBounds = new Vector3(float.MinValue, float.MinValue, float.MinValue)
			};

			// Calculate what our max frames/time is going to be.
			int maxFrames = 0;
			foreach (AnimationClip ac in animationClips)
			{
				int frames = Mathf.FloorToInt(fps * ac.length);

				if (maxFrames < frames)
				{
					maxFrames = frames;
				}
			}

			// Get the target mesh to calculate the animation info.
			Mesh mesh = model.GetComponent<SkinnedMeshRenderer>().sharedMesh;

			// Get the info for the biggest animation.
			AnimationInfo animationInfo = new AnimationInfo(mesh, applyRootMotion, maxFrames, textureWidth, fps);

			foreach (AnimationClip ac in animationClips)
			{
				// Set the frames for this animation.
				animationInfo.frames = Mathf.FloorToInt(fps * ac.length);

				BakedData bd = Bake(model, ac, animationInfo);
				bakedData.mesh = bd.mesh;
				bakedData.positionMaps.AddRange(bd.positionMaps);
				bakedData.maxFrames = maxFrames;
				bakedData.minBounds = Vector3.Min(bakedData.minBounds, bd.minBounds);
				bakedData.maxBounds = Vector3.Max(bakedData.maxBounds, bd.maxBounds);
			}

			bakedData.mesh.bounds = new Bounds()
			{
				max = bakedData.maxBounds,
				min = bakedData.minBounds
			};

			return bakedData;
		}

		public static BakedData Bake(this GameObject model, AnimationClip animationClip, AnimationInfo animationInfo)
		{
			Mesh mesh = new Mesh
			{
				name = string.Format("{0}", model.name)
			};

			// Set root motion options.
			if (model.TryGetComponent(out Animator animator))
			{
				animator.applyRootMotion = animationInfo.applyRootMotion;
			}

			// Bake mesh for a copy and to apply the new UV's to.
			SkinnedMeshRenderer skinnedMeshRenderer = model.GetComponent<SkinnedMeshRenderer>();
			mesh = model.GetComponent<SkinnedMeshRenderer>().sharedMesh.Copy();
			mesh.RecalculateBounds();

			mesh.uv3 = mesh.BakePositionUVs(animationInfo);

			BakedAnimation bakedAnimation = BakeAnimation(model, animationClip, animationInfo);

			BakedData bakedData = new BakedData()
			{
				mesh = mesh,
				positionMaps = new List<Texture2D>() { bakedAnimation.positionMap },
				maxFrames = animationInfo.maxFrames,
				minBounds = bakedAnimation.minBounds,
				maxBounds = bakedAnimation.maxBounds
			};

			mesh.bounds = new Bounds()
			{ 
				max = bakedAnimation.maxBounds, 
				min = bakedAnimation.minBounds
			};

			return bakedData;
		}

		public static BakedAnimation BakeAnimation(this GameObject model, AnimationClip animationClip, AnimationInfo animationInfo)
		{
			// Create positionMap Texture without MipMaps which is Linear and HDR to store values in a bigger range.
			Texture2D positionMap = new Texture2D(animationInfo.textureWidth, animationInfo.textureHeight, TextureFormat.RGBAHalf, false, true);

			// Keep track of min/max bounds.
			Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

			// Create instance to sample from.
			GameObject inst = GameObject.Instantiate(model);
			SkinnedMeshRenderer skinnedMeshRenderer = inst.GetComponent<SkinnedMeshRenderer>();

			int y = 0;
			for (int f = 0; f < animationInfo.frames; f++)
			{
				animationClip.SampleAnimation(inst, (animationClip.length / animationInfo.frames) * f);

				Mesh sampledMesh = new Mesh();
				skinnedMeshRenderer.BakeMesh(sampledMesh);

				List<Vector3> verts = new List<Vector3>();
				sampledMesh.GetVertices(verts);
				List<Vector3> normals = new List<Vector3>();
				sampledMesh.GetNormals(normals);

				int x = 0;
				for (int v = 0; v < verts.Count; v++)
				{
					min = Vector3.Min(min, verts[v]);
					max = Vector3.Max(max, verts[v]);

					positionMap.SetPixel(x, y,
							new Color(verts[v].x, verts[v].y, verts[v].z,
							VectorUtils.EncodeFloat3ToFloat1(normals[v]))
						);

					x++;
					if (x >= animationInfo.textureWidth)
					{
						x = 0;
						y++;
					}
				}
				y += animationInfo.frameSpacing;
			}

			GameObject.DestroyImmediate(inst);

			positionMap.name = string.Format("VA_N-{0}_F-{1}_MF-{2}_FPS-{3}", NamingConventionUtils.ConvertToValidString(animationClip.name), animationInfo.frames, animationInfo.maxFrames, animationInfo.fps);
			positionMap.filterMode = FilterMode.Point;
			positionMap.Apply(false, true);

			return new BakedAnimation()
			{
				positionMap = positionMap,
				minBounds = min,
				maxBounds = max
			};
		}

		public static Vector2[] BakePositionUVs(this Mesh mesh, AnimationInfo animationInfo)
		{
			Vector2[] uv3 = new Vector2[mesh.vertexCount];

			float xOffset = 1.0f / animationInfo.textureWidth;
			float yOffset = 1.0f / animationInfo.textureHeight;

			float x = xOffset / 2.0f;
			float y = yOffset / 2.0f;

			for (int v = 0; v < uv3.Length; v++)
			{
				uv3[v] = new Vector2(x, y);

				x += xOffset;
				if (x >= 1.0f)
				{
					x = xOffset / 2.0f;
					y += yOffset;
				}
			}

			mesh.uv3 = uv3;

			return uv3;
		}
	}
}