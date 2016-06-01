##`dumpling` service
#####What is `dumpling` service?

`dumpling` is the name of the DotNet dump analytics cli, and its storage service. Given a dump file, the service extracts information and catalogues it for future review.

#####The Vision
A runtime developer is looking for bugs to fix, and visits the `dumpling` website. They are presented with a view of the most impactful runtime issues. If they come across a failure they'd like to investigate, they may download the offending zipped up dump file and extract it to their machines for further review.


#####`The Status`The Pieces
![For Context](images/drawing1.png)
[Events that emit in to EventHub](events.md)

1. Supported Users
   - Automated Infrastructure
   - Tools
   - Humans
2. `stable`[RESTful Front-End](rest.md)
   - **upload dump file**
   - Get Status
   - Retrieve Dump File
3. `stable` [State Table](state-and-storage.md)
4. `stable` [**dump + runtime artifacts storage**](state-and-storage.md)
5. `stable`**analysis** Infrastructure
	- (**queue**) Azure Topic
	- Workers
		- Ubuntu 14.04 VM
		- CentOS VM
6. `stable` [**analysis artifacts storage**](state-and-storage.md)
7. `stable` Report Viewing
   - `cut?` PowerBI for high-level status
   - `stable` [ASP.Net WebView for actionable data](http://aka.ms/dumpling)


 
