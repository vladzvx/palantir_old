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
import os
from alchemysession import AlchemySessionContainer


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
		container = AlchemySessionContainer('postgresql://postgres:qw12cv90@176.119.156.220/sessions')
		session = container.new_session(phone)
		self._client = TelegramClient(session, api_id, api_hash)
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

	async def _get_full_channel_request(self, order):
			link =order.Link;
			if link=="":
				link =  order.PairLink
				if link=="":
					return None;
			client = self._client;
			ie = await client.get_input_entity(link)
			ipc = telethon.types.InputPeerChannel(ie.channel_id,ie.access_hash);
			return await client(functions.channels.GetFullChannelRequest(ipc))

	async def _get_full_channel(self,order):
		try:
			client = self._client;
			loop  = self._loop;
			res = await self._get_full_channel_request(order)
			if res is None:
				return None
			if len(res.chats)>1:
				chat = res.chats[1]
				result = OrderBoard_pb2.Entity();
				result.Id = chat.id;
				if chat.username is not None:
					result.Link = chat.username;
				result.FirstName = chat.title;
				result.PairLink = order.Link;
				result.PairId = order.Id;
				result.Type = OrderBoard_pb2.EntityType.Value=2;
				return result
			else:
				return None
		except BaseException as e:
			return None

	def get_full_channel(self,order):
		return self._loop.run_until_complete(self._get_full_channel(order))

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

			if message.entities is not None:
				for entity in message.entities:
					format = OrderBoard_pb2.Formating();
					if isinstance(entity,telethon.types.MessageEntityTextUrl):
						format.Type = 7
						format.Content = entity.url
						if entity.length is not None:
							format.Length = entity.length
						if entity.length is not None:
							format.Offset = entity.offset
					elif isinstance(entity,telethon.types.MessageEntityMentionName):
						format.Type = 6
						format.Content = entity.user_id
						if entity.length is not None:
							format.Length = entity.length
						if entity.length is not None:
							format.Offset = entity.offset
					elif isinstance(entity,telethon.types.MessageEntityBold):
						format.Type = 0
						if entity.length is not None:
							format.Length = entity.length
						if entity.length is not None:
							format.Offset = entity.offset
					elif isinstance(entity,telethon.types.MessageEntityItalic):
						format.Type = 2
						if entity.length is not None:
							format.Length = entity.length
						if entity.length is not None:
							format.Offset = entity.offset
					elif isinstance(entity,telethon.types.MessageEntityStrike):
						format.Type = 1
						if entity.length is not None:
							format.Length = entity.length
						if entity.length is not None:
							format.Offset = entity.offset
					elif isinstance(entity,telethon.types.MessageEntityUnderline):
						format.Type = 3
						if entity.length is not None:
							format.Length = entity.length
						if entity.length is not None:
							format.Offset = entity.offset
					elif isinstance(entity,telethon.types.MessageEntityCode):
						format.Type = 4
						if entity.length is not None:
							format.Length = entity.length
						if entity.length is not None:
							format.Offset = entity.offset
					elif isinstance(entity,telethon.types.MessageEntityPre):
						format.Type = 5
						if entity.length is not None:
							format.Length = entity.length
						if entity.length is not None:
							format.Offset = entity.offset

					message_for_send.Formating.append(format);

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
					if tg_entity.username is not None:
						entity.Link = tg_entity.username
					if tg_entity.first_name is not None:
						entity.FirstName = tg_entity.first_name
					if tg_entity.last_name is not None:
						entity.LastName = tg_entity.last_name
					stub.PostEntity(entity)
				users[from_id.user_id] = 0;
				return True;

		elif isinstance(from_id, telethon.tl.types.PeerChannel):
			if chats.get(from_id.channel_id) is None:
				entity = OrderBoard_pb2.Entity();
				entity.Id = from_id.channel_id
				entity.Type=1;
				if not stub.CheckEntity(entity).Result:
					tg_entity = await client.get_entity(from_id)
					if tg_entity.username is not None:
						entity.Link = tg_entity.username
					if tg_entity.title is not None:
						entity.LastName = tg_entity.title
					stub.PostEntity(entity)
				chats[from_id.channel_id] = 0;
		return False;

	async def _get_history(self,order):
		offset = order.Offset
		client = self._client;
		global stub;
		global GetFullChannelCounter
		global GetFullChannelCounterLimit
		try:
			logging.debug("Trying get chat (channel) entity by id...")
			entity = await client.get_entity(order.Id)
		except ValueError as e:
			if "Could not find the input entity for" in e.args[0]:
				logging.debug("Failed.")
				if order.Link!="":
					logging.debug("Trying by link...")
					entity = await client.get_entity(order.Link)
				elif order.PairLink!="" and GetFullChannelCounter<GetFullChannelCounterLimit:
					logging.info("Trying learn id by get_full_channel request...")
					temp_entity = await self._get_full_channel(order)
					GetFullChannelCounter+=1
					if temp_entity is not None:
						logging.info("Full channel getted!")
						logging.info("Trying get chat (channel) entity by id...")
						#stub.PostEntity(temp_entity)
						entity = await client.get_entity(order.Id)
						if entity is not None:
							logging.info("Ok!")
					else:
						return;
				else:
					return;
			else:
				logging.warn("Unexpected exception! "+e.args[0])
				raise e;
			
		limit_msg=80
		offset+=limit_msg+1;
		need_break = False
		last_action_time=datetime.datetime.min
		while(True):
			delta_time =(datetime.datetime.utcnow()-last_action_time) 
			if delta_time.seconds==0:
				logging.debug("Waiting next iteration...")
				dt = 1-delta_time.microseconds/1000000
				time.sleep(dt)
			last_action_time = datetime.datetime.utcnow()


			logging.debug("Requesting history... Offset: "+str(offset));
			history = await client(GetHistoryRequest(
				peer=entity,
				offset_date=None, add_offset=0,hash=0,
				offset_id=offset,
				min_id=0,
				max_id=0,
				limit=limit_msg))
			logging.debug("Ok!")
			logging.debug("Streaming messages...")
			stub.StreamMessages(TgDataGetter.messages_iteration(reversed(history.messages)))
			logging.debug("Ok!")
			logging.debug("Checking messages for reposts from new channels...")
			for message in history.messages:
				if message.from_id is not None:
					await self.post_entity_if_need(message.from_id)
				if message.fwd_from is not None:
					await self.post_entity_if_need(message.fwd_from.from_id)
			logging.debug("Ok!")
			if need_break:
				break
			offset=offset+limit_msg
			diff = offset - history.count
			if diff>0:
				offset=0;
				limit_msg = history.count-history.messages[0].id;
				if limit_msg<0:
					limit_msg=0;
				need_break=True

	def get_history(self,order):
		loop  = self._loop;
		loop.run_until_complete(self._get_history(order))

		
