using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Callbacks;

public class KeyboardBuildPostProcessor
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        Debug.Log("OnPostProcessBuild");
        if (target == BuildTarget.iOS)
        {
            string replace = "UITextAutocapitalizationType capitalization = UITextAutocapitalizationTypeSentences";
            string with = "UITextAutocapitalizationType capitalization = UITextAutocapitalizationTypeNone";

            Debug.Log(target.ToString());
            Debug.Log(pathToBuiltProject);
            Debug.Log("Removing autouppercase on keyboard");
            string targetfile = pathToBuiltProject + "/Classes/UI/Keyboard.mm";
            string filecontents = System.IO.File.ReadAllText(targetfile);
            if (filecontents.Contains(replace))
            {
                Debug.Log("replacing autocapitalization type");
                filecontents = filecontents.Replace(replace, with);
            }

            if (filecontents.Contains("toolbar.hidden = NO;"))
            {
                Debug.Log("commenting out [self createToolbars]");
                filecontents = filecontents.Replace("toolbar.hidden = NO;", "toolbar.hidden = YES;");
            }

            System.IO.File.WriteAllText(targetfile, filecontents);
        }
    }
}