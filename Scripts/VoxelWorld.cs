using Godot;
using System;
using System.Collections.Generic;

namespace MinecraftClone
{
    public enum BlockType
    {
        Air = 0,
        Grass = 1,
        Wood = 2,
        Metal = 3,
        Iron = 4,
        Water = 5,
        Lava = 6,
        Fire = 7,
        Stone = 8
    }

    public partial class VoxelWorld : Node3D
    {
        [Export] public int ChunkSize = 16;
        [Export] public int WorldHeight = 128;
        [Export] public int RenderDistance = 5;
        [Export] public float NoiseScale = 0.05f;
        [Export] public int Seed = 12345;

        private Dictionary<Vector3I, Chunk> chunks = new Dictionary<Vector3I, Chunk>();
        private FastNoiseLite noise;
        private PackedScene chunkScene;
        private Node3D player;

        public override void _Ready()
        {
            noise = new FastNoiseLite();
            noise.Seed = Seed;
            noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
            noise.Frequency = NoiseScale;

            // Load chunk scene (we'll create this)
            chunkScene = GD.Load<PackedScene>("res://Scenes/Chunk.tscn");

            // Generate initial chunks around origin
            GenerateInitialWorld();

            // Signal that world is ready
            CallDeferred("EmitSignal", "world_ready");
        }

        [Signal]
        public delegate void WorldReadyEventHandler();

        private void GenerateInitialWorld()
        {
            for (int x = -RenderDistance; x <= RenderDistance; x++)
            {
                for (int z = -RenderDistance; z <= RenderDistance; z++)
                {
                    Vector3I chunkPos = new Vector3I(x, 0, z);
                    GenerateChunk(chunkPos);
                }
            }
        }

        private void GenerateChunk(Vector3I chunkPos)
        {
            if (chunks.ContainsKey(chunkPos))
                return;

            Chunk newChunk = new Chunk();
            newChunk.Initialize(ChunkSize, WorldHeight);
            newChunk.Position = new Vector3(chunkPos.X * ChunkSize, 0, chunkPos.Z * ChunkSize);

            // Generate terrain
            GenerateTerrain(newChunk, chunkPos);

            // Build mesh
            newChunk.BuildMesh();

            AddChild(newChunk);
            chunks[chunkPos] = newChunk;
        }

        private void GenerateTerrain(Chunk chunk, Vector3I chunkPos)
        {
            for (int x = 0; x < ChunkSize; x++)
            {
                for (int z = 0; z < ChunkSize; z++)
                {
                    float worldX = chunkPos.X * ChunkSize + x;
                    float worldZ = chunkPos.Z * ChunkSize + z;

                    // Get height from noise
                    float height = noise.GetNoise2D(worldX, worldZ);
                    height = (height + 1f) * 0.5f; // Normalize to 0-1
                    int groundHeight = (int)(height * 64) + 32; // Height between 32-96

                    for (int y = 0; y < WorldHeight; y++)
                    {
                        BlockType blockType = BlockType.Air;

                        if (y < groundHeight - 3)
                        {
                            blockType = BlockType.Stone;
                        }
                        else if (y < groundHeight)
                        {
                            blockType = BlockType.Grass;
                        }
                        else if (y < 20) // Water level
                        {
                            blockType = BlockType.Water;
                        }

                        chunk.SetBlock(x, y, z, blockType);
                    }
                }
            }
        }

        public BlockType GetBlock(Vector3 worldPos)
        {
            Vector3I chunkPos = new Vector3I(
                Mathf.FloorToInt(worldPos.X / ChunkSize),
                0,
                Mathf.FloorToInt(worldPos.Z / ChunkSize)
            );

            if (!chunks.ContainsKey(chunkPos))
                return BlockType.Air;

            Vector3I localPos = new Vector3I(
                ((int)worldPos.X) % ChunkSize,
                (int)worldPos.Y,
                ((int)worldPos.Z) % ChunkSize
            );

            return chunks[chunkPos].GetBlock(localPos.X, localPos.Y, localPos.Z);
        }

        public void SetBlock(Vector3 worldPos, BlockType blockType)
        {
            Vector3I chunkPos = new Vector3I(
                Mathf.FloorToInt(worldPos.X / ChunkSize),
                0,
                Mathf.FloorToInt(worldPos.Z / ChunkSize)
            );

            if (!chunks.ContainsKey(chunkPos))
                return;

            Vector3I localPos = new Vector3I(
                ((int)worldPos.X) % ChunkSize,
                (int)worldPos.Y,
                ((int)worldPos.Z) % ChunkSize
            );

            chunks[chunkPos].SetBlock(localPos.X, localPos.Y, localPos.Z, blockType);
            chunks[chunkPos].BuildMesh();
        }

        public void SetPlayer(Node3D playerNode)
        {
            player = playerNode;
        }

        public override void _Process(double delta)
        {
            if (player == null)
                return;

            // Dynamic chunk loading around player
            Vector3I playerChunk = new Vector3I(
                Mathf.FloorToInt(player.Position.X / ChunkSize),
                0,
                Mathf.FloorToInt(player.Position.Z / ChunkSize)
            );

            for (int x = -RenderDistance; x <= RenderDistance; x++)
            {
                for (int z = -RenderDistance; z <= RenderDistance; z++)
                {
                    Vector3I chunkPos = playerChunk + new Vector3I(x, 0, z);
                    GenerateChunk(chunkPos);
                }
            }
        }
    }
}