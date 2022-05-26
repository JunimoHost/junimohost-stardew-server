// Code generated by protoc-gen-go-grpc. DO NOT EDIT.

package pbsd

import (
	context "context"
	grpc "google.golang.org/grpc"
	codes "google.golang.org/grpc/codes"
	status "google.golang.org/grpc/status"
)

// This is a compile-time assertion to ensure that this generated file
// is compatible with the grpc package it is being compiled against.
const _ = grpc.SupportPackageIsVersion7

// StardewDaemonServiceClient is the client API for StardewDaemonService service.
//
// For semantics around ctx use and closing/ending streaming RPCs, please refer to https://pkg.go.dev/google.golang.org/grpc/?tab=doc#ClientConn.NewStream.
type StardewDaemonServiceClient interface {
	IndexBackup(ctx context.Context, in *IndexBackupRequest, opts ...grpc.CallOption) (*IndexBackupResponse, error)
	GetStartupConfig(ctx context.Context, in *GetStartupConfigRequest, opts ...grpc.CallOption) (*GetStartupConfigResponse, error)
	UpdateStatus(ctx context.Context, in *UpdateStatusRequest, opts ...grpc.CallOption) (*UpdateStatusResponse, error)
}

type stardewDaemonServiceClient struct {
	cc grpc.ClientConnInterface
}

func NewStardewDaemonServiceClient(cc grpc.ClientConnInterface) StardewDaemonServiceClient {
	return &stardewDaemonServiceClient{cc}
}

func (c *stardewDaemonServiceClient) IndexBackup(ctx context.Context, in *IndexBackupRequest, opts ...grpc.CallOption) (*IndexBackupResponse, error) {
	out := new(IndexBackupResponse)
	err := c.cc.Invoke(ctx, "/junimohost.stardewdaemon.v1.StardewDaemonService/IndexBackup", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

func (c *stardewDaemonServiceClient) GetStartupConfig(ctx context.Context, in *GetStartupConfigRequest, opts ...grpc.CallOption) (*GetStartupConfigResponse, error) {
	out := new(GetStartupConfigResponse)
	err := c.cc.Invoke(ctx, "/junimohost.stardewdaemon.v1.StardewDaemonService/GetStartupConfig", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

func (c *stardewDaemonServiceClient) UpdateStatus(ctx context.Context, in *UpdateStatusRequest, opts ...grpc.CallOption) (*UpdateStatusResponse, error) {
	out := new(UpdateStatusResponse)
	err := c.cc.Invoke(ctx, "/junimohost.stardewdaemon.v1.StardewDaemonService/UpdateStatus", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

// StardewDaemonServiceServer is the server API for StardewDaemonService service.
// All implementations must embed UnimplementedStardewDaemonServiceServer
// for forward compatibility
type StardewDaemonServiceServer interface {
	IndexBackup(context.Context, *IndexBackupRequest) (*IndexBackupResponse, error)
	GetStartupConfig(context.Context, *GetStartupConfigRequest) (*GetStartupConfigResponse, error)
	UpdateStatus(context.Context, *UpdateStatusRequest) (*UpdateStatusResponse, error)
	mustEmbedUnimplementedStardewDaemonServiceServer()
}

// UnimplementedStardewDaemonServiceServer must be embedded to have forward compatible implementations.
type UnimplementedStardewDaemonServiceServer struct {
}

func (UnimplementedStardewDaemonServiceServer) IndexBackup(context.Context, *IndexBackupRequest) (*IndexBackupResponse, error) {
	return nil, status.Errorf(codes.Unimplemented, "method IndexBackup not implemented")
}
func (UnimplementedStardewDaemonServiceServer) GetStartupConfig(context.Context, *GetStartupConfigRequest) (*GetStartupConfigResponse, error) {
	return nil, status.Errorf(codes.Unimplemented, "method GetStartupConfig not implemented")
}
func (UnimplementedStardewDaemonServiceServer) UpdateStatus(context.Context, *UpdateStatusRequest) (*UpdateStatusResponse, error) {
	return nil, status.Errorf(codes.Unimplemented, "method UpdateStatus not implemented")
}
func (UnimplementedStardewDaemonServiceServer) mustEmbedUnimplementedStardewDaemonServiceServer() {}

// UnsafeStardewDaemonServiceServer may be embedded to opt out of forward compatibility for this service.
// Use of this interface is not recommended, as added methods to StardewDaemonServiceServer will
// result in compilation errors.
type UnsafeStardewDaemonServiceServer interface {
	mustEmbedUnimplementedStardewDaemonServiceServer()
}

func RegisterStardewDaemonServiceServer(s grpc.ServiceRegistrar, srv StardewDaemonServiceServer) {
	s.RegisterService(&_StardewDaemonService_serviceDesc, srv)
}

func _StardewDaemonService_IndexBackup_Handler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(IndexBackupRequest)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(StardewDaemonServiceServer).IndexBackup(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/junimohost.stardewdaemon.v1.StardewDaemonService/IndexBackup",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(StardewDaemonServiceServer).IndexBackup(ctx, req.(*IndexBackupRequest))
	}
	return interceptor(ctx, in, info, handler)
}

func _StardewDaemonService_GetStartupConfig_Handler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(GetStartupConfigRequest)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(StardewDaemonServiceServer).GetStartupConfig(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/junimohost.stardewdaemon.v1.StardewDaemonService/GetStartupConfig",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(StardewDaemonServiceServer).GetStartupConfig(ctx, req.(*GetStartupConfigRequest))
	}
	return interceptor(ctx, in, info, handler)
}

func _StardewDaemonService_UpdateStatus_Handler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(UpdateStatusRequest)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(StardewDaemonServiceServer).UpdateStatus(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/junimohost.stardewdaemon.v1.StardewDaemonService/UpdateStatus",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(StardewDaemonServiceServer).UpdateStatus(ctx, req.(*UpdateStatusRequest))
	}
	return interceptor(ctx, in, info, handler)
}

var _StardewDaemonService_serviceDesc = grpc.ServiceDesc{
	ServiceName: "junimohost.stardewdaemon.v1.StardewDaemonService",
	HandlerType: (*StardewDaemonServiceServer)(nil),
	Methods: []grpc.MethodDesc{
		{
			MethodName: "IndexBackup",
			Handler:    _StardewDaemonService_IndexBackup_Handler,
		},
		{
			MethodName: "GetStartupConfig",
			Handler:    _StardewDaemonService_GetStartupConfig_Handler,
		},
		{
			MethodName: "UpdateStatus",
			Handler:    _StardewDaemonService_UpdateStatus_Handler,
		},
	},
	Streams:  []grpc.StreamDesc{},
	Metadata: "junimohost/stardewdaemon/v1/stardewdaemon.proto",
}
