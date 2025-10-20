using Godot;
using System;
using System.Collections.Generic;

namespace MinecraftClone
{
    public partial class Chunk : StaticBody3D
    {
        private int chunkSize;
        private int worldHeight;
        private BlockType[,,] blocks;
        private ArrayMesh mesh;
        private MeshInstance3D meshInstance;

        // Block face vertices
        private static readonly Vector3[] vertices = new Vector3[]
        {
            // Front face
            new Vector3(0, 0, 1), new Vector3(1, 0, 1),
            new Vector3(1, 1, 1), new Vector3(0, 1, 1),
            // Back face
            new Vector3(1, 0, 0), new Vector3(0, 0, 0),
            new Vector3(0, 1, 0), new Vector3(1, 1, 0),
            // Top face
            new Vector3(0, 1, 1), new Vector3(1, 1, 1),
            new Vector3(1, 1, 0), new Vector3(0, 1, 0),
            // Bottom face
            new Vector3(0, 0, 0), new Vector3(1, 0, 0),
            new Vector3(1, 0, 1), new Vector3(0, 0, 1),
            // Right face
            new Vector3(1, 0, 1), new Vector3(1, 0, 0),
            new Vector3(1, 1, 0), new Vector3(1, 1, 1),
            // Left face
            new Vector3(0, 0, 0), new Vector3(0, 0, 1),
            new Vector3(0, 1, 1), new Vector3(0, 1, 0)
        };

        // UV coordinates for each face
        private static readonly Vector2[] uvs = new Vector2[]
        {
            new Vector2(0, 1), new Vector2(1, 1),
            new Vector2(1, 0), new Vector2(0, 0)
        };

        public void Initialize(int size, int height)
        {
            chunkSize = size;
            worldHeight = height;
            blocks = new BlockType[size, height, size];
            mesh = new ArrayMesh();

            // Create MeshInstance as child
            meshInstance = new MeshInstance3D();
            meshInstance.Mesh = mesh;
            AddChild(meshInstance);
        }

        public void SetBlock(int x, int y, int z, BlockType blockType)
        {
            if (x < 0 || x >= chunkSize || y < 0 || y >= worldHeight || z < 0 || z >= chunkSize)
                return;

            blocks[x, y, z] = blockType;
        }

        public BlockType GetBlock(int x, int y, int z)
        {
            if (x < 0 || x >= chunkSize || y < 0 || y >= worldHeight || z < 0 || z >= chunkSize)
                return BlockType.Air;

            return blocks[x, y, z];
        }

        public void BuildMesh()
        {
            var arrays = new Godot.Collections.Array();
            arrays.Resize((int)Mesh.ArrayType.Max);

            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var colors = new List<Color>();

            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < worldHeight; y++)
                {
                    for (int z = 0; z < chunkSize; z++)
                    {
                        BlockType block = blocks[x, y, z];
                        if (block == BlockType.Air)
                            continue;

                        Vector3 blockPos = new Vector3(x, y, z);
                        Color blockColor = GetBlockColor(block);

                        // Check each face
                        if (ShouldDrawFace(x, y, z, Vector3I.Up))
                            AddFace(vertices, normals, uvs, colors, blockPos, 2, blockColor); // Top
                        if (ShouldDrawFace(x, y, z, Vector3I.Down))
                            AddFace(vertices, normals, uvs, colors, blockPos, 3, blockColor); // Bottom
                        if (ShouldDrawFace(x, y, z, Vector3I.Forward))
                            AddFace(vertices, normals, uvs, colors, blockPos, 0, blockColor); // Front
                        if (ShouldDrawFace(x, y, z, Vector3I.Back))
                            AddFace(vertices, normals, uvs, colors, blockPos, 1, blockColor); // Back
                        if (ShouldDrawFace(x, y, z, Vector3I.Right))
                            AddFace(vertices, normals, uvs, colors, blockPos, 4, blockColor); // Right
                        if (ShouldDrawFace(x, y, z, Vector3I.Left))
                            AddFace(vertices, normals, uvs, colors, blockPos, 5, blockColor); // Left
                    }
                }
            }

