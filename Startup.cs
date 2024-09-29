using Amazon;
using Amazon.Runtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebAPI.Samples.Model;

namespace WebAPI.Samples
{
    public class Startup
    {
        //private static readonly List<WebSocket> _connectedClients = new List<WebSocket>();
        // This dictionary holds the connected clients
        private static readonly Dictionary<string, WebSocket> _connectedClients = new Dictionary<string, WebSocket>();
        private static readonly Dictionary<string, string> _ClientsRooms = new Dictionary<string, string>();
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers(options =>
            {
                options.RespectBrowserAcceptHeader = true; // false by default
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPI.Samples", Version = "v1" });
            });
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder =>
                    {
                        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                    });
            });
            var credentials = new BasicAWSCredentials(AWSConstants.AccessKeyID, AWSConstants.SecretAccessKey);
            Utility.AWSS3Utils.SetIAmazonS3Connection(credentials, RegionEndpoint.USEast1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPI.Samples v1"));
            }
            app.UseWebSockets();
            _ = app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/chat")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        string clientId=string.Empty;
                        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        {
                            // Add the new client to the dictionary with a unique identifier
                            clientId = Guid.NewGuid().ToString();
                            _connectedClients.Add(clientId, webSocket);
                            _ClientsRooms.Add(clientId, "");
                        };
                       
                        await Echo(context, webSocket);

                        // Remove the client from the collection
                        _connectedClients.Remove(clientId);
                        _ClientsRooms.Remove(clientId);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }
            });
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors();
            app.UseAuthorization();

          // app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //app.Run(async (context) =>
            //{
            //    await context.Response.WriteAsync("Blank data");
            //});
        }


        private async Task Echo(HttpContext context, WebSocket webSocket)
        {
            try
            {
                var buffer = new byte[1024 * 4];
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                while (!result.CloseStatus.HasValue)
                {
                    var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var ressagetoSocket = JsonConvert.DeserializeObject(receivedMessage);
                    //MessagetoSocket rmessagetoSocket = (MessagetoSocket)JsonConvert.DeserializeObject((string)ressagetoSocket);

                    MessagetoSocket rmessagetoSocket = JsonConvert.DeserializeObject<MessagetoSocket>(ressagetoSocket.ToString());

                    string roomid = rmessagetoSocket.roomid;
                    

                    if (rmessagetoSocket.connectiontype == Connectiontype.connect)
                    {
                        if (!string.IsNullOrEmpty(roomid))
                        {
                            var connectedCli = _connectedClients.Where(y => y.Value == webSocket).FirstOrDefault();
                            _ClientsRooms[connectedCli.Key] = roomid;
                        }
                        var connectedClients = _connectedClients.Where(y => y.Value == webSocket).FirstOrDefault();
                        MessagetoSocket sendmessagetoSocket = new MessagetoSocket()
                        {
                            clientid = connectedClients.Key,
                            roomid = rmessagetoSocket.roomid,
                            message = rmessagetoSocket.message,
                            connectiontype = Connectiontype.connect
                        };
                        var tosendItem = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sendmessagetoSocket));
                        await webSocket.SendAsync(new ArraySegment<byte>(tosendItem), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    }
                    else if (rmessagetoSocket.connectiontype == Connectiontype.open)
                    {

                        foreach (var item in _ClientsRooms.Where(x => x.Value == rmessagetoSocket.roomid).ToList())
                        {
                            var connectedClients = _connectedClients.Where(y => y.Key == item.Key).FirstOrDefault();
                            MessagetoSocket sendmessagetoSocket = new MessagetoSocket()
                            {
                                clientid = rmessagetoSocket.clientid,
                                roomid = rmessagetoSocket.roomid,
                                message = rmessagetoSocket.message,
                                connectiontype = rmessagetoSocket.connectiontype
                            };
                            var tosendItem = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sendmessagetoSocket));
                            await connectedClients.Value.SendAsync(new ArraySegment<byte>(tosendItem), result.MessageType, result.EndOfMessage, CancellationToken.None);

                        }
                    }
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                }
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task HandleConnection(HttpContext context, WebSocket webSocket,string clientId)
        {
            // Send a welcome message to the client
            var welcomeMessage = Encoding.UTF8.GetBytes("Welcome!"+ clientId);
            await webSocket.SendAsync(new ArraySegment<byte>(welcomeMessage), WebSocketMessageType.Text, true, CancellationToken.None);

            // Then proceed with your existing code...
        }
    }
    
    public class MessagetoSocket
    {
        public string roomid { get; set; }
        public string clientid { get; set; }
        public DataTransfer message { get; set; }
        public Connectiontype connectiontype { get; set; }
    }
    public class DataTransfer
    {
        public Actiontype actiontype { get; set; }
        public string strData { get; set; }
    }
    public  enum Actiontype
    {
        input = 0,
        navigate = 1,
        expander = 2,
    }
    public enum Connectiontype
    {
        connect=0,
        open=1
    }
}
