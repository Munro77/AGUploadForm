# Astley Gilbert Upload Form
*Technical Documentation*

<!-- TOC depthFrom:2 depthTo:6 withLinks:1 updateOnSave:1 orderedList:0 -->

- [Credentials and locations](#credentials-and-locations)
	- [Production](#production)
	- [Staging](#staging)
- [Administration Tasks](#administration-tasks)
	- [Restarting the Application](#restarting-the-application)
		- [Restart Using IIS Manager](#restart-using-iis-manager)
		- [Restart using Command line](#restart-using-command-line)
	- [Checking Log Files](#checking-log-files)
	- [Occasional Cleanup of Data Drive](#occasional-cleanup-of-data-drive)
- [Configuration Tasks](#configuration-tasks)
	- [Changing formsettings.json](#changing-formsettingsjson)
	- [Changing appsettings.json](#changing-appsettingsjson)
- [Content Tasks](#content-tasks)
	- [Changing “copy” on the site pages](#changing-copy-on-the-site-pages)

<!-- /TOC -->

## Credentials and locations

### Production

|    Item  | Value      |
| :------------- | :------------- |
| IP address      | 207.34.241.20      |
| RDP|207.34.241.20:3389|
|RDP User| t4g-astleygilbert|
|RDP Password| RobAG@2017|
|Application Pool User | AGUploadFormSystem |
|Application Pool Password | File$2017SubmitAG|
|Application File Location | E:\Sites\AGUploadForm |
|Temp Upload Location | E:\Sites\AGUploadForm\wwwroot\Uploads |
|Submitted Files Location | E:\Temp |
|Application Log Files Location | E:\Logs\AGUploadForm |

SQL Server Express  

|    Item  | Value      |
| :------------- | :------------- |
| Named Instance      | SQLEXPRESS      |
| sa Password | SQL$2017AG|
|Administrators | Current user and Builtin\Administrators|

### Staging

|    Item  | Value      |
| :------------- | :------------- |
| IP address      | 207.34.241.57      |
| RDP|207.34.241.57:3389|
|User| t4g-astleygilbert|
|Password| T4gAG@2016|

SQL Server Express  

|    Item  | Value      |
| :------------- | :------------- |
| Named Instance      | SQLEXPRESS      |
| sa Password | T4gAG@2016|
|Administrators | Current user and Builtin\Administrators|

## Administration Tasks
### Restarting the Application

This can be accomplished in two ways.

#### Restart Using IIS Manager

1. Open IIS Manager
    * Press the Windows Key or click the Start button and type “IIS”
    * This will autocomplete and bring up the Internet Information Services Manager
    * Start this application
2. Locate the AGUploadForm application
    * In the left hand panel, expand the server node of the tree
    * Expand the “Sites” node underneath the server
    * Click on the AGUploadForm application title to bring up the application configuration options
    * In the right hand panel, under “Manage Website” click Stop
    * Wait a few seconds for the process click Start
3. In the right hand panel, clicking “Browse \*:80” will open the browser and cause the site to re-start (may take a few moments)

#### Restart using Command line

1. Open a command prompt with administrative priveleges
    * While holding Shift, Right-Click on the Windows Start Menu button
    * Select "Command Prompt (Admin)" or "Windows Powershell (admin)"
2.  At the command prompt, type:
        net stop WAS

    and press ENTER
3. type Y and then press ENTER to also stop W3SVC
4. To restart the Web server, type:
        net start W3SVC

    and press ENTER to start both WAS and W3SVC.

    *Reference available at [Microsoft](https://technet.microsoft.com/en-us/library/cc732317%28v=ws.10%29.aspx?f=255&MSPPError=-2147217396)*

### Checking Log Files

### Occasional Cleanup of Data Drive

## Configuration Tasks
### Changing formsettings.json

The *formsettings.json* file is responsible for most configuration of the application, including file save and share locations, email addresses, branch/department configs etc.  Instructions for modifying the file are below:

1. The formsettings.json file is located in the root of the application directory
    * This is the folder that is the physical source of the AGUploadForm application configured in IIS Manager
    * The initial/default location for this folder is:
    > E:\Sites\AGUploadForm

2.	Before making any changes, make a backup of the configuration file    
    * Recommended approach is to store in another folder using a date stamp label (e.g. *E:\Backups\Feb28\formsettings.json*)
    * Back up either the files being updated or an entire copy of the AGUploadForm folder
3.	Modify the formsettings.json file with any new configurations
4.	Once the file is modified and saved, the application will need to be restarted in IIS
    * [Restarting the Application](#restarting-the-application)
    * Clicking “Browse \*:80” will open the browser and cause the site to re-start
5.	Test the changes that were updated in the formsettings.json on the site
    * If the site throws unexpected errors, re-check the changes to the file to ensure nothing is configured incorrectly

### Changing appsettings.json

The appsettings.json files are responsible for system configuration of the application, notably the email and database configurations

1.	The appsettings.json files are located in the root of the application directory
		* This is the folder that is the physical source of the AGUploadForm application configured in IIS Manager
		* The initial location for this folder is: E:\Sites\AGUploadForm
2.	Choose the appropriate file for editing
		* There is a file for each environment.
		* Appsettings.json is the “base” file and used in development and for storing the default values
		* Appsettings.production.json is the file used to store production specific values
3.	Before making any changes, make a backup of the required configuration file
		* Recommended approach is to store in another folder using a datastamp label (i.e. E:\Backups\Feb28\appsettings.production.json)
		* Back up either the files being updated or an entire copy of the AGUploadForm folder
4.	Edit the file in a text editor such as Notepad++
5.	Saving the file should trigger a re-start of the application
		* You can force a restart by following [Restarting the Application](#restarting-the-application)
6.	Browse the application to test the changes

## Content Tasks
### Changing “copy” on the site pages
The site copy shown to the end user is stored in View pages in the application.  These can be edited/refreshed without recompiling or deploying the application.

1.	Locate the View file you would like to update in the application folder.
		* The default folder is E:\Sites\AGUploadForm
2.	The notable view files are stored in the \Views\Home sub folder
		* *Index.cshtml* – This is the main page containing all form items, as well as the alternative upload instructions
		* *FormSubmitted.cshtml* – This page contains the thank you content
3.	Backup the file being edited
		* Copy to an alternative location, ideally date stamped, such as E:\Backups\Feb28
4.	Open the file and edit the HTML as required.  
		* Use a text editor such as Notepad++
		* Generally avoid all content within the HTML <> tags or Razor markup (lines starting with “@”)
		* Change written content as necessary
5.	Save the file and browse to the application in the browser to see desired changes.  
		* *It may be necessary to force a re-fresh of the site (Ctrl-R or Ctrl-F5) to see the new content.*