            if (vertices.Count == 0)
                return;

            arrays[(int)Mesh.ArrayType.Vertex] = vertices.ToArray();
            arrays[(int)Mesh.ArrayType.Normal] = normals.ToArray();
            arrays[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
            arrays[(int)Mesh.ArrayType.Color] = colors.ToArray();

            mesh.ClearSurfaces();
            mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

            // Create material
            var material = new StandardMaterial3D();
            material.VertexColorUseAsAlbedo = true;
            material.AlbedoColor = Colors.White;
            meshInstance.SetSurfaceOverrideMaterial(0, material);

            // Generate collision from the mesh
            meshInstance.CreateTrimeshCollision();

            // Move collision shape from MeshInstance to StaticBody
            if (meshInstance.GetChildCount() > 0)
            {
                var collisionShape = meshInstance.GetChild(0);
                meshInstance.RemoveChild(collisionShape);
                AddChild(collisionShape);
            }
        }

        private bool ShouldDrawFace(int x, int y, int z, Vector3I direction)
        {
            Vector3I checkPos = new Vector3I(x, y, z) + direction;

            if (checkPos.X < 0 || checkPos.X >= chunkSize ||
                checkPos.Y < 0 || checkPos.Y >= worldHeight ||
                checkPos.Z < 0 || checkPos.Z >= chunkSize)
                return true;

            BlockType adjacentBlock = blocks[checkPos.X, checkPos.Y, checkPos.Z];

            // Don't draw faces between solid blocks
            if (adjacentBlock != BlockType.Air && adjacentBlock != BlockType.Water)
                return false;

            return true;
        }

        private void AddFace(List<Vector3> vertices, List<Vector3> normals,
                           List<Vector2> uvs, List<Color> colors,
                           Vector3 position, int faceIndex, Color color)
        {
            int vertexIndex = faceIndex * 4;

            // Add vertices
            vertices.Add(position + Chunk.vertices[vertexIndex]);
            vertices.Add(position + Chunk.vertices[vertexIndex + 1]);
            vertices.Add(position + Chunk.vertices[vertexIndex + 2]);
            vertices.Add(position + Chunk.vertices[vertexIndex]);
            vertices.Add(position + Chunk.vertices[vertexIndex + 2]);
            vertices.Add(position + Chunk.vertices[vertexIndex + 3]);

            // Add normals
            Vector3 normal = GetFaceNormal(faceIndex);
            for (int i = 0; i < 6; i++)
            {
                normals.Add(normal);
                colors.Add(color);
            }

            // Add UVs
            uvs.Add(Chunk.uvs[0]);
            uvs.Add(Chunk.uvs[1]);
            uvs.Add(Chunk.uvs[2]);
            uvs.Add(Chunk.uvs[0]);
            uvs.Add(Chunk.uvs[2]);
            uvs.Add(Chunk.uvs[3]);
        }

        private Vector3 GetFaceNormal(int faceIndex)
        {
            return faceIndex switch
            {
                0 => Vector3.Forward,
                1 => Vector3.Back,
                2 => Vector3.Up,
                3 => Vec<tor3.Down,
                4 => Vector3.Right,
                5 => Vector3.Left,
                _ => Vector3.Up
            };
        }

        private Color GetBlockColor(BlockType blockType)
        {
            return blockType switch
            {
                BlockType.Grass => new Color(0.2f, 0.7f, 0.2f),
                BlockType.Wood => new Color(0.55f, 0.35f, 0.2f),
                BlockType.Metal => new Color(0.6f, 0.6f, 0.7f),
                BlockType.Iron => new Color(0.7f, 0.7f, 0.75f),
                BlockType.Water => new Color(0.2f, 0.5f, 0.8f, 0.8f),
                BlockType.Lava => new Color(1f, 0.3f, 0f),
                BlockType.Fire => new Color(1f, 0.5f, 0f),
                BlockType.Stone => new Color(0.5f, 0.5f, 0.5f),
                _ => Colors.White
            };
        }
    }
}