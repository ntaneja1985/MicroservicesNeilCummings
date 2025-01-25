'use client'
import {FaSearch} from "react-icons/fa";
import {useParamsStore} from "@/hooks/useParamsStore";
import {usePathname, useRouter} from "next/navigation";

export default function Search() {
    const setParams = useParamsStore(state =>state.setParams);
    const setSearchValue = useParamsStore(state => state.setSearchValue);
    const searchValue = useParamsStore(state => state.searchValue);
    const router = useRouter();
    const pathName = usePathname();
    // function handleSearch(event: any) {
    //     setValue(event.target.value);
    // }

    function search(){
       if(pathName !== '/'){
           router.push('/');
       }
        setParams({searchTerm:searchValue});

    }

    return (
        <div className="flex w-full items-center border-2 rounded-full py-2 shadow-sm">
            <input
                type ='text'
                placeholder='Search for cars'
                onKeyDown={(e)=>{
                    if(e.key === 'Enter'){
                        search()
                    }
                }}
                value={searchValue}
                onChange={(event) => setSearchValue(event.target.value)}
                className='
                input-custom
                text-sm
                text-gray-600
                '
            />
            <button onClick={search}>
                <FaSearch size={34} className="bg-red-400 text-white rounded-full p-2 cursor-pointer mx-2" />
            </button>
        </div>
    )
}
