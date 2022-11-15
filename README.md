# subscription-based-clock

This is a subscription based clock.

Feel free to open a pull-request if you would find an issue with this project.

The code is very well documented, if you don't have dotnet installed, you should be able to run this
project in a Docker container using the following commands:

```
docker build -t subscription-clock -f Dockerfile .
docker run -it --rm -p 8080:80 -e PORT=80 subscription-clock
```

Once the service is running, one can navigate to the URL: http://localhost:8080 and see a [swagger ui page](https://editor.swagger.io), explaining how the api works


