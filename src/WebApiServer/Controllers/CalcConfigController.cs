﻿using NTMiner.Core.MinerServer;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace NTMiner.Controllers {
    public class CalcConfigController : ApiControllerBase, ICalcConfigController {
        #region CalcConfigs
        // 挖矿端实时展示理论收益的功能需要调用此服务所以调用此方法不需要登录
        // TODO:提供一个压缩接口
        [HttpGet]
        [HttpPost]
        public DataResponse<List<CalcConfigData>> CalcConfigs() {
            try {
                var data = WebApiRoot.CalcConfigSet.GetAll();
                return DataResponse<List<CalcConfigData>>.Ok(data);
            }
            catch (Exception e) {
                Logger.ErrorDebugLine(e);
                return ResponseBase.ServerError<DataResponse<List<CalcConfigData>>>(e.Message);
            }
        }
        #endregion

        #region SaveCalcConfigs
        [Role.Admin]
        [HttpPost]
        public ResponseBase SaveCalcConfigs([FromBody]SaveCalcConfigsRequest request) {
            if (request == null || request.Data == null) {
                return ResponseBase.InvalidInput("参数错误");
            }
            try {
                WebApiRoot.CalcConfigSet.SaveCalcConfigs(request.Data);
                return ResponseBase.Ok();
            }
            catch (Exception e) {
                Logger.ErrorDebugLine(e);
                return ResponseBase.ServerError(e.Message);
            }
        }
        #endregion
    }
}
