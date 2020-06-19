"""
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
"""

import ast
import datetime
import hashlib
import json
import logging
import os
import random
import socket
import ssl
import sys
import threading
import urllib.parse
from collections import OrderedDict
from collections.abc import Hashable
from functools import wraps
from queue import Queue

import click
import requests
from flask import Flask, request, jsonify, _request_ctx_stack
from flask_cors import CORS
from jose import jwt
from pyVim import connect
from pyVim.task import WaitForTask
from pyVmomi import vim

logging.basicConfig(level=logging.DEBUG)
VCENTER_HOST = os.getenv('VCENTER_HOST')
VCENTER_USERNAME = os.getenv('VCENTER_USERNAME')
VCENTER_PASSWORD = os.getenv('VCENTER_PASSWORD')
VCENTER_CAFILE = os.getenv('VCENTER_CAFILE')
PLAYER_API_URL = os.getenv('PLAYER_API_URL')
VM_API_URL = os.getenv('VM_API_URL')
VM_CONSOLE_WEB_URL = os.getenv('VM_CONSOLE_WEB_URL')
VM_API_SCOPES = os.getenv('VM_API_SCOPES', 's3 s3-vm')
IDENTITY_URL = os.getenv('IDENTITY_URL')
IDENTITY_URL_VALIDATION_OVERRIDE = os.getenv('IDENTITY_URL_VALIDATION_OVERRIDE', IDENTITY_URL)
IDENTITY_SIGNING_ALGORITHM = os.getenv('IDENTITY_SIGNING_ALGORITHM', 'RS256')
IDENTITY_AUDIENCE = os.getenv('IDENTITY_AUDIENCE', 's3')
TOKEN_USERNAME = os.getenv('TOKEN_USERNAME', '')
TOKEN_PASSWORD = os.getenv('TOKEN_PASSWORD', '')
TOKEN_CLIENT_ID = os.getenv('TOKEN_CLIENT_ID', 'welder')
CURRENT_TOKEN = {'token': '', 'refresh_after': datetime.datetime.fromtimestamp(0)}
CURRENT_TOKEN_LOCK = threading.Lock()
VERIFY_SSL = os.getenv('VERIFY_SSL', True)
WORKER_THREAD_COUNT = int(os.getenv('WORKER_THREAD_COUNT', 10))

# vCenter VM name character limit is 80, but the total of the following three variables should not exceed 64 to allow
# a partial hash and counter to ensure name uniqueness.
VM_NAME_TEAM_LENGTH_LIMIT = int(os.getenv('VM_NAME_TEAM_LENGTH_LIMIT', 16))
VM_NAME_USER_LENGTH_LIMIT = int(os.getenv('VM_NAME_USER_LENGTH_LIMIT', 16))
VM_NAME_TEMPLATE_LENGTH_LIMIT = int(os.getenv('VM_NAME_TEMPLATE_LENGTH_LIMIT', 32))

if VM_NAME_TEAM_LENGTH_LIMIT + VM_NAME_USER_LENGTH_LIMIT + VM_NAME_TEMPLATE_LENGTH_LIMIT > 64:
    logging.error('The total of the team name, user name, and template name length limits must not exceed 64.')
    sys.exit(1)

if None in (VCENTER_HOST,
            VCENTER_USERNAME,
            VCENTER_PASSWORD,
            PLAYER_API_URL,
            IDENTITY_URL,
            VM_API_URL,
            VM_CONSOLE_WEB_URL):
    logging.error('VCENTER_HOST, VCENTER_USERNAME, VCENTER_PASSWORD, PLAYER_API_URL, IDENTITY_URL, VM_API_URL, '
                  'and VM_CONSOLE_WEB_URL environment variables MUST be specified.')
    sys.exit(1)

PLAYER_API_URL = PLAYER_API_URL.rstrip('/')
VM_API_URL = VM_API_URL.rstrip('/')
IDENTITY_URL = IDENTITY_URL.rstrip('/')
VM_CONSOLE_WEB_URL = VM_CONSOLE_WEB_URL.rstrip('/')

