# Contributing

If you are developing within the Crucible Framework or building an application to work alongside the Crucible Framework, please consider contributing documentation back to this repository. The documentation contained within is written in Markdown. The static site to display it is built using [Material for MkDocs](https://squidfunk.github.io/mkdocs-material/) and GitHub Pages.
If you are developing within the [Crucible Framework](https://github.com/cmu-sei/crucible) or building an application to work alongside the Crucible Framework, please consider contributing documentation to this repository. The documentation contained within is written in Markdown. The static site generator used to display it is [Material for MkDocs](https://squidfunk.github.io/mkdocs-material/) and the documentation is hosted by [GitHub Pages](https://pages.github.com/).

## Using GitHub DevContainers

### GitHub Dev Containers

The easiest way to contribute is to use GitHub dev containers to setup a development environment accessible through your browser. In that environment, you can create a new branch, make changes, preview changes, and create pull requests to the main repo.

1. From the main page of this repo, click the green "Code" button, click the "Codespaces" tab, and click the green "Create codespace on main" button. This will open up a new tab in your browser
2. Wait for the codespace to finish setting up. When done, you will be presented with an in-browser VSCode editor with a terminal open at the bottom. Watch the bottom terminal for a few setup commands to finish running. You will see a pip3 installing several prerequisites. When finished, the terminal will show `[@your-username -> /workspaces/crucible (main)`
3. Using the terminal, create a new branch: `git checkout -b [your-branch-name]`
4. Build the documentation site and serve it over a web server local to your dev container: `mkdocs serve`. After a few seconds the command will complete and you should see a popup in the bottom right of your editor that says "Your application running on port 8000 is available." Click the green "Open in Browser" button to see the documentation site in a new tab.  As long as you do not cancel the command in the terminal, this website will automatically update when any change is made to `mkdocs.yml` or any file in the `docs/` directory (if you keep the terminal open, you will see the build commands running every time you make a change).  If you need to run more terminal commands, use `ctrl+c` to cancel. Run `mkdocs serve` again when you need to preview changes.
5. Make changes to the documentation as necessary, previewing the changes as you go.
6. When you are ready to submit your documentation for review, cancel the `mkdocs` command in the terminal with `ctrl+c`. Commit and push changes to your branch: `git add *`, `git commit -m "[description commit message]"`, `git push --set-upstream origin [your branch name]`
7. Return to the main Crucible repo and switch to your branch. Click the green "Compare & Pull Request" button. Make any necessary changes to the PR title and comments, and click the green "Create Pull Request" button.
8. A Crucible maintainer will review your pull request and accept or reject changes.
The easiest way to contribute to the docs is to use **GitHub DevContainers** to setup a development environment accessible through your browser. In that environment, you can create a new branch, make changes, preview changes, and create pull requests (PR) to the main repository.

1. On the main page of this repo, click the green **Code** button, **Codespaces** tab, then **Create codespace on main** button. This opens the new codespace on a new tab in your browser.
2. Wait for the codespace to finish building. Once completed, you have an in-browser Visual Studio Code (VSCode) editor with a terminal open at the bottom. Watch the terminal for setup commands to finish running, and `pip3` will install several prerequisites. When finished, the terminal shows something like:

```bash
[@your-username -> /workspaces/crucible (main)
```

3. Using the terminal, create a new branch:

```bash
git checkout -b [your-branch-name]
```

4. Run `mkdocs serve` to build and serve the documentation site locally in your dev container. After a few seconds the command will complete, and you should see a message in the bottom-right corner that says: *Your application (mkdocs) running on port 8000 is available.*
5. Click the green **Open in Browser** button to see the Crucible Documentation site in a new tab. As long as you don't cancel the command in the terminal, the docs site automatically updates any time a change is made to `mkdocs.yml` or any file in the `docs/` directory.

   > If you keep the terminal open, you'll see the build commands running every time you make a change.  If you need to run more terminal commands, use `ctrl+c` to cancel. When you need to preview changes, run `mkdocs serve` again.

6. Edit the documentation as necessary, previewing your changes as you go.
7. When ready to submit your documentation for review, stop the `mkdocs` command by pressing `Ctrl+C` in the terminal.
8. Stage, commit, and push the changes to your branch:

```bash
git add *
git commit -m "[description commit message]"
git push --set-upstream origin [your branch name]
```

9. Return to the main Crucible repo and switch to your branch. Click the green **Compare & Pull Request** button. Edit the PR title and comments (if needed) and click the **Create Pull Request**.

A Crucible maintainer will review your PR and accept or reject your changes. Thank you!

## Forking the Repository

1. You may also choose to **fork this repo**. This will create a copy of the repository under your own personal account, allowing you to experiment with the documentation and formatting without affecting the main project. It will also copy all GitHub Actions workflows so that you can build your own static GitHub Pages site.
2. After forking, you need to enable workflows since GitHub automatically disables them for forks. Click **Actions** and then click **I understand my workflows, go ahead and enable them**. There is only one workflow, *build-mkdocs* that will run when code is pushed to the main branch. This workflow will create a new branch titled *gh-pages* that we will use as a source for GitHub Pages.  You must also ensure that GitHub Actions has read/write permissions to the repository. Navigate to your project **Settings** and select **Actions** then **General** under **Code and automation**. Scroll to the bottom and select *Read and Write Permissions*. Make sure to **Save**.
3. Add your documentation as appropriate. All current documentation is under the *docs* directory.  If you are contributing a new application, create a new directory under *docs* for it. If adding screenshots or other media, add them under the *assets* directory. Keep in mind that Material for MkDocs prefers relative links when adding screenshots etc. to markdown documentation.
4. If adding new application documentation, adding it to the navigation header in the static GitHub Pages site is done in the *mkdocs.yml* file at the root of the repository.  Add a new section under the *nav:* header. If you want to nest further sections under your application sections, make sure each sub-section's relevant documentation is contained within its own subdirectory.
5. When you are satisfied with your documentation, push to the *main* branch of your fork. This will trigger the *build-mkdocs* workflow which will create the *gh-pages* branch.
6. Confirm that the *build-mkdocs* workflow completed under **Actions**. If there is an error, first ensure that you set GitHub Actions permissions properly in Step 2. You can view the workflow run in the Actions sections to view specific error messages. Misconfigurations in *mkdocs.yml* can cause a failed run. If unable to resolve errors, please submit an issue to the main Crucible project.
7. Once the *build-mkdocs* workflow has succeeded, go to your project **Settings** and select **Pages**. Under *Build and Deployment* ensure that *Deploy from branch* is selected as the **Source** and choose the *gh-pages* branch and *root* directory. Make sure to **Save**.

If all steps are performed successfully, GitHub Pages will build a static site every time new code is pushed to the *main* branch of your fork. This site will be available at `https://[github-username].github.io/crucible`. This allows you to make and preview changes rapidly. If you are not seeing the changes, please check the *build-mkdocs* workflow for errors.
Forking the repository creates a copy of the repository under your account, allowing you to experiment without affecting the main project. This also copies all GitHub Actions workflows, including *build-mkdocs*, enabling you to build your own GitHub Pages site.

1. In the top-right corner of the repository page, click **Fork**. GitHub will create a copy of the repository under your account.
2. GitHub disables workflows by default for forks, so you need to enable them manually. Go to the **Actions** tab and click **I understand my workflows, go ahead and enable them**.
3. Next, grant GitHub Actions read/write permissions. Go to **Settings**, **Actions**, **General**. Scroll down to **Workflow permissions**, select **Read and Write Permissions**, and click **Save**.
4. To add new documentation, place your content in the `docs` directory. For a new application, create a subdirectory under `docs` specifically for the new app. Store any media files in the `assets` directory and ensure you use relative links when referencing them in your Markdown files.
5. If required, update the navigation by modifying the `mkdocs.yml` file to add new sections under the `nav:`header. Make sure that any sub-sections are placed in their respective subdirectories to keep the docs organized.
6. Commit and push to the `main` branch of your fork. This triggers the *build-mkdocs* workflow, which creates the `gh-pages` branch.
7. To verify workflow success, click the **Actions** tab to confirm that *build-mkdocs* has completed. If errors occur, verify that GitHub Actions permissions are correct (see Step 3 above) and review the workflow logs for errors, particularly in the `mkdocs.yml` file. If you're unable to resolve errors, please submit an issue to the Crucible project for help.
8. Finally, you'll configure GitHub Pages. Go to **Settings**, **Pages**. Under **Build and Deployment**, select **Deploy from branch**, then choose `gh-pages` as the branch and `root` as the directory. Click **Save**. From now on, GitHub Pages will build a static site whenever you push to the `main` branch. Your site will be available at: `https://[github-username].github.io/crucible`. If updates don’t appear, check the *build-mkdocs* workflow for errors.

When you are satisfied with your documentation and want to contribute back to the main Crucible project, open a Pull Request. Your contribution will be reviewed by a Crucible maintainer. Thank you!