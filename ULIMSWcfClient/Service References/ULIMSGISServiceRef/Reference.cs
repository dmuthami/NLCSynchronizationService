﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18449
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ULIMSWcfClient.ULIMSGISServiceRef {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://wcf.ulims.com.na", ConfigurationName="ULIMSGISServiceRef.IULIMSGISService")]
    public interface IULIMSGISService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://wcf.ulims.com.na/IULIMSGISService/startGISSyncProcess", ReplyAction="http://wcf.ulims.com.na/IULIMSGISService/startGISSyncProcessResponse")]
        string startGISSyncProcess(bool shallStart);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://wcf.ulims.com.na/IULIMSGISService/startGISSyncProcess", ReplyAction="http://wcf.ulims.com.na/IULIMSGISService/startGISSyncProcessResponse")]
        System.Threading.Tasks.Task<string> startGISSyncProcessAsync(bool shallStart);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://wcf.ulims.com.na/IULIMSGISService/isSuccessGISSyncProcess", ReplyAction="http://wcf.ulims.com.na/IULIMSGISService/isSuccessGISSyncProcessResponse")]
        bool isSuccessGISSyncProcess();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://wcf.ulims.com.na/IULIMSGISService/isSuccessGISSyncProcess", ReplyAction="http://wcf.ulims.com.na/IULIMSGISService/isSuccessGISSyncProcessResponse")]
        System.Threading.Tasks.Task<bool> isSuccessGISSyncProcessAsync();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IULIMSGISServiceChannel : ULIMSWcfClient.ULIMSGISServiceRef.IULIMSGISService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ULIMSGISServiceClient : System.ServiceModel.ClientBase<ULIMSWcfClient.ULIMSGISServiceRef.IULIMSGISService>, ULIMSWcfClient.ULIMSGISServiceRef.IULIMSGISService {
        
        public ULIMSGISServiceClient() {
        }
        
        public ULIMSGISServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public ULIMSGISServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ULIMSGISServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ULIMSGISServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string startGISSyncProcess(bool shallStart) {
            return base.Channel.startGISSyncProcess(shallStart);
        }
        
        public System.Threading.Tasks.Task<string> startGISSyncProcessAsync(bool shallStart) {
            return base.Channel.startGISSyncProcessAsync(shallStart);
        }
        
        public bool isSuccessGISSyncProcess() {
            return base.Channel.isSuccessGISSyncProcess();
        }
        
        public System.Threading.Tasks.Task<bool> isSuccessGISSyncProcessAsync() {
            return base.Channel.isSuccessGISSyncProcessAsync();
        }
    }
}
