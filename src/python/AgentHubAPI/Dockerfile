FROM python:3.11

WORKDIR /code
COPY ./AgentHubAPI/requirements.txt /code/requirements.txt
RUN pip install --no-cache-dir --upgrade -r /code/requirements.txt
COPY ./PythonSDK/foundationallm /code/foundationallm
COPY ./AgentHubAPI /code

EXPOSE 80

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "80", "--forwarded-allow-ips", "*", "--proxy-headers", "True"]