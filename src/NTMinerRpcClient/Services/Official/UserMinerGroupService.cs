﻿using NTMiner.Controllers;
using NTMiner.Core;
using NTMiner.User;
using System;
using System.Collections.Generic;

namespace NTMiner.Services.Official {
    public class UserMinerGroupService {
        private readonly string _controllerName = ControllerUtil.GetControllerName<IUserMinerGroupController>();

        internal UserMinerGroupService() {
        }

        #region GetMinerGroupsAsync
        public void GetMinerGroupsAsync(Action<DataResponse<List<UserMinerGroupData>>, Exception> callback) {
            RpcRoot.JsonRpc.SignPostAsync(
                RpcRoot.OfficialServerHost, 
                RpcRoot.OfficialServerPort, 
                _controllerName, 
                nameof(IUserMinerGroupController.MinerGroups), 
                data: new object(), 
                callback, 
                timeountMilliseconds: 5000);
        }
        #endregion

        #region AddOrUpdateMinerGroupAsync
        public void AddOrUpdateMinerGroupAsync(MinerGroupData entity, Action<ResponseBase, Exception> callback) {
            entity.ModifiedOn = DateTime.Now;
            DataRequest<MinerGroupData> request = new DataRequest<MinerGroupData> {
                Data = entity
            };
            RpcRoot.JsonRpc.SignPostAsync(
                RpcRoot.OfficialServerHost, 
                RpcRoot.OfficialServerPort, 
                _controllerName, 
                nameof(IUserMinerGroupController.AddOrUpdateMinerGroup), 
                data: request, 
                callback);
        }
        #endregion

        #region RemoveMinerGroupAsync
        public void RemoveMinerGroupAsync(Guid id, Action<ResponseBase, Exception> callback) {
            DataRequest<Guid> request = new DataRequest<Guid>() {
                Data = id
            };
            RpcRoot.JsonRpc.SignPostAsync(
                RpcRoot.OfficialServerHost, 
                RpcRoot.OfficialServerPort, 
                _controllerName, 
                nameof(IUserMinerGroupController.RemoveMinerGroup), 
                data: request, 
                callback);
        }
        #endregion
    }
}
