﻿using Aliyun.OSS;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace NTMiner.Controllers {
    public class FileUrlController : ApiController {
        [HttpPost]
        public string MinerJsonPutUrl(string fileName) {
            try {
                var req = new GeneratePresignedUriRequest("minerjson", fileName, SignHttpMethod.Put);
                var uri = HostRoot.Current.OssClient.GeneratePresignedUri(req);
                return uri.ToString();
            }
            catch (Exception e) {
                Global.Logger.ErrorDebugLine(e.Message, e);
                return string.Empty;
            }
        }

        [HttpPost]
        public string NTMinerUrl(string fileName) {
            if (string.IsNullOrEmpty(fileName)) {
                return string.Empty;
            }
            var req = new GeneratePresignedUriRequest("ntminer", fileName, SignHttpMethod.Get) {
                Expiration = DateTime.Now.AddMinutes(10)
            };
            var uri = HostRoot.Current.OssClient.GeneratePresignedUri(req);
            return uri.ToString();
        }

        [HttpPost]
        public List<NTMinerFileData> NTMinerFiles() {
            var list = HostRoot.Current.NTMinerFileSet.GetNTMinerFiles();
            return list;
        }

        [HttpPost]
        public ResponseBase AddOrUpdateNTMinerFile([FromBody]AddOrUpdateNTMinerFileRequest request) {
            if (request == null || request.Data == null) {
                return ResponseBase.InvalidInput(Guid.Empty, "参数错误");
            }
            try {
                ResponseBase response;
                if (!request.IsValid(HostRoot.Current.UserSet, out response)) {
                    return response;
                }
                HostRoot.Current.NTMinerFileSet.AddOrUpdate(request.Data);
                return ResponseBase.Ok(request.MessageId);
            }
            catch (Exception e) {
                Global.Logger.ErrorDebugLine(e.Message, e);
                return ResponseBase.ServerError(request.MessageId, e.Message);
            }
        }

        [HttpPost]
        public ResponseBase RemoveNTMinerFile([FromBody]RemoveNTMinerFileRequest request) {
            if (request == null) {
                return ResponseBase.InvalidInput(Guid.Empty, "参数错误");
            }
            try {
                LoadClientsResponse response;
                if (!request.IsValid(HostRoot.Current.UserSet, out response)) {
                    return response;
                }
                HostRoot.Current.NTMinerFileSet.Remove(request.NTMinerId);
                return ResponseBase.Ok(request.MessageId);
            }
            catch (Exception e) {
                Global.Logger.ErrorDebugLine(e.Message, e);
                return ResponseBase.ServerError(request.MessageId, e.Message);
            }
        }

        [HttpPost]
        public string NTMinerUpdaterUrl() {
            try {
                IAppSetting ntminerUpdaterFileNameSetting = HostRoot.Current.AppSettingSet.GetAppSetting("ntminerUpdaterFileName");
                string ntminerUpdaterFileName = ntminerUpdaterFileNameSetting == null ? "NTMinerUpdater.exe" : (string)ntminerUpdaterFileNameSetting.Value;
                var req = new GeneratePresignedUriRequest("ntminer", ntminerUpdaterFileName, SignHttpMethod.Get);
                var uri = HostRoot.Current.OssClient.GeneratePresignedUri(req);
                return uri.ToString();
            }
            catch (Exception e) {
                Global.Logger.ErrorDebugLine(e.Message, e);
                return string.Empty;
            }
        }

        [HttpPost]
        public string LiteDBExplorerUrl() {
            try {
                var req = new GeneratePresignedUriRequest("ntminer", "LiteDBExplorerPortable.zip", SignHttpMethod.Get);
                var uri = HostRoot.Current.OssClient.GeneratePresignedUri(req);
                return uri.ToString();
            }
            catch (Exception e) {
                Global.Logger.ErrorDebugLine(e.Message, e);
                return string.Empty;
            }
        }

        [HttpPost]
        public string PackageUrl(string package) {
            try {
                if (string.IsNullOrEmpty(package)) {
                    return string.Empty;
                }
                var req = new GeneratePresignedUriRequest("ntminer", $"packages/{package}", SignHttpMethod.Get) {
                    Expiration = DateTime.Now.AddMinutes(10)
                };
                var uri = HostRoot.Current.OssClient.GeneratePresignedUri(req);
                return uri.ToString();
            }
            catch (Exception e) {
                Global.Logger.ErrorDebugLine(e.Message, e);
                return string.Empty;
            }
        }
    }
}
