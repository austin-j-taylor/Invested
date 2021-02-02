using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "KogAnimation", menuName = "ScriptableObjects/KogAnimationScriptableObject")]
public class KogAnimation_SO : ScriptableObject {

    // Rig weights
    [SerializeField] private float weight_toMoving_lerp = 4, weight_toIdle_lerp = 1, weight_toCombat_lerp = 4, weight_toCatch_lerp = 8;
    // Stepping
    [SerializeField] private float step_ToTarget_Delta = 12;
    [SerializeField] private float step_ToTargetRotation_lerp = 15;
    [SerializeField] private float step_defaultDistance = .2f, step_distance_withSpeed = 0.1f;
    [SerializeField] private float step_defaultTime = 0.33f, step_time_withSpeed = .25f;
    [SerializeField] private float step_defaultHeight = .5f;
    [SerializeField] private float stepTime_h = -0.13f, stepTime_a = -0.7f, stepTime_b = -0.5f, stepTime_c = 1.9f;
    [SerializeField] private float distance_h = -0.86f, distance_a = 2.9f, distance_b = -0.05f;
    // Moving/sprinting
    [SerializeField] private float sprint_speedRatioThreshold = 0.5f;
    [SerializeField] private float move_crouch_withSpeed = 0.3f;
    [SerializeField] private float move_crouch_lerp = 10;
    [SerializeField] private float move_height_withSpeed = 0.1f;
    // Crouching
    [SerializeField] private float crouch_max = 0.6f;
    [SerializeField] private float crouch_anchored_amount = .4f;
    [SerializeField] private float crouch_anchored_stepHeight = 0.1f;
    [SerializeField] private float crouch_legSpreadMax = 0.4f, crouch_LegPoleSpreadMax = 0.5f;
    // Head
    [SerializeField] private float head_lookAt_lerp = 5;
    [SerializeField] private float headMinX = -40, headMaxX = 25, headMinY = -60, headMaxY = 60, headMinZ = -20, headMaxZ = 20;
    // Waist
    [SerializeField] private float waist_lean_withSpeed = 15f, waist_lean_withSprinting = 20f;
    [SerializeField] private float waist_lean_withCrouch = 15f;
    [SerializeField] private float waist_rotate_withLegAngleYaw = 0.2f;
    [SerializeField] private float waist_rotate_withLegAngleRoll_withSpeed = .2f;
    [SerializeField] private float waist_rotate_lerp = 10;
    [SerializeField] private float waist_bob = 0.2f, waist_bob_legAngleMax = 90, waist_bob_lerp = 9, waist_bob_withSpeed = .2f, waist_fall_lerp = 10;
    // Leg
    [SerializeField] private float leg_forwards_withSpeed = 0.08f;
    [SerializeField] private float leg_raycast_radius = 0.125f;
    [SerializeField] private float leg_airborne_length = 1.25f;
    [SerializeField] private float leg_tInStep_threshold = 0.25f;
    // Arm
    [SerializeField] private float armHeight = 1.65f;
    [SerializeField] private float armY_a = 0.000304f, armY_b = 1, armY_c = -0.903f, armX_d = -0.00009f, armX_f = 45, armX_g = 0.3f, armZ_o = 0.8f;
    [SerializeField] private float armPoleX_h = -0.01f, armPoleX_j = 65, armPoleY_k = 0.03f, armPoleY_l = 20.8f, armPoleZ_m = 0.04f, armPoleZ_n = -21.74f;
    [SerializeField] private float arm_constraint_weight_combat = 0.5f, arm_constraint_weight_caught = 0.2f;
    [SerializeField] private float arm_reachingToMetal_lerp = 4;



    public float Weight_toMoving_lerp => weight_toMoving_lerp;
    public float Weight_toIdle_lerp => weight_toIdle_lerp;
    public float Weight_toCombat_lerp => weight_toCombat_lerp;
    public float Weight_toCatch_lerp => weight_toCatch_lerp;
    public float Step_ToTarget_Delta => step_ToTarget_Delta;
    public float Step_ToTargetRotation_lerp => step_ToTargetRotation_lerp;
    public float Step_defaultDistance => step_defaultDistance;
    public float Step_distance_withSpeed => step_distance_withSpeed;
    public float Step_defaultTime => step_defaultTime;
    public float Step_time_withSpeed => step_time_withSpeed;
    public float Step_defaultHeight => step_defaultHeight;
    public float StepTime_h => stepTime_h;
    public float StepTime_a => stepTime_a;
    public float StepTime_b => stepTime_b;
    public float StepTime_c => stepTime_c;
    public float Distance_h => distance_h;
    public float Distance_a => distance_a;
    public float Distance_b => distance_b;
    public float Sprint_speedRatioThreshold => sprint_speedRatioThreshold;
    public float Move_crouch_withSpeed => move_crouch_withSpeed;
    public float Move_crouch_lerp => move_crouch_lerp;
    public float Move_height_withSpeed => move_height_withSpeed;
    public float Crouch_max => crouch_max;
    public float Crouch_anchored_amount => crouch_anchored_amount;
    public float Crouch_anchored_stepHeight => crouch_anchored_stepHeight;
    public float Crouch_legSpreadMax => crouch_legSpreadMax;
    public float Crouch_LegPoleSpreadMax => crouch_LegPoleSpreadMax;
    public float Head_lookAt_lerp => head_lookAt_lerp;
    public float HeadMinX => headMinX;
    public float HeadMaxX => headMaxX;
    public float HeadMinY => headMinY;
    public float HeadMaxY => headMaxY;
    public float HeadMinZ => headMinZ;
    public float HeadMaxZ => headMaxZ;
    public float Waist_lean_withSpeed => waist_lean_withSpeed;
    public float Waist_lean_withSprinting => waist_lean_withSprinting;
    public float Waist_lean_withCrouch => waist_lean_withCrouch;
    public float Waist_rotate_withLegAngleYaw => waist_rotate_withLegAngleYaw;
    public float Waist_rotate_withLegAngleRoll_withSpeed => waist_rotate_withLegAngleRoll_withSpeed;
    public float Waist_rotate_lerp => waist_rotate_lerp;
    public float Waist_bob => waist_bob;
    public float Waist_bob_legAngleMax => waist_bob_legAngleMax;
    public float Waist_bob_lerp => waist_bob_lerp;
    public float Waist_bob_withSpeed => waist_bob_withSpeed;
    public float Waist_fall_lerp => waist_fall_lerp;
    public float Leg_forwards_withSpeed => leg_forwards_withSpeed;
    public float Leg_raycast_radius => leg_raycast_radius;
    public float Leg_airborne_length => leg_airborne_length;
    public float Leg_tInStep_threshold => leg_tInStep_threshold;
    public float ArmHeight => armHeight;
    public float ArmY_a => armY_a;
    public float ArmY_b => armY_b;
    public float ArmY_c => armY_c;
    public float ArmX_d => armX_d;
    public float ArmX_f => armX_f;
    public float ArmX_g => armX_g;
    public float ArmZ_o => armZ_o;
    public float ArmPoleX_h => armPoleX_h;
    public float ArmPoleX_j => armPoleX_j;
    public float ArmPoleY_k => armPoleY_k;
    public float ArmPoleY_l => armPoleY_l;
    public float ArmPoleZ_m => armPoleZ_m;
    public float ArmPoleZ_n => armPoleZ_n;
    public float Arm_constraint_weight_combat => arm_constraint_weight_combat;
    public float Arm_constraint_weight_caught => arm_constraint_weight_caught;
    public float Arm_reachingToMetal_lerp => arm_reachingToMetal_lerp;
}