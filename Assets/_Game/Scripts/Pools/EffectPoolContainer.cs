using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Common;

public class EffectPoolContainer : PoolsContainer<ParticleSystem, EffectPoolContainer>
{
    public EffectPoolContainer(Transform pContainer) : base(pContainer)
    {
    }
}
