# Contributing

If you are developing within the [Crucible Framework](https://github.com/cmu-sei/crucible) or building an application to work alongside the Crucible Framework, please consider contributing documentation to this repository. The documentation contained within is written in Markdown. The static site generator used to display it is [Material for MkDocs](https://squidfunk.github.io/mkdocs-material/) and the documentation is hosted by [GitHub Pages](https://pages.github.com/).

## Using GitHub Dev Containers

The easiest way to contribute to the docs is to use **GitHub Dev Containers** to setup a development environment accessible through your browser. In that environment, you can create a new branch, make changes, preview changes, and create pull requests (PR) to the main repository.

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

   >If you keep the terminal open, you'll see the build commands running every time you make a change.  If you need to run more terminal commands, use `ctrl+c` to cancel. When you need to preview changes, run `mkdocs serve` again.

6. Edit the documentation as necessary, previewing your changes as you go.
7. When ready to submit your documentation for review, stop the `mkdocs` command by pressing `Ctrl+C` in the terminal.
8. Stage, commit, and push the changes to your branch:

   ```bash
   git add *
   git commit -m "[description commit message]"
   git push --set-upstream origin [your branch name]
   ```

9. Return to the main Crucible repo and switch to your branch. Click the green **Compare & pull request** button. Edit the PR title and comments (if needed) and click **Create Pull Request**.

A Crucible maintainer will review your PR and accept or reject your changes. Thank you!

## Forking the Repository

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