using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PsqlToCSClass
{
    class PropertyInfo
    {
        public string Name { get; set; }
        public string Datatype { get; set; }
        public bool isNotNullable { get; set; }
    }

    class ClassInfo
    {
        public string className { get; set; }
        public string baseClassName { get; set; }
        public List<PropertyInfo> propertyInfos { get; set; }
    }

    class Program
    {
        const string ClassNamespace = "TrakGud.DAL.Models";

        static void Main(string[] args)
        {
            // Generate the C# class files.

            string[] sqlLines = File.ReadAllLines("db.sql.txt");

            bool tBody = false;

            List<ClassInfo> classInfos = new List<ClassInfo>();

            string className = string.Empty;
            string baseClassName = string.Empty;
            List<PropertyInfo> propertyInfos = new List<PropertyInfo>();

            const string CT_ = "create table public.\"";
            const string ET_ = ")";
            const string IT_ = "inherits (public.\"";

            const string NNT_ = "not null";
            const string StringDTT1_ = "character";
            const string StringDTT2_ = "text";
            const string IntDTT_ = "integer";
            const string FloatDTT_ = "real";
            const string DatetimeDTT_ = "timestamp";

            for (int li = 0; li < sqlLines.Length; li++)
            {
                string l = sqlLines[li];

                if (tBody)
                {
                    if (l.ToLower().IndexOf(ET_) == 0)
                    {
                        tBody = false;
                        string l2 = (li + 1 != sqlLines.Length) ? sqlLines[li + 1] : "";

                        if (l2.ToLower().IndexOf(IT_) != -1)
                        {
                            string btn = l2.Split("\"", StringSplitOptions.RemoveEmptyEntries)[1];
                            string[] bcn_ = btn.Split("__", StringSplitOptions.RemoveEmptyEntries);
                            baseClassName = bcn_[bcn_.Length - 1];
                        }

                        //GenerateClassFile(className, baseClassName, propertyInfos);
                        ClassInfo classInfo = new ClassInfo
                        {
                            className = className,
                            baseClassName = baseClassName,
                            propertyInfos = propertyInfos
                        };
                        classInfos.Add(classInfo);

                        className = string.Empty;
                        baseClassName = string.Empty;
                        propertyInfos = new List<PropertyInfo>();
                    }
                    else
                    {
                        // Extract property info.

                        PropertyInfo pi = new PropertyInfo();

                        string afterColName;

                        if (l[4] == '\"')
                        {
                            string[] crp = l.Split("\"", StringSplitOptions.RemoveEmptyEntries);
                            pi.Name = crp[1];

                            afterColName = crp[2];
                        }
                        else
                        {
                            string[] crp = l.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                            pi.Name = crp[0];

                            afterColName = l.Substring(4 + crp[0].Length);
                        }

                        afterColName = afterColName.ToLower();

                        if (afterColName.Contains(NNT_))
                        {
                            pi.isNotNullable = true;
                        }
                        else
                        {
                            pi.isNotNullable = false;
                        }

                        if (afterColName.Contains(IntDTT_))
                        {
                            pi.Datatype = "int";
                        }
                        else if (afterColName.Contains(StringDTT1_) || afterColName.Contains(StringDTT2_))
                        {
                            pi.Datatype = "string";
                        }
                        else if (afterColName.Contains(FloatDTT_))
                        {
                            pi.Datatype = "float";
                        }
                        else if (afterColName.Contains(DatetimeDTT_))
                        {
                            pi.Datatype = "DateTime";
                        }

                        propertyInfos.Add(pi);
                    }
                }
                else if (l.ToLower().IndexOf(CT_) != -1)
                {
                    tBody = true;

                    string tn = l.Split("\"", StringSplitOptions.RemoveEmptyEntries)[1];
                    Console.WriteLine(tn);

                    string[] cn_ = tn.Split("__", StringSplitOptions.RemoveEmptyEntries);
                    className = cn_[cn_.Length - 1];
                    Console.WriteLine(className);
                }
            }

            GenerateClassFiles(classInfos);
        }

        private static void GenerateClassFiles(List<ClassInfo> classInfos)
        {
            // Create output "models" folder.

            Directory.CreateDirectory("models");

            foreach (ClassInfo classInfo in classInfos)
            {
                ClassInfo baseClassInfo;
                List<PropertyInfo> basePropertyInfos = null;
                if (!string.IsNullOrEmpty(classInfo.baseClassName))
                {
                    baseClassInfo = classInfos.Find(ci => ci.className == classInfo.baseClassName);
                    basePropertyInfos = baseClassInfo.propertyInfos;
                }
                GenerateClassFile(classInfo.className, classInfo.baseClassName, classInfo.propertyInfos, basePropertyInfos);
            }
        }

        private static void GenerateClassFile(string className, string baseClassName, List<PropertyInfo> propertyInfos, List<PropertyInfo> basePropertyInfos)
        {
            // Generate the text.

            StringBuilder classFileText = new StringBuilder();

            classFileText.Append("using System;\n");
            classFileText.Append("using System.Collections.Generic;\n");
            classFileText.Append("using System.Text;\n\n");
            classFileText.Append("namespace " + ClassNamespace + "\n{\n");
            classFileText.Append("	public class " + className);

            if (!string.IsNullOrEmpty(baseClassName))
            {
                classFileText.Append(": " + baseClassName);
            }

            classFileText.Append("\n	{\n");

            foreach (PropertyInfo pi in propertyInfos)
            {
                classFileText.Append("		public " + pi.Datatype + " " + pi.Name + " { get; set; }\n");
            }

            classFileText.Append("\n		public " + className + "(");

            bool isFirst = true;

            if (!string.IsNullOrEmpty(baseClassName))
            {
                foreach (PropertyInfo pi in basePropertyInfos)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        classFileText.Append(", ");
                    }
                    classFileText.Append(pi.Datatype + " " + pi.Name + "_");
                }
            }

            foreach (PropertyInfo pi in propertyInfos)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    classFileText.Append(", ");
                }
                classFileText.Append(pi.Datatype + " " + pi.Name + "_");
            }

            classFileText.Append(")");

            if (!string.IsNullOrEmpty(baseClassName))
            {
                classFileText.Append(": base(");

                bool isFirst2 = true;
                foreach (PropertyInfo pi in basePropertyInfos)
                {
                    if (isFirst2)
                    {
                        isFirst2 = false;
                    }
                    else
                    {
                        classFileText.Append(", ");
                    }
                    classFileText.Append(pi.Name + "_");
                }

                classFileText.Append(")\n");
            }

            classFileText.Append("		{\n");

            foreach (PropertyInfo pi in propertyInfos)
            {
                classFileText.Append("			this." + pi.Name + " = " + pi.Name + "_;\n");
            }

            classFileText.Append("		}\n	}\n}");


            // Write to the file.

            string filePath = "models\\" + className + ".cs";
            File.WriteAllText(filePath, classFileText.ToString());
        }


        public string sqlToCSDatatype(string sqlDatatype)
        {
            string csDatatype;
            switch (sqlDatatype.ToLower())
            {
                case "integer":
                    csDatatype = "int";
                    break;
                case "real":
                    csDatatype = "float";
                    break;
                case "boolean":
                    csDatatype = "bool";
                    break;
                case "character":
                    csDatatype = "string";
                    break;
                case "text":
                    csDatatype = "string";
                    break;
                default:
                    csDatatype = "<" + sqlDatatype + ">";
                    break;
            }

            return csDatatype;
        }
    }
}
