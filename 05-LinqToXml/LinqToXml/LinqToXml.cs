using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace LinqToXml
{
    public static class LinqToXml
    {
        /// <summary>
        /// Creates hierarchical data grouped by category
        /// </summary>
        /// <param name="xmlRepresentation">Xml representation (refer to CreateHierarchySourceFile.xml in Resources)</param>
        /// <returns>Xml representation (refer to CreateHierarchyResultFile.xml in Resources)</returns>
        public static string CreateHierarchy(string xmlRepresentation)
        {
            XDocument document = XDocument.Parse(xmlRepresentation); 
            XDocument result = new XDocument();
            XElement resultRoot = new XElement("Root");
            result.Add(resultRoot);
            XElement documnentRoot = document.Root;

            string[] allCategory = documnentRoot.Descendants("Data").Select((a) => a.Element("Category").Value).Distinct().ToArray();
            if (allCategory == null)
                return null;
            Dictionary<string, XElement> categoryElements = new Dictionary<string, XElement>();
            foreach (string category in allCategory)
            {
                XElement element = new XElement("Group");
                element.Add(new XAttribute("ID", category));
                resultRoot.Add(element);
                categoryElements.Add(category, element);
            }
            foreach (XElement element in documnentRoot.Elements("Data"))
            {
                string category = element.Element("Category").Value;
                element.Element("Category").Remove();
                categoryElements[category].Add(element);
            }
            return result.ToString();

        }

        /// <summary>
        /// Get list of orders numbers (where shipping state is NY) from xml representation
        /// </summary>
        /// <param name="xmlRepresentation">Orders xml representation (refer to PurchaseOrdersSourceFile.xml in Resources)</param>
        /// <returns>Concatenated orders numbers</returns>
        /// <example>
        /// 99301,99189,99110
        /// </example>
        public static string GetPurchaseOrders(string xmlRepresentation)
        {
            string result = "";
            XElement root = XDocument.Parse(xmlRepresentation).Root;
            XNamespace nameSp = root.GetNamespaceOfPrefix("aw");
            XElement[] Orders = root.Elements(nameSp+"PurchaseOrder").ToArray();
            if (Orders == null)
                return null;
            foreach (XElement order in Orders)
            {
                XElement[] Addresses = order.Elements(nameSp+"Address").ToArray();
                if (Addresses == null)
                    continue;
                bool NYContain = true;
                foreach (XElement address in Addresses)
                {
                    if (address.Element(nameSp+"State")?.Value != "NY")
                    {
                        NYContain = false;
                        break;
                    }
                }
                if (NYContain)
                {
                    string Value = order.Attribute(nameSp+"PurchaseOrderNumber").Value;
                    result +=Value+",";
                }
                    
            }
            result = result.TrimEnd(',');
            return result;
        }

        /// <summary>
        /// Reads csv representation and creates appropriate xml representation
        /// </summary>
        /// <param name="customers">Csv customers representation (refer to XmlFromCsvSourceFile.csv in Resources)</param>
        /// <returns>Xml customers representation (refer to XmlFromCsvResultFile.xml in Resources)</returns>
        public static string ReadCustomersFromCsv(string customers)
        {
            string[] lines = customers.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            if (lines == null)
                return null;
            XDocument resutDocument = new XDocument();
            XElement resultRoot = new XElement("Root");
            resutDocument.Add(resultRoot);
            foreach (var line in lines)
            {
                string[] elements = line.Split(',');
                XElement element = new XElement("Customer", new XAttribute("CustomerID", elements[0]),
                    new XElement("CompanyName", elements[1]),
                    new XElement("ContactName", elements[2]),
                    new XElement("ContactTitle", elements[3]),
                    new XElement("Phone", elements[4]),
                    new XElement("FullAddress",
                        new XElement("Address", elements[5]),
                        new XElement("City", elements[6]),
                        new XElement("Region", elements[7]),
                        new XElement("PostalCode", elements[8]),
                        new XElement("Country", elements[9])));
                resultRoot.Add(element);
            }
            return resutDocument.ToString();
        }

        /// <summary>
        /// Gets recursive concatenation of elements
        /// </summary>
        /// <param name="xmlRepresentation">Xml representation of document with Sentence, Word and Punctuation elements. (refer to ConcatenationStringSource.xml in Resources)</param>
        /// <returns>Concatenation of all this element values.</returns>
        public static string GetConcatenationString(string xmlRepresentation)
        {
            XElement root = XDocument.Parse(xmlRepresentation).Root;
            //return root.Value;
            return GetConcatenation(root);
            string GetConcatenation(XElement element)
            {
                string result = "";
                if (element.HasElements)
                    foreach (XElement item in element.Elements())
                        result += GetConcatenation(item);
                else
                    return element.Value;
                return result;
            }
        }

        /// <summary>
        /// Replaces all "customer" elements with "contact" elements with the same childs
        /// </summary>
        /// <param name="xmlRepresentation">Xml representation with customers (refer to ReplaceCustomersWithContactsSource.xml in Resources)</param>
        /// <returns>Xml representation with contacts (refer to ReplaceCustomersWithContactsResult.xml in Resources)</returns>
        public static string ReplaceAllCustomersWithContacts(string xmlRepresentation)
        {
            XDocument document = XDocument.Parse(xmlRepresentation);
            XElement root = document.Root;
            foreach(XElement element in root.Elements())
            {
                element.Name = "contact";
            }
            return document.ToString();
        }

        /// <summary>
        /// Finds all ids for channels with 2 or more subscribers and mark the "DELETE" comment
        /// </summary>
        /// <param name="xmlRepresentation">Xml representation with channels (refer to FindAllChannelsIdsSource.xml in Resources)</param>
        /// <returns>Sequence of channels ids</returns>
        public static IEnumerable<int> FindChannelsIds(string xmlRepresentation)
        {
            XDocument document = XDocument.Parse(xmlRepresentation);
            List<int> ids = new List<int>();
            var comments = document.Root.Elements().Nodes().OfType<XComment>();
            foreach (XComment comment in comments)
            {
                XElement parent = comment.Parent;
                if (parent.Elements().Count() < 2 || comment.Value != "DELETE")
                    continue;
                ids.Add(int.Parse(parent.Attribute("id").Value));
            }
            return ids;
        }

        /// <summary>
        /// Sort customers in docement by Country and City
        /// </summary>
        /// <param name="xmlRepresentation">Customers xml representation (refer to GeneralCustomersSourceFile.xml in Resources)</param>
        /// <returns>Sorted customers representation (refer to GeneralCustomersResultFile.xml in Resources)</returns>
        public static string SortCustomers(string xmlRepresentation)
        {
            XDocument document = XDocument.Parse(xmlRepresentation);
            XElement root = document.Root;
            XElement[] sortedElements = root.Elements().
                OrderBy((a) => a.Element("FullAddress").Element("Country").Value).
                ThenBy((a) => a.Element("FullAddress").Element("City").Value).ToArray();
            root.RemoveAll();
            root.Add(sortedElements);
            return document.ToString();
        }

        /// <summary>
        /// Gets XElement flatten string representation to save memory
        /// </summary>
        /// <param name="xmlRepresentation">XElement object</param>
        /// <returns>Flatten string representation</returns>
        /// <example>
        ///     <root><element>something</element></root>
        /// </example>
        public static string GetFlattenString(XElement xmlRepresentation)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets total value of orders by calculating products value
        /// </summary>
        /// <param name="xmlRepresentation">Orders and products xml representation (refer to GeneralOrdersFileSource.xml in Resources)</param>
        /// <returns>Total purchase value</returns>
        public static int GetOrdersValue(string xmlRepresentation)
        {
            XDocument document = XDocument.Parse(xmlRepresentation);
            XElement root = document.Root;
            Dictionary<int, int> productPrices = new Dictionary<int, int>();
            int totalPrice = 0;
            foreach(XElement element in root.Element("products").Elements())
            {
                int id = int.Parse(element.Attribute("Id").Value);
                int Value = int.Parse(element.Attribute("Value").Value);
                productPrices.Add(id, Value);
            }
            foreach (XElement order in root.Element("Orders").Elements())
            {
                int id = int.Parse(order.Element("product").Value);
                totalPrice += productPrices[id];
            }
            return totalPrice;

        }
    }
}
