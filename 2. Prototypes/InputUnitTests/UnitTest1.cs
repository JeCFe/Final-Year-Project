using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InputValidationTesting;
using Microsoft.VisualStudio.Services.FormInput;

namespace InputUnitTests
{
    [TestClass]
    public class InputTests
    {
        private InputValidation validation;

        public InputValidation Validation { get => validation; set => validation = value; }

        [TestInitialize]
        public void Initialise()
        {
            Validation = new InputValidation();
        }
        [TestCleanup]
        public void Cleanup() { }
        [TestMethod]
        public void NameValidationTest()
        {
            Assert.AreEqual(true, Validation.NameValidation("Abc"));
            Assert.AreEqual(true, Validation.NameValidation("Abc123"));
            Assert.AreEqual(true, Validation.NameValidation("Abc_123"));
            Assert.AreEqual(false, Validation.NameValidation("!"));
            Assert.AreEqual(false, Validation.NameValidation("!Abc"));
            Assert.AreEqual(false, Validation.NameValidation("!Abc123"));
        }
        [TestMethod]
        public void PasswordValidation()
        {
            Assert.AreEqual(true, Validation.PasswordValidation("AAbb!!11"));
        }
    }
}
