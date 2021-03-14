const path = require('path')
const { Client } = require('tdlnode')


const api_id = '1265209'
const api_hash = '573c08a50294f33f1092409df80addac'
const phone_number = '+380983952298' // or const token = 'your token'

const configuration = {
    path_to_binary_file: path.resolve(__dirname, '../lib/libtdjson'),
    database_directory: path.resolve(__dirname, '../storage'),
    files_directory: path.resolve(__dirname, '../downloads'),
    log_file_path: path.resolve(__dirname, '../logs/tdl.log'),
}

const up = async () => {
    const client = new Client({ api_id, api_hash, phone_number }, configuration)

    await client.init()

    const chats = await client.fetch({
        '@type': 'getChats',
        'offset_order': '9223372036854775807',
        'offset_chat_id': 0,
        'limit': 100,
    })

    console.log(chats)



    client.stop()
}

up()




const readline = require('readline-sync');
const { TelegramClient } = require('telegram');



const name = readline.question("What is your name?");

const client = new TelegramClient(new StringSession('1111'), api_id, api_hash, {
    connectionRetries: 3,
});



client.cha
ff=0




//const path = require('path');
//const grpc = require('grpc');
//const protoLoader = require('@grpc/proto-loader');
//const protoFiles = require('google-proto-files');
//const { Console } = require('console');

//const PROTO_PATH = 'OrderBoard.proto';

//const packageDefinition = protoLoader.loadSync(
//    PROTO_PATH,
//    {
//        keepCase: true,
//        longs: String,
//        enums: String,
//        defaults: true,
//        oneofs: true,
//       // includeDirs: ['node_modules/google-proto-files']
//    },
//);


//async function f () {
//    try {
//        const protoDescriptor = grpc.loadPackageDefinition(packageDefinition);


//        const client = new protoDescriptor.orders.OrderBoard("176.119.156.220:5005", grpc.credentials.createInsecure());
//        obj = {
//            Id: "11233421n",
//            SourceInfo1: "11233421n",
//            SourceInfo2: "11233421n",
//            Type: "Empty"
//        }
//        q = await client.PostOrder(obj, function (err, response) {
//            console.log(err)
//            console.log(response)
//        })
//        q2 = await client.GetOrder({}, function (err, response) {
//            console.log(err)
//            console.log(response)
//        })
//        q = 0;
//    }
//    catch (e) {
//        q = 0;
//    }
//}


//f();