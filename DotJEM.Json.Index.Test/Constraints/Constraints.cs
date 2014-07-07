﻿using System;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using DotJEM.Json.Index.Test.Util;
using Newtonsoft.Json.Linq;
using NUnit.Framework.Constraints;

// ReSharper disable ParameterHidesMember
namespace DotJEM.Json.Index.Test.Constraints
{
    /// <summary/>
    public abstract class AbstractConstraint : Constraint
    {
        private bool result = true;
        private StringBuilder message;

        #region Core
        public override bool Matches(object actual)
        {
            try
            {
                base.actual = actual;
                message = new StringBuilder();
                DoMatches(actual);
                return result;
            }
            finally
            {
                result = true;
            }
        }

        protected abstract void DoMatches(object actual);

        /// <summary/>
        public override void WriteMessageTo(MessageWriter writer)
        {
            writer.WriteMessageLine(GetType().FullName + " Failed!");
            using (StringReader reader = new StringReader(message.ToString()))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                    writer.WriteMessageLine(line);
            }
        }

        /// <summary/>
        public override void WriteDescriptionTo(MessageWriter writer)
        {
        }
        #endregion

        /// <summary/>
        protected bool FailWithMessage(string format, params object[] args)
        {
            AppendLine(string.Format(format, args));
            return Fail();
        }

        /// <summary/>
        protected bool Fail()
        {
            return result = false;
        }

        /// <summary/>
        protected StringBuilder AppendFormat(string format, params object[] args)
        {
            return message.AppendFormat(format, args);
        }

        /// <summary/>
        protected StringBuilder AppendLine(string value)
        {
            return message.AppendLine(value);
        }

        /// <summary/>
        protected StringBuilder AppendLine()
        {
            return message.AppendLine();
        }
    }

    // ReSharper disable InconsistentNaming
    public class HAS
    // ReSharper restore InconsistentNaming
    {
        public static ResolvableConstraintExpression Property<T>(Expression<Func<T, object>> property)
        {
            return new ConstraintExpression().Property(property.GetPropertyInfo().Name);
        }

        public static IResolveConstraint JProperties(object expected)
        {
            //Note: If expected was not a JObject, Most likely used with an anonomous type...
            //      but this also means we can allow for actual business objects to be passed in directly.
            JObject jobj = expected as JObject ?? JObject.FromObject(expected);
            return new HasJsonPropertiesConstraint(jobj);
        }
    }

    public class HasJsonPropertiesConstraint : AbstractConstraint
    {
        private readonly JObject expectedJObject;

        public HasJsonPropertiesConstraint(JObject expectedJObject)
        {
            this.expectedJObject = expectedJObject;
        }

        protected override void DoMatches(object actual)
        {
            JObject actualJObject = actual as JObject;
            if (actualJObject == null)
            {
                FailWithMessage("Object was not a JToken");
                return;
            }

            CompareJObjects(expectedJObject, actualJObject);
        }

        private void CompareJObjects(JObject expected, JObject actual, string path = "")
        {
            foreach (JProperty expectedProperty in expected.Properties())
            {
                string propertyPath = string.IsNullOrEmpty(path) ? expectedProperty.Name : path + "." + expectedProperty.Name;

                JProperty actualProperty = actual.Property(expectedProperty.Name);
                if (actualProperty == null)
                {
                    FailWithMessage("Actual object did not contain a property named '{0}'", propertyPath);
                    continue;
                }

                if (actualProperty.Value.Type != expectedProperty.Value.Type)
                {
                    FailWithMessage("'{0}' was expected to be of type '{1}' but was of type '{2}'.",
                        propertyPath, expectedProperty.Value.Type, actualProperty.Value.Type);
                    continue;
                }

                JObject obj = expectedProperty.Value as JObject;
                if (obj != null)
                {
                    //Note: We compared types above, so we know they should pass for both in this case.
                    CompareJObjects(obj, (JObject)actualProperty.Value, propertyPath);
                }

                JArray array = expectedProperty.Value as JArray;
                if (array != null)
                {
                    //Note: We compared types above, so we know they should pass for both in this case.
                    CompareJArray(array, (JArray)actualProperty.Value, propertyPath);
                }

                JValue value = expectedProperty.Value as JValue;
                if (value != null)
                {
                    //Note: We compared types above, so we know they should pass for both in this case.
                    if (!value.Equals((JValue)actualProperty.Value))
                    {
                        FailWithMessage("'{0}' was expected to be '{1}' but was '{2}'.",
                            propertyPath, expectedProperty.Value, actualProperty.Value);
                    }
                }
            }
        }

        private void CompareJArray(JArray expectedArr, JArray actualArr, string propertyPath)
        {
            if(expectedArr.Count != actualArr.Count)
                FailWithMessage("'{0}' was expected to have '{1}' elements but had '{2}'.",
                        propertyPath, expectedArr.Count, actualArr.Count);

            for (int i = 0; i < expectedArr.Count; i++)
            {
                string itemPath = propertyPath + "[" + i + "]";
                JToken expectedToken = expectedArr[i];
                JToken actualToken = actualArr[i];

                if (expectedToken.Type != actualToken.Type)
                {
                    FailWithMessage("Item at [{0}] in '{1}' was expected to be of type '{2}' but was of type '{3}'.",
                        i, propertyPath, expectedToken.Type, actualToken.Type);
                    continue;
                }

                JObject obj = expectedToken as JObject;
                if (obj != null)
                {
                    //Note: We compared types above, so we know they should pass for both in this case.
                    CompareJObjects(obj, (JObject)actualToken, itemPath);
                }

                JArray array = expectedToken as JArray;
                if (array != null)
                {
                    //Note: We compared types above, so we know they should pass for both in this case.
                    CompareJArray(array, (JArray)actualToken, itemPath);
                }

                JValue value = expectedToken as JValue;
                if (value != null)
                {
                    //Note: We compared types above, so we know they should pass for both in this case.
                    if (!value.Equals((JValue)actualToken))
                    {
                        FailWithMessage("'{0}' was expected to be '{1}' but was '{2}'.",
                            itemPath, expectedToken, actualToken);
                    }
                }
            }
        }
    }
}
// ReSharper restore ParameterHidesMember
