// Code generated by protoc-gen-go-grpc. DO NOT EDIT.
// versions:
// - protoc-gen-go-grpc v1.2.0
// - protoc             (unknown)
// source: junimohost/gamemanager/v1/gamemanager.proto

package gamemanagerv1

import (
	context "context"
	grpc "google.golang.org/grpc"
	codes "google.golang.org/grpc/codes"
	status "google.golang.org/grpc/status"
	emptypb "google.golang.org/protobuf/types/known/emptypb"
)

// This is a compile-time assertion to ensure that this generated file
// is compatible with the grpc package it is being compiled against.
// Requires gRPC-Go v1.32.0 or later.
const _ = grpc.SupportPackageIsVersion7

// GameManagerServiceClient is the client API for GameManagerService service.
//
// For semantics around ctx use and closing/ending streaming RPCs, please refer to https://pkg.go.dev/google.golang.org/grpc/?tab=doc#ClientConn.NewStream.
type GameManagerServiceClient interface {
	// maybe need to return long running operation? idk how long this will take
	CreateGame(ctx context.Context, in *CreateGameRequest, opts ...grpc.CallOption) (*emptypb.Empty, error)
}

type gameManagerServiceClient struct {
	cc grpc.ClientConnInterface
}

func NewGameManagerServiceClient(cc grpc.ClientConnInterface) GameManagerServiceClient {
	return &gameManagerServiceClient{cc}
}

func (c *gameManagerServiceClient) CreateGame(ctx context.Context, in *CreateGameRequest, opts ...grpc.CallOption) (*emptypb.Empty, error) {
	out := new(emptypb.Empty)
	err := c.cc.Invoke(ctx, "/junimohost.gamemanager.v1.GameManagerService/CreateGame", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

// GameManagerServiceServer is the server API for GameManagerService service.
// All implementations must embed UnimplementedGameManagerServiceServer
// for forward compatibility
type GameManagerServiceServer interface {
	// maybe need to return long running operation? idk how long this will take
	CreateGame(context.Context, *CreateGameRequest) (*emptypb.Empty, error)
	mustEmbedUnimplementedGameManagerServiceServer()
}

// UnimplementedGameManagerServiceServer must be embedded to have forward compatible implementations.
type UnimplementedGameManagerServiceServer struct {
}

func (UnimplementedGameManagerServiceServer) CreateGame(context.Context, *CreateGameRequest) (*emptypb.Empty, error) {
	return nil, status.Errorf(codes.Unimplemented, "method CreateGame not implemented")
}
func (UnimplementedGameManagerServiceServer) mustEmbedUnimplementedGameManagerServiceServer() {}

// UnsafeGameManagerServiceServer may be embedded to opt out of forward compatibility for this service.
// Use of this interface is not recommended, as added methods to GameManagerServiceServer will
// result in compilation errors.
type UnsafeGameManagerServiceServer interface {
	mustEmbedUnimplementedGameManagerServiceServer()
}

func RegisterGameManagerServiceServer(s grpc.ServiceRegistrar, srv GameManagerServiceServer) {
	s.RegisterService(&GameManagerService_ServiceDesc, srv)
}

func _GameManagerService_CreateGame_Handler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(CreateGameRequest)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(GameManagerServiceServer).CreateGame(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/junimohost.gamemanager.v1.GameManagerService/CreateGame",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(GameManagerServiceServer).CreateGame(ctx, req.(*CreateGameRequest))
	}
	return interceptor(ctx, in, info, handler)
}

// GameManagerService_ServiceDesc is the grpc.ServiceDesc for GameManagerService service.
// It's only intended for direct use with grpc.RegisterService,
// and not to be introspected or modified (even as a copy)
var GameManagerService_ServiceDesc = grpc.ServiceDesc{
	ServiceName: "junimohost.gamemanager.v1.GameManagerService",
	HandlerType: (*GameManagerServiceServer)(nil),
	Methods: []grpc.MethodDesc{
		{
			MethodName: "CreateGame",
			Handler:    _GameManagerService_CreateGame_Handler,
		},
	},
	Streams:  []grpc.StreamDesc{},
	Metadata: "junimohost/gamemanager/v1/gamemanager.proto",
}
