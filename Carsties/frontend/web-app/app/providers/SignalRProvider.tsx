'use client'
import React, {useCallback, useEffect, useRef} from 'react'
import {HubConnection, HubConnectionBuilder} from "@microsoft/signalr";
import {useAuctionStore} from "@/hooks/useAuctionStore";
import {useBidStore} from "@/hooks/useBidStore";
import {useParams} from "next/navigation";
import {Auction, AuctionFinished, Bid} from "@/types";
import {User} from "next-auth";
import AuctionCreatedToast from "@/app/components/AuctionCreatedToast";
import toast from "react-hot-toast";
import {getDetailedViewData} from "@/app/actions/auctionActions";
import AuctionFinishedToast from "@/app/components/AuctionFinishedToast";

type Props = {
    children: React.ReactNode
    user: User | null
}
export default function SignalRProvider({user,children}: Props) {
    const connection = useRef<HubConnection | null>(null);
    const setCurrentPrice = useAuctionStore(state => state.setCurrentPrice);
    const addBid = useBidStore(state => state.addBid);
    const params = useParams<{id:string}>();

    const handleAuctionCreated = useCallback((auction:Auction) => {
        if(user?.username !== auction.seller)
        {
            return toast(<AuctionCreatedToast auction={auction} />,{
                duration: 10000,
            })
        }
    },[user?.username])

    const handleAuctionFinished = useCallback((finishedAuction:AuctionFinished) => {
        const auction = getDetailedViewData(finishedAuction.auctionId);
        return toast.promise(auction, {
            loading:'loading',
            success: (auction) =>
                <AuctionFinishedToast
                    finishedAuction={finishedAuction} auction={auction} />
            , error: (err) => 'Auction Finished'
        },{success: {duration:10000,icon:null}})
    },[])

    const handleBidPlaced = useCallback((bid:Bid) =>{
        if(bid.bidStatus.includes('Accepted')){
            setCurrentPrice(bid.auctionId, bid.amount);
        }

        //check what is there in the router parameters using the useParams hook
        if(params.id === bid.auctionId)
        {
            addBid(bid);
        }
    },[setCurrentPrice,addBid,params.id]);


    useEffect(() => {
        if(!connection.current)
        {
            connection.current = new HubConnectionBuilder()
                .withUrl('http://localhost:6001/notifications')
                .withAutomaticReconnect()
                .build();
            connection.current.start()
                .then(()=> 'Connected to notification hub')
                .catch(err => console.error(err));
        }
        connection.current.on('BidPlaced',handleBidPlaced);
        connection.current.on('AuctionCreated',handleAuctionCreated);
        connection.current.on('AuctionFinished',handleAuctionFinished);

        //Cleanup the connection
        return () => {
            connection.current?.off('BidPlaced',handleBidPlaced);
            connection.current?.off('AuctionCreated',handleAuctionCreated);
            connection.current?.off('AuctionFinished',handleAuctionFinished);
        }
    },[handleBidPlaced,handleAuctionCreated,handleAuctionFinished])

    return (
        children
    )
}
