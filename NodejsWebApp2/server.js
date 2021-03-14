const path = require('path');
const grpc = require('grpc');
const protoLoader = require('@grpc/proto-loader');
const protoFiles = require('google-proto-files');
const PROTO_PATH = 'OrderBoard.proto';
const packageDefinition = protoLoader.loadSync(PROTO_PATH, {
    keepCase: true,
    longs: String,
    enums: String,
    defaults: true,
    oneofs: true,
    includeDirs: ['node_modules/google-proto-files']
});
try {
    const protoDescriptor = grpc.loadPackageDefinition(packageDefinition);
    const client = new protoDescriptor.orders.OrderBoard("http://176.119.156.220:5005", grpc.credentials.createInsecure());
    client.GetOrder();
}
catch (e) {
    var q = 0;
}
//# sourceMappingURL=server.js.map