FROM python:3.7

RUN apt-get update && \
    apt-get install -y vim

WORKDIR /usr/src/app

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY welder .

ENV FLASK_APP=welder.py
ENV PYTHONUNBUFFERED=1

EXPOSE 5000

CMD [ "flask", "run", "--host=0.0.0.0"]