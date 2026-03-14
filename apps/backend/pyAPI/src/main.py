from fastapi import FastAPI
from src.routers.detect import router

app = FastAPI()

app.include_router(router)