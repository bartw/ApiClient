# [ApiClient](http://github.com/bartw/ApiClient)

ApiClient contains a HttpWebRequest factory and a HttpWebRequest throttler for Windows Phone 7.
ApiClient is created and maintained by [Bart Wijnants](http://beewee.be).

##Getting Started

```C#
//Create a GET request
string uri = "api.mysite.com";
Method method = Method.Get;
string userAgent = "MyApp/1.0 +http://MyApp.com/
string contentType = null;
List<Parameter> paramters = null;
List<Headers> headers = null;
string contont = null;
string consumerKey = null;
string consumerSecret = null;
string accessKey = null;
string accessSecret = null;
string verifier = null;

var request = RequestFactory.GetRequest(uri, method, userAgent, contentType, parameters, headers, content, consumerKey, consumerSecret, accessKey, accessSecret, verifier)

//Create a request throttler with 5 requests per second and a 5 second timeout
var throttler = new RequestThrottler(5, 5000);

//Perform the request on the throtller
throttler.EnqueueRequest(
  request,
  response =>
  {
    try
    {
      var webResponse = response as HttpWebResponse;
      
      if (webResponse != null)
      {
        if (webResponse.StatusCode == HttpStatusCode.OK)
        {
          //do stuff
        }
        else
        {
          exceptionCallback.Invoke(new WebException(string.Format("The response came back with statuscode {0}.", (int)webResponse.StatusCode)));
        }
      }
      else
      {
        exceptionCallback.Invoke(new WebException("The response could not be processed."));
      }
    }
    catch (Exception e)
    {
      exceptionCallback.Invoke(e);
    }
  },
  exception =>
  {
    exceptionCallback.Invoke(exception);
  },
  Priority.High);
