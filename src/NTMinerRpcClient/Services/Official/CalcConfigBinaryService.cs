﻿using NTMiner.Controllers;
using NTMiner.Core.MinerServer;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace NTMiner.Services.Official {
    public class CalcConfigBinaryService {
        private readonly string _controllerName = ControllerUtil.GetControllerName<ICalcConfigBinaryController<HttpResponseMessage>>();

        internal CalcConfigBinaryService() {
        }

        #region GetCalcConfigsAsync
        public void GetCalcConfigsAsync(Action<List<CalcConfigData>> callback) {
            RpcRoot.JsonRequestBinaryResponseRpcHelper.PostAsync(
                RpcRoot.OfficialServerHost,
                RpcRoot.OfficialServerPort,
                _controllerName,
                nameof(ICalcConfigBinaryController<HttpResponseMessage>.CalcConfigs),
                null,
                callback: (DataResponse<List<CalcConfigData>> response, Exception e) => {
                    if (response.IsSuccess()) {
                        callback?.Invoke(response.Data);
                    }
                    else {
                        callback?.Invoke(new List<CalcConfigData>());
                    }
                }, timeountMilliseconds: 10 * 1000);
        }
        #endregion
    }
}
