## Oswos
Oswos stands for 'OWIN simple web only server'. It is a simple http server which supports multiple websites hosted on a single machine. Just point the dns to your oswos server and GO!

# Goal
The goal is to build a webserver which can host multiple sites on an old netbook. This is not an heavy production webserver, but just for a site with 10-100 hits a day. It should be configurable from a website and the logging should be visible real time on this website using websockets.

# Design
You can see it on [prezi] [prezi]

# How it works
Oswos uses an async socket pattern with bufferpools. This is the recommended pattern to handle a lot (10000+) connections at fast speed.
The stream is then decoded to an HtmlStream, which reads the headers in an array and the body in a MemoryStream.
Once all headers are loaded, the data is parsed through a router to the specified WCF endpoint.

The available websites are all seperated by an AppDomain with the base-path at the root of the website. Therfore the website can always find the referenced paths. An WCF service is loaded in the AppDomain which accepts HttpRequestStreams. The router looks up the host as set in the header and passes the Stream to the WCF service in the AppDomain.

# The future of oswos
For me oswos was a learning project. I've learned to use GitHub and fork [nancyfx] [nancyfx]. I learned that building a fast webserver in .net is hard. If there are any people who want to help or try to fix some small bugs still in the code. Let me know.

[prezi]: http://prezi.com/ixi1xjeusn5j/oswos/ "oswos how it works"
[nancyfx]: https://github.com/NancyFx/Nancy "NancyFx"