USER_IDENTITY_FIELD_NAME = 'guestinfo.userId'

if VERIFY_SSL is not True:
    try:
        VERIFY_SSL = ast.literal_eval(VERIFY_SSL)
    except ValueError:
        VERIFY_SSL = True
        logging.error('VERIFY_SSL environment variable was set, but had some value other than True or False. '
                      'It is True by default and for safety reasons it has been set to True now. '
                      'If SSL verification is intended to be off, the environment variable must be exactly set to '
                      'False including the capital letter and with no other characters.')
        
JWKS = requests.get(f'{IDENTITY_URL}/.well-known/openid-configuration/jwks', verify=VERIFY_SSL).json()

app = Flask(__name__)
CORS(app)
if app.config['ENV'] == "development":
    logging.basicConfig(level=logging.DEBUG)
app.config['jsonfile'] = 'welder.json'

with open(app.config['jsonfile'], 'r') as f:
    JSON_CACHE = json.load(f)


class OrderedDictQueue(Queue):
    def _init(self, maxsize: int) -> None:
        self.queue = OrderedDict()

    def _put(self, item: (Hashable, object)) -> None:
        try:
            key, value = item
            if not isinstance(key, Hashable):
                raise ValueError("Key must be hashable.")
        except ValueError as e:
            raise ValueError('OrderedDictQueue only accepts a (key, value) pair.') from e
        self.queue[key] = value

    def _get(self) -> object:
        return self.queue.popitem(False)

    def __contains__(self, item):
        with self.mutex:
            return item in self.queue

    def __getitem__(self, item):
        with self.mutex:
            return self.queue[item]


DEPLOYMENT_QUEUE = OrderedDictQueue()
CURRENT_TASK_DICT_LOCK = threading.Lock()
CURRENT_TASK_DICT = {}
WORKER_THREADS = []
COMPLETED_TASKS_LOCK = threading.Lock()
COMPLETED_TASKS = 0
POSITION_TRACKER_LOCK = threading.Lock()
POSITION_TRACKER = 0


# To allow using worker, which is defined near the end of the file.
def spawn_workers():
    for i in range(WORKER_THREAD_COUNT):
        t = threading.Thread(target=worker, args=(DEPLOYMENT_QUEUE,))
        t.daemon = True
        t.start()
        WORKER_THREADS.append(t)


if __name__ == '__main__':
    app.run(host='0.0.0.0')


class ServiceInstance:
    def __init__(self):
        ssl_context = ssl.SSLContext(protocol=ssl.PROTOCOL_TLSv1_2)
        if VCENTER_CAFILE:
            ssl_context.load_verify_locations(cafile=VCENTER_CAFILE)

        self.si = None
        try:
            self.si = connect.SmartConnect(host=VCENTER_HOST,
                                           user=VCENTER_USERNAME,
                                           pwd=VCENTER_PASSWORD,
                                           sslContext=ssl_context)
        except vim.fault.InvalidLogin:
            logging.error('Incorrect vCenter username or password specified.')
        except socket.gaierror:
            logging.error('Unable to connect to vCenter instance. Ensure that the given vCenter host is correct.')
        except ConnectionRefusedError:
            logging.error('Connection to vCenter refused. Check that the port is open and the service is running.')
        if not self.si:
            raise Exception('Unable to get a service instance.')

    def __enter__(self):
        return self.si

    def __exit__(self, exc_type, exc_val, exc_tb):
        connect.Disconnect(self.si)


def get_vmware_obj(content, type_list, name, folder=None):
    folder = folder or content.rootFolder
    container = content.viewManager.CreateContainerView(folder, type_list, True)
    for item in container.view:
        if item.name == name:
            return item
    return None


