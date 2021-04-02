from flask import Flask
import os

app = Flask(__name__)


@app.route('/', methods=['POST'])
def hello():
    """Renders a sample page."""
    return os.environ.get('grpc_host')



if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5555)
