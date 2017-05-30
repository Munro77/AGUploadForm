# Astley Gilbert Upload Form
*Technical Documentation*

<!-- TOC depthFrom:2 depthTo:6 withLinks:1 updateOnSave:1 orderedList:0 -->

- [Administrative Tasks](#administrative-tasks)
	- [Changing formsettings.json](#changing-formsettingsjson)
	- [Changing appsettings.json](#changing-appsettingsjson)
	- [Restarting the Application](#restarting-the-application)
- [Content Tasks](#content-tasks)
	- [Changing “copy” on the site pages](#changing-copy-on-the-site-pages)

<!-- /TOC -->
## Administrative Tasks
### Changing formsettings.json
The formsettings.json file is responsible for most configuration of the application, including file save and share locations, email addresses, branch/department configs etc.  Instructions for modifying the file are below:

1. The formsettings.json file is located in the root of the application directory
  1. This is the folder that is the physical source of the AGUploadForm application configured in IIS Manager
  2. The initial location for this folder is E:\Sites\AGUploadForm
2.	Before making any changes, make a backup of the configuration file
  1. Recommended approach is to store in another folder using a date stamp label (i.e. E:\Backups\Feb28\formsettings.json)
  2. Back up either the files being updated or an entire copy of the AGUploadForm folder
3.	Modify the formsettings.json file with any new configurations
4.	Once the file is modified and saved, the application will need to be restarted.  
  * [Restarting the Application](#restarting-the-application)
  * Clicking “Browse \*:80” will open the browser and cause the site to re-start
5.	Test the changes that were updated in the formsettings.json on the site
a.	If the site throws unexpected errors, re-check the changes to the file to ensure nothing is configured incorrectly



### Changing appsettings.json

The appsettings.json files are responsible for system configuration of the application, notably the email and database configurations

1.	The appsettings.json files are located in the root of the application directory
a.	This is the folder that is the physical source of the AGUploadForm application configured in IIS Manager
b.	The initial location for this folder is E:\Sites\AGUploadForm
2.	Choose the appropriate file for editing
a.	There is a file for each environment.
b.	Appsettings.json is the “base” file and used in development and for storing the default values
c.	Appsettings.production.json is the file used to store production specific values
3.	Before making any changes, make a backup of the required configuration file
a.	Recommended approach is to store in another folder using a datastamp label (i.e. E:\Backups\Feb28\appsettings.production.json)
b.	Back up either the files being updated or an entire copy of the AGUploadForm folder
4.	Edit the file in a text editor such as Notepad++
5.	Saving the file should trigger a re-start of the application
  * You can force a restart by following [Restarting the Application](#restarting-the-application)
6.	Browse the application to test the changes


### Restarting the Application

This can be accomplished in IIS Manager:

1. Open IIS Manager
  1.	Press the Windows Key or click the Start button and type “IIS”
  2.	This will autocomplete and bring up the Internet Information Services Manager
  3.	Start this application
2. Locate the AGUploadForm application
  * In the left hand panel, expand the server node of the tree
  * Expand the “Sites” node underneath the server
  * Click on the AGUploadForm application title to bring up the application configuration options
  * In the right hand panel, under “Manage Website” click Stop
  * Wait a few seconds for the process click Start
3. In the right hand panel, clicking “Browse \*:80” will open the browser and cause the site to re-start (may take a few moments)

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
  * Generally avoid all content within the HTML or Razor markup (lines starting with “@”)
  * Change written content as necessary
5.	Save the file and browse to the application in the browser to see desired changes.  
  * *It may be necessary to force a re-fresh of the site (Ctrl-R or Ctrl-F5) to see the new content.*
