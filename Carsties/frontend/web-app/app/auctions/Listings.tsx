'use client'
import React, {useEffect, useState} from 'react'
import AuctionCard from "@/app/auctions/AuctionCard";
import AppPagination from "@/app/components/AppPagination";
import {getData} from "@/app/actions/auctionActions";
import Filters from "@/app/auctions/Filters";
import {useParamsStore} from "@/hooks/useParamsStore";
import {useShallow} from "zustand/react/shallow";
import qs from 'query-string'
import EmptyFilter from "@/app/components/EmptyFilter";
import {useAuctionStore} from "@/hooks/useAuctionStore";


export default function Listings() {
    // const [auctions,setAuctions] = useState<Auction[]>([]);
    // const [pageCount,setPageCount] = useState(0);
    // const [pageNumber,setPageNumber] = useState(1);
    // const [pageSize, setPageSize] = useState(4);
    //const [data,setData] = useState<PagedResult<Auction>>();
    const [loading,setLoading] = useState<boolean>(true);

    const params = useParamsStore(useShallow (state => ({
        pageNumber: state.pageNumber,
        pageSize: state.pageSize,
        searchTerm: state.searchTerm,
        orderBy: state.orderBy,
        filterBy: state.filterBy,
        seller: state.seller,
        winner: state.winner
    })));

    const data = useAuctionStore(useShallow(state => ({
        auctions: state.auctions,
        totalCount: state.totalCount,
        pageCount: state.pageCount
    })));

    const setData = useAuctionStore(state => state.setData);
    const setParams = useParamsStore(state =>state.setParams);
    const url = qs.stringifyUrl({url:'',query:params});

    function setPageNumber(pageNumber: number) {
        setParams({pageNumber: pageNumber});
    }

    useEffect(() => {
        getData(url).then(data=>{
            // setAuctions(data.results);
            // setPageCount(data.pageCount);
            setData(data);
            setLoading(false);
        })
    },[url,setData])

    if(loading){
        return <h3>Loading...</h3>
    }

    return (
        <>
            <Filters />
            {data.totalCount  === 0 ? (
                <EmptyFilter showReset/>
            ):(
                <>
                    <div className="grid grid-cols-4 gap-6">
                        {data.auctions.map((auction) => (
                            <AuctionCard key={auction.id} auction={auction}/>
                        ))}
                    </div>
                    <div className="flex justify-center mt-4">
                        <AppPagination currentPage={params.pageNumber} pageCount={data.pageCount}
                                       pageChanged={setPageNumber}/>
                    </div>
                </>
            )
            }
        </>
    )
}
