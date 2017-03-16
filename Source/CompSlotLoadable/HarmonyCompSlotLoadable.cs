﻿using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using System.Reflection;
using UnityEngine;
using Verse.Sound;

namespace CompSlotLoadable
{
    [StaticConstructorOnStartup]
    static class HarmonyCompSlotLoadable
    {
        static HarmonyCompSlotLoadable()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.jecrell.comps.slotloadable");

            harmony.Patch(AccessTools.Method(typeof(Pawn), "GetGizmos"), null, new HarmonyMethod(typeof(HarmonyCompSlotLoadable).GetMethod("GetGizmosPrefix")));
            //harmony.Patch(AccessTools.Method(typeof(Thing), "get_Graphic"), null, new HarmonyMethod(typeof(HarmonyCompSlotLoadable).GetMethod("get_Graphic_PostFix")));
            harmony.Patch(AccessTools.Method(typeof(StatExtension), "GetStatValue"), null, new HarmonyMethod(typeof(HarmonyCompSlotLoadable).GetMethod("GetValue_PostFix")));
            harmony.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"), null, new HarmonyMethod(typeof(HarmonyCompSlotLoadable).GetMethod("AddHumanlikeOrders_PostFix")));
            harmony.Patch(AccessTools.Method(typeof(Verb_MeleeAttack), "DamageInfosToApply"), null, new HarmonyMethod(typeof(HarmonyCompSlotLoadable).GetMethod("DamageInfosToApply_PostFix")), null);
            harmony.Patch(AccessTools.Method(typeof(ITab_Pawn_Gear), "DrawThingRow"), null, new HarmonyMethod(typeof(HarmonyCompSlotLoadable).GetMethod("DrawThingRow_PostFix")), null);


            //Color postfixes
            //harmony.Patch(typeof(ThingWithComps).GetMethod("get_DrawColor"), null, new HarmonyMethod(typeof(HarmonyCompSlotLoadable).GetMethod("DrawColorPostFix")));
            //harmony.Patch(typeof(Thing).GetMethod("get_DrawColorTwo"), null, new HarmonyMethod(typeof(HarmonyCompSlotLoadable).GetMethod("DrawColorTwoPostFix")));
        }


        private static readonly Color HighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        private static readonly Color ThingLabelColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        //=================================== COMPSLOTLOADABLE

        public static void DrawThingRow_PostFix(ITab_Pawn_Gear __instance, ref float y, float width, Thing thing, bool showDropButtonIfPrisoner = false)
        {
            //Log.Message("1");
            ThingWithComps thingWithComps = thing as ThingWithComps;
            if (thingWithComps != null)
            {
                ThingComp comp = thingWithComps.AllComps.FirstOrDefault((ThingComp x) => x is CompSlotLoadable);
                if (comp != null)
                {
                    CompSlotLoadable compSlotLoadable = comp as CompSlotLoadable;
                    if (compSlotLoadable.Slots != null && compSlotLoadable.Slots.Count > 0)
                    {
                        foreach (SlotLoadable slot in compSlotLoadable.Slots)
                        {
                            if (!slot.IsEmpty())
                            {
                                Rect rect = new Rect(0f, y, width, 28f);
                                Widgets.InfoCardButton(rect.width - 24f, y, slot.SlotOccupant);
                                rect.width -= 24f;
                                //bool CanControl = (bool)AccessTools.Method(typeof(ITab_Pawn_Gear), "get_CanControl").Invoke(__instance, null);
                                if (Mouse.IsOver(rect))
                                {
                                    GUI.color = HarmonyCompSlotLoadable.HighlightColor;
                                    GUI.DrawTexture(rect, TexUI.HighlightTex);
                                }
                                if (slot.SlotOccupant.def.DrawMatSingle != null && slot.SlotOccupant.def.DrawMatSingle.mainTexture != null)
                                {
                                    Widgets.ThingIcon(new Rect(4f, y, 28f, 28f), slot.SlotOccupant, 1f);
                                }
                                Text.Anchor = TextAnchor.MiddleLeft;
                                GUI.color = HarmonyCompSlotLoadable.ThingLabelColor;
                                Rect rect4 = new Rect(36f, y, width - 36f, 28f);
                                string text = slot.SlotOccupant.LabelCap;
                                Widgets.Label(rect4, text);
                                y += 28f;
                            }
                        }
                    }
                }
            }
        }


