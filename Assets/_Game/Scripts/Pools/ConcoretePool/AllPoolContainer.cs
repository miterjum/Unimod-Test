using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Common;

public class AllPoolContainer : PoolsContainer<AllPool,AllPoolContainer>
{
    public AllPoolContainer(Transform pContainer) : base(pContainer)
    {
    }
}
