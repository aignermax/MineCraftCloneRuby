using Godot;
using System;

namespace MinecraftClone
{
    public enum AnimalType
    {
        Panda,
        Cat,
        Dog,
        Parrot,
        Cow,
        Fox,
        Fish,
        Creeper
    }

    public partial class Animal : CharacterBody3D
    {
        [Export] public AnimalType Type { get; set; }
        [Export] public float MoveSpeed = 2.0f;
        [Export] public float Health = 20.0f;
        [Export] public float WanderRadius = 10.0f;
        [Export] public bool IsHostile = false;
        [Export] public float AttackDamage = 5.0f;
        [Export] public float AttackRange = 2.0f;

        private Vector3 targetPosition;
        private float wanderTimer = 0.0f;
        private float wanderInterval = 3.0f;
        private MeshInstance3D meshInstance;
        private float gravity = 20.0f;

        // For creeper
        private bool isExploding = false;
        private float explosionTimer = 0.0f;
        private const float ExplosionDelay = 1.5f;

        private Node3D player;
        private VoxelWorld world;

        public override void _Ready()
        {
            CreateAnimalMesh();
            SetNewWanderTarget();

            // Get references
            player = GetNode<Node3D>("/root/Game/Player");
            world = GetNode<VoxelWorld>("/root/Game/World");
        }

        private void CreateAnimalMesh()
        {
            meshInstance = new MeshInstance3D();
            AddChild(meshInstance);

            switch (Type)
            {
                case AnimalType.Panda:
                    CreatePandaMesh();
                    break;
                case AnimalType.Cat:
                    CreateCatMesh();
                    break;
                case AnimalType.Dog:
                    CreateDogMesh();
                    break;
                case AnimalType.Parrot:
                    CreateParrotMesh();
                    break;
                case AnimalType.Cow:
                    CreateCowMesh();
                    break;
                case AnimalType.Fox:
                    CreateFoxMesh();
                    break;
                case AnimalType.Fish:
                    CreateFishMesh();
                    break;
                case AnimalType.Creeper:
                    CreateCreeperMesh();
                    IsHostile = true;
                    break;
            }
        }

        private void CreatePandaMesh()
        {
            // Create simple panda from cubes
            var body = new BoxMesh();
            body.Size = new Vector3(0.8f, 0.8f, 1.2f);
            meshInstance.Mesh = body;

            // Add head
            var headMesh = new MeshInstance3D();
            headMesh.Mesh = new BoxMesh() { Size = new Vector3(0.6f, 0.6f, 0.6f) };
            headMesh.Position = new Vector3(0, 0.5f, 0.4f);
            meshInstance.AddChild(headMesh);

            // Add material
            var material = new StandardMaterial3D();
            material.AlbedoColor = Colors.White;
            meshInstance.MaterialOverride = material;

            var blackMaterial = new StandardMaterial3D();
            blackMaterial.AlbedoColor = Colors.Black;

            // Add black patches (ears, eyes, limbs)
            AddLimbs(blackMaterial);
        }

        private void CreateCatMesh()
        {
            var body = new BoxMesh();
            body.Size = new Vector3(0.4f, 0.3f, 0.8f);
            meshInstance.Mesh = body;

            var headMesh = new MeshInstance3D();
            headMesh.Mesh = new BoxMesh() { Size = new Vector3(0.3f, 0.3f, 0.3f) };
            headMesh.Position = new Vector3(0, 0.1f, 0.35f);
            meshInstance.AddChild(headMesh);

            var material = new StandardMaterial3D();
            material.AlbedoColor = new Color(0.8f, 0.5f, 0.2f); // Orange
            meshInstance.MaterialOverride = material;
        }

        private void CreateDogMesh()
        {
            var body = new BoxMesh();
            body.Size = new Vector3(0.5f, 0.5f, 0.9f);
            meshInstance.Mesh = body;

            var headMesh = new MeshInstance3D();
            headMesh.Mesh = new BoxMesh() { Size = new Vector3(0.4f, 0.4f, 0.5f) };
            headMesh.Position = new Vector3(0, 0.1f, 0.4f);
            meshInstance.AddChild(headMesh);

            var material = new StandardMaterial3D();
            material.AlbedoColor = new Color(0.6f, 0.4f, 0.2f); // Brown
            meshInstance.MaterialOverride = material;
        }

        private void CreateParrotMesh()
        {
            var body = new BoxMesh();
            body.Size = new Vector3(0.3f, 0.4f, 0.3f);
            meshInstance.Mesh = body;

            // Add wings
            var wingLeft = new MeshInstance3D();
            wingLeft.Mesh = new BoxMesh() { Size = new Vector3(0.4f, 0.1f, 0.2f) };
            wingLeft.Position = new Vector3(-0.3f, 0, 0);
            meshInstance.AddChild(wingLeft);

            var wingRight = new MeshInstance3D();
            wingRight.Mesh = new BoxMesh() { Size = new Vector3(0.4f, 0.1f, 0.2f) };
            wingRight.Position = new Vector3(0.3f, 0, 0);
            meshInstance.AddChild(wingRight);

            var material = new StandardMaterial3D();
            material.AlbedoColor = new Color(0.2f, 0.8f, 0.2f); // Green
            meshInstance.MaterialOverride = material;
        }

