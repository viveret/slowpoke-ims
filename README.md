# SlowPoke IMS: Information Management System
Like a content management system, but for power users.


## What does it do?
- Add drives, connected devices, and local/remote network machines to your unified group of instances
- Search and view files and folders across all of your devices
- Preview notes, images, PDFs, sound clips, videos, and more within the application
- Create, edit, delete, or open in an external program files and projects
- (goal) Link together notes, images, videos, sound clips, and documents into a dynamic document
- (goal) Automatically export newest version of dynamic documents as PDF or HTML
- (goal) Integrate with open protocols for automated social media posts as scheduled blog posts

(goal) = to do / in progress

See TODO.md for details list of goals and in progress features.


## What is the tech stack?
.NET Core 6, XUnit, Razor, Stacks


## Project / Folder Structure
- *.core = library project
- *.web = MVC related code
- *.tests = unit tests project
- *.integration.tests = longer, more complex tests that utilize multiple services, projects, or running servers