import axios, { AxiosError, AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios';

const createApi = (baseURL = import.meta.env.VITE_API_BASE_URL) => {
  const api: AxiosInstance = axios.create({
    baseURL,
  });

  api.interceptors.request.use(async (req: AxiosRequestConfig) => {
    // Authorization should be processed here
    return req;
  });

  api.interceptors.response.use(
    (res: AxiosResponse) => {
      return res.data;
    },
    (err: AxiosError) => {
      return Promise.reject(err);
    }
  );

  return api;
};

const api = createApi();

export { api, createApi, axios };
