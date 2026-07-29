using UnityEngine;

public class PlayerVFX : MonoBehaviour
{
    private PlayerRole playerRole;

    [SerializeField] private bool isHero;
    public VFX vfx;

    private void Awake()
    {
        playerRole = GetComponent<PlayerRole>();

        if (playerRole == null)
            Debug.LogError("PlayerRole component not found on the player object.");
    }

    private void OnEnable()
    {
        playerRole.OnRoleChanged += HandleRoleChanged;

    }
    private void OnDisable()
    {
        playerRole.OnRoleChanged -= HandleRoleChanged;
    }

    private void Start()
    {
        vfx.DeactivateAllVFX();
    }

    private void HandleRoleChanged(Role newRole)
    {
        if (newRole == Role.Hero)
        {
            isHero = true;
        }
        else if (newRole == Role.Banner)
        {
            isHero = false;
        }
    }

    public void ActivateVFX(ParticleSystem vfxToActivate)
    {
        if (vfxToActivate != null)
        {
            vfxToActivate.Play();
        }
    }
    public void DeactivateVFX(ParticleSystem vfxToDeactivate)
    {
        if (vfxToDeactivate != null)
        {
            vfxToDeactivate.Stop();
        }
    }
}

[System.Serializable]
public class VFX
{
    [SerializeField] private ParticleSystem dust;
    [SerializeField] private ParticleSystem cobweb;
    [SerializeField] private ParticleSystem speedUp;

    public ParticleSystem Dust => dust;
    public ParticleSystem Cobweb => cobweb;
    public ParticleSystem SpeedUp => speedUp;

    public void DeactivateAllVFX()
    {
        if (dust != null) dust.Stop();
        if (cobweb != null) cobweb.Stop();
        if (speedUp != null) speedUp.Stop();
    }
}