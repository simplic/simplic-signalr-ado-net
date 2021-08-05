using System;
using System.Text;
using Xunit;

namespace Simplic.SignalR.Ado.Net.Client.Test
{
    public class ConnectionStringParserTest
    {
        [Theory]
        [InlineData("{url=http://api-key@test.com/};key1=val1;key2=val2", "http://api-key@test.com/", "key1=val1;key2=val2")]
        [InlineData("{url=http://v\\}\\;sias@test.com/};key1=val1;key2=val2", "http://v}\\;sias@test.com/", "key1=val1;key2=val2")]
        [InlineData("   {url=http://v\\}\\;sias@test.com/};key1=val1;key2=val2  ", "http://v}\\;sias@test.com/", "key1=val1;key2=val2")]
        public void ParseConnectionStringTest_Success(string actualConnectionString, string expectedUrl, string expectedDbConnectionString)
        {
            var parseResult = ConnectionStringParser.ParseConnectionString(actualConnectionString, out ConnectionStringParameter parameter);

            Assert.True(parseResult);
            Assert.Equal(parameter.Url, expectedUrl);
            Assert.Equal(0, parameter.ErrorMessage.Length);
            Assert.Equal(parameter.DbConnectionString, expectedDbConnectionString);
        }

        [Theory]
        [InlineData(null, "Connection string must not be null or empty")]
        [InlineData("", "Connection string must not be null or empty")]
        [InlineData(" ", "Connection string must not be null or empty")]
        [InlineData("http://", "Connection string must start with SignalR endpoint url: `{url=http://your-key@sample.com/}<db-connection-string>`")]
        [InlineData("{http://", "Connection string must start with SignalR endpoint url: `{url=http://your-key@sample.com/}<db-connection-string>`")]
        [InlineData("{http://} ", "Connection string must contain database-connection-string: `{url=http://your-key@sample.com/}<db-connection-string>`")]
        public void ParseConnectionStringTest_Failed(string actualConnectionString, string expectedMessage)
        {
            var parseResult = ConnectionStringParser.ParseConnectionString(actualConnectionString, out ConnectionStringParameter parameter);

            Assert.False(parseResult);
            Assert.Equal(parameter.ErrorMessage, expectedMessage);
        }
    }
}
