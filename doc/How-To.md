# How to build docker image locally:

 - Open powershell in `_src` folder
 - Run command `docker build -f .\zoka-test-suite.Console\Dockerfile .`

# How to publish official build of Zoka-Test-Suite
- Create tag in format `vX.X.X` and apply it to the revision to be published
- Push source source and tags into github
- Go to `Releases` subpage of the project ([link](https://github.com/zoka-cz/zoka-test-suite/releases)).
- Click `Draft new release` button, choose the released tag, do other stuff (like release notes) and click `Publish release`. This will automatically start build action which builds the docker image and uploads it to the docker hub.