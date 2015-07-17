# Opensim-Avatar-Archiver

##Creating Avatar Archive
##
To create an avatar archive, you'll first set up a user on your sim, and have that user wear all of 
   the clothing and attachments that you want the archive to have.
After this is done, then you'll type the following into the console

save avatar archive "First" "Last" "password" "Filename" "FolderNameToSaveInto" (--snapshot <UUID>) (--private)

in which <First> is the user from above's first name, <Last> is the user from above's last name,
<password> is the avatars account password,
  <Filename> is the file that you want to save it into (DO NOT add the extension .aa it is auto added),
  
  <FolderNameToSaveInfo> is the folder name that will be created when this is loaded into a new
  avatar. The last two parameters are optional, but necessary for setting up default avatars.
  Adding "--snapshot <UUID>" will assign a screenshot to the archive, allowing a picture of it to be
  set for the web interface. Adding "--private" will not allow the archive to be found by the web interface,
  and it's existance will disallow the archives use for default avatars.
  
##  Loading an avatar archive
##
If you want to load an avatar archive to a user (and replace their current appearance), you can load
an archive. To do this, make sure that the user is logged in, then type the following into the console

### Loading Avatar Archive from URL
###
load avatar archive "First" "Last" "password" "url"

exclude the quotes!

in which <First> is the user from above's first name, <Last> is the user from above's last name, <password> is the avatars password,
  <url> is the url or file that you want to load ex (http://example.com/avatar.aa) Once you type this, 
  it will load the archive.
  
### Loading Avatar Archive from file 
###
load avatar archive "First" "Last" "password" "filename"

exclude the quotes!

in which <First> is the user from above's first name, <Last> is the user from above's last name, <password> is the avatars password,
 <filename> (exclude the file extention) is the url or file that you want to load ex (avatarArchive) Once you type this, 
  it will load the archive.
  
