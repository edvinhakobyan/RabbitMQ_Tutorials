using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;




namespace Redis
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class TokenAuthorizationAttribute : ActionFilterAttribute
    {
        public override bool AllowMultiple => false;
​
        private IMapper Mapper { get { return MapperConfigurator.GetMapper(); } }
​
        public override OnActionExecuting(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (!IsSkippingAuthorization(actionContext))
            {
                bool isAuthorized = await IsAuthorizedAsync(actionContext);
                if (!isAuthorized)
                {
                    HandleUnauthorizedRequest(actionContext);
                    return;
                }
            }
            await base.OnActionExecuting(actionContext, cancellationToken);
        }
​
        private async Task<bool> IsAuthorizedAsync(HttpActionContext actionContext)
        {
            try
            {
                string token = string.Empty;
​
                if (actionContext.Request.Headers.TryGetValues("Token", out var values))
                    token = values.FirstOrDefault();
​
                if (string.IsNullOrEmpty(token))
                {
                    if (actionContext.ActionArguments.FirstOrDefault(x => x.Value is ITokenAuthorization).Value is ITokenAuthorization clientToken)
                        token = clientToken.Token;

                    if (string.IsNullOrEmpty(token))
                    {
                        Logger.ServiceLog.Warn($"TokenAuthorizationFilter: missing Token. {actionContext.ActionDescriptor.ActionName}");
                        return false;
                    }
​
                    //Logger.ServiceLog.Info($"TokenAuthorizationFilter: missing Token in Headers. The Token is taken from the request body. {actionContext.ActionDescriptor.ActionName}");
                }
​
                Session session = null;
​
                var bl = RedisBLFactory.CreateAuthenicationRedisBl();
                session = await bl.GetClientSession(token);
​
                if (session == null)
                {
                    var siteInfo = new SiteInfo();
                    try
                    {
                        siteInfo.PartnerId = Convert.ToInt32(actionContext.ControllerContext.RouteData.Values["partnerId"]);
                        siteInfo.LangId = Convert.ToString(actionContext.ControllerContext.RouteData.Values["langId"]);
                    }
                    catch { }
​
                    if (siteInfo.PartnerId > 0)
                    {
                        try
                        {
                            string partnerApiToken = $"PartnerAPI_{siteInfo.PartnerId}_{token}";
                            ClientSession clientsession;
                            if (actionContext.ActionDescriptor.ControllerDescriptor.ControllerType == typeof(ClientController)
                                && string.Compare(actionContext.ActionDescriptor.ActionName, "RestoreLogin", StringComparison.InvariantCultureIgnoreCase) == 0)
                            {
                                int? cashDeskId = null;
                                //if (requestData["CashDeskId"] != null)
                                //    cashDeskId = requestData["CashDeskId"].Value<int?>();
                                using (var clientBl = WebApiApplication.BlFactory.CreateClientsBL(null, WebApiApplication.GetSessionInfo(siteInfo), false))
                                    clientsession = await clientBl.TryGetClientSessionFromPartner(siteInfo.PartnerId, token, cashDeskId);
​
                                if (clientsession != null)
                                {
                                    session = Mapper.Map<ClientSession, Session>(clientsession);
                                    await bl.AddClientSession(partnerApiToken, session, true);
                                }
                            }
                            else
                            {
                                session = await bl.GetClientSession(partnerApiToken);
                                if (session != null)
                                    await bl.RefreshToken(partnerApiToken, session);
                                if (session is null)
                                {
                                    using (var clientBl = WebApiApplication.BlFactory.CreateClientsBL(null, WebApiApplication.GetSessionInfo(siteInfo), false))
                                        clientsession = clientBl.TryAuthorizeToken(siteInfo.PartnerId, token);
                                    if (clientsession != null)
                                        session = Mapper.Map<ClientSession, Session>(clientsession);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.ServiceLog.Error(ex, "TokenAuthorizationFilter-TryPartnerAPI with token {0} for PartnerId {1}.", token, siteInfo.PartnerId);
                            throw;
                        }
                    }
                }
​
                var controller = actionContext.ControllerContext.Controller as BaseController;
                if (controller == null || session == null)
                    return false;
​
                if (session.AuthenticationStatus == AuthenticationStatus.TwoFactorCodeNeeded)
                {
                    var actionAnonymousAttribute = actionContext.ActionDescriptor.GetCustomAttributes<TwoFactorCodeAttribute>();
                    if (!actionAnonymousAttribute.Any())
                        return false;
                }
​
                controller.ClientSession = session;
                return true;
            }
            catch (Exception ex)
            {
                Logger.ServiceLog.Error(ex, "TokenAuthorizationFilter-Unexpected error.");
                return false;
            }
​
        }
​
        private bool IsSkippingAuthorization(HttpActionContext actionContext)
        {
            var actionAnonymousAttribute = actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>();
            if (actionAnonymousAttribute.Any())
                return true;
            var controllerAnonymousAttribute = actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>();
            if (controllerAnonymousAttribute.Any())
                return true;
            return false;
        }       
​
        private void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            if (actionContext.ControllerContext.ControllerDescriptor.ControllerType == typeof(ProductAPIV2Controller))
            {
                var resp = new Models.ProductAPIV2.BaseResponse();
                string token = string.Empty;
                if (actionContext.ActionArguments.FirstOrDefault(x => x.Value is ITokenAuthorization).Value is ITokenAuthorization clientToken)
                    token = clientToken.Token;
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.OK, resp.Error(Constants.Errors.WrongClientToken, token));
            }
            else
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.OK,
                    ResponseWrapper<string>.ErrorResponse(Constants.Errors.WrongClientToken, Constants.Errors.WrongClientToken));
            }
​
        }
​
    }











    class Program
    {


    }
}
