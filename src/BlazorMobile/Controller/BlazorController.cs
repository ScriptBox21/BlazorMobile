﻿using BlazorMobile.Components;
using BlazorMobile.Interop;
using BlazorMobile.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Constants;
using Unosquare.Labs.EmbedIO.Modules;
using Xamarin.Forms;

namespace BlazorMobile.Controller
{
    public class BlazorController : WebApiController
    {
        public BlazorController(IHttpContext context) : base(context)
        {
        }

        public async Task<bool> ValidateRequest()
        {
            string cancel = "false";

            var webview = BlazorWebViewFactory.GetLastBlazorWebViewInstance();
            if (webview != null)
            {
                string uri = this.Request.QueryString.Get("uri");
                string referrer = this.Request.QueryString.Get("referrer");

                var args = new WebNavigatingEventArgs(
                    WebNavigationEvent.NewPage,
                    new UrlWebViewSource() { Url = referrer },
                    uri);

                webview.SendNavigating(args);

                if (args.Cancel)
                {
                    cancel = "true";
                }
            }

            IWebResponse response = new EmbedIOWebResponse(this.Request, this.Response);
            response.SetEncoding("UTF-8");
            response.AddResponseHeader("Cache-Control", "no-cache");
            response.SetStatutCode(200);
            response.SetReasonPhrase("OK");
            response.SetMimeType("text/plain");
            await response.SetDataAsync(new MemoryStream(Encoding.UTF8.GetBytes(cancel)));
            return true;
        }

        [WebApiHandler(HttpVerbs.Get, @"^(?!\/contextBridge.*$).*")]
        public async Task<bool> BlazorAppRessources()
        {
            try
            {
                //BlazorMobile Request validator context used with GeckoView and Iframe validation
                if (!string.IsNullOrEmpty(this.Request.Headers.Get("BlazorMobile-Validator")))
                {
                    return await ValidateRequest();
                }
                else
                {
                    //Standard behavior
                    IWebResponse response = new EmbedIOWebResponse(this.Request, this.Response);
                    await WebApplicationFactory.ManageRequest(response);
                    return true;
                }
            }
            catch (Exception ex)
            {
                return this.JsonExceptionResponse(ex);
            }
        }

        // You can override the default headers and add custom headers to each API Response.
        public override void SetDefaultHeaders() => this.NoCache();
    }
}
