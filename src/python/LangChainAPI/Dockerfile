FROM python:3.11-bookworm

# RUN curl https://packages.microsoft.com/keys/microsoft.asc > /etc/apt/trusted.gpg.d/microsoft.asc

# #Debian 12
# RUN curl https://packages.microsoft.com/config/debian/12/prod.list > /etc/apt/sources.list.d/mssql-release.list

# RUN apt-get update
# RUN ACCEPT_EULA=Y apt-get install -y msodbcsql18
# # optional: for bcp and sqlcmd
# RUN ACCEPT_EULA=Y apt-get install -y mssql-tools18
# RUN echo 'export PATH="$PATH:/opt/mssql-tools18/bin"' >> ~/.bashrc
# RUN source ~/.bashrc
# # optional: for unixODBC development headers
# RUN apt-get install -y unixodbc-dev

WORKDIR /code
COPY ./LangChainAPI/requirements.txt /code/requirements.txt
RUN pip install --no-cache-dir --upgrade -r /code/requirements.txt
COPY ./PythonSDK/foundationallm /code/foundationallm
COPY ./LangChainAPI /code

EXPOSE 80

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "80", "--forwarded-allow-ips", "*", "--proxy-headers"]