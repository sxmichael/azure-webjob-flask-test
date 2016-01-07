"""
Routes and views for the flask application.
"""

from datetime import datetime
from flask import render_template, request
from FlaskWebProject import app
import time
import random
import json

class Stats(object):
    def __init__(self):
        self.receive_time = str(datetime.now())
        self.total_start_time = time.time()
        self.total_time = None
        self.send_time = None
        self.processed = 0

    def end(self):
        self.total_time = time.time() - self.total_start_time
        self.send_time = str(datetime.now())

    def dumps(self):
        return {
            "receive_time" : self.receive_time,
            "send_time" : self.send_time,
            "total_time" : self.total_time,
            "processed": self.processed,
        }

def log_info(message, unique_id):
    app.logger.info(message, extra={'guid': unique_id})

@app.route('/')
@app.route('/home')
def home():
    """Renders the home page."""
    return render_template(
        'index.html',
        title='Home Page',
        year=datetime.now().year,
    )

@app.route('/contact')
def contact():
    """Renders the contact page."""
    return render_template(
        'contact.html',
        title='Contact',
        year=datetime.now().year,
        message='Your contact page.'
    )

@app.route('/about')
def about():
    """Renders the about page."""
    return render_template(
        'about.html',
        title='About',
        year=datetime.now().year,
        message='Your application description page.'
    )

@app.route('/do', methods = ['POST'])
def post_something():
    current_id = request.args.get('id', 'none')
    stats = Stats()
    names = request.data.split('|')

    log_info("start working on: " + current_id, current_id)
    
    should_take = random.randrange(100, 150)
    for name in names:
        time.sleep(should_take / 5000.0)
        stats.processed += 1

    stats.end()
    stat_string = json.dumps(stats.dumps(), indent=4)
    log_info(stat_string, current_id)
    log_info("finish working on: {current_id}".format(current_id=current_id), current_id)
    return stat_string
