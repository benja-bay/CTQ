using UnityEngine;

public class PlayerVFX : MonoBehaviour
{
    public VFX vfx;

    private bool _isVfxActive;

    private void Start()
    {
        vfx.DeactivateAllVFX();
    }

    public void ActivateVFX(ParticleSystem vfxToActivate)
    {
        if (vfxToActivate != null && !_isVfxActive)
        {
            vfxToActivate.Play();
            _isVfxActive = true;
        }
    }
    public void DeactivateVFX(ParticleSystem vfxToDeactivate)
    {
        if (vfxToDeactivate != null && _isVfxActive)
        {
            vfxToDeactivate.Stop();
            _isVfxActive = false;
        }
    }
}

[System.Serializable]
public class VFX
{
    [SerializeField] private ParticleSystem dust;
    [SerializeField] private ParticleSystem cobweb;

    public ParticleSystem Dust => dust;
    public ParticleSystem Cobweb => cobweb;

    public void DeactivateAllVFX()
    {
        if (dust != null) dust.Stop();
        if (cobweb != null) cobweb.Stop();
    }
}