        private void CreateCowMesh()
        {
            var body = new BoxMesh();
            body.Size = new Vector3(0.9f, 0.8f, 1.4f);
            meshInstance.Mesh = body;

            var headMesh = new MeshInstance3D();
            headMesh.Mesh = new BoxMesh() { Size = new Vector3(0.5f, 0.5f, 0.6f) };
            headMesh.Position = new Vector3(0, 0.1f, 0.6f);
            meshInstance.AddChild(headMesh);

            var material = new StandardMaterial3D();
            material.AlbedoColor = new Color(0.3f, 0.2f, 0.1f); // Dark brown
            meshInstance.MaterialOverride = material;

            // Add white spots
            AddCowSpots();
        }

        private void CreateFoxMesh()
        {
            var body = new BoxMesh();
            body.Size = new Vector3(0.4f, 0.4f, 0.8f);
            meshInstance.Mesh = body;

            var headMesh = new MeshInstance3D();
            headMesh.Mesh = new PrismMesh() { Size = new Vector3(0.3f, 0.3f, 0.4f) };
            headMesh.Position = new Vector3(0, 0.1f, 0.35f);
            meshInstance.AddChild(headMesh);

            // Add tail
            var tail = new MeshInstance3D();
            tail.Mesh = new BoxMesh() { Size = new Vector3(0.2f, 0.2f, 0.6f) };
            tail.Position = new Vector3(0, 0, -0.5f);
            tail.RotationDegrees = new Vector3(45, 0, 0);
            meshInstance.AddChild(tail);

            var material = new StandardMaterial3D();
            material.AlbedoColor = new Color(0.9f, 0.5f, 0.1f); // Orange
            meshInstance.MaterialOverride = material;
        }

        private void CreateFishMesh()
        {
            var body = new SphereMesh();
            body.RadialSegments = 8;
            body.Height = 0.2f;
            body.Radius = 0.15f;
            meshInstance.Mesh = body;

            // Add tail fin
            var tail = new MeshInstance3D();
            tail.Mesh = new PrismMesh() { Size = new Vector3(0.2f, 0.1f, 0.3f) };
            tail.Position = new Vector3(0, 0, -0.2f);
            tail.RotationDegrees = new Vector3(90, 0, 0);
            meshInstance.AddChild(tail);

            var material = new StandardMaterial3D();
            material.AlbedoColor = new Color(0.3f, 0.6f, 0.8f); // Light blue
            meshInstance.MaterialOverride = material;
        }

        private void CreateCreeperMesh()
        {
            var body = new BoxMesh();
            body.Size = new Vector3(0.6f, 1.2f, 0.6f);
            meshInstance.Mesh = body;

            var headMesh = new MeshInstance3D();
            headMesh.Mesh = new BoxMesh() { Size = new Vector3(0.8f, 0.8f, 0.8f) };
            headMesh.Position = new Vector3(0, 1.0f, 0);
            meshInstance.AddChild(headMesh);

            var material = new StandardMaterial3D();
            material.AlbedoColor = new Color(0.0f, 0.8f, 0.0f); // Green
            meshInstance.MaterialOverride = material;

            // Add creeper face
            AddCreeperFace(headMesh);
        }

        private void AddLimbs(Material material)
        {
            // Add simple limbs for panda
            for (int i = 0; i < 4; i++)
            {
                var limb = new MeshInstance3D();
                limb.Mesh = new BoxMesh() { Size = new Vector3(0.3f, 0.4f, 0.3f) };
                float x = (i % 2 == 0) ? -0.3f : 0.3f;
                float z = (i < 2) ? 0.3f : -0.3f;
                limb.Position = new Vector3(x, -0.5f, z);
                limb.MaterialOverride = material;
                meshInstance.AddChild(limb);
            }
        }

        private void AddCowSpots()
        {
            // Add white spots to cow
            var spotMaterial = new StandardMaterial3D();
            spotMaterial.AlbedoColor = Colors.White;

            for (int i = 0; i < 3; i++)
            {
                var spot = new MeshInstance3D();
                spot.Mesh = new SphereMesh() { RadialSegments = 6, Rings = 4, Height = 0.2f, Radius = 0.2f};
                spot.Position = new Vector3(
                    GD.Randf() * 0.4f - 0.2f,
                    GD.Randf() * 0.3f,
                    GD.Randf() * 0.6f - 0.3f
                );
                spot.MaterialOverride = spotMaterial;
                meshInstance.AddChild(spot);
            }
        }

