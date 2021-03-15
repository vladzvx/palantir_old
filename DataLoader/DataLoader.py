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
import logging

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
		if not loop.run_until_complete(client.is_user_authorized()):
			loop.run_until_complete(client.sign_in(phone))
			code=input("insert_code:")
			try:
				loop.run_until_complete(client.sign_in(code=code))
			except:
				loop.run_until_complete(client.sign_in(password =input("insert_password:") ))

	async def _get_full_channel(self, username):
			client = self._client;
			ie = await client.get_input_entity(username)
			#return await client(functions.channels.GetFullChannelRequest(telethon.types.InputPeerChannel(id,access_hash)))
			#return await client(functions.channels.GetFullChannelRequest(telethon.types.InputPeerChannel(id,access_hash)))
			ipc = telethon.types.InputPeerChannel(ie.channel_id,ie.access_hash);
			return await client(functions.channels.GetFullChannelRequest(ipc))

	def get_full_channel(self,username):
		try:
			client = self._client;
			loop  = self._loop;
			res = loop.run_until_complete(self._get_full_channel(username))
			if len(res.chats)>1:
				chat = res.chats[1]
				result = OrderBoard_pb2.Entity();
				result.Id = chat.id;
				result.AccessHash = chat.access_hash;
				if chat.username is not None:
					result.Username = chat.username;
				result.FirstName = chat.title;
				result.Type = OrderBoard_pb2.EntityType.Value=1;
				return result
			else:
				return None
		except:
			return None

	def messages_iteration(messages):
		for message in messages:
			message_for_send = OrderBoard_pb2.Message()
			if message.message is not None:
				message_for_send.Text = message.message;
			
			message_for_send.Id = message.id ;
			if message.peer_id is not None:
				if isinstance(message.peer_id,telethon.types.PeerChannel):
					message_for_send.ChatId = message.peer_id.channel_id ;
				elif isinstance(message.peer_id,telethon.types.PeerChat):
					message_for_send.ChatId = message.peer_id.chat_id ;
			if message.from_id is not None:
				if isinstance( message.from_id, telethon.types.PeerChannel):
					message_for_send.FromId = message.from_id.channel_id
				elif isinstance( message.from_id, telethon.types.PeerUser):
					message_for_send.FromId = message.from_id.user_id

			if (message.media is not None):
				message_for_send.Media = json.dumps(message.media,cls=CustomEncoder)

			if (message.fwd_from  is not None):#обработку пользователя добавить
				if (message.fwd_from.from_id is not None and isinstance(message.fwd_from.from_id,telethon.types.PeerChannel)):
					message_for_send.ForwardFromId = message.fwd_from.from_id.channel_id
					if message.fwd_from.saved_from_msg_id is not None:
						message_for_send.ForwardFromMessageId = message.fwd_from.saved_from_msg_id

				
			if message.grouped_id is not None:
				message_for_send.MediagroupId = message.grouped_id ;

			message_for_send.Timestamp.FromDatetime(message.date)
			
			if message.reply_to is not None:
				message_for_send.ReplyTo = message.reply_to.reply_to_msg_id
				if message.reply_to.reply_to_top_id is not None:
					message_for_send.ThreadStart = message.reply_to.reply_to_top_id
			yield message_for_send

	async def post_entity_if_need(self, from_id):
		client = self._client;
		global stub;
		global users;
		global chats;
		if isinstance(from_id, telethon.tl.types.PeerUser):
			if users.get(from_id.user_id) is None:
				entity = OrderBoard_pb2.Entity();
				entity.Id = from_id.user_id
				entity.Type=0;
				if not stub.CheckEntity(entity).Result:
					tg_entity = await client.get_entity(from_id)
					entity.AccessHash = tg_entity.access_hash
					if tg_entity.username is not None:
						entity.Username = tg_entity.username
					if tg_entity.first_name is not None:
						entity.FirstName = tg_entity.first_name
					if tg_entity.last_name is not None:
						entity.LastName = tg_entity.last_name
					stub.PostEntity(entity)
				users[from_id.user_id] = 0;

		elif isinstance(from_id, telethon.tl.types.PeerChannel):
			if chats.get(from_id.channel_id) is None:
				entity = OrderBoard_pb2.Entity();
				entity.Id = from_id.channel_id
				entity.Type=1;
				if not stub.CheckEntity(entity).Result:
					tg_entity = await client.get_entity(from_id)
					entity.AccessHash = tg_entity.access_hash
					if tg_entity.username is not None:
						entity.Username = tg_entity.username
					if tg_entity.title is not None:
						entity.LastName = tg_entity.title
					stub.PostEntity(entity)
				chats[from_id.channel_id] = 0;

	async def _get_history_by_id(self,id, access_hash,offset):
		client = self._client;
		try:
			entity = await client.get_entity(telethon.types.InputPeerChat(id))
		except BaseException:
			pass

		limit_msg=80
		offset+=limit_msg+1;
		need_break = False
		last_action_time=datetime.datetime.min
		global stub;
		global users;
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
			
			stub.StreamMessages(TgDataGetter.messages_iteration(reversed(history.messages)))

			for message in history.messages:
				if message.from_id is not None:
					await self.post_entity_if_need(message.from_id)
				if message.fwd_from is not None:
					await self.post_entity_if_need(message.fwd_from.from_id)

			if need_break:
				break
			offset=offset+limit_msg
			diff = offset - history.count
			if diff>0:
				offset=0;
				limit_msg = history.count-history.messages[0].id;
				need_break=True
	
	async def _get_history_by_link(self,link,offset):
		client = self._client;
		entity = await client.get_entity(link)
		limit_msg=80
		offset+=limit_msg+1;
		need_break = False
		last_action_time=datetime.datetime.min
		global stub;
		global users;
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
			
			stub.StreamMessages(TgDataGetter.messages_iteration(reversed(history.messages)))

			for message in history.messages:
				if message.from_id is not None:
					await self.post_entity_if_need(message.from_id)
				if message.fwd_from is not None:
					await self.post_entity_if_need(message.fwd_from.from_id)

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

	def get_history_by_id(self,id, access_hash,offset=1):
		loop  = self._loop;
		loop.run_until_complete(self._get_history_by_id(id, access_hash,offset))

		
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

#phone = "+380983952298";
phone = "+380950270822";
getter = TgDataGetter("sss", api_id, api_hash,phone)
getter.start();
time.sleep(3);


channel = grpc.insecure_channel("localhost:5005")
stub = OrderBoard_pb2_grpc.OrderBoardStub(channel)
GetFullChannelCounter = 0
timestamp = datetime.datetime.utcnow();
users = {}
chats = {}



while True:
	delta_time =(datetime.datetime.utcnow()-timestamp) 
	if delta_time.days>=24:
		GetFullChannelCounter=0
		timestamp=datetime.datetime.utcnow()

	order  = stub.GetOrder(OrderBoard_pb2.google_dot_protobuf_dot_empty__pb2.Empty())

	if order.Type==1:
		if order.Link=="":
			getter.get_history_by_id(order.Id,order.AccessHash,order.Offset)
		else:
			getter.get_history_by_link(order.Link,order.Offset)
	elif order.Type==2:
		try:
			if GetFullChannelCounter<180:
				GetFullChannelCounter+=1
				fch = getter.get_full_channel(order.Link)
				if fch is not None:
					stub.PostEntity(fch)
			else:
				stub.PostOrder(order);
		except BaseException as e:
			pass
	elif order.Type==0:
		q=0;
	#except:
	#	pass
	
	time.sleep(1)


