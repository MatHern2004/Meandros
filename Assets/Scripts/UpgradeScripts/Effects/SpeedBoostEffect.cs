using UnityEngine;

// Makes the player move faster based on their MoveSpeed upgrades.
// This works on top of the player controller without changing it: after the controller
// moves the player each frame, this script measures how far they moved sideways and
// adds an extra percentage of that same movement.
[RequireComponent(typeof(PlayerUpgrades))]
public class SpeedBoostEffect : MonoBehaviour
{
    // Never let curses slow the player down by more than this (0.5 = half speed)
    [SerializeField] private float minimumMultiplier = 0.5f;

    private PlayerUpgrades upgrades;
    private CharacterController characterController;
    // Where the player was at the end of last frame (after our own boost)
    private Vector3 lastPosition;

    private void Start()
    {
        upgrades = GetComponent<PlayerUpgrades>();
        characterController = GetComponent<CharacterController>();
        lastPosition = transform.position;
    }

    // LateUpdate runs after the player controller's Update, so its movement is already done
    private void LateUpdate()
    {
        float bonus = upgrades.GetTotal(UpgradeType.MoveSpeed);
        // Keep curses from freezing the player
        bonus = Mathf.Max(bonus, minimumMultiplier - 1f);

        // How far the controller moved the player this frame (ignore up/down so jumps and gravity aren't boosted)
        Vector3 moved = transform.position - lastPosition;
        moved.y = 0f;

        // Add (or remove, for curses) a percentage of that movement
        if (bonus != 0f && characterController != null && moved.sqrMagnitude > 0.000001f)
        {
            Vector3 extra = moved * bonus;
            // CharacterController.isGrounded only reflects the LAST Move call. If the player was on the
            // ground, push down slightly so our extra move still counts as grounded. Otherwise the player
            // controller would think we're in the air next frame (no jumping, no footsteps).
            if (characterController.isGrounded)
                extra.y = -0.05f;
            characterController.Move(extra);
        }

        lastPosition = transform.position;
    }
}
