using NUnit.Framework;

namespace Authify.Test
{
    public class Tests
    {
        private Wlniao.Authify.Client client;
        [SetUp]
        public void Setup()
        {
            //client = new Wlniao.Authify.Client();
            client = new Wlniao.Authify.Client("http://127.0.0.1:4999", "0ecnpfe5mfs57d9tqojmo4qsi2mgy06r");
        }

        [Test]
        public void Test1()
        {
            var mobile = "18623107751";
            var send = client.SendRandPwd(mobile, "123456");
            if (send.success)
            {
                var verify = client.CheckRandPwd(mobile, "123456");
                if (verify.success)
                {
                    var query = client.GetAccount(verify.data);
                    Assert.AreEqual(query.data.mobile, mobile);
                }
                else
                {
                    Assert.Fail(verify.message);
                }
            }
            else
            {
                Assert.Fail(send.message);
            }
        }
        [Test]
        public void Test2()
        {
            var mobile = "18623107751";
            var query = client.GetAccount("222");
            if (query.success)
            {
                Assert.AreEqual(query.data.mobile, mobile);
            }
            else
            {
                Assert.Fail(query.message);
            }
        }
    }
}