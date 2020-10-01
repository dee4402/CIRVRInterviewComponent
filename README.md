# CIRVRInterviewComponent
Career Interview Readiness in Virtual Reality Project


Required: Unity Version 2019.3.11f1

TEXT TO SPEECH 

- You will need add add your own Azure key and end point for Text-To-Speech
- Go to "Assets\Scripts\Utility\ConversationManager"
- Open the ConversationManager.cs file
- On Lines 166 and 167, you need to add the primary key and the location for the Text-to-Speech Service to work.


DIALOG CHANGE AND TESTING

- Go to "\Assets\StreamingAssets\Data\JSON files"
- Open the Dialog-jw.json file 
- That is the format of the Dialog Object that works with our system. 
- You can modify the DialogText for this project. Do not change anything else. 
-We might need a connection (perhaps a database) from the WebApp to this Unity project to dynamically update the DialogObject to match 
 what will be input in the Dialogue Text box in the web app. 
