using UnityEngine;

public class PlayerVFX : MonoBehaviour
{
    public VFX vfx;

    private void Start()
    {
        vfx.DeactivateAllVFX();
    }

    public void ActivateVFX(ParticleSystem vfxToActivate)
    {
        if (vfxToActivate != null && !vfxToActivate.isPlaying)
        {
            vfxToActivate.Play();
        }
    }

    public void DeactivateVFX(ParticleSystem vfxToDeactivate)
    {
        if (vfxToDeactivate != null && vfxToDeactivate.isPlaying)
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

    public ParticleSystem Dust => dust;
    public ParticleSystem Cobweb => cobweb;

    public void DeactivateAllVFX()
    {
        if (dust != null) dust.Stop();
        if (cobweb != null) cobweb.Stop();
    }
}