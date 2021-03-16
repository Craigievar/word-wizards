// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Exit Games GmbH">
//   Exit Games GmbH, 2012
// </copyright>
// <summary>
//   The "Particle" demo is a load balanced and Photon Cloud compatible "coding" demo.
//   The focus is on showing how to use the Photon features without too much "game" code cluttering the view.
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------


/// <summary>
/// Class to define a few constants used in this demo (for event codes, properties, etc).
/// </summary>
/// <remarks>
/// These values are something made up for this particular demo! 
/// You can define other values (and more) in your games, as needed.
/// </remarks>
///

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Com.TypeGames.TSBR
{
    public class WordListSet : MonoBehaviour
    {

        public static Dictionary<int, WordList> wordListDict;

        public static WordListSet wordLists;
        public static bool generated = false;

        public static WordListSet instance
        {
            get
            {
                if (!wordLists)
                {
                    wordLists = FindObjectOfType(typeof(WordListSet)) as WordListSet;

                    if (!wordLists)
                    {
                        Debug.Log("There needs to be one active EventManger script on a GameObject in your scene.");
                    }
                    else
                    {
                        WordListSet.Init();
                    }
                }

                return wordLists;
            }
        }

        public void Awake()
        {
            DontDestroyOnLoad(transform.gameObject);
        }

        public static void Generate()
        {
            //Debug.Log("Generating");
            generated = true;
            //TODO: Programmatically get this from a database
            wordListDict = new Dictionary<int, WordList>();

            wordListDict.Add(0, new WordList(0, "Craig is cool", "Self-evident",
                new List<string> { "craig", "cool", "sexy", "synonyms", "awesome" }));
            wordListDict.Add(1, new WordList(1, "Angela is cool", "Also self-evident",
                new List<string> { "angela", "sm0rt", "nerd", "cakes", "thyme" }));
            wordListDict.Add(2, new WordList(1, "Angela is cool", "Also self-evident",
                new List<string> { "angela", "sm0rt", "nerd", "cakes", "thyme" }));
            wordListDict.Add(3, new WordList(0, "Craig is cool", "Self-evident",
                new List<string> { "craig", "cool", "sexy", "synonyms", "awesome" }));
            wordListDict.Add(4, new WordList(1, "Angela is cool", "Also self-evident",
                new List<string> { "angela", "sm0rt", "nerd", "cakes", "thyme" }));
            wordListDict.Add(5, new WordList(1, "Angela is cool", "Also self-evident",
                new List<string> { "angela", "sm0rt", "nerd", "cakes", "thyme" }));
            wordListDict.Add(6, new WordList(0, "Craig is cool", "Self-evident",
                new List<string> { "craig", "cool", "sexy", "synonyms", "awesome" }));
            wordListDict.Add(7, new WordList(1, "Angela is cool", "Also self-evident",
                new List<string> { "angela", "sm0rt", "nerd", "cakes", "thyme" }));
            wordListDict.Add(8, new WordList(1, "Angela is cool", "Also self-evident",
                new List<string> { "angela", "sm0rt", "nerd", "cakes", "thyme" }));
            wordListDict.Add(9, new WordList(1, "Angela is cool", "Also self-evident",
                new List<string> { "angela", "sm0rt", "nerd", "cakes", "thyme" }));
            wordListDict.Add(10, new WordList(1, "Angela is cool", "Also self-evident",
                new List<string> { "angela", "sm0rt", "nerd", "cakes", "thyme" }));
            wordListDict.Add(11, new WordList(0, "Craig is cool", "Self-evident",
                new List<string> { "craig", "cool", "sexy", "synonyms", "awesome" }));
            wordListDict.Add(12, new WordList(1, "Angela is cool", "Also self-evident",
                new List<string> { "angela", "sm0rt", "nerd", "cakes", "thyme" }));
            wordListDict.Add(13, new WordList(1, "Angela is cool", "Also self-evident",
                new List<string> { "angela", "sm0rt", "nerd", "cakes", "thyme" }));
        }

        public static void Init()
        {
        }

        public static WordList GetWordListById(int id)
        {
            if (wordListDict.ContainsKey(id))
            {
                return wordListDict[id];
            }
            else
            {
                Debug.Log("Can't find word list with ID " + id);
                return null;
            }
        }
    }
}