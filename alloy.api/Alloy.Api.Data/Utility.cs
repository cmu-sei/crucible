/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Alloy.Api.Data.Models;
using Alloy.Api.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Alloy.Api.Data
{
    public static class Utility
    {
        
        static Random _random = new Random();
        static SHA1CryptoServiceProvider _sha = new SHA1CryptoServiceProvider();

        public static string RandomString()
        {
            return _random.Next().ToString("x8");
        }

        public static string RandomPin()
        {
            return _random.Next(1000, 10000).ToString();
        }

        public static byte[] Hash(string data)
        {
            return _sha.ComputeHash(Encoding.UTF8.GetBytes(data));
        }

        public static string NewAccountName(int teamId)
        {
            return String.Format("{0}-{1}$step", teamId, Utility.RandomString());
        }

        // public static bool Authenticate(string account, string password)
        // {
        //     using (ExerciseContext db = new ExerciseContext())
        //     {
        //         Member m = db.Members.Where(p => p.Account.ToLower() == account.ToLower()).FirstOrDefault();
        //         return (m != null) ? m.Password == password : false;
        //     }
        // }

        public static void SaveObject(object obj, string filename)
        {
            XmlSerializer ser = new XmlSerializer(obj.GetType());
            XmlTextWriter writer = new XmlTextWriter(filename, Encoding.ASCII);
            writer.Formatting = Formatting.Indented;
            ser.Serialize(writer, obj);
            writer.Close();
        }

        public static void SaveObject(object obj, Stream stream)
        {
            XmlSerializer ser = new XmlSerializer(obj.GetType());
            XmlTextWriter writer = new XmlTextWriter(stream, Encoding.ASCII);
            writer.Formatting = Formatting.Indented;
            ser.Serialize(writer, obj);
            writer.Flush();
        }

        public static object LoadObject(Type type, MemoryStream stream)
        {
            Object result = null;            
            XmlTextReader reader = null;

            try
            {
                XmlSerializer ser = new XmlSerializer(type);                
                reader = new XmlTextReader(stream);
                result = ser.Deserialize(reader);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                reader = null;
            }
            
            return result;
        }

        public static object LoadObject(Type type, string filename)
        {
            Object result = null;
            FileStream fs = null;
            XmlTextReader reader = null;
            if (System.IO.File.Exists(filename))
            {
                try
                {
                    XmlSerializer ser = new XmlSerializer(type);
                    fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                    reader = new XmlTextReader(fs);
                    result = ser.Deserialize(reader);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                    if (fs != null)
                        fs.Close();
                    reader = null;
                }
            }
            return result;
        }

        // #region Import

        // //assets.Key = fileName, assets.Value = asset contents
        // //Returns string.empty on success, error messages on failure
        // public static string ImportAssets(int scenarioId, Dictionary<string, MemoryStream> assets)            
        // {
        //     string errors = string.Empty;

        //     using(DataWrapper<Scenario> scenarioWrapper = new DataWrapper<Scenario>())
        //     {
        //         try
        //         {
        //             Scenario scenario = scenarioWrapper.Get(scenarioId);
        //             bool saved = false;

        //             //Must import config file last so linked files will be present
        //             foreach (KeyValuePair<string, MemoryStream> kvp in assets.OrderBy(x => x.Key.EndsWith(".config")))
        //             {                                                
        //                 if (kvp.Key.EndsWith(".config") && !saved)
        //                 {
        //                     scenarioWrapper.Save();
        //                     saved = true;
        //                 }

        //                 try
        //                 {
        //                     ImportAsset(scenario, kvp.Value, kvp.Key);
        //                 }
        //                 catch(Exception ex)
        //                 {
        //                     Logger.Log(ex, 1);
        //                     errors += ex + "\n";
        //                 }
        //             }
        //         }
        //         catch(Exception ex)
        //         {
        //             Logger.Log(ex, 1);
        //             errors += ex + "\n";
        //         }                
        //     }

        //     return errors;
        // }

        // public static void ImportAsset(Scenario scenario, MemoryStream asset, string fileName)
        // {            
        //     bool handled = false;
        //     FileType fileType = FileType.unknown;

        //     switch (System.IO.Path.GetExtension(fileName))
        //     {
        //         case ".map":
        //             fileType = FileType.map;
        //             break;

        //         case ".pdf":
        //             fileType = FileType.document;
        //             break;

        //         case ".png":
        //         case ".jpg":
        //         case ".gif":
        //         case ".bmp":
        //             fileType = FileType.image;
        //             break;

        //         case ".csv":
        //         case ".dat":
        //             fileType = FileType.data;
        //             break;

        //         case ".gus":
        //             fileType = FileType.gus;
        //             ImportGusConfig(asset, fileName);
        //             handled = true;
        //             break;

        //         case ".xga":
        //             fileType = FileType.xga;
        //             List<GuestAgentTaskTemplate> list = (List<GuestAgentTaskTemplate>)LoadObject(typeof(List<GuestAgentTaskTemplate>), asset);
        //             ImportTaskTemplates(scenario, list);
        //             handled = true;
        //             break;

        //         case ".quiz":
        //             fileType = FileType.quiz;
        //             break;

        //         case ".templates":
        //             ImportTemplates(scenario, asset);
        //             handled = true;
        //             break;

        //         case ".xml":
        //             xnet.data.TemplateLibrary lib = (xnet.data.TemplateLibrary)LoadObject(typeof(xnet.data.TemplateLibrary), asset);
        //             ConvertTemplates(scenario, lib);
        //             handled = true;
        //             break;

        //         case ".config":
        //             fileType = FileType.config;
        //             ImportConfig(scenario, asset.ToArray(), fileName);
        //             handled = true;
        //             break;

        //         case ".ckeiconfig":
        //             fileType = FileType.ckeiconfig;
        //             ImportCKEIConfig(scenario, asset.ToArray(), fileName);
        //             handled = true;
        //             break;

        //         case ".disks":                    
        //             //Ignore
        //             handled = true;
        //             break;

        //         default:
        //             fileType = FileType.unknown;
        //             handled = true;
        //             throw new Exception(String.Format("Cannot import unsupported file - {0}", fileName));
        //             break;
        //     }

        //     if (!handled)
        //     {
        //         Data.File df = scenario.Files.Where(o => o.Name.ToLower() == fileName.ToLower()).FirstOrDefault();
        //         if (df == null)
        //         {
        //             df = new Data.File();
        //             df.Name = fileName;
        //             df.Type = fileType;
        //             df.FileData = new FileData();
        //             scenario.Files.Add(df);
        //         }

        //         df.FileData.Data = asset.ToArray();

        //         if (df.Type == FileType.map)
        //         {
        //             string map = Encoding.ASCII.GetString(df.FileData.Data);
        //             map = map.Replace("ExerciseMap", "Map");
        //             map = map.Replace("ImageFile>", "ImageName>");
        //             map = map.Replace("Base64Image>", "ImageBytesEncoded>");
        //             df.FileData.Data = Encoding.ASCII.GetBytes(map);
        //         }
        //     }       
        // }

        // public static void ImportGusConfig(MemoryStream config, string fileName)
        // {
        //     string path = ConfigurationManager.AppSettings["GusConfigPath"];
        //     System.IO.File.WriteAllBytes(path + "\\" + fileName, config.ToArray());
        // }

        // public static void ImportFile(Scenario scenario, string fullpath)
        // {
        //     string fileName = System.IO.Path.GetFileName(fullpath);
        //     using(MemoryStream asset = new MemoryStream(System.IO.File.ReadAllBytes(fullpath)))
        //     {
        //         asset.Position = 0;
        //         ImportAsset(scenario, asset, fileName);
        //     }
        // }

        // static void ImportConfig(Scenario scenario, byte[] fileData, string fileName)
        // {
        //     bool added = false;

        //     Data.File file = scenario.Files.Where(o => o.Name.ToLower() == fileName.ToLower()).FirstOrDefault();

        //     if(file == null)
        //     {
        //         added = true;

        //         file = new Data.File();
        //         file.Name = fileName;
        //         file.Type = FileType.config;
        //         file.FileData = new FileData();
        //         file.ScenarioId = scenario.Id;
        //         file.Guid = Guid.NewGuid();
        //     }

        //     file.FileData.Data = fileData;

        //     //Process as ScenarioConfigurationModel to convert names into ids
        //     ScenarioConfigurationModel configModel = new ScenarioConfigurationModel(file);
        //     configModel.RemoveEmptyElements();
        //     file.FileData.Data = configModel.ToFileData();

        //     using(FileWrapper wrapper = new FileWrapper())
        //     {
        //         if(added)
        //         {
        //             wrapper.Add(file);
        //         }
        //         else
        //         {
        //             wrapper.UpdateTextFile(file);
        //         }
        //     }
        // }

        // static void ImportCKEIConfig(Scenario scenario, byte[] fileData, string fileName)
        // {
        //     bool added = false;

        //     Data.File file = scenario.Files.Where(o => o.Name.ToLower() == fileName.ToLower()).FirstOrDefault();

        //     if (file == null)
        //     {
        //         added = true;

        //         file = new Data.File();
        //         file.Name = fileName;
        //         file.Type = FileType.ckeiconfig;
        //         file.FileData = new FileData();
        //         file.ScenarioId = scenario.Id;
        //     }

        //     file.FileData.Data = fileData;

        //     //Process as ScenarioConfigurationModel to convert names into ids
        //     CKEIConfigurationModel configModel = CKEIConfigurationModel.GetFromFile(file, true);
        //     file.FileData.Data = configModel.ToFileData();

        //     using (FileWrapper wrapper = new FileWrapper())
        //     {
        //         if (added)
        //         {
        //             wrapper.Add(file);
        //         }
        //         else
        //         {
        //             wrapper.UpdateTextFile(file);
        //         }
        //     }
        // }

        // static void ImportTaskTemplates(Scenario scenario, List<GuestAgentTaskTemplate> list)
        // {            
        //     if (list != null && list.Count > 0)
        //     {
        //         using (DataWrapper<GuestAgentTaskTemplate> wrapper = new DataWrapper<GuestAgentTaskTemplate>())
        //         {
        //             List<GuestAgentTaskTemplate> scenarioTemplates = wrapper.GetAll().Where(t => t.ScenarioId == scenario.Id).ToList();
        //             List<GuestAgentTaskTemplate> dbTemplates = new List<GuestAgentTaskTemplate>();

        //             foreach (GuestAgentTaskTemplate tt in list)
        //             {
        //                 tt.ScenarioId = scenario.Id;
        //                 GuestAgentTaskTemplate existing = scenarioTemplates.Where(o => o.ScenarioId == scenario.Id && o.Name == tt.Name).FirstOrDefault();
        //                 if (existing != null)
        //                 {
        //                     tt.Id = existing.Id;
        //                     tt.SuccessTemplateId = existing.SuccessTemplateId;
        //                     tt.FailureTemplateId = existing.FailureTemplateId;
        //                     wrapper.Update(tt);
        //                     dbTemplates.Add(tt);
        //                 }
        //                 else
        //                 {
        //                     tt.Id = 0;
        //                     tt.SuccessTemplateId = null;
        //                     tt.FailureTemplateId = null;
        //                     wrapper.Add(tt);
        //                     dbTemplates.Add(tt);
        //                 }
        //             }

        //             //Have to save here so that Ids are generated to use as success/failure templates
        //             wrapper.Save();

        //             foreach(GuestAgentTaskTemplate dbTemplate in dbTemplates)
        //             {
        //                 if(dbTemplate.SuccessTemplateExportable.HasValue())
        //                 {
        //                     GuestAgentTaskTemplate successTemplate = dbTemplates.Where(t => t.Name == dbTemplate.SuccessTemplateExportable).FirstOrDefault();

        //                     if(successTemplate == null)
        //                     {
        //                         successTemplate = scenarioTemplates.Where(t => t.Name == dbTemplate.SuccessTemplateExportable).FirstOrDefault();
        //                     }

        //                     if(successTemplate == null)
        //                     {
        //                         string error = String.Format("Could not find Success Template {0} to add to Template {1} ({2})", dbTemplate.SuccessTemplateExportable, dbTemplate.Name, dbTemplate.Id);
        //                         Logger.Log(error, 1);                                
        //                     }
        //                     else
        //                     {
        //                         dbTemplate.SuccessTemplateId = successTemplate.Id;                                
        //                     }
        //                 }
        //                 else
        //                 {
        //                     dbTemplate.SuccessTemplateId = null;
        //                 }

        //                 if (dbTemplate.FailureTemplateExportable.HasValue())
        //                 {
        //                     GuestAgentTaskTemplate failureTemplate = dbTemplates.Where(t => t.Name == dbTemplate.FailureTemplateExportable).FirstOrDefault();

        //                     if (failureTemplate == null)
        //                     {
        //                         failureTemplate = scenarioTemplates.Where(t => t.Name == dbTemplate.FailureTemplateExportable).FirstOrDefault();
        //                     }

        //                     if (failureTemplate == null)
        //                     {
        //                         string error = String.Format("Could not find Failure Template {0} to add to Template {1} ({2})", dbTemplate.FailureTemplateExportable, dbTemplate.Name, dbTemplate.Id);
        //                         Logger.Log(error, 1);
        //                     }
        //                     else
        //                     {
        //                         dbTemplate.FailureTemplateId = failureTemplate.Id;
        //                         wrapper.Update(dbTemplate);
        //                     }
        //                 }
        //                 else
        //                 {
        //                     dbTemplate.FailureTemplateId = null;
        //                 }

        //                 wrapper.Update(dbTemplate);
        //             }
        //         }
        //     }
        // }

        // #region ImportTemplates

        // static void ImportTemplates(Scenario scenario, MemoryStream stream)
        // {
        //     TextReader tr = new StreamReader(stream);
        //     ImportTemplates(scenario, tr);
        // }

        // static void ImportTemplates(Scenario scenario, string fullpath)
        // {
        //     TextReader tr = new StreamReader(fullpath);
        //     ImportTemplates(scenario, tr);
        // }

        // static void ImportTemplates(Scenario scenario, TextReader tr)
        // {            
        //     string config = "";
        //     string name = "";
        //     bool detail = false;
        //     Template last = null;

        //     string line = tr.ReadLine();
        //     while (line != null)
        //     {
        //         if (line.StartsWith("["))  //new template
        //         {
        //             //save current
        //             if (!String.IsNullOrWhiteSpace(name))
        //             {
        //                 if (detail)
        //                 {
        //                     if (last != null)
        //                     {
        //                         TemplateDetail td = last.Detail.Where(o => o.Name == name).FirstOrDefault();
        //                         if (td == null)
        //                         {
        //                             td = new TemplateDetail { Name = name };
        //                             last.Detail.Add(td);
        //                         }
        //                         td.Config = config;
        //                     }
        //                 }
        //                 else
        //                 {
        //                     Template t = scenario.Templates.Where(o => o.Name == name).FirstOrDefault();
        //                     if (t == null)
        //                     {
        //                         t = new Template { Name = name };
        //                         scenario.Templates.Add(t);
        //                     }

        //                     t.Config = config;
        //                     last = t;
        //                 }
        //             }

        //             //setup new
        //             name = line.Inner();
        //             detail = !line.EndsWith("]");
        //             config = "";

        //         }
        //         else if (!String.IsNullOrWhiteSpace(line))
        //             config += line + Environment.NewLine;

        //         line = tr.ReadLine();
        //     }

        //     if (!String.IsNullOrWhiteSpace(name))
        //     {
        //         if (detail)
        //         {
        //             if (last != null)
        //             {
        //                 TemplateDetail td = last.Detail.Where(o => o.Name == name).FirstOrDefault();
        //                 if (td == null)
        //                 {
        //                     td = new TemplateDetail { Name = name };
        //                     last.Detail.Add(td);
        //                 }
        //                 td.Config = config;
        //             }
        //         }
        //         else
        //         {
        //             Template t = scenario.Templates.Where(o => o.Name == name).FirstOrDefault();
        //             if (t == null)
        //             {
        //                 t = new Template { Name = name };
        //                 scenario.Templates.Add(t);
        //             }

        //             t.Config = config;
        //             last = t;
        //         }

        //     }

        //     tr.Close();
        // }

        // #endregion

        // static void ConvertTemplates(Scenario scenario, xnet.data.TemplateLibrary lib)
        // {
        //     //string fileName = System.IO.Path.GetFileName(fullpath);
        //     if (scenario.Templates == null)
        //         scenario.Templates = new List<Template>();
            
        //     if (lib != null)
        //     {
        //         foreach (xnet.data.TemplateGroup tg in lib.TemplateGroups)
        //         {
        //             string name = tg.Name + "+";
        //             Template t = scenario.Templates.Where(o => o.Name == name).FirstOrDefault();
        //             if (t == null)
        //             {
        //                 t = new Template { Name = name };
        //                 scenario.Templates.Add(t);
        //             }
        //             t.Config = String.Join(Environment.NewLine, tg.Items.ToArray());
        //         }

        //         foreach (xnet.data.VirtualMachineTemplate vmt in lib.VirtualMachinesTemplates)
        //         {
        //             if (String.IsNullOrWhiteSpace(vmt.Name))
        //                 continue;

        //             Template t = scenario.Templates.Where(o => o.Name == vmt.Name).FirstOrDefault();
        //             if (t == null)
        //             {
        //                 t = new Template { Name = vmt.Name };
        //                 scenario.Templates.Add(t);
        //             }
        //             t.Config = vmt.TransformToList();
        //         }
        //     }
        // }

        // #endregion
    }

    public class AlphaNumericStringComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            int r = 0, ai = 0, bi = 0, am = 0, bm = 0;
            int an = 0, bn = 0;
            bool af = false, bf = false;
            char[] a = x.ToCharArray();
            char[] b = y.ToCharArray();
            
            //iterate through each character
            while (ai < a.Length && bi < b.Length && ai == bi && r == 0)
            {
                af = false;
                bf = false;

                if (Char.IsDigit(a[ai]))
                {
                    //run to end of number in a
                    am = ai;
                    do
                    {
                        ai += 1;
                    } while (ai < a.Length && Char.IsDigit(a[ai]));
                    Int32.TryParse(new String(a, am, ai - am), out an);
                    af = true;
                }
                else
                {
                    //grab char value
                    an = (int)a[ai];
                    ai += 1;
                }

                if (Char.IsDigit(b[bi]))
                {
                    //run to end of number in b
                    bm = bi;
                    do
                    {
                        bi += 1;
                    } while (bi < b.Length && Char.IsDigit(b[bi]));
                    Int32.TryParse(new String(b, bm, bi - bm), out bn);
                    bf = true;
                }
                else
                {
                    //grab char value
                    bn = (int)b[bi];
                    bi += 1;
                }

                if (af == bf)               //if both numbers or both character values
                    r = an.CompareTo(bn);
                else                        // return number < text
                    r = (af) ? -1 : 1;
            }

            if (r == 0)                     //if completely matching up to termination point, return shorter < longer
                r = x.Length.CompareTo(y.Length);

            return r;
        }
    }

    public class AlphaNumericComparer : IComparer<Object>
    {
        string _member = "";
        public AlphaNumericComparer()
        {
        }
        public AlphaNumericComparer(string member)
        {
            _member = member;
        }

        public int Compare(Object x, Object y)
        {
            int r = 0, ai = 0, bi = 0, am = 0, bm = 0;
            int an = 0, bn = 0;
            bool af = false, bf = false;
            string xValue = "", yValue = "";
            if (_member.HasValue())
            {
                xValue = x.GetPropertyValue(_member).ToString();
                yValue = y.GetPropertyValue(_member).ToString();
            }
            else
            {
                xValue = x.ToString();
                yValue = y.ToString();
            }
            char[] a = xValue.ToCharArray();
            char[] b = yValue.ToCharArray();

            //iterate through each character
            while (ai < a.Length && bi < b.Length && ai == bi && r == 0)
            {
                af = false;
                bf = false;

                if (Char.IsDigit(a[ai]))
                {
                    //run to end of number in a
                    am = ai;
                    do
                    {
                        ai += 1;
                    } while (ai < a.Length && Char.IsDigit(a[ai]));
                    Int32.TryParse(new String(a, am, ai - am), out an);
                    af = true;
                }
                else
                {
                    //grab char value
                    an = (int)a[ai];
                    ai += 1;
                }

                if (Char.IsDigit(b[bi]))
                {
                    //run to end of number in b
                    bm = bi;
                    do
                    {
                        bi += 1;
                    } while (bi < b.Length && Char.IsDigit(b[bi]));
                    Int32.TryParse(new String(b, bm, bi - bm), out bn);
                    bf = true;
                }
                else
                {
                    //grab char value
                    bn = (int)b[bi];
                    bi += 1;
                }

                if (af == bf)               //if both numbers or both character values
                    r = an.CompareTo(bn);
                else                        // return number < text
                    r = (af) ? -1 : 1;
            }

            if (r == 0)                     //if completely matching up to termination point, return shorter < longer
                r = xValue.Length.CompareTo(yValue.Length);

            return r;
        }
    }
    
}
