from telethon import TelegramClient
import telethon
import asyncio
import datetime
from telethon import functions
from telethon.tl.functions.messages import GetHistoryRequest
import datetime 
import time
import json
import grpc
import OrderBoard_pb2_grpc
import OrderBoard_pb2

class CustomEncoder(json.JSONEncoder):
	def default(self, o):
		if isinstance(o,bytes):
			return {'__class__': 'bytes',
                '__value__': list(o)}     
		if isinstance(o, datetime.datetime):
			return {'__datetime__': o.replace(microsecond=0).isoformat()}
		return {'__{}__'.format(o.__class__.__name__): o.__dict__}

class TgDataGetter():

	def __init__(self,session_name, api_id,api_hash,phone):
		self._phone=phone;
		self._client = TelegramClient(session_name, api_id, api_hash)
		self._loop = asyncio.get_event_loop()

	def start(self):
		loop  = self._loop;
		client = self._client;
		loop.run_until_complete(client.connect())
		if not client.is_user_authorized:
			loop.run_until_complete(client.sign_in(phone))
			loop.run_until_complete(client.sign_in(code=input("insert_code:")))

	async def _get_full_channel(self,id):
			client = self._client;
			return await client(functions.channels.GetFullChannelRequest(id))

	def get_full_channel(self,id):
		try:
			client = self._client;
			loop  = self._loop;
			res = loop.run_until_complete(self._get_full_channel(id))
			chat = res.chats[1]
			result = OrderBoard_pb2.Entity();
			result.Id = chat.id;
			result.AccessHash = chat.access_hash;
			result.Username = chat.username;
			result.FirstName = chat.title;
			result.Type = OrderBoard_pb2.EntityType.Value=1;
			return result
		except:
			return None



	def iter_users_in_messages( messages):
		for mess in messages:
			global users
			if users.get(message.from_id.id) is None:
				ent = OrderBoard_pb2.Entity()
				ent.Id = message.from_id.id;
				ent.Type = 0
				yield ent


	def messages_iteration(messages):
		for message in messages:
			message_for_send = OrderBoard_pb2.Message()
			message_for_send.Text = message.message;
			message_for_send.Id = message.id ;
			message_for_send.ChatId = message.peer_id.id ;
			message_for_send.FromId = message.from_id.id

			if (message.media is not None):
				message_for_send.media = json.dumps(mess.media,cls=CustomEncoder)

			if (message.fwd_from  is not None):#обработку пользователя добавить
				if (message.fwd_from.from_id is not None and isinstance(message.fwd_from.from_id,telethon.types.PeerChannel)):
					message_for_send.ForwardFromId = message.fwd_from.from_id.channel_id
					message_for_send.ForwardFromMessageId = message.fwd_from.saved_from_msg_id

				
			if message.grouped_id is not None:
				message_for_send.MediagroupId = message.grouped_id ;

			message_for_send.Timestamp = message.date 
			
			if message.reply_to is not None:
				message_for_send.ReplyTo = message.reply_to.reply_to_msg_id
				if message.reply_to.reply_to_top_id is not None:
					message_for_send.ThreadStart = message.reply_to.reply_to_top_id
			yield message_for_send
	
	async def _get_history_by_link(self,link,offset):
		client = self._client;
		entity = await client.get_entity(link)
		limit_msg=10
		offset+=limit_msg+1;
		need_break = False
		last_action_time=datetime.datetime.min
		users = {}
		global stub;
		while(True):
			delta_time =(datetime.datetime.utcnow()-last_action_time) 
			if delta_time.seconds==0:
				dt = 1-delta_time.microseconds/1000000
				time.sleep(dt)
			last_action_time = datetime.datetime.utcnow()
			print(last_action_time)

			history = await client(GetHistoryRequest(
				peer=entity,
				offset_date=None, add_offset=0,hash=0,
				offset_id=offset,
				min_id=0,
				max_id=0,
				limit=limit_msg))
			
			stub.StreamMessages(messages_iteration(reversed(history.messages)))
			stub.StreamEntity(stub.CheckingStream(iter_users_in_messages(history.messages)))

			if need_break:
				break
			offset=offset+limit_msg
			diff = offset - history.count
			if diff>0:
				offset=0;
				limit_msg = history.count-history.messages[0].id;
				need_break=True

	def get_history_by_link(self,link,offset=1):
		loop  = self._loop;
		loop.run_until_complete(self._get_history_by_link(link,offset))

		
id_user1 = 837759702
id_channel=1030852584
id_channel2=1052645483
id_chat = 1287530549
channel_link1='https://t.me/ekvinokurova'
channel_link2='https://t.me/rufuturism'
channel_link3='https://t.me/gayasylum'
chat_link='https://t.me/kvinokurova'
api_hash = "573c08a50294f33f1092409df80addac";
api_id = 1265209;

phone = "+380983952298";

time.sleep(3);
getter = TgDataGetter("test_session", api_id, api_hash,phone)

#getter.start();
#getter.get_history_by_link(chat_link,11110)


li =[]
i=0;
while(i<10):
	ent = OrderBoard_pb2.Entity();
	ent.Id = 111;
	i+=1
	li.append(ent);


def test_gen(li):
	for l in li:
		yield l

channel = grpc.insecure_channel("localhost:5005")
stub = OrderBoard_pb2_grpc.OrderBoardStub(channel)
GetFullChannelCounter = 0
timestamp = datetime.datetime.utcnow();
users = {}
loop = asyncio.get_event_loop();



while True:
	delta_time =(datetime.datetime.utcnow()-timestamp) 
	if delta_time.hour>=24:
		GetFullChannelCounter=0
		timestamp=datetime.datetime.utcnow()

	order  = stub.GetOrder(OrderBoard_pb2.google_dot_protobuf_dot_empty__pb2.Empty())

	if order.Type==1:
		getter.get_history_by_link(order.Link)
	elif order.Type==2:
		try:
			if GetFullChannelCounter<180:
				GetFullChannelCounter+=1
				fch = getter.get_full_channel(order.Id)
				if fcn is not None:
					stub.PostEntity(fch)
			else:
				stub.PostOrder(order);
		except:
			pass


