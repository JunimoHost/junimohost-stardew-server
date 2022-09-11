// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: junimohost/stardewsteamauth/v1/stardew_steam_auth.proto
// </auto-generated>
#pragma warning disable 0414, 1591, 8981
#region Designer generated code

using grpc = global::Grpc.Core;

namespace Junimohost.Stardewsteamauth.V1 {
  public static partial class StardewSteamAuthService
  {
    static readonly string __ServiceName = "junimohost.stardewsteamauth.v1.StardewSteamAuthService";

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static void __Helper_SerializeMessage(global::Google.Protobuf.IMessage message, grpc::SerializationContext context)
    {
      #if !GRPC_DISABLE_PROTOBUF_BUFFER_SERIALIZATION
      if (message is global::Google.Protobuf.IBufferMessage)
      {
        context.SetPayloadLength(message.CalculateSize());
        global::Google.Protobuf.MessageExtensions.WriteTo(message, context.GetBufferWriter());
        context.Complete();
        return;
      }
      #endif
      context.Complete(global::Google.Protobuf.MessageExtensions.ToByteArray(message));
    }

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static class __Helper_MessageCache<T>
    {
      public static readonly bool IsBufferMessage = global::System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(global::Google.Protobuf.IBufferMessage)).IsAssignableFrom(typeof(T));
    }

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static T __Helper_DeserializeMessage<T>(grpc::DeserializationContext context, global::Google.Protobuf.MessageParser<T> parser) where T : global::Google.Protobuf.IMessage<T>
    {
      #if !GRPC_DISABLE_PROTOBUF_BUFFER_SERIALIZATION
      if (__Helper_MessageCache<T>.IsBufferMessage)
      {
        return parser.ParseFrom(context.PayloadAsReadOnlySequence());
      }
      #endif
      return parser.ParseFrom(context.PayloadAsNewBuffer());
    }

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Junimohost.Stardewsteamauth.V1.GetSteamTicketRequest> __Marshaller_junimohost_stardewsteamauth_v1_GetSteamTicketRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Junimohost.Stardewsteamauth.V1.GetSteamTicketRequest.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Junimohost.Stardewsteamauth.V1.GetSteamTicketResponse> __Marshaller_junimohost_stardewsteamauth_v1_GetSteamTicketResponse = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Junimohost.Stardewsteamauth.V1.GetSteamTicketResponse.Parser));

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::Junimohost.Stardewsteamauth.V1.GetSteamTicketRequest, global::Junimohost.Stardewsteamauth.V1.GetSteamTicketResponse> __Method_GetSteamTicket = new grpc::Method<global::Junimohost.Stardewsteamauth.V1.GetSteamTicketRequest, global::Junimohost.Stardewsteamauth.V1.GetSteamTicketResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "GetSteamTicket",
        __Marshaller_junimohost_stardewsteamauth_v1_GetSteamTicketRequest,
        __Marshaller_junimohost_stardewsteamauth_v1_GetSteamTicketResponse);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::Junimohost.Stardewsteamauth.V1.StardewSteamAuthReflection.Descriptor.Services[0]; }
    }

    /// <summary>Base class for server-side implementations of StardewSteamAuthService</summary>
    [grpc::BindServiceMethod(typeof(StardewSteamAuthService), "BindService")]
    public abstract partial class StardewSteamAuthServiceBase
    {
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task<global::Junimohost.Stardewsteamauth.V1.GetSteamTicketResponse> GetSteamTicket(global::Junimohost.Stardewsteamauth.V1.GetSteamTicketRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

    }

    /// <summary>Client for StardewSteamAuthService</summary>
    public partial class StardewSteamAuthServiceClient : grpc::ClientBase<StardewSteamAuthServiceClient>
    {
      /// <summary>Creates a new client for StardewSteamAuthService</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public StardewSteamAuthServiceClient(grpc::ChannelBase channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for StardewSteamAuthService that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public StardewSteamAuthServiceClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      protected StardewSteamAuthServiceClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      protected StardewSteamAuthServiceClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Junimohost.Stardewsteamauth.V1.GetSteamTicketResponse GetSteamTicket(global::Junimohost.Stardewsteamauth.V1.GetSteamTicketRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return GetSteamTicket(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Junimohost.Stardewsteamauth.V1.GetSteamTicketResponse GetSteamTicket(global::Junimohost.Stardewsteamauth.V1.GetSteamTicketRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_GetSteamTicket, null, options, request);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Junimohost.Stardewsteamauth.V1.GetSteamTicketResponse> GetSteamTicketAsync(global::Junimohost.Stardewsteamauth.V1.GetSteamTicketRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return GetSteamTicketAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Junimohost.Stardewsteamauth.V1.GetSteamTicketResponse> GetSteamTicketAsync(global::Junimohost.Stardewsteamauth.V1.GetSteamTicketRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_GetSteamTicket, null, options, request);
      }
      /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      protected override StardewSteamAuthServiceClient NewInstance(ClientBaseConfiguration configuration)
      {
        return new StardewSteamAuthServiceClient(configuration);
      }
    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    public static grpc::ServerServiceDefinition BindService(StardewSteamAuthServiceBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_GetSteamTicket, serviceImpl.GetSteamTicket).Build();
    }

    /// <summary>Register service method with a service binder with or without implementation. Useful when customizing the service binding logic.
    /// Note: this method is part of an experimental API that can change or be removed without any prior notice.</summary>
    /// <param name="serviceBinder">Service methods will be bound by calling <c>AddMethod</c> on this object.</param>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    public static void BindService(grpc::ServiceBinderBase serviceBinder, StardewSteamAuthServiceBase serviceImpl)
    {
      serviceBinder.AddMethod(__Method_GetSteamTicket, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Junimohost.Stardewsteamauth.V1.GetSteamTicketRequest, global::Junimohost.Stardewsteamauth.V1.GetSteamTicketResponse>(serviceImpl.GetSteamTicket));
    }

  }
}
#endregion