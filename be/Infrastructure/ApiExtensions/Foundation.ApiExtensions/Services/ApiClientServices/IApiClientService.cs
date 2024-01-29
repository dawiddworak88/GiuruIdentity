﻿using Foundation.ApiExtensions.Communications;
using Foundation.ApiExtensions.Models.Request;
using Foundation.ApiExtensions.Models.Response;
using System.Threading.Tasks;

namespace Foundation.ApiExtensions.Services.ApiClientServices
{
    public interface IApiClientService
    {
        Task<ApiResponse<T>> PostAsync<S, W, T>(S request) where S : ApiRequest<W> where T : BaseResponseModel;
        Task<ApiResponse<T>> PostMultipartFormAsync<S, W, T>(S request) where S : ApiRequest<W> where T : BaseResponseModel where W : FileRequestModelBase;
        Task<ApiResponse<T>> GetAsync<S, W, T>(S request) where S : ApiRequest<W> where T : class;
        Task<ApiResponse<T>> DeleteAsync<S, W, T>(S request) where S : ApiRequest<W> where T : BaseResponseModel;
    }
}
