using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CompSlotLoadable
{
    public class CompProperties_SlottedBonus : CompProperties
    {
        public List<StatModifier> statModifiers = null;

        public DamageDef damageDef = null;

        public Color color = Color.white;

        public CompProperties_SlottedBonus()
        {
            this.compClass = typeof(CompSlottedBonus);
        }
    }
}
