using System;

namespace Framework.Environment.Extensions.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SystemFeatureAttribute : Attribute {
        public SystemFeatureAttribute(string text) {
            FeatureName = text;
        }

        public string FeatureName { get; set; }
    }
}