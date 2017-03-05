﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CompSlotLoadable
{
    public class CompSlotLoadable : ThingComp
    {
        public bool GizmosOnEquip = true;

        private List<SlotLoadable> slots = new List<SlotLoadable>();
        public List<SlotLoadable> Slots
        {
            get
            {
                return slots;
            }
        }
        private SlotLoadable colorChangingSlot = null;
        public SlotLoadable ColorChangingSlot
        {
            get
            {
                if (colorChangingSlot != null) return colorChangingSlot;
                if (this.Slots != null)
                {
                    if (this.Slots.Count > 0)
                    {
                        colorChangingSlot = this.Slots.FirstOrDefault((SlotLoadable x) => ((SlotLoadableDef)(x.def)).doesChangeColor);
                    }
                }
                return colorChangingSlot;

            }
        }


        private SlotLoadable secondColorChangingSlot = null;
        public SlotLoadable SecondColorChangingSlot
        {
            get
            {
                if (secondColorChangingSlot != null) return secondColorChangingSlot;
                if (this.Slots != null)
                {
                    if (this.Slots.Count > 0)
                    {
                        secondColorChangingSlot = this.Slots.FirstOrDefault((SlotLoadable x) => ((SlotLoadableDef)(x.def)).doesChangeSecondColor);
                    }
                }
                return colorChangingSlot;

            }
        }

        public List<SlotLoadableDef> SlotDefs
        {
            get
            {
                List<SlotLoadableDef> result = new List<SlotLoadableDef>();
                if (slots != null)
                {
                    if (slots.Count > 0)
                    {
                        foreach (SlotLoadable slot in slots)
                        {
                            result.Add(slot.def as SlotLoadableDef);
                        }
                    }
                }
                return result;
            }
        }

        private bool isInitialized = false;

        private bool isGathering = false;

        public Map GetMap
        {
            get
            {
                Map map = this.parent.Map;
                if (map == null)
                {
                    if (GetPawn != null) map = GetPawn.Map;
                }
                return map;
            }
        }

        public CompEquippable GetEquippable
        {
            get
            {
                return this.parent.GetComp<CompEquippable>();
            }
        }

        public Pawn GetPawn
        {
            get
            {
                return GetEquippable.verbTracker.PrimaryVerb.CasterPawn;
            }
        }

        public void Initialize()
        {
            //Log.Message("1");
            if (!isInitialized)
            {

                //Log.Message("2");
                isInitialized = true;
                if (this.Props != null)
                {

                    //Log.Message("3");
                    if (this.Props.slots != null)
                    {

                        //Log.Message("4");
                        if (this.Props.slots.Count > 0)
                        {

                            //Log.Message("5");
                            foreach (SlotLoadableDef slot in this.Props.slots)
                            {
                                SlotLoadable newSlot = new SlotLoadable(slot, this.parent);
                                //Log.Message("Added Slot");
                                slots.Add(newSlot);
                            }
                        }
                    }
                }
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!isInitialized) Initialize();
        }

        private void TryCancel(string reason = "")
        {
            Pawn pawn = GetPawn;
            if (pawn != null)
            {
                if (pawn.CurJob.def == CompSlotLoadableDefOf.GatherSlotItem)
                {
                    pawn.jobs.StopAll();
                }
                isGathering = false;
                //Messages.Message("Cancelling sacrifice. " + reason, MessageSound.Negative);
            }
        }

        private void TryGiveLoadSlotJob(Thing itemToLoad)
        {
            if (GetPawn != null)
            {
                if (!GetPawn.Drafted)
                {
                    isGathering = true;

                    Job job = new Job(CompSlotLoadableDefOf.GatherSlotItem, itemToLoad);
                    job.count = 1;
                    GetPawn.QueueJob(job);
                    GetPawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }
                else Messages.Message("IsDrafted".Translate(new object[]
                    {
                        GetPawn.Label
                    }), MessageSound.RejectInput);
            }
        }

        public bool TryLoadSlot(Thing thing)
        {
            Log.Message("TryLoadSlot Called");
            isGathering = false;
            if (slots != null)
            {
                if (slots.Count > 0)
                {
                    SlotLoadable loadSlot = slots.FirstOrDefault((SlotLoadable x) => x.IsEmpty() && x.CanLoad(thing.def));
                    if (loadSlot == null) loadSlot = slots.FirstOrDefault((SlotLoadable y) => y.CanLoad(thing.def));
                    if (loadSlot != null)
                    {
                        loadSlot.TryLoadSlot(thing, true);
                        return true;
                    }
                }
            }
            return false;
        }

        public void ProcessInput(SlotLoadable slot)
        {
            List<ThingDef> loadTypes = new List<ThingDef>();
            List<FloatMenuOption> floatList = new List<FloatMenuOption>();
            if (!isGathering)
            {
                Map map = GetMap;
                loadTypes = slot.SlottableTypes;
                if (slot.SlotOccupant == null)
                {
                    if (loadTypes != null)
                    {
                        if (loadTypes.Count != 0)
                        {
                            foreach (ThingDef current in loadTypes)
                            {
                                List<Thing> thingsWithDef = new List<Thing>(map.listerThings.AllThings.FindAll((Thing x) => x.def == current));
                                if (thingsWithDef != null)
                                {
                                    if (thingsWithDef.Count > 0)
                                    {
                                        Thing thingToLoad = thingsWithDef.FirstOrDefault((Thing x) => map.reservationManager.CanReserve(GetPawn, x));
                                        if (thingToLoad != null)
                                        {
                                            string text = "Load".Translate() + " " + thingToLoad.def.label;
                                            //Func<Rect, bool> extraPartOnGUI = (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, current);
                                            floatList.Add(new FloatMenuOption(text, delegate
                                            {
                                                this.TryGiveLoadSlotJob(thingToLoad);
                                            }, MenuOptionPriority.Default, null, null, 29f, null, null));
                                        }
                                        else
                                        {
                                            floatList.Add(new FloatMenuOption(current.label + " " + "Unavailable".Translate(), delegate
                                            {
                                            }, MenuOptionPriority.Default));
                                        }
                                    }
                                    else
                                    {
                                        floatList.Add(new FloatMenuOption(current.label + " " + "Unavailable".Translate(), delegate
                                        {
                                        }, MenuOptionPriority.Default));
                                    }
                                }
                                else
                                {
                                    floatList.Add(new FloatMenuOption(current.label + " " + "Unavailable".Translate(), delegate
                                    {
                                    }, MenuOptionPriority.Default));
                                }
                            }
                        }
                        else
                        {
                            floatList.Add(new FloatMenuOption("NoLoadOptions".Translate(), delegate
                            {
                            }, MenuOptionPriority.Default));
                        }
                    }
                }
            }
            else
            {
                //TryCancel();
            }
            if (!slot.IsEmpty())
            {
                string text = "Unload".Translate() + " " + slot.SlotOccupant.Label;
                //Func<Rect, bool> extraPartOnGUI = (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, current);
                floatList.Add(new FloatMenuOption(text, delegate
                {
                    slot.TryEmptySlot();
                }, MenuOptionPriority.Default, null, null, 29f, null, null));
            }
            Find.WindowStack.Add(new FloatMenu(floatList));
        }

        public virtual IEnumerable<Gizmo> EquippedGizmos()
        {
            if (slots != null)
            {
                if (slots.Count > 0)
                {
                    if (isGathering)
                    {
                        yield return new Command_Action
                        {
                            defaultLabel = "DesignatorCancel".Translate(),
                            defaultDesc = "DesignatorCancelDesc".Translate(),
                            icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true),
                            action = delegate
                            {
                                this.TryCancel();
                            }
                        };
                    }
                    foreach (SlotLoadable slot in slots)
                    {
                        if (slot.IsEmpty())
                        {
                            yield return new Command_Action
                            {
                                defaultLabel = slot.Label,
                                icon = Command.BGTex,
                                defaultDesc = SlotDesc(slot),
                                action = delegate
                                {
                                    this.ProcessInput(slot);
                                }
                            };
                        }
                        else
                        {
                            yield return new Command_Action
                            {
                                defaultLabel = slot.Label,
                                icon = slot.SlotIcon(),
                                defaultDesc = SlotDesc(slot),
                                defaultIconColor = slot.SlotColor(),
                                action = delegate
                                {
                                    this.ProcessInput(slot);
                                }
                            };
                        }
                    }
                }
                
            }
            yield break;
        }

        public virtual string SlotDesc(SlotLoadable slot)
        {
            StringBuilder s = new StringBuilder();
            s.AppendLine(slot.GetDescription());
            if (!slot.IsEmpty())
            {
                s.AppendLine();
                s.AppendLine("CurrentlyLoaded".Translate() + ": " + slot.SlotOccupant.LabelCap);
                s.AppendLine();
                s.AppendLine("Effects".Translate() + ":");
                if (((SlotLoadableDef)slot.def).doesChangeColor)
                {
                    s.AppendLine("\t" + "ChangesPrimaryColor".Translate());
                }
                if (((SlotLoadableDef)slot.def).doesChangeStats)
                {
                    
                    if (slot.SlotOccupant.def.statBases != null && slot.SlotOccupant.def.statBases.Count > 0)
                    {
                        List<StatModifier> statMods = slot.SlotOccupant.def.statBases.FindAll(
                            (StatModifier z) => z.stat.category == StatCategoryDefOf.Weapon ||
                                                z.stat.category == StatCategoryDefOf.EquippedStatOffsets);
                        if (statMods != null && statMods.Count > 0)
                        {
                            s.AppendLine();
                            s.AppendLine("StatModifiers".Translate() + ":");
                            foreach (StatModifier mod in statMods)
                            {
                                s.AppendLine("\t" + mod.stat.LabelCap + " " + mod.ToStringAsOffset);
                            }
                        }
                    }

                }
            }
            return s.ToString();
        }

        public override void PostExposeData()
        {
            Scribe_Values.LookValue<bool>(ref this.isInitialized, "isInitialized", false);
            Scribe_Values.LookValue<bool>(ref this.isGathering, "isGathering", false);
            Scribe_Collections.LookList<SlotLoadable>(ref this.slots, "slots", LookMode.Deep, new object[0]);
            base.PostExposeData();
            if (slots == null)
            {
                slots = new List<SlotLoadable>();
            }
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                //Scribe.writingForDebug = false;
            }
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                //Scribe.writingForDebug = true;
            }
        }


        public CompProperties_SlotLoadable Props
        {
            get
            {
                return (CompProperties_SlotLoadable)this.props;
            }
        }

    }
}