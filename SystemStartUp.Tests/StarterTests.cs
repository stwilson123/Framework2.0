using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Moq;
using Xunit;

namespace SystemStartUp.Tests
{
    public class StarterTests
    {
        [Fact]
        public void Test1()
        {
            BeginEventHandler beginEventHandler = default(BeginEventHandler); 
            EndEventHandler endEventHandler = default(EndEventHandler);
            IAsyncResult iAsyncResult = default(IAsyncResult);
            
            var mockHttpApplication = new Mock<HttpApplication>();
            mockHttpApplication.Setup(m =>
                    m.AddOnBeginRequestAsync(It.IsAny<BeginEventHandler>(), It.IsAny<EndEventHandler>()))
                .Callback<BeginEventHandler, EndEventHandler>((beginEventHandlerTmp, endEventHandlerTMp)
                    =>
                {
                    beginEventHandler = beginEventHandlerTmp;
                    endEventHandler = endEventHandlerTMp;
                });
           
            mockHttpApplication.Setup(m =>
                    ((IHttpAsyncHandler)m).BeginProcessRequest(It.IsAny<HttpContext>(), null, null))
                .Callback<HttpApplication>((appTmp) =>
                {
                    iAsyncResult = beginEventHandler(null, null, null, null);
                });
            
            mockHttpApplication.Setup(m =>
                    ((IHttpAsyncHandler)m).EndProcessRequest(iAsyncResult))
                .Callback<HttpApplication>((appTmp) =>
                {
                    endEventHandler(iAsyncResult);
                });
            var httpApplicationModel = new HttpApplication();
            IHttpModule iHttpModule = new StartUpHttpModule();
            var listLog = new List<string> { };
           
            var starterModel = new Starter<IList<string>>(
                httpApplication =>
                {
                    listLog.Add("InitStarter");
                    return listLog;
                },
                (httpApplication, logInput) => { logInput.Add("BeginBeginResult"); },
                (httpApplication, logInput) => { logInput.Add("BeginEndResult"); }
            );
           
            starterModel.OnApplicationStart(httpApplicationModel);
            iHttpModule.Init(httpApplicationModel);
            var asyncResult = ((IHttpAsyncHandler) httpApplicationModel).BeginProcessRequest(httpApplicationModel.Context, null, null);
            
            starterModel.OnBeginRequest(httpApplicationModel);
            ((IHttpAsyncHandler) httpApplicationModel).EndProcessRequest(asyncResult);

            starterModel.OnEndRequest(httpApplicationModel);
            
            
            Assert.True(listLog[0] == "InitStarter");
            Assert.True(listLog[1] == "BeginBeginResult");
            Assert.True(listLog[2] == "BeginEndResult");

        }
        
        
    }
}