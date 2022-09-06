// Code generated by protoc-gen-go-grpc. DO NOT EDIT.
// versions:
// - protoc-gen-go-grpc v1.2.0
// - protoc             (unknown)
// source: junimohost/billing/v1/billing.proto

package billingv1

import (
	context "context"
	grpc "google.golang.org/grpc"
	codes "google.golang.org/grpc/codes"
	status "google.golang.org/grpc/status"
)

// This is a compile-time assertion to ensure that this generated file
// is compatible with the grpc package it is being compiled against.
// Requires gRPC-Go v1.32.0 or later.
const _ = grpc.SupportPackageIsVersion7

// BillingServiceClient is the client API for BillingService service.
//
// For semantics around ctx use and closing/ending streaming RPCs, please refer to https://pkg.go.dev/google.golang.org/grpc/?tab=doc#ClientConn.NewStream.
type BillingServiceClient interface {
	CreateCheckoutSession(ctx context.Context, in *CreateCheckoutSessionRequest, opts ...grpc.CallOption) (*CreateCheckoutSessionResponse, error)
	GetCustomerPortal(ctx context.Context, in *GetCustomerPortalRequest, opts ...grpc.CallOption) (*GetCustomerPortalResponse, error)
}

type billingServiceClient struct {
	cc grpc.ClientConnInterface
}

func NewBillingServiceClient(cc grpc.ClientConnInterface) BillingServiceClient {
	return &billingServiceClient{cc}
}

func (c *billingServiceClient) CreateCheckoutSession(ctx context.Context, in *CreateCheckoutSessionRequest, opts ...grpc.CallOption) (*CreateCheckoutSessionResponse, error) {
	out := new(CreateCheckoutSessionResponse)
	err := c.cc.Invoke(ctx, "/junimohost.billing.v1.BillingService/CreateCheckoutSession", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

func (c *billingServiceClient) GetCustomerPortal(ctx context.Context, in *GetCustomerPortalRequest, opts ...grpc.CallOption) (*GetCustomerPortalResponse, error) {
	out := new(GetCustomerPortalResponse)
	err := c.cc.Invoke(ctx, "/junimohost.billing.v1.BillingService/GetCustomerPortal", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

// BillingServiceServer is the server API for BillingService service.
// All implementations must embed UnimplementedBillingServiceServer
// for forward compatibility
type BillingServiceServer interface {
	CreateCheckoutSession(context.Context, *CreateCheckoutSessionRequest) (*CreateCheckoutSessionResponse, error)
	GetCustomerPortal(context.Context, *GetCustomerPortalRequest) (*GetCustomerPortalResponse, error)
	mustEmbedUnimplementedBillingServiceServer()
}

// UnimplementedBillingServiceServer must be embedded to have forward compatible implementations.
type UnimplementedBillingServiceServer struct {
}

func (UnimplementedBillingServiceServer) CreateCheckoutSession(context.Context, *CreateCheckoutSessionRequest) (*CreateCheckoutSessionResponse, error) {
	return nil, status.Errorf(codes.Unimplemented, "method CreateCheckoutSession not implemented")
}
func (UnimplementedBillingServiceServer) GetCustomerPortal(context.Context, *GetCustomerPortalRequest) (*GetCustomerPortalResponse, error) {
	return nil, status.Errorf(codes.Unimplemented, "method GetCustomerPortal not implemented")
}
func (UnimplementedBillingServiceServer) mustEmbedUnimplementedBillingServiceServer() {}

// UnsafeBillingServiceServer may be embedded to opt out of forward compatibility for this service.
// Use of this interface is not recommended, as added methods to BillingServiceServer will
// result in compilation errors.
type UnsafeBillingServiceServer interface {
	mustEmbedUnimplementedBillingServiceServer()
}

func RegisterBillingServiceServer(s grpc.ServiceRegistrar, srv BillingServiceServer) {
	s.RegisterService(&BillingService_ServiceDesc, srv)
}

func _BillingService_CreateCheckoutSession_Handler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(CreateCheckoutSessionRequest)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(BillingServiceServer).CreateCheckoutSession(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/junimohost.billing.v1.BillingService/CreateCheckoutSession",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(BillingServiceServer).CreateCheckoutSession(ctx, req.(*CreateCheckoutSessionRequest))
	}
	return interceptor(ctx, in, info, handler)
}

func _BillingService_GetCustomerPortal_Handler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(GetCustomerPortalRequest)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(BillingServiceServer).GetCustomerPortal(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/junimohost.billing.v1.BillingService/GetCustomerPortal",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(BillingServiceServer).GetCustomerPortal(ctx, req.(*GetCustomerPortalRequest))
	}
	return interceptor(ctx, in, info, handler)
}

// BillingService_ServiceDesc is the grpc.ServiceDesc for BillingService service.
// It's only intended for direct use with grpc.RegisterService,
// and not to be introspected or modified (even as a copy)
var BillingService_ServiceDesc = grpc.ServiceDesc{
	ServiceName: "junimohost.billing.v1.BillingService",
	HandlerType: (*BillingServiceServer)(nil),
	Methods: []grpc.MethodDesc{
		{
			MethodName: "CreateCheckoutSession",
			Handler:    _BillingService_CreateCheckoutSession_Handler,
		},
		{
			MethodName: "GetCustomerPortal",
			Handler:    _BillingService_GetCustomerPortal_Handler,
		},
	},
	Streams:  []grpc.StreamDesc{},
	Metadata: "junimohost/billing/v1/billing.proto",
}
