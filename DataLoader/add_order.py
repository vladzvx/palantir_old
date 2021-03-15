import grpc
import OrderBoard_pb2_grpc
import OrderBoard_pb2
import time

channel_link1='https://t.me/ekvinokurova'
channel_link2='https://t.me/rufuturism'
channel_link3='https://t.me/gayasylum'
chat_link='kvinokurova'

time.sleep(3)

channel = grpc.insecure_channel("localhost:5005")
stub = OrderBoard_pb2_grpc.OrderBoardStub(channel)

get_history_order =  OrderBoard_pb2.Order();
get_history_order.Type = 1;
get_history_order.Id = 1231749243;
get_history_order.Link = "";
get_history_order.AccessHash = 8904464647894535828;

get_full_channel_order =  OrderBoard_pb2.Order();
get_full_channel_order.Type = 2;
get_full_channel_order.Id = 1231749243;
get_full_channel_order.Link = "";
get_full_channel_order.AccessHash = 8904464647894535828;

stub.PostOrder(get_history_order);

q=0;