#api_hash = os.environ.get('api_hash') 
#api_id =  os.environ.get('api_id') 
#phone = os.environ.get('phone')
#password = os.environ.get('password')
#session_name = os.environ.get('session_name')

api_hash = '573c08a50294f33f1092409df80addac'
api_id = 1265209
phone = '++380950270822'
session_name ="test_session"


getter = TgDataGetter(session_name, api_id, api_hash,phone)
getter.start();

time.sleep(3)

channel = grpc.insecure_channel("localhost:5005")
stub = OrderBoard_pb2_grpc.OrderBoardStub(channel)
GetFullChannelCounter = 0
GetFullChannelCounterLimit=180
timestamp = datetime.datetime.utcnow();
users = {}
chats = {}

logging.basicConfig(level=logging.DEBUG,filename='app.log')

while True:
	try:
		delta_time =(datetime.datetime.utcnow()-timestamp) 
		if delta_time.days>=24:
			GetFullChannelCounter=0
			timestamp=datetime.datetime.utcnow()
			logging.debug("Reset daily limits.")
		logging.debug("Getting order...")
		order  = stub.GetOrder(OrderBoard_pb2.google_dot_protobuf_dot_empty__pb2.Empty())
		logging.debug("Ok! Order.Id: {0}; Order.Type: {1}; Order.Link: {2}; Order.PairId: {3}; Order.PairLink: {4};".format(order.Id,
																														 order.Type,
																														 order.Link,
																														 order.PairId,
																														 order.PairLink))
		if order.Type==1:
				logging.debug("History reading...")
				getter.get_history(order)
		elif order.Type==2:
			if GetFullChannelCounter<GetFullChannelCounterLimit:
				GetFullChannelCounter+=1
				fch = getter.get_full_channel(order)
				if fch is not None:
					stub.PostEntity(fch)
			else:
				stub.PostOrder(order);
		elif order.Type==0:
			q=0;
	except BaseException as e:
		logging.error(e.args[0])

	time.sleep(1)


