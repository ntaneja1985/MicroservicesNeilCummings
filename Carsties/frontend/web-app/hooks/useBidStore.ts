import {Bid} from "@/types";
import {create} from "zustand/react";

type State = {
    bids : Bid[]
    open: boolean
}

type Actions = {
    setBids: (bids : Bid[]) => void
    addBid: (bid: Bid) => void
    setOpen: (value:boolean) => void
}

export const useBidStore = create<State & Actions>((set)=>({
    bids:[],
    open: true,
    setBids:(bids:Bid[]) =>{
        set(()=>({
            bids:bids
        }))
    },
    addBid:(bid: Bid) =>{
        set((state)=>({
            //Check if the bid already exists in the list of bids, if not add it to the top of the bids[] else just return existing list of bids
            bids: !state.bids.find(x=>x.id==bid.id) ? [bid,...state.bids]: [...state.bids]
    }))
    },
    setOpen:(value:boolean) =>{
        set(()=>({
            open: value
        }))
    }
}))