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
import Configurator_pb2_grpc
import OrderBoard_pb2
import Configurator_pb2
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

	def __init__(self,session_name, api_id,api_hash,phone,connection_string):
		self._phone=phone;
		container = AlchemySessionContainer(connection_string)
		session = container.new_session(phone)
		self._client = TelegramClient(session, api_id, api_hash)
		self._loop = asyncio.get_event_loop()

	def start(self):
		loop  = self._loop;
		client = self._client;
		loop.run_until_complete(client.connect())
		if not loop.run_until_complete(client.is_user_authorized()):
			loop.run_until_complete(client.sign_in(self._phone))
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
			global last_expensive_operation_time
			last_expensive_operation_time = datetime.datetime.utcnow()
			ie = await client.get_input_entity(link)
			ipc = telethon.types.InputPeerChannel(ie.channel_id,ie.access_hash);
			return await client(functions.channels.GetFullChannelRequest(ipc))

	async def _get_full_channel(self,order):
		try:
			global phone
			global GetFullChannelCounter;
			GetFullChannelCounter+=1
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
				result.Finder =phone;
				order.PairId =  chat.id;
				result.Type = OrderBoard_pb2.EntityType.Value=2;
				return result
			elif len(res.chats)==1:
				result = OrderBoard_pb2.Entity();
				result.Id = 0;
				result.Finder =phone;
				result.PairLink = order.Link;
				result.PairId = order.Id;
				result.Type = OrderBoard_pb2.EntityType.Value=2;
				return result
		except BaseException as e:
			raise e;
			return None

	def get_full_channel(self,order):
		return self._loop.run_until_complete(self._get_full_channel(order))

	def create_json(type,id,content):
		if content!="":
			result = '{"type":'+str(type)+',"id":'+str(id)+',"content":'+content+'}'
		else:
			result = '{"type":'+str(type)+',"id":'+str(id)+'}'
		return result;

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
				
				try:
					if isinstance( message.media, telethon.types.MessageMediaPoll):
						question = message.media.poll.question;
						answers = []
						for ans in message.media.poll.answers:
							answers.append(ans.text);
						temp_dict = {}
						temp_dict["ansvers"]=answers;
						temp_dict["question"] = question
						content  = json.dumps(temp_dict)
						message_for_send.Media = TgDataGetter.create_json(5,message.media.poll.id,'"'+url+'"')
					elif isinstance( message.media, telethon.types.MessageMediaDocument):
						message_for_send.Media = TgDataGetter.create_json(2,message.media.document.id,'')
					elif isinstance( message.media, telethon.types.MessageMediaPhoto):
						message_for_send.Media = TgDataGetter.create_json(3,message.media.photo.id,'')
					elif isinstance( message.media, telethon.types.MessageMediaWebPage):
						if isinstance( message.media.webpage, telethon.types.WebPage):
							type = 4;
							id  = message.media.webpage.id
							url = message.media.webpage.url
							message_for_send.Media = TgDataGetter.create_json(type,id,'"'+url+'"')

					#else:
						#message_for_send.Media = TgDataGetter.create_json(0,-1,json.dumps(message.media,cls=CustomEncoder)) 
				except BaseException as e:
					pass

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
					format_exists = False;
					if isinstance(entity,telethon.types.MessageEntityTextUrl):
						format.Type = 7
						format.Content = entity.url
						if entity.length is not None:
							format.Length = entity.length
						if entity.length is not None:
							format.Offset = entity.offset
						format_exists=True;
					elif isinstance(entity,telethon.types.MessageEntityMentionName):
						format.Type = 6
						format.Content = str(entity.user_id)
						if entity.length is not None:
							format.Length = entity.length
						if entity.length is not None:
							format.Offset = entity.offset
						format_exists=True;
					elif isinstance(entity,telethon.types.MessageEntityBold):
						format.Type = 0
						if entity.length is not None:
							format.Length = entity.length
						if entity.length is not None:
							format.Offset = entity.offset
						format_exists=True;
					elif isinstance(entity,telethon.types.MessageEntityItalic):
						format.Type = 2
						if entity.length is not None:
							format.Length = entity.length
						if entity.length is not None:
							format.Offset = entity.offset
						format_exists=True;
					elif isinstance(entity,telethon.types.MessageEntityStrike):
						format.Type = 1
						if entity.length is not None:
							format.Length = entity.length
						if entity.length is not None:
							format.Offset = entity.offset
						format_exists=True;
					elif isinstance(entity,telethon.types.MessageEntityUnderline):
						format.Type = 3
						if entity.length is not None:
							format.Length = entity.length
						if entity.length is not None:
							format.Offset = entity.offset
						format_exists=True;
					elif isinstance(entity,telethon.types.MessageEntityCode):
						format.Type = 4
						if entity.length is not None:
							format.Length = entity.length
						if entity.length is not None:
							format.Offset = entity.offset
						format_exists=True;
					elif isinstance(entity,telethon.types.MessageEntityPre):
						format.Type = 5
						if entity.length is not None:
							format.Length = entity.length
						if entity.length is not None:
							format.Offset = entity.offset
						format_exists=True;

					if format_exists:
						message_for_send.Formating.append(format);

			yield message_for_send

	async def post_entity_if_need(self, from_id):
		client = self._client;
		global stub;
		global users;
		global chats;
		global phone;
		if isinstance(from_id, telethon.tl.types.PeerUser):
			if users.get(from_id.user_id) is None:
				entity = OrderBoard_pb2.Entity();
				entity.Id = from_id.user_id
				entity.Type=0;
				#if not stub.CheckEntity(entity).Result:
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
				tg_entity = await client.get_entity(from_id)
				if tg_entity.username is not None:
					entity.Link = tg_entity.username
				if tg_entity.title is not None:
					entity.FirstName = tg_entity.title
				entity.Finder=phone;
				stub.PostEntity(entity)
				chats[from_id.channel_id] = 0;
		return False;

	def hascyr(self,s):
		lower = set('абвгдеёжзийклмнопрстуфхцчшщъыьэюя')
		return lower.intersection(s.lower()) != set()


	async def _get_history(self,order):
		offset = order.Offset
		client = self._client;
		global stub;
		global GetFullChannelCounter
		global GetFullChannelCounterLimit
		global ResolveUsernameRequestBan
		global last_expensive_operation_time
		try:
			logging.debug("Trying get chat (channel) entity by id...")
			try:
				entity = await client.get_entity(order.Id)
			except TypeError as e2:
				time.sleep(3);
				entity = await client.get_entity(order.Id)

		except ValueError as e:
			if "Could not find the input entity for" in e.args[0]:
				logging.debug("Failed.")
				if ResolveUsernameRequestBan:
					order.RedirectCounter+=1;
					if order.Type!=4:
						stub.PostOrder(order);
					return;

				if order.Link!="":
					logging.debug("Trying by link...")
					logging.info("Trying learn id by get_full_channel request...")
					last_expensive_operation_time = datetime.datetime.utcnow()
					temp_entity = await self._get_full_channel(order)
					if temp_entity is not None:
						logging.info("Full channel getted!")
						#logging.info("Trying get chat (channel) entity by id...")
						stub.PostEntity(temp_entity)
						entity = await client.get_entity(order.Id)
						if entity is not None:
							pass
							#logging.info("Ok!")
						else:
							return
					else:
						return;
				elif order.PairLink!="":
					#logging.info("Trying learn id by get_full_channel request...")
					last_expensive_operation_time = datetime.datetime.utcnow()
					entity = await client.get_entity(order.PairLink)
					if entity is not None:
						pass
						#logging.info("Ok!")
					else:
						return;
				else:
					return;
			else:
				logging.warn("Unexpected exception! "+e.args[0])
				raise e;
		except TypeError as e2:
			ddddd=0
			
		limit_msg=80
		old_offset = offset;
		if offset==1:
			old_offset=0;
		offset+=limit_msg+1;
		need_break = False
		last_action_time=datetime.datetime.min
		ban_counter=0
		checks=0
		last_expensive_operation_time = datetime.datetime.utcnow()
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
				min_id=old_offset,
				max_id=0,
				limit=limit_msg))
			logging.debug("Ok!")
			logging.debug("Streaming messages...")
			stub.StreamMessages(TgDataGetter.messages_iteration(reversed(history.messages)))

			ban = False;
			if checks<6:
				checks+=1
				for mess in history.messages:
					if mess.message is not None:
						if mess.message!="":
							res = self.hascyr(mess.message)
							if res:
								ban = False;
								break;
							else:
								ban=True;

			if ban:
				ban_counter+=1

			if ban_counter>4:
				entity22 = OrderBoard_pb2.Entity();
				entity22.Id = order.Id
				entity22.Type=3;
				stub.PostEntity(entity22)
				return

			logging.debug("Ok!")
			logging.debug("Checking messages for reposts from new channels...")
			for message in history.messages:
				try:
					if message.from_id is not None:
						await self.post_entity_if_need(message.from_id)
					if message.fwd_from is not None:
						await self.post_entity_if_need(message.fwd_from.from_id)
				except telethon.errors.ChannelPrivateError:
					pass;

			logging.debug("Ok!")
			old_offset = offset-1;
			offset=offset+limit_msg
			if len(history.messages)==0:
				if need_break:
					break;
				need_break=True;
				offset = 0;

	def get_history(self,order):
		loop  = self._loop;
		loop.run_until_complete(self._get_history(order))

	async def _read_pair(self,order):
		offset = order.Offset
		client = self._client;
		global stub;
		global GetFullChannelCounter
		global GetFullChannelCounterLimit
		global ResolveUsernameRequestBan
		global last_expensive_operation_time
		global phone
		try:
			need_request_full_channel = False;
			entity1 = await client.get_entity(order.Id)
			if order.PairId!=0:
				entity2 = await client.get_entity(order.PairId)
			else:
				need_request_full_channel=True;
		except ValueError as e:
			if "Could not find the input entity for" in e.args[0]:
				if ResolveUsernameRequestBan:
					order.RedirectCounter+=1;
					stub.PostOrder(order);
					return;
				if order.Link=="":
					return;
				need_request_full_channel=True;

		if need_request_full_channel:
			temp_entity = await self._get_full_channel(order)
			if temp_entity is not None:
				stub.PostEntity(temp_entity)
				time.sleep(3)
			#entity1 = await client.get_entity(order.Id)
			#entity2 = await client.get_entity(order.PairId)

		order2 = OrderBoard_pb2.Order();
		order2.Id = order.PairId;
		order2.PairId = order.Id;
		order2.Link = order.PairLink;
		order2.PairLink = order.Link;
		order2.Offset = order.PairOffset;
		order2.Type = 4;
		await self._get_history(order)
		order.Type = 5;
		order.Finders.append(phone)
		stub.PostOrder(order);
		if order.PairId ==0:
			return;
		try:
			entity2 = await client.get_entity(order.PairId)
			await self._get_history(order2)
			order2.Type = 5;
			order2.Finders.append(phone)
			stub.PostOrder(order2);
		except ValueError as e:
			pass


	def get_pair_history(self,order):
		loop  = self._loop;
		loop.run_until_complete(self._read_pair(order))


