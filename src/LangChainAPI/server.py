import uvicorn

if __name__ == '__main__':
    uvicorn.run('main:app', host='0.0.0.0', port=8765, reload=True)