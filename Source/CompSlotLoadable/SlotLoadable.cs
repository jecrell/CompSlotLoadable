using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace CompSlotLoadable
{
    public class SlotLoadable : Thing, IThingContainerOwner
    {
        #region Variables

        //Exposable Variables
        private Thing slotOccupant;
        private ThingContainer slot;

        //Settable variables
        public List<ThingDef> slottableThingDefs;
        //
        //Spawn variables
        public Thing owner;

        #endregion Variables
        
        //Spawn methods
        public SlotLoadable()
        {

        }

        public SlotLoadable(Thing newOwner)
        {
            Log.Message("Slot started");
            SlotLoadableDef def = this.def as SlotLoadableDef;
            this.slottableThingDefs = def.slottableThingDefs;
            owner = newOwner;
            ThingIDMaker.GiveIDTo(this);
            slot = new ThingContainer(this, false, LookMode.Deep);
        }

        public SlotLoadable(SlotLoadableDef xmlDef, Thing newOwner)
        {
            Log.Message("Slot Loaded");
            this.def = xmlDef;
            this.slottableThingDefs = xmlDef.slottableThingDefs;
            owner = newOwner;
            ThingIDMaker.GiveIDTo(this);
            slot = new ThingContainer(this, false, LookMode.Deep);
        }

        public Texture2D SlotIcon()
        {
            if (slotOccupant != null)
            {
                if (slotOccupant.def != null)
                {
                    return slotOccupant.def.uiIcon;
                }
            }
            return null;
        }

        public Color SlotColor()
        {
            if (slotOccupant != null)
            {
                if (slotOccupant.def != null)
                {
                    return slotOccupant.def.graphic.Color;
                }
            }
            return Color.white;
        }

        public bool IsEmpty()
        {
            if (slotOccupant != null) return false;
            return true;
        }

        public bool CanLoad(ThingDef defType)
        {
            if (this.slottableThingDefs != null)
            {
                if (this.slottableThingDefs.Count > 0)
                {
                    if (this.slottableThingDefs.Contains(defType))
                    {
                        //Log.Message("Can Load: " + defType.ToString());
                        return true;
                    }
                }
            }
            return false;
        }

        #region IThingContainerOwner

        public Map GetMap()
        {
            return ParentMap;
        }

        public ThingContainer GetInnerContainer()
        {
            return slot;
        }

        public IntVec3 GetPosition()
        {
            return ParentLoc;
        }
        #endregion IThingContainerOwner

        #region Properties
        //Get methods
        public Thing SlotOccupant
        {
            get
            {
                return slotOccupant;
            }
            set
            {
                slotOccupant = value;
            }
        }
        public ThingContainer Slot
        {
            get
            {
                return slot;
            }
            set
            {
                slot = value;
            }
        }

        public Pawn ParentHolder
        {
            get
            {
                Pawn result = null;
                if (owner != null)
                {
                    CompEquippable eq = owner.TryGetComp<CompEquippable>();
                    if (eq != null)
                    {
                        if (eq.PrimaryVerb != null)
                        {
                            Pawn pawn = eq.PrimaryVerb.CasterPawn;
                            if (pawn != null)
                            {
                                if (pawn.Spawned)
                                {
                                    result = pawn;
                                }
                            }
                        }
                    }
                }
                return result;
            }
        }
        
        public Map ParentMap
        {
            get
            {
                Map result = null;
                //Does our parent have an equippable class?
                //Use that to find a pawn location if it's equipped.
                if (owner != null)
                {
                    if (ParentHolder != null)
                    {
                        return ParentHolder.Map;
                    }
                    return owner.Map;
                }
                return result;
            }
        }

        public IntVec3 ParentLoc
        {
            get
            {
                IntVec3 result = IntVec3.Invalid;
                //Does our parent have an equippable class?
                //Use that to find a pawn location if it's equipped.
                if (owner != null)
                {
                    if (ParentHolder != null)
                    {
                        return ParentHolder.Position;
                    }
                    return owner.Position;
                }
                return result;
            }
        }

        public List<ThingDef> SlottableTypes
        {
            get
            {
                return this.slottableThingDefs;
            }
        }

        #endregion Properties

        #region Methods

        public virtual bool TryLoadSlot(Thing thingToLoad, bool emptyIfFilled = false)
        {
            //Log.Message("TryLoadSlot Called");
            if ((slotOccupant != null && emptyIfFilled) || slotOccupant == null)
            {
                TryEmptySlot();
                if (thingToLoad != null)
                {
                    if (slottableThingDefs != null)
                    {
                        if (slottableThingDefs.Contains(thingToLoad.def))
                        {
                            slotOccupant = thingToLoad;
                            slot.TryAdd(thingToLoad, false);
                            if (((SlotLoadableDef)def).doesChangeColor)
                            {
                                owner.Notify_ColorChanged();
                            }
                            return true;
                        }
                    }
                }
            }
            else
            {
                Messages.Message("ExceptionSlotAlreadyFilled".Translate(new object[]{
                    owner.Label
                }), MessageSound.RejectInput);
            }
            return false;
        }

        public virtual bool TryEmptySlot()
        {
            if (!CanEmptySlot()) return false;
            if (slot.TryDropAll(ParentLoc, ParentMap, ThingPlaceMode.Near))
            {
                slotOccupant = null;
            }
            return true;
        }

        public virtual bool CanEmptySlot()
        {
            return true;
        }

        #endregion Methods

        public override void ExposeData()
        {

            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                if (this.thingIDNumber == -1)
                {
                    ThingIDMaker.GiveIDTo(this);
                }
            }
            Scribe_Deep.LookDeep<ThingContainer>(ref this.slot, "slot", new object[]
            {
                this
            });
            Scribe_Collections.LookList<ThingDef>(ref this.slottableThingDefs, "slottableThingDefs", LookMode.Undefined, new object[0]);
            Scribe_References.LookReference<Thing>(ref this.owner, "owner");
            Scribe_References.LookReference<Thing>(ref this.slotOccupant, "slotOccupant");
        }
    }
}
