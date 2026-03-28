import numpy as np
import cv2
from src.inference.sfnet_model import sfnet_model


def detect_sfnet(image_bytes):

    image_array = np.frombuffer(image_bytes, np.uint8)
    image = cv2.imdecode(image_array, cv2.IMREAD_COLOR)

    confidence = sfnet_model.predict(image)

    result = {
        "isFake": confidence > 0.5,
        "confidence": confidence,
        "maskUrl": None
    }

    return result