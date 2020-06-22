"""
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
"""

import requests
import threading
import time


THREADS = 200
REQ_PER_THREAD = 100


def request_thread(token):
    headers = {'Authorization': f'Bearer {token}'}
    for i in range(REQ_PER_THREAD):
        r = requests.post('http://localhost:5000/api/Welder%20Test%20View', headers=headers)
        if r.status_code != 200:
            print(f'Got code {r.status_code}')


def main():
    token_data = {
        'grant_type': 'password',
        'username': '',
        'password': '',
        'scope': 's3',
    }

    token_response = requests.post(
        'http://localhost:10000/connect/token',
        data=token_data,
        auth=('welder', 'a')
    )
    if not token_response.status_code == 200:
        raise Exception('Got a bad token response.')
    else:
        print(token_response.json())

    token = token_response.json()['access_token']

    start = time.time()
    pool = []
    for i in range(THREADS):
        t = threading.Thread(target=request_thread, args=(token,))
        t.start()
        pool.append(t)

    for t in pool:
        t.join()

    print(f'With {THREADS} threads and {REQ_PER_THREAD} requests per thread, took {time.time() - start}')


if __name__ == '__main__':
    main()

