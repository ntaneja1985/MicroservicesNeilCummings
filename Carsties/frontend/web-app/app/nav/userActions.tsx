'use client'
import React from 'react'
import { Dropdown, DropdownDivider, DropdownItem} from "flowbite-react";
import Link from "next/link";
import {User} from 'next-auth'
//import {useRouter} from "next/router";
import {HiCog, HiUser} from "react-icons/hi";
import {AiFillCar, AiFillTrophy, AiOutlineLogout} from "react-icons/ai";
import {signOut} from "next-auth/react";
import {usePathname, useRouter} from "next/navigation";
import {useParamsStore} from "@/hooks/useParamsStore";


type Props = {
    user: User
}
export default function UserActions({user}:Props) {
    const router = useRouter();
    const pathName = usePathname();
    const setParams = useParamsStore(state => state.setParams);

    function setWinner(){
        setParams({winner: user.username, seller: undefined});
        if(pathName !== '/'){
            router.push('/');
        }
    }

    function setSeller(){
        setParams({seller: user.username, winner: undefined});
        if(pathName !== '/'){
            router.push('/');
        }
    }

    return (
       <Dropdown inline label = {`Welcome ${user.name}`} >
            <DropdownItem icon = {HiUser} onClick={setSeller}>
                    My Auctions
            </DropdownItem>
           <DropdownItem icon = {AiFillTrophy} onClick={setWinner}>
                   Auctions Won
           </DropdownItem>
           <DropdownItem icon = {AiFillCar}>
               <Link href="/auctions/create">
                   Sell My Car
               </Link>
           </DropdownItem>
           <DropdownItem icon = {HiCog}>
               <Link href="/session">
                   Session (dev)
               </Link>
           </DropdownItem>
           <DropdownDivider/>
           <DropdownItem icon = {AiOutlineLogout} onClick={()=> signOut({redirectTo:'/'})}>
              Sign Out
           </DropdownItem>
       </Dropdown>
    )
}