        private void AddCreeperFace(MeshInstance3D head)
        {
            var faceMaterial = new StandardMaterial3D();
            faceMaterial.AlbedoColor = Colors.Black;

            // Eyes
            var leftEye = new MeshInstance3D();
            leftEye.Mesh = new BoxMesh() { Size = new Vector3(0.1f, 0.2f, 0.02f) };
            leftEye.Position = new Vector3(-0.2f, 0.1f, 0.41f);
            leftEye.MaterialOverride = faceMaterial;
            head.AddChild(leftEye);

            var rightEye = new MeshInstance3D();
            rightEye.Mesh = new BoxMesh() { Size = new Vector3(0.1f, 0.2f, 0.02f) };
            rightEye.Position = new Vector3(0.2f, 0.1f, 0.41f);
            rightEye.MaterialOverride = faceMaterial;
            head.AddChild(rightEye);

            // Mouth
            var mouth = new MeshInstance3D();
            mouth.Mesh = new BoxMesh() { Size = new Vector3(0.3f, 0.1f, 0.02f) };
            mouth.Position = new Vector3(0, -0.2f, 0.41f);
            mouth.MaterialOverride = faceMaterial;
            head.AddChild(mouth);
        }

        public override void _PhysicsProcess(double delta)
        {
            Vector3 velocity = Velocity;

            // Apply gravity
            if (!IsOnFloor())
                velocity.Y -= gravity * (float)delta;

            // AI behavior
            if (IsHostile && player != null)
            {
                HandleHostileBehavior(delta);
            }
            else
            {
                HandlePassiveBehavior(delta);
            }

            // Move towards target
            Vector3 direction = (targetPosition - GlobalPosition).Normalized();
            direction.Y = 0; // Keep movement horizontal

            if (direction.Length() > 0.1f)
            {
                velocity.X = direction.X * MoveSpeed;
                velocity.Z = direction.Z * MoveSpeed;

                // Face movement direction
                if (direction.Length() > 0)
                {
                    LookAt(GlobalPosition + direction, Vector3.Up);
                }
            }
            else
            {
                velocity.X = 0;
                velocity.Z = 0;
            }

            Velocity = velocity;
            MoveAndSlide();

            // Creeper explosion logic
            if (isExploding)
            {
                explosionTimer += (float)delta;
                if (explosionTimer >= ExplosionDelay)
                {
                    Explode();
                }
            }
        }

        private void HandlePassiveBehavior(double delta)
        {
            wanderTimer += (float)delta;
            if (wanderTimer >= wanderInterval)
            {
                SetNewWanderTarget();
                wanderTimer = 0.0f;
            }

            // Check if reached target
            if (GlobalPosition.DistanceTo(targetPosition) < 0.5f)
            {
                SetNewWanderTarget();
            }
        }

        private void HandleHostileBehavior(double delta)
        {
            float distanceToPlayer = GlobalPosition.DistanceTo(player.GlobalPosition);

            if (Type == AnimalType.Creeper)
            {
                if (distanceToPlayer < 2.0f && !isExploding)
                {
                    // Start explosion countdown
                    isExploding = true;
                    explosionTimer = 0.0f;

                    // Flash red
                    var material = meshInstance.MaterialOverride as StandardMaterial3D;
                    material.AlbedoColor = Colors.Red;
                }
                else if (distanceToPlayer < 10.0f)
                {
                    // Chase player
                    targetPosition = player.GlobalPosition;
                }
                else
                {
                    HandlePassiveBehavior(delta);
                }
            }
        }

        private void SetNewWanderTarget()
        {
            float angle = GD.Randf() * Mathf.Pi * 2;
            float distance = GD.Randf() * WanderRadius;

            targetPosition = GlobalPosition + new Vector3(
                Mathf.Cos(angle) * distance,
                0,
                Mathf.Sin(angle) * distance
            );
        }

        public void TakeDamage(float damage)
        {
            Health -= damage;
            if (Health <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            // Drop items
            DropItems();

            // Remove from scene
            QueueFree();
        }

        private void DropItems()
        {
            // Implement item dropping based on animal type
            switch (Type)
            {
                case AnimalType.Cow:
                    // Drop leather and beef
                    GD.Print("Dropped leather and beef");
                    break;
                case AnimalType.Creeper:
                    // Drop gunpowder
                    GD.Print("Dropped gunpowder");
                    break;
                    // Add more cases as needed
            }
        }

        private void Explode()
        {
            // Creeper explosion
            float explosionRadius = 4.0f;
            float explosionPower = 3.0f;

            // Destroy blocks in radius
            for (int x = -4; x <= 4; x++)
            {
                for (int y = -4; y <= 4; y++)
                {
                    for (int z = -4; z <= 4; z++)
                    {
                        Vector3 blockPos = GlobalPosition + new Vector3(x, y, z);
                        float distance = blockPos.DistanceTo(GlobalPosition);

                        if (distance <= explosionRadius)
                        {
                            // Random chance to destroy block based on distance
                            float destroyChance = 1.0f - (distance / explosionRadius);
                            if (GD.Randf() < destroyChance)
                            {
                                world.SetBlock(blockPos.Floor(), BlockType.Air);
                            }
                        }
                    }
                }
            }

            // Damage player if nearby
            float playerDistance = GlobalPosition.DistanceTo(player.GlobalPosition);
            if (playerDistance < explosionRadius)
            {
                float damage = (1.0f - playerDistance / explosionRadius) * 20.0f;
                // Apply damage to player
                GD.Print($"Explosion damage to player: {damage}");
            }

            // Remove creeper
            QueueFree();
        }
    }
}