time.sleep(2);
grpc_host =os.environ.get('grpc_host') 
collector_type =os.environ.get('type') 

channel = grpc.insecure_channel(grpc_host)
config_stub = Configurator_pb2_grpc.ConfiguratorStub(channel);


emp = Configurator_pb2.ConfigurationRequest()
emp.Group = collector_type;
for cfg in config_stub.GetConfiguration(emp):
	config = cfg

	connection_string = "{0}://{1}:{2}@{3}/{4}".format(config.Session.SQLDialect,
																  config.Session.SessionStorageUser,
																  config.Session.SessionStoragePassword,
																  config.Session.SessionStorageHost,
																  config.Session.SessionStorageDB)
	getter = TgDataGetter(config.CollectorParams.SessionName, config.CollectorParams.ApiId, 
						  config.CollectorParams.ApiHash,
						  config.CollectorParams.Phone,connection_string)
	getter.start();
	phone = config.CollectorParams.Phone;
	stub = OrderBoard_pb2_grpc.OrderBoardStub(channel)
	GetFullChannelCounter = 0;
	GetFullChannelCounterLimit=180
	timestamp = datetime.datetime.utcnow();
	users = {}
	chats = {}
	ResolveUsernameRequestBan = False;
	GetFullChannelCounter = 0;
	ResolveUsernameRequestTime = datetime.datetime.utcnow();

	logging.basicConfig(format='%(asctime)s %(levelname)-8s %(message)s',level=logging.DEBUG,filename='app.log',datefmt='%Y-%m-%d %H:%M:%S')
	
	while True:
		try:
			if 	ResolveUsernameRequestBan:
				delta_timeRes =(datetime.datetime.utcnow()-ResolveUsernameRequestTime) 
				if delta_timeRes.seconds>=1.2*86400:
					ResolveUsernameRequestBan=False;
					GetFullChannelCounter=0
					
			delta_time =(datetime.datetime.utcnow()-timestamp) 
			if delta_time.seconds>=1.2*86400:
				GetFullChannelCounter=0
				timestamp=datetime.datetime.utcnow()
				logging.debug("Reset daily limits.")
			logging.debug("Getting order...")
			req = OrderBoard_pb2.OrderRequest();
			req.Finder = config.CollectorParams.Phone;
			req.Banned  = ResolveUsernameRequestBan;
			req.HeavyRequestsCounter = GetFullChannelCounter;
			order  = stub.GetOrder(req)
			logging.debug("Ok! Order.Id: {0}; Order.Type: {1}; Order.Link: {2}; Order.PairId: {3}; Order.PairLink: {4};".format(order.Id,
																															 order.Type,
																															 order.Link,
																															 order.PairId,
																															 order.PairLink))
			if order.Type==1:
					logging.debug("History reading...")
					getter.get_history(order)
					order.Type=5;
					stub.PostOrder(order);

			elif order.Type==2:
				if not ResolveUsernameRequestBan:
					fch = getter.get_full_channel(order)
					if fch is not None:
						stub.PostEntity(fch)
					time.sleep(order.Time)
				else:
					if order.RedirectCounter<3:
						order.RedirectCounter+=1
						stub.PostOrder(order);
			elif order.Type==4:
				getter.get_pair_history(order)
			elif order.Type==0:
				q=0;
			elif order.Type==3:
				time.sleep(order.Time)
		except telethon.errors.ChannelPrivateError:
			pass;
		except telethon.errors.FloodWaitError as e:
			if "ResolveUsernameRequest" in e.args[0]:
				ResolveUsernameRequestBan = True;
				ResolveUsernameRequestTime = datetime.datetime.utcnow();
		except BaseException as e:
			if "This session is in 'prepared' state;" in e.args[0]:
				time.sleep(120)
				getter = TgDataGetter(config.CollectorParams.SessionName, config.CollectorParams.ApiId, 
						config.CollectorParams.ApiHash,
						config.CollectorParams.Phone,connection_string)
				getter.start();
			elif "No user has" in e.args[0]:
				entity22 = OrderBoard_pb2.Entity();
				entity22.Id = order.Id
				entity22.Type=3;
				stub.PostEntity(entity22)
			else:
				logging.error(e.args[0])

		time.sleep(1)


