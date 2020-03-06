/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System;
using System.Net;

namespace Stackstorm.Api.Client.Exceptions
{
    /// <summary> Exception for signalling failed request errors. </summary>
    /// <seealso cref="T:System.Exception"/>
    public class FailedRequestException
        : Exception
    {
        public string FailureMessage;

        public Uri RequestUri;

        public HttpStatusCode StatusCode;

        public string ResponseMessage;

        /// <summary>
        ///  Initializes a new instance of the
        ///  Stackstorm.Api.Client.Exceptions.FailedRequestException class.
        /// </summary>
        /// <param name="message"> The message. </param>
        public FailedRequestException(string message) :
            base(message)
        {
        }

        /// <summary>
        ///  Initializes a new instance of the
        ///  FailedRequestException class.
        /// </summary>
        /// <param name="failureMessage">  Message describing the failure. </param>
        /// <param name="requestUri">    URI of the request. </param>
        /// <param name="statusCode">    The status code. </param>
        /// <param name="responseMessage"> Message describing the response. </param>
        public FailedRequestException(string failureMessage, Uri requestUri, HttpStatusCode statusCode, string responseMessage)
            : base(String.Format("{3}, Request to {0} failed with status '{1}', response was {2}", requestUri, statusCode,
                responseMessage, failureMessage))
        {
            FailureMessage = failureMessage;
            RequestUri = requestUri;
            StatusCode = statusCode;
            ResponseMessage = responseMessage;
        }
    }
}