        // RimWorld.Verb_MeleeAttack
        public static void DamageInfosToApply_PostFix(Verb_MeleeAttack __instance, ref IEnumerable<DamageInfo> __result, LocalTargetInfo target)
        {
            List<DamageInfo> newList = new List<DamageInfo>();
            //__result = null;
            ThingWithComps ownerEquipment = __instance.ownerEquipment;
            if (ownerEquipment != null)
            {

                //Log.Message("1");
                ThingComp comp = ownerEquipment.AllComps.FirstOrDefault((ThingComp x) => x is CompSlotLoadable);
                if (comp != null)
                {

                    //Log.Message("2");
                    CompSlotLoadable compSlotLoadable = comp as CompSlotLoadable;
                    if (compSlotLoadable.Slots != null && compSlotLoadable.Slots.Count > 0)
                    {

                        //Log.Message("3");
                        List<SlotLoadable> statSlots = compSlotLoadable.Slots.FindAll((SlotLoadable z) => !z.IsEmpty() && ((SlotLoadableDef)z.def).doesChangeStats == true);
                        if (statSlots != null && statSlots.Count > 0)
                        {

                            //Log.Message("4");
                            foreach (SlotLoadable slot in statSlots)
                            {

                                //Log.Message("5");
                                CompSlottedBonus slotBonus = slot.SlotOccupant.TryGetComp<CompSlottedBonus>();
                                if (slotBonus != null)
                                {

                                    //Log.Message("6");
                                    Type superClass = __instance.GetType().BaseType;
                                    if (slotBonus.Props.damageDef != null)
                                    {

                                        //Log.Message("7");
                                        float num = (float)__instance.verbProps.AdjustedMeleeDamageAmount(__instance, __instance.CasterPawn, __instance.ownerEquipment);
                                        DamageDef def = __instance.verbProps.meleeDamageDef;
                                        BodyPartGroupDef weaponBodyPartGroup = null;
                                        HediffDef weaponHediff = null;
                                        if (__instance.CasterIsPawn)
                                        {

                                            //Log.Message("8");
                                            if (num >= 1f)
                                            {
                                                weaponBodyPartGroup = __instance.verbProps.linkedBodyPartsGroup;
                                                if (__instance.ownerHediffComp != null)
                                                {
                                                    weaponHediff = __instance.ownerHediffComp.Def;
                                                }
                                            }
                                            else
                                            {
                                                num = 1f;
                                                def = DamageDefOf.Blunt;
                                            }
                                        }

                                        //Log.Message("9");
                                        ThingDef def2;
                                        if (__instance.ownerEquipment != null)
                                        {
                                            def2 = __instance.ownerEquipment.def;
                                        }
                                        else
                                        {
                                            def2 = __instance.CasterPawn.def;
                                        }

                                        //Log.Message("10");
                                        Vector3 angle = (target.Thing.Position - __instance.CasterPawn.Position).ToVector3();

                                        //Log.Message("11");
                                        Thing caster = __instance.caster;

                                        //Log.Message("12");
                                        DamageInfo damageInfo = new DamageInfo(slotBonus.Props.damageDef, GenMath.RoundRandom(num), -1f, caster, null, def2);
                                        damageInfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
                                        damageInfo.SetWeaponBodyPartGroup(weaponBodyPartGroup);
                                        damageInfo.SetWeaponHediff(weaponHediff);
                                        damageInfo.SetAngle(angle);

                                        //Log.Message("13");
                                        newList.Add(damageInfo);

                                        __result = newList.AsEnumerable<DamageInfo>();
                                        //Log.Message("14");
                                        //bool surpriseAttack = (bool)AccessTools.Field(typeof(Verb_MeleeAttack), "surpriseAttack").GetValue(__instance);
                                        //if (surpriseAttack && __instance.verbProps.surpriseAttack != null && __instance.verbProps.surpriseAttack.extraMeleeDamages != null)
                                        //{
                                        //    List<ExtraMeleeDamage> extraMeleeDamages = __instance.verbProps.surpriseAttack.extraMeleeDamages;
                                        //    for (int i = 0; i < extraMeleeDamages.Count; i++)
                                        //    {
                                        //        ExtraMeleeDamage extraMeleeDamage = extraMeleeDamages[i];
                                        //        int amount = GenMath.RoundRandom((float)extraMeleeDamage.amount * __instance.GetDamageFactorFor(__instance.CasterPawn));
                                        //        caster = __instance.caster;
                                        //        DamageInfo damageInfo2 = new DamageInfo(extraMeleeDamage.def, amount, -1f, caster, null, def2);
                                        //        damageInfo2.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
                                        //        damageInfo2.SetWeaponBodyPartGroup(weaponBodyPartGroup);
                                        //        damageInfo2.SetWeaponHediff(weaponHediff);
                                        //        damageInfo2.SetAngle(angle);
                                        //        __result.Add(damageInfo2);
                                        //    }
                                        //}
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        public static void AddHumanlikeOrders_PostFix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            IntVec3 c = IntVec3.FromVector3(clickPos);

            ThingWithComps slotLoadable = pawn.equipment.AllEquipment.FirstOrDefault((ThingWithComps x) => x.TryGetComp<CompSlotLoadable>() != null);
            if (slotLoadable != null)
            {
                CompSlotLoadable compSlotLoadable = slotLoadable.GetComp<CompSlotLoadable>();
                if (compSlotLoadable != null)
                {
                    List<Thing> thingList = c.GetThingList(pawn.Map);
                    
                    foreach (SlotLoadable slot in compSlotLoadable.Slots)
                    {
                        Thing loadableThing = thingList.FirstOrDefault((Thing y) => slot.CanLoad(y.def));
                        if (loadableThing != null)
                        {
                            FloatMenuOption itemSlotLoadable;
                            string labelShort = loadableThing.Label;
                            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                            {
                                itemSlotLoadable = new FloatMenuOption("CannotEquip".Translate(new object[]
                                {
                    labelShort
                                }) + " (" + "Incapable".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                            }
                            else if (!pawn.CanReach(loadableThing, PathEndMode.ClosestTouch, Danger.Deadly))
                            {
                                itemSlotLoadable = new FloatMenuOption("CannotEquip".Translate(new object[]
                                {
                    labelShort
                                }) + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                            }
                            else if (!pawn.CanReserve(loadableThing, 1))
                            {
                                itemSlotLoadable = new FloatMenuOption("CannotEquip".Translate(new object[]
                                {
                    labelShort
                                }) + " (" + "ReservedBy".Translate(new object[]
                                {
                    pawn.Map.reservationManager.FirstReserverOf(loadableThing, pawn.Faction, true).LabelShort
                                }) + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                            }
                            else
                            {
                                string text2 = "Equip".Translate(new object[]
                                {
                    labelShort
                                });
                                //if (loadableThing.def.IsRangedWeapon && pawn.story != null && pawn.story.traits.HasTrait(TraitDefOf.Brawler))
                                //{
                                //    text2 = text2 + " " + "EquipWarningBrawler".Translate();
                                //}
                                itemSlotLoadable = new FloatMenuOption(text2, delegate
                                {
                                    loadableThing.SetForbidden(false, true);
                                    pawn.jobs.TryTakeOrderedJob(new Job(DefDatabase<JobDef>.GetNamed("GatherSlotItem"), loadableThing));
                                    MoteMaker.MakeStaticMote(loadableThing.DrawPos, loadableThing.Map, ThingDefOf.Mote_FeedbackEquip, 1f);
                                    //PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.EquippingWeapons, KnowledgeAmount.Total);
                                }, MenuOptionPriority.High, null, null, 0f, null, null);
                            }
                            opts.Add(itemSlotLoadable);
                        }
                    }

                    
                }
            }
        }

        public static void GetValue_PostFix(ref float __result, Thing thing, StatDef stat, bool applyPostProcess)
        {
            //Log.Message("1");
            ThingWithComps ownerEquipment = thing as ThingWithComps;
            if (ownerEquipment != null)
            {
                ThingComp comp = ownerEquipment.AllComps.FirstOrDefault((ThingComp x) => x is CompSlotLoadable);
                if (comp != null)
                {
                    CompSlotLoadable compSlotLoadable = comp as CompSlotLoadable;
                    if (compSlotLoadable.Slots != null && compSlotLoadable.Slots.Count > 0)
                    {
                        List<SlotLoadable> statSlots = compSlotLoadable.Slots.FindAll((SlotLoadable z) => !z.IsEmpty() && ((SlotLoadableDef)z.def).doesChangeStats == true);
                        if (statSlots != null && statSlots.Count > 0)
                        {
                            foreach (SlotLoadable slot in statSlots)
                            {
                                CompSlottedBonus slotBonus = slot.SlotOccupant.TryGetComp<CompSlottedBonus>();
                                if (slotBonus != null)
                                {
                                    if (slotBonus.Props != null)
                                    {
                                        if (slotBonus.Props.statModifiers != null && slotBonus.Props.statModifiers.Count > 0)
                                        {
                                            StatModifier thisStat = slotBonus.Props.statModifiers.FirstOrDefault(
                                                (StatModifier y) => y.stat == stat &&
                                                (y.stat.category == StatCategoryDefOf.Weapon ||
                                                y.stat.category == StatCategoryDefOf.EquippedStatOffsets
                                                ));
                                            if (thisStat != null)
                                            {
                                                __result += thisStat.value;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    } 

                }
            }
        }

        public static void get_Graphic_PostFix(Thing __instance, ref Graphic __result)
        {
            ThingWithComps thingWithComps = __instance as ThingWithComps;
            if (thingWithComps != null)
            {
                //Log.Message("3");
                CompSlotLoadable CompSlotLoadable = thingWithComps.GetComp<CompSlotLoadable>();
                if (CompSlotLoadable != null)
                {
                    //ThingComp activatableEffect = thingWithComps.AllComps.FirstOrDefault<ThingComp>((ThingComp y) => y.GetType().ToString() == "CompActivatableEffect.CompActivatableEffect");

                    SlotLoadable slot = CompSlotLoadable.ColorChangingSlot;
                    if (slot != null)
                    {
                        if (!slot.IsEmpty())
                        {
                            CompSlottedBonus slotBonus = slot.SlotOccupant.TryGetComp<CompSlottedBonus>();
                            if (slotBonus != null)
                            {
                                //if (activatableEffect != null)
                                //{
                                //    AccessTools.Field(activatableEffect.GetType(), "overrideColor").SetValue(activatableEffect, slot.SlotOccupant.DrawColor);
                                //    Log.ErrorOnce("GraphicPostFix_Called_Activatable", 1866);
                                //}
                                //else
                                //{
                                Graphic tempGraphic = (Graphic)AccessTools.Field(typeof(Thing), "graphicInt").GetValue(__instance);
                                if (tempGraphic != null)
                                {
                                    if (tempGraphic.Shader != null)
                                    {
                                        tempGraphic = tempGraphic.GetColoredVersion(tempGraphic.Shader, slotBonus.Props.color, slotBonus.Props.color); //slot.SlotOccupant.DrawColor;
                                        __result = tempGraphic;
                                        //Log.Message("SlotLoadableDraw");

                                    }
                                }
                            }
                            //Log.ErrorOnce("GraphicPostFix_Called_5", 1866);
                            //}
                        }
                    }
                }
            }

        }

        public static void DrawColorPostFix(ThingWithComps __instance, ref Color __result)
        {
            ThingWithComps thingWithComps = __instance as ThingWithComps;
            if (thingWithComps != null)
            {
                //Log.Message("3");
                CompSlotLoadable CompSlotLoadable = thingWithComps.GetComp<CompSlotLoadable>();
                if (CompSlotLoadable != null)
                {
                    SlotLoadable slot = CompSlotLoadable.ColorChangingSlot;
                    if (slot != null)
                    {
                        if (!slot.IsEmpty())
                        {  
                            __result = slot.SlotOccupant.DrawColor;
                            __instance.Graphic.color = slot.SlotOccupant.DrawColor;        
                        }
                    }
                }
            }
            
        }

        public static void DrawColorTwoPostFix(Thing __instance, ref Color __result)
        {
            ThingWithComps thingWithComps = __instance as ThingWithComps;
            if (thingWithComps != null)
            {
                //Log.Message("3");
                CompSlotLoadable CompSlotLoadable = thingWithComps.GetComp<CompSlotLoadable>();
                if (CompSlotLoadable != null)
                {
                    SlotLoadable slot = CompSlotLoadable.SecondColorChangingSlot;
                    if (slot != null)
                    {
                        if (!slot.IsEmpty())
                        {
                            __result = slot.SlotOccupant.DrawColor;
                            __instance.Graphic.colorTwo = slot.SlotOccupant.DrawColor;
                        }
                    }
                }
            }

        }

        public static IEnumerable<Gizmo> gizmoGetter(CompSlotLoadable CompSlotLoadable)
        {
            //Log.Message("5");
            if (CompSlotLoadable.GizmosOnEquip)
            {
                //Log.Message("6");
                //Iterate EquippedGizmos
                IEnumerator<Gizmo> enumerator = CompSlotLoadable.EquippedGizmos().GetEnumerator();
                while (enumerator.MoveNext())
                {
                    //Log.Message("7");
                    Gizmo current = enumerator.Current;
                    yield return current;
                }
            }
        }

        public static void GetGizmosPrefix(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            //Log.Message("1");
            Pawn_EquipmentTracker pawn_EquipmentTracker = __instance.equipment;
            if (pawn_EquipmentTracker != null)
            {
                //Log.Message("2");
                ThingWithComps thingWithComps = (ThingWithComps)AccessTools.Field(typeof(Pawn_EquipmentTracker), "primaryInt").GetValue(pawn_EquipmentTracker);

                if (thingWithComps != null)
                {
                    //Log.Message("3");
                    CompSlotLoadable CompSlotLoadable = thingWithComps.GetComp<CompSlotLoadable>();
                    if (CompSlotLoadable != null)
                    {
                        if (gizmoGetter(CompSlotLoadable).Count<Gizmo>() > 0)
                        { 
                            //Log.Message("4");
                            if (__instance != null)
                            {
                                if (__instance.Faction == Faction.OfPlayer)
                                {
                                    __result = __result.Concat<Gizmo>(gizmoGetter(CompSlotLoadable));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}