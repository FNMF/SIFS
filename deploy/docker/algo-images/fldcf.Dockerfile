FROM pytorch/pytorch:2.2.2-cuda12.1-cudnn8-runtime

ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    PYTHONPATH=/app \
    SIFS_DATA_ROOT=/data

WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends libgl1 libglib2.0-0 \
    && rm -rf /var/lib/apt/lists/*

COPY Reference/FLDCF/Forgery-localization-for-remote-sensing/requirement.txt ./requirement.txt
RUN pip install --no-cache-dir -r requirement.txt \
    && pip install --no-cache-dir fastapi uvicorn requests opencv-python-headless

COPY Reference/FLDCF/Forgery-localization-for-remote-sensing/ .

EXPOSE 8000
CMD ["uvicorn", "src.api.main:app", "--host", "0.0.0.0", "--port", "8000"]