def make_relocate_spec(host, pool):
    relocate_spec = vim.vm.RelocateSpec()
    relocate_spec.diskMoveType = 'createNewChildDiskBacking'
    relocate_spec.host = random.choice(host)
    relocate_spec.pool = pool

    return relocate_spec


def ensure_template_snapshot(vm):
    if len(vm.rootSnapshot) < 1:
        task = vm.CreateSnapshot_Task(name='clone_snapshot',
                                      memory=False,
                                      quiesce=False)
        WaitForTask(task)


def clone_template(template, vm_name, vm_folder, relocate_spec, user_id):
    """

    :param template: template VM managed object - can be a normal VM (vim.VirtualMachine)
    :param vm_name: name the new VM will have (str)
    :param vm_folder: VIM folder object where the new VM will be created
    :param relocate_spec: VIM object describing the location
    :param user_id: the ID of the user this VM is for (str(UUID))
    :return: the new VM object (vim.VirtualMachine)
    """
    ensure_template_snapshot(template)

    guestinfos = [vim.OptionValue(
        key=USER_IDENTITY_FIELD_NAME,
        value=user_id
    )]

    vm_spec = vim.vm.ConfigSpec(extraConfig=guestinfos)
    clone_spec = vim.vm.CloneSpec(
        powerOn=True,
        template=False,
        location=relocate_spec,
        snapshot=template.snapshot.rootSnapshotList[0].snapshot,
        config=vm_spec
    )

    task = template.Clone(name=vm_name, folder=vm_folder, spec=clone_spec)
    WaitForTask(task)
    logging.info(f'Successfully cloned template {template.name} into a new VM named {vm_name}.')
    return task.info.result


def deploy_virtual_machine(vm_name, cluster_name, template_name, user_id):
    """
    Deploy a new VM. The deployed VM will have its `USER_IDENTITY_FIELD_NAME` guestinfo variable set to `user_id`.

    :param vm_name: Name to deploy as (str)
    :param cluster_name: Cluster to deploy into. (str)
    :param template_name: Template (or VM) to linked-clone from. (str)
    :param user_id: GUID of the user for which this VM is being deployed. (str)
    :return: tuple of ID of the new VM after creation and its final name ( (str(UUID), str) )
    """
    with ServiceInstance() as si:
        content = si.RetrieveContent()

        cluster = get_vmware_obj(content, [vim.ClusterComputeResource], cluster_name)
        item = cluster
        while item.parent is not None:
            item = item.parent
            if isinstance(item, vim.Datacenter):
                break
        datacenter = item
        vm_folder = datacenter.vmFolder

        template = get_vmware_obj(content, [vim.VirtualMachine], template_name, vm_folder)
        relocate_spec = make_relocate_spec(cluster.host, cluster.resourcePool)

        vm = clone_template(template, vm_name, vm_folder, relocate_spec, user_id)
        return vm.config.uuid


def user_details(team_name, user_name, view_name=None):
    """
    Retrieves JSON config data for a given user on a given team.

    :param team_name: (str)
    :param user_name: (str)
    :param view_name: (str)
    :return: dict containing 'cluster' which is the cluster name, 'templates' which is a dict of template names to
             clone with values being what to name the template when it is cloned.
    """
    cluster = ''
    templates = []

    logging.debug(f'Team Name: {team_name}, User Name: {user_name}')

    json_obj = JSON_CACHE

    for view in json_obj['view']:
        if view_name is None or view_name == view.get('name'):
            cluster = view.get('cluster', cluster)
            templates = view.get('templates', templates)

            for team in view.get('teams', []):
                if team['name'] == team_name:
                    cluster = team.get('cluster', cluster)
                    templates = team.get('templates', templates)

                    for user in team.get('users', []):
                        if user['name'] == user_name:
                            cluster = user.get('cluster', cluster)
                            templates = user.get('templates', templates)

    hasher = hashlib.sha256()
    hasher.update(f'{str(team_name)}{str(user_name)}{str(cluster)}{str(templates)}'.encode())
    templates_dict = {}
    for template in templates:
        templates_dict[template] = (f'{team_name[:VM_NAME_TEAM_LENGTH_LIMIT]}-'
                                    f'{user_name[:VM_NAME_USER_LENGTH_LIMIT]}-'
                                    f'{template[:VM_NAME_TEMPLATE_LENGTH_LIMIT]}-'
                                    f'{hasher.hexdigest()[:8]}')

    return {'cluster': cluster, 'templates': templates_dict}


