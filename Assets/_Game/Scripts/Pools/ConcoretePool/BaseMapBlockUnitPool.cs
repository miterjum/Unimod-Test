using UnityEngine;
using Utilities.Common;

public class BaseMapBlockUnitPool : CustomPool<BasePoolUnit>
{
    public BaseMapBlockUnitPool(BasePoolUnit pPrefab, int pInitialCount, Transform pParent, bool pBuildinPrefab, string pName = "", bool pAutoRelocate = true) : base(pPrefab, pInitialCount, pParent, pBuildinPrefab, pName, pAutoRelocate)
    {
    }
}