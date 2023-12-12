using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Imagine.WebAR
{
    public class PostProcessBuild : MonoBehaviour
    {
        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget target, string buildPath)
        {
            //System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

            Debug.Log(buildPath);
            var targetsHtml = "";

            if(!Directory.Exists(buildPath + "/targets"))
            {
                Directory.CreateDirectory(buildPath + "/targets");
            }

            foreach (var info in CT_GlobalSettings.Instance.curvedTargetInfos)
            {
                var src = AssetDatabase.GetAssetPath(info.texture);
                var fileName = Path.GetFileName(src);
                Debug.Log(info.id + "->" + src);

                File.Copy(src, buildPath + "/targets/" + fileName, true);

                var arc = info.arc/360;
                var rf = info.radMul;
                var height = info.height;

                var arcStr = arc.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var rfStr = rf.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var heightStr = height.ToString(System.Globalization.CultureInfo.InvariantCulture);

                // Debug.Log(arc + "=>" + arcStr);
                // Debug.Log(rf + "=>" + rfStr);
                // Debug.Log(height + "=>" + heightStr);

                targetsHtml += ("\t\t<imagetarget id='" + info.id + "' src='targets/" + fileName + "' arc=" + arcStr + " rf=" + rfStr + " h=" + heightStr + "></imagetarget>\n");
            }

            Debug.Log(targetsHtml);

            var lines = File.ReadAllLines(buildPath + "/index.html").ToList();
            var html = "";
            foreach(var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                var trimmed = line.Trim();
                if (trimmed.StartsWith("<imagetarget") && trimmed.EndsWith("</imagetarget>"))
                    continue;
                html += line + "\n";
            }
            html = html.Replace("<!--IMAGETARGETS-->", "<!--IMAGETARGETS-->\n" + targetsHtml);
            File.WriteAllText(buildPath + "/index.html", html);
        }
    }
}