@app.cli.command()
@click.option('--config', default='welder.json', help='Optional config file. Defaults to welder.json')
def config(config):
    """Assign JSON config file. Defaults to welder.json"""
    click.echo('Assign JSON config file. Defaults to welder.json')
    app.config['jsonfile'] = config
    logging.debug('Launching with JSON file %s' % config)
    app.run()


@app.route('/')
def index():
    return ['/login', '/api/<view_id>']


class AuthError(Exception):
    def __init__(self, error, status_code):
        self.error = error
        self.status_code = status_code


@app.errorhandler(AuthError)
def handle_auth_error(ex):
    response = jsonify(ex.error)
    response.status_code = ex.status_code
    return response


def get_token_auth_header():
    """Obtains the Access Token from the Authorization Header
    """
    auth = request.headers.get("Authorization", None)
    if not auth:
        raise AuthError({"code": "authorization_header_missing",
                        "description":
                            "Authorization header is expected"}, 401)

    parts = auth.split()

    if parts[0].lower() != "bearer":
        raise AuthError({"code": "invalid_header",
                        "description":
                            "Authorization header must start with"
                            " Bearer"}, 401)
    elif len(parts) == 1:
        raise AuthError({"code": "invalid_header",
                        "description": "Token not found"}, 401)
    elif len(parts) > 2:
        raise AuthError({"code": "invalid_header",
                        "description":
                            "Authorization header must be"
                            " Bearer token"}, 401)

    token = parts[1]
    return token


def requires_auth(f):
    """Determines if the Access Token is valid
    """
    @wraps(f)
    def decorated(*args, **kwargs):
        token = get_token_auth_header()
        logging.info(f'Token validation got {token}')
        jwks = JWKS
        unverified_header = jwt.get_unverified_header(token)
        rsa_key = {}
        for key in jwks["keys"]:
            if key["kid"] == unverified_header["kid"]:
                rsa_key = {
                    "kty": key["kty"],
                    "kid": key["kid"],
                    "use": key["use"],
                    "n": key["n"],
                    "e": key["e"]
                }
        if rsa_key:
            try:
                payload = jwt.decode(
                    token,
                    rsa_key,
                    algorithms=[IDENTITY_SIGNING_ALGORITHM],
                    audience=IDENTITY_AUDIENCE,
                    issuer=f'{IDENTITY_URL_VALIDATION_OVERRIDE}'
                )
            except jwt.ExpiredSignatureError:
                raise AuthError({"code": "token_expired",
                                "description": "token is expired"}, 401)
            except jwt.JWTClaimsError:
                raise AuthError({"code": "invalid_claims",
                                "description":
                                    "incorrect claims,"
                                    "please check the audience and issuer"}, 401)
            except Exception:
                raise AuthError({"code": "invalid_header",
                                "description":
                                    "Unable to parse authentication"
                                    " token."}, 401)

            _request_ctx_stack.top.current_user = payload
            return f(*args, **kwargs)
        raise AuthError({"code": "invalid_header",
                        "description": "Unable to find appropriate key"}, 401)
    return decorated


def requires_scope(required_scope):
    """Determines if the required scope is present in the Access Token
    Args:
        required_scope (str): The scope required to access the resource
    """
    token = get_token_auth_header()
    unverified_claims = jwt.get_unverified_claims(token)
    if unverified_claims.get("scope"):
            token_scopes = unverified_claims["scope"].split()
            for token_scope in token_scopes:
                if token_scope == required_scope:
                    return True
    return False


