FROM python:3.11

WORKDIR /code
COPY ./LangChainAPI/requirements.txt /code/requirements.txt
RUN pip install --no-cache-dir --upgrade -r /code/requirements.txt
COPY ./PythonSDK/foundationallm /code/foundationallm
COPY ./LangChainAPI /code

EXPOSE 80

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "80", "--forwarded-allow-ips", "*"]