namespace {0};

using System.IO;
using System.Xml.XPath;

/// <summary>
/// Parses WMI DTD 2.0 KVP (Key-Value Pair) exchange data items.
/// Each item is an XML string in the format:
/// <c>&lt;INSTANCE&gt;&lt;PROPERTY NAME="Name"&gt;&lt;VALUE&gt;key&lt;/VALUE&gt;&lt;/PROPERTY&gt;&lt;PROPERTY NAME="Data"&gt;&lt;VALUE&gt;value&lt;/VALUE&gt;&lt;/PROPERTY&gt;&lt;/INSTANCE&gt;</c>.
/// </summary>
internal static class KvpDataParser
{
    /// <summary>
    /// Searches an array of DTD 2.0 KVP XML items for a matching key and returns its value.
    /// </summary>
    /// <param name="kvpXmlItems">An array of KVP XML strings.</param>
    /// <param name="key">The key name to search for.</param>
    /// <returns>The value associated with the key, or <see langword="null"/> if not found.</returns>
    public static string GetValue(string[] kvpXmlItems, string key)
    {
        if (kvpXmlItems is null)
        {
            return null;
        }

        foreach (string item in kvpXmlItems)
        {
            using var reader = new StringReader(item);
            var document = new XPathDocument(reader);
            XPathNavigator navigator = document.CreateNavigator();

            XPathNavigator nameNode = navigator.SelectSingleNode(
                $"/INSTANCE/PROPERTY[@NAME='Name']/VALUE[child::text() = '{key}']");

            if (nameNode is not null)
            {
                XPathNavigator dataNode = navigator.SelectSingleNode(
                    "/INSTANCE/PROPERTY[@NAME='Data']/VALUE/child::text()");

                return dataNode?.Value;
            }
        }

        return null;
    }
}
