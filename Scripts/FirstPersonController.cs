using Godot;
using System;

namespace MinecraftClone
{
    public partial class FirstPersonController : CharacterBody3D
    {
        [Export] public float MoveSpeed = 5.0f;
        [Export] public float JumpVelocity = 8.0f;
        [Export] public float MouseSensitivity = 0.3f;
        [Export] public float MaxLookAngle = 80.0f;
        [Export] public float Gravity = 20.0f;

        private Node3D head;
        private Camera3D camera;
        private RayCast3D interactRay;
        private float mouseDelta = 0.0f;

        // Current equipped tool
        private ToolType currentTool = ToolType.Hand;

        public enum ToolType
        {
            Hand,
            Sword,
            Pickaxe,
            Bow
        }

        public override void _Ready()
        {
            // Set up head and camera
            head = GetNode<Node3D>("Head");
            camera = head.GetNode<Camera3D>("Camera3D");
            interactRay = camera.GetNode<RayCast3D>("InteractRay");

            // Capture mouse
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }

        public override void _Input(InputEvent @event)
        {
            // Mouse look
            if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
            {
                RotateY(-Mathf.DegToRad(mouseMotion.Relative.X * MouseSensitivity));

                mouseDelta -= mouseMotion.Relative.Y * MouseSensitivity;
                mouseDelta = Mathf.Clamp(mouseDelta, -MaxLookAngle, MaxLookAngle);
                head.RotationDegrees = new Vector3(mouseDelta, 0, 0);
            }

            // Tool selection
            if (@event.IsActionPressed("tool_1"))
                currentTool = ToolType.Hand;
            else if (@event.IsActionPressed("tool_2"))
                currentTool = ToolType.Sword;
            else if (@event.IsActionPressed("tool_3"))
                currentTool = ToolType.Pickaxe;
            else if (@event.IsActionPressed("tool_4"))
                currentTool = ToolType.Bow;

            // Mouse clicks for interaction
            if (@event.IsActionPressed("interact_primary"))
                PrimaryInteract();
            else if (@event.IsActionPressed("interact_secondary"))
                SecondaryInteract();

            // Toggle mouse capture
            if (@event.IsActionPressed("ui_cancel"))
            {
                Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured ?
                    Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            Vector3 velocity = Velocity;

            // Apply gravity
            if (!IsOnFloor())
                velocity.Y -= Gravity * (float)delta;

            // Handle jump
            if (Input.IsActionPressed("jump") && IsOnFloor())
                velocity.Y = JumpVelocity;

            // Get input direction
            Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
            Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

            // Apply movement
            if (direction != Vector3.Zero)
            {
                velocity.X = direction.X * MoveSpeed;
                velocity.Z = direction.Z * MoveSpeed;
            }
            else
            {
                velocity.X = Mathf.MoveToward(Velocity.X, 0, MoveSpeed * (float)delta);
                velocity.Z = Mathf.MoveToward(Velocity.Z, 0, MoveSpeed * (float)delta);
            }

            Velocity = velocity;
            MoveAndSlide();
        }

        private void PrimaryInteract()
        {
            if (interactRay.IsColliding())
            {
                var collider = interactRay.GetCollider();
                Vector3 hitPoint = interactRay.GetCollisionPoint();
                Vector3 hitNormal = interactRay.GetCollisionNormal();

                // Get world reference
                var world = GetNode<VoxelWorld>("/root/Game/World");

                switch (currentTool)
                {
                    case ToolType.Pickaxe:
                        // Remove block
                        Vector3 blockPos = (hitPoint - hitNormal * 0.1f).Floor();
                        world.SetBlock(blockPos, BlockType.Air);
                        break;

                    case ToolType.Sword:
                        // Attack enemies
                        if (collider.HasMethod("TakeDamage"))
                        {
                            collider.Call("TakeDamage", 10.0f);
                        }
                        break;

                    case ToolType.Bow:
                        // Shoot arrow (implement arrow projectile)
                        ShootArrow();
                        break;
                }
            }
        }

        private void SecondaryInteract()
        {
            if (interactRay.IsColliding())
            {
                Vector3 hitPoint = interactRay.GetCollisionPoint();
                Vector3 hitNormal = interactRay.GetCollisionNormal();

                // Place block
                var world = GetNode<VoxelWorld>("/root/Game/World");
                Vector3 blockPos = (hitPoint + hitNormal * 0.1f).Floor();
                world.SetBlock(blockPos, BlockType.Wood); // Default to wood for now
            }
        }

        private void ShootArrow()
        {
            // Implement arrow shooting logic
            GD.Print("Shooting arrow!");
        }

        public ToolType GetCurrentTool()
        {
            return currentTool;
        }
    }
}