def get_deploys(user_id, view_name=None):
    # Workaround for view names coming from Player encoded so that spaces are replaced
    # with the '+' character: https://INTERNAL_SOURCE_REPO/bitbucket/projects/S3/repos/s3.player.api/browse/S3.Player.Api/Infrastructure/Mappings/ApplicationProfile.cs#39
    view_name = urllib.parse.unquote_plus(view_name)
    
    token = get_token_auth_header()
    view_info = requests.get(
        f'{PLAYER_API_URL}/users/{user_id}/view-memberships',
        verify=VERIFY_SSL,
        headers={'authorization': f'Bearer {token}', 'accept': 'text/plain'}
    )

    deploys = []
    try:
        json_obj = view_info.json()
    except json.JSONDecodeError:
        return jsonify('Error parsing response from scenario-player. '
                       f'Got {view_info.text}, status code {view_info.status_code}')

    for view in json_obj:
        if view_name is None or view_name == view.get('viewName'):
            details_dict = user_details(
                view.get('primaryTeamName'),
                view.get('userName'),
                view_name
            )
            details_dict['team_id'] = view.get('primaryTeamId')
            details_dict['view_id'] = view.get('viewId')
            deploys.append(details_dict)
    return deploys


def register_new_vm(vm_name, vm_id, team_id, user_id, token):
    vm_url = f'{VM_CONSOLE_WEB_URL}/vm/{vm_id}/console'

    payload = {
        'id': vm_id,
        'url': vm_url,
        'name': vm_name,
        'teamIds': [team_id],
        'userId': user_id
    }
    headers = {
        'authorization': f'Bearer {token}',
        'accept': 'text/plain'
    }

    response = requests.post(
        f'{VM_API_URL}/api/vms',
        verify=VERIFY_SSL,
        headers=headers,
        json=payload
    )
    logging.debug(f'{VM_API_URL}/api/vms')
    logging.debug(f'{headers}')
    logging.debug(f'{payload}')
    logging.info(f'Attempted to register new VM with Player and got status code: {response.status_code}')
    logging.debug(f'If response is 403, ensure the user identified by TOKEN_USERNAME has management permissions on the Player team the VM will be added to.')


def get_register_token():
    token_data = {
        'grant_type': 'password',
        'username': TOKEN_USERNAME,
        'password': TOKEN_PASSWORD,
        'scope': VM_API_SCOPES
    }
    logging.debug(token_data)
    logging.debug(TOKEN_CLIENT_ID)
    # The second entry of the auth tuple needs to be a non-empty stripped string, but I don't know what it specifically
    # is for.
    with CURRENT_TOKEN_LOCK:
        if CURRENT_TOKEN['refresh_after'] < datetime.datetime.now():
            token_response = requests.post(
                f'{IDENTITY_URL}/connect/token',
                data=token_data,
                auth=(TOKEN_CLIENT_ID, 'a')
            )
            if not token_response.status_code == 200:
                raise Exception('Got a bad token response.')
            data = token_response.json()
            logging.debug(f'Refreshed token and got data:\n {data}')
            token = data['access_token']
            # This is the number of seconds the token is good for. Often an hour, but not necessarily.
            time_until_expires = data['expires_in']
            # If we wait until it expires, we're going to end up having some jobs fail. Let some of its time expire,
            # and then refresh it when it's in the last quarter of its lifetime.
            refresh_after = datetime.datetime.now() + datetime.timedelta(
                seconds=int(time_until_expires * 3 / 4)
            )
            CURRENT_TOKEN['token'] = token
            CURRENT_TOKEN['refresh_after'] = refresh_after
        else:
            token = CURRENT_TOKEN['token']
    return token


