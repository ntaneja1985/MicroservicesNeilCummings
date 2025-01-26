'use client'
import {FieldValues, useForm} from "react-hook-form";
import {Button} from "flowbite-react";
import Input from "@/app/components/Input";
import {useEffect} from "react";
import DateInput from "@/app/components/DateInput";
import {createAuction, updateAuction} from "@/app/actions/auctionActions";
import {usePathname, useRouter} from "next/navigation";
import toast from "react-hot-toast";
import {Auction, ErrorMessage} from "@/types";



type Props = {
    auction?: Auction
}

export default function AuctionForm({auction}:Props) {
    // const {register, handleSubmit, setFocus, formState:{isSubmitting,isValid,isDirty,errors}} = useForm();
    const {control, handleSubmit, setFocus,reset,
        formState:{isSubmitting,isValid}} =
        useForm({
        mode:'onTouched'
    });
    const router = useRouter();
    const pathName = usePathname();
    useEffect(()=>{
        if(auction){
            const {make,model, year, mileage, color} = auction;
            reset({make,model,year,mileage,color})
        }
        setFocus('make')
    },[setFocus,auction,reset]);
    async function onSubmit(data: FieldValues) {
        try {
            console.log(data);
            let id = '';
            let res;
            if(pathName === '/auctions/create') {
                 res =  await createAuction(data);
                id = res.id;
            } else {
                if(auction){
                    res = await updateAuction(data, auction.id);
                    id = auction.id;
                }
            }

           if(res.error){
               throw res.error;
           }
           router.push(`/auctions/details/${id}`);
        }
        catch(error: unknown){
            const err = error as ErrorMessage;
            toast.error(err.status + ' '+ err.message);
        }
    }
    return (
        <form className='flex flex-col mt-3' onSubmit={handleSubmit(onSubmit)}>
            <Input label='Make'
                   name='make'
                   control={control}
                   rules={{required: 'Make is required'}}/>
            <Input label='Model'
                   name='model'
                   control={control}
                   rules={{required: 'Model is required'}}/>
            <Input label='Color'
                   name='color'
                   control={control}
                   rules={{required: 'Color is required'}}/>
            <div className="grid grid-cols-2 gap-3">
                <Input label='Year'
                       name='year'
                       control={control}
                       type='number'
                       rules={{required: 'Year is required'}}/>
                <Input label='Mileage'
                       name='mileage'
                       type='number'
                       control={control}
                       rules={{required: 'Mileage is required'}}/>
            </div>

            {
                pathName === '/auctions/create' &&
            <>

            <Input label='Image Url'
                   name='imageUrl'
                   control={control}
                   rules={{required: 'Image Url is required'}}/>
            <div className="grid grid-cols-2 gap-3">
                <Input label='Reserve Price(enter 0 if no reserve)'
                       name='reservePrice'
                       control={control}
                       type='number'
                       rules={{required: 'Reserve Price is required'}}/>
                <DateInput
                    label='Auction End Date/Time'
                    name='auctionEnd'
                    dateFormat={'dd MMMM yyyy h:mm a'}
                    showTimeSelect
                    control={control}
                    rules={{required: 'Auction End Date is required'}}
                />
            </div>
            </>
            }
            <div className='flex justify-between'>
                <Button outline color='gray'>Cancel</Button>
                <Button
                    isProcessing={isSubmitting}
                    disabled={!isValid}
                    type='submit'
                    outline
                    color='success'>Submit</Button>
            </div>
        </form>
    )
}
