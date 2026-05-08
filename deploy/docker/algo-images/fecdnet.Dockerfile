FROM pytorch/pytorch:2.2.2-cuda12.1-cudnn8-runtime

ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    PYTHONPATH=/app \
    SIFS_DATA_ROOT=/data \
    FECDNET_WEIGHT_PATH=/app/model/fecdnet.pth

WORKDIR /app

COPY Reference/FECDNet/FECDNet/api/requirements-api.txt ./api/requirements-api.txt
RUN grep -vE '^(torch|torchvision)($|[=<>])' api/requirements-api.txt > /tmp/requirements-api.txt \
    && pip install --no-cache-dir -r /tmp/requirements-api.txt

COPY Reference/FECDNet/FECDNet/ .

EXPOSE 8001
CMD ["uvicorn", "api.main:app", "--host", "0.0.0.0", "--port", "8001"]
