using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test
{
    [TestClass]
    public class EstimationTest
    {
        [TestMethod]
        public void Constructor_Null_ValueIsNull()
        {
            // Arrange
            double? value = null;

            // Act
            var result = new Estimation(value);

            // Verify
            Assert.IsNull(result.Value);
        }

        [TestMethod]
        public void Constructor_Zero_ValueIsZero()
        {
            // Arrange
            double value = 0.0;

            // Act
            var result = new Estimation(value);

            // Verify
            Assert.AreEqual<double?>(value, result.Value);
        }

        [TestMethod]
        public void Constructor_3point3_ValueIs3point3()
        {
            // Arrange
            double value = 3.3;

            // Act
            var result = new Estimation(value);

            // Verify
            Assert.AreEqual<double?>(value, result.Value);
        }

        [TestMethod]
        public void Constructor_PositiveInfinity_ValueIsPositiveInfinity()
        {
            // Arrange
            double value = double.PositiveInfinity;

            // Act
            var result = new Estimation(value);

            // Verify
            Assert.AreEqual<double?>(value, result.Value);
        }

        [TestMethod]
        public void Constructor_NotANumber_ValueIsNaN()
        {
            // Arrange
            double value = double.NaN;

            // Act
            var result = new Estimation(value);

            // Verify
            Assert.AreEqual<double?>(value, result.Value);
        }

        [TestMethod]
        public void Equals_ZeroAndZero_ReturnsTrue()
        {
            // Arrange
            var target = new Estimation(0.0);
            var estimation2 = new Estimation(0.0);

            // Act
            var result = target.Equals(estimation2);

            // Verify
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Equals_3point6And3point6_ReturnsTrue()
        {
            // Arrange
            var target = new Estimation(3.6);
            var estimation2 = new Estimation(3.6);

            // Act
            var result = target.Equals(estimation2);

            // Verify
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Equals_6And2_ReturnsFalse()
        {
            // Arrange
            var target = new Estimation(6);
            var estimation2 = new Estimation(2);

            // Act
            var result = target.Equals(estimation2);

            // Verify
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Equals_DiferentObjectTypes_ReturnsFalse()
        {
            // Arrange
            var target = new Estimation(6);

            // Act
            var result = target.Equals(2.0);

            // Verify
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Equals_Null_ReturnsFalse()
        {
            // Arrange
            var target = new Estimation(6);

            // Act
            var result = target.Equals(null);

            // Verify
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Equals_NullEstimations_ReturnsTrue()
        {
            // Arrange
            var target = new Estimation();
            var estimation2 = new Estimation(null);

            // Act
            var result = target.Equals(estimation2);

            // Verify
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Equals_NullEstimationAndZero_ReturnsTrue()
        {
            // Arrange
            var target = new Estimation();
            var estimation2 = new Estimation(0.0);

            // Act
            var result = target.Equals(estimation2);

            // Verify
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Equals_PositiveInfinityAndPositiveInfinity_ReturnsTrue()
        {
            // Arrange
            var target = new Estimation(double.PositiveInfinity);
            var estimation2 = new Estimation(double.PositiveInfinity);

            // Act
            var result = target.Equals(estimation2);

            // Verify
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Equals_ZeroAndPositiveInfinity_ReturnsFalse()
        {
            // Arrange
            var target = new Estimation();
            var estimation2 = new Estimation(double.PositiveInfinity);

            // Act
            var result = target.Equals(estimation2);

            // Verify
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Equals_NaNAndNaN_ReturnsFalse()
        {
            // Arrange
            var target = new Estimation(double.NaN);
            var estimation2 = new Estimation(double.NaN);

            // Act
            var result = target.Equals(estimation2);

            // Verify
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Equals_ZeroAndNaN_ReturnsFalse()
        {
            // Arrange
            var target = new Estimation();
            var estimation2 = new Estimation(double.NaN);

            // Act
            var result = target.Equals(estimation2);

            // Verify
            Assert.IsFalse(result);
        }
    }
}
