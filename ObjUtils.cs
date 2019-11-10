﻿// (c) Nick Polyak 2018 - http://awebpros.com/
// License: Apache License 2.0 (http://www.apache.org/licenses/LICENSE-2.0.html)
//
// short overview of copyright rules:
// 1. you can use this framework in any commercial or non-commercial 
//    product as long as you retain this copyright message
// 2. Do not blame the author(s) of this software if something goes wrong. 
// 
// Also, please, mention this software in any documentation for the 
// products that use it.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace NP.Utilities
{
    public static class ObjUtils
    {
        public static bool ReferenceEq(this object obj1, object obj2)
        {
            return ReferenceEquals(obj1, obj2);
        }

        public static bool ObjEquals(this object obj1, object obj2)
        {
            if (obj1 == obj2)
                return true;

            if ( (obj1 != null) && (obj1.Equals(obj2)))
                return true;

            return false;
        }

        public static int GetHashCodeExtension(this object obj)
        {
            if (obj == null)
                return 0;

            return obj.GetHashCode();
        }


        public static T[] ObjToCollection<T>(this T obj)
        {
            if (obj == null)
            {
                return new T[0];
            }

            return new T[] { obj };
        }



        public static string ToStr(this object obj)
        {
            if (obj == null)
                return string.Empty;

            return obj.ToString();
        }

        public static object ConvertToType(this Type resultType, object sourceValue)
        {
            if ( (sourceValue == null) || resultType.IsAssignableFrom(sourceValue.GetType()))
            {
                return sourceValue;
            }
            else
            {
                TypeConverter typeConverter = null;

                if (resultType.IsAbstract)
                {
                    TypeConverterAttribute attr =
                        resultType.GetCustomAttributes(typeof(TypeConverterAttribute), false).FirstOrDefault() as TypeConverterAttribute;

                    if (attr != null)
                    {
                        string typeConverterTypeName = attr.ConverterTypeName.SubstrFromTo(null, ",");

                        if (typeConverterTypeName != null)
                        {
                            Type typeConverterType = ReflectionUtils.FindTypeByFullName(typeConverterTypeName);

                            if (typeConverterType != null)
                            {
                                typeConverter = Activator.CreateInstance(typeConverterType) as TypeConverter;
                            }
                        }
                    }
                }
                else
                {
                    typeConverter = TypeDescriptor.GetConverter(resultType);
                }

                if (sourceValue is string strVal)
                {
                    if (typeConverter == null || (!typeConverter.CanConvertFrom(sourceValue.GetType())))
                    {
                        if (resultType.IsCollection())
                        {
                            var strItems =
                                 strVal.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                       .Select(str => str.Trim()).ToArray();

                            Type collectionCellType =
                                resultType.GenericTypeArguments.FirstOrDefault();

                            if (collectionCellType == null)
                                return null;

                            IList resultList;
                            if (resultType.IsAbstract)
                            {
                                Type collectionType = typeof(List<>).MakeGenericType(collectionCellType);
                                resultList = Activator.CreateInstance(collectionType) as IList;
                            }
                            else
                            {
                                resultList = Activator.CreateInstance(resultType) as IList;
                            }

                            if (resultList == null)
                            {
                                return null;
                            }
                            
                            foreach (string strItem in strItems)
                            {
                                resultList.Add(collectionCellType.ConvertToType(strItem));
                            }

                            return resultList;
                        }

                        return null;
                    }
                }

                return typeConverter?.ConvertFrom(sourceValue);
            }
        }

        public static TTarget TypeConvert<TTarget>(this object source)
        {
            object resultObj = typeof(TTarget).ConvertToType(source);

            return resultObj.ObjToType<TTarget>();
        }

        public static TTarget ObjToType<TTarget>(this object source)
        {
            if (source == null)
                return default(TTarget);

            return (TTarget)source;
        }
    }
}
