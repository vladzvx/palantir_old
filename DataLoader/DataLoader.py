from telethon import TelegramClient
import telethon
import asyncio
import datetime
from telethon import functions
from telethon.tl.functions.messages import GetHistoryRequest
import datetime 
from dateutil import tz
import time
import json

class CustomEncoder(json.JSONEncoder):
	def default(self, o):
		if isinstance(o,bytes):
			return {'bytes':str(o)}
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

	async def get_full_channel(self,id):
		client = self._client;
		full = await client(functions.channels.GetFullChannelRequest(id))
		q=0;

	async def _get_history_by_link(self,link,offset):
		client = self._client;
		entity = await client.get_entity(link)
		limit_msg=10
		offset+=limit_msg+1;
		need_break = False
		last_action_time=datetime.datetime.min		
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
			
			for mess in reversed(history.messages):
				print()
				print(mess.id, mess.message ,mess.grouped_id,  mess.to_json())
				print()
				print(mess.media)
				if (mess.media is not None):

					#di = mess.to_dict()
				#	print(di.keys())
					js = mess.to_dict();
					tt=js['media']['photo']['file_reference']
					if  tt!= mess.media.photo.file_reference:
						q=0;
					else:
						ii=0;
					
					
					#print(q1)
					#if isinstance(mess.media, telethon.types.MessageMediaDocument):
					#	e=123
					##else if isinstance(mess.media, telethon.types.MessageMe)
					#file_id = mess.media.photo.id;
					#file_access_hash=mess.media.photo.access_hash
					#file_reference=mess.media.photo.file_reference
					#file = await client.download_file(telethon.types.InputPhotoFileLocation(
					#	id=file_id,
					#	access_hash=file_access_hash,
					#	file_reference=file_reference,thumb_size="m"),
					#	  "qqqq.png")
					#q=0;
					#temp = json.dumps(mess.media)

			if need_break:
				break
			offset=offset+limit_msg
			diff = offset - history.count
			if diff>0:
				offset=0;
				limit_msg = history.count-history.messages[0].id;
				need_break=True

	def test(self,id):
		client = self._client;
		loop  = self._loop;
		loop.run_until_complete(self.get_full_channel(id))

	def get_history_by_link(self,link,offset):
		loop  = self._loop;
		loop.run_until_complete(self._get_history_by_link(link,offset))
		



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

getter = TgDataGetter("test_session", api_id, api_hash,phone)

getter.start();
getter.get_history_by_link("https://t.me/loader_test",11111)



