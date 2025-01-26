import {auth} from "@/auth";
import {CustomHeaders} from "@/types";

const baseUrl = process.env.API_URL;

async function handleResponse(response: Response) {
    const text = await response.text();
    //const data = text && JSON.parse(text);
    let data;
    try {
        data = text && JSON.parse(text);
    }
    catch (error)
    {
        console.log(error)
        data = text;
    }
    if(response.ok) {
        return data || response.statusText;
    } else {
        const error = {
            status: response.status,
            message: typeof(data) === 'string' ? data : response.statusText
        }
        return {error};
    }
}

async function getHeaders() {
    const session = await auth();
    const headers : CustomHeaders = {
        'Content-Type': 'application/json',
    };
    if(session?.accessToken) {
        headers.Authorization =  `Bearer ${session?.accessToken}`
        }

    return headers;
}

async function get(url: string) {
    const requestOptions = {
        method: 'GET',
        headers: await getHeaders()
    }
    const response = await fetch(baseUrl + url, requestOptions);

    return handleResponse(response);
}

async function post(url: string, body: object) {
    const requestOptions = {
        method: 'POST',
        headers: await getHeaders(),
        body: JSON.stringify(body)
    }
    const response = await fetch(baseUrl + url, requestOptions);

    return handleResponse(response);
}

async function put(url: string, body: object) {
    const requestOptions = {
        method: 'PUT',
        headers: await getHeaders(),
        body: JSON.stringify(body)
    }
    const response = await fetch(baseUrl + url, requestOptions);

    return handleResponse(response);
}

async function del(url: string) {
    const requestOptions = {
        method: 'DELETE',
        headers: await getHeaders()
    }
    const response = await fetch(baseUrl + url, requestOptions);

    return handleResponse(response);
}

export const fetchWrapper = {
    get,post,put,del
}