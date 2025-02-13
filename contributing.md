# Contributing

If you are developing within the Crucible Framework or building an application to work alongside the Crucible Framework, please consider contributing documentation back to this repository. The documentation contained within is written in Markdown. The static site to display it is built using [Material for MkDocs](https://squidfunk.github.io/mkdocs-material/) and GitHub Pages.

## Documentation Contribution Process

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

### Fork

1. You may also choose to **fork this repo**. This will create a copy of the repository under your own personal account, allowing you to experiment with the documentation and formatting without affecting the main project. It will also copy all GitHub Actions workflows so that you can build your own static GitHub Pages site.
2. After forking, you need to enable workflows since GitHub automatically disables them for forks. Click **Actions** and then click **I understand my workflows, go ahead and enable them**. There is only one workflow, *build-mkdocs* that will run when code is pushed to the main branch. This workflow will create a new branch titled *gh-pages* that we will use as a source for GitHub Pages.  You must also ensure that GitHub Actions has read/write permissions to the repository. Navigate to your project **Settings** and select **Actions** then **General** under **Code and automation**. Scroll to the bottom and select *Read and Write Permissions*. Make sure to **Save**.
3. Add your documentation as appropriate. All current documentation is under the *docs* directory.  If you are contributing a new application, create a new directory under *docs* for it. If adding screenshots or other media, add them under the *assets* directory. Keep in mind that Material for MkDocs prefers relative links when adding screenshots etc. to markdown documentation.
4. If adding new application documentation, adding it to the navigation header in the static GitHub Pages site is done in the *mkdocs.yml* file at the root of the repository.  Add a new section under the *nav:* header. If you want to nest further sections under your application sections, make sure each sub-section's relevant documentation is contained within its own subdirectory.
5. When you are satisfied with your documentation, push to the *main* branch of your fork. This will trigger the *build-mkdocs* workflow which will create the *gh-pages* branch.
6. Confirm that the *build-mkdocs* workflow completed under **Actions**. If there is an error, first ensure that you set GitHub Actions permissions properly in Step 2. You can view the workflow run in the Actions sections to view specific error messages. Misconfigurations in *mkdocs.yml* can cause a failed run. If unable to resolve errors, please submit an issue to the main Crucible project.
7. Once the *build-mkdocs* workflow has succeeded, go to your project **Settings** and select **Pages**. Under *Build and Deployment* ensure that *Deploy from branch* is selected as the **Source** and choose the *gh-pages* branch and *root* directory. Make sure to **Save**.

If all steps are performed successfully, GitHub Pages will build a static site every time new code is pushed to the *main* branch of your fork. This site will be available at `https://[github-username].github.io/crucible`. This allows you to make and preview changes rapidly. If you are not seeing the changes, please check the *build-mkdocs* workflow for errors.

When you are satisfied with your documentation and want to contribute back to the main project, open a Pull Request and your contribution will be reviewed by a Crucible maintainer.
