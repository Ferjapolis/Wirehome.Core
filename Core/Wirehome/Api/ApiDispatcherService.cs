﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Wirehome.Contracts.Api;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Services;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Wirehome.Api
{
    public class ApiDispatcherService : ServiceBase, IApiDispatcherService
    {
        private readonly List<IApiAdapter> _adapters = new List<IApiAdapter>();
        private readonly Dictionary<string, Action<IApiCall>> _actions = new Dictionary<string, Action<IApiCall>>(StringComparer.OrdinalIgnoreCase);
        private readonly ILogger _log;

        public ApiDispatcherService(ILogService logService)
        {
            if (logService == null) throw new ArgumentNullException(nameof(logService));

            Route("GetStatus", HandleGetStatusRequest);
            Route("GetConfiguration", HandleGetConfigurationRequest);
            Route("GetActions", HandleGetActionsRequest);
            Route("Ping", HandlePingRequest);
            Route("Execute", HandleExecuteRequest);

            _log = logService.CreatePublisher(nameof(ApiDispatcherService));
        }

        public event EventHandler<ApiRequestReceivedEventArgs> StatusRequested;
        public event EventHandler<ApiRequestReceivedEventArgs> StatusRequestCompleted;
        public event EventHandler<ApiRequestReceivedEventArgs> ConfigurationRequested;
        public event EventHandler<ApiRequestReceivedEventArgs> ConfigurationRequestCompleted;

        public void NotifyStateChanged(IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
 
            foreach (var adapter in _adapters)
            {
                adapter.NotifyStateChanged(component);
            }
        }

        public void Route(string action, Action<IApiCall> handler)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            lock (_actions)
            {
                action = action.Trim();

                if (_actions.ContainsKey(action))
                {
                    _log.Warning($"Overriding action route: {action}");    
                }

                _actions[action] = handler;
            }
        }

        public void Expose(object controller)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            var controllerType = controller.GetType();

            var classAttribute = controllerType.GetTypeInfo().GetCustomAttribute<ApiClassAttribute>();
            if (classAttribute == null)
            {
                return;
            }

            Expose(classAttribute.Namespace, controller);
        }

        public void RegisterAdapter(IApiAdapter adapter)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));

            _adapters.Add(adapter);
            adapter.ApiRequestReceived += RouteRequest;
        }

        private void Expose(string @namespace, object controller)
        {
            if (@namespace == null) throw new ArgumentNullException(nameof(@namespace));
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            foreach (var method in controller.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                var methodAttribute = method.GetCustomAttribute<ApiMethodAttribute>();
                if (methodAttribute == null)
                {
                    continue;
                }

                var action = @namespace + "/" + method.Name;
                void Handler(IApiCall apiCall) => method.Invoke(controller, new object[] {apiCall});
                Route(action, Handler);

                _log.Verbose($"Exposed API method to action '{action}'.");
            }
        }

        private void HandleGetActionsRequest(IApiCall apiCall)
        {
            var actions = new JArray();

            lock (_actions)
            {
                foreach (var action in _actions)
                {
                    actions.Add(action.Key);
                }
            }

            apiCall.Result.Add("Actions", actions);
        }

        private void HandleGetStatusRequest(IApiCall apiCall)
        {
            var eventArgs = new ApiRequestReceivedEventArgs(apiCall);
            StatusRequested?.Invoke(this, eventArgs);
            StatusRequestCompleted?.Invoke(this, eventArgs);
        }

        private void HandleGetConfigurationRequest(IApiCall apiCall)
        {
            var eventArgs = new ApiRequestReceivedEventArgs(apiCall);
            ConfigurationRequested?.Invoke(this, eventArgs);
            ConfigurationRequestCompleted?.Invoke(this, eventArgs);
        }

        private void HandlePingRequest(IApiCall apiCall)
        {
            apiCall.ResultCode = ApiResultCode.Success;
            apiCall.Result = apiCall.Parameter;
        }

        private void HandleExecuteRequest(IApiCall apiCall)
        {
            if (apiCall.Parameter == null || string.IsNullOrEmpty(apiCall.Action))
            {
                apiCall.ResultCode = ApiResultCode.InvalidParameter;
                return;
            }
            
            var apiRequest = apiCall.Parameter.ToObject<ApiRequest>();
            if (apiRequest == null)
            {
                apiCall.ResultCode = ApiResultCode.InvalidParameter;
                return;
            }

            if (apiRequest.Action.Equals("Execute", StringComparison.OrdinalIgnoreCase))
            {
                apiCall.ResultCode = ApiResultCode.ActionNotSupported;
                return;
            }

            var innerApiContext = new ApiCall(apiRequest.Action, apiRequest.Parameter ?? new JObject(), apiRequest.ResultHash);

            var eventArgs = new ApiRequestReceivedEventArgs(innerApiContext);
            RouteRequest(this, eventArgs);

            apiCall.ResultCode = innerApiContext.ResultCode;
            apiCall.Result = innerApiContext.Result;
            apiCall.ResultHash = innerApiContext.ResultHash;

            if (apiCall.ResultHash != null)
            {
                using (var md5 = MD5.Create())
                {
                    var newHash = Convert.ToBase64String(md5.ComputeHash(Encoding.UTF8.GetBytes(apiCall.Result.ToString())));

                    if (apiCall.ResultHash.Equals(newHash))
                    {
                        apiCall.Result = new JObject();
                    }

                    apiCall.ResultHash = newHash;
                }
            }
        }

        private void RouteRequest(object sender, ApiRequestReceivedEventArgs e)
        {
            Action<IApiCall> handler;
            lock (_actions)
            {
                if (!_actions.TryGetValue(e.ApiContext.Action, out handler))
                {
                    e.ApiContext.ResultCode = ApiResultCode.ActionNotSupported;
                    return;
                }
            }

            try
            {
                handler(e.ApiContext);
            }
            catch (Exception exception)
            {
                e.ApiContext.ResultCode = ApiResultCode.UnhandledException;
                e.ApiContext.Result = ExceptionSerializer.SerializeException(exception);
            }
            finally
            {
                e.IsHandled = true;
            }
        }

        
    }
}
