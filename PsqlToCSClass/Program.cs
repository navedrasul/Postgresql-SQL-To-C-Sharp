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
        public string namespaceName { get; set; }
        public string className { get; set; }
        public string baseClassName { get; set; }
        public List<PropertyInfo> propertyInfos { get; set; }
    }

    class Program
    {
        const string ClassNamespaceParent = "TrakGud.DAL.Models";

        static void Main(string[] args)
        {
            // Generate the C# class files.

            string[] sqlLines = File.ReadAllLines("db.sql.txt");

            bool tBody = false;

            List<ClassInfo> classInfos = new List<ClassInfo>();

            string namespaceName = string.Empty;
            string className = string.Empty;
            string baseClassName = string.Empty;
            List<PropertyInfo> propertyInfos = new List<PropertyInfo>();

            // General search keywords in the SQL.

            const string CmntT_ = "--";
            const string CT_ = "create table public.\"";
            const string ET_ = ")";
            const string IT_ = "inherits (public.\"";

            // Custom Namespace Name search keywords in the SQL.

            const string DomainNST_ = "d";
            const string DomainNS = "Domain";
            const string GlobalSettingsNST_ = "gs";
            const string GlobalSettingsNS = "Global Settings";
            const string NotifMgmtNST_ = "nm";
            const string NotifMgmtNS = "NotificationManagement";
            const string FleetMgmtNST_ = "fm";
            const string FleetMgmtNS = "FleetManagement";
            const string ShpmntMgmtNST_ = "sm";
            const string ShpmntMgmtNS = "ShipmentManagement";
            const string SalesNST_ = "s";
            const string SalesNS = "Sales";
            const string HRMNST_ = "hrm";
            const string HRMNS = "HumanResourceManagement";
            const string FinMgmtNST_ = "fim";
            const string FinMgmtNS = "FinanceManagement";
            const string UserMgmtNST_ = "um";
            const string UserMgmtNS = "UserManagement";
            const string ContactMgmtNST_ = "cm";
            const string ContactMgmtNS = "ContactManagement";

            // Property-specific search keywords in the SQL.

            const string NNT_ = "not null";

            const string StringDTT1_ = "character";
            const string StringDTT2_ = "text";
            const string StringDT = "string";
            const string IntDTT_ = "integer";
            const string IntDT = "int";
            const string FloatDTT_ = "real";
            const string FloatDT = "float";
            const string DatetimeDTT1_ = "timestamp";
            const string DatetimeDTT2_ = "date";
            const string DatetimeDT = "DateTime";
            const string BoolDTT_ = "boolean";
            const string BoolDT = "bool";

            for (int li = 0; li < sqlLines.Length; li++)
            {
                string l = sqlLines[li];

                if (l.TrimStart().StartsWith(CmntT_))
                {
                    continue;
                }

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
                            baseClassName = bcn_[^1];
                        }

                        //GenerateClassFile(className, baseClassName, propertyInfos);
                        ClassInfo classInfo = new ClassInfo
                        {
                            namespaceName = namespaceName,
                            className = className,
                            baseClassName = baseClassName,
                            propertyInfos = propertyInfos
                        };
                        classInfos.Add(classInfo);

                        namespaceName = string.Empty;
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
                            pi.Datatype = IntDT;
                        }
                        else if (afterColName.Contains(StringDTT1_) || afterColName.Contains(StringDTT2_))
                        {
                            pi.Datatype = StringDT;
                        }
                        else if (afterColName.Contains(FloatDTT_))
                        {
                            pi.Datatype = FloatDT;
                        }
                        else if (afterColName.Contains(DatetimeDTT1_) || afterColName.Contains(DatetimeDTT2_))
                        {
                            pi.Datatype = DatetimeDT;
                        }
                        else if (afterColName.Contains(BoolDTT_))
                        {
                            pi.Datatype = BoolDT;
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

                    // Find class namespace.

                    string nst = cn_[0];

                    switch (nst)
                    {
                        case DomainNST_:
                            namespaceName = DomainNS;
                            break;
                        case GlobalSettingsNST_:
                            namespaceName = GlobalSettingsNS;
                            break;
                        case NotifMgmtNST_:
                            namespaceName = NotifMgmtNS;
                            break;
                        case FleetMgmtNST_:
                            namespaceName = FleetMgmtNS;
                            break;
                        case ShpmntMgmtNST_:
                            namespaceName = ShpmntMgmtNS;
                            break;
                        case SalesNST_:
                            namespaceName = SalesNS;
                            break;
                        case HRMNST_:
                            namespaceName = HRMNS;
                            break;
                        case FinMgmtNST_:
                            namespaceName = FinMgmtNS;
                            break;
                        case UserMgmtNST_:
                            namespaceName = UserMgmtNS;
                            break;
                        case ContactMgmtNST_:
                            namespaceName = ContactMgmtNS;
                            break;
                    }

                    // Find class name.

                    className = cn_[^1];
                    Console.WriteLine(className);
                }
            }

            GenerateClassFiles(classInfos);
        }

        private static void GenerateClassFiles(List<ClassInfo> classInfos)
        {
            foreach (ClassInfo classInfo in classInfos)
            {
                ClassInfo baseClassInfo;
                if (!string.IsNullOrEmpty(classInfo.baseClassName))
                {
                    baseClassInfo = classInfos.Find(ci => ci.className == classInfo.baseClassName);
                }
                else
                {
                    baseClassInfo = new ClassInfo();
                }

                GenerateClassFile(classInfo.namespaceName, classInfo.className, baseClassInfo.namespaceName, classInfo.baseClassName, classInfo.propertyInfos, baseClassInfo.propertyInfos);
            }
        }

        private static void GenerateClassFile(string namespaceName, string className, string baseNamespaceName, string baseClassName, List<PropertyInfo> propertyInfos, List<PropertyInfo> basePropertyInfos)
        {
            // Generate the text.

            StringBuilder classFileText = new StringBuilder();

            classFileText.Append("using System;\n");
            classFileText.Append("using System.Collections.Generic;\n");
            classFileText.Append("using System.Text;\n");

            // Include / import the base-class namespace if it's different from the current class namespace
            if (!string.IsNullOrEmpty(baseClassName) && !namespaceName.Equals(baseNamespaceName))
            {
                classFileText.Append("using " + ClassNamespaceParent + "." + baseNamespaceName + ";\n");
            }

            classFileText.Append("\n");
            classFileText.Append("namespace " + ClassNamespaceParent + "." + namespaceName + "\n{\n");
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

            string dirPath = "models\\" + namespaceName;

            // Create output "models" folder.

            Directory.CreateDirectory(dirPath);

            // Write to the file.

            string filePath = dirPath + "\\" + className + ".cs";
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
