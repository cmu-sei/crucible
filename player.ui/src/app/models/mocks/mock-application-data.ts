/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { ApplicationData } from '../application-data';

export const MockApplicationData: ApplicationData[] = [
  {
    id: '1',
    icon: 'assets/img/server-map.jpg',
    name: 'National Map',
    url: 'assets/demo/NAUS-National.png',
    embeddable: true,
    loadInBackground: false,
    applicationId: '',
    displayOrder: 1,
    viewId: '',
    safeUrl: null,
  },
  {
    id: '2',
    icon: 'assets/img/net-map.png',
    name: 'Tactical Map',
    url: 'assets/demo/NAUS-Tactical.png',
    embeddable: true,
    loadInBackground: false,
    applicationId: '',
    displayOrder: 2,
    viewId: '',
    safeUrl: null,
  },
  {
    id: '3',
    icon: 'assets/img/vm.png',
    name: 'Virtual Machines',
    url: 'http://localhost:4303/453d394e-bf18-499b-9786-149b0f8d69ec',
    embeddable: true,
    loadInBackground: false,
    applicationId: '',
    displayOrder: 3,
    viewId: '',
    safeUrl: null,
  },
  {
    id: '4',
    icon: 'assets/img/pdf.png',
    name: 'Intel Report',
    url: 'https://www.dni.gov/files/documents/ICA_2017_01.pdf',
    embeddable: true,
    loadInBackground: false,
    applicationId: '',
    displayOrder: 4,
    viewId: '',
    safeUrl: null,
  },
  {
    id: '5',
    icon: 'assets/img/chat.jpg',
    name: 'Chat',
    url: 'https://INTERNAL_CHAT_SERVER',
    embeddable: false,
    loadInBackground: false,
    applicationId: '',
    displayOrder: 5,
    viewId: '',
    safeUrl: null,
  },
  {
    id: '6',
    icon: 'assets/img/ghosts-icon.jpg',
    name: 'Ghosts',
    url: 'assets/demo/ghosts-screen-shot.png',
    embeddable: true,
    loadInBackground: false,
    applicationId: '',
    displayOrder: 6,
    viewId: '',
    safeUrl: null,
  },
  {
    id: '7',
    icon: 'assets/img/osticket.png',
    name: 'Help Desk',
    url: 'http://localhost/osticket/login.php?do=ext&bk=identity.client',
    embeddable: true,
    loadInBackground: false,
    applicationId: '',
    displayOrder: 7,
    viewId: '',
    safeUrl: null,
  },
];
