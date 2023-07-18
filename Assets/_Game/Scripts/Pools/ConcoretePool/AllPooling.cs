using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Common;

public class AllPooling : CustomPool<AllPool>
{
    public AllPooling(AllPool pPrefab, int pInitialCount, Transform pParent, bool pBuildinPrefab, string pName = "", bool pAutoRelocate = true) : base(pPrefab, pInitialCount, pParent, pBuildinPrefab, pName, pAutoRelocate) { }
}
