VECTO-CSE: Development
======================

### BUILD
Before compiling you need to add/check references to:

  * vectolic.dll

In order to use the latest `Vecto Hashing` and `Vecto Common` packages add _vecto nuget registry_:

```sh
dotnet nuget add source "https://code.europa.eu/api/v4/projects/89/packages/nuget/index.json" --name "gitlab-registry" --username <USERNAME> --password <PERSONAL_ACCESS_TOKEN>
```

or for more details follow the [Vecto Wiki](https://code.europa.eu/vecto/vectoci/-/wikis/VECTO-CI/setup#vecto-users-setup).

### EXECUTE
The following directories/files must be provided in the application folder (e.g. ..\bin\Debug):

  * vectolic.dll (check correct version)
  * Newtonsoft.Json.dll 6.0.0
  * license.dat
  * Declaration/
  * DemoData/
  * Docs/CSE-User Manual.pdf (generated from Word-file)

### RELEASE

For releasing follow the steps:
1. Update CHANGES.md with latest modifications
2. Run `Docs/convert_release_notes.ps1` in its containing folder.
3. Push the updated documentation.
4. Trigger a release pipeline by creating a new version tag.
5. Update GitLab tags.
6. Send announcement email.

### DEPRECATED *RELEASE* Section

⚠️ **Deprecated method, see New Releasing section.**

Checklist to build a new release:

1. (ask to) Create a new "version" in JIRA.
   * Mark all resolved issues in this new version.
2. Update CHANGES.md with work done since last release.
3. Make  zip-folder named with the "Semantic-version", ie: 2014_15_5-VECTO_CSE-2.0.1-beta1.
4. Copy into it:
    * executable (`.EXE` and optionally `.PDB` when a prerelease)
    * vectolic.dll (check correct version)
    * Declaration/ (With all its files marked as READONLY!!)
    * DemoData/
    * Docs/CSE-User Manual.pdf (*remember* to generate from Word-file and delete irrelevant files)
    * Docs/GenericData.xlsx
    * README.md
    * CHANGES.md
    * COPYING.txt
5. Make a temp-copy of the folder and run it with a license.
    * Does the "User Manual" link in Help-menu open?
6. Check everything alright, or else go back to step (2).
7. Issue a Pull-request to CITNet with all latest changes.
8. ZIP the original folder.
9. Upload into CITNet's SVN:
    https://webgate.ec.europa.eu/CITnet/svn/VECTO/trunk/Share/
  and link from:
    https://webgate.ec.europa.eu/CITnet/confluence/display/VECTO_CSE/Releases
1.  Make licenses and update private pages
2.  Tag repos in git.
3.  Send announcement email through JIRA (ie see VECTO-28).
    * Linking any fixed ISSUES in the announcement and Wiki's release-page.
