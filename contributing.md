# Contributing

If you are developing within the [Crucible Framework](https://github.com/cmu-sei/crucible) or building an application to work alongside the Crucible Framework, please consider contributing documentation to this repository. The documentation contained within is written in Markdown. The static site generator used to display it is [Material for MkDocs](https://squidfunk.github.io/mkdocs-material/) and the documentation is hosted by [GitHub Pages](https://pages.github.com/).

## Using GitHub DevContainers

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

4. Run `mkdocs serve` to build and serve the documentation site locally in your devcontainer. After a few seconds the command will complete, and you should see a message in the bottom-right corner that says: *Your application (mkdocs) running on port 8000 is available.* 
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

### Fork

1. You may also choose to **fork this repo**. This will create a copy of the repository under your own personal account, allowing you to experiment with the documentation and formatting without affecting the main project. It will also copy all Github Actions workflows so that you can build your own static Github Pages site.
2. After forking, you need to enable workflows since Github automatically disables them for forks. Click **Actions** and then click **I understand my workflows, go ahead and enable them**. There is only one workflow, *build-mkdocs* that will run when code is pushed to the main branch. This workflow will create a new branch titled *gh-pages* that we will use as a source for Github Pages.  You must also ensure that GitHub Actions has read/write permissions to the repository. Navigate to your project **Settings** and select **Actions** then **General** under **Code and automation**. Scroll to the bottom and select *Read and Write Permissions*. Make sure to **Save**.
3. Add your documentation as appropriate. All current documentation is under the *docs* directory.  If you are contributing a new application, create a new directory under *docs* for it. If adding screenshots or other media, add them under the *assets* directory. Keep in mind that Material for MkDocs prefers relative links when adding screenshots etc. to markdown documentation.
4. If adding new application documentaiton, adding it to the navigation header in the static GitHub Pages site is done in the *mkdocs.yml* file at the root of the repository.  Add a new section under the *nav:* header. If you want to nest further sections under your application sections, make sure each sub-section's relevant documenation is contained within its own subdirectory.
5. When you are satisified with your documentaiton, push to the *main* branch of your fork. This will trigger the *build-mkdocs* workflow which will create the *gh-pages* branch.
6. Confirm that the *build-mkdocs* workflow completed under **Actions**. If there is an error, first ensure that you set Github Actions permissions properly in Step 2. You can view the workflow run in the Actions sections to view specific error messages. Misconfigurations in *mkdocs.yml* can cause a failed run. If unable to resolve errors, please submit an issue to the main Crucible project.
7. Once the *build-mkdocs* workflow has succeeded, go to your project **Settings** and select **Pages**. Under *Build and Deployment* ensure that *Deploy from branch* is selected as the **Source** and choose the *gh-pages* branch and *root* directory. Make sure to **Save**.

If all steps are performed successfully, GitHub Pages will build a static site every time new code is pushed to the *main* branch of your fork. This site will be available at *https://[github-username].github.io/crucible*. This allows you to make and preview changes rapidly. If you are not seeing the changes, please check the *build-mkdocs* workflow for errors.

When you are satisfied with your documentation and want to contribute back to the main project, open a Pull Request and your contribution will be reviewed by a Crucible maintainer.
