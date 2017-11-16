using System;
using System.ComponentModel.DataAnnotations;

namespace Framework.Data.Conventions
{
    public class StringLengthMaxAttribute : Attribute {//: StringLengthAttribute {
        public StringLengthMaxAttribute(){ //: base(10000) {
            // 10000 is an arbitrary number large enough to be in the nvarchar(max) range 
        }
    }

//    public class StringLengthConvention : AttributePropertyConvention<StringLengthAttribute> {
//        protected override void Apply(StringLengthAttribute attribute, IPropertyInstance instance) {
//            instance.Length(attribute.MaximumLength);
//        }
//    }
}