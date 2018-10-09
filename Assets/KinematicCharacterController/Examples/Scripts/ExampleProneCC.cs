using System.Collections.Generic;
using Core.Utils;
using UnityEngine;

namespace KinematicCharacterController.Examples
{
    public class ExampleProneCC:BaseCharacterController
    {
        private static readonly LoggerAdapter Logger = new LoggerAdapter(typeof(ExamplePlayer));
        [Header("Stable Movement")]
        public float MaxStableMoveSpeed = 10f;
        public float StableMovementSharpness = 15;
        public float OrientationSharpness = 10;
        
        [Header("Air Movement")]
        public float MaxAirMoveSpeed = 10f;
        public float AirAccelerationSpeed = 5f;
        public float Drag = 0.1f;

        [Header("Misc")]
        public List<Collider> IgnoredColliders = new List<Collider>();
        public bool OrientTowardsGravity = false;
        public Vector3 Gravity = new Vector3(0, -30f, 0);
        public Transform MeshRoot;
        public Transform CameraFollowPoint;
        
        
        public Vector3 WorldUp = Vector3.up;
        public CharacterState CurrentCharacterState { get; private set; }

        private Collider[] _probedColliders = new Collider[8];
        private Vector3 _moveInputVector;
        private Vector3 _lookInputVector;
        private Quaternion _lookRotation;
        private Quaternion _lookPrevRotation = Quaternion.identity;
        private int QERotate = 0;

        public void SetInputs(ref PlayerCharacterInputs inputs)
        {
            // Clamp input
            Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(inputs.MoveAxisRight, 0f, inputs.MoveAxisForward), 1f);

            // Calculate camera direction and rotation on the character plane
            Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, WorldUp).normalized;
            if (cameraPlanarDirection.sqrMagnitude == 0f)
            {
                cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, WorldUp).normalized;
            }
            Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, WorldUp);
            
            // Move and look inputs
            _moveInputVector = cameraPlanarRotation * moveInputVector;
            _lookInputVector = cameraPlanarDirection;
            _lookRotation = cameraPlanarRotation;
            QERotate = inputs.ClockRotate;
            DebugDraw.DebugArrow(transform.position + new Vector3(0,2,0), (_lookRotation * Vector3.forward).normalized * 5, Color.cyan, 0);
        }
        
        
        public override void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (_lookInputVector != Vector3.zero && OrientationSharpness > 0f)
            {
                var euler = currentRotation.eulerAngles;
                var cameraEuler = _lookRotation.eulerAngles;
                cameraEuler.z = euler.z;
                currentRotation = Quaternion.Slerp(currentRotation, Quaternion.Euler(cameraEuler),
                    1 - Mathf.Exp(-OrientationSharpness * deltaTime));
            }
        }
        
        public override void UpdateRotation(ref Quaternion currentRotation, float deltaTime, out Vector3 deltaEuler)
        {
            float delta = 0.0f;
            if (_lookInputVector != Vector3.zero)
            {
                var cameraEuler = _lookRotation.eulerAngles;
                delta = Mathf.DeltaAngle(_lookPrevRotation.eulerAngles.y, _lookRotation.eulerAngles.y);
                _lookPrevRotation = _lookRotation;
            }

            delta = QERotate * deltaTime * -50f;
            deltaEuler = new Vector3(0f, delta, 0f);
        }

        public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            Vector3 targetMovementVelocity = Vector3.zero;
            // Ground movement
            if (Motor.GroundingStatus.IsStableOnGround)
            {
                Vector3 effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;
                if (currentVelocity.sqrMagnitude > 0f && Motor.GroundingStatus.SnappingPrevented)
                {
                    // Take the normal from where we're coming from
                    Vector3 groundPointToCharacter = Motor.TransientPosition - Motor.GroundingStatus.GroundPoint;
                    if (Vector3.Dot(currentVelocity, groundPointToCharacter) >= 0f)
                    {
                        effectiveGroundNormal = Motor.GroundingStatus.OuterGroundNormal;
                    }
                    else
                    {
                        effectiveGroundNormal = Motor.GroundingStatus.InnerGroundNormal;
                    }
                }

                // Reorient velocity on slope
                currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) *
                                  currentVelocity.magnitude;

                // Calculate target velocity
                // 左手系的话，应该是左, 前×上，逆时针，左
                Vector3 inputLeft = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
                // 上×左，顺时针，前
                Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputLeft).normalized *
                                          _moveInputVector.magnitude;
                targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;

                // Smooth movement Velocity
                // 函数图像查看
                // http://www.fooplot.com/#W3sidHlwZSI6MCwiZXEiOiIxLWVeKC14KSIsImNvbG9yIjoiIzAwMDAwMCJ9LHsidHlwZSI6MTAwMH1d
                // 大致是一个前面快增长，后面慢增长
                currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity,
                    1 - Mathf.Exp(-StableMovementSharpness * deltaTime));
            }
            // Air movement
            else
            {
                // Add move input
                if (_moveInputVector.sqrMagnitude > 0f)
                {
                    targetMovementVelocity = _moveInputVector * MaxAirMoveSpeed;

                    // Prevent climbing on un-stable slopes with air movement
                    if (Motor.GroundingStatus.FoundAnyGround)
                    {
                        Vector3 perpenticularObstructionNormal = Vector3
                            .Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal),
                                Motor.CharacterUp).normalized;
                        targetMovementVelocity =
                            Vector3.ProjectOnPlane(targetMovementVelocity, perpenticularObstructionNormal);
                    }

                    Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, Gravity);
                    currentVelocity += velocityDiff * AirAccelerationSpeed * deltaTime;
                }

                // Gravity
                currentVelocity += Gravity * deltaTime;

                // Drag
                //http://www.fooplot.com/#W3sidHlwZSI6MCwiZXEiOiIxLygxKygwLjEqeCkpIiwiY29sb3IiOiIjMDAwMDAwIn0seyJ0eXBlIjoxMDAwfV0-
                currentVelocity *= (1f / (1f + (Drag * deltaTime)));
            }
        }

        public override void BeforeCharacterUpdate(float deltaTime)
        {

        }

        public override void PostGroundingUpdate(float deltaTime)
        {
 
        }

        public override void AfterCharacterUpdate(float deltaTime)
        {
            
        }

        public override bool IsColliderValidForCollisions(Collider coll)
        {
            if(IgnoredColliders.Count >= 0)
            {
                return true;
            }

            if (IgnoredColliders.Contains(coll))
            {
                return false;
            }
            return true;
        }

        public override void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            DebugDraw.DrawArrow(hitPoint, hitNormal * 2.0f, Color.black);
        }

        public override void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
            DebugDraw.DrawArrow(hitPoint, hitNormal * 5.0f, Color.green);
        }

        public override void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
            Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
            DebugDraw.DrawArrow(hitPoint, hitNormal, Color.yellow);
            if (hitStabilityReport.LedgeDetected)
            {
                DebugDraw.DrawArrow(hitPoint, hitStabilityReport.LedgeGroundNormal, Color.green);
                DebugDraw.DrawArrow(hitPoint, hitStabilityReport.LedgeRightDirection, Color.red);
                DebugDraw.DrawArrow(hitPoint, hitStabilityReport.LedgeFacingDirection, Color.blue);
            }
        }
        
        private static readonly float MarkerSize = 1.0f;
        private static readonly float RaySize = 2.0f;
    }
}