def worker(deployment_queue):
    """ Service deployments sent in the deployment queue.

    :param deployment_queue: queue.Queue
    :return:
    """
    global COMPLETED_TASKS
    while True:
        user_id, task = deployment_queue.get()
        # NOTE: There is technically a tiny window here where a queue check could look at the deployment queue and not
        # find anything, and before the task is added into the current task dict, it could check there and also not find
        # anything. However, this should be fairly rare and even when it does happen it's pretty benign.
        with CURRENT_TASK_DICT_LOCK:
            CURRENT_TASK_DICT[user_id] = task
        deploys = task['deploys']
        try:
            token = get_register_token()
        except Exception as e:
            logging.exception(e)
            token = None

        i = 0  # In case the same user has more than one template that are identical, we are adding a counter.
        for deploy in deploys:
            logging.debug(f'Got new deploy in worker thread: {deploy}')
            cluster = deploy['cluster']

            for (template, vm_name) in deploy['templates'].items():
                # This could exceed the VMWare VM name length limit, in the extremely unlikely case that the counter
                # reaches 5 digits.
                vm_name = f'{vm_name}-{str(i)}'
                try:
                    vm_id = deploy_virtual_machine(vm_name, cluster, template, user_id)
                except Exception as e:
                    logging.exception(e)
                else:
                    try:
                        if token:
                            register_new_vm(vm_name, vm_id, deploy['team_id'], user_id, token)
                    except Exception as e:
                        logging.exception(e)
                    i += 1
        with CURRENT_TASK_DICT_LOCK:
            CURRENT_TASK_DICT.pop(user_id)
        with COMPLETED_TASKS_LOCK:
            COMPLETED_TASKS += 1


@app.route('/api/<view_name>', methods=['GET'])
@requires_auth
def deploys_get(view_name=None):
    user = _request_ctx_stack.top.current_user
    user_id = user['sub']

    deploys = get_deploys(user_id, view_name)
    return jsonify(deploys)


@app.route('/api/<view_name>', methods=['POST'])
@requires_auth
def deploys_post(view_name=None):
    global POSITION_TRACKER
    user = _request_ctx_stack.top.current_user
    user_id = user['sub']

    try:
        deploys = DEPLOYMENT_QUEUE[user_id]['deploys']
    except KeyError:
        try:
            with CURRENT_TASK_DICT_LOCK:
                deploys = CURRENT_TASK_DICT[user_id]['deploys']
        except KeyError:
            pass
        else:
            return jsonify(deploys)
    else:
        return jsonify(deploys)

    deploys = get_deploys(user_id, view_name)
    logging.debug(deploys)
    with POSITION_TRACKER_LOCK:
        POSITION_TRACKER += 1

    task = {'deploys': deploys, 'position': POSITION_TRACKER}
    DEPLOYMENT_QUEUE.put((user_id, task))

    return jsonify(deploys)


@app.route('/test/<view_name>')
@requires_auth
def test(view_name=None):
    user = _request_ctx_stack.top.current_user
    user_id = user['sub']

    return jsonify(f'Got request from user: {user_id} and view {view_name}')


@app.route('/queue')
@requires_auth
def queue():
    user = _request_ctx_stack.top.current_user
    user_id = user['sub']
    try:
        task = DEPLOYMENT_QUEUE[user_id]
    except KeyError:
        try:
            with CURRENT_TASK_DICT_LOCK:
                _ = CURRENT_TASK_DICT[user_id]
        except KeyError:
            return jsonify(None)
        else:
            return jsonify('Your request is currently being handled.')
    else:
        # The user is in the queue.
        position = task['position']
        with COMPLETED_TASKS_LOCK:
            # Subtract WORKER_THREAD_COUNT because as soon as a worker thread gets a task, that task is removed from the
            # deployment queue.
            current_position = position - COMPLETED_TASKS - WORKER_THREAD_COUNT

        return jsonify(f'Your position in the VM deployment queue is: {current_position}')


# Hack because the worker function isn't declared until near the end of the file.
spawn_workers()

