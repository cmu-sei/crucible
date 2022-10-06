# Player How to: Upload files

## Upload from local to VM

These procedures describe how to upload files from a local machine (an _out-of-game_ computer) to a virtual machine in the Player VM Console app. Please note the **Send File to VM** option should only be used for uploading small files.

These procedures assume you are in the Player VM Console app. For help on Player applications, see the [Player Applications](https://cmu-sei.github.io/crucible/player-applications) guide.

1. In the Player VM Console app, in the VM List, launch the virtual machine you want.
2. On the VM tab, click the **gear icon**, and then select __Files__, **Send File to VM**.

> If the **Send File to VM** option is grayed out you will have to enter the credentials used to log into the virtual machine.
> Select __Enter VM Credentials__. In the VM Send File Settings window, enter the **Destination File Path**, **Username**, and **Password**. Click **Done**.
> Any user registered for the exercise will have the virtual machine credentials.

3. After entering your credentials, select the __Send File to VM__ option (this should no longer be grayed out). 
4. Select the file from your local machine that you want to upload.
5. Click __Open__. This will copy the file to the destination folder specified in the VM Send File Settings window.

## Upload files and mount ISO

These procedures describe how to upload files from a local machine to be mounted as a DVD (files are not uploaded directly to a virtual machine – it’s a two-step process). This is useful for installers and uploading larger files (such as ISO files).

1. In the VM List screen, click __Upload File__.
2. On your local machine, select the file you want to upload (the upload may take some time depending upon the file size, so you will see a progress bar highlighting the upload progress).
3. Click __Open__.
4. On the VM tab, click the **gear icon**, and then select __Files__, __Mount File to DVD__.
5. In the Search box that opens look for and select the uploaded file that you now want to mount.

   > Note that there are two file areas: Team Files and Public Files. Files can be mounted for only your team (under __Team Files__) or any team (under __Public Files__).

6. Click __Mount__. This process automatically mounts the file as a DVD Drive ISO.
7. After the ISO has been mounted/used for file copy, right-click the DVD Drive ISO and and select __Eject__ to eject the ISO.
