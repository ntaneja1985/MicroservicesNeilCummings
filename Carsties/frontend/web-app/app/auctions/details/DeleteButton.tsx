'use client'


import {useState} from "react";
import {Button} from "flowbite-react";
import {deleteAuction} from "@/app/actions/auctionActions";

import toast from "react-hot-toast";
import {useRouter} from "next/navigation";

type Props = {
    id:string;
}

export default function DeleteButton({id}: Props) {
    const [loading,setLoading] = useState(false);
    const router = useRouter();

     function doDelete() {
        setLoading(true);
         deleteAuction(id).then(res => {
             if(res.error) throw res.error;
             router.push('/');
         }).catch(err => toast.error(err.status + ' '+err.message))
             .finally(() => setLoading(false));
    }
    return (
        <Button color='failure' onClick={doDelete}  isProcessing={loading} >
            Delete Auction
        </Button>
    )
}
