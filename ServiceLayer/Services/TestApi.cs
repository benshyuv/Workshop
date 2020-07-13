using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Http;

namespace ServiceLayer.Services
{
    [Route("wsep/test")]
    public class TestApi : ApiController
    {
        [HttpGet]
        public string TestGet()
        {
            return "Works";
        }
    }
}
