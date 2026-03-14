from fastapi import APIRouter, UploadFile
from src.services.sfnet_service import detect_sfnet

router = APIRouter()

@router.post("/detect/sfnet")
async def detect(file: UploadFile):

    image_bytes = await file.read()

    return detect_sfnet(image_bytes)