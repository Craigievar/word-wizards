using System;
using System.Collections.Generic;

namespace Com.TypeGames.TSBR
{
    [Serializable]
    public class MatchData
    {

        //public 
        public List<String> positions;

        public MatchData(List<string> list)
        {
            this.positions = list;
        }
    }

}