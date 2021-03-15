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

		
api_hash = os.environ.get('api_hash') 
api_id =  os.environ.get('api_id') 
phone = os.environ.get('phone')
password = os.environ.get('password')
session_name = os.environ.get('